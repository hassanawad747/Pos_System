using System;

namespace Pos_System.Models
{
    public class SupplierPayment
    {
        public int SupplierPaymentId { get; set; }
        public int SupplierId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }
        public int UserId { get; set; }
        public DateTime PaymentDate { get; set; }

        public Supplier Supplier { get; set; }
        public User User { get; set; }
    }
}