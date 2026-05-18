using Pos_System.Models;
using System;

namespace Pos_System.Services
{
    internal static class AppSession
    {
        public static int UserId { get; private set; }
        public static string Username { get; private set; }
        public static string Role { get; private set; }

        public static bool IsAdministrator =>
            string.Equals(Username, "admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Role, "admin", StringComparison.OrdinalIgnoreCase);

        public static bool IsCashier =>
            !IsAdministrator &&
            string.Equals(Role, "cashier", StringComparison.OrdinalIgnoreCase);

        public static void Set(User user)
        {
            if (user == null)
            {
                Clear();
                return;
            }

            UserId = user.User_Id;
            Username = user.Username ?? string.Empty;
            Role = IsAdministratorUser(user.Username) ? "admin" : (user.Role ?? string.Empty);
        }

        public static void Clear()
        {
            UserId = 0;
            Username = string.Empty;
            Role = string.Empty;
        }

        private static bool IsAdministratorUser(string username)
        {
            return string.Equals(username ?? string.Empty, "admin", StringComparison.OrdinalIgnoreCase);
        }
    }
}
