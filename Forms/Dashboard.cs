using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class Dashboard : Form
    {
        private Label lblUserInfo;
        private Label lblSales, lblTransactions, lblItems, lblReturns;

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
            LoginForm loginForm = new LoginForm();
            loginForm.ShowDialog();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Products prd = new Products();
            prd.ShowDialog();
            this.Hide();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Customers prd = new Customers();
            prd.ShowDialog();
            this.Hide();
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            
            this.Close();
            Sales salesForm = new Sales(_username);
            salesForm.ShowDialog();
            this.Hide();
        }

        private string _username;

        public Dashboard(string username)
        {
            InitializeComponent();
            _username = username;
            label3.Text = username;//| Shift: Morning Shift | " + DateTime.Now.ToString("dd MMM yyyy");
            label8.Text = DateTime.Now.ToString("yyyy-MM-dd");
            labeldate.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }





            //try
            //{
            //    BuildDashboardLayout();   // build UI once
            //    LoadDashboardData();      // load data from DB
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Error loading dashboard: " + ex.Message);
            //}
        }

        //    private void BuildDashboardLayout()
        //    {
        //        // Header
        //        Panel panelHeader = new Panel
        //        {
        //            Dock = DockStyle.Top,
        //            Height = 60,
        //            BackColor = Color.SteelBlue
        //        };
        //        Label lblTitle = new Label
        //        {
        //            Text = "POS SYSTEM - DASHBOARD",
        //            Font = new Font("Segoe UI", 16, FontStyle.Bold),
        //            ForeColor = Color.White,
        //            Location = new Point(10, 15)
        //        };
        //        lblUserInfo = new Label
        //        {
        //            Text = "Welcome, Admin | Shift: Morning Shift | " + DateTime.Now.ToString("dd MMM yyyy"),
        //            ForeColor = Color.White,
        //            Location = new Point(300, 20)
        //        };
        //        Button btnLogout = new Button
        //        {
        //            Text = "Logout",
        //            Location = new Point(700, 15),
        //            BackColor = Color.DarkRed,
        //            ForeColor = Color.White
        //        };
        //        btnLogout.Click += BtnLogout_Click; // navigation event
        //        panelHeader.Controls.AddRange(new Control[] { lblTitle, lblUserInfo, btnLogout });
        //        this.Controls.Add(panelHeader);

        //        // Sales Summary
        //        Panel panelSummary = new Panel
        //        {
        //            Dock = DockStyle.Top,
        //            Height = 120
        //        };
        //        lblSales = CreateSummaryPanel("Today's Sales", "$0.00", Color.MediumSeaGreen, new Point(10, 10));
        //        lblTransactions = CreateSummaryPanel("Total Transactions", "0 Orders", Color.Orange, new Point(190, 10));
        //        lblItems = CreateSummaryPanel("Items Sold", "0 Items", Color.SkyBlue, new Point(370, 10));
        //        lblReturns = CreateSummaryPanel("Returns", "0 Returns", Color.MediumPurple, new Point(550, 10));

        //        panelSummary.Controls.AddRange(new Control[] { lblSales.Parent, lblTransactions.Parent, lblItems.Parent, lblReturns.Parent });
        //        this.Controls.Add(panelSummary);

        //        // Shift Info
        //        GroupBox groupShift = new GroupBox
        //        {
        //            Text = "Shift Information",
        //            Location = new Point(10, 150),
        //            Size = new Size(300, 120)
        //        };
        //        groupShift.Controls.Add(new Label { Text = "Shift: Morning Shift", Location = new Point(10, 30) });
        //        groupShift.Controls.Add(new Label { Text = "Cash in Drawer: $500.00", Location = new Point(10, 60) });
        //        groupShift.Controls.Add(new Label { Text = "Shift Started: 8:00 AM", Location = new Point(10, 90) });
        //        this.Controls.Add(groupShift);

        //        // Notifications
        //        GroupBox groupNotifications = new GroupBox
        //        {
        //            Text = "Notifications",
        //            Location = new Point(320, 150),
        //            Size = new Size(400, 120)
        //        };
        //        ListBox listNotifications = new ListBox { Dock = DockStyle.Fill };
        //        listNotifications.Items.Add("⚠ Low Stock: Item 'Soda' is low!");
        //        listNotifications.Items.Add("🔔 Reminder: Complete end of day report");
        //        listNotifications.Items.Add("👤 New Customer Registered: John Smith");
        //        groupNotifications.Controls.Add(listNotifications);
        //        this.Controls.Add(groupNotifications);

        //        // Quick Links
        //        Button btnSale = new Button { Text = "New Sale", Size = new Size(120, 60), Location = new Point(10, 300) };
        //        btnSale.Click += BtnSale_Click;

        //        Button btnInventory = new Button { Text = "Inventory", Size = new Size(120, 60), Location = new Point(140, 300) };
        //        btnInventory.Click += BtnInventory_Click;

        //        Button btnReports = new Button { Text = "Reports", Size = new Size(120, 60), Location = new Point(270, 300) };
        //        btnReports.Click += BtnReports_Click;

        //        Button btnUsers = new Button { Text = "Manage Users", Size = new Size(120, 60), Location = new Point(400, 300) };
        //        btnUsers.Click += BtnUsers_Click;

        //        this.Controls.AddRange(new Control[] { btnSale, btnInventory, btnReports, btnUsers });
        //    }

        //    // Helper method returns label for value so you can update later
        //    private Label CreateSummaryPanel(string title, string value, Color backColor, Point location)
        //    {
        //        Panel p = new Panel
        //        {
        //            Size = new Size(160, 100),
        //            BackColor = backColor,
        //            Location = location
        //        };
        //        Label lblTitle = new Label { Text = title, ForeColor = Color.White, Location = new Point(10, 10) };
        //        Label lblValue = new Label { Text = value, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(10, 40) };
        //        p.Controls.AddRange(new Control[] { lblTitle, lblValue });
        //        this.Controls.Add(p);
        //        return lblValue; // return the value label so we can update it later
        //    }

        //    private void LoadDashboardData()
        //    {
        //        string connStr = @"Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";
        //        using (SqlConnection conn = new SqlConnection(connStr))
        //        {
        //            conn.Open();

        //            SqlCommand cmdSales = new SqlCommand(
        //                "SELECT ISNULL(SUM(total_amount),0) FROM Sales WHERE CAST(sale_date AS DATE) = CAST(GETDATE() AS DATE)", conn);
        //            decimal todaySales = (decimal)cmdSales.ExecuteScalar();
        //            lblSales.Text = todaySales.ToString("C");

        //            SqlCommand cmdTrans = new SqlCommand(
        //                "SELECT COUNT(*) FROM Sales WHERE CAST(sale_date AS DATE) = CAST(GETDATE() AS DATE)", conn);
        //            int transCount = (int)cmdTrans.ExecuteScalar();
        //            lblTransactions.Text = transCount + " Orders";

        //            SqlCommand cmdItems = new SqlCommand(
        //                "SELECT ISNULL(SUM(quantity),0) FROM Sale_Items WHERE CAST(sale_date AS DATE) = CAST(GETDATE() AS DATE)", conn);
        //            int itemsSold = (int)cmdItems.ExecuteScalar();
        //            lblItems.Text = itemsSold + " Items";

        //            SqlCommand cmdReturns = new SqlCommand(
        //                "SELECT COUNT(*) FROM Returns WHERE CAST(return_date AS DATE) = CAST(GETDATE() AS DATE)", conn);
        //            int returnsCount = (int)cmdReturns.ExecuteScalar();
        //            lblReturns.Text = returnsCount + " Returns";
        //        }
        //    }

        //    // Navigation events
        //    private void BtnLogout_Click(object sender, EventArgs e)
        //    {
        //        this.Hide();
        //        LoginForm login = new LoginForm();
        //        login.Show();
        //    }

        //    private void BtnSale_Click(object sender, EventArgs e)
        //    {
        //        this.Hide();
        //        //SaleForm sale = new SaleForm();
        //      //  sale.Show();
        //    }

        //    private void BtnInventory_Click(object sender, EventArgs e)
        //    {
        //        this.Hide();
        //       // InventoryForm inv = new InventoryForm();
        //        //inv.Show();
        //    }

        //    private void BtnReports_Click(object sender, EventArgs e)
        //    {
        //        this.Hide();
        //        //ReportsForm rep = new ReportsForm();
        //        //rep.Show();
        //    }

        //    private void BtnUsers_Click(object sender, EventArgs e)
        //    {
        //        this.Hide();
        //       // ManageUsersForm users = new ManageUsersForm();
        //        //users.Show();
        //    }
        //}
    }
