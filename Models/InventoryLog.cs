using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class InventoryLog
    {
        public int LogId { get; set; }   // ✅ Primary Key

        public int ProductId { get; set; }
        public string ChangeType { get; set; } // add, remove, adjust
        public int QuantityChange { get; set; }
        public int UserId { get; set; }
        public DateTime Timestamp { get; set; }

        // Navigation
        public Product Product { get; set; }
        public User User { get; set; }

    }

}
