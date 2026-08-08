using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Pos_System.Services
{
    internal sealed class CheckoutService
    {
        internal sealed class Line
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal OriginalUnitPrice { get; set; }
            public int? ProductUnitId { get; set; }
            public int? WarehouseId { get; set; }
            public string BatchNumber { get; set; }
            public IList<string> SerialNumbers { get; set; } = new List<string>();
        }

        internal sealed class Payment
        {
            public string Method { get; set; }
            public decimal Amount { get; set; }
            public string Reference { get; set; }
            public string ProviderName { get; set; }
            public string PayerPhone { get; set; }
            public string ProviderStatus { get; set; }
            public string ProviderTransactionId { get; set; }
            public string ProviderMessage { get; set; }
        }

        internal sealed class Request
        {
            public Guid OperationKey { get; set; } = Guid.NewGuid();
            public int UserId { get; set; }
            public string Username { get; set; }
            public int? CustomerId { get; set; }
            public string CustomerName { get; set; }
            public string Currency { get; set; }
            public decimal ExchangeRate { get; set; }
            public string Notes { get; set; }
            public string SourceType { get; set; }
            public int? SourceId { get; set; }
            public decimal RedeemLoyaltyPoints { get; set; }
            public IList<Line> Lines { get; set; } = new List<Line>();
            public IList<Payment> Payments { get; set; } = new List<Payment>();
        }

        internal sealed class Result
        {
            public int SaleId { get; set; }
            public decimal Subtotal { get; set; }
            public decimal Discount { get; set; }
            public decimal Tax { get; set; }
            public decimal Total { get; set; }
            public decimal Paid { get; set; }
            public decimal Remaining { get; set; }
            public decimal LoyaltyEarned { get; set; }
            public decimal LoyaltyRedeemed { get; set; }
            public bool WasAlreadyCompleted { get; set; }
        }

        private sealed class PreparedLine
        {
            public Line Source;
            public int WarehouseId;
            public int? ProductUnitId;
            public decimal BaseQuantity;
            public decimal OriginalUnitPrice;
            public decimal UnitPrice;
            public decimal Gross;
            public decimal Discount;
            public decimal Tax;
            public decimal Total;
            public decimal UnitCost;
            public int? TaxRateId;
            public decimal TaxRatePercent;
            public bool TaxInclusive;
            public int? PromotionId;
            public string BatchAllocation;
        }

        private readonly string connectionString;

        public CheckoutService(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public Result Complete(Request request)
        {
            Validate(request);
            ActionPermissionService.Demand("SALE.CREATE");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    try
                    {
                        Result result = CompleteCore(connection, transaction, request);
                        transaction.Commit();
                        return result;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        internal Result CompleteWithinTransaction(SqlConnection connection, SqlTransaction transaction, Request request)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            Validate(request);
            ActionPermissionService.Demand("SALE.CREATE");
            return CompleteCore(connection, transaction, request);
        }

        private Result CompleteCore(SqlConnection connection, SqlTransaction transaction, Request request)
        {
            Result existing = FindExisting(connection, transaction, request.OperationKey);
            if (existing != null) { existing.WasAlreadyCompleted = true; return existing; }

            LockSource(connection, transaction, request);
            List<PreparedLine> lines = PrepareLines(connection, transaction, request);
            decimal subtotal = Round(lines.Sum(x => x.Gross));
            decimal discount = Round(lines.Sum(x => x.Discount));
            decimal tax = Round(lines.Sum(x => x.Tax));
            decimal total = Round(lines.Sum(x => x.Total));
            decimal redeemedValue = ApplyLoyaltyRedemption(connection, transaction, request, total);
            if (redeemedValue > 0m) { discount = Round(discount + redeemedValue); total = Round(total - redeemedValue); }
            decimal paid = Round(request.Payments.Sum(x => x.Amount));
            if (paid > total) throw new InvalidOperationException("Payments cannot exceed the sale total.");
            decimal remaining = Round(total - paid);
            int? cashSessionId = ResolveCashSession(connection, transaction, request);
            decimal loyaltyEarned = CalculateLoyaltyEarned(connection, transaction, request.Currency, total);

            int saleId = InsertSale(connection, transaction, request, subtotal, discount, tax, total, paid, remaining, loyaltyEarned);
            foreach (PreparedLine line in lines)
            {
                int saleItemId = InsertSaleItem(connection, transaction, saleId, request, line);
                DeductInventory(connection, transaction, saleId, saleItemId, request.UserId, line);
            }
            InsertPayments(connection, transaction, saleId, request, cashSessionId);
            UpdateCustomerBalance(connection, transaction, request, remaining);
            RecordLoyalty(connection, transaction, request, saleId, loyaltyEarned);
            CompleteSource(connection, transaction, request, saleId);
            return new Result { SaleId = saleId, Subtotal = subtotal, Discount = discount, Tax = tax, Total = total, Paid = paid, Remaining = remaining, LoyaltyEarned = loyaltyEarned, LoyaltyRedeemed = request.RedeemLoyaltyPoints };
        }

        public Result Preview(Request request)
        {
            Validate(request);
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        List<PreparedLine> lines = PrepareLines(connection, transaction, request);
                        decimal subtotal = Round(lines.Sum(x => x.Gross));
                        decimal discount = Round(lines.Sum(x => x.Discount));
                        decimal tax = Round(lines.Sum(x => x.Tax));
                        decimal total = Round(lines.Sum(x => x.Total));
                        decimal redeemed = ApplyLoyaltyRedemption(connection, transaction, request, total);
                        transaction.Rollback();
                        return new Result { Subtotal = subtotal, Discount = Round(discount + redeemed), Tax = tax, Total = Round(total - redeemed), LoyaltyRedeemed = request.RedeemLoyaltyPoints };
                    }
                    catch { transaction.Rollback(); throw; }
                }
            }
        }

        private static void Validate(Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.UserId <= 0) throw new InvalidOperationException("A logged-in user is required.");
            request.Currency = NormalizeCurrency(request.Currency);
            if (request.Currency == "LBP" && request.ExchangeRate <= 0m) throw new InvalidOperationException("A positive exchange-rate snapshot is required for LBP sales.");
            if (request.Lines == null || request.Lines.Count == 0) throw new InvalidOperationException("The cart is empty.");
            if (request.Payments == null) request.Payments = new List<Payment>();
            foreach (Line line in request.Lines)
            {
                if (line == null || line.ProductId <= 0 || line.Quantity <= 0) throw new InvalidOperationException("Every sale line needs a valid product and quantity.");
                if (line.UnitPrice < 0m || line.OriginalUnitPrice < 0m) throw new InvalidOperationException("Prices cannot be negative.");
            }
            foreach (Payment payment in request.Payments)
            {
                if (payment == null || payment.Amount <= 0m || string.IsNullOrWhiteSpace(payment.Method)) throw new InvalidOperationException("Every payment needs a method and positive amount.");
                payment.Method = payment.Method.Trim().ToUpperInvariant();
                if (payment.Method == "WHISH" && payment.ProviderStatus != "AUTHORIZED" && payment.ProviderStatus != "VERIFIED")
                    throw new InvalidOperationException("WHISH payment is not provider-authorized; checkout remains fail-closed.");
            }
            if (request.Lines.Any(x => x.OriginalUnitPrice > 0m && x.UnitPrice < x.OriginalUnitPrice)) ActionPermissionService.Demand("SALE.DISCOUNT");
        }

        private Result FindExisting(SqlConnection connection, SqlTransaction transaction, Guid key)
        {
            using (var command = new SqlCommand(@"SELECT TOP(1) sale_id,ISNULL(subtotal,total_amount),ISNULL(discount_total,0),ISNULL(tax_total,0),total_amount,ISNULL(paid_amount,0),ISNULL(remaining_amount,0),ISNULL(loyalty_points_earned,0),ISNULL(loyalty_points_redeemed,0)
FROM dbo.Sales WITH(UPDLOCK,HOLDLOCK) WHERE operation_key=@key;", connection, transaction))
            {
                command.Parameters.Add("@key", SqlDbType.UniqueIdentifier).Value = key;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new Result { SaleId = reader.GetInt32(0), Subtotal = reader.GetDecimal(1), Discount = reader.GetDecimal(2), Tax = reader.GetDecimal(3), Total = reader.GetDecimal(4), Paid = reader.GetDecimal(5), Remaining = reader.GetDecimal(6), LoyaltyEarned = reader.GetDecimal(7), LoyaltyRedeemed = reader.GetDecimal(8) };
                }
            }
        }

        private static void LockSource(SqlConnection connection, SqlTransaction transaction, Request request)
        {
            if (!request.SourceId.HasValue || string.IsNullOrWhiteSpace(request.SourceType)) return;
            string source = request.SourceType.Trim().ToUpperInvariant();
            string sql;
            if (source == "HELD") sql = "SELECT status FROM dbo.HeldSales WITH(UPDLOCK,HOLDLOCK) WHERE held_sale_id=@id";
            else if (source == "QUOTATION") sql = "SELECT status FROM dbo.Quotations WITH(UPDLOCK,HOLDLOCK) WHERE quotation_id=@id";
            else throw new InvalidOperationException("Unsupported checkout source.");
            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = request.SourceId.Value;
                string status = Convert.ToString(command.ExecuteScalar());
                if (source == "HELD" && status != "HELD") throw new InvalidOperationException("The held sale was already resumed or completed.");
                if (source == "QUOTATION" && status != "OPEN") throw new InvalidOperationException("The quotation was already converted, cancelled, or expired.");
            }
        }

        private List<PreparedLine> PrepareLines(SqlConnection connection, SqlTransaction transaction, Request request)
        {
            var result = new List<PreparedLine>();
            foreach (Line source in request.Lines)
            {
                int warehouseId = source.WarehouseId ?? DefaultWarehouse(connection, transaction);
                decimal factor = 1m;
                decimal? unitPrice = null;
                if (source.ProductUnitId.HasValue)
                {
                    using (var command = new SqlCommand("SELECT conversion_factor,selling_price FROM dbo.ProductUnits WHERE product_unit_id=@id AND product_id=@p;", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@id", source.ProductUnitId.Value); command.Parameters.AddWithValue("@p", source.ProductId);
                        using (var reader = command.ExecuteReader()) { if (!reader.Read()) throw new InvalidOperationException("The selected product unit is invalid."); factor = reader.GetDecimal(0); unitPrice = reader.IsDBNull(1) ? (decimal?)null : reader.GetDecimal(1); }
                    }
                }

                int categoryId; decimal cost; decimal productPrice;
                using (var command = new SqlCommand("SELECT ISNULL(category_id,0),ISNULL(purchase_price,0),CASE WHEN @cur=N'LBP' THEN ISNULL(NULLIF(sale_price_lb,0),price_lb) ELSE ISNULL(NULLIF(sale_price_usd,0),COALESCE(NULLIF(selling_price,0),price_usd,price)) END FROM dbo.Products WITH(UPDLOCK,HOLDLOCK) WHERE product_id=@p;", connection, transaction))
                {
                    command.Parameters.AddWithValue("@p", source.ProductId); command.Parameters.AddWithValue("@cur", request.Currency);
                    using (var reader = command.ExecuteReader()) { if (!reader.Read()) throw new InvalidOperationException("Product not found: " + source.ProductName); categoryId = reader.GetInt32(0); cost = reader.GetDecimal(1); productPrice = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2); }
                }

                decimal resolvedPrice = ResolveCustomerPrice(connection, transaction, request.CustomerId, source.ProductId, source.ProductUnitId, request.Currency) ?? unitPrice ?? (source.UnitPrice > 0m ? source.UnitPrice : productPrice);
                decimal original = source.OriginalUnitPrice > 0m ? source.OriginalUnitPrice : resolvedPrice;
                decimal manualDiscount = Math.Max(0m, (original - resolvedPrice) * source.Quantity);
                int? promotionId; decimal promotionDiscount;
                ResolvePromotion(connection, transaction, request.CustomerId, source.ProductId, categoryId, source.Quantity, resolvedPrice * source.Quantity, out promotionId, out promotionDiscount);
                decimal discount = Math.Max(manualDiscount, promotionDiscount);
                if (manualDiscount > 0m) promotionId = null;

                int? taxId; decimal taxPercent; bool inclusive;
                ResolveTax(connection, transaction, source.ProductId, categoryId, out taxId, out taxPercent, out inclusive);
                decimal gross = Round(original * source.Quantity);
                decimal taxable = Math.Max(0m, gross - discount);
                decimal tax = inclusive ? Round(taxable * taxPercent / (100m + taxPercent)) : Round(taxable * taxPercent / 100m);
                decimal total = inclusive ? taxable : Round(taxable + tax);

                result.Add(new PreparedLine { Source = source, WarehouseId = warehouseId, ProductUnitId = source.ProductUnitId, BaseQuantity = factor * source.Quantity, OriginalUnitPrice = original, UnitPrice = resolvedPrice, Gross = gross, Discount = discount, Tax = tax, Total = total, UnitCost = cost, TaxRateId = taxId, TaxRatePercent = taxPercent, TaxInclusive = inclusive, PromotionId = promotionId });
            }
            return result;
        }

        private static int DefaultWarehouse(SqlConnection connection, SqlTransaction transaction)
        {
            using (var command = new SqlCommand("SELECT TOP(1) warehouse_id FROM dbo.Warehouses WHERE is_active=1 ORDER BY is_default DESC,warehouse_id;", connection, transaction))
            { object value = command.ExecuteScalar(); if (value == null || value == DBNull.Value) throw new InvalidOperationException("No active warehouse is configured."); return Convert.ToInt32(value); }
        }

        private static decimal? ResolveCustomerPrice(SqlConnection connection, SqlTransaction transaction, int? customerId, int productId, int? unitId, string currency)
        {
            if (!customerId.HasValue) return null;
            using (var command = new SqlCommand(@"SELECT TOP(1) pp.price FROM dbo.Customers c JOIN dbo.ProductPrices pp ON pp.price_level_id=c.price_level_id
WHERE c.customer_id=@c AND pp.product_id=@p AND pp.currency=@cur AND (pp.unit_id=@u OR (pp.unit_id IS NULL AND @u IS NULL))
AND (pp.effective_from IS NULL OR pp.effective_from<=SYSUTCDATETIME()) AND (pp.effective_to IS NULL OR pp.effective_to>SYSUTCDATETIME()) ORDER BY pp.effective_from DESC,pp.product_price_id DESC;", connection, transaction))
            { command.Parameters.AddWithValue("@c", customerId.Value); command.Parameters.AddWithValue("@p", productId); command.Parameters.AddWithValue("@cur", currency); command.Parameters.AddWithValue("@u", unitId.HasValue ? (object)unitId.Value : DBNull.Value); object value = command.ExecuteScalar(); return value == null || value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value); }
        }

        private static void ResolvePromotion(SqlConnection connection, SqlTransaction transaction, int? customerId, int productId, int categoryId, int quantity, decimal gross, out int? promotionId, out decimal discount)
        {
            using (var command = new SqlCommand(@"SELECT TOP(1) p.promotion_id,ISNULL(r.discount_percent,0),ISNULL(r.discount_amount,0)
FROM dbo.Promotions p JOIN dbo.PromotionRules r ON r.promotion_id=p.promotion_id
WHERE p.is_active=1 AND (p.start_at IS NULL OR p.start_at<=SYSUTCDATETIME()) AND (p.end_at IS NULL OR p.end_at>SYSUTCDATETIME())
AND (r.product_id IS NULL OR r.product_id=@product) AND (r.category_id IS NULL OR r.category_id=@category) AND (r.customer_id IS NULL OR r.customer_id=@customer)
AND (r.min_quantity IS NULL OR r.min_quantity<=@qty) AND (r.min_invoice_amount IS NULL OR r.min_invoice_amount<=@gross)
ORDER BY p.priority,p.promotion_id;", connection, transaction))
            {
                command.Parameters.AddWithValue("@product", productId); command.Parameters.AddWithValue("@category", categoryId); command.Parameters.AddWithValue("@customer", customerId.HasValue ? (object)customerId.Value : DBNull.Value); AddDecimal(command, "@qty", quantity); AddDecimal(command, "@gross", gross);
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) { promotionId = null; discount = 0m; return; }
                    promotionId = reader.GetInt32(0); decimal percent = reader.GetDecimal(1); decimal amount = reader.GetDecimal(2); discount = Math.Min(gross, Math.Max(amount, gross * percent / 100m));
                }
            }
        }

        private static void ResolveTax(SqlConnection connection, SqlTransaction transaction, int productId, int categoryId, out int? taxId, out decimal percent, out bool inclusive)
        {
            using (var command = new SqlCommand(@"SELECT TOP(1) tr.tax_rate_id,tr.rate_percent,tr.is_inclusive FROM dbo.TaxRates tr
WHERE tr.tax_rate_id=COALESCE((SELECT tax_rate_id FROM dbo.ProductTaxRates WHERE product_id=@p),(SELECT tax_rate_id FROM dbo.Categories WHERE category_id=@c),(SELECT TOP(1) tax_rate_id FROM dbo.TaxRates WHERE code=N'STD'))
AND tr.is_active=1 AND (tr.effective_from IS NULL OR tr.effective_from<=SYSUTCDATETIME()) AND (tr.effective_to IS NULL OR tr.effective_to>SYSUTCDATETIME());", connection, transaction))
            {
                command.Parameters.AddWithValue("@p", productId); command.Parameters.AddWithValue("@c", categoryId);
                using (var reader = command.ExecuteReader()) { if (!reader.Read()) { taxId = null; percent = 0m; inclusive = false; } else { taxId = reader.GetInt32(0); percent = reader.GetDecimal(1); inclusive = reader.GetBoolean(2); } }
            }
        }

        private static int InsertSale(SqlConnection connection, SqlTransaction transaction, Request request, decimal subtotal, decimal discount, decimal tax, decimal total, decimal paid, decimal remaining, decimal loyaltyEarned)
        {
            using (var command = new SqlCommand(@"INSERT dbo.Sales(user_id,customer_id,total_amount,payment_method,created_by,customer_name,balance_usd,balance_lb,quantity,currency,exchange_rate,subtotal,discount_total,tax_total,paid_amount,remaining_amount,payment_status,sale_status,notes,operation_key,loyalty_points_earned,loyalty_points_redeemed)
VALUES(@u,@c,@total,N'SPLIT',@by,@customer,@usd,@lb,@qty,@currency,@rate,@subtotal,@discount,@tax,@paid,@remaining,@paymentStatus,N'COMPLETED',@notes,@key,@earned,@redeemed);SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction))
            {
                command.Parameters.AddWithValue("@u", request.UserId); command.Parameters.AddWithValue("@c", request.CustomerId.HasValue ? (object)request.CustomerId.Value : DBNull.Value); AddDecimal(command, "@total", total); command.Parameters.AddWithValue("@by", request.Username ?? string.Empty); command.Parameters.AddWithValue("@customer", request.CustomerName ?? string.Empty);
                AddNullableDecimal(command, "@usd", request.Currency == "USD" ? (decimal?)remaining : null); AddNullableDecimal(command, "@lb", request.Currency == "LBP" ? (decimal?)remaining : null); command.Parameters.AddWithValue("@qty", request.Lines.Sum(x => x.Quantity)); command.Parameters.AddWithValue("@currency", request.Currency); AddNullableDecimal(command, "@rate", request.ExchangeRate > 0m ? (decimal?)request.ExchangeRate : null);
                AddDecimal(command, "@subtotal", subtotal); AddDecimal(command, "@discount", discount); AddDecimal(command, "@tax", tax); AddDecimal(command, "@paid", paid); AddDecimal(command, "@remaining", remaining); command.Parameters.AddWithValue("@paymentStatus", remaining == 0m ? "PAID" : paid > 0m ? "PARTIAL" : "UNPAID"); command.Parameters.AddWithValue("@notes", string.IsNullOrWhiteSpace(request.Notes) ? (object)DBNull.Value : request.Notes.Trim()); command.Parameters.Add("@key", SqlDbType.UniqueIdentifier).Value = request.OperationKey; AddDecimal(command, "@earned", loyaltyEarned); AddDecimal(command, "@redeemed", request.RedeemLoyaltyPoints);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static int InsertSaleItem(SqlConnection connection, SqlTransaction transaction, int saleId, Request request, PreparedLine line)
        {
            using (var command = new SqlCommand(@"INSERT dbo.Sale_Items(sale_id,product_id,quantity,quantity_decimal,product_unit_id,base_quantity,warehouse_id,batch_number,serial_number,unit_price,original_unit_price,discount_amount,discount_type,discount_value,discount_by,name_product,customer_name,created_by,unit_cost_snapshot,tax_rate_id,tax_rate_percent,tax_inclusive,tax_amount,promotion_id)
VALUES(@sale,@product,@qty,@qtyd,@unit,@base,@warehouse,@batch,@serial,@price,@original,@discount,@discountType,@discountValue,@discountBy,@productName,@customer,@createdBy,@cost,@taxId,@taxPercent,@inclusive,@tax,@promotion);SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction))
            {
                command.Parameters.AddWithValue("@sale", saleId); command.Parameters.AddWithValue("@product", line.Source.ProductId); command.Parameters.AddWithValue("@qty", line.Source.Quantity); AddDecimal(command, "@qtyd", line.Source.Quantity); command.Parameters.AddWithValue("@unit", line.ProductUnitId.HasValue ? (object)line.ProductUnitId.Value : DBNull.Value); AddDecimal(command, "@base", line.BaseQuantity); command.Parameters.AddWithValue("@warehouse", line.WarehouseId); command.Parameters.AddWithValue("@batch", string.IsNullOrWhiteSpace(line.BatchAllocation) ? (object)DBNull.Value : line.BatchAllocation); command.Parameters.AddWithValue("@serial", line.Source.SerialNumbers != null && line.Source.SerialNumbers.Count == 1 ? (object)line.Source.SerialNumbers[0] : DBNull.Value);
                AddDecimal(command, "@price", line.UnitPrice); AddDecimal(command, "@original", line.OriginalUnitPrice); AddDecimal(command, "@discount", line.Discount); command.Parameters.AddWithValue("@discountType", line.Discount > 0m ? (line.PromotionId.HasValue ? "PROMOTION" : "MANUAL") : "NONE"); AddDecimal(command, "@discountValue", line.Discount); command.Parameters.AddWithValue("@discountBy", line.Discount > 0m ? (object)(request.Username ?? string.Empty) : DBNull.Value); command.Parameters.AddWithValue("@productName", line.Source.ProductName ?? string.Empty); command.Parameters.AddWithValue("@customer", request.CustomerName ?? string.Empty); command.Parameters.AddWithValue("@createdBy", request.Username ?? string.Empty); AddDecimal(command, "@cost", line.UnitCost); command.Parameters.AddWithValue("@taxId", line.TaxRateId.HasValue ? (object)line.TaxRateId.Value : DBNull.Value); AddDecimal(command, "@taxPercent", line.TaxRatePercent); command.Parameters.AddWithValue("@inclusive", line.TaxInclusive); AddDecimal(command, "@tax", line.Tax); command.Parameters.AddWithValue("@promotion", line.PromotionId.HasValue ? (object)line.PromotionId.Value : DBNull.Value);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static void DeductInventory(SqlConnection connection, SqlTransaction transaction, int saleId, int saleItemId, int userId, PreparedLine line)
        {
            decimal oldQuantity;
            using (var command = new SqlCommand("SELECT quantity FROM dbo.ProductStock WITH(UPDLOCK,HOLDLOCK) WHERE warehouse_id=@w AND product_id=@p;", connection, transaction))
            { command.Parameters.AddWithValue("@w", line.WarehouseId); command.Parameters.AddWithValue("@p", line.Source.ProductId); object value = command.ExecuteScalar(); oldQuantity = value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value); }
            if (oldQuantity < line.BaseQuantity) throw new InvalidOperationException("Insufficient stock for " + line.Source.ProductName + ".");
            decimal newQuantity = oldQuantity - line.BaseQuantity;

            using (var command = new SqlCommand("UPDATE dbo.ProductStock SET quantity=@q,updated_at=SYSUTCDATETIME() WHERE warehouse_id=@w AND product_id=@p;", connection, transaction))
            { AddDecimal(command, "@q", newQuantity); command.Parameters.AddWithValue("@w", line.WarehouseId); command.Parameters.AddWithValue("@p", line.Source.ProductId); if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Warehouse stock row is missing."); }

            line.BatchAllocation = DeductBatches(connection, transaction, line);
            DeductSerials(connection, transaction, saleId, saleItemId, line);

            using (var command = new SqlCommand(@"UPDATE p SET stock_quantity_decimal=x.total_quantity,stock_quantity=CONVERT(INT,FLOOR(CASE WHEN x.total_quantity<0 THEN 0 ELSE x.total_quantity END)),updated_at=SYSUTCDATETIME()
FROM dbo.Products p CROSS APPLY(SELECT ISNULL(SUM(quantity),0) total_quantity FROM dbo.ProductStock WHERE product_id=p.product_id)x WHERE p.product_id=@p;", connection, transaction))
            { command.Parameters.AddWithValue("@p", line.Source.ProductId); command.ExecuteNonQuery(); }

            using (var command = new SqlCommand(@"INSERT dbo.InventoryTransactions(product_id,transaction_type,quantity_change,old_quantity,new_quantity,quantity_change_decimal,old_quantity_decimal,new_quantity_decimal,reference_type,reference_id,user_id,notes,created_at,warehouse_id,unit_id,batch_number)
VALUES(@p,N'SALE',@legacyChange,@legacyOld,@legacyNew,@change,@old,@new,N'SALE',@sale,@u,N'Checkout stock deduction',SYSUTCDATETIME(),@w,(SELECT unit_id FROM dbo.ProductUnits WHERE product_unit_id=@productUnit),@batch);", connection, transaction))
            { command.Parameters.AddWithValue("@p", line.Source.ProductId); command.Parameters.AddWithValue("@legacyChange", -Convert.ToInt32(Math.Round(line.BaseQuantity, 0))); command.Parameters.AddWithValue("@legacyOld", Convert.ToInt32(Math.Floor(oldQuantity))); command.Parameters.AddWithValue("@legacyNew", Convert.ToInt32(Math.Floor(newQuantity))); AddDecimal(command, "@change", -line.BaseQuantity); AddDecimal(command, "@old", oldQuantity); AddDecimal(command, "@new", newQuantity); command.Parameters.AddWithValue("@sale", saleId); command.Parameters.AddWithValue("@u", userId); command.Parameters.AddWithValue("@w", line.WarehouseId); command.Parameters.AddWithValue("@productUnit", line.ProductUnitId.HasValue ? (object)line.ProductUnitId.Value : DBNull.Value); command.Parameters.AddWithValue("@batch", string.IsNullOrWhiteSpace(line.BatchAllocation) ? (object)DBNull.Value : line.BatchAllocation); command.ExecuteNonQuery(); }
        }

        private static string DeductBatches(SqlConnection connection, SqlTransaction transaction, PreparedLine line)
        {
            var allocations = new List<string>(); decimal remaining = line.BaseQuantity;
            string filter = string.IsNullOrWhiteSpace(line.Source.BatchNumber) ? string.Empty : " AND batch_number=@batch";
            using (var command = new SqlCommand("SELECT product_batch_id,batch_number,quantity FROM dbo.ProductBatches WITH(UPDLOCK,HOLDLOCK) WHERE product_id=@p AND warehouse_id=@w AND quantity>0" + filter + " ORDER BY CASE WHEN expiry_date IS NULL THEN 1 ELSE 0 END,expiry_date,product_batch_id;", connection, transaction))
            {
                command.Parameters.AddWithValue("@p", line.Source.ProductId); command.Parameters.AddWithValue("@w", line.WarehouseId); if (filter.Length > 0) command.Parameters.AddWithValue("@batch", line.Source.BatchNumber.Trim());
                var rows = new List<Tuple<int, string, decimal>>(); using (var reader = command.ExecuteReader()) while (reader.Read()) rows.Add(Tuple.Create(reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2)));
                if (rows.Count == 0) return null;
                foreach (Tuple<int, string, decimal> row in rows)
                {
                    decimal take = Math.Min(remaining, row.Item3); if (take <= 0m) continue;
                    using (var update = new SqlCommand("UPDATE dbo.ProductBatches SET quantity=quantity-@q WHERE product_batch_id=@id;", connection, transaction)) { AddDecimal(update, "@q", take); update.Parameters.AddWithValue("@id", row.Item1); update.ExecuteNonQuery(); }
                    allocations.Add(row.Item2 + ":" + take.ToString("0.########")); remaining -= take; if (remaining == 0m) break;
                }
            }
            if (remaining > 0m) throw new InvalidOperationException("Batch stock is insufficient for " + line.Source.ProductName + ".");
            return string.Join(",", allocations);
        }

        private static void DeductSerials(SqlConnection connection, SqlTransaction transaction, int saleId, int saleItemId, PreparedLine line)
        {
            int available;
            using (var command = new SqlCommand("SELECT COUNT(*) FROM dbo.ProductSerials WITH(UPDLOCK,HOLDLOCK) WHERE product_id=@p AND warehouse_id=@w AND status=N'IN_STOCK';", connection, transaction)) { command.Parameters.AddWithValue("@p", line.Source.ProductId); command.Parameters.AddWithValue("@w", line.WarehouseId); available = Convert.ToInt32(command.ExecuteScalar()); }
            if (available == 0 && (line.Source.SerialNumbers == null || line.Source.SerialNumbers.Count == 0)) return;
            if (line.BaseQuantity != Math.Truncate(line.BaseQuantity)) throw new InvalidOperationException("Serialized products require a whole-number base quantity.");
            int required = Convert.ToInt32(line.BaseQuantity);
            if (line.Source.SerialNumbers == null || line.Source.SerialNumbers.Count != required) throw new InvalidOperationException("Select one serial/IMEI for each sold unit of " + line.Source.ProductName + ".");
            if (line.Source.SerialNumbers.Distinct(StringComparer.OrdinalIgnoreCase).Count() != required) throw new InvalidOperationException("The same serial/IMEI cannot be selected twice.");
            foreach (string serial in line.Source.SerialNumbers)
            {
                using (var command = new SqlCommand("UPDATE dbo.ProductSerials SET status=N'SOLD',sale_id=@sale,sale_item_id=@item WHERE product_id=@p AND warehouse_id=@w AND serial_number=@serial AND status=N'IN_STOCK';", connection, transaction))
                { command.Parameters.AddWithValue("@sale", saleId); command.Parameters.AddWithValue("@item", saleItemId); command.Parameters.AddWithValue("@p", line.Source.ProductId); command.Parameters.AddWithValue("@w", line.WarehouseId); command.Parameters.AddWithValue("@serial", serial.Trim()); if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("Serial/IMEI is unavailable or already sold: " + serial); }
            }
        }

        private static int? ResolveCashSession(SqlConnection connection, SqlTransaction transaction, Request request)
        {
            if (!request.Payments.Any(x => x.Method == "CASH")) return null;
            using (var command = new SqlCommand("SELECT TOP(1) cash_session_id FROM dbo.CashSessions WITH(UPDLOCK,HOLDLOCK) WHERE user_id=@u AND status=N'OPEN' ORDER BY opened_at DESC;", connection, transaction))
            { command.Parameters.AddWithValue("@u", request.UserId); object value = command.ExecuteScalar(); if (value == null || value == DBNull.Value) throw new InvalidOperationException("Open a Cash Shift before accepting or refunding CASH."); return Convert.ToInt32(value); }
        }

        private static void InsertPayments(SqlConnection connection, SqlTransaction transaction, int saleId, Request request, int? cashSessionId)
        {
            foreach (Payment payment in request.Payments)
            {
                using (var command = new SqlCommand(@"INSERT dbo.SalePayments(sale_id,payment_method,amount,currency,exchange_rate,reference_number,cash_session_id,is_legacy_auto,provider_name,payer_phone,provider_status,provider_transaction_id,provider_message,provider_verified_at,created_at)
VALUES(@sale,@method,@amount,@currency,@rate,@reference,@session,0,@provider,@phone,@status,@providerId,@message,@verified,SYSUTCDATETIME());", connection, transaction))
                {
                    command.Parameters.AddWithValue("@sale", saleId); command.Parameters.AddWithValue("@method", payment.Method); AddDecimal(command, "@amount", payment.Amount); command.Parameters.AddWithValue("@currency", request.Currency); AddNullableDecimal(command, "@rate", request.ExchangeRate > 0m ? (decimal?)request.ExchangeRate : null); command.Parameters.AddWithValue("@reference", Db(payment.Reference)); command.Parameters.AddWithValue("@session", payment.Method == "CASH" && cashSessionId.HasValue ? (object)cashSessionId.Value : DBNull.Value); command.Parameters.AddWithValue("@provider", Db(payment.ProviderName)); command.Parameters.AddWithValue("@phone", Db(payment.PayerPhone)); command.Parameters.AddWithValue("@status", Db(payment.ProviderStatus)); command.Parameters.AddWithValue("@providerId", Db(payment.ProviderTransactionId)); command.Parameters.AddWithValue("@message", Db(payment.ProviderMessage)); command.Parameters.AddWithValue("@verified", payment.Method == "WHISH" ? (object)DateTime.UtcNow : DBNull.Value); command.ExecuteNonQuery();
                }
            }
        }

        private static void UpdateCustomerBalance(SqlConnection connection, SqlTransaction transaction, Request request, decimal remaining)
        {
            if (!request.CustomerId.HasValue || remaining == 0m) return;
            string column = request.Currency == "LBP" ? "balance_lb" : "balance_usd";
            using (var command = new SqlCommand("UPDATE dbo.Customers SET " + column + "=ISNULL(" + column + ",0)+@amount,balance_updated_at=SYSUTCDATETIME() WHERE customer_id=@c;", connection, transaction))
            { AddDecimal(command, "@amount", remaining); command.Parameters.AddWithValue("@c", request.CustomerId.Value); command.ExecuteNonQuery(); }
        }

        private static decimal ApplyLoyaltyRedemption(SqlConnection connection, SqlTransaction transaction, Request request, decimal total)
        {
            if (request.RedeemLoyaltyPoints <= 0m) return 0m;
            if (!request.CustomerId.HasValue) throw new InvalidOperationException("A customer is required to redeem loyalty points.");
            decimal balance;
            using (var command = new SqlCommand("SELECT points_balance FROM dbo.LoyaltyAccounts WITH(UPDLOCK,HOLDLOCK) WHERE customer_id=@c;", connection, transaction)) { command.Parameters.AddWithValue("@c", request.CustomerId.Value); object value = command.ExecuteScalar(); balance = value == null || value == DBNull.Value ? 0m : Convert.ToDecimal(value); }
            if (balance < request.RedeemLoyaltyPoints) throw new InvalidOperationException("The customer does not have enough loyalty points.");
            decimal valuePerPoint = SettingDecimal(connection, transaction, "loyalty_point_value_" + request.Currency.ToLowerInvariant(), request.Currency == "LBP" ? 1000m : 0.01m);
            return Math.Min(total, Round(request.RedeemLoyaltyPoints * valuePerPoint));
        }

        private static decimal CalculateLoyaltyEarned(SqlConnection connection, SqlTransaction transaction, string currency, decimal total)
        {
            decimal rate = SettingDecimal(connection, transaction, "loyalty_earn_rate_" + currency.ToLowerInvariant(), currency == "LBP" ? 0.00001m : 1m);
            return Round(total * rate);
        }

        private static void RecordLoyalty(SqlConnection connection, SqlTransaction transaction, Request request, int saleId, decimal earned)
        {
            if (!request.CustomerId.HasValue || (earned == 0m && request.RedeemLoyaltyPoints == 0m)) return;
            int accountId; decimal balance;
            using (var command = new SqlCommand("SELECT loyalty_account_id,points_balance FROM dbo.LoyaltyAccounts WITH(UPDLOCK,HOLDLOCK) WHERE customer_id=@c;", connection, transaction))
            {
                command.Parameters.AddWithValue("@c", request.CustomerId.Value);
                using (var reader = command.ExecuteReader()) { if (reader.Read()) { accountId = reader.GetInt32(0); balance = reader.GetDecimal(1); } else { reader.Close(); using (var create = new SqlCommand("INSERT dbo.LoyaltyAccounts(customer_id,points_balance,lifetime_points) VALUES(@c,0,0);SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction)) { create.Parameters.AddWithValue("@c", request.CustomerId.Value); accountId = Convert.ToInt32(create.ExecuteScalar()); balance = 0m; } } }
            }
            if (request.RedeemLoyaltyPoints > 0m)
            {
                balance -= request.RedeemLoyaltyPoints;
                InsertLoyaltyTransaction(connection, transaction, accountId, "REDEEM", -request.RedeemLoyaltyPoints, balance, saleId, request.UserId);
            }
            if (earned > 0m)
            {
                balance += earned;
                InsertLoyaltyTransaction(connection, transaction, accountId, "EARN", earned, balance, saleId, request.UserId);
            }
            using (var command = new SqlCommand("UPDATE dbo.LoyaltyAccounts SET points_balance=@balance,lifetime_points=lifetime_points+@earned,updated_at=SYSUTCDATETIME() WHERE loyalty_account_id=@id;", connection, transaction)) { AddDecimal(command, "@balance", balance); AddDecimal(command, "@earned", earned); command.Parameters.AddWithValue("@id", accountId); command.ExecuteNonQuery(); }
        }

        private static void InsertLoyaltyTransaction(SqlConnection connection, SqlTransaction transaction, int accountId, string type, decimal points, decimal balance, int saleId, int userId)
        {
            using (var command = new SqlCommand("INSERT dbo.LoyaltyTransactions(loyalty_account_id,transaction_type,points,balance_after,reference_type,reference_id,description,user_id) VALUES(@a,@type,@points,@balance,N'SALE',@sale,N'Checkout loyalty transaction',@u);", connection, transaction)) { command.Parameters.AddWithValue("@a", accountId); command.Parameters.AddWithValue("@type", type); AddDecimal(command, "@points", points); AddDecimal(command, "@balance", balance); command.Parameters.AddWithValue("@sale", saleId); command.Parameters.AddWithValue("@u", userId); command.ExecuteNonQuery(); }
        }

        private static void CompleteSource(SqlConnection connection, SqlTransaction transaction, Request request, int saleId)
        {
            if (!request.SourceId.HasValue || string.IsNullOrWhiteSpace(request.SourceType)) return;
            string source = request.SourceType.Trim().ToUpperInvariant();
            string sql = source == "HELD"
                ? "UPDATE dbo.HeldSales SET status=N'COMPLETED',completed_sale_id=@sale,completed_at=SYSUTCDATETIME(),resumed_at=COALESCE(resumed_at,SYSUTCDATETIME()) WHERE held_sale_id=@id AND status=N'HELD';"
                : "UPDATE dbo.Quotations SET status=N'CONVERTED',converted_sale_id=@sale,updated_at=SYSUTCDATETIME() WHERE quotation_id=@id AND status=N'OPEN';";
            using (var command = new SqlCommand(sql, connection, transaction)) { command.Parameters.AddWithValue("@sale", saleId); command.Parameters.AddWithValue("@id", request.SourceId.Value); if (command.ExecuteNonQuery() != 1) throw new InvalidOperationException("The source document was changed by another user."); }
        }

        private static decimal SettingDecimal(SqlConnection connection, SqlTransaction transaction, string key, decimal fallback)
        {
            using (var command = new SqlCommand("SELECT TOP(1) value FROM dbo.Settings WHERE key_name=@key;", connection, transaction)) { command.Parameters.AddWithValue("@key", key); object value = command.ExecuteScalar(); decimal parsed; return value != null && value != DBNull.Value && decimal.TryParse(Convert.ToString(value), out parsed) ? parsed : fallback; }
        }

        private static object Db(string value) { return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim(); }
        private static decimal Round(decimal value) { return Math.Round(value, 8, MidpointRounding.AwayFromZero); }
        private static string NormalizeCurrency(string currency) { string value = (currency ?? "USD").Trim().ToUpperInvariant(); if (value != "USD" && value != "LBP") throw new InvalidOperationException("Currency must be USD or LBP."); return value; }
        private static void AddDecimal(SqlCommand command, string name, decimal value) { SqlParameter p = command.Parameters.Add(name, SqlDbType.Decimal); p.Precision = 24; p.Scale = 8; p.Value = Round(value); }
        private static void AddNullableDecimal(SqlCommand command, string name, decimal? value) { SqlParameter p = command.Parameters.Add(name, SqlDbType.Decimal); p.Precision = 24; p.Scale = 8; p.Value = value.HasValue ? (object)Round(value.Value) : DBNull.Value; }
    }
}
