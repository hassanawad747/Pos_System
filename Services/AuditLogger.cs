using Pos_System;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class AuditLogger
    {
        public static void EnsureAuditTable()
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                IF OBJECT_ID(N'dbo.AuditLogs', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.AuditLogs
                    (
                        audit_log_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        username NVARCHAR(100) NOT NULL,
                        user_role NVARCHAR(50) NULL,
                        action_type NVARCHAR(20) NOT NULL,
                        entity_name NVARCHAR(100) NOT NULL,
                        entity_id NVARCHAR(100) NULL,
                        details NVARCHAR(1000) NULL,
                        created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
                    );
                END;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void EnsureCustomerBalanceColumns()
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                IF COL_LENGTH('dbo.Customers', 'balance_usd') IS NULL
                    ALTER TABLE dbo.Customers ADD balance_usd DECIMAL(24, 8) NOT NULL CONSTRAINT DF_Customers_balance_usd DEFAULT (0);

                IF COL_LENGTH('dbo.Customers', 'balance_lb') IS NULL
                    ALTER TABLE dbo.Customers ADD balance_lb DECIMAL(24, 8) NOT NULL CONSTRAINT DF_Customers_balance_lb DEFAULT (0);

                IF COL_LENGTH('dbo.Customers', 'balance_updated_at') IS NULL
                    ALTER TABLE dbo.Customers ADD balance_updated_at DATETIME2 NULL;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void Log(string actionType, string entityName, object entityId, string details)
        {
            try
            {
                EnsureAuditTable();

                using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
                using (SqlCommand command = new SqlCommand(@"
                    INSERT INTO dbo.AuditLogs
                    (username, user_role, action_type, entity_name, entity_id, details)
                    VALUES
                    (@username, @userRole, @actionType, @entityName, @entityId, @details);", connection))
                {
                    string username = string.IsNullOrWhiteSpace(LoginForm.LoggedInUsername)
                        ? Environment.UserName
                        : LoginForm.LoggedInUsername;

                    command.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value = username;
                    command.Parameters.Add("@userRole", SqlDbType.NVarChar, 50).Value = DBNull.Value;
                    command.Parameters.Add("@actionType", SqlDbType.NVarChar, 20).Value = actionType ?? string.Empty;
                    command.Parameters.Add("@entityName", SqlDbType.NVarChar, 100).Value = entityName ?? string.Empty;
                    command.Parameters.Add("@entityId", SqlDbType.NVarChar, 100).Value =
                        entityId == null ? (object)DBNull.Value : entityId.ToString();
                    command.Parameters.Add("@details", SqlDbType.NVarChar, 1000).Value =
                        string.IsNullOrWhiteSpace(details) ? (object)DBNull.Value : details;

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Audit log could not be saved: " + ex.Message, "Audit Log");
            }
        }
    }
}
