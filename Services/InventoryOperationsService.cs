using System;
using System.Data;
using System.Data.SqlClient;

namespace Pos_System.Services
{
    internal sealed class InventoryOperationsService
    {
        private readonly string connectionString;
        public InventoryOperationsService(string connectionString) { this.connectionString = connectionString; }

        public DataTable Warehouses() => Query("SELECT warehouse_id,warehouse_code,name,is_default,is_active FROM dbo.Warehouses ORDER BY name;");
        public DataTable Products() => Query("SELECT product_id,name,barcode,stock_quantity,purchase_price,selling_price FROM dbo.Products ORDER BY name;");
        public DataTable StockHealth() => Query("SELECT * FROM dbo.vw_StockHealth ORDER BY needs_reorder DESC,product_name;");
        public DataTable Units() => Query("SELECT unit_id,code,name,unit_type,is_active FROM dbo.Units ORDER BY name;");

        public int CompleteStockCount(int warehouseId, int productId, decimal actualQty, string reason, int userId)
        {
            ActionPermissionService.Demand("STOCK.COUNT");
            if (actualQty < 0m) throw new InvalidOperationException("Actual stock cannot be negative.");
            using (var conn = Open()) using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    decimal expected = GetWarehouseQty(conn, tx, warehouseId, productId, true);
                    string number = "SC-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                    int countId;
                    using (var cmd = new SqlCommand(@"INSERT dbo.StockCounts(count_number,warehouse_id,status,notes,user_id,completed_at)
VALUES(@n,@w,N'COMPLETED',@notes,@u,SYSUTCDATETIME()); SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                    { cmd.Parameters.AddWithValue("@n", number); cmd.Parameters.AddWithValue("@w", warehouseId); cmd.Parameters.AddWithValue("@notes", string.IsNullOrWhiteSpace(reason) ? (object)DBNull.Value : reason); cmd.Parameters.AddWithValue("@u", userId); countId = Convert.ToInt32(cmd.ExecuteScalar()); }
                    using (var cmd = new SqlCommand("INSERT dbo.StockCountItems(stock_count_id,product_id,expected_quantity,actual_quantity,reason) VALUES(@c,@p,@e,@a,@r);", conn, tx))
                    { cmd.Parameters.AddWithValue("@c", countId); cmd.Parameters.AddWithValue("@p", productId); AddDecimal(cmd,"@e",expected); AddDecimal(cmd,"@a",actualQty); cmd.Parameters.AddWithValue("@r", string.IsNullOrWhiteSpace(reason) ? (object)DBNull.Value : reason); cmd.ExecuteNonQuery(); }
                    UpsertWarehouseStock(conn,tx,warehouseId,productId,actualQty);
                    int oldLegacy = GetLegacyQty(conn,tx,productId,true); int newLegacy = oldLegacy + Convert.ToInt32(Math.Round(actualQty-expected,0));
                    using (var cmd = new SqlCommand("UPDATE dbo.Products SET stock_quantity=@q,updated_at=SYSUTCDATETIME() WHERE product_id=@p;",conn,tx)) { cmd.Parameters.AddWithValue("@q",newLegacy); cmd.Parameters.AddWithValue("@p",productId); cmd.ExecuteNonQuery(); }
                    InsertInventoryTransaction(conn,tx,productId,"STOCK_COUNT",actualQty-expected,expected,actualQty,"STOCK_COUNT",countId,userId,warehouseId,reason);
                    tx.Commit(); return countId;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public int Transfer(int fromWarehouseId, int toWarehouseId, int productId, decimal qty, string notes, int userId)
        {
            ActionPermissionService.Demand("STOCK.TRANSFER");
            if (fromWarehouseId == toWarehouseId) throw new InvalidOperationException("Source and destination warehouses must be different.");
            if (qty <= 0) throw new InvalidOperationException("Transfer quantity must be greater than zero.");
            using (var conn = Open()) using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    decimal fromQty = GetWarehouseQty(conn,tx,fromWarehouseId,productId,true); if (fromQty < qty) throw new InvalidOperationException("Not enough stock in source warehouse.");
                    decimal toQty = GetWarehouseQty(conn,tx,toWarehouseId,productId,true);
                    string number="TRF-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"); int id;
                    using(var cmd=new SqlCommand(@"INSERT dbo.StockTransfers(transfer_number,from_warehouse_id,to_warehouse_id,status,notes,user_id,completed_at)
VALUES(@n,@f,@t,N'COMPLETED',@notes,@u,SYSUTCDATETIME());SELECT CAST(SCOPE_IDENTITY() AS INT);",conn,tx)) { cmd.Parameters.AddWithValue("@n",number);cmd.Parameters.AddWithValue("@f",fromWarehouseId);cmd.Parameters.AddWithValue("@t",toWarehouseId);cmd.Parameters.AddWithValue("@notes",string.IsNullOrWhiteSpace(notes)?(object)DBNull.Value:notes);cmd.Parameters.AddWithValue("@u",userId);id=Convert.ToInt32(cmd.ExecuteScalar()); }
                    using(var cmd=new SqlCommand("INSERT dbo.StockTransferItems(stock_transfer_id,product_id,quantity) VALUES(@id,@p,@q);",conn,tx)){cmd.Parameters.AddWithValue("@id",id);cmd.Parameters.AddWithValue("@p",productId);AddDecimal(cmd,"@q",qty);cmd.ExecuteNonQuery();}
                    UpsertWarehouseStock(conn,tx,fromWarehouseId,productId,fromQty-qty); UpsertWarehouseStock(conn,tx,toWarehouseId,productId,toQty+qty);
                    int legacy=GetLegacyQty(conn,tx,productId,true);
                    InsertInventoryTransaction(conn,tx,productId,"TRANSFER_OUT",-qty,fromQty,fromQty-qty,"STOCK_TRANSFER",id,userId,fromWarehouseId,notes);
                    InsertInventoryTransaction(conn,tx,productId,"TRANSFER_IN",qty,toQty,toQty+qty,"STOCK_TRANSFER",id,userId,toWarehouseId,notes);
                    tx.Commit();return id;
                } catch { tx.Rollback(); throw; }
            }
        }

        public void SetReorder(int warehouseId,int productId,decimal? minimum,decimal? maximum,decimal? reorder)
        {
            ActionPermissionService.Demand("PRODUCT.STOCK_ADJUST");
            if(minimum<0||maximum<0||reorder<0)throw new InvalidOperationException("Reorder values cannot be negative.");
            if(minimum.HasValue&&maximum.HasValue&&minimum>maximum)throw new InvalidOperationException("Minimum stock cannot exceed maximum stock.");
            using(var conn=Open()) using(var cmd=new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.ProductStock WHERE warehouse_id=@w AND product_id=@p)
UPDATE dbo.ProductStock SET minimum_stock=@min,maximum_stock=@max,reorder_point=@reorder,updated_at=SYSUTCDATETIME() WHERE warehouse_id=@w AND product_id=@p
ELSE INSERT dbo.ProductStock(warehouse_id,product_id,quantity,minimum_stock,maximum_stock,reorder_point) VALUES(@w,@p,0,@min,@max,@reorder);",conn))
            {cmd.Parameters.AddWithValue("@w",warehouseId);cmd.Parameters.AddWithValue("@p",productId);AddNullableDecimal(cmd,"@min",minimum);AddNullableDecimal(cmd,"@max",maximum);AddNullableDecimal(cmd,"@reorder",reorder);cmd.ExecuteNonQuery();}
        }

        public void AddBarcode(int productId,int? productUnitId,string barcode,bool primary)
        {
            ActionPermissionService.Demand("PRODUCT.STOCK_ADJUST");
            if(string.IsNullOrWhiteSpace(barcode)) throw new InvalidOperationException("Barcode is required.");
            using(var conn=Open()) using(var tx=conn.BeginTransaction())
            { try { if(primary){using(var c=new SqlCommand("UPDATE dbo.ProductBarcodes SET is_primary=0 WHERE product_id=@p;",conn,tx)){c.Parameters.AddWithValue("@p",productId);c.ExecuteNonQuery();}}
                using(var c=new SqlCommand("INSERT dbo.ProductBarcodes(product_id,product_unit_id,barcode,is_primary) VALUES(@p,@u,@b,@primary);",conn,tx)){c.Parameters.AddWithValue("@p",productId);c.Parameters.AddWithValue("@u",productUnitId.HasValue?(object)productUnitId.Value:DBNull.Value);c.Parameters.AddWithValue("@b",barcode.Trim());c.Parameters.AddWithValue("@primary",primary);c.ExecuteNonQuery();} tx.Commit(); } catch{tx.Rollback();throw;} }
        }

        public void AddBatch(int productId,int warehouseId,string batchNumber,DateTime? expiry,decimal qty,decimal? cost)
        {
            ActionPermissionService.Demand("PRODUCT.STOCK_ADJUST");
            if(string.IsNullOrWhiteSpace(batchNumber)) throw new InvalidOperationException("Batch number is required."); if(qty<0) throw new InvalidOperationException("Batch quantity cannot be negative.");
            using(var conn=Open()) using(var cmd=new SqlCommand(@"IF EXISTS(SELECT 1 FROM dbo.ProductBatches WHERE product_id=@p AND warehouse_id=@w AND batch_number=@b)
UPDATE dbo.ProductBatches SET expiry_date=@e,quantity=@q,unit_cost=@cost WHERE product_id=@p AND warehouse_id=@w AND batch_number=@b
ELSE INSERT dbo.ProductBatches(product_id,warehouse_id,batch_number,expiry_date,quantity,unit_cost) VALUES(@p,@w,@b,@e,@q,@cost);",conn))
            {cmd.Parameters.AddWithValue("@p",productId);cmd.Parameters.AddWithValue("@w",warehouseId);cmd.Parameters.AddWithValue("@b",batchNumber.Trim());cmd.Parameters.AddWithValue("@e",expiry.HasValue?(object)expiry.Value.Date:DBNull.Value);AddDecimal(cmd,"@q",qty);AddNullableDecimal(cmd,"@cost",cost);cmd.ExecuteNonQuery();}
        }

        public void AddSerial(int productId,int warehouseId,string serial)
        {
            ActionPermissionService.Demand("PRODUCT.STOCK_ADJUST");
            if(string.IsNullOrWhiteSpace(serial)) throw new InvalidOperationException("Serial number is required.");
            using(var conn=Open()) using(var cmd=new SqlCommand("INSERT dbo.ProductSerials(product_id,warehouse_id,serial_number,status) VALUES(@p,@w,@s,N'IN_STOCK');",conn)){cmd.Parameters.AddWithValue("@p",productId);cmd.Parameters.AddWithValue("@w",warehouseId);cmd.Parameters.AddWithValue("@s",serial.Trim());cmd.ExecuteNonQuery();}
        }

        public int AddProductUnit(int productId,int unitId,decimal factor,bool isBase,decimal? price)
        {
            ActionPermissionService.Demand("PRODUCT.STOCK_ADJUST");
            if(factor<=0m)throw new InvalidOperationException("Unit conversion factor must be greater than zero.");
            using(var conn=Open()) using(var cmd=new SqlCommand(@"INSERT dbo.ProductUnits(product_id,unit_id,conversion_factor,is_base_unit,selling_price) VALUES(@p,@u,@f,@base,@price);SELECT CAST(SCOPE_IDENTITY() AS INT);",conn)){cmd.Parameters.AddWithValue("@p",productId);cmd.Parameters.AddWithValue("@u",unitId);AddDecimal(cmd,"@f",factor);cmd.Parameters.AddWithValue("@base",isBase);AddNullableDecimal(cmd,"@price",price);return Convert.ToInt32(cmd.ExecuteScalar());}
        }

        private SqlConnection Open(){var c=new SqlConnection(connectionString);c.Open();return c;}
        private DataTable Query(string sql){var t=new DataTable();using(var c=new SqlConnection(connectionString))using(var da=new SqlDataAdapter(sql,c))da.Fill(t);return t;}
        private decimal GetWarehouseQty(SqlConnection c,SqlTransaction tx,int w,int p,bool locked){using(var cmd=new SqlCommand("SELECT quantity FROM dbo.ProductStock "+(locked?"WITH (UPDLOCK,HOLDLOCK) ":"")+"WHERE warehouse_id=@w AND product_id=@p;",c,tx)){cmd.Parameters.AddWithValue("@w",w);cmd.Parameters.AddWithValue("@p",p);object v=cmd.ExecuteScalar();return v==null||v==DBNull.Value?0m:Convert.ToDecimal(v);}}
        private int GetLegacyQty(SqlConnection c,SqlTransaction tx,int p,bool locked){using(var cmd=new SqlCommand("SELECT stock_quantity FROM dbo.Products "+(locked?"WITH (UPDLOCK,HOLDLOCK) ":"")+"WHERE product_id=@p;",c,tx)){cmd.Parameters.AddWithValue("@p",p);return Convert.ToInt32(cmd.ExecuteScalar());}}
        private void UpsertWarehouseStock(SqlConnection c,SqlTransaction tx,int w,int p,decimal q){using(var cmd=new SqlCommand("IF EXISTS(SELECT 1 FROM dbo.ProductStock WHERE warehouse_id=@w AND product_id=@p) UPDATE dbo.ProductStock SET quantity=@q,updated_at=SYSUTCDATETIME() WHERE warehouse_id=@w AND product_id=@p ELSE INSERT dbo.ProductStock(warehouse_id,product_id,quantity) VALUES(@w,@p,@q);",c,tx)){cmd.Parameters.AddWithValue("@w",w);cmd.Parameters.AddWithValue("@p",p);AddDecimal(cmd,"@q",q);cmd.ExecuteNonQuery();}}
        private void InsertInventoryTransaction(SqlConnection c,SqlTransaction tx,int p,string type,decimal change,decimal oldQ,decimal newQ,string refType,int refId,int user,int warehouse,string notes){using(var cmd=new SqlCommand(@"INSERT dbo.InventoryTransactions(product_id,transaction_type,quantity_change,old_quantity,new_quantity,reference_type,reference_id,user_id,notes,created_at,warehouse_id,quantity_change_decimal,old_quantity_decimal,new_quantity_decimal) VALUES(@p,@t,@chg,@old,@new,@rt,@rid,@u,@notes,SYSUTCDATETIME(),@w,@chgd,@oldd,@newd);",c,tx)){cmd.Parameters.AddWithValue("@p",p);cmd.Parameters.AddWithValue("@t",type);cmd.Parameters.AddWithValue("@chg",Convert.ToInt32(Math.Round(change,0)));cmd.Parameters.AddWithValue("@old",Convert.ToInt32(Math.Round(oldQ,0)));cmd.Parameters.AddWithValue("@new",Convert.ToInt32(Math.Round(newQ,0)));cmd.Parameters.AddWithValue("@rt",refType);cmd.Parameters.AddWithValue("@rid",refId);cmd.Parameters.AddWithValue("@u",user);cmd.Parameters.AddWithValue("@notes",string.IsNullOrWhiteSpace(notes)?(object)DBNull.Value:notes);cmd.Parameters.AddWithValue("@w",warehouse);AddDecimal(cmd,"@chgd",change);AddDecimal(cmd,"@oldd",oldQ);AddDecimal(cmd,"@newd",newQ);cmd.ExecuteNonQuery();}}
        private static void AddDecimal(SqlCommand c,string n,decimal v){var p=c.Parameters.Add(n,SqlDbType.Decimal);p.Precision=24;p.Scale=8;p.Value=v;}
        private static void AddNullableDecimal(SqlCommand c,string n,decimal? v){var p=c.Parameters.Add(n,SqlDbType.Decimal);p.Precision=24;p.Scale=8;p.Value=v.HasValue?(object)v.Value:DBNull.Value;}
    }
}
