using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class SaleItemsBackup
    {
        public int SaleItemsBackupId { get; set; }   // ✅ Primary Key

        public int SalesBackupId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public DateTime BackupDate { get; set; }

        // Navigation
        public SalesBackup SalesBackup { get; set; }
        public Product Product { get; set; }

    }

}
