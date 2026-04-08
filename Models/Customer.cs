using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int LoyaltyPoints { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<Sale> Sales { get; set; }
        public ICollection<Return> Returns { get; set; }
    }

}
