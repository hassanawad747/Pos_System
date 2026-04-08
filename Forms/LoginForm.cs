using Pos_System.Controllers;
using Pos_System.Data;
using Pos_System.Forms;
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
        }

        public LoginForm(POSDbContext context)
        {
            _context = context;
            _userController = new UserController(_context);
            InitializeComponent();
        }

        

        private void btnlogin_Click(object sender, EventArgs e)
        {
            var user = _userController.Login(txtusername.Text, txtpassword.Text);

            if (user != null)
            {
                // Pass role into MainForm
                //user.Role
                LoggedInUsername = txtusername.Text; // خزّن اسم المستخدم
                string username = txtusername.Text;
                Dashboard dashboard = new Dashboard(username);
                dashboard.ShowDialog();
                this.Hide();


            }
            else
            {
                MessageBox.Show("Invalid login!");
            }


        }
    }
}
