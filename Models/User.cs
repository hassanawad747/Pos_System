using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Models
{
    public class User
    {
        [Key]
        public int User_Id { get; set; }
        public string Username { get; set; }
        public string Password_Hash { get; set; }
        public string Role { get; set; }
        public DateTime Created_At { get; set; }
        public ICollection<Sale> Sales { get; set; }

    }
}
