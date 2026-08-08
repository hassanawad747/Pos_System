using System;

namespace Pos_System.Models
{
    public class CustomerTransaction
    {
        public int CustomerTransactionId { get; set; }
        public int CustomerId { get; set; }
        public string TransactionType { get; set; }
        public string ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public int? UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Customer Customer { get; set; }
        public User User { get; set; }
    }
}
