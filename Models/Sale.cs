using System;
using System.Collections.Generic;

namespace Pos_System.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public string InvoiceNumber { get; set; }
        public int? UserId { get; set; }
        public int? CustomerId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public User User { get; set; }
        public Customer Customer { get; set; }
        public ICollection<SaleItem> SaleItems { get; set; }
    }
}
