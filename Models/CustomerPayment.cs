using System;

namespace Pos_System.Models
{
    public class CustomerPayment
    {
        public int CustomerPaymentId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }
        public int UserId { get; set; }
        public DateTime PaymentDate { get; set; }

        public Customer Customer { get; set; }
        public User User { get; set; }
    }
}
