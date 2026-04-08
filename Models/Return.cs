using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class Return
    {
        public int ReturnId { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int UserId { get; set; }
        public DateTime ReturnDate { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public decimal RefundAmount { get; set; }

        // Navigation
        public Sale Sale { get; set; }
        public Product Product { get; set; }
        public Customer Customer { get; set; }
        public User User { get; set; }
    }


}
