using Microsoft.EntityFrameworkCore;
using Pos_System.Controllers;
using Pos_System.Data;
using Pos_System.Forms;
using Pos_System.Models;
using Pos_System.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pos_System
{
    public partial class LoginForm : Form
    {
        private readonly UserController _userController;
        private readonly POSDbContext _context;

        public static string LoggedInUsername;
        public static int LoggedInUserId;

        private static int failedLoginAttempts;
        private bool isPasswordVisible;
        private bool signingIn;
        private bool openingDashboard;

        public LoginForm()
            : this(CreateContext())
        {
        }

        public LoginForm(POSDbContext context)
        {
            _context = context ?? CreateContext();
            _userController = new UserController(_context);
            InitializeComponent();
            ConfigureLoginShortcuts();
            FormClosing += LoginForm_FormClosing;
            Shown += LoginForm_Shown;
            POS_System.Program.SettingsManager.RegisterForm(this);
        }

        private static POSDbContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<POSDbContext>();
            optionsBuilder.UseSqlServer(POS_System.Program.SettingsManager.ConnectionString);
            return new POSDbContext(optionsBuilder.Options);
        }

        private void ConfigureLoginShortcuts()
        {
            AcceptButton = btnlogin;
            CancelButton = btnExit;
            KeyPreview = true;

            txtusername.KeyDown += LoginTextBox_KeyDown;
            txtpassword.KeyDown += LoginTextBox_KeyDown;
            KeyDown += LoginTextBox_KeyDown;
        }

        private void LoginForm_Shown(object sender, EventArgs e)
        {
            if (txtusername != null && txtusername.CanFocus)
            {
                txtusername.Focus();
            }
        }

        private void LoginTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;

            if (sender == txtusername && string.IsNullOrWhiteSpace(txtpassword.Text))
            {
                txtpassword.Focus();
                return;
            }

            btnlogin.PerformClick();
        }

        private void btnlogin_Click(object sender, EventArgs e)
        {
            if (signingIn)
            {
                return;
            }

            ClearError();

            bool lockEnabled = POS_System.Program.SettingsManager.GetBoolSetting("lock_system_after_failed_logins", true);
            int maxAttempts = POS_System.Program.SettingsManager.GetIntSetting("login_lock_attempts", 3);

            if (lockEnabled && failedLoginAttempts >= maxAttempts)
            {
                ShowLockedMessage();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtusername.Text) || string.IsNullOrEmpty(txtpassword.Text))
            {
                ShowError("Enter username and password.\r\nأدخل اسم المستخدم وكلمة المرور.");
                if (string.IsNullOrWhiteSpace(txtusername.Text))
                {
                    txtusername.Focus();
                }
                else
                {
                    txtpassword.Focus();
                }

                return;
            }

            User user;
            signingIn = true;
            SetBusy(true);
            try
            {
                string loadingMessage = IsArabic()
                    ? "جاري تسجيل الدخول..."
                    : "Signing in...";

                using (LoadingOverlayService.Show(this, loadingMessage))
                {
                    user = _userController.Login(txtusername.Text, txtpassword.Text);
                }
            }
            catch (Exception ex)
            {
                ShowError("Unable to sign in.\r\nتعذر تسجيل الدخول.\r\n" + ex.Message);
                return;
            }
            finally
            {
                signingIn = false;
                SetBusy(false);
            }

            if (user != null)
            {
                failedLoginAttempts = 0;
                if (string.Equals(user.Username, "admin", StringComparison.OrdinalIgnoreCase))
                {
                    user.Role = "admin";
                }

                LoggedInUserId = user.User_Id;
                LoggedInUsername = user.Username;
                AppSession.Set(user);
                OpenDashboard(user);
                return;
            }

            failedLoginAttempts++;

            if (lockEnabled && failedLoginAttempts >= maxAttempts)
            {
                ShowLockedMessage();
                return;
            }

            int remainingAttempts = Math.Max(maxAttempts - failedLoginAttempts, 0);
            string remaining = lockEnabled
                ? "\r\nRemaining attempts: " + remainingAttempts + "\r\nالمحاولات المتبقية: " + remainingAttempts
                : string.Empty;
            ShowError("Username or password is incorrect.\r\nاسم المستخدم أو كلمة المرور غير صحيحة!" + remaining);
            txtpassword.SelectAll();
            txtpassword.Focus();
        }

        private void OpenDashboard(User user)
        {
            openingDashboard = true;
            Hide();

            try
            {
                using (PosSystemDashboard dashboard = new PosSystemDashboard(user.Username, user.Role))
                {
                    dashboard.ShowDialog();
                }
            }
            finally
            {
                openingDashboard = false;
            }

            ResetForNextSignIn();
            if (IsDisposed)
            {
                return;
            }

            Show();
            Activate();
            if (txtusername != null && txtusername.CanFocus)
            {
                txtusername.Focus();
            }
        }

        private void ResetForNextSignIn()
        {
            AppSession.Clear();
            LoggedInUserId = 0;
            LoggedInUsername = null;
            txtpassword.Text = string.Empty;
            isPasswordVisible = false;
            txtpassword.PasswordChar = '*';
            btnTogglePassword.Invalidate();
            ClearError();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            txtpassword.PasswordChar = isPasswordVisible ? '\0' : '*';
            btnTogglePassword.AccessibleName = isPasswordVisible ? "Hide password" : "Show password";
            btnTogglePassword.Invalidate();
            txtpassword.Focus();
            txtpassword.SelectionStart = txtpassword.Text.Length;
        }

        private void btnTogglePassword_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color ink = btnTogglePassword.ForeColor.IsEmpty || btnTogglePassword.ForeColor == Color.Transparent
                ? Color.White
                : btnTogglePassword.ForeColor;

            using (Pen pen = new Pen(ink, 2F))
            using (SolidBrush brush = new SolidBrush(ink))
            {
                Rectangle eyeBounds = new Rectangle(7, 9, 18, 11);
                e.Graphics.DrawArc(pen, eyeBounds, 20, 140);
                e.Graphics.DrawArc(pen, eyeBounds, 200, 140);

                if (isPasswordVisible)
                {
                    e.Graphics.FillEllipse(brush, 14, 12, 4, 4);
                }
                else
                {
                    e.Graphics.DrawLine(pen, 7, 23, 26, 6);
                }
            }
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (openingDashboard)
            {
                e.Cancel = true;
                return;
            }

            if (e.CloseReason == CloseReason.ApplicationExitCall)
            {
                return;
            }

            Application.Exit();
        }

        private void SetBusy(bool busy)
        {
            txtusername.Enabled = !busy;
            txtpassword.Enabled = !busy;
            btnlogin.Enabled = !busy;
            btnTogglePassword.Enabled = !busy;
            btnExit.Enabled = !busy;
            UseWaitCursor = busy;
        }

        private void ShowLockedMessage()
        {
            string message = "The system is locked because the maximum failed login attempts has been reached.\r\nتم قفل النظام بعد تجاوز عدد المحاولات المسموح.";
            ShowError(message);
            MessageBox.Show(this, message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void ShowError(string message)
        {
            if (lblError == null)
            {
                return;
            }

            lblError.Text = message ?? string.Empty;
            lblError.ForeColor = PosPalette.Danger;
            lblError.Visible = true;
        }

        private void ClearError()
        {
            if (lblError == null)
            {
                return;
            }

            lblError.Text = string.Empty;
        }

        private static bool IsArabic()
        {
            return string.Equals(
                POS_System.Program.SettingsManager.GetSetting("language", "English"),
                "Arabic",
                StringComparison.OrdinalIgnoreCase);
        }
    }
}
