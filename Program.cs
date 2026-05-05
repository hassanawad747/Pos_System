using Microsoft.EntityFrameworkCore;
using Pos_System;
using Pos_System.Data;
using Pos_System.Forms;
using Pos_System.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace POS_System
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            var options = new DbContextOptionsBuilder<POSDbContext>()
                .UseSqlServer(SettingsManager.ConnectionString)
                .Options;

            var context = new POSDbContext(options);

            AuditLogger.EnsureAuditTable();
            WorkHistoryService.EnsureHistoryTable();
            AuditLogger.EnsureCustomerBalanceColumns();
            InitializeDefaultSettings();
            SettingsManager.LoadSettings();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm(context));
        }

        private static void InitializeDefaultSettings()
        {
            SettingsManager.EnsureDefaultSettings();
        }

        public static class SettingsManager
        {
            private const string ConnectionStringName =
                "Pos_System.Properties.Settings.pos_systemConnectionString";

            private static readonly Dictionary<string, string> settingsCache =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            private static readonly Dictionary<string, string> defaultSettings =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "currency", "USD" },
                    { "language", "English" },
                    { "theme", "default" },
                    { "default_role", "cashier" },
                    { "password_max_length", "12" },
                    { "session_timeout_minutes", "30" },
                    { "tax_percentage", "10" },
                    { "max_discount", "20" },
                    { "low_stock_threshold", "5" },
                    { "report_format", "PDF" },
                    { "backup_schedule", "Daily" },
                    { "audit_logs_enabled", "true" },
                    { "refund_approval_required", "true" },
                    { "lock_system_after_failed_logins", "true" },
                    { "login_lock_attempts", "3" },
                    { "exchange_rate", "89000" },
                    { "default_price", "89000" }
                };

            private static readonly HashSet<Form> registeredForms = new HashSet<Form>();
            private static readonly Dictionary<Control, ControlColorSnapshot> originalColors =
                new Dictionary<Control, ControlColorSnapshot>();

            public static string ConnectionString
            {
                get
                {
                    ConnectionStringSettings configuredConnection =
                        ConfigurationManager.ConnectionStrings[ConnectionStringName];

                    if (configuredConnection != null &&
                        !string.IsNullOrWhiteSpace(configuredConnection.ConnectionString))
                    {
                        return configuredConnection.ConnectionString;
                    }

                    return Pos_System.Properties.Settings.Default.pos_systemConnectionString;
                }
            }

            public static IReadOnlyDictionary<string, string> DefaultSettings => defaultSettings;

            public static void EnsureDefaultSettings()
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    foreach (KeyValuePair<string, string> kvp in defaultSettings)
                    {
                        using (SqlCommand cmd = new SqlCommand(@"
                            IF NOT EXISTS (SELECT 1 FROM Settings WHERE key_name = @key)
                                INSERT INTO Settings (key_name, value) VALUES (@key, @value)", conn))
                        {
                            cmd.Parameters.AddWithValue("@key", kvp.Key);
                            cmd.Parameters.AddWithValue("@value", kvp.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }

            public static void LoadSettings()
            {
                EnsureDefaultSettings();
                settingsCache.Clear();

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("SELECT key_name, value FROM Settings", conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string key = reader["key_name"].ToString();
                            string value = reader["value"].ToString();
                            settingsCache[key] = value;
                        }
                    }
                }

                foreach (KeyValuePair<string, string> kvp in defaultSettings)
                {
                    if (!settingsCache.ContainsKey(kvp.Key))
                    {
                        settingsCache[kvp.Key] = kvp.Value;
                    }
                }
            }

            public static string GetSetting(string key, string defaultValue = "")
            {
                if (settingsCache.TryGetValue(key, out string value))
                {
                    return value;
                }

                if (defaultSettings.TryGetValue(key, out string fallback))
                {
                    return fallback;
                }

                return defaultValue;
            }

            public static bool GetBoolSetting(string key, bool defaultValue = false)
            {
                string value = GetSetting(key, defaultValue ? "true" : "false");
                return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                       value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                       value.Equals("yes", StringComparison.OrdinalIgnoreCase);
            }

            public static int GetIntSetting(string key, int defaultValue = 0)
            {
                return int.TryParse(GetSetting(key, defaultValue.ToString()), out int value)
                    ? value
                    : defaultValue;
            }

            public static decimal GetDecimalSetting(string key, decimal defaultValue = 0m)
            {
                string raw = GetSetting(key, defaultValue.ToString(CultureInfo.InvariantCulture));

                if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal invariantValue))
                {
                    return invariantValue;
                }

                return decimal.TryParse(raw, out decimal currentValue) ? currentValue : defaultValue;
            }

            public static decimal GetExchangeRate()
            {
                decimal exchangeRate = GetDecimalSetting("exchange_rate", 0m);
                if (exchangeRate <= 0m)
                {
                    exchangeRate = GetDecimalSetting("default_price", 89000m);
                }

                return exchangeRate > 0m ? exchangeRate : 89000m;
            }

            public static void SaveSettings(IDictionary<string, string> settings)
            {
                if (settings != null && settings.TryGetValue("exchange_rate", out string exchangeRate))
                {
                    settings["default_price"] = exchangeRate;
                }

                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    conn.Open();
                    SqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        foreach (KeyValuePair<string, string> kvp in settings)
                        {
                            using (SqlCommand cmd = new SqlCommand(@"
                                IF EXISTS (SELECT 1 FROM Settings WHERE key_name = @key)
                                    UPDATE Settings SET value = @value WHERE key_name = @key
                                ELSE
                                    INSERT INTO Settings (key_name, value) VALUES (@key, @value)", conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@key", kvp.Key);
                                cmd.Parameters.AddWithValue("@value", kvp.Value ?? string.Empty);
                                cmd.ExecuteNonQuery();
                            }

                            settingsCache[kvp.Key] = kvp.Value ?? string.Empty;
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            public static void RegisterForm(Form form)
            {
                if (form == null || registeredForms.Contains(form))
                {
                    return;
                }

                CaptureOriginalColors(form);
                ResponsiveLayoutService.Register(form);
                ArabicToolTipService.Register(form);
                registeredForms.Add(form);
                form.Shown += RegisteredFormShown;
                form.Disposed += RegisteredFormDisposed;
                ApplySettingsToForm(form);
            }

            private static void RegisteredFormShown(object sender, EventArgs e)
            {
                Form form = sender as Form;
                ArabicToolTipService.Register(form);
                ApplySettingsToForm(form);
            }

            private static void RegisteredFormDisposed(object sender, EventArgs e)
            {
                Form form = sender as Form;
                if (form == null)
                {
                    return;
                }

                registeredForms.Remove(form);
                form.Shown -= RegisteredFormShown;
                form.Disposed -= RegisteredFormDisposed;
            }

            public static void ApplySettingsToOpenForms()
            {
                List<Form> openForms = GetOpenFormsSnapshot();
                List<Form> registeredSnapshot = GetRegisteredFormsSnapshot();

                foreach (Form form in openForms)
                {
                    TryApplySettingsToForm(form);
                }

                foreach (Form form in registeredSnapshot)
                {
                    if (!openForms.Contains(form))
                    {
                        TryApplySettingsToForm(form);
                    }
                }
            }

            private static List<Form> GetOpenFormsSnapshot()
            {
                List<Form> forms = new List<Form>();

                for (int index = 0; index < Application.OpenForms.Count; index++)
                {
                    Form form = Application.OpenForms[index];
                    if (form != null && !form.IsDisposed && !forms.Contains(form))
                    {
                        forms.Add(form);
                    }
                }

                return forms;
            }

            private static List<Form> GetRegisteredFormsSnapshot()
            {
                try
                {
                    return registeredForms
                        .Where(form => form != null && !form.IsDisposed)
                        .ToList();
                }
                catch (InvalidOperationException)
                {
                    return new List<Form>();
                }
            }

            private static void TryApplySettingsToForm(Form form)
            {
                try
                {
                    ApplySettingsToForm(form);
                }
                catch (InvalidOperationException)
                {
                    // A form/control collection changed while applying UI settings.
                    // The next open/register event will apply the same settings again.
                }
            }

            public static void ApplySettingsToForm(Form form)
            {
                if (form == null)
                {
                    return;
                }

                bool isArabic = GetSetting("language", "English")
                    .Equals("Arabic", StringComparison.OrdinalIgnoreCase);
                string theme = GetSetting("theme", "default").Trim().ToLowerInvariant();

                ApplyDirection(form, isArabic);
                ApplyTheme(form, theme);
                RuntimeLanguageService.Apply(form, isArabic);
                form.Refresh();
            }

            private static void ApplyTheme(Control control, string theme)
            {
                if (control == null)
                {
                    return;
                }

                switch (theme)
                {
                    case "light":
                        ApplyColorTheme(control, Color.White, Color.Black);
                        break;
                    case "dark":
                        ApplyColorTheme(control, Color.Black, Color.White);
                        break;
                    default:
                        RestoreOriginalTheme(control);
                        break;
                }
            }

            private static void ApplyColorTheme(Control control, Color backColor, Color foreColor)
            {
                if (!originalColors.ContainsKey(control))
                {
                    originalColors[control] = new ControlColorSnapshot(control.BackColor, control.ForeColor);
                }

                if (control is Label label)
                {
                    label.BackColor = Color.Transparent;
                    label.ForeColor = foreColor;
                }
                else if (control is Button button)
                {
                    button.UseVisualStyleBackColor = false;
                    button.BackColor = backColor;
                    button.ForeColor = foreColor;
                    button.FlatAppearance.BorderColor = foreColor;
                    button.FlatAppearance.MouseOverBackColor = backColor;
                    button.FlatAppearance.MouseDownBackColor = backColor;
                }
                else if (control is TextBox || control is ComboBox || control is ListBox)
                {
                    control.BackColor = backColor;
                    control.ForeColor = foreColor;
                }
                else if (control is DataGridView grid)
                {
                    grid.BackgroundColor = backColor;
                    grid.GridColor = foreColor;
                    grid.EnableHeadersVisualStyles = false;
                    grid.ColumnHeadersDefaultCellStyle.BackColor = backColor;
                    grid.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
                    grid.DefaultCellStyle.BackColor = backColor;
                    grid.DefaultCellStyle.ForeColor = foreColor;
                    grid.DefaultCellStyle.SelectionBackColor = foreColor;
                    grid.DefaultCellStyle.SelectionForeColor = backColor;
                    grid.RowHeadersDefaultCellStyle.BackColor = backColor;
                    grid.RowHeadersDefaultCellStyle.ForeColor = foreColor;
                    grid.AlternatingRowsDefaultCellStyle.BackColor = backColor;
                    grid.AlternatingRowsDefaultCellStyle.ForeColor = foreColor;
                }
                else if (!(control is PictureBox))
                {
                    control.BackColor = backColor;
                    control.ForeColor = foreColor;
                }

                foreach (Control child in control.Controls.Cast<Control>().ToList())
                {
                    ApplyColorTheme(child, backColor, foreColor);
                }
            }

            private static void RestoreOriginalTheme(Control control)
            {
                if (control == null)
                {
                    return;
                }

                if (originalColors.TryGetValue(control, out ControlColorSnapshot snapshot))
                {
                    control.BackColor = snapshot.BackColor;
                    control.ForeColor = snapshot.ForeColor;
                }

                if (control is Button button)
                {
                    button.UseVisualStyleBackColor = false;
                }

                foreach (Control child in control.Controls.Cast<Control>().ToList())
                {
                    RestoreOriginalTheme(child);
                }
            }

            private static void CaptureOriginalColors(Control control)
            {
                if (control == null || originalColors.ContainsKey(control))
                {
                    return;
                }

                originalColors[control] = new ControlColorSnapshot(control.BackColor, control.ForeColor);

                foreach (Control child in control.Controls.Cast<Control>().ToList())
                {
                    CaptureOriginalColors(child);
                }
            }

            private static void ApplyRightToLeft(Control.ControlCollection controls, bool isArabic)
            {
                List<Control> snapshot = controls.Cast<Control>().ToList();

                foreach (Control control in snapshot)
                {
                    control.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;

                    if (control is DataGridView grid)
                    {
                        grid.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;
                    }

                    if (control is ToolStrip toolStrip)
                    {
                        toolStrip.RightToLeft = isArabic ? RightToLeft.Yes : RightToLeft.No;
                    }

                    if (control.HasChildren)
                    {
                        ApplyRightToLeft(control.Controls, isArabic);
                    }
                }
            }

            private static void ApplyDirection(Form form, bool isArabic)
            {
                RightToLeft direction = isArabic ? RightToLeft.Yes : RightToLeft.No;
                form.RightToLeft = direction;
                form.RightToLeftLayout = isArabic;
                ApplyRightToLeft(form.Controls, isArabic);
            }

            private sealed class ControlColorSnapshot
            {
                public ControlColorSnapshot(Color backColor, Color foreColor)
                {
                    BackColor = backColor;
                    ForeColor = foreColor;
                }

                public Color BackColor { get; }
                public Color ForeColor { get; }
            }
        }
    }
}
