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

        private readonly string connectionString;

        public SalesLifecycleService(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public int HoldSale(int? customerId, int userId, string currency, decimal exchangeRate, IEnumerable<CartLine> lines, string notes)
        {
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
INSERT dbo.HeldSaleItems(held_sale_id,product_id,quantity,unit_price,original_unit_price,discount_amount,tax_amount,line_total)
VALUES(@h,@p,@q,@price,@orig,@disc,@tax,@total);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@h", id); cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.Parameters.AddWithValue("@q", line.Quantity);
                                AddDecimal(cmd, "@price", line.UnitPrice); AddDecimal(cmd, "@orig", line.OriginalUnitPrice); AddDecimal(cmd, "@disc", line.DiscountAmount); AddDecimal(cmd, "@tax", line.TaxAmount); AddDecimal(cmd, "@total", line.LineTotal);
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
INSERT dbo.QuotationItems(quotation_id,product_id,quantity,unit_price,discount_amount,tax_amount,line_total)
VALUES(@qtn,@p,@q,@price,@disc,@tax,@total);", conn, tx))
                            {
                                cmd.Parameters.AddWithValue("@qtn", id); cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.Parameters.AddWithValue("@q", line.Quantity);
                                AddDecimal(cmd, "@price", line.UnitPrice); AddDecimal(cmd, "@disc", line.DiscountAmount); AddDecimal(cmd, "@tax", line.TaxAmount); AddDecimal(cmd, "@total", line.LineTotal);
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
            using (var cmd = new SqlCommand(@"SELECT i.product_id,p.name,i.quantity,i.unit_price,i.original_unit_price,i.discount_amount,i.tax_amount,i.line_total
FROM dbo.HeldSaleItems i JOIN dbo.Products p ON p.product_id=i.product_id WHERE i.held_sale_id=@id ORDER BY i.held_sale_item_id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", heldSaleId); conn.Open();
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) list.Add(new CartLine { ProductId = Convert.ToInt32(r[0]), ProductName = Convert.ToString(r[1]), Quantity = Convert.ToInt32(r[2]), UnitPrice = Convert.ToDecimal(r[3]), OriginalUnitPrice = r[4] == DBNull.Value ? Convert.ToDecimal(r[3]) : Convert.ToDecimal(r[4]), DiscountAmount = Convert.ToDecimal(r[5]), TaxAmount = Convert.ToDecimal(r[6]), LineTotal = Convert.ToDecimal(r[7]) });
            }
            return list;
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
            List<ReturnLine> lines = selectedLines.Where(x => x.ReturnQuantity > 0).ToList();
            if (lines.Count == 0) throw new InvalidOperationException("Select at least one quantity to return.");
            foreach (ReturnLine l in lines) if (l.ReturnQuantity > l.SoldQuantity - l.AlreadyReturned) throw new InvalidOperationException("Return quantity exceeds available quantity for " + l.ProductName + ".");

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        int? customerId = null; string currency = "USD"; decimal? exchangeRate = null;
                        using (var cmd = new SqlCommand("SELECT customer_id,ISNULL(NULLIF(currency,N''),CASE WHEN balance_lb IS NOT NULL AND balance_usd IS NULL THEN N'LBP' ELSE N'USD' END),exchange_rate FROM dbo.Sales WITH (UPDLOCK,HOLDLOCK) WHERE sale_id=@id;", conn, tx))
                        { cmd.Parameters.AddWithValue("@id", saleId); using (var r = cmd.ExecuteReader()) { if (!r.Read()) throw new InvalidOperationException("Sale was not found."); customerId = r[0] == DBNull.Value ? (int?)null : Convert.ToInt32(r[0]); currency = Convert.ToString(r[1]); exchangeRate = r[2] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(r[2]); } }

                        decimal total = lines.Sum(x => x.UnitPrice * x.ReturnQuantity);
                        string number = "RET-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                        int returnId;
                        using (var cmd = new SqlCommand(@"INSERT dbo.SalesReturns(return_number,sale_id,customer_id,user_id,return_type,refund_method,currency,exchange_rate,total_amount,reason,status)
VALUES(@n,@sale,@customer,@user,@type,@method,@currency,@rate,@total,@reason,N'COMPLETED'); SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@n", number); cmd.Parameters.AddWithValue("@sale", saleId); cmd.Parameters.AddWithValue("@customer", customerId.HasValue ? (object)customerId.Value : DBNull.Value); cmd.Parameters.AddWithValue("@user", userId); cmd.Parameters.AddWithValue("@type", string.IsNullOrWhiteSpace(returnType) ? "RETURN" : returnType.ToUpperInvariant()); cmd.Parameters.AddWithValue("@method", string.IsNullOrWhiteSpace(refundMethod) ? (object)DBNull.Value : refundMethod.ToUpperInvariant()); cmd.Parameters.AddWithValue("@currency", currency); cmd.Parameters.AddWithValue("@rate", exchangeRate.HasValue ? (object)exchangeRate.Value : DBNull.Value); AddDecimal(cmd, "@total", total); cmd.Parameters.AddWithValue("@reason", string.IsNullOrWhiteSpace(reason) ? (object)DBNull.Value : reason.Trim());
                            returnId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (ReturnLine line in lines)
                        {
                            using (var cmd = new SqlCommand(@"INSERT dbo.SalesReturnItems(sales_return_id,sale_item_id,product_id,quantity,unit_price,line_total,restock)
VALUES(@r,@si,@p,@q,@price,@total,@restock);", conn, tx))
                            { cmd.Parameters.AddWithValue("@r", returnId); cmd.Parameters.AddWithValue("@si", line.SaleItemId); cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.Parameters.AddWithValue("@q", line.ReturnQuantity); AddDecimal(cmd, "@price", line.UnitPrice); AddDecimal(cmd, "@total", line.UnitPrice * line.ReturnQuantity); cmd.Parameters.AddWithValue("@restock", line.Restock); cmd.ExecuteNonQuery(); }
                            if (line.Restock)
                            {
                                int oldQty;
                                using (var cmd = new SqlCommand("SELECT stock_quantity FROM dbo.Products WITH (UPDLOCK,ROWLOCK) WHERE product_id=@p;", conn, tx)) { cmd.Parameters.AddWithValue("@p", line.ProductId); oldQty = Convert.ToInt32(cmd.ExecuteScalar()); }
                                using (var cmd = new SqlCommand("UPDATE dbo.Products SET stock_quantity=stock_quantity+@q,updated_at=SYSUTCDATETIME() WHERE product_id=@p;", conn, tx)) { cmd.Parameters.AddWithValue("@q", line.ReturnQuantity); cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.ExecuteNonQuery(); }
                                using (var cmd = new SqlCommand(@"INSERT dbo.InventoryTransactions(product_id,transaction_type,quantity_change,old_quantity,new_quantity,reference_type,reference_id,user_id,notes,created_at)
VALUES(@p,N'RETURN_IN',@q,@old,@new,N'SALES_RETURN',@rid,@u,N'Sales return restock',SYSUTCDATETIME());", conn, tx)) { cmd.Parameters.AddWithValue("@p", line.ProductId); cmd.Parameters.AddWithValue("@q", line.ReturnQuantity); cmd.Parameters.AddWithValue("@old", oldQty); cmd.Parameters.AddWithValue("@new", oldQty + line.ReturnQuantity); cmd.Parameters.AddWithValue("@rid", returnId); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
                            }
                        }

                        if (customerId.HasValue)
                        {
                            decimal before = GetCustomerBalance(conn, tx, customerId.Value, currency);
                            decimal after = before - total;
                            using (var cmd = new SqlCommand(@"INSERT dbo.CustomerTransactions(customer_id,transaction_type,reference_type,reference_id,debit,credit,balance_after,currency,description,user_id)
VALUES(@c,N'RETURN',N'SALES_RETURN',@r,0,@credit,@balance,@currency,N'Sales return credit',@u);", conn, tx)) { cmd.Parameters.AddWithValue("@c", customerId.Value); cmd.Parameters.AddWithValue("@r", returnId); AddDecimal(cmd, "@credit", total); AddDecimal(cmd, "@balance", after); cmd.Parameters.AddWithValue("@currency", currency); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
                            string col = currency == "LBP" ? "balance_lb" : "balance_usd";
                            using (var cmd = new SqlCommand("UPDATE dbo.Customers SET " + col + "=@b,balance_updated_at=SYSUTCDATETIME() WHERE customer_id=@c;", conn, tx)) { AddDecimal(cmd, "@b", after); cmd.Parameters.AddWithValue("@c", customerId.Value); cmd.ExecuteNonQuery(); }
                        }

                        if (string.Equals(refundMethod, "CASH", StringComparison.OrdinalIgnoreCase))
                        {
                            object session;
                            using (var cmd = new SqlCommand("SELECT TOP(1) cash_session_id FROM dbo.CashSessions WHERE user_id=@u AND status=N'OPEN' ORDER BY opened_at DESC;", conn, tx)) { cmd.Parameters.AddWithValue("@u", userId); session = cmd.ExecuteScalar(); }
                            if (session == null || session == DBNull.Value) throw new InvalidOperationException("Open a Cash Shift before issuing a CASH refund.");
                            using (var cmd = new SqlCommand(@"INSERT dbo.CashMovements(cash_session_id,movement_type,amount,currency,reference_type,reference_id,description,user_id,created_at)
VALUES(@s,N'REFUND',@amount,@currency,N'SALES_RETURN',@r,N'Sales return cash refund',@u,SYSUTCDATETIME());", conn, tx)) { cmd.Parameters.AddWithValue("@s", Convert.ToInt32(session)); AddDecimal(cmd, "@amount", -total); cmd.Parameters.AddWithValue("@currency", currency); cmd.Parameters.AddWithValue("@r", returnId); cmd.Parameters.AddWithValue("@u", userId); cmd.ExecuteNonQuery(); }
                        }

                        tx.Commit(); return returnId;
                    }
                    catch { tx.Rollback(); throw; }
                }
            }
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
