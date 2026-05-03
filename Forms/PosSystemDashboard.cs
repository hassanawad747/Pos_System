using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Pos_System.Services;
using static POS_System.Program;

namespace Pos_System.Forms
{
    public partial class PosSystemDashboard : Form
    {
        private string _username;
        private string _role;
        private Timer sessionTimer;
        private UserActivityMessageFilter activityFilter;
        private bool isLoggingOut;
        private Button notificationButton;
        private Label notificationBadge;
        private Button activeMenuButton;




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
            BuildNotificationControl();
            ApplyDashboardVisualStyle();
            ApplyRoleAccess();
            UpdateNotificationBadge();

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
            ActivateMenuButton(btnsales);
            LoadForm(new Sales(lbusername.Text, _role));
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnProducts);
            LoadForm(new InventoryPageForm());
        }

        private void btnAddSales_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnAddSales);
            LoadForm(new ProductsPageForm());
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnCustomers);
            LoadForm(new CustomersPageForm());
        }

        private void btnEarningReports_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnEarningReports);
            LoadForm(new ActivityLogForm());
        }

        private void btnWarhouseReports_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnWarhouseReports);
            LoadForm(new SuppliersPageForm());
        }

        private void LoadForm(Form form)
        {
            for (int i = panelContent.Controls.Count - 1; i >= 0; i--)
            {
                Control control = panelContent.Controls[i];
                if (control != panelheader)
                {
                    panelContent.Controls.RemoveAt(i);
                    control.Dispose();
                }
            }

            POS_System.Program.SettingsManager.RegisterForm(form);
            form.FormClosed += LoadedForm_FormClosed;
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            panelContent.Controls.Add(form);
            panelContent.Controls.SetChildIndex(panelheader, 0);
            form.Show();
        }

        private void LoadedForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UpdateNotificationBadge();
        }

        private void btnpossystemdashboard_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnpossystemdashboard);
            ShowDashboardHome();
        }

        private void ShowDashboardHome()
        {
            LoadForm(new AdminDashboardHomeForm());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LogoutToLogin(); // Hide the dashboard
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(button8);
            LoadForm(new ReportsOverviewPageForm());
        }

        private void btnAddUsers_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnAddUsers);
            LoadForm(new AddUsers());
        }

        private void PosSystemDashboard_Load(object sender, EventArgs e)
        {
            UpdateNotificationBadge();
            if (AppSession.IsCashier)
            {
                ActivateMenuButton(btnsales);
                LoadForm(new Sales(lbusername.Text, _role));
            }
            else
            {
                ActivateMenuButton(btnpossystemdashboard);
                ShowDashboardHome();
            }

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

        private void ApplyRoleAccess()
        {
            bool isCashier = string.Equals(_role, "cashier", StringComparison.OrdinalIgnoreCase);

            Button[] restrictedButtons =
            {
                btnEarningReports,
                btnProducts,
                btnCustomers,
                btnWarhouseReports,
                button11,
                button12,
                button8,
                btnAddUsers,
                btnsettings
            };

            foreach (Button button in restrictedButtons)
            {
                button.Enabled = !isCashier;
                button.Visible = !isCashier;
                
            }

            btnpossystemdashboard.Enabled = true;
            btnsales.Enabled = true;
            btnAddSales.Enabled = !isCashier;
            btnAddSales.Visible = !isCashier;
            btnpossystemdashboard.Enabled = !isCashier;
            btnpossystemdashboard.Visible = !isCashier;
        }

        private void ApplyDashboardVisualStyle()
        {
            BackColor = Color.FromArgb(244, 247, 252);
            panel1.BackColor = Color.FromArgb(15, 23, 42);
            panelheader.BackColor = Color.White;
            panelContent.BackColor = Color.FromArgb(244, 247, 252);
            label11.Text = "Dashboard";
            label11.ForeColor = Color.FromArgb(15, 23, 42);
            label10.ForeColor = Color.FromArgb(100, 116, 139);
            lbuser1.ForeColor = Color.FromArgb(37, 99, 235);
            lbdatetime.ForeColor = Color.FromArgb(100, 116, 139);
            label5.ForeColor = Color.FromArgb(100, 116, 139);
            label6.ForeColor = Color.FromArgb(15, 23, 42);
            label10.Text = "Welcome,";
            label11.Text = "Dashboard";
            label6.Text = "Admin Panel";
            label5.Text = "Role:";
            label1.Text = "POS SYSTEM";
            btnpossystemdashboard.Text = "Dashboard";
            btnsales.Text = "Sales";
            btnCustomers.Text = "Customers";
            btnAddSales.Text = "Products";
            btnProducts.Text = "Inventory";
            btnWarhouseReports.Text = "Suppliers";
            button11.Text = "Purchases";
            button12.Text = "Expenses";
            button8.Text = "Reports";
            btnAddUsers.Text = "Add Users";
            btnEarningReports.Text = "Activity Log";
            btnsettings.Text = "Settings";
            panel3.Visible = false;
            panel2.Visible = false;
            panelheader.Height = 74;

            foreach (Control control in panel1.Controls)
            {
                if (control is Button menuButton && menuButton != button1)
                {
                    menuButton.FlatStyle = FlatStyle.Flat;
                    menuButton.FlatAppearance.BorderSize = 0;
                    menuButton.Height = 42;
                    menuButton.BackColor = Color.Transparent;
                    menuButton.ForeColor = Color.White;
                    menuButton.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
                    menuButton.TextAlign = ContentAlignment.MiddleLeft;
                    menuButton.Padding = new Padding(14, 0, 0, 0);
                }
            }

            button1.FlatAppearance.BorderSize = 0;
            button1.BackColor = Color.FromArgb(30, 41, 59);
            button1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        }

        private void ActivateMenuButton(Button button)
        {
            if (activeMenuButton != null && activeMenuButton != button)
            {
                activeMenuButton.BackColor = Color.Transparent;
                activeMenuButton.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular);
            }

            activeMenuButton = button;
            activeMenuButton.BackColor = Color.FromArgb(37, 99, 235);
            activeMenuButton.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        }

        private void BuildNotificationControl()
        {
            if (notificationButton != null)
            {
                return;
            }

            notificationButton = new Button
            {
                Size = new Size(44, 44),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(239, 246, 255),
                ForeColor = Color.FromArgb(30, 64, 175),
                Font = new Font("Segoe UI Symbol", 16F, FontStyle.Bold),
                Text = "\uD83D\uDD14",
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(panelheader.Width - 180, 14),
                Cursor = Cursors.Hand
            };

            notificationButton.FlatAppearance.BorderSize = 0;
            notificationButton.Click += NotificationButton_Click;

            notificationBadge = new Label
            {
                AutoSize = false,
                Size = new Size(24, 24),
                BackColor = Color.FromArgb(239, 68, 68),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(notificationButton.Right - 12, notificationButton.Top - 6)
            };

            panelheader.Controls.Add(notificationButton);
            panelheader.Controls.Add(notificationBadge);
            panelheader.Resize += Panelheader_Resize;
            Panelheader_Resize(panelheader, EventArgs.Empty);
        }

        private void Panelheader_Resize(object sender, EventArgs e)
        {
            if (notificationButton == null || notificationBadge == null)
            {
                return;
            }

            notificationButton.Location = new Point(panelheader.Width - 170, 14);
            notificationBadge.Location = new Point(notificationButton.Right - 10, notificationButton.Top - 5);
        }

        private void NotificationButton_Click(object sender, EventArgs e)
        {
            using (ActivityLogForm form = new ActivityLogForm())
            {
                form.ShowDialog(this);
            }

            UpdateNotificationBadge();
        }

        private void UpdateNotificationBadge()
        {
            if (notificationBadge == null)
            {
                return;
            }

            int activityCount = AuditService.GetTodayActivityCount();
            notificationBadge.Text = activityCount > 99 ? "99+" : activityCount.ToString();
            notificationBadge.Visible = activityCount > 0;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnsettings);
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
            AppSession.Clear();

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

        private void button11_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(button11);
            LoadForm(new SimpleTablePageForm(
                "Purchases",
                "Purchase management screen",
                "Purchases",
                "The Purchases table does not exist yet. Create it when you are ready to save supplier purchase invoices."));
        }

        private void button12_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(button12);
            LoadForm(new SimpleTablePageForm(
                "Expenses",
                "Expense management screen",
                "Expenses",
                "The Expenses table does not exist yet. Create it when you are ready to save operating expenses."));
        }
    }
    }


