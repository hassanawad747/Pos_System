using Microsoft.EntityFrameworkCore;
using Pos_System.Controllers;
using Pos_System.Data;
using Pos_System.Forms;
using Pos_System.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Pos_System
{
    public partial class LoginForm : Form
    {
        private readonly UserController _userController;

        private readonly POSDbContext _context;

        public static string LoggedInUsername; // متغير عام

        public LoginForm()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            var optionsBuilder = new DbContextOptionsBuilder<POSDbContext>();
            optionsBuilder.UseSqlServer("Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;");
            // غيّر الـ Connection String حسب إعداداتك

            // إنشاء الـ DbContext وتمريره للـ UserController
            POSDbContext dbContext = new POSDbContext(optionsBuilder.Options);
            _userController = new UserController(dbContext);
        }


        public User Login(string username, string password)
        {
            return _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password_Hash == password);
        }


        public LoginForm(POSDbContext context)
        {
            _context = context;
            _userController = new UserController(_context);
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            this.FormClosing += LoginForm_FormClosing;
        }



        public static int LoggedInUserId; // متغير عام لتخزين الـ ID
        private static int failedLoginAttempts;

        private void btnlogin_Click(object sender, EventArgs e)
        {
            bool lockEnabled = POS_System.Program.SettingsManager.GetBoolSetting("lock_system_after_failed_logins", true);
            int maxAttempts = POS_System.Program.SettingsManager.GetIntSetting("login_lock_attempts", 3);

            if (lockEnabled && failedLoginAttempts >= maxAttempts)
            {
                MessageBox.Show("The system is locked because the maximum failed login attempts has been reached.");
                return;
            }

            var user = _userController.Login(txtusername.Text, txtpassword.Text);

            if (user != null)
            {
                failedLoginAttempts = 0;
                // خزّن الـ user_id
                LoggedInUserId = user.User_Id;   // تأكد أن خاصية اسمها User_Id أو user_id في الموديل

                LoggedInUsername = user.Username; // خزّن اسم المستخدم لو محتاجه

                this.Hide();
                string username = user.Username;
                string role = user.Role;
                PosSystemDashboard dashboard = new PosSystemDashboard(username, role);
                dashboard.ShowDialog();
                this.Close();
            }
            else
            {
                failedLoginAttempts++;

                if (lockEnabled && failedLoginAttempts >= maxAttempts)
                {
                    MessageBox.Show("Login failed. The system is now locked because the maximum number of attempts was reached.");
                    return;
                }

                int remainingAttempts = Math.Max(maxAttempts - failedLoginAttempts, 0);
                MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة!" +
                    (lockEnabled ? $"\nRemaining attempts: {remainingAttempts}" : string.Empty));
            }
        }



        // When user clicks X
        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit(); // ensures app fully closes
        }
    }
}
