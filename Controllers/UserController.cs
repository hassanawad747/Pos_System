using Pos_System.Data;
using Pos_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos_System.Controllers
{
    public class UserController
    {
        private readonly POSDbContext _context;

        public UserController(POSDbContext context)
        {
            _context = context;
        }

        public User Login(string username, string password)
        {
            return _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password_Hash == password);
        }
    }

}
