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
            {
                connection.Open();

                string[] commands =
                {
                    @"
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
                    END;",
                    "IF COL_LENGTH('dbo.AuditLogs', 'username') IS NULL ALTER TABLE dbo.AuditLogs ADD username NVARCHAR(100) NULL;",
                    "IF COL_LENGTH('dbo.AuditLogs', 'user_role') IS NULL ALTER TABLE dbo.AuditLogs ADD user_role NVARCHAR(50) NULL;",
                    "IF COL_LENGTH('dbo.AuditLogs', 'entity_id') IS NULL ALTER TABLE dbo.AuditLogs ADD entity_id NVARCHAR(100) NULL;",
                    "IF COL_LENGTH('dbo.AuditLogs', 'details') IS NULL ALTER TABLE dbo.AuditLogs ADD details NVARCHAR(1000) NULL;",
                    "IF COL_LENGTH('dbo.AuditLogs', 'record_key') IS NOT NULL AND COL_LENGTH('dbo.AuditLogs', 'entity_id') IS NOT NULL EXEC(N'UPDATE dbo.AuditLogs SET entity_id = ISNULL(entity_id, record_key) WHERE entity_id IS NULL');",
                    "IF COL_LENGTH('dbo.AuditLogs', 'description') IS NOT NULL AND COL_LENGTH('dbo.AuditLogs', 'details') IS NOT NULL EXEC(N'UPDATE dbo.AuditLogs SET details = ISNULL(details, description) WHERE details IS NULL');",
                    "IF COL_LENGTH('dbo.AuditLogs', 'changed_by') IS NOT NULL AND COL_LENGTH('dbo.AuditLogs', 'username') IS NOT NULL EXEC(N'UPDATE dbo.AuditLogs SET username = ISNULL(username, changed_by) WHERE username IS NULL');"
                };

                foreach (string sql in commands)
                {
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
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

        public static void EnsureSupplierBalanceColumns()
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                IF COL_LENGTH('dbo.Suppliers', 'balance_usd') IS NULL
                    ALTER TABLE dbo.Suppliers ADD balance_usd DECIMAL(24, 8) NOT NULL CONSTRAINT DF_Suppliers_balance_usd DEFAULT (0);

                IF COL_LENGTH('dbo.Suppliers', 'balance_lb') IS NULL
                    ALTER TABLE dbo.Suppliers ADD balance_lb DECIMAL(24, 8) NOT NULL CONSTRAINT DF_Suppliers_balance_lb DEFAULT (0);

                IF COL_LENGTH('dbo.Suppliers', 'balance_updated_at') IS NULL
                    ALTER TABLE dbo.Suppliers ADD balance_updated_at DATETIME2 NULL;

                IF COL_LENGTH('dbo.Suppliers', 'email') IS NULL
                    ALTER TABLE dbo.Suppliers ADD email NVARCHAR(255) NULL;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void EnsureSalesColumns()
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                IF COL_LENGTH('dbo.Sales', 'is_returned') IS NULL
                    ALTER TABLE dbo.Sales ADD is_returned BIT NOT NULL CONSTRAINT DF_Sales_is_returned DEFAULT (0);

                IF COL_LENGTH('dbo.Sales', 'status') IS NULL
                    ALTER TABLE dbo.Sales ADD status NVARCHAR(50) NULL;", connection))
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
                AuditService.EnsureNotificationTable();

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

                    using (SqlCommand notificationCommand = new SqlCommand(@"
                        INSERT INTO dbo.Notifications
                        (username, action_type, entity_name, entity_id, title, message)
                        VALUES
                        (@username, @actionType, @entityName, @entityId, @title, @message);", connection))
                    {
                        notificationCommand.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value = username;
                        notificationCommand.Parameters.Add("@actionType", SqlDbType.NVarChar, 20).Value = actionType ?? string.Empty;
                        notificationCommand.Parameters.Add("@entityName", SqlDbType.NVarChar, 100).Value = entityName ?? string.Empty;
                        notificationCommand.Parameters.Add("@entityId", SqlDbType.NVarChar, 100).Value =
                            entityId == null ? (object)DBNull.Value : entityId.ToString();
                        notificationCommand.Parameters.Add("@title", SqlDbType.NVarChar, 200).Value =
                            (actionType ?? "INFO") + " " + (entityName ?? "Record");
                        notificationCommand.Parameters.Add("@message", SqlDbType.NVarChar, 1000).Value =
                            string.IsNullOrWhiteSpace(details) ? (object)DBNull.Value : details;
                        notificationCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Audit log could not be saved: " + ex.Message, "Audit Log");
            }
        }
    }
}
