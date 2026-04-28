using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static POS_System.Program;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Pos_System.Forms
{
    public partial class PosSystemDashboard : Form
    {
        private string _username;
        private string _role;
        private Timer sessionTimer;
        private UserActivityMessageFilter activityFilter;
        private bool isLoggingOut;




        public PosSystemDashboard(string username, string role)
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            lbusername.Text = $"{username}";
            _username = username;
            lbuser1.Text = username;//| Shift: Morning Shift | " + DateTime.Now.ToString("dd MMM yyyy");
                                    // label8.Text = DateTime.Now.ToString("yyyy-MM-dd");
            lbdatetime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _role = role;
            lbrole.Text = role; // هنا نعرض الدور

            // Assign icons to PictureBoxes
            ////////////////////////
            ///bas 5ales sho8ol bsha8el hayda code ta 7ot sowar be 2lb kel button w 2lb kel button 3ala 7asab el esm taba3o 
            ////////////////////////
            //picSales.Image = Properties.Resources.cart;
            //picPurchases.Image = Properties.Resources.purchase;
            //picInventory.Image = Properties.Resources.warehouse;
            //picProfit.Image = Properties.Resources.report;
            //picWarehouse.Image = Properties.Resources.storage;
            //picUsers.Image = Properties.Resources.users;
            //picCategories.Image = Properties.Resources.categories;

            ConfigureSessionTimeout();
        }
        private void btnsales_Click(object sender, EventArgs e)
        {
            LoadForm(new Sales(lbusername.Text , _role)); // pass username if needed
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            LoadForm(new Products());
        }

        private void btnAddSales_Click(object sender, EventArgs e)
        {
            LoadForm(new addsalesform());
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            LoadForm(new Customers());
        }

        private void btnEarningReports_Click(object sender, EventArgs e)
        {
            LoadForm(new EarningReports());
        }

        private void btnWarhouseReports_Click(object sender, EventArgs e)
        {
            LoadForm(new WarhouseReports());
        }

        private void LoadForm(Form form)
        {
            panelContent.Controls.Clear();   // clear old form
            POS_System.Program.SettingsManager.RegisterForm(form);
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            panelContent.Controls.Add(form);
            form.Show();
        }

        private void btnpossystemdashboard_Click(object sender, EventArgs e)
        {

            this.Hide(); // Hide the current dashboard
            // ShowDashboardHome();

            // Clear any loaded form from the content panel
            panelContent.Controls.Clear();

            // Recreate the dashboard home view
            PosSystemDashboard home = new PosSystemDashboard(lbusername.Text, lbrole.Text);
            home.ShowDialog();

            // Close the current instance if you want only one dashboard open
            this.Close();
        }

        private void ShowDashboardHome()
        {
            panelContent.Controls.Clear(); // remove any loaded form

            // Optionally, add a welcome label or picture back into panelContent
            Label lblWelcome = new Label
            {
                Text = "مرحباً بك في لوحة التحكم",
                Dock = DockStyle.Fill,
                Font = new Font("Tahoma", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelContent.Controls.Add(lblWelcome);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LogoutToLogin(); // Hide the dashboard
        }

        private void button8_Click(object sender, EventArgs e)
        {
            LoadForm(new ReportsForm());
        }

        private void btnAddUsers_Click(object sender, EventArgs e)
        {
            LoadForm(new AddUsers());
        }

        private void PosSystemDashboard_Load(object sender, EventArgs e)
        {

        //    // Apply theme
        //    string theme = SettingsManager.GetSetting("theme", "light");
        //    if (theme == "dark")
        //    {
        //        this.BackColor = Color.FromArgb(45, 45, 48);
        //        foreach (Control ctrl in this.Controls)
        //        {
        //            ctrl.ForeColor = Color.White;
        //        }
        //    }
        //    else
        //    {
        //        this.BackColor = SystemColors.Control;
        //        foreach (Control ctrl in this.Controls)
        //        {
        //            ctrl.ForeColor = Color.Black;
        //        }
        //        //}

        //        //// Apply currency
        //        //string currency = SettingsManager.GetSetting("currency", "USD");
        //        //lblCurrency.Text = "Currency: " + currency;

        //        // Apply language
        //        string language = SettingsManager.GetSetting("language", "English");
        //        if (language == "Arabic")
        //        {
        //            this.RightToLeft = RightToLeft.Yes;
        //            this.RightToLeftLayout = true;
        //        }
        //        else
        //        {
        //            this.RightToLeft = RightToLeft.No;
        //            this.RightToLeftLayout = false;
        //        }

        //        //// Apply tax percentage
        //        //string tax = SettingsManager.GetSetting("tax_percentage", "10");
        //        //lblTax.Text = "Tax: " + tax + "%";
           }

        private void button10_Click(object sender, EventArgs e)
        {
            LoadForm(new Settings());
        }

        private void lbrole_Click(object sender, EventArgs e)
        {
             
        }

        private void ConfigureSessionTimeout()
        {
            int timeoutMinutes = POS_System.Program.SettingsManager.GetIntSetting("session_timeout_minutes", 30);
            if (timeoutMinutes <= 0)
            {
                timeoutMinutes = 30;
            }

            if (sessionTimer == null)
            {
                sessionTimer = new Timer();
                sessionTimer.Tick += SessionTimer_Tick;
            }

            sessionTimer.Interval = timeoutMinutes * 60 * 1000;
            sessionTimer.Stop();
            sessionTimer.Start();

            if (activityFilter == null)
            {
                activityFilter = new UserActivityMessageFilter(ResetSessionTimeout);
                Application.AddMessageFilter(activityFilter);
            }

            this.FormClosed -= PosSystemDashboard_FormClosed;
            this.FormClosed += PosSystemDashboard_FormClosed;
        }

        private void ResetSessionTimeout()
        {
            if (sessionTimer == null || isLoggingOut)
            {
                return;
            }

            sessionTimer.Stop();
            sessionTimer.Start();
        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            sessionTimer.Stop();
            MessageBox.Show("Your session expired because of inactivity. Please log in again.");
            LogoutToLogin();
        }

        private void LogoutToLogin()
        {
            if (isLoggingOut)
            {
                return;
            }

            isLoggingOut = true;
            CleanupSessionTracking();

            Hide();
            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();
            Close();
        }

        private void PosSystemDashboard_FormClosed(object sender, FormClosedEventArgs e)
        {
            CleanupSessionTracking();
        }

        private void CleanupSessionTracking()
        {
            if (sessionTimer != null)
            {
                sessionTimer.Stop();
            }

            if (activityFilter != null)
            {
                Application.RemoveMessageFilter(activityFilter);
                activityFilter = null;
            }
        }

        private sealed class UserActivityMessageFilter : IMessageFilter
        {
            private readonly Action onActivity;

            public UserActivityMessageFilter(Action onActivity)
            {
                this.onActivity = onActivity;
            }

            public bool PreFilterMessage(ref Message m)
            {
                switch (m.Msg)
                {
                    case 0x0100:
                    case 0x0101:
                    case 0x0200:
                    case 0x0201:
                    case 0x0202:
                    case 0x0204:
                    case 0x0205:
                    case 0x0207:
                    case 0x0208:
                    case 0x020A:
                        onActivity?.Invoke();
                        break;
                }

                return false;
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
    }


