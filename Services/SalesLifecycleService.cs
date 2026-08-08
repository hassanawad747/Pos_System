using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Pos_System.Services
{
    internal sealed class SalesLifecycleService
    {
        internal sealed class CartLine
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal OriginalUnitPrice { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal LineTotal { get; set; }
            public int? ProductUnitId { get; set; }
            public int? WarehouseId { get; set; }
            public string BatchNumber { get; set; }
            public string SerialNumber { get; set; }
        }

        internal sealed class ReturnLine
        {
            public int SaleItemId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int SoldQuantity { get; set; }
            public int AlreadyReturned { get; set; }
            public int ReturnQuantity { get; set; }
            public decimal UnitPrice { get; set; }
            public bool Restock { get; set; } = true;
        }

        internal sealed class ExchangeResult
        {
            public int ReturnId { get; set; }
            public int ReplacementSaleId { get; set; }
            public decimal ReturnedAmount { get; set; }
            public decimal ReplacementAmount { get; set; }
            public decimal Difference { get; set; }
        }

        internal sealed class CheckoutSourceData
        {
            public int SourceId { get; set; }
            public string SourceType { get; set; }
            public int? CustomerId { get; set; }
            public string Currency { get; set; }
            public decimal ExchangeRate { get; set; }
            public string Notes { get; set; }
            public List<CartLine> Lines { get; set; } = new List<CartLine>();
        }

        private readonly string connectionString;

        public SalesLifecycleService(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public int HoldSale(int? customerId, int userId, string currency, decimal exchangeRate, IEnumerable<CartLine> lines, string notes)
        {
            ActionPermissionService.Demand("SALE.CREATE");
            List<CartLine> items = ValidateCart(lines);
            string number = "HOLD-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            decimal subtotal = items.Sum(x => x.UnitPrice * x.Quantity);
            decimal discount = items.Sum(x => x.DiscountAmount);
            decimal tax = items.Sum(x => x.TaxAmount);
            decimal total = items.Sum(x => x.LineTotal);

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int id;
                        using (var cmd = new SqlCommand(@"
INSERT dbo.HeldSales(hold_number,customer_id,user_id,currency,exchange_rate,subtotal,discount_total,tax_total,total_amount,notes,status)
VALUES(@n,@c,@u,@cur,@rate,@sub,@disc,@tax,@total,@notes,N'HELD');
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@n", number);
                            cmd.Parameters.AddWithValue("@c", customerId.HasValue ? (object)customerId.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@u", userId);
                            cmd.Parameters.AddWithValue("@cur", currency);
                            AddDecimal(cmd, "@rate", exchangeRate);
                            AddDecimal(cmd, "@sub", subtotal); AddDecimal(cmd, "@disc", discount); AddDecimal(cmd, "@tax", tax); AddDecimal(cmd, "@total", total);
                            cmd.Parameters.AddWithValue("@notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes.Trim());
                            id = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        foreach (CartLine line in items)
                        {
                            using (var cmd = new SqlCommand(@"
INSERT dbo.HeldSaleItems(held_sale_id,product_id,quantity,unit_price,original_unit_price,discount_amount,tax_amount,line_total,product_unit_id,warehouse_id,batch_number,serial_number)
VALUES(@h,@p,@q,@price,@orig,@disc,@tax,@total,@unit,@warehouse,@batch,@serial);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@h", id); cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.Parameters.AddWithValue("@q", line.Quantity);
                                AddDecimal(cmd, "@price", line.UnitPrice); AddDecimal(cmd, "@orig", line.OriginalUnitPrice); AddDecimal(cmd, "@disc", line.DiscountAmount); AddDecimal(cmd, "@tax", line.TaxAmount); AddDecimal(cmd, "@total", line.LineTotal);
                                cmd.Parameters.AddWithValue("@unit",line.ProductUnitId.HasValue?(object)line.ProductUnitId.Value:DBNull.Value);cmd.Parameters.AddWithValue("@warehouse",line.WarehouseId.HasValue?(object)line.WarehouseId.Value:DBNull.Value);cmd.Parameters.AddWithValue("@batch",string.IsNullOrWhiteSpace(line.BatchNumber)?(object)DBNull.Value:line.BatchNumber);cmd.Parameters.AddWithValue("@serial",string.IsNullOrWhiteSpace(line.SerialNumber)?(object)DBNull.Value:line.SerialNumber);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit();
                        return id;
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }

        public int SaveQuotation(int? customerId, int userId, string currency, decimal exchangeRate, IEnumerable<CartLine> lines, DateTime? validUntil, string notes)
        {
            ActionPermissionService.Demand("SALE.CREATE");
            List<CartLine> items = ValidateCart(lines);
            string number = "QTN-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            decimal subtotal = items.Sum(x => x.UnitPrice * x.Quantity);
            decimal discount = items.Sum(x => x.DiscountAmount);
            decimal tax = items.Sum(x => x.TaxAmount);
            decimal total = items.Sum(x => x.LineTotal);
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        int id;
                        using (var cmd = new SqlCommand(@"
INSERT dbo.Quotations(quotation_number,customer_id,user_id,currency,exchange_rate,subtotal,discount_total,tax_total,total_amount,status,valid_until,notes)
VALUES(@n,@c,@u,@cur,@rate,@sub,@disc,@tax,@total,N'OPEN',@valid,@notes);
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@n", number); cmd.Parameters.AddWithValue("@c", customerId.HasValue ? (object)customerId.Value : DBNull.Value); cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@cur", currency);
                            AddDecimal(cmd, "@rate", exchangeRate); AddDecimal(cmd, "@sub", subtotal); AddDecimal(cmd, "@disc", discount); AddDecimal(cmd, "@tax", tax); AddDecimal(cmd, "@total", total);
                            cmd.Parameters.AddWithValue("@valid", validUntil.HasValue ? (object)validUntil.Value : DBNull.Value); cmd.Parameters.AddWithValue("@notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes.Trim());
                            id = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        foreach (CartLine line in items)
                        {
                            using (var cmd = new SqlCommand(@"
INSERT dbo.QuotationItems(quotation_id,product_id,quantity,unit_price,original_unit_price,discount_amount,tax_amount,line_total,product_unit_id,warehouse_id,batch_number,serial_number)
VALUES(@qtn,@p,@q,@price,@orig,@disc,@tax,@total,@unit,@warehouse,@batch,@serial);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@qtn", id); cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.Parameters.AddWithValue("@q", line.Quantity);
                                AddDecimal(cmd, "@price", line.UnitPrice);AddDecimal(cmd,"@orig",line.OriginalUnitPrice); AddDecimal(cmd, "@disc", line.DiscountAmount); AddDecimal(cmd, "@tax", line.TaxAmount); AddDecimal(cmd, "@total", line.LineTotal);
                                cmd.Parameters.AddWithValue("@unit",line.ProductUnitId.HasValue?(object)line.ProductUnitId.Value:DBNull.Value);cmd.Parameters.AddWithValue("@warehouse",line.WarehouseId.HasValue?(object)line.WarehouseId.Value:DBNull.Value);cmd.Parameters.AddWithValue("@batch",string.IsNullOrWhiteSpace(line.BatchNumber)?(object)DBNull.Value:line.BatchNumber);cmd.Parameters.AddWithValue("@serial",string.IsNullOrWhiteSpace(line.SerialNumber)?(object)DBNull.Value:line.SerialNumber);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        tx.Commit(); return id;
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }

        public DataTable GetHeldSales()
        {
            return Query(@"SELECT h.held_sale_id,h.hold_number,c.name AS customer,h.currency,h.total_amount,h.status,h.held_at,u.username
FROM dbo.HeldSales h LEFT JOIN dbo.Customers c ON c.customer_id=h.customer_id JOIN dbo.Users u ON u.user_id=h.user_id
WHERE h.status=N'HELD' ORDER BY h.held_at DESC;");
        }

        public DataTable GetQuotations()
        {
            return Query(@"SELECT q.quotation_id,q.quotation_number,c.name AS customer,q.currency,q.total_amount,q.status,q.valid_until,q.created_at,u.username
FROM dbo.Quotations q LEFT JOIN dbo.Customers c ON c.customer_id=q.customer_id JOIN dbo.Users u ON u.user_id=q.user_id
ORDER BY q.created_at DESC;");
        }

        public List<CartLine> LoadHeldItems(int heldSaleId)
        {
            var list = new List<CartLine>();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"SELECT i.product_id,p.name,i.quantity,i.unit_price,i.original_unit_price,i.discount_amount,i.tax_amount,i.line_total,i.product_unit_id,i.warehouse_id,i.batch_number,i.serial_number
FROM dbo.HeldSaleItems i JOIN dbo.Products p ON p.product_id=i.product_id WHERE i.held_sale_id=@id ORDER BY i.held_sale_item_id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", heldSaleId); conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new CartLine { ProductId = Convert.ToInt32(r[0]), ProductName = Convert.ToString(r[1]), Quantity = Convert.ToInt32(r[2]), UnitPrice = Convert.ToDecimal(r[3]), OriginalUnitPrice = r[4] == DBNull.Value ? Convert.ToDecimal(r[3]) : Convert.ToDecimal(r[4]), DiscountAmount = Convert.ToDecimal(r[5]), TaxAmount = Convert.ToDecimal(r[6]), LineTotal = Convert.ToDecimal(r[7]),ProductUnitId=r[8]==DBNull.Value?(int?)null:Convert.ToInt32(r[8]),WarehouseId=r[9]==DBNull.Value?(int?)null:Convert.ToInt32(r[9]),BatchNumber=r[10]==DBNull.Value?null:Convert.ToString(r[10]),SerialNumber=r[11]==DBNull.Value?null:Convert.ToString(r[11]) });
            }
            return list;
        }

        public CheckoutSourceData LoadHeldSale(int heldSaleId)
        {
            var data = new CheckoutSourceData { SourceId = heldSaleId, SourceType = "HELD" };
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("SELECT customer_id,currency,ISNULL(exchange_rate,0),notes,status FROM dbo.HeldSales WHERE held_sale_id=@id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", heldSaleId); conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) throw new InvalidOperationException("Held sale was not found.");
                    if (!string.Equals(Convert.ToString(reader[4]), "HELD", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Held sale is no longer available.");
                    data.CustomerId = reader[0] == DBNull.Value ? (int?)null : Convert.ToInt32(reader[0]);
                    data.Currency = Convert.ToString(reader[1]); data.ExchangeRate = Convert.ToDecimal(reader[2]); data.Notes = reader[3] == DBNull.Value ? null : Convert.ToString(reader[3]);
                }
            }
            data.Lines = LoadHeldItems(heldSaleId);
            return data;
        }

        public CheckoutSourceData LoadQuotation(int quotationId)
        {
            var data = new CheckoutSourceData { SourceId = quotationId, SourceType = "QUOTATION" };
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT customer_id,currency,ISNULL(exchange_rate,0),notes,status,valid_until FROM dbo.Quotations WHERE quotation_id=@id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", quotationId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) throw new InvalidOperationException("Quotation was not found.");
                        if (!string.Equals(Convert.ToString(reader[4]), "OPEN", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Quotation is no longer open.");
                        if (reader[5] != DBNull.Value && Convert.ToDateTime(reader[5]) < DateTime.UtcNow) throw new InvalidOperationException("Quotation has expired.");
                        data.CustomerId = reader[0] == DBNull.Value ? (int?)null : Convert.ToInt32(reader[0]); data.Currency = Convert.ToString(reader[1]); data.ExchangeRate = Convert.ToDecimal(reader[2]); data.Notes = reader[3] == DBNull.Value ? null : Convert.ToString(reader[3]);
                    }
                }
                using (var cmd = new SqlCommand(@"SELECT i.product_id,p.name,i.quantity,i.unit_price,i.original_unit_price,i.discount_amount,i.tax_amount,i.line_total,i.product_unit_id,i.warehouse_id,i.batch_number,i.serial_number
FROM dbo.QuotationItems i JOIN dbo.Products p ON p.product_id=i.product_id WHERE i.quotation_id=@id ORDER BY i.quotation_item_id;", conn))
                {
                    cmd.Parameters.AddWithValue("@id", quotationId);
                    using (var reader = cmd.ExecuteReader()) while (reader.Read()) data.Lines.Add(new CartLine { ProductId = Convert.ToInt32(reader[0]), ProductName = Convert.ToString(reader[1]), Quantity = Convert.ToInt32(reader[2]), UnitPrice = Convert.ToDecimal(reader[3]), OriginalUnitPrice = reader[4]==DBNull.Value?Convert.ToDecimal(reader[3]):Convert.ToDecimal(reader[4]), DiscountAmount = Convert.ToDecimal(reader[5]), TaxAmount = Convert.ToDecimal(reader[6]), LineTotal = Convert.ToDecimal(reader[7]),ProductUnitId=reader[8]==DBNull.Value?(int?)null:Convert.ToInt32(reader[8]),WarehouseId=reader[9]==DBNull.Value?(int?)null:Convert.ToInt32(reader[9]),BatchNumber=reader[10]==DBNull.Value?null:Convert.ToString(reader[10]),SerialNumber=reader[11]==DBNull.Value?null:Convert.ToString(reader[11]) });
                }
            }
            return data;
        }

        public void MarkHeldSaleResumed(int heldSaleId)
        {
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand("UPDATE dbo.HeldSales SET status=N'RESUMED',resumed_at=SYSUTCDATETIME() WHERE held_sale_id=@id AND status=N'HELD';", conn))
            { cmd.Parameters.AddWithValue("@id", heldSaleId); conn.Open(); cmd.ExecuteNonQuery(); }
        }

        public List<ReturnLine> GetReturnableSaleItems(int saleId)
        {
            var list = new List<ReturnLine>();
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(@"
SELECT si.sale_item_id,si.product_id,ISNULL(si.name_product,p.name),si.quantity,si.unit_price,
       ISNULL((SELECT SUM(ri.quantity) FROM dbo.SalesReturnItems ri JOIN dbo.SalesReturns r ON r.sales_return_id=ri.sales_return_id WHERE r.sale_id=@sale AND ri.sale_item_id=si.sale_item_id AND r.status=N'COMPLETED'),0) returned_qty
FROM dbo.Sale_Items si JOIN dbo.Products p ON p.product_id=si.product_id WHERE si.sale_id=@sale ORDER BY si.sale_item_id;", conn))
            {
                cmd.Parameters.AddWithValue("@sale", saleId); conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new ReturnLine { SaleItemId = Convert.ToInt32(r[0]), ProductId = Convert.ToInt32(r[1]), ProductName = Convert.ToString(r[2]), SoldQuantity = Convert.ToInt32(r[3]), UnitPrice = Convert.ToDecimal(r[4]), AlreadyReturned = Convert.ToInt32(r[5]) });
            }
            return list;
        }

        public int ProcessReturn(int saleId, int userId, string returnType, string refundMethod, string reason, IEnumerable<ReturnLine> selectedLines)
        {
            ActionPermissionService.Demand("SALE.RETURN");
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open(); using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try { decimal total; int? customer; string currency; decimal? rate; int id = ProcessReturnWithinTransaction(conn, tx, saleId, userId, returnType, refundMethod, reason, selectedLines, out total, out customer, out currency, out rate); tx.Commit(); return id; }
                    catch { tx.Rollback(); throw; }
                }
            }
        }

        public ExchangeResult ProcessExchange(int saleId, int userId, string reason, IEnumerable<ReturnLine> returnedLines, CheckoutService.Request replacement)
        {
            ActionPermissionService.Demand("SALE.EXCHANGE");
            if (replacement == null) throw new ArgumentNullException(nameof(replacement));
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open(); using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        decimal returned; int? customer; string currency; decimal? rate;
                        int returnId = ProcessReturnWithinTransaction(conn, tx, saleId, userId, "EXCHANGE", "CUSTOMER_CREDIT", reason, returnedLines, out returned, out customer, out currency, out rate);
                        replacement.UserId = userId; replacement.CustomerId = customer; replacement.Currency = currency; replacement.ExchangeRate = rate ?? replacement.ExchangeRate;
                        if (customer.HasValue) using (var nameCommand = new SqlCommand("SELECT name FROM dbo.Customers WHERE customer_id=@id;", conn, tx)) { nameCommand.Parameters.AddWithValue("@id", customer.Value); replacement.CustomerName = Convert.ToString(nameCommand.ExecuteScalar()); }
                        CheckoutService.Result sale = new CheckoutService(connectionString).CompleteWithinTransaction(conn, tx, replacement);
                        decimal difference = sale.Total - returned;
                        using (var cmd = new SqlCommand("UPDATE dbo.SalesReturns SET exchange_sale_id=@sale,refund_status=N'APPLIED_TO_EXCHANGE' WHERE sales_return_id=@return; INSERT dbo.SaleExchangeLinks(sales_return_id,replacement_sale_id,original_total,replacement_total,amount_due,currency) VALUES(@return,@sale,@returned,@replacement,@difference,@currency);", conn, tx))
                        { cmd.Parameters.AddWithValue("@sale", sale.SaleId); cmd.Parameters.AddWithValue("@return", returnId); AddDecimal(cmd, "@returned", returned); AddDecimal(cmd, "@replacement", sale.Total); AddDecimal(cmd, "@difference", difference); cmd.Parameters.AddWithValue("@currency", currency); cmd.ExecuteNonQuery(); }
                        tx.Commit(); return new ExchangeResult { ReturnId = returnId, ReplacementSaleId = sale.SaleId, ReturnedAmount = returned, ReplacementAmount = sale.Total, Difference = difference };
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
        }

        private int ProcessReturnWithinTransaction(SqlConnection conn, SqlTransaction tx, int saleId, int userId, string returnType, string refundMethod, string reason, IEnumerable<ReturnLine> selectedLines, out decimal total, out int? customerId, out string currency, out decimal? exchangeRate)
        {
            List<ReturnLine> lines = (selectedLines ?? Enumerable.Empty<ReturnLine>()).Where(x => x != null && x.ReturnQuantity > 0).ToList();
            if (lines.Count == 0) throw new InvalidOperationException("Select at least one quantity to return.");
            string type = string.IsNullOrWhiteSpace(returnType) ? "RETURN" : returnType.Trim().ToUpperInvariant();
            string method = string.IsNullOrWhiteSpace(refundMethod) ? "CUSTOMER_CREDIT" : refundMethod.Trim().ToUpperInvariant();
            string[] methods = { "CASH", "CARD", "BANK", "CUSTOMER_CREDIT", "OTHER" };
            if (!methods.Contains(method)) throw new InvalidOperationException("Unsupported refund destination.");
            if (type == "EXCHANGE") ActionPermissionService.Demand("SALE.EXCHANGE");
            if (method != "CUSTOMER_CREDIT") ActionPermissionService.Demand("SALE.REFUND");

            decimal saleTotal;
            using (var cmd = new SqlCommand("SELECT customer_id,ISNULL(NULLIF(currency,N''),CASE WHEN balance_lb IS NOT NULL AND balance_usd IS NULL THEN N'LBP' ELSE N'USD' END),exchange_rate,total_amount FROM dbo.Sales WITH(UPDLOCK,HOLDLOCK) WHERE sale_id=@id AND ISNULL(sale_status,N'COMPLETED')<>N'VOID';", conn, tx))
            {
                cmd.Parameters.AddWithValue("@id", saleId); using (var r = cmd.ExecuteReader()) { if (!r.Read()) throw new InvalidOperationException("Sale was not found or is void."); customerId = r[0] == DBNull.Value ? (int?)null : Convert.ToInt32(r[0]); currency = Convert.ToString(r[1]); exchangeRate = r[2] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r[2]); saleTotal = Convert.ToDecimal(r[3]); }
            }

            total = 0m;
            foreach (ReturnLine line in lines)
            {
                using (var cmd = new SqlCommand(@"SELECT si.product_id,si.quantity,si.unit_price,ISNULL((SELECT SUM(ri.quantity) FROM dbo.SalesReturnItems ri JOIN dbo.SalesReturns sr ON sr.sales_return_id=ri.sales_return_id WHERE ri.sale_item_id=si.sale_item_id AND sr.status=N'COMPLETED'),0)
FROM dbo.Sale_Items si WITH(UPDLOCK,HOLDLOCK) WHERE si.sale_item_id=@item AND si.sale_id=@sale;", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@item", line.SaleItemId); cmd.Parameters.AddWithValue("@sale", saleId);
                    using (var r = cmd.ExecuteReader()) { if (!r.Read()) throw new InvalidOperationException("A selected sale line does not belong to this sale."); line.ProductId = Convert.ToInt32(r[0]); line.SoldQuantity = Convert.ToInt32(r[1]); line.UnitPrice = Convert.ToDecimal(r[2]); line.AlreadyReturned = Convert.ToInt32(r[3]); }
                }
                if (line.ReturnQuantity > line.SoldQuantity - line.AlreadyReturned) throw new InvalidOperationException("Return quantity exceeds the quantity still available for return.");
                total += line.UnitPrice * line.ReturnQuantity;
            }

            string number = "RET-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 5).ToUpperInvariant(); int returnId;
            using (var cmd = new SqlCommand(@"INSERT dbo.SalesReturns(return_number,sale_id,customer_id,user_id,return_type,refund_method,currency,exchange_rate,total_amount,reason,status,refund_status)
VALUES(@n,@sale,@customer,@user,@type,@method,@currency,@rate,@total,@reason,N'COMPLETED',@refundStatus);SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
            { cmd.Parameters.AddWithValue("@n", number); cmd.Parameters.AddWithValue("@sale", saleId); cmd.Parameters.AddWithValue("@customer", customerId.HasValue ? (object)customerId.Value : DBNull.Value); cmd.Parameters.AddWithValue("@user", userId); cmd.Parameters.AddWithValue("@type", type); cmd.Parameters.AddWithValue("@method", method); cmd.Parameters.AddWithValue("@currency", currency); cmd.Parameters.AddWithValue("@rate", exchangeRate.HasValue ? (object)exchangeRate.Value : DBNull.Value); AddDecimal(cmd, "@total", total); cmd.Parameters.AddWithValue("@reason", string.IsNullOrWhiteSpace(reason) ? (object)DBNull.Value : reason.Trim()); cmd.Parameters.AddWithValue("@refundStatus", method == "CUSTOMER_CREDIT" ? "CREDITED" : "REFUNDED"); returnId = Convert.ToInt32(cmd.ExecuteScalar()); }

            int defaultWarehouse;
            using (var cmd = new SqlCommand("SELECT TOP(1) warehouse_id FROM dbo.Warehouses WHERE is_active=1 ORDER BY is_default DESC,warehouse_id;", conn, tx)) defaultWarehouse = Convert.ToInt32(cmd.ExecuteScalar());
            foreach (ReturnLine line in lines)
            {
                int warehouseId = defaultWarehouse; decimal baseQty = line.ReturnQuantity;
                using (var cmd = new SqlCommand("SELECT ISNULL(warehouse_id,@default),ISNULL(base_quantity,quantity) FROM dbo.Sale_Items WHERE sale_item_id=@item;", conn, tx)) { cmd.Parameters.AddWithValue("@default", defaultWarehouse); cmd.Parameters.AddWithValue("@item", line.SaleItemId); using (var r = cmd.ExecuteReader()) if (r.Read()) { warehouseId = Convert.ToInt32(r[0]); baseQty = Convert.ToDecimal(r[1]) * line.ReturnQuantity / Math.Max(1, line.SoldQuantity); } }
                using (var cmd = new SqlCommand("INSERT dbo.SalesReturnItems(sales_return_id,sale_item_id,product_id,quantity,unit_price,line_total,restock) VALUES(@r,@si,@p,@q,@price,@total,@restock);", conn, tx)) { cmd.Parameters.AddWithValue("@r", returnId); cmd.Parameters.AddWithValue("@si", line.SaleItemId); cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.Parameters.AddWithValue("@q", line.ReturnQuantity); AddDecimal(cmd, "@price", line.UnitPrice); AddDecimal(cmd, "@total", line.UnitPrice * line.ReturnQuantity); cmd.Parameters.AddWithValue("@restock", line.Restock); cmd.ExecuteNonQuery(); }
                if (line.Restock) RestockLine(conn, tx, returnId, userId, line, warehouseId, baseQty);
            }

            decimal finalBalance = 0m;
            if (customerId.HasValue)
            {
                decimal before = GetCustomerBalance(conn, tx, customerId.Value, currency); decimal credited = before - total; finalBalance = method == "CUSTOMER_CREDIT" ? credited : before;
                using (var cmd = new SqlCommand("INSERT dbo.CustomerTransactions(customer_id,transaction_type,reference_type,reference_id,debit,credit,balance_after,currency,description,user_id) VALUES(@c,N'RETURN',N'SALES_RETURN',@r,0,@amount,@balance,@currency,N'Sales return credit',@u);", conn, tx)) { cmd.Parameters.AddWithValue("@c", customerId.Value); cmd.Parameters.AddWithValue("@r", returnId); AddDecimal(cmd, "@amount", total); AddDecimal(cmd, "@balance", credited); cmd.Parameters.AddWithValue("@currency", currency); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
                if (method != "CUSTOMER_CREDIT") using (var cmd = new SqlCommand("INSERT dbo.CustomerTransactions(customer_id,transaction_type,reference_type,reference_id,debit,credit,balance_after,currency,description,user_id) VALUES(@c,N'REFUND',N'SALES_RETURN',@r,@amount,0,@balance,@currency,N'Refund paid to customer',@u);", conn, tx)) { cmd.Parameters.AddWithValue("@c", customerId.Value); cmd.Parameters.AddWithValue("@r", returnId); AddDecimal(cmd, "@amount", total); AddDecimal(cmd, "@balance", finalBalance); cmd.Parameters.AddWithValue("@currency", currency); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
                string column = currency == "LBP" ? "balance_lb" : "balance_usd"; using (var cmd = new SqlCommand("UPDATE dbo.Customers SET " + column + "=@balance,balance_updated_at=SYSUTCDATETIME() WHERE customer_id=@c;", conn, tx)) { AddDecimal(cmd, "@balance", finalBalance); cmd.Parameters.AddWithValue("@c", customerId.Value); cmd.ExecuteNonQuery(); }
                ReverseLoyalty(conn, tx, saleId, returnId, userId, customerId.Value, total, saleTotal);
            }

            int? cashSession = null;
            if (method == "CASH")
            {
                using (var cmd = new SqlCommand("SELECT TOP(1) cash_session_id FROM dbo.CashSessions WITH(UPDLOCK,HOLDLOCK) WHERE user_id=@u AND status=N'OPEN' ORDER BY opened_at DESC;", conn, tx)) { cmd.Parameters.AddWithValue("@u", userId); object value = cmd.ExecuteScalar(); if (value == null || value == DBNull.Value) throw new InvalidOperationException("Open a Cash Shift before issuing a CASH refund."); cashSession = Convert.ToInt32(value); }
                using (var cmd = new SqlCommand("INSERT dbo.CashMovements(cash_session_id,movement_type,amount,currency,reference_type,reference_id,description,user_id,created_at) VALUES(@s,N'REFUND',@amount,@currency,N'SALES_RETURN',@r,N'Sales return cash refund',@u,SYSUTCDATETIME());", conn, tx)) { cmd.Parameters.AddWithValue("@s", cashSession.Value); AddDecimal(cmd, "@amount", -total); cmd.Parameters.AddWithValue("@currency", currency); cmd.Parameters.AddWithValue("@r", returnId); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
            }
            using (var cmd = new SqlCommand("INSERT dbo.RefundTransactions(sales_return_id,refund_method,amount,currency,exchange_rate,cash_session_id,user_id) VALUES(@r,@method,@amount,@currency,@rate,@session,@u);", conn, tx)) { cmd.Parameters.AddWithValue("@r", returnId); cmd.Parameters.AddWithValue("@method", method); AddDecimal(cmd, "@amount", total); cmd.Parameters.AddWithValue("@currency", currency); cmd.Parameters.AddWithValue("@rate", exchangeRate.HasValue ? (object)exchangeRate.Value : DBNull.Value); cmd.Parameters.AddWithValue("@session", cashSession.HasValue ? (object)cashSession.Value : DBNull.Value); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
            return returnId;
        }

        private static void RestockLine(SqlConnection conn, SqlTransaction tx, int returnId, int userId, ReturnLine line, int warehouseId, decimal baseQty)
        {
            decimal old; using (var cmd = new SqlCommand("SELECT quantity FROM dbo.ProductStock WITH(UPDLOCK,HOLDLOCK) WHERE warehouse_id=@w AND product_id=@p;", conn, tx)) { cmd.Parameters.AddWithValue("@w", warehouseId); cmd.Parameters.AddWithValue("@p", line.ProductId); object value = cmd.ExecuteScalar(); old = value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value); }
            using (var cmd = new SqlCommand("IF EXISTS(SELECT 1 FROM dbo.ProductStock WHERE warehouse_id=@w AND product_id=@p) UPDATE dbo.ProductStock SET quantity=quantity+@q,updated_at=SYSUTCDATETIME() WHERE warehouse_id=@w AND product_id=@p ELSE INSERT dbo.ProductStock(warehouse_id,product_id,quantity) VALUES(@w,@p,@q);", conn, tx)) { cmd.Parameters.AddWithValue("@w", warehouseId); cmd.Parameters.AddWithValue("@p", line.ProductId); AddDecimal(cmd, "@q", baseQty); cmd.ExecuteNonQuery(); }
            using (var cmd = new SqlCommand("UPDATE p SET stock_quantity_decimal=x.total,stock_quantity=CONVERT(INT,FLOOR(x.total)),updated_at=SYSUTCDATETIME() FROM dbo.Products p CROSS APPLY(SELECT ISNULL(SUM(quantity),0) total FROM dbo.ProductStock WHERE product_id=p.product_id)x WHERE p.product_id=@p;", conn, tx)) { cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.ExecuteNonQuery(); }
            using (var cmd = new SqlCommand("INSERT dbo.InventoryTransactions(product_id,transaction_type,quantity_change,old_quantity,new_quantity,quantity_change_decimal,old_quantity_decimal,new_quantity_decimal,reference_type,reference_id,user_id,notes,created_at,warehouse_id) VALUES(@p,N'RETURN_IN',@legacy,@legacyOld,@legacyNew,@q,@old,@new,N'SALES_RETURN',@r,@u,N'Sales return restock',SYSUTCDATETIME(),@w);", conn, tx)) { cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.Parameters.AddWithValue("@legacy", Convert.ToInt32(Math.Round(baseQty, 0))); cmd.Parameters.AddWithValue("@legacyOld", Convert.ToInt32(Math.Floor(old))); cmd.Parameters.AddWithValue("@legacyNew", Convert.ToInt32(Math.Floor(old + baseQty))); AddDecimal(cmd, "@q", baseQty); AddDecimal(cmd, "@old", old); AddDecimal(cmd, "@new", old + baseQty); cmd.Parameters.AddWithValue("@r", returnId); cmd.Parameters.AddWithValue("@u", userId); cmd.Parameters.AddWithValue("@w", warehouseId); cmd.ExecuteNonQuery(); }
            using (var cmd = new SqlCommand("UPDATE dbo.ProductSerials SET status=N'IN_STOCK',sale_id=NULL,sale_item_id=NULL WHERE sale_item_id=@item AND product_id=@p;", conn, tx)) { cmd.Parameters.AddWithValue("@item", line.SaleItemId); cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.ExecuteNonQuery(); }
        }

        private static void ReverseLoyalty(SqlConnection conn, SqlTransaction tx, int saleId, int returnId, int userId, int customerId, decimal returnTotal, decimal saleTotal)
        {
            if (saleTotal <= 0m) return; decimal ratio = Math.Min(1m, returnTotal / saleTotal); decimal earned; decimal redeemed;
            using (var cmd = new SqlCommand("SELECT ISNULL(loyalty_points_earned,0),ISNULL(loyalty_points_redeemed,0) FROM dbo.Sales WHERE sale_id=@sale;", conn, tx)) { cmd.Parameters.AddWithValue("@sale", saleId); using (var r = cmd.ExecuteReader()) { if (!r.Read()) return; earned = Convert.ToDecimal(r[0]) * ratio; redeemed = Convert.ToDecimal(r[1]) * ratio; } }
            decimal delta = redeemed - earned; if (delta == 0m) return;
            using (var cmd = new SqlCommand(@"DECLARE @id INT,@before DECIMAL(24,8);SELECT @id=loyalty_account_id,@before=points_balance FROM dbo.LoyaltyAccounts WITH(UPDLOCK,HOLDLOCK) WHERE customer_id=@customer;
IF @id IS NOT NULL BEGIN DECLARE @after DECIMAL(24,8)=CASE WHEN @before+@delta<0 THEN 0 ELSE @before+@delta END;UPDATE dbo.LoyaltyAccounts SET points_balance=@after,updated_at=SYSUTCDATETIME() WHERE loyalty_account_id=@id;INSERT dbo.LoyaltyTransactions(loyalty_account_id,transaction_type,points,balance_after,reference_type,reference_id,description,user_id) VALUES(@id,N'RETURN_REVERSAL',@after-@before,@after,N'SALES_RETURN',@return,N'Prorated loyalty reversal',@user);END;", conn, tx)) { cmd.Parameters.AddWithValue("@customer", customerId); AddDecimal(cmd, "@delta", delta); cmd.Parameters.AddWithValue("@return", returnId); cmd.Parameters.AddWithValue("@user", userId); cmd.ExecuteNonQuery(); }
        }

        private decimal GetCustomerBalance(SqlConnection conn, SqlTransaction tx, int customerId, string currency)
        {
            using (var cmd = new SqlCommand("SELECT TOP(1) balance_after FROM dbo.CustomerTransactions WHERE customer_id=@c AND currency=@cur ORDER BY customer_transaction_id DESC;", conn, tx))
            { cmd.Parameters.AddWithValue("@c", customerId); cmd.Parameters.AddWithValue("@cur", currency); object v = cmd.ExecuteScalar(); return v == null || v == DBNull.Value ? 0m : Convert.ToDecimal(v); }
        }

        private DataTable Query(string sql)
        {
            var table = new DataTable(); using (var conn = new SqlConnection(connectionString)) using (var da = new SqlDataAdapter(sql, conn)) da.Fill(table); return table;
        }

        private static List<CartLine> ValidateCart(IEnumerable<CartLine> lines)
        {
            List<CartLine> items = (lines ?? Enumerable.Empty<CartLine>()).Where(x => x != null && x.Quantity > 0).ToList();
            if (items.Count == 0) throw new InvalidOperationException("The cart is empty.");
            foreach (CartLine line in items) { if (line.ProductId <= 0) throw new InvalidOperationException("A cart product is invalid."); if (line.UnitPrice < 0 || line.LineTotal < 0) throw new InvalidOperationException("Cart prices cannot be negative."); }
            return items;
        }

        private static void AddDecimal(SqlCommand cmd, string name, decimal value)
        {
            SqlParameter p = cmd.Parameters.Add(name, SqlDbType.Decimal); p.Precision = 24; p.Scale = 8; p.Value = value;
        }
    }
}
