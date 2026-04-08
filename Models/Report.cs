using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class Report
    {
        public int ReportId { get; set; }
        public string ReportType { get; set; } // daily, weekly, monthly, custom
        public int GeneratedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string FilePath { get; set; }

        // Navigation
        public User User { get; set; }
    }

}


