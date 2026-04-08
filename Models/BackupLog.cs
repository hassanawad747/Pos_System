using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class BackupLog
    {
        public int BackupId { get; set; }   // ✅ Primary Key
        public DateTime BackupDate { get; set; }
        public string BackupType { get; set; } // full, incremental
        public string FilePath { get; set; }
        public string Status { get; set; } // success, failed
        public int PerformedBy { get; set; }

        // Navigation
        public User User { get; set; }

    }

}
