using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class SalesBackup
    {
        public int SalesBackupId { get; set; }   // ✅ Primary Key


        public int SaleId { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public DateTime BackupDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }

        // Navigation
        public User User { get; set; }
        public Customer Customer { get; set; }
        public ICollection<SaleItemsBackup> SaleItemsBackups { get; set; }
    }

}
