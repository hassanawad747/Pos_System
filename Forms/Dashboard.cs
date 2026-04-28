using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class Dashboard : Form
    {
        private void button1_Click(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Products prd = new Products();
            prd.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Customers prd = new Customers();
            prd.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            addsalesform prd = new addsalesform();
            prd.ShowDialog();
        }

        private void btnearningreport_Click(object sender, EventArgs e)
        {
            EarningReports earningReports = new EarningReports();
            earningReports.ShowDialog();

        }

        private void btnwarhouse_Click(object sender, EventArgs e)
        {
            WarhouseReports warhouse = new WarhouseReports();
            warhouse.ShowDialog();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void panelheader_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            
            //Sales salesForm = new Sales(_username , _role);
            //salesForm.ShowDialog();
        }

        private string _username;

        public Dashboard(string username)
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            _username = username;
            label3.Text = username;//| Shift: Morning Shift | " + DateTime.Now.ToString("dd MMM yyyy");
            label8.Text = DateTime.Now.ToString("yyyy-MM-dd");
            labeldate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }





        }

    }
