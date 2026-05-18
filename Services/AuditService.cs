using System;
using System.Data;
using System.Data.SqlClient;

namespace Pos_System.Services
{
    internal static class AuditService
    {
        public static void EnsureNotificationTable()
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Notifications
                    (
                        notification_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        username NVARCHAR(100) NULL,
                        action_type NVARCHAR(20) NOT NULL,
                        entity_name NVARCHAR(100) NOT NULL,
                        entity_id NVARCHAR(100) NULL,
                        title NVARCHAR(200) NOT NULL,
                        message NVARCHAR(1000) NULL,
                        is_read BIT NOT NULL CONSTRAINT DF_Notifications_is_read DEFAULT (0),
                        created_at DATETIME2 NOT NULL CONSTRAINT DF_Notifications_created_at DEFAULT SYSUTCDATETIME()
                    );
                END;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void Log(string entityName, string action, string recordKey, string description)
        {
            string normalizedAction = NormalizeAction(action);
            object entityId = string.IsNullOrWhiteSpace(recordKey) ? null : (object)recordKey;
            AuditLogger.Log(normalizedAction, entityName, entityId, description);
        }

        public static int GetTodayActivityCount()
        {
            try
            {
                EnsureNotificationTable();

                using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
                using (SqlCommand command = new SqlCommand(@"
                    SELECT COUNT(1)
                    FROM dbo.Notifications
                    WHERE is_read = 0;", connection))
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch
            {
                return 0;
            }
        }

        public static DataTable GetNotifications(string usernameFilter, string actionFilter, DateTime? dateFilter)
        {
            EnsureNotificationTable();

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                SELECT TOP 100
                    notification_id AS [ID],
                    created_at AS [Time],
                    ISNULL(username, N'System') AS [User],
                    action_type AS [Action],
                    entity_name AS [Screen/Table],
                    entity_id AS [Record],
                    message AS [Details],
                    CASE WHEN is_read = 1 THEN N'Read' ELSE N'New' END AS [Status]
                FROM dbo.Notifications
                WHERE (@username = N'' OR ISNULL(username, N'') LIKE N'%' + @username + N'%')
                  AND (@action = N'' OR action_type LIKE N'%' + @action + N'%' OR title LIKE N'%' + @action + N'%' OR message LIKE N'%' + @action + N'%')
                  AND (@dateFrom IS NULL OR created_at >= @dateFrom)
                  AND (@dateTo IS NULL OR created_at < @dateTo)
                ORDER BY notification_id DESC;", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(usernameFilter) ? string.Empty : usernameFilter.Trim();
                command.Parameters.Add("@action", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(actionFilter) ? string.Empty : NormalizeAction(actionFilter);
                command.Parameters.Add("@dateFrom", SqlDbType.DateTime2).Value =
                    dateFilter.HasValue ? (object)dateFilter.Value : DBNull.Value;
                command.Parameters.Add("@dateTo", SqlDbType.DateTime2).Value =
                    dateFilter.HasValue ? (object)dateFilter.Value.AddDays(1) : DBNull.Value;

                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static void MarkAllNotificationsAsRead()
        {
            EnsureNotificationTable();

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand("UPDATE dbo.Notifications SET is_read = 1 WHERE is_read = 0;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static void DeleteNotification(int notificationId)
        {
            EnsureNotificationTable();

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand("DELETE FROM dbo.Notifications WHERE notification_id = @id;", connection))
            {
                command.Parameters.Add("@id", SqlDbType.Int).Value = notificationId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static string NormalizeAction(string action)
        {
            string value = (action ?? string.Empty).Trim().ToUpperInvariant();

            switch (value)
            {
                case "CREATE":
                    return "ADD";
                case "EDIT":
                case "UPDATE":
                    return "EDIT";
                case "DELETE":
                    return "DELETE";
                default:
                    return string.IsNullOrWhiteSpace(value) ? "INFO" : value;
            }
        }
    }
}
