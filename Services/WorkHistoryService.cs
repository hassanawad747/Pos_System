using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class WorkHistoryService
    {
        public static void EnsureHistoryTable()
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                IF OBJECT_ID(N'dbo.History', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.History
                    (
                        history_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        user_id INT NULL,
                        username NVARCHAR(100) NOT NULL,
                        work_date DATE NOT NULL,
                        start_time DATETIME2 NULL,
                        end_time DATETIME2 NULL,
                        worked_hours INT NULL,
                        worked_minutes INT NULL,
                        worked_duration_minutes INT NULL,
                        created_at DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
                        updated_at DATETIME2 NULL
                    );
                END;

                IF COL_LENGTH('dbo.History', 'worked_duration_minutes') IS NULL
                    ALTER TABLE dbo.History ADD worked_duration_minutes INT NULL;

                IF COL_LENGTH('dbo.History', 'updated_at') IS NULL
                    ALTER TABLE dbo.History ADD updated_at DATETIME2 NULL;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static int StartWork(int userId, string username)
        {
            EnsureHistoryTable();
            DateTime startedAt = DateTime.Now;

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                DECLARE @ExistingHistoryId INT;

                SELECT TOP (1) @ExistingHistoryId = history_id
                FROM dbo.History
                WHERE username = @username
                  AND work_date = @workDate
                  AND end_time IS NULL
                ORDER BY start_time DESC, history_id DESC;

                IF @ExistingHistoryId IS NULL
                BEGIN
                    INSERT INTO dbo.History (user_id, username, work_date, start_time)
                    VALUES (@userId, @username, @workDate, @startTime);

                    SET @ExistingHistoryId = SCOPE_IDENTITY();
                END;

                SELECT @ExistingHistoryId;", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId > 0 ? (object)userId : DBNull.Value;
                command.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value = NormalizeUsername(username);
                command.Parameters.Add("@workDate", SqlDbType.Date).Value = startedAt.Date;
                command.Parameters.Add("@startTime", SqlDbType.DateTime2).Value = startedAt;

                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static bool EndWork(int userId, string username, out TimeSpan workedTime)
        {
            EnsureHistoryTable();
            DateTime endedAt = DateTime.Now;
            workedTime = TimeSpan.Zero;

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                DECLARE @HistoryId INT;
                DECLARE @StartTime DATETIME2;

                SELECT TOP (1)
                    @HistoryId = history_id,
                    @StartTime = start_time
                FROM dbo.History
                WHERE username = @username
                  AND work_date = @workDate
                  AND end_time IS NULL
                ORDER BY start_time DESC, history_id DESC;

                IF @HistoryId IS NULL
                BEGIN
                    SELECT CAST(0 AS BIT) AS updated, CAST(0 AS INT) AS worked_minutes_total;
                    RETURN;
                END;

                DECLARE @WorkedMinutesTotal INT = DATEDIFF(MINUTE, @StartTime, @endTime);
                IF @WorkedMinutesTotal < 0 SET @WorkedMinutesTotal = 0;

                UPDATE dbo.History
                SET end_time = @endTime,
                    worked_hours = @WorkedMinutesTotal / 60,
                    worked_minutes = @WorkedMinutesTotal % 60,
                    worked_duration_minutes = @WorkedMinutesTotal,
                    updated_at = @endTime
                WHERE history_id = @HistoryId;

                SELECT CAST(1 AS BIT) AS updated, @WorkedMinutesTotal AS worked_minutes_total;", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId > 0 ? (object)userId : DBNull.Value;
                command.Parameters.Add("@username", SqlDbType.NVarChar, 100).Value = NormalizeUsername(username);
                command.Parameters.Add("@workDate", SqlDbType.Date).Value = endedAt.Date;
                command.Parameters.Add("@endTime", SqlDbType.DateTime2).Value = endedAt;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read() || !Convert.ToBoolean(reader["updated"]))
                    {
                        return false;
                    }

                    int minutes = Convert.ToInt32(reader["worked_minutes_total"]);
                    workedTime = TimeSpan.FromMinutes(minutes);
                    return true;
                }
            }
        }

        private static string NormalizeUsername(string username)
        {
            if (!string.IsNullOrWhiteSpace(username))
            {
                return username.Trim();
            }

            if (!string.IsNullOrWhiteSpace(LoginForm.LoggedInUsername))
            {
                return LoginForm.LoggedInUsername.Trim();
            }

            return Environment.UserName;
        }
    }
}
