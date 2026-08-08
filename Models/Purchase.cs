using System;
using System.Collections.Generic;

namespace Pos_System.Models
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public string InvoiceNumber { get; set; }
        public int SupplierId { get; set; }
        public int UserId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Supplier Supplier { get; set; }
        public User User { get; set; }
        public ICollection<PurchaseItem> PurchaseItems { get; set; }
    }
}
