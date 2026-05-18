using Pos_System.Data;
using Pos_System.Models;
using System.Linq;
using Pos_System.Services;

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
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            string normalizedUsername = username.Trim();
            User user = _context.Users
                .FirstOrDefault(u => u.Username == normalizedUsername);

            if (user == null || !PasswordHasher.Verify(password, user.Password_Hash))
            {
                return null;
            }

            if (PasswordHasher.NeedsRehash(user.Password_Hash))
            {
                user.Password_Hash = PasswordHasher.Hash(password);
                _context.SaveChanges();
            }

            return user;
        }
    }

}
