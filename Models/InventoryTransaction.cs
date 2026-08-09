using System;

namespace Pos_System.Models
{
    public class InventoryTransaction
    {
        public int InventoryTransactionId { get; set; }
        public int ProductId { get; set; }
        public string TransactionType { get; set; }
        public int QuantityChange { get; set; }
        public int OldQuantity { get; set; }
        public int NewQuantity { get; set; }
        public string ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public int? UserId { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; }
        public User User { get; set; }
    }
}
