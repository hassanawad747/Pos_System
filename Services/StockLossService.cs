using System;
using System.Data;
using System.Data.SqlClient;

namespace Pos_System.Services
{
    internal sealed class StockLossService
    {
        private readonly string cs;
        public StockLossService(string connectionString){cs=connectionString;}

        public DataTable Warehouses()=>Query("SELECT warehouse_id,name FROM dbo.Warehouses WHERE is_active=1 ORDER BY name;");
        public DataTable Products()=>Query("SELECT product_id,name FROM dbo.Products ORDER BY name;");
        public DataTable Batches(int productId,int warehouseId)
        {
            var t=new DataTable();using(var c=new SqlConnection(cs))using(var da=new SqlDataAdapter("SELECT product_batch_id,batch_number,expiry_date,quantity FROM dbo.ProductBatches WHERE product_id=@p AND warehouse_id=@w ORDER BY expiry_date,batch_number;",c)){da.SelectCommand.Parameters.AddWithValue("@p",productId);da.SelectCommand.Parameters.AddWithValue("@w",warehouseId);da.Fill(t);}return t;
        }

        public int RecordLoss(int warehouseId,int productId,int quantity,string type,string batchNumber,string reason,int userId)
        {
            ActionPermissionService.Demand("PRODUCT.STOCK_ADJUST");
            type=(type??"").Trim().ToUpperInvariant();
            if(type!="DAMAGE"&&type!="EXPIRED")throw new InvalidOperationException("Loss type must be DAMAGE or EXPIRED.");
            if(quantity<=0)throw new InvalidOperationException("Quantity must be greater than zero.");

            using(var c=new SqlConnection(cs))
            {
                c.Open();using(var tx=c.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        decimal warehouseQty;
                        using(var cmd=new SqlCommand("SELECT quantity FROM dbo.ProductStock WITH(UPDLOCK,HOLDLOCK) WHERE warehouse_id=@w AND product_id=@p;",c,tx)){cmd.Parameters.AddWithValue("@w",warehouseId);cmd.Parameters.AddWithValue("@p",productId);object v=cmd.ExecuteScalar();warehouseQty=v==null||v==DBNull.Value?0m:Convert.ToDecimal(v);}
                        if(warehouseQty<quantity)throw new InvalidOperationException("Not enough stock in this warehouse.");

                        int legacyQty;
                        using(var cmd=new SqlCommand("SELECT stock_quantity FROM dbo.Products WITH(UPDLOCK,HOLDLOCK) WHERE product_id=@p;",c,tx)){cmd.Parameters.AddWithValue("@p",productId);legacyQty=Convert.ToInt32(cmd.ExecuteScalar());}
                        if(legacyQty<quantity)throw new InvalidOperationException("Legacy total stock is lower than requested loss quantity.");

                        if(!string.IsNullOrWhiteSpace(batchNumber))
                        {
                            decimal batchQty;
                            using(var cmd=new SqlCommand("SELECT quantity FROM dbo.ProductBatches WITH(UPDLOCK,HOLDLOCK) WHERE product_id=@p AND warehouse_id=@w AND batch_number=@b;",c,tx)){cmd.Parameters.AddWithValue("@p",productId);cmd.Parameters.AddWithValue("@w",warehouseId);cmd.Parameters.AddWithValue("@b",batchNumber.Trim());object v=cmd.ExecuteScalar();batchQty=v==null||v==DBNull.Value?0m:Convert.ToDecimal(v);}
                            if(batchQty<quantity)throw new InvalidOperationException("Selected batch does not contain enough quantity.");
                            using(var cmd=new SqlCommand("UPDATE dbo.ProductBatches SET quantity=quantity-@q WHERE product_id=@p AND warehouse_id=@w AND batch_number=@b;",c,tx)){cmd.Parameters.AddWithValue("@q",quantity);cmd.Parameters.AddWithValue("@p",productId);cmd.Parameters.AddWithValue("@w",warehouseId);cmd.Parameters.AddWithValue("@b",batchNumber.Trim());cmd.ExecuteNonQuery();}
                        }

                        using(var cmd=new SqlCommand("UPDATE dbo.ProductStock SET quantity=quantity-@q,updated_at=SYSUTCDATETIME() WHERE warehouse_id=@w AND product_id=@p;",c,tx)){cmd.Parameters.AddWithValue("@q",quantity);cmd.Parameters.AddWithValue("@w",warehouseId);cmd.Parameters.AddWithValue("@p",productId);cmd.ExecuteNonQuery();}
                        using(var cmd=new SqlCommand("UPDATE dbo.Products SET stock_quantity=stock_quantity-@q,updated_at=SYSUTCDATETIME() WHERE product_id=@p;",c,tx)){cmd.Parameters.AddWithValue("@q",quantity);cmd.Parameters.AddWithValue("@p",productId);cmd.ExecuteNonQuery();}

                        int newQty=legacyQty-quantity;
                        using(var cmd=new SqlCommand(@"INSERT dbo.InventoryTransactions(product_id,transaction_type,quantity_change,old_quantity,new_quantity,reference_type,user_id,notes,created_at,warehouse_id,batch_number)
VALUES(@p,@type,@change,@old,@new,N'STOCK_LOSS',@u,@notes,SYSUTCDATETIME(),@w,@batch);SELECT CAST(SCOPE_IDENTITY() AS INT);",c,tx))
                        {
                            cmd.Parameters.AddWithValue("@p",productId);cmd.Parameters.AddWithValue("@type",type);cmd.Parameters.AddWithValue("@change",-quantity);cmd.Parameters.AddWithValue("@old",legacyQty);cmd.Parameters.AddWithValue("@new",newQty);cmd.Parameters.AddWithValue("@u",userId);cmd.Parameters.AddWithValue("@notes",string.IsNullOrWhiteSpace(reason)?(object)DBNull.Value:reason.Trim());cmd.Parameters.AddWithValue("@w",warehouseId);cmd.Parameters.AddWithValue("@batch",string.IsNullOrWhiteSpace(batchNumber)?(object)DBNull.Value:batchNumber.Trim());int id=Convert.ToInt32(cmd.ExecuteScalar());tx.Commit();return id;
                        }
                    }
                    catch{tx.Rollback();throw;}
                }
            }
        }

        private DataTable Query(string sql){var t=new DataTable();using(var c=new SqlConnection(cs))using(var da=new SqlDataAdapter(sql,c))da.Fill(t);return t;}
    }
}
