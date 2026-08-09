using Microsoft.EntityFrameworkCore;
using Pos_System.Controllers;
using Pos_System.Data;
using Pos_System.Forms;
using Pos_System.Services;
using System;
using System.Drawing;
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

        public LoginForm()
        {
            InitializeComponent();
            ModernUiService.EnableGlobalTheme();
            ConfigureLoginExperience();
            POS_System.Program.SettingsManager.RegisterForm(this);

            var optionsBuilder = new DbContextOptionsBuilder<POSDbContext>();
            optionsBuilder.UseSqlServer(POS_System.Program.SettingsManager.ConnectionString);
            _context = new POSDbContext(optionsBuilder.Options);
            _userController = new UserController(_context);
        }

        public LoginForm(POSDbContext context)
        {
            _context = context;
            _userController = new UserController(_context);
            InitializeComponent();
            ModernUiService.EnableGlobalTheme();
            ConfigureLoginExperience();
            POS_System.Program.SettingsManager.RegisterForm(this);
            FormClosing += LoginForm_FormClosing;
        }

        private void ConfigureLoginExperience()
        {
            AcceptButton = btnlogin;
            KeyPreview = true;
            txtusername.TabIndex = 0;
            passwordPanel.TabIndex = 1;
            passwordPanel.TabStop = false;
            txtpassword.TabIndex = 0;
            btnlogin.TabIndex = 2;
            txtusername.KeyDown += LoginTextBox_KeyDown;
            txtpassword.KeyDown += LoginTextBox_KeyDown;
            KeyDown += LoginTextBox_KeyDown;

            btnlogin.MouseEnter += (s, e) => btnlogin.BackColor = Color.FromArgb(29, 78, 216);
            btnlogin.MouseLeave += (s, e) => btnlogin.BackColor = Color.FromArgb(37, 99, 235);
            txtusername.GotFocus += (s, e) => txtusername.BackColor = Color.FromArgb(248, 250, 252);
            txtusername.LostFocus += (s, e) => txtusername.BackColor = Color.White;
            txtpassword.GotFocus += (s, e) => passwordPanel.BackColor = Color.FromArgb(248, 250, 252);
            txtpassword.LostFocus += (s, e) => passwordPanel.BackColor = Color.White;

            Shown += (s, e) =>
            {
                txtusername.Focus();
                lblConnectionStatus.Text = "● Database connection ready";
                lblConnectionStatus.ForeColor = Color.FromArgb(22, 163, 74);
                BeginInvoke(new Action(OfferFirstAdministratorSetup));
            };
        }

        private void LoginTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
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
            bool lockEnabled = POS_System.Program.SettingsManager.GetBoolSetting("lock_system_after_failed_logins", true);
            int maxAttempts = POS_System.Program.SettingsManager.GetIntSetting("login_lock_attempts", 3);

            if (string.IsNullOrWhiteSpace(txtusername.Text) || string.IsNullOrWhiteSpace(txtpassword.Text))
            {
                MessageBox.Show("Enter both username and password.", "Sign in", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (lockEnabled && failedLoginAttempts >= maxAttempts)
            {
                MessageBox.Show("The system is locked because the maximum failed login attempts has been reached.");
                return;
            }

            string oldText = btnlogin.Text;
            btnlogin.Enabled = false;
            btnlogin.Text = "Signing in...";
            Cursor oldCursor = Cursor;
            Cursor = Cursors.WaitCursor;

            try
            {
                var user = _userController.Login(txtusername.Text.Trim(), txtpassword.Text);

                if (user != null)
                {
                    failedLoginAttempts = 0;
                    if (string.Equals(user.Username, "admin", StringComparison.OrdinalIgnoreCase))
                        user.Role = "admin";

                    LoggedInUserId = user.User_Id;
                    LoggedInUsername = user.Username;
                    AppSession.Set(user);

                    Hide();
                    PosSystemDashboard dashboard = new PosSystemDashboard(user.Username, user.Role);
                    dashboard.ShowDialog();
                    Close();
                    return;
                }

                failedLoginAttempts++;
                txtpassword.Clear();
                txtpassword.Focus();

                if (lockEnabled && failedLoginAttempts >= maxAttempts)
                {
                    MessageBox.Show("Login failed. The system is now locked because the maximum number of attempts was reached.");
                    return;
                }

                int remainingAttempts = Math.Max(maxAttempts - failedLoginAttempts, 0);
                MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة!" +
                    (lockEnabled ? $"\nRemaining attempts: {remainingAttempts}" : string.Empty),
                    "Sign in failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                if (!IsDisposed)
                {
                    btnlogin.Text = oldText;
                    btnlogin.Enabled = true;
                    Cursor = oldCursor;
                }
            }
        }

        private void btnTogglePassword_Click(object sender, EventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            txtpassword.PasswordChar = isPasswordVisible ? '\0' : '●';
            btnTogglePassword.Invalidate();
            txtpassword.Focus();
            txtpassword.SelectionStart = txtpassword.Text.Length;
        }

        private void btnTogglePassword_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(Color.FromArgb(71, 85, 105), 2F))
            using (SolidBrush brush = new SolidBrush(Color.FromArgb(71, 85, 105)))
            {
                Rectangle eyeBounds = new Rectangle(7, 9, 18, 11);
                e.Graphics.DrawArc(pen, eyeBounds, 20, 140);
                e.Graphics.DrawArc(pen, eyeBounds, 200, 140);
                if (isPasswordVisible)
                    e.Graphics.FillEllipse(brush, 14, 12, 4, 4);
                else
                    e.Graphics.DrawLine(pen, 7, 23, 26, 6);
            }
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
