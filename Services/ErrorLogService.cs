using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class ErrorLogService
    {
        private static bool enabled;
        public static void Enable()
        {
            if (enabled) return;
            enabled = true;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Log(e.Exception, "UI_THREAD", Form.ActiveForm == null ? null : Form.ActiveForm.Name);
            MessageBox.Show("An unexpected error occurred and was recorded.\n\n" + e.Exception.Message, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex != null) Log(ex, "UNHANDLED", null);
        }

        public static void Log(Exception ex, string operation, string formName)
        {
            if (ex == null) return;
            try
            {
                using (var conn = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
                using (var cmd = new SqlCommand(@"IF OBJECT_ID(N'dbo.ErrorLogs',N'U') IS NOT NULL
INSERT dbo.ErrorLogs(user_id,machine_name,form_name,operation,message,stack_trace,inner_message,severity)
VALUES(@u,@machine,@form,@operation,@message,@stack,@inner,N'ERROR');", conn))
                {
                    cmd.Parameters.Add("@u", SqlDbType.Int).Value = AppSession.UserId > 0 ? (object)AppSession.UserId : DBNull.Value;
                    cmd.Parameters.Add("@machine", SqlDbType.NVarChar, 120).Value = Environment.MachineName;
                    cmd.Parameters.Add("@form", SqlDbType.NVarChar, 120).Value = string.IsNullOrWhiteSpace(formName) ? (object)DBNull.Value : formName;
                    cmd.Parameters.Add("@operation", SqlDbType.NVarChar, 120).Value = string.IsNullOrWhiteSpace(operation) ? (object)DBNull.Value : operation;
                    cmd.Parameters.Add("@message", SqlDbType.NVarChar, -1).Value = ex.Message;
                    cmd.Parameters.Add("@stack", SqlDbType.NVarChar, -1).Value = string.IsNullOrWhiteSpace(ex.StackTrace) ? (object)DBNull.Value : ex.StackTrace;
                    cmd.Parameters.Add("@inner", SqlDbType.NVarChar, -1).Value = ex.InnerException == null ? (object)DBNull.Value : ex.InnerException.Message;
                    conn.Open(); cmd.ExecuteNonQuery();
                }
            }
            catch { /* Logging must never crash the POS. */ }
        }
    }
}
