using System;
using System.Data.SqlClient;

namespace Pos_System.Services
{
    internal static class ActionPermissionService
    {
        public static bool Can(string actionKey, string connectionString = null)
        {
            if (AppSession.IsAdministrator) return true;
            if (string.IsNullOrWhiteSpace(actionKey) || string.IsNullOrWhiteSpace(AppSession.Role)) return false;
            try
            {
                string effectiveConnectionString = string.IsNullOrWhiteSpace(connectionString)
                    ? POS_System.Program.SettingsManager.ConnectionString
                    : connectionString;
                using (var conn = new SqlConnection(effectiveConnectionString))
                using (var cmd = new SqlCommand("SELECT TOP(1) is_allowed FROM dbo.ActionPermissions WHERE role_name=@role AND action_key=@action;", conn))
                {
                    cmd.Parameters.AddWithValue("@role", AppSession.Role);
                    cmd.Parameters.AddWithValue("@action", actionKey.Trim().ToUpperInvariant());
                    conn.Open(); object value = cmd.ExecuteScalar();
                    return value != null && value != DBNull.Value && Convert.ToBoolean(value);
                }
            }
            catch { return false; }
        }

        public static void Demand(string actionKey)
        {
            if (!Can(actionKey)) throw new UnauthorizedAccessException("You do not have permission for action: " + actionKey);
        }
    }
}
