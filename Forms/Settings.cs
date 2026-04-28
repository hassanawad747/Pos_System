using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using static POS_System.Program;

namespace Pos_System.Forms
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            SettingsManager.RegisterForm(this);
            ConfigureSettingsControls();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            labeldate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            LoadSettingsIntoControls();
        }

        private void ConfigureSettingsControls()
        {
            comboCurrency.DropDownStyle = ComboBoxStyle.DropDownList;
            comboLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboLength.DropDownStyle = ComboBoxStyle.DropDownList;
            comboReport.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBuckup.DropDownStyle = ComboBoxStyle.DropDownList;

            comboCurrency.Items.Clear();
            comboCurrency.Items.AddRange(new object[] { "USD", "LBP", "EUR" });

            comboLanguage.Items.Clear();
            comboLanguage.Items.AddRange(new object[] { "English", "Arabic" });

            comboLength.Items.Clear();
            comboLength.Items.AddRange(new object[] { "8", "10", "12", "16", "20" });

            comboReport.Items.Clear();
            comboReport.Items.AddRange(new object[] { "PDF", "Excel", "Screen" });

            comboBuckup.Items.Clear();
            comboBuckup.Items.AddRange(new object[] { "Daily", "Weekly", "Monthly", "Manual" });

            txtpercentage.MaxLength = 5;
            txtDixcount.MaxLength = 5;
            txtstock.MaxLength = 5;
            txtsession.MaxLength = 4;
            textBox1.MaxLength = 2;

            txtpercentage.KeyPress += NumericTextBox_KeyPress;
            txtDixcount.KeyPress += NumericTextBox_KeyPress;
            txtstock.KeyPress += IntegerTextBox_KeyPress;
            txtsession.KeyPress += IntegerTextBox_KeyPress;
            textBox1.KeyPress += IntegerTextBox_KeyPress;
            checkBox_Lock.CheckedChanged += CheckBox_Lock_CheckedChanged;
        }

        private void LoadSettingsIntoControls()
        {
            comboCurrency.SelectedItem = SettingsManager.GetSetting("currency", "USD");
            comboLanguage.SelectedItem = SettingsManager.GetSetting("language", "English");
            comboLength.SelectedItem = SettingsManager.GetSetting("password_max_length", "12");
            comboReport.SelectedItem = SettingsManager.GetSetting("report_format", "PDF");
            comboBuckup.SelectedItem = SettingsManager.GetSetting("backup_schedule", "Daily");

            string theme = SettingsManager.GetSetting("theme", "default");
            rdDefault.Checked = theme.Equals("default", StringComparison.OrdinalIgnoreCase);
            rdlight.Checked = theme.Equals("light", StringComparison.OrdinalIgnoreCase);
            rdDark.Checked = theme.Equals("dark", StringComparison.OrdinalIgnoreCase);

            if (!rdDefault.Checked && !rdlight.Checked && !rdDark.Checked)
            {
                rdDefault.Checked = true;
            }

            ApplyRoleSelection(SettingsManager.GetSetting("default_role", "cashier"));

            txtsession.Text = SettingsManager.GetIntSetting("session_timeout_minutes", 30).ToString();
            txtpercentage.Text = SettingsManager.GetDecimalSetting("tax_percentage", 10m)
                .ToString("0.##", CultureInfo.InvariantCulture);
            txtDixcount.Text = SettingsManager.GetDecimalSetting("max_discount", 20m)
                .ToString("0.##", CultureInfo.InvariantCulture);
            txtstock.Text = SettingsManager.GetIntSetting("low_stock_threshold", 5).ToString();
            textBox1.Text = SettingsManager.GetIntSetting("login_lock_attempts", 3).ToString();

            checkBox_Audit.Checked = SettingsManager.GetBoolSetting("audit_logs_enabled", true);
            checkBoxManager.Checked = SettingsManager.GetBoolSetting("refund_approval_required", true);
            checkBox_Lock.Checked = SettingsManager.GetBoolSetting("lock_system_after_failed_logins", true);

            ToggleLockAttemptsField();
        }

        private void ApplyDefaultValuesToControls()
        {
            comboCurrency.SelectedItem = "USD";
            comboLanguage.SelectedItem = "English";
            comboLength.SelectedItem = "12";
            comboReport.SelectedItem = "PDF";
            comboBuckup.SelectedItem = "Daily";
            rdDefault.Checked = true;
            rdlight.Checked = false;
            rdDark.Checked = false;
            ApplyRoleSelection("cashier");
            txtsession.Text = "30";
            txtpercentage.Text = "10";
            txtDixcount.Text = "20";
            txtstock.Text = "5";
            checkBox_Audit.Checked = true;
            checkBoxManager.Checked = true;
            checkBox_Lock.Checked = true;
            textBox1.Text = "3";
            ToggleLockAttemptsField();
        }

        private Dictionary<string, string> CollectSettingsFromControls()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "currency", comboCurrency.SelectedItem?.ToString() ?? "USD" },
                { "language", comboLanguage.SelectedItem?.ToString() ?? "English" },
                { "theme", rdDark.Checked ? "dark" : (rdlight.Checked ? "light" : "default") },
                { "default_role", GetSelectedRole() },
                { "password_max_length", comboLength.SelectedItem?.ToString() ?? "12" },
                { "session_timeout_minutes", txtsession.Text.Trim() },
                { "tax_percentage", txtpercentage.Text.Trim() },
                { "max_discount", txtDixcount.Text.Trim() },
                { "low_stock_threshold", txtstock.Text.Trim() },
                { "report_format", comboReport.SelectedItem?.ToString() ?? "PDF" },
                { "backup_schedule", comboBuckup.SelectedItem?.ToString() ?? "Daily" },
                { "audit_logs_enabled", checkBox_Audit.Checked ? "true" : "false" },
                { "refund_approval_required", checkBoxManager.Checked ? "true" : "false" },
                { "lock_system_after_failed_logins", checkBox_Lock.Checked ? "true" : "false" },
                { "login_lock_attempts", textBox1.Text.Trim() }
            };
        }

        private bool ValidateSettings(out string validationMessage)
        {
            validationMessage = string.Empty;

            if (comboCurrency.SelectedItem == null)
            {
                validationMessage = "Please select a currency.";
                return false;
            }

            if (comboLanguage.SelectedItem == null)
            {
                validationMessage = "Please select a language.";
                return false;
            }

            if (comboLength.SelectedItem == null)
            {
                validationMessage = "Please select the password maximum length.";
                return false;
            }

            if (comboReport.SelectedItem == null)
            {
                validationMessage = "Please select a report format.";
                return false;
            }

            if (comboBuckup.SelectedItem == null)
            {
                validationMessage = "Please select a backup schedule.";
                return false;
            }

            if (!decimal.TryParse(txtpercentage.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal taxPercentage) &&
                !decimal.TryParse(txtpercentage.Text.Trim(), out taxPercentage))
            {
                validationMessage = "Tax percentage must be a valid number.";
                return false;
            }

            if (taxPercentage < 0m || taxPercentage > 100m)
            {
                validationMessage = "Tax percentage must be between 0 and 100.";
                return false;
            }

            if (!decimal.TryParse(txtDixcount.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal maxDiscount) &&
                !decimal.TryParse(txtDixcount.Text.Trim(), out maxDiscount))
            {
                validationMessage = "Max discount must be a valid number.";
                return false;
            }

            if (maxDiscount < 0m || maxDiscount > 100m)
            {
                validationMessage = "Max discount must be between 0 and 100.";
                return false;
            }

            if (!int.TryParse(txtstock.Text.Trim(), out int lowStockThreshold) || lowStockThreshold < 0)
            {
                validationMessage = "Low stock alert must be a valid positive number.";
                return false;
            }

            if (!int.TryParse(txtsession.Text.Trim(), out int sessionTimeout) || sessionTimeout < 1 || sessionTimeout > 1440)
            {
                validationMessage = "Session timeout must be between 1 and 1440 minutes.";
                return false;
            }

            if (checkBox_Lock.Checked)
            {
                if (!int.TryParse(textBox1.Text.Trim(), out int loginAttempts) || loginAttempts < 1 || loginAttempts > 10)
                {
                    validationMessage = "Lock attempts must be between 1 and 10.";
                    return false;
                }
            }

            return true;
        }

        private string GetSelectedRole()
        {
            if (rdAdmin.Checked)
            {
                return "admin";
            }

            if (rdManager.Checked)
            {
                return "manager";
            }

            return "cashier";
        }

        private void ApplyRoleSelection(string role)
        {
            string normalizedRole = (role ?? string.Empty).Trim().ToLowerInvariant();

            rdAdmin.Checked = normalizedRole == "admin";
            rdManager.Checked = normalizedRole == "manager";
            rdCasheir.Checked = !rdAdmin.Checked && !rdManager.Checked;
        }

        private void CheckBox_Lock_CheckedChanged(object sender, EventArgs e)
        {
            ToggleLockAttemptsField();
        }

        private void ToggleLockAttemptsField()
        {
            textBox1.Enabled = checkBox_Lock.Checked;

            if (!checkBox_Lock.Checked)
            {
                textBox1.Text = "0";
            }
            else if (string.IsNullOrWhiteSpace(textBox1.Text) || textBox1.Text == "0")
            {
                textBox1.Text = "3";
            }
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = sender as TextBox;
            if (e.KeyChar == '.' && textBox != null && textBox.Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        private void IntegerTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateSettings(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                SettingsManager.SaveSettings(CollectSettingsFromControls());
                SettingsManager.LoadSettings();
                SettingsManager.ApplySettingsToOpenForms();

                MessageBox.Show(
                    "Settings were saved and applied successfully across the application.",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to save settings.\n" + ex.Message,
                    "Save Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Reset all settings in this form to the recommended defaults?",
                "Reset Defaults",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            ApplyDefaultValuesToControls();
        }

        private void btnCLose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void rdDefault_CheckedChanged(object sender, System.EventArgs e)
        {
        }
    }
}
