using Microsoft.EntityFrameworkCore;
using Pos_System.Data;
using Pos_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace Pos_System.Services
{
    public class PurchaseService
    {
        private readonly POSDbContext _context;

        public PurchaseService(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Purchase CreatePurchase(Purchase purchase)
        {
            ActionPermissionService.Demand("PURCHASE.CREATE");
            if (purchase == null) throw new ArgumentNullException(nameof(purchase));
            if (purchase.SupplierId <= 0) throw new InvalidOperationException("A supplier is required for a purchase.");
            if (purchase.UserId <= 0) throw new InvalidOperationException("A user is required for a purchase.");
            if (purchase.PurchaseItems == null || !purchase.PurchaseItems.Any()) throw new InvalidOperationException("A purchase must contain at least one item.");
            if (!_context.Suppliers.Any(s => s.SupplierId == purchase.SupplierId)) throw new InvalidOperationException("The selected supplier does not exist.");
            if (!_context.Users.Any(u => u.User_Id == purchase.UserId)) throw new InvalidOperationException("The selected user does not exist.");

            if (string.IsNullOrWhiteSpace(purchase.InvoiceNumber)) purchase.InvoiceNumber = GeneratePurchaseNumber();
            if(purchase.OperationKey==Guid.Empty)purchase.OperationKey=Guid.NewGuid();
            purchase.Currency=string.IsNullOrWhiteSpace(purchase.Currency)?"USD":purchase.Currency.Trim().ToUpperInvariant();
            if(purchase.Currency!="USD"&&purchase.Currency!="LBP")throw new InvalidOperationException("Purchase currency must be USD or LBP.");
            if(!purchase.ExchangeRate.HasValue||purchase.ExchangeRate<=0m)purchase.ExchangeRate=POS_System.Program.SettingsManager.GetExchangeRate();
            if (_context.Purchases.Any(p => p.InvoiceNumber == purchase.InvoiceNumber)) throw new InvalidOperationException("The purchase invoice number already exists.");

            DateTime now = DateTime.UtcNow;
            purchase.PurchaseDate = purchase.PurchaseDate == default(DateTime) ? now : purchase.PurchaseDate;
            purchase.CreatedAt = now;
            purchase.UpdatedAt = now;

            decimal subtotal = 0m;
            decimal totalDiscount = 0m;
            decimal totalTax = 0m;
            var inventoryRows = new List<InventoryTransaction>();

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    foreach (PurchaseItem item in purchase.PurchaseItems)
                    {
                        if (item.ProductId <= 0) throw new InvalidOperationException("Every purchase item must have a product.");
                        if (item.Quantity <= 0) throw new InvalidOperationException("Purchase item quantity must be greater than zero.");
                        if (item.UnitCost < 0) throw new InvalidOperationException("Purchase item unit cost cannot be negative.");

                        Product product = _context.Products.SingleOrDefault(p => p.ProductId == item.ProductId);
                        if (product == null) throw new InvalidOperationException("Product does not exist. ProductId: " + item.ProductId);

                        decimal baseAmount = item.UnitCost * item.Quantity;
                        if (item.DiscountAmount < 0 || item.DiscountAmount > baseAmount) throw new InvalidOperationException("Purchase item discount is invalid.");
                        if (item.TaxAmount < 0) throw new InvalidOperationException("Purchase item tax cannot be negative.");

                        item.LineTotal = baseAmount - item.DiscountAmount + item.TaxAmount;
                        subtotal += baseAmount;
                        totalDiscount += item.DiscountAmount;
                        totalTax += item.TaxAmount;

                        int oldQuantity = product.StockQuantity;
                        int newQuantity = checked(oldQuantity + item.Quantity);
                        product.StockQuantity = newQuantity;
                        product.PurchasePrice = item.UnitCost;
                        product.UpdatedAt = now;

                        inventoryRows.Add(new InventoryTransaction
                        {
                            ProductId = product.ProductId,
                            TransactionType = "PURCHASE",
                            QuantityChange = item.Quantity,
                            OldQuantity = oldQuantity,
                            NewQuantity = newQuantity,
                            ReferenceType = "PURCHASE",
                            UserId = purchase.UserId,
                            CreatedAt = now,
                            Notes = "Stock received from purchase " + purchase.InvoiceNumber
                        });
                    }

                    purchase.Subtotal = subtotal;
                    purchase.DiscountAmount = totalDiscount;
                    purchase.TaxAmount = totalTax;
                    purchase.TotalAmount = subtotal - totalDiscount + totalTax;

                    if (purchase.PaidAmount < 0 || purchase.PaidAmount > purchase.TotalAmount)
                        throw new InvalidOperationException("Paid amount must be between zero and the purchase total.");

                    purchase.RemainingAmount = purchase.TotalAmount - purchase.PaidAmount;
                    purchase.PaymentStatus = purchase.RemainingAmount == 0m ? "PAID" : purchase.PaidAmount > 0m ? "PARTIAL" : "UNPAID";

                    decimal previousSupplierBalance = _context.SupplierTransactions
                        .Where(x => x.SupplierId == purchase.SupplierId && x.Currency == purchase.Currency)
                        .Select(x => (decimal?)(x.Credit - x.Debit))
                        .Sum() ?? 0m;

                    _context.Purchases.Add(purchase);
                    _context.SaveChanges();

                    foreach (InventoryTransaction inventoryRow in inventoryRows)
                    {
                        inventoryRow.ReferenceId = purchase.PurchaseId;
                        _context.InventoryTransactions.Add(inventoryRow);
                    }

                    decimal balanceAfterPurchase = previousSupplierBalance + purchase.TotalAmount;
                    _context.SupplierTransactions.Add(new SupplierTransaction
                    {
                        SupplierId = purchase.SupplierId,
                        TransactionType = "PURCHASE",
                        ReferenceType = "PURCHASE",
                        ReferenceId = purchase.PurchaseId,
                        Debit = 0m,
                        Credit = purchase.TotalAmount,
                        BalanceAfter = balanceAfterPurchase,
                        Currency = purchase.Currency,
                        Description = "Purchase " + purchase.InvoiceNumber,
                        UserId = purchase.UserId,
                        CreatedAt = now
                    });

                    if (purchase.PaidAmount > 0m)
                    {
                        var payment = new SupplierPayment
                        {
                            SupplierId = purchase.SupplierId,
                            Amount = purchase.PaidAmount,
                            Currency = purchase.Currency,
                            PaymentMethod = string.IsNullOrWhiteSpace(purchase.PaymentMethod) ? "CASH" : purchase.PaymentMethod.Trim().ToUpperInvariant(),
                            ReferenceNumber = purchase.InvoiceNumber,
                            Notes = "Payment recorded with purchase",
                            UserId = purchase.UserId,
                            PaymentDate = now
                        };
                        _context.SupplierPayments.Add(payment);
                        _context.SaveChanges();

                        _context.SupplierTransactions.Add(new SupplierTransaction
                        {
                            SupplierId = purchase.SupplierId,
                            TransactionType = "PAYMENT",
                            ReferenceType = "SUPPLIER_PAYMENT",
                            ReferenceId = payment.SupplierPaymentId,
                            Debit = purchase.PaidAmount,
                            Credit = 0m,
                            BalanceAfter = balanceAfterPurchase - purchase.PaidAmount,
                            Currency = purchase.Currency,
                            Description = "Payment with purchase " + purchase.InvoiceNumber,
                            UserId = purchase.UserId,
                            CreatedAt = now
                        });
                    }

                    _context.SaveChanges();
                    _context.Database.ExecuteSqlRaw(@"
DECLARE @warehouse_id INT=(SELECT TOP(1) warehouse_id FROM dbo.Warehouses WHERE is_active=1 ORDER BY is_default DESC,warehouse_id);
IF @warehouse_id IS NULL THROW 52910,'An active warehouse is required before receiving purchases.',1;
MERGE dbo.ProductStock WITH(HOLDLOCK) AS target
USING(SELECT @warehouse_id warehouse_id,pi.product_id,SUM(CONVERT(DECIMAL(24,8),pi.quantity)) quantity FROM dbo.PurchaseItems pi WHERE pi.purchase_id=@purchase GROUP BY pi.product_id) AS source
ON target.warehouse_id=source.warehouse_id AND target.product_id=source.product_id
WHEN MATCHED THEN UPDATE SET quantity=target.quantity+source.quantity,updated_at=SYSUTCDATETIME()
WHEN NOT MATCHED THEN INSERT(warehouse_id,product_id,quantity) VALUES(source.warehouse_id,source.product_id,source.quantity);
UPDATE it SET warehouse_id=@warehouse_id,quantity_change_decimal=CONVERT(DECIMAL(24,8),it.quantity_change),old_quantity_decimal=CONVERT(DECIMAL(24,8),it.old_quantity),new_quantity_decimal=CONVERT(DECIMAL(24,8),it.new_quantity)
FROM dbo.InventoryTransactions it WHERE it.reference_type=N'PURCHASE' AND it.reference_id=@purchase;",
                        new SqlParameter("@purchase",purchase.PurchaseId));
                    transaction.Commit();
                    return purchase;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private string GeneratePurchaseNumber()
        {
            return "PUR-" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpperInvariant();
        }
    }
}
