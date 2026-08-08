using Microsoft.EntityFrameworkCore;
using Pos_System.Data;
using Pos_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pos_System.Services
{
    public class SupplierLedgerService
    {
        private readonly POSDbContext _context;

        public SupplierLedgerService(POSDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public decimal GetBalance(int supplierId, string currency = "USD")
        {
            string normalized = NormalizeCurrency(currency);
            return _context.SupplierTransactions
                .Where(x => x.SupplierId == supplierId && x.Currency == normalized)
                .Select(x => (decimal?)(x.Credit - x.Debit))
                .Sum() ?? 0m;
        }

        public List<SupplierTransaction> GetTransactions(int supplierId, string currency = "USD")
        {
            string normalized = NormalizeCurrency(currency);
            return _context.SupplierTransactions
                .Where(x => x.SupplierId == supplierId && x.Currency == normalized)
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.SupplierTransactionId)
                .AsNoTracking()
                .ToList();
        }

        public SupplierPayment RecordPayment(int supplierId, decimal amount, string paymentMethod, string referenceNumber, string notes, int userId, string currency = "USD")
        {
            ActionPermissionService.Demand("SUPPLIER.PAYMENT");
            if (supplierId <= 0) throw new InvalidOperationException("Select a supplier.");
            if (userId <= 0) throw new InvalidOperationException("A logged-in user is required.");
            if (amount <= 0) throw new InvalidOperationException("Payment amount must be greater than zero.");
            if (!_context.Suppliers.Any(x => x.SupplierId == supplierId)) throw new InvalidOperationException("Supplier does not exist.");

            string normalized = NormalizeCurrency(currency);
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    decimal previous = GetBalance(supplierId, normalized);
                    var payment = new SupplierPayment
                    {
                        SupplierId = supplierId,
                        Amount = amount,
                        Currency = normalized,
                        PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "CASH" : paymentMethod.Trim().ToUpperInvariant(),
                        ReferenceNumber = string.IsNullOrWhiteSpace(referenceNumber) ? null : referenceNumber.Trim(),
                        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
                        UserId = userId,
                        PaymentDate = DateTime.UtcNow
                    };

                    _context.SupplierPayments.Add(payment);
                    _context.SaveChanges();

                    _context.SupplierTransactions.Add(new SupplierTransaction
                    {
                        SupplierId = supplierId,
                        TransactionType = "PAYMENT",
                        ReferenceType = "SUPPLIER_PAYMENT",
                        ReferenceId = payment.SupplierPaymentId,
                        Debit = amount,
                        Credit = 0m,
                        BalanceAfter = previous - amount,
                        Currency = normalized,
                        Description = "Payment to supplier" + (string.IsNullOrWhiteSpace(referenceNumber) ? string.Empty : " - " + referenceNumber.Trim()),
                        UserId = userId,
                        CreatedAt = payment.PaymentDate
                    });

                    _context.SaveChanges();
                    transaction.Commit();
                    return payment;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static string NormalizeCurrency(string currency)
        {
            string value = (currency ?? "USD").Trim().ToUpperInvariant();
            if (value == "L.L" || value == "LB" || value == "LBP") return "LBP";
            return "USD";
        }
    }
}
