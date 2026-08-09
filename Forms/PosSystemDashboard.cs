using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Pos_System.Services;
using static POS_System.Program;

namespace Pos_System.Forms
{
    public partial class PosSystemDashboard : Form
    {
        private string _username;
        private string _role;
        private Timer sessionTimer;
        private Timer clockTimer;
        private UserActivityMessageFilter activityFilter;
        private bool isLoggingOut;
        private List<Control> dashboardHomeControls;
        private Button endWorkDayButton;
        private Button auditNotificationButton;
        private Panel auditPanel;
        private DataGridView auditGrid;
        private Button deleteAuditButton;
        private Button closeAuditButton;
        private Label auditBadgeLabel;
        private TextBox auditUserSearchTextBox;
        private TextBox auditActionSearchTextBox;
        private DateTimePicker auditDateSearchPicker;
        private Button searchAuditButton;
        private Button clearAuditSearchButton;
        private Form activeChildForm;
        private Panel activeFormHostPanel;
        private MenuStrip dashboardMenuStrip;
        private ToolStripButton menuDashboard;
        private ToolStripButton menuSales;
        private ToolStripButton menuProducts;
        private ToolStripButton menuSupplier;
        private ToolStripButton menuCustomers;
        private ToolStripButton menuEarningReports;
        private ToolStripButton menuWarhouseReports;
        private ToolStripButton menuReports;
        private ToolStripButton menuAddUsers;
        private ToolStripButton menuSettings;
        private ToolStripButton menuSave;
        private ToolStripButton menuSaveAs;
        private ToolStripButton menuEdit;
        private ToolStripButton menuDelete;
        private ToolStripButton menuPrint;
        private ToolStripButton menuLogout;
        private Button activeMenuButton;




        public PosSystemDashboard(string username, string role)
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            dashboardHomeControls = panelContent.Controls.Cast<Control>().ToList();
            lbusername.Text = $"{username}";
            _username = username;
            lbuser1.Text = username;//| Shift: Morning Shift | " + DateTime.Now.ToString("dd MMM yyyy");
                                    // label8.Text = DateTime.Now.ToString("yyyy-MM-dd");
            lbdatetime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            _role = string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase) ? "admin" : role;
            lbrole.Text = _role; // هنا نعرض الدور
            ApplyDashboardVisualStyle();
            ApplyRoleAccess();
            RefreshAuditNotification();

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
            ConfigureDashboardShell();
            ApplyRolePermissions();
            LayoutDashboardHome();
        }
        private void btnsales_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnsales);
            SetWorkspaceTitle("Sales");
            OpenSalesWorkspace();
        }

        private void StartSalesWork_Click(object sender, EventArgs e)
        {
            try
            {
                WorkHistoryService.StartWork(LoginForm.LoggedInUserId, _username);
                LoadSalesForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start sales work: " + ex.Message, "Start Work");
            }
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenProducts))
            {
                MessageBox.Show("You do not have permission to open products.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateMenuButton(btnProducts);
            SetWorkspaceTitle("Inventory");
            LoadForm(new InventoryPageForm());
        }

        private void btnAddSales_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Add Sales form is disabled.", "Disabled", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenCustomers))
            {
                MessageBox.Show("You do not have permission to open customers.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateMenuButton(btnCustomers);
            SetWorkspaceTitle("Customers");
            LoadForm(new CustomersPageForm());
        }

        private void menuSupplier_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenSuppliers))
            {
                MessageBox.Show("You do not have permission to open suppliers.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SetWorkspaceTitle("Suppliers");
            LoadForm(new SupplierBalanceForm());
        }

        private void btnEarningReports_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenEarningReports))
            {
                MessageBox.Show("You do not have permission to open earning reports.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateMenuButton(btnEarningReports);
            SetWorkspaceTitle("Earning Report");
            LoadForm(new EarningReports());
        }

        private void btnWarhouseReports_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenWarehouse))
            {
                MessageBox.Show("You do not have permission to open warehouse.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateMenuButton(btnWarhouseReports);
            SetWorkspaceTitle("Warehouse Report");
            LoadForm(new WarhouseReports());
        }

        private void LoadForm(Form form)
        {
            activeChildForm = form;
            panelContent.SuspendLayout();
            panelContent.Controls.Clear();

            activeFormHostPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(244, 247, 252)
            };

            panelContent.Controls.Add(activeFormHostPanel);
            panelContent.Controls.Add(panelheader);
            panelContent.Controls.Add(dashboardMenuStrip);
            LayoutDashboardHeaderAndMenu();
            LayoutActiveChildForm();
            activeFormHostPanel.Resize += ActiveFormHostPanel_Resize;

            form.TopLevel = false;
            POS_System.Program.SettingsManager.RegisterForm(form);
            form.FormClosed += LoadedForm_FormClosed;
            form.FormBorderStyle = FormBorderStyle.None;
            form.WindowState = FormWindowState.Normal;
            form.Dock = DockStyle.Fill;
            activeFormHostPanel.Controls.Add(form);
            HideChildFormHeaders(form);
            form.Bounds = activeFormHostPanel.ClientRectangle;
            form.Show();
            BringDashboardHeaderToFront();
            panelContent.ResumeLayout();
        }

        private void HideChildFormHeaders(Form form)
        {
            foreach (Control control in GetAllControls(form))
            {
                if (control.Name.Equals("panelheader", StringComparison.OrdinalIgnoreCase))
                {
                    control.Visible = false;
                }
            }
        }

        private void ActiveFormHostPanel_Resize(object sender, EventArgs e)
        {
            if (activeChildForm != null && activeFormHostPanel != null)
            {
                activeChildForm.Bounds = activeFormHostPanel.ClientRectangle;
            }
        }

        private void LoadedForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            RefreshAuditNotification();
        }

        private void btnpossystemdashboard_Click(object sender, EventArgs e)
        {
            ActivateMenuButton(btnpossystemdashboard);
            ShowDashboardHome();
        }

        private void ShowDashboardHome()
        {
            SetWorkspaceTitle("Dashboard");
            activeChildForm = null;
            activeFormHostPanel = null;
            panelContent.Controls.Clear();

            foreach (Control control in dashboardHomeControls)
            {
                panelContent.Controls.Add(control);
            }

            if (dashboardMenuStrip != null && !panelContent.Controls.Contains(dashboardMenuStrip))
            {
                panelContent.Controls.Add(dashboardMenuStrip);
            }

            LayoutDashboardHome();
            BringDashboardHeaderToFront();

            if (auditPanel != null && !panelContent.Controls.Contains(auditPanel))
            {
                panelContent.Controls.Add(auditPanel);
                auditPanel.BringToFront();
            }

            if (customerBalancePanel != null && !panelContent.Controls.Contains(customerBalancePanel))
            {
                panelContent.Controls.Add(customerBalancePanel);
            }

            if (endWorkDayButton != null && !panelContent.Controls.Contains(endWorkDayButton))
            {
                panelContent.Controls.Add(endWorkDayButton);
                endWorkDayButton.BringToFront();
            }

            ApplyRolePermissions();
            LoadDashboardData();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LogoutToLogin(); // Hide the dashboard
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenReports))
            {
                MessageBox.Show("You do not have permission to open reports.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateMenuButton(button8);
            SetWorkspaceTitle("Reports Overview");
            LoadForm(new ReportsOverviewPageForm());
        }

        private void btnAddUsers_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenUsers))
            {
                MessageBox.Show("You do not have permission to open users.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateMenuButton(btnAddUsers);
            SetWorkspaceTitle("Users");
            LoadForm(new AddUsers());
        }

        private void PosSystemDashboard_Load(object sender, EventArgs e)
        {
            RefreshAuditNotification();
            LoadDashboardData();
            if (AppSession.IsCashier)
            {
                ActivateMenuButton(btnsales);
                OpenSalesWorkspace();
            }
            else
            {
                ActivateMenuButton(btnpossystemdashboard);
                ShowDashboardHome();
            }
        }

        private void ConfigureDashboardShell()
        {
            Text = "POS System Dashboard";
            BackColor = Color.FromArgb(244, 247, 252);
            panelContent.BackColor = Color.FromArgb(244, 247, 252);
            panel1.BackColor = Color.FromArgb(15, 23, 42);
            panelheader.BackColor = Color.FromArgb(17, 24, 39);
            panelheader.Dock = DockStyle.None;
            panelheader.Height = 84;
            panel2.Dock = DockStyle.None;
            panel3.Dock = DockStyle.None;
            panel2.BackColor = Color.White;
            panel3.BackColor = Color.White;

            button11.Visible = false;
            button12.Visible = false;
            panel1.Visible = false;
            btnAddSales.Visible = false;
            btnAddSales.Enabled = false;

            StyleMenuButton(btnpossystemdashboard);
            StyleMenuButton(btnsales);
            StyleMenuButton(btnProducts);
            StyleMenuButton(btnAddSales);
            StyleMenuButton(btnCustomers);
            StyleMenuButton(btnEarningReports);
            StyleMenuButton(btnWarhouseReports);
            StyleMenuButton(button8);
            StyleMenuButton(btnAddUsers);
            StyleMenuButton(btnsettings);

            StyleMetricBox(textBox1);
            StyleMetricBox(textBox2);
            StyleMetricBox(textBox3);
            StyleMetricBox(textBox4);
            labelprofit.ForeColor = Color.FromArgb(22, 101, 52);
            labelprofit.Font = new Font("Segoe UI", 9F, FontStyle.Bold);

            label11.Text = "POS DASHBOARD";
            label6.Text = DateTime.Now.Hour < 12 ? "Morning Shift" :
                DateTime.Now.Hour < 18 ? "Afternoon Shift" : "Evening Shift";

            ConfigureChart(chart1, "Sales Last 7 Days");
            ConfigureChart(chart2, "Inventory By Category");
            AddDashboardMenuStrip();
            AddEndWorkDayButton();
            AddAuditNotificationControls();
            AddCustomerBalanceControls();
            ApplyRolePermissions();
            StartClock();
            panelContent.Resize += DashboardContent_Resize;
            Resize += DashboardContent_Resize;
            LayoutDashboardHome();
        }

        private void DashboardContent_Resize(object sender, EventArgs e)
        {
            LayoutDashboardHome();
        }

        private void LayoutDashboardHome()
        {
            if (panelContent == null || panelContent.Width <= 0 || panelContent.Height <= 0)
            {
                return;
            }

            panelheader.Width = panelContent.ClientSize.Width;
            panel2.Width = panelContent.ClientSize.Width;
            LayoutDashboardHeaderAndMenu();

            LayoutMetricCards();
            LayoutCharts();
            LayoutCustomerBalancePanel();
            LayoutFloatingDashboardControls();
            LayoutActiveChildForm();
        }

        private void LayoutDashboardHeaderAndMenu()
        {
            if (panelContent == null || panelheader == null)
            {
                return;
            }

            panelheader.SetBounds(0, 0, panelContent.ClientSize.Width, 84);

            if (dashboardMenuStrip != null)
            {
                dashboardMenuStrip.SetBounds(0, panelheader.Bottom, panelContent.ClientSize.Width, 52);
                LayoutDashboardMenuButtons();
            }
        }

        private int DashboardBodyTop
        {
            get
            {
                return dashboardMenuStrip != null ? dashboardMenuStrip.Bottom : panelheader.Bottom;
            }
        }

        private void LayoutActiveChildForm()
        {
            if (activeFormHostPanel == null || panelContent == null)
            {
                return;
            }

            int top = DashboardBodyTop;
            activeFormHostPanel.SetBounds(
                0,
                top,
                panelContent.ClientSize.Width,
                Math.Max(0, panelContent.ClientSize.Height - top));
        }

        private void LayoutMetricCards()
        {
            if (panel2 == null)
            {
                return;
            }

            panel2.SetBounds(0, DashboardBodyTop, panelContent.ClientSize.Width, 100);

            var cards = new[]
            {
                new MetricCard(textBox1, label8, lbmbe3atlyoum, labelprofit),
                new MetricCard(textBox2, label9, lb3ddfweter, label18),
                new MetricCard(textBox3, label13, lb3ddmontajet, label19),
                new MetricCard(textBox4, label14, lbr2slmal, label20)
            }.Where(card => card.Backdrop.Visible).ToList();

            if (cards.Count == 0)
            {
                return;
            }

            int gap = 12;
            int cardWidth = Math.Max(190, (panel2.ClientSize.Width - ((cards.Count + 1) * gap)) / cards.Count);
            int cardHeight = Math.Max(82, panel2.ClientSize.Height - (gap * 2));

            for (int index = 0; index < cards.Count; index++)
            {
                MetricCard card = cards[index];
                int left = gap + (index * (cardWidth + gap));

                card.Backdrop.SetBounds(left, gap, cardWidth, cardHeight);
                card.Title.SetBounds(left + 14, gap + 8, cardWidth - 28, 22);
                card.Value.SetBounds(left + 14, gap + 30, cardWidth - 28, 30);
                card.Subtitle.SetBounds(left + 14, gap + cardHeight - 26, cardWidth - 28, 20);
                card.Title.BringToFront();
                card.Value.BringToFront();
                card.Subtitle.BringToFront();
            }
        }

        private void LayoutCharts()
        {
            if (panel3 == null)
            {
                return;
            }

            int margin = 10;
            int top = panel2.Bottom + margin;
            bool showBalancePanel = customerBalancePanel != null && customerBalancePanel.Visible;
            int balanceReserve = showBalancePanel ? 220 : 0;
            int bottomReserve = 72 + balanceReserve;
            int height = Math.Max(240, panelContent.ClientSize.Height - top - bottomReserve);

            panel3.SetBounds(margin, top, Math.Max(300, panelContent.ClientSize.Width - (margin * 2)), height);

            int chartGap = 10;
            int chartWidth = Math.Max(220, (panel3.ClientSize.Width - chartGap) / 2);
            chart1.SetBounds(0, 0, chartWidth, panel3.ClientSize.Height);
            chart2.SetBounds(chartWidth + chartGap, 0, Math.Max(220, panel3.ClientSize.Width - chartWidth - chartGap), panel3.ClientSize.Height);
        }

        private void LayoutCustomerBalancePanel()
        {
            if (customerBalancePanel == null || !customerBalancePanel.Visible)
            {
                return;
            }

            int margin = 10;
            int top = panel3.Bottom + margin;
            int bottomReserve = endWorkDayButton != null ? endWorkDayButton.Height + 28 : 72;
            int height = Math.Max(180, panelContent.ClientSize.Height - top - bottomReserve);

            customerBalancePanel.SetBounds(
                margin,
                top,
                Math.Max(300, panelContent.ClientSize.Width - (margin * 2)),
                height);

            int summaryWidth = Math.Max(220, (customerBalancePanel.ClientSize.Width / 2) - 220);

            if (customerBalanceSummaryLabel != null)
            {
                customerBalanceSummaryLabel.SetBounds(190, 10, summaryWidth, 22);
            }

            if (supplierBalanceSummaryLabel != null)
            {
                supplierBalanceSummaryLabel.SetBounds(196, 10, summaryWidth, 22);
            }

            int gridTop = 36;
            int gridGap = 12;
            int gridWidth = Math.Max(120, (customerBalancePanel.ClientSize.Width - gridGap) / 2);
            int gridHeight = Math.Max(60, customerBalancePanel.ClientSize.Height - gridTop - 8);

            if (dataGridView1 != null)
            {
                dataGridView1.SetBounds(8, gridTop, gridWidth - 8, gridHeight);
            }

            if (dataGridView2 != null)
            {
                dataGridView2.SetBounds(gridWidth + gridGap, gridTop, Math.Max(120, customerBalancePanel.ClientSize.Width - gridWidth - gridGap - 8), gridHeight);
            }
        }

        private void LayoutFloatingDashboardControls()
        {
            if (endWorkDayButton != null)
            {
                endWorkDayButton.Location = new Point(
                    Math.Max(12, panelContent.ClientSize.Width - endWorkDayButton.Width - 20),
                    Math.Max(panel3.Bottom + 12, panelContent.ClientSize.Height - endWorkDayButton.Height - 20));
            }

            if (auditNotificationButton != null)
            {
                auditNotificationButton.Location = new Point(
                    Math.Max(12, panelheader.ClientSize.Width - auditNotificationButton.Width - 16),
                    15);
            }

            if (auditBadgeLabel != null)
            {
                auditBadgeLabel.Location = new Point(
                    Math.Max(12, panelheader.ClientSize.Width - auditBadgeLabel.Width - 4),
                    10);
            }

            if (auditPanel != null)
            {
                auditPanel.Location = new Point(
                    Math.Max(12, panelContent.ClientSize.Width - auditPanel.Width - 20),
                    panelheader.Bottom + 12);
                auditPanel.Height = Math.Min(420, Math.Max(220, panelContent.ClientSize.Height - auditPanel.Top - 20));
            }
        }

        private void AddEndWorkDayButton()
        {
            if (endWorkDayButton != null)
            {
                return;
            }

            endWorkDayButton = new Button
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 38, 38),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(panelContent.Width - 190, panelContent.Height - 58),
                Name = "btnEndWorkDay",
                Size = new Size(170, 38),
                Text = "End Work Today"
            };

            endWorkDayButton.FlatAppearance.BorderSize = 0;
            endWorkDayButton.Click += EndWorkDayButton_Click;
            panelContent.Controls.Add(endWorkDayButton);
            endWorkDayButton.BringToFront();
        }

        private void AddAuditNotificationControls()
        {
            if (auditNotificationButton != null)
            {
                return;
            }

            auditNotificationButton = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(17, 24, 39),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Location = new Point(panelheader.Width - 58, 15),
                Name = "btnAuditNotifications",
                Size = new Size(42, 42),
                Text = string.Empty
            };

            auditNotificationButton.FlatAppearance.BorderSize = 0;
            auditNotificationButton.Click += AuditNotificationButton_Click;
            auditNotificationButton.Paint += AuditNotificationButton_Paint;
            panelheader.Controls.Add(auditNotificationButton);
            auditNotificationButton.BringToFront();

            auditBadgeLabel = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7F, FontStyle.Bold),
                Location = new Point(panelheader.Width - 28, 10),
                Name = "auditBadgeLabel",
                Size = new Size(24, 18),
                Text = "0",
                TextAlign = ContentAlignment.MiddleCenter
            };
            auditBadgeLabel.Click += AuditNotificationButton_Click;
            panelheader.Controls.Add(auditBadgeLabel);
            auditBadgeLabel.BringToFront();
            ApplyRolePermissions();

            auditPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(panelContent.Width - 760, 85),
                Name = "auditPanel",
                Size = new Size(860, 460),
                Visible = false
            };

            auditGrid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Dock = DockStyle.Fill,
                MultiSelect = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            Panel auditToolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 86,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            deleteAuditButton = new Button
            {
                BackColor = Color.FromArgb(220, 38, 38),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Location = new Point(12, 8),
                Size = new Size(140, 30),
                Text = "Delete Selected"
            };
            deleteAuditButton.FlatAppearance.BorderSize = 0;
            deleteAuditButton.Click += DeleteAuditButton_Click;

            Label userSearchLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(168, 13),
                Text = "User"
            };

            auditUserSearchTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(210, 9),
                Name = "txtAuditUserSearch",
                Size = new Size(150, 27)
            };

            Label actionSearchLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(376, 13),
                Text = "Action"
            };

            auditActionSearchTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 9F),
                Location = new Point(432, 9),
                Name = "txtAuditActionSearch",
                Size = new Size(110, 27)
            };

            Label dateSearchLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(556, 13),
                Text = "Date"
            };

            auditDateSearchPicker = new DateTimePicker
            {
                Checked = false,
                CustomFormat = "yyyy-MM-dd",
                Format = DateTimePickerFormat.Custom,
                Location = new Point(600, 9),
                Name = "dtpAuditDateSearch",
                ShowCheckBox = true,
                Size = new Size(140, 27)
            };

            searchAuditButton = new Button
            {
                BackColor = Color.FromArgb(37, 99, 235),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Location = new Point(168, 48),
                Size = new Size(90, 30),
                Text = "Search"
            };
            searchAuditButton.FlatAppearance.BorderSize = 0;
            searchAuditButton.Click += SearchAuditButton_Click;

            clearAuditSearchButton = new Button
            {
                BackColor = Color.FromArgb(100, 116, 139),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Location = new Point(268, 48),
                Size = new Size(90, 30),
                Text = "Clear"
            };
            clearAuditSearchButton.FlatAppearance.BorderSize = 0;
            clearAuditSearchButton.Click += ClearAuditSearchButton_Click;

            closeAuditButton = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(100, 116, 139),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Location = new Point(748, 8),
                Size = new Size(100, 30),
                Text = "Close"
            };
            closeAuditButton.FlatAppearance.BorderSize = 0;
            closeAuditButton.Click += (sender, args) => auditPanel.Visible = false;

            auditToolbar.Controls.Add(deleteAuditButton);
            auditToolbar.Controls.Add(userSearchLabel);
            auditToolbar.Controls.Add(auditUserSearchTextBox);
            auditToolbar.Controls.Add(actionSearchLabel);
            auditToolbar.Controls.Add(auditActionSearchTextBox);
            auditToolbar.Controls.Add(dateSearchLabel);
            auditToolbar.Controls.Add(auditDateSearchPicker);
            auditToolbar.Controls.Add(searchAuditButton);
            auditToolbar.Controls.Add(clearAuditSearchButton);
            auditToolbar.Controls.Add(closeAuditButton);
            auditPanel.Controls.Add(auditGrid);
            auditPanel.Controls.Add(auditToolbar);
            panelContent.Controls.Add(auditPanel);
            auditPanel.BringToFront();

            ApplyRolePermissions();
            RefreshAuditNotification();
        }

        private void AddCustomerBalanceControls()
        {
            if (customerBalancePanel != null)
            {
                StyleDashboardBalanceGrid(dataGridView1);
                StyleDashboardBalanceGrid(dataGridView2);
                return;
            }

            customerBalancePanel = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Name = "customerBalancePanel",
                Visible = false
            };

            TableLayoutPanel balanceLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(8)
            };
            balanceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            balanceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            balanceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Panel customerPanel = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 36, 6, 0)
            };

            Panel supplierPanel = new Panel
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Padding = new Padding(6, 36, 0, 0)
            };

            customerBalanceTitleLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(0, 8),
                Text = "Customer Balances"
            };

            customerBalanceSummaryLabel = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(190, 10),
                Size = new Size(420, 22),
                TextAlign = ContentAlignment.MiddleRight
            };

            supplierBalanceTitleLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(6, 8),
                Text = "Supplier Balances"
            };

            supplierBalanceSummaryLabel = new Label
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 85, 105),
                Location = new Point(196, 10),
                Size = new Size(420, 22),
                TextAlign = ContentAlignment.MiddleRight
            };

            dataGridView1 = CreateDashboardBalanceGrid("dataGridView1");
            dataGridView2 = CreateDashboardBalanceGrid("dataGridView2");

            customerPanel.Controls.Add(dataGridView1);
            customerPanel.Controls.Add(customerBalanceTitleLabel);
            customerPanel.Controls.Add(customerBalanceSummaryLabel);
            supplierPanel.Controls.Add(dataGridView2);
            supplierPanel.Controls.Add(supplierBalanceTitleLabel);
            supplierPanel.Controls.Add(supplierBalanceSummaryLabel);
            balanceLayout.Controls.Add(customerPanel, 0, 0);
            balanceLayout.Controls.Add(supplierPanel, 1, 0);
            customerBalancePanel.Controls.Add(balanceLayout);
            panelContent.Controls.Add(customerBalancePanel);
        }

        private DataGridView CreateDashboardBalanceGrid(string name)
        {
            DataGridView grid = new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                MultiSelect = false,
                Name = name,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            return grid;
        }

        private void StyleDashboardBalanceGrid(DataGridView grid)
        {
            if (grid == null)
            {
                return;
            }

            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = Color.White;
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        }

        private void StyleMenuButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = Color.FromArgb(15, 23, 42);
            button.ForeColor = Color.White;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(18, 0, 0, 0);
            button.Cursor = Cursors.Hand;
        }

        private void StyleMetricBox(TextBox textBox)
        {
            textBox.BackColor = Color.White;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.ReadOnly = true;
            textBox.TabStop = false;
        }

        private void AddDashboardMenuStrip()
        {
            if (dashboardMenuStrip != null)
            {
                return;
            }

            dashboardMenuStrip = new MenuStrip
            {
                AutoSize = false,
                BackColor = Color.FromArgb(15, 23, 42),
                CanOverflow = false,
                ForeColor = Color.White,
                Dock = DockStyle.None,
                GripStyle = ToolStripGripStyle.Hidden,
                ImageScalingSize = new Size(20, 20),
                LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow,
                Name = "dashboardMenuStrip",
                Padding = new Padding(8, 4, 8, 4),
                RenderMode = ToolStripRenderMode.System,
                RightToLeft = RightToLeft.No
            };

            menuDashboard = CreateDashboardMenuButton("Dashboard", btnpossystemdashboard_Click);
            menuSales = CreateDashboardMenuButton("Sales", btnsales_Click);
            menuProducts = CreateDashboardMenuButton("Inventory", btnProducts_Click);
            menuSupplier = CreateDashboardMenuButton("Supplier", menuSupplier_Click);
            menuCustomers = CreateDashboardMenuButton("Customers", btnCustomers_Click);
            menuEarningReports = CreateDashboardMenuButton("Earning Report", btnEarningReports_Click);
            menuWarhouseReports = CreateDashboardMenuButton("Warehouse Reports", btnWarhouseReports_Click);
            menuReports = CreateDashboardMenuButton("Reports", button8_Click);
            menuAddUsers = CreateDashboardMenuButton("Users", btnAddUsers_Click);
            menuSettings = CreateDashboardMenuButton("Settings", button10_Click);
            menuSave = CreateDashboardMenuButton("Save", (sender, args) => ExecuteActiveFormCommand("save"));
            menuSaveAs = CreateDashboardMenuButton("Save As", (sender, args) => ExecuteActiveFormCommand("saveas"));
            menuEdit = CreateDashboardMenuButton("Edit", (sender, args) => ExecuteActiveFormCommand("edit"));
            menuDelete = CreateDashboardMenuButton("Delete", (sender, args) => ExecuteActiveFormCommand("delete"));
            menuPrint = CreateDashboardMenuButton("Print", (sender, args) => ExecuteActiveFormCommand("print"));
            menuLogout = CreateDashboardMenuButton("Logout", button1_Click);
            menuLogout.Alignment = ToolStripItemAlignment.Right;
            menuLogout.BackColor = Color.FromArgb(220, 38, 38);
            dashboardMenuStrip.Resize += (sender, args) => LayoutDashboardMenuButtons();

            dashboardMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                menuDashboard,
                menuSales,
                menuProducts,
                menuSupplier,
                menuCustomers,
                menuEarningReports,
                menuWarhouseReports,
                menuReports,
                menuAddUsers,
                menuSettings,
                new ToolStripSeparator(),
                menuSave,
                menuSaveAs,
                menuEdit,
                menuDelete,
                menuPrint,
                menuLogout
            });

            MainMenuStrip = dashboardMenuStrip;
            panelContent.Controls.Add(dashboardMenuStrip);
            BringDashboardHeaderToFront();
        }

        private ToolStripButton CreateDashboardMenuButton(string text, EventHandler clickHandler)
        {
            ToolStripButton item = new ToolStripButton(text)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ForeColor = Color.White,
                Padding = new Padding(10, 4, 10, 4)
            };

            item.Click += clickHandler;
            return item;
        }

        private void LayoutDashboardMenuButtons()
        {
            if (dashboardMenuStrip == null || menuLogout == null)
            {
                return;
            }

            foreach (ToolStripItem item in dashboardMenuStrip.Items)
            {
                item.AutoSize = true;
                item.Margin = new Padding(3, 1, 3, 1);
            }

            menuLogout.Alignment = ToolStripItemAlignment.Right;
            menuLogout.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void BringDashboardHeaderToFront()
        {
            if (dashboardMenuStrip != null)
            {
                dashboardMenuStrip.BringToFront();
            }

            if (panelheader != null)
            {
                panelheader.BringToFront();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                ExecuteActiveFormCommand("save");
                return true;
            }

            if (keyData == (Keys.Control | Keys.Shift | Keys.S))
            {
                ExecuteActiveFormCommand("saveas");
                return true;
            }

            if (keyData == (Keys.Control | Keys.E))
            {
                ExecuteActiveFormCommand("edit");
                return true;
            }

            if (keyData == Keys.Delete)
            {
                ExecuteActiveFormCommand("delete");
                return true;
            }

            if (keyData == (Keys.Control | Keys.P))
            {
                ExecuteActiveFormCommand("print");
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ExecuteActiveFormCommand(string command)
        {
            if (activeChildForm == null)
            {
                MessageBox.Show("Open a form first.", "Dashboard Menu");
                return;
            }

            if (TryClickMatchingButton(activeChildForm, command) || TryClickGridButton(activeChildForm, command))
            {
                return;
            }

            MessageBox.Show("This command is not available on the current form.", "Dashboard Menu");
        }

        private bool TryClickMatchingButton(Control parent, string command)
        {
            foreach (Control control in GetAllControls(parent))
            {
                if (control is Button button && button.Visible && button.Enabled && IsButtonMatch(button, command))
                {
                    button.PerformClick();
                    return true;
                }
            }

            return false;
        }

        private bool IsButtonMatch(Button button, string command)
        {
            string name = (button.Name ?? string.Empty).ToLowerInvariant();
            string text = (button.Text ?? string.Empty).Trim().ToLowerInvariant();

            switch (command)
            {
                case "save":
                    return name.Contains("save") || name.Contains("add") || text.Contains("save") || text.Contains("حفظ") || text.Contains("بيع");
                case "saveas":
                    return name.Contains("excel") || text.Contains("excel") || text.Contains("export");
                case "edit":
                    return name.Contains("edit") || text.Contains("edit");
                case "delete":
                    return name.Contains("delete") || text.Contains("delete") || text.Contains("مسح");
                case "print":
                    return name.Contains("print") || text.Contains("print") || text.Contains("طبع");
                default:
                    return false;
            }
        }

        private bool TryClickGridButton(Control parent, string command)
        {
            foreach (Control control in GetAllControls(parent))
            {
                if (control is DataGridView grid &&
                    grid.Visible &&
                    grid.Enabled &&
                    grid.CurrentRow != null &&
                    grid.CurrentRow.Index >= 0)
                {
                    foreach (DataGridViewColumn column in grid.Columns)
                    {
                        if (column is DataGridViewButtonColumn &&
                            column.Visible &&
                            column.Name.IndexOf(command, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            grid.CurrentCell = grid.Rows[grid.CurrentRow.Index].Cells[column.Index];
                            MethodInfo handler = typeof(DataGridView).GetMethod(
                                "OnCellContentClick",
                                BindingFlags.Instance | BindingFlags.NonPublic);
                            handler?.Invoke(grid, new object[] { new DataGridViewCellEventArgs(column.Index, grid.CurrentRow.Index) });
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private IEnumerable<Control> GetAllControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                yield return control;

                foreach (Control child in GetAllControls(control))
                {
                    yield return child;
                }
            }
        }

        private void ConfigureChart(Chart chart, string title)
        {
            chart.BackColor = Color.White;
            chart.Titles.Clear();
            chart.Titles.Add(new Title(title, Docking.Top, new Font("Segoe UI", 11F, FontStyle.Bold), Color.FromArgb(30, 41, 59)));
            chart.Legends.Clear();
            chart.Legends.Add(new Legend("Legend"));
            chart.ChartAreas[0].BackColor = Color.White;
            chart.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.FromArgb(226, 232, 240);
            chart.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(226, 232, 240);
        }

        private void StartClock()
        {
            if (clockTimer == null)
            {
                clockTimer = new Timer();
                clockTimer.Interval = 1000;
                clockTimer.Tick += (sender, args) =>
                {
                    lbdatetime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                };
            }

            clockTimer.Start();
        }

        private void ApplyRolePermissions()
        {
            string role = (_role ?? string.Empty).Trim().ToLowerInvariant();
            bool isAdmin = role == "admin";
            bool isManager = role == "manager" || role == "manger";
            bool isCashier = role == "cashier";
            bool canViewDashboard = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenDashboard);
            bool canViewSales = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenSales);
            bool canViewUsers = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenUsers);
            bool canViewSettings = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenSettings) ||
                PermissionService.CanManagePermissions(AppSession.UserId, _role);
            bool canViewReports = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenReports);
            bool canViewEarningReports = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenEarningReports);
            bool canViewWarehouse = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenWarehouse);
            bool canViewProducts = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenProducts);
            bool canViewSuppliers = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenSuppliers);
            bool canViewCustomers = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenCustomers);
            bool canViewDashboardBalances = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenDashboardBalances);

            btnAddUsers.Visible = canViewUsers;
            btnsettings.Visible = canViewSettings;
            button8.Visible = canViewReports;
            btnEarningReports.Visible = canViewEarningReports;
            btnWarhouseReports.Visible = canViewWarehouse;
            btnProducts.Visible = canViewProducts;
            btnAddSales.Visible = false;
            btnCustomers.Visible = canViewCustomers;
            btnpossystemdashboard.Visible = canViewDashboard;
            btnsales.Visible = canViewSales;

            if (menuAddUsers != null)
            {
                menuDashboard.Visible = canViewDashboard;
                menuSales.Visible = canViewSales;
                menuAddUsers.Visible = canViewUsers;
                menuSettings.Visible = canViewSettings;
                menuReports.Visible = canViewReports;
                menuEarningReports.Visible = canViewEarningReports;
                menuWarhouseReports.Visible = canViewWarehouse;
                menuProducts.Visible = canViewProducts;
                menuSupplier.Visible = canViewSuppliers;
                menuCustomers.Visible = canViewCustomers;
            }

            bool canSeeAudit = PermissionService.CanViewNotifications(AppSession.UserId, _role);
            bool canDeleteAudit = PermissionService.CanDeleteNotifications(AppSession.UserId, _role);
            if (auditNotificationButton != null)
            {
                auditNotificationButton.Visible = canSeeAudit;
            }

            if (auditBadgeLabel != null)
            {
                auditBadgeLabel.Visible = canSeeAudit;
            }

            if (deleteAuditButton != null)
            {
                deleteAuditButton.Visible = canSeeAudit && canDeleteAudit;
            }

            bool canSeeInventoryMetrics = isAdmin || isManager;
            textBox3.Visible = canSeeInventoryMetrics;
            textBox4.Visible = canSeeInventoryMetrics;
            label13.Visible = canSeeInventoryMetrics;
            label14.Visible = canSeeInventoryMetrics;
            lb3ddmontajet.Visible = canSeeInventoryMetrics;
            lbr2slmal.Visible = canSeeInventoryMetrics;
            label19.Visible = canSeeInventoryMetrics;
            label20.Visible = canSeeInventoryMetrics;
            labelprofit.Visible = !isCashier;
            SetCustomerBalancePanelVisible(canViewDashboardBalances);
            LayoutDashboardHome();
        }

        private void LoadDashboardData()
        {
            try
            {
                DashboardStats stats = GetDashboardStats();

                lbmbe3atlyoum.Text = FormatCurrency(stats.TodaySales);
                lb3ddfweter.Text = stats.TodayInvoices.ToString("N0");
                lb3ddmontajet.Text = stats.ProductCount.ToString("N0");
                lbr2slmal.Text = FormatCurrency(stats.InventoryCapital);

                labelprofit.Text = "Profit today: " + FormatCurrency(stats.TodayProfit);
                label18.Text = "Customers: " + stats.CustomerCount.ToString("N0");
                label19.Text = "Low stock: " + stats.LowStockCount.ToString("N0");
                label20.Text = "Top product: " + stats.TopProductName;

                LoadSalesTrendChart();
                LoadInventoryChart();
                LoadCustomerBalanceData();
                RefreshAuditNotification();
            }
            catch (Exception ex)
            {
                lbmbe3atlyoum.Text = "$0.00";
                lb3ddfweter.Text = "0";
                lb3ddmontajet.Text = "0";
                lbr2slmal.Text = "$0.00";
                labelprofit.Text = "Dashboard error";
                label18.Text = ex.Message;
                label19.Text = string.Empty;
                label20.Text = string.Empty;
            }
        }

        private void SetCustomerBalancePanelVisible(bool visible)
        {
            if (customerBalancePanel == null)
            {
                return;
            }

            customerBalancePanel.Visible = visible;

            if (!visible && dataGridView1 != null)
            {
                dataGridView1.DataSource = null;
            }

            if (!visible && dataGridView2 != null)
            {
                dataGridView2.DataSource = null;
            }
        }

        private void LoadCustomerBalanceData()
        {
            if (customerBalancePanel == null || dataGridView1 == null || dataGridView2 == null || !customerBalancePanel.Visible)
            {
                return;
            }

            AuditLogger.EnsureCustomerBalanceColumns();
            AuditLogger.EnsureSupplierBalanceColumns();

            DataTable table = ExecuteDataTable(@"
                WITH CustomerBalances AS
                (
                    SELECT
                        name,
                        phone,
                        balance_updated_at,
                        EffectiveUsd = CASE
                            WHEN ISNULL(balance_usd, 0) = 0
                             AND ISNULL(balance_lb, 0) = 0
                             AND ISNULL(balance, 0) <> 0 THEN ISNULL(balance, 0)
                            ELSE ISNULL(balance_usd, 0)
                        END,
                        EffectiveLb = ISNULL(balance_lb, 0)
                    FROM Customers
                )
                SELECT TOP 50
                       name AS [Customer],
                       phone AS [Phone],
                       CASE
                           WHEN EffectiveUsd > 0 OR EffectiveLb > 0 THEN N'Customer owes you'
                           WHEN EffectiveUsd < 0 OR EffectiveLb < 0 THEN N'You owe customer'
                           ELSE N'No balance'
                       END AS [Balance Type],
                       CASE
                           WHEN EffectiveUsd = 0 THEN N'$ 0.00'
                           ELSE CONCAT(CASE WHEN EffectiveUsd < 0 THEN N'-' ELSE N'' END, N'$ ', FORMAT(ABS(EffectiveUsd), 'N2'))
                       END AS [USD Balance],
                       CASE
                           WHEN EffectiveLb = 0 THEN N'0 L.L'
                           ELSE CONCAT(CASE WHEN EffectiveLb < 0 THEN N'-' ELSE N'' END, FORMAT(ABS(EffectiveLb), 'N0'), N' L.L')
                       END AS [L.L Balance],
                       CASE
                           WHEN balance_updated_at IS NULL THEN N'-'
                           ELSE FORMAT(balance_updated_at, 'yyyy-MM-dd HH:mm')
                       END AS [Updated]
                FROM CustomerBalances
                WHERE EffectiveUsd <> 0
                   OR EffectiveLb <> 0
                ORDER BY
                    CASE WHEN balance_updated_at IS NULL THEN 1 ELSE 0 END,
                    balance_updated_at DESC,
                    name ASC;");

            dataGridView1.DataSource = table;
            customerBalanceSummaryLabel.Text = BuildCustomerBalanceSummary();

            if (dataGridView1.Columns.Contains("Phone"))
            {
                dataGridView1.Columns["Phone"].FillWeight = 90;
            }

            if (dataGridView1.Columns.Contains("Balance Type"))
            {
                dataGridView1.Columns["Balance Type"].FillWeight = 130;
            }

            LoadSupplierBalanceData();
        }

        private void LoadSupplierBalanceData()
        {
            DataTable table = ExecuteDataTable(@"
                SELECT TOP 50
                       name AS [Supplier],
                       contact_info AS [Phone],
                       CASE
                           WHEN ISNULL(balance_usd, 0) > 0 OR ISNULL(balance_lb, 0) > 0 THEN N'You owe supplier'
                           WHEN ISNULL(balance_usd, 0) < 0 OR ISNULL(balance_lb, 0) < 0 THEN N'Supplier owes you'
                           ELSE N'No balance'
                       END AS [Balance Type],
                       CASE
                           WHEN ISNULL(balance_usd, 0) = 0 THEN N'$ 0.00'
                           ELSE CONCAT(CASE WHEN balance_usd < 0 THEN N'-' ELSE N'' END, N'$ ', FORMAT(ABS(balance_usd), 'N2'))
                       END AS [USD Balance],
                       CASE
                           WHEN ISNULL(balance_lb, 0) = 0 THEN N'0 L.L'
                           ELSE CONCAT(CASE WHEN balance_lb < 0 THEN N'-' ELSE N'' END, FORMAT(ABS(balance_lb), 'N0'), N' L.L')
                       END AS [L.L Balance],
                       CASE
                           WHEN balance_updated_at IS NULL THEN N'-'
                           ELSE FORMAT(balance_updated_at, 'yyyy-MM-dd HH:mm')
                       END AS [Balance DateTime]
                FROM Suppliers
                WHERE ISNULL(balance_usd, 0) <> 0
                   OR ISNULL(balance_lb, 0) <> 0
                ORDER BY
                    CASE WHEN balance_updated_at IS NULL THEN 1 ELSE 0 END,
                    balance_updated_at DESC,
                    name ASC;");

            dataGridView2.DataSource = table;
            supplierBalanceSummaryLabel.Text = BuildSupplierBalanceSummary();

            if (dataGridView2.Columns.Contains("Phone"))
            {
                dataGridView2.Columns["Phone"].FillWeight = 90;
            }

            if (dataGridView2.Columns.Contains("Balance Type"))
            {
                dataGridView2.Columns["Balance Type"].FillWeight = 130;
            }
        }

        private string BuildCustomerBalanceSummary()
        {
            DataTable table = ExecuteDataTable(@"
                WITH CustomerBalances AS
                (
                    SELECT
                        EffectiveUsd = CASE
                            WHEN ISNULL(balance_usd, 0) = 0
                             AND ISNULL(balance_lb, 0) = 0
                             AND ISNULL(balance, 0) <> 0 THEN ISNULL(balance, 0)
                            ELSE ISNULL(balance_usd, 0)
                        END,
                        EffectiveLb = ISNULL(balance_lb, 0)
                    FROM Customers
                )
                SELECT
                    CustomerOwesUsd = ISNULL(SUM(CASE WHEN EffectiveUsd > 0 THEN EffectiveUsd ELSE 0 END), 0),
                    YouOweUsd = ISNULL(SUM(CASE WHEN EffectiveUsd < 0 THEN ABS(EffectiveUsd) ELSE 0 END), 0),
                    CustomerOwesLb = ISNULL(SUM(CASE WHEN EffectiveLb > 0 THEN EffectiveLb ELSE 0 END), 0),
                    YouOweLb = ISNULL(SUM(CASE WHEN EffectiveLb < 0 THEN ABS(EffectiveLb) ELSE 0 END), 0)
                FROM CustomerBalances;");

            if (table.Rows.Count == 0)
            {
                return "No open balances";
            }

            DataRow row = table.Rows[0];
            decimal customerOwesUsd = Convert.ToDecimal(row["CustomerOwesUsd"]);
            decimal youOweUsd = Convert.ToDecimal(row["YouOweUsd"]);
            decimal customerOwesLb = Convert.ToDecimal(row["CustomerOwesLb"]);
            decimal youOweLb = Convert.ToDecimal(row["YouOweLb"]);

            return string.Format(
                CultureInfo.InvariantCulture,
                "Customers owe: ${0:N2} / {1:N0} L.L  You owe: ${2:N2} / {3:N0} L.L",
                customerOwesUsd,
                customerOwesLb,
                youOweUsd,
                youOweLb);
        }

        private string BuildSupplierBalanceSummary()
        {
            DataTable table = ExecuteDataTable(@"
                SELECT
                    YouOweUsd = ISNULL(SUM(CASE WHEN ISNULL(balance_usd, 0) > 0 THEN balance_usd ELSE 0 END), 0),
                    SupplierOwesUsd = ISNULL(SUM(CASE WHEN ISNULL(balance_usd, 0) < 0 THEN ABS(balance_usd) ELSE 0 END), 0),
                    YouOweLb = ISNULL(SUM(CASE WHEN ISNULL(balance_lb, 0) > 0 THEN balance_lb ELSE 0 END), 0),
                    SupplierOwesLb = ISNULL(SUM(CASE WHEN ISNULL(balance_lb, 0) < 0 THEN ABS(balance_lb) ELSE 0 END), 0)
                FROM Suppliers;");

            if (table.Rows.Count == 0)
            {
                return "No open balances";
            }

            DataRow row = table.Rows[0];
            decimal youOweUsd = Convert.ToDecimal(row["YouOweUsd"]);
            decimal supplierOwesUsd = Convert.ToDecimal(row["SupplierOwesUsd"]);
            decimal youOweLb = Convert.ToDecimal(row["YouOweLb"]);
            decimal supplierOwesLb = Convert.ToDecimal(row["SupplierOwesLb"]);

            return string.Format(
                CultureInfo.InvariantCulture,
                "You owe: ${0:N2} / {1:N0} L.L  Supplier owes: ${2:N2} / {3:N0} L.L",
                youOweUsd,
                youOweLb,
                supplierOwesUsd,
                supplierOwesLb);
        }

        private DashboardStats GetDashboardStats()
        {
            DashboardStats stats = new DashboardStats();
            int lowStockThreshold = POS_System.Program.SettingsManager.GetIntSetting("low_stock_threshold", 5);
            DateTime workDayStart = GetWorkDayStart();

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                SELECT
                    TodaySales = ISNULL(SUM(s.total_amount), 0),
                    TodayInvoices = COUNT(1)
                FROM Sales s
                WHERE s.sale_date >= @WorkDayStart
                  AND s.sale_date < DATEADD(day, 1, CONVERT(date, GETDATE()))
                  AND TRY_CONVERT(INT, s.user_id) = @CurrentUserId
                  AND ISNULL(s.is_returned, 0) = 0;

                SELECT
                    ProductCount = COUNT(1),
                    InventoryCapital = ISNULL(SUM(ISNULL(sale_price_usd, 0) * ISNULL(stock_quantity, 0)), 0),
                    LowStockCount = SUM(CASE WHEN ISNULL(stock_quantity, 0) <= @LowStockThreshold THEN 1 ELSE 0 END)
                FROM Products;

                SELECT CustomerCount = COUNT(1) FROM Customers;

                SELECT TodayProfit = ISNULL(SUM(
                    CASE
                        WHEN ISNULL(si.unit_price, 0) >= (ISNULL(NULLIF(p.price_lb, 0), 0) / 2)
                             AND ISNULL(p.price_lb, 0) > 0
                            THEN
                                CASE
                                    WHEN ABS(ISNULL(si.unit_price, 0) - ISNULL(p.price_lb, 0)) < 0.0001
                                         AND ISNULL(p.sale_price_lb, 0) > ISNULL(p.price_lb, 0)
                                        THEN ISNULL(p.sale_price_lb, 0) - ISNULL(p.price_lb, 0)
                                    ELSE ISNULL(si.unit_price, 0) - ISNULL(p.price_lb, 0)
                                END
                        ELSE
                            CASE
                                WHEN ABS(ISNULL(si.unit_price, 0) - ISNULL(p.price_usd, 0)) < 0.0001
                                     AND ISNULL(p.sale_price_usd, 0) > ISNULL(p.price_usd, 0)
                                    THEN ISNULL(p.sale_price_usd, 0) - ISNULL(p.price_usd, 0)
                                ELSE ISNULL(si.unit_price, 0) - ISNULL(p.price_usd, 0)
                            END
                    END * ISNULL(si.quantity, 0)), 0)
                FROM Sale_Items si
                INNER JOIN Sales s ON si.sale_id = s.sale_id
                LEFT JOIN Products p ON si.product_id = p.product_id
                WHERE s.sale_date >= @WorkDayStart
                  AND s.sale_date < DATEADD(day, 1, CONVERT(date, GETDATE()))
                  AND TRY_CONVERT(INT, s.user_id) = @CurrentUserId
                  AND ISNULL(s.is_returned, 0) = 0;

                SELECT TOP 1 TopProductName = ISNULL(si.name_product, p.name)
                FROM Sale_Items si
                INNER JOIN Sales s ON si.sale_id = s.sale_id
                LEFT JOIN Products p ON si.product_id = p.product_id
                WHERE s.sale_date >= @WorkDayStart
                  AND s.sale_date < DATEADD(day, 1, CONVERT(date, GETDATE()))
                  AND TRY_CONVERT(INT, s.user_id) = @CurrentUserId
                  AND ISNULL(s.is_returned, 0) = 0
                GROUP BY ISNULL(si.name_product, p.name)
                ORDER BY SUM(ISNULL(si.quantity, 0)) DESC;", connection))
            {
                command.Parameters.AddWithValue("@LowStockThreshold", lowStockThreshold);
                command.Parameters.Add("@WorkDayStart", SqlDbType.DateTime2).Value = workDayStart;
                command.Parameters.Add("@CurrentUserId", SqlDbType.Int).Value = AppSession.UserId;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats.TodaySales = GetDecimal(reader, "TodaySales");
                        stats.TodayInvoices = GetInt(reader, "TodayInvoices");
                    }

                    if (reader.NextResult() && reader.Read())
                    {
                        stats.ProductCount = GetInt(reader, "ProductCount");
                        stats.InventoryCapital = GetDecimal(reader, "InventoryCapital");
                        stats.LowStockCount = GetInt(reader, "LowStockCount");
                    }

                    if (reader.NextResult() && reader.Read())
                    {
                        stats.CustomerCount = GetInt(reader, "CustomerCount");
                    }

                    if (reader.NextResult() && reader.Read())
                    {
                        stats.TodayProfit = GetDecimal(reader, "TodayProfit");
                    }

                    if (reader.NextResult() && reader.Read())
                    {
                        stats.TopProductName = Convert.ToString(reader["TopProductName"]);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(stats.TopProductName))
            {
                stats.TopProductName = "No sales yet";
            }

            return stats;
        }

        private DateTime GetWorkDayStart()
        {
            return WorkHistoryService.GetDashboardWorkStart(LoginForm.LoggedInUserId, _username) ?? DateTime.Today;
        }

        private void EndWorkDayButton_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show(
                "Close the current work day?\n\nThis will save the ending time in the History table and reset dashboard values for today's sales and invoices from this moment. It will not delete sales, invoices, products, or reports.",
                "End Work Day",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!WorkHistoryService.EndWork(LoginForm.LoggedInUserId, _username, out TimeSpan workedTime))
                {
                    MessageBox.Show("No open start work record was found for today. Click Sales, then START before ending work.", "End Work Day");
                    return;
                }

                MessageBox.Show(
                    $"Work saved in history. Worked time: {(int)workedTime.TotalHours} hour(s) and {workedTime.Minutes} minute(s).",
                    "End Work Day");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save ending work time: " + ex.Message, "End Work Day");
                return;
            }

            LoadDashboardData();

            if (AppSession.IsCashier)
            {
                ActivateMenuButton(btnsales);
                LoadStartSalesForm();
            }
        }

        private void LoadSalesTrendChart()
        {
            bool showAllUsers = CanCurrentUserSeeAllSalesTrend();

            DataTable table = ExecuteDataTable(@"
                SELECT SaleDay = CONVERT(date, sale_date),
                       TotalSales = ISNULL(SUM(total_amount), 0)
                FROM Sales
                WHERE sale_date >= DATEADD(day, -6, CONVERT(date, GETDATE()))
                  AND (@ShowAllUsers = 1 OR TRY_CONVERT(INT, user_id) = @CurrentUserId)
                  AND ISNULL(is_returned, 0) = 0
                GROUP BY CONVERT(date, sale_date)
                ORDER BY SaleDay;",
                new SqlParameter("@ShowAllUsers", SqlDbType.Bit) { Value = showAllUsers },
                new SqlParameter("@CurrentUserId", SqlDbType.Int) { Value = AppSession.UserId });

            chart1.Series.Clear();
            Series series = new Series("Sales")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 3,
                Color = Color.FromArgb(37, 99, 235),
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 7
            };

            foreach (DataRow row in table.Rows)
            {
                DateTime day = Convert.ToDateTime(row["SaleDay"]);
                decimal total = Convert.ToDecimal(row["TotalSales"]);
                series.Points.AddXY(day.ToString("MM-dd"), total);
            }

            chart1.Series.Add(series);
        }

        private void LoadInventoryChart()
        {
            DataTable table = ExecuteDataTable(@"
                SELECT TOP 8 c.category_name,
                       ProductCount = COUNT(p.product_id)
                FROM Products p
                LEFT JOIN Categories c ON p.category_id = c.category_id
                GROUP BY c.category_name
                ORDER BY COUNT(p.product_id) DESC;");

            chart2.Series.Clear();
            Series series = new Series("Products")
            {
                ChartType = SeriesChartType.Doughnut,
                IsValueShownAsLabel = true
            };

            foreach (DataRow row in table.Rows)
            {
                string category = string.IsNullOrWhiteSpace(Convert.ToString(row["category_name"]))
                    ? "Uncategorized"
                    : Convert.ToString(row["category_name"]);
                int count = Convert.ToInt32(row["ProductCount"]);
                series.Points.AddXY(category, count);
            }

            chart2.Series.Add(series);
        }

        private DataTable ExecuteDataTable(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private void OpenSalesWorkspace()
        {
            if (HasOpenSalesSession())
            {
                LoadSalesForm();
                return;
            }

            LoadStartSalesForm();
        }

        private void LoadSalesForm()
        {
            LoadForm(new Sales(lbusername.Text, _role));
        }

        private void LoadStartSalesForm()
        {
            LoadForm(new SalesStartForm(_username, StartSalesWork_Click));
        }

        private bool HasOpenSalesSession()
        {
            return WorkHistoryService.HasOpenWorkSession(LoginForm.LoggedInUserId, _username);
        }

        private bool CanCurrentUserSeeAllSalesTrend()
        {
            string role = (_role ?? string.Empty).Trim().ToLowerInvariant();
            return role == "admin" || role == "manager" || role == "manger";
        }

        private void AuditNotificationButton_Click(object sender, EventArgs e)
        {
            if (auditNotificationButton != null && !auditNotificationButton.Visible)
            {
                return;
            }

            auditPanel.Visible = !auditPanel.Visible;

            if (auditPanel.Visible)
            {
                LoadAuditLogs();
                MarkAuditNotificationsSeen();
                auditPanel.BringToFront();
            }
        }

        private void AuditNotificationButton_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (Pen pen = new Pen(Color.White, 2F))
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                Rectangle bellBody = new Rectangle(11, 12, 20, 18);
                e.Graphics.DrawArc(pen, bellBody, 200, 140);
                e.Graphics.DrawLine(pen, 11, 23, 8, 30);
                e.Graphics.DrawLine(pen, 31, 23, 34, 30);
                e.Graphics.DrawLine(pen, 8, 30, 34, 30);
                e.Graphics.DrawLine(pen, 21, 8, 21, 12);
                e.Graphics.FillEllipse(brush, 18, 32, 6, 6);
            }
        }

        private void LoadAuditLogs()
        {
            DateTime? selectedDate = auditDateSearchPicker != null && auditDateSearchPicker.Checked
                ? auditDateSearchPicker.Value.Date
                : (DateTime?)null;

            LoadAuditLogs(
                auditUserSearchTextBox != null ? auditUserSearchTextBox.Text : string.Empty,
                auditActionSearchTextBox != null ? auditActionSearchTextBox.Text : string.Empty,
                selectedDate);
        }

        private void LoadAuditLogs(string usernameFilter, string actionFilter, DateTime? dateFilter)
        {
            auditGrid.DataSource = AuditService.GetNotifications(usernameFilter, actionFilter, dateFilter);

            if (auditGrid.Columns.Contains("ID"))
            {
                auditGrid.Columns["ID"].Width = 55;
            }
        }

        private string NormalizeAuditActionFilter(string actionFilter)
        {
            string value = string.IsNullOrWhiteSpace(actionFilter) ? string.Empty : actionFilter.Trim();

            if (value.Equals("update", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("updated", StringComparison.OrdinalIgnoreCase))
            {
                return "EDIT";
            }

            if (value.Equals("create", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("created", StringComparison.OrdinalIgnoreCase))
            {
                return "ADD";
            }

            if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            if (value.Equals("delete", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("deleted", StringComparison.OrdinalIgnoreCase))
            {
                return "DELETE";
            }

            return value;
        }

        private void SearchAuditButton_Click(object sender, EventArgs e)
        {
            LoadAuditLogs();
        }

        private void ClearAuditSearchButton_Click(object sender, EventArgs e)
        {
            if (auditUserSearchTextBox != null)
            {
                auditUserSearchTextBox.Clear();
            }

            if (auditActionSearchTextBox != null)
            {
                auditActionSearchTextBox.Clear();
            }

            if (auditDateSearchPicker != null)
            {
                auditDateSearchPicker.Checked = false;
            }

            LoadAuditLogs();
        }

        private void RefreshAuditNotification()
        {
            if (auditNotificationButton == null)
            {
                return;
            }

            try
            {
                int count = AuditService.GetTodayActivityCount();
                auditBadgeLabel.Text = count > 99 ? "99+" : count.ToString("N0");
                auditBadgeLabel.Visible = count > 0 && auditNotificationButton.Visible;
            }
            catch
            {
                auditBadgeLabel.Text = "0";
                auditBadgeLabel.Visible = false;
            }
        }

        private void MarkAuditNotificationsSeen()
        {
            AuditService.MarkAllNotificationsAsRead();
            RefreshAuditNotification();
        }

        private void DeleteAuditButton_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanDeleteNotifications(AppSession.UserId, _role))
            {
                MessageBox.Show("You do not have permission to delete notifications.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (auditGrid.CurrentRow == null || auditGrid.CurrentRow.Cells["ID"].Value == null)
            {
                MessageBox.Show("Select an audit log row first.", "Audit Log");
                return;
            }

            DialogResult confirm = MessageBox.Show(
                "Delete this audit log entry?",
                "Audit Log",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            int auditLogId = Convert.ToInt32(auditGrid.CurrentRow.Cells["ID"].Value);
            AuditService.DeleteNotification(auditLogId);
            LoadAuditLogs();
            RefreshAuditNotification();
        }

        private static decimal GetDecimal(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];
            return value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        }

        private static int GetInt(SqlDataReader reader, string columnName)
        {
            object value = reader[columnName];
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static string FormatCurrency(decimal value)
        {
            return "$" + value.ToString("N2");
        }

        private void ApplyRoleAccess()
        {
            ApplyRolePermissions();
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
            btnAddSales.Text = "Disabled";
            btnProducts.Text = "Inventory";
            btnWarhouseReports.Text = "Warehouse";
            button11.Text = "Purchases";
            button12.Text = "Expenses";
            button8.Text = "Reports";
            btnAddUsers.Text = "Add Users";
            btnEarningReports.Text = "Earning Reports";
            btnsettings.Text = "Settings";
            panel3.Visible = true;
            panel2.Visible = true;
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

        private void button10_Click(object sender, EventArgs e)
        {
            bool canViewSettings = PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenSettings);
            bool canManageOptions = PermissionService.CanManagePermissions(AppSession.UserId, _role);

            if (!canViewSettings && !canManageOptions)
            {
                MessageBox.Show("You do not have permission to open settings.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ActivateMenuButton(btnsettings);
            SetWorkspaceTitle("Settings");
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

        private sealed class DashboardStats
        {
            public decimal TodaySales { get; set; }
            public int TodayInvoices { get; set; }
            public int ProductCount { get; set; }
            public decimal InventoryCapital { get; set; }
            public decimal TodayProfit { get; set; }
            public int CustomerCount { get; set; }
            public int LowStockCount { get; set; }
            public string TopProductName { get; set; }
        }

        private sealed class MetricCard
        {
            public MetricCard(TextBox backdrop, Label title, Label value, Label subtitle)
            {
                Backdrop = backdrop;
                Title = title;
                Value = value;
                Subtitle = subtitle;
            }

            public TextBox Backdrop { get; }
            public Label Title { get; }
            public Label Value { get; }
            public Label Subtitle { get; }
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

        private void customerBalanceTitleLabel_Click(object sender, EventArgs e)
        {

        }
    }
    }


