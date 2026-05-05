using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Pos_System.Services;
using Excel = Microsoft.Office.Interop.Excel;

namespace Pos_System.Forms
{
    public partial class ReportsForm : Form
    {
        private static readonly string ConnectionString = POS_System.Program.SettingsManager.ConnectionString;

        private readonly Dictionary<string, ReportDefinition> reportDefinitions =
            new Dictionary<string, ReportDefinition>(StringComparer.OrdinalIgnoreCase);

        private ComboBox comboReportType;
        private ComboBox comboUserFilter;
        private Button btnRefresh;
        private Label lblReportType;
        private Label lblUserFilter;
        private Label lblMainGridTitle;
        private Label lblHistoryTitle;

        private DataTable currentSummaryTable = new DataTable();
        private string currentReportKey = "sales_by_user";
        private string currentReportType = "sales_by_user";
        private string currentReportTitle = "Sales By User";
        private DateTime? currentStartDate;
        private DateTime? currentEndDateExclusive;

        public ReportsForm()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            btnsave.Click += btnsave_Click;
        }

        private void ReportsForm_Load(object sender, EventArgs e)
        {
            try
            {
                ConfigureReportControls();
                EnsureReportsTable();
                WorkHistoryService.EnsureHistoryTable();
                LoadReportsHistory();
                LoadSelectedReport("all");
            }
            catch (Exception ex)
            {
                ShowReportStartupError(ex);
            }
        }

        private void ConfigureReportControls()
        {
            Text = "Reports";
            labeldate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy  hh:mm tt", CultureInfo.InvariantCulture);

            datetimestart.Format = DateTimePickerFormat.Custom;
            datetimestart.CustomFormat = "dd/MM/yyyy";
            datetimeend.Format = DateTimePickerFormat.Custom;
            datetimeend.CustomFormat = "dd/MM/yyyy";

            DateTime today = DateTime.Today;
            datetimestart.Value = new DateTime(today.Year, today.Month, 1);
            datetimeend.Value = today;

            btndailyreport.Text = "اليومي";
            btnmonthlyreport.Text = "الشهري";
            btnsave.Text = "عرض";
            btnexcel.Text = "Excel";
            btndailyreport.Text = "\u0627\u0644\u064a\u0648\u0645\u064a";
            btnmonthlyreport.Text = "\u0627\u0644\u0634\u0647\u0631\u064a";
            btnsave.Text = "\u0639\u0631\u0636";
            label4.Text = "\u0645\u0646 \u062a\u0627\u0631\u064a\u062e";
            label5.Text = "\u0627\u0644\u0649 \u062a\u0627\u0631\u064a\u062e";
            label6.Text = "Total Sales";
            label8.Text = "Profit";
            label9.Text = "Top";

            ConfigureGrid(dataGridViewSummary);
            ConfigureGrid(dataGridViewReportsHistory);
            ConfigureSummaryCards();
            ConfigureDynamicControls();
            LoadUserFilter();
            BuildReportDefinitions();
            ResetSummaryCards();
            ApplyReportLayout();
            Resize -= ReportsForm_Resize;
            Resize += ReportsForm_Resize;
        }

        private void ReportsForm_Resize(object sender, EventArgs e)
        {
            ApplyReportLayout();
        }

        private void ConfigureGrid(DataGridView grid)
        {
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.RowHeadersVisible = false;
            grid.ColumnHeadersVisible = true;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            grid.ColumnHeadersHeight = 32;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.FixedSingle;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        }

        private void ConfigureSummaryCards()
        {
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            textBox3.ReadOnly = true;
            textBox1.BackColor = Color.White;
            textBox2.BackColor = Color.White;
            textBox3.BackColor = Color.White;
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox2.BorderStyle = BorderStyle.FixedSingle;
            textBox3.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            textBox2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            textBox3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }

        private void ConfigureDynamicControls()
        {
            if (comboReportType == null)
            {
                comboReportType = new ComboBox
                {
                    Name = "comboReportType",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Location = new Point(16, 64),
                    Size = new Size(244, 28)
                };

                panel1.Controls.Add(comboReportType);
            }

            if (lblReportType == null)
            {
                lblReportType = new Label
                {
                    Name = "lblReportType",
                    Text = "\u0646\u0648\u0639 \u0627\u0644\u062a\u0642\u0631\u064a\u0631",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    AutoSize = true
                };

                panel1.Controls.Add(lblReportType);
            }

            if (btnRefresh == null)
            {
                btnRefresh = new Button
                {
                    Name = "btnRefresh",
                    Text = "تحديث",
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Location = new Point(322, 64),
                    Size = new Size(112, 28)
                };
                btnRefresh.Text = "\u062a\u062d\u062f\u064a\u062b";
                btnRefresh.Click += (sender, args) => LoadSelectedReport("custom");
                panel1.Controls.Add(btnRefresh);
            }

            if (comboUserFilter == null)
            {
                comboUserFilter = new ComboBox
                {
                    Name = "comboUserFilter",
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    Location = new Point(450, 64),
                    Size = new Size(180, 28)
                };

                panel1.Controls.Add(comboUserFilter);
            }

            if (lblUserFilter == null)
            {
                lblUserFilter = new Label
                {
                    Name = "lblUserFilter",
                    Text = "User",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    AutoSize = true
                };

                panel1.Controls.Add(lblUserFilter);
            }

            if (lblMainGridTitle == null)
            {
                lblMainGridTitle = new Label
                {
                    Text = "نتائج التقرير",
                    Dock = DockStyle.Top,
                    Height = 28,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                panel2.Controls.Add(lblMainGridTitle);
                lblMainGridTitle.Text = "\u0646\u062a\u0627\u0626\u062c \u0627\u0644\u062a\u0642\u0631\u064a\u0631";
            }

            if (lblHistoryTitle == null)
            {
                lblHistoryTitle = new Label
                {
                    Text = "سجل التقارير",
                    Dock = DockStyle.Top,
                    Height = 28,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                panel2.Controls.Add(lblHistoryTitle);
                lblHistoryTitle.Text = "\u0633\u062c\u0644 \u0627\u0644\u062a\u0642\u0627\u0631\u064a\u0631";
            }
        }

        private void ApplyReportLayout()
        {
            if (panel1 == null || panel2 == null || comboReportType == null || comboUserFilter == null || btnRefresh == null)
            {
                return;
            }

            SuspendLayout();

            BackColor = Color.FromArgb(241, 245, 249);
            panelheader.Height = 60;
            panelheader.BackColor = Color.FromArgb(2, 6, 23);
            panel1.Height = 156;
            panel1.BackColor = Color.FromArgb(248, 250, 252);
            panel1.Padding = new Padding(12);
            panel2.BackColor = Color.FromArgb(226, 232, 240);
            panel2.Padding = new Padding(10);
            panel3.Height = 54;
            panel3.BackColor = Color.FromArgb(248, 250, 252);

            label3.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            label3.Location = new Point(14, 18);
            labeldate.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            labeldate.Location = new Point(Math.Max(720, panelheader.Width - 290), 20);
            label1.Font = new Font("Segoe UI", 18F, FontStyle.Bold | FontStyle.Italic);
            label2.Font = new Font("Segoe UI", 18F, FontStyle.Bold | FontStyle.Italic);
            label1.Location = new Point(Math.Max(340, panelheader.Width / 2 - 78), 12);
            label2.Location = new Point(label1.Right + 8, 12);

            int margin = 12;
            int labelTop = 8;
            int filterTop = 28;
            int pickerWidth = 150;
            int controlHeight = 32;
            int buttonWidth = 88;
            int comboWidth = Math.Max(190, Math.Min(280, panel1.Width - 1000));
            int userComboWidth = 170;

            label4.Location = new Point(margin, labelTop);
            datetimestart.Location = new Point(margin, filterTop);
            datetimestart.Size = new Size(pickerWidth, controlHeight);

            int x = datetimestart.Right + margin;
            label5.Location = new Point(x, labelTop);
            datetimeend.Location = new Point(x, filterTop);
            datetimeend.Size = new Size(pickerWidth, controlHeight);

            x = datetimeend.Right + margin;
            lblReportType.Location = new Point(x, labelTop);
            comboReportType.Location = new Point(x, filterTop);
            comboReportType.Size = new Size(comboWidth, controlHeight);

            x = comboReportType.Right + margin;
            lblUserFilter.Location = new Point(x, labelTop);
            comboUserFilter.Location = new Point(x, filterTop);
            comboUserFilter.Size = new Size(userComboWidth, controlHeight);

            x = comboUserFilter.Right + margin;
            btnRefresh.Location = new Point(x, filterTop);
            btnRefresh.Size = new Size(buttonWidth, controlHeight);
            btnsave.Location = new Point(btnRefresh.Right + 8, filterTop);
            btnsave.Size = new Size(buttonWidth, controlHeight);
            btndailyreport.Location = new Point(btnsave.Right + 8, filterTop);
            btndailyreport.Size = new Size(buttonWidth, controlHeight);
            btnmonthlyreport.Location = new Point(btndailyreport.Right + 8, filterTop);
            btnmonthlyreport.Size = new Size(buttonWidth, controlHeight);
            btnexcel.Location = new Point(btnmonthlyreport.Right + 8, filterTop);
            btnexcel.Size = new Size(buttonWidth, controlHeight);

            StyleReportButton(btnRefresh, Color.FromArgb(37, 99, 235));
            StyleReportButton(btnsave, Color.FromArgb(22, 163, 74));
            StyleReportButton(btndailyreport, Color.FromArgb(71, 85, 105));
            StyleReportButton(btnmonthlyreport, Color.FromArgb(71, 85, 105));
            StyleReportButton(btnexcel, Color.FromArgb(5, 150, 105));

            int cardTop = 76;
            int cardGap = 10;
            int cardWidth = Math.Max(180, (panel1.Width - margin * 2 - cardGap * 2) / 3);
            int cardHeight = 66;
            LayoutSummaryCard(textBox1, margin, cardTop, cardWidth, cardHeight);
            LayoutSummaryCard(textBox2, margin + cardWidth + cardGap, cardTop, cardWidth, cardHeight);
            LayoutSummaryCard(textBox3, margin + (cardWidth + cardGap) * 2, cardTop, cardWidth, cardHeight);

            label6.Visible = false;
            label8.Visible = false;
            label9.Visible = false;
            lbtotalsales.Visible = false;
            lbprofit.Visible = false;
            lbtopuser.Visible = false;

            panel3.Dock = DockStyle.Bottom;
            dataGridViewSummary.Dock = DockStyle.None;
            dataGridViewReportsHistory.Dock = DockStyle.None;
            lblMainGridTitle.Dock = DockStyle.None;
            lblHistoryTitle.Dock = DockStyle.None;

            if (lblMainGridTitle.Parent != panel2)
            {
                lblMainGridTitle.Parent.Controls.Remove(lblMainGridTitle);
                panel2.Controls.Add(lblMainGridTitle);
            }

            if (lblHistoryTitle.Parent != panel2)
            {
                lblHistoryTitle.Parent.Controls.Remove(lblHistoryTitle);
                panel2.Controls.Add(lblHistoryTitle);
            }

            int gridTop = 10;
            int gridHeight = Math.Max(220, panel2.ClientSize.Height - panel3.Height - 24);
            int gridGap = 10;
            int leftWidth = Math.Max(400, (panel2.ClientSize.Width - gridGap - 20) * 2 / 3);
            int rightWidth = Math.Max(300, panel2.ClientSize.Width - leftWidth - gridGap - 20);

            lblMainGridTitle.Location = new Point(10, gridTop);
            lblMainGridTitle.Size = new Size(0, 0);
            lblMainGridTitle.Visible = false;
            lblHistoryTitle.Location = new Point(lblMainGridTitle.Right + gridGap, gridTop);
            lblHistoryTitle.Size = new Size(0, 0);
            lblHistoryTitle.Visible = false;

            dataGridViewSummary.Location = new Point(10, gridTop);
            dataGridViewSummary.Size = new Size(leftWidth, gridHeight);
            dataGridViewReportsHistory.Location = new Point(dataGridViewSummary.Right + gridGap, gridTop);
            dataGridViewReportsHistory.Size = new Size(rightWidth, gridHeight);
            dataGridViewSummary.BringToFront();
            dataGridViewReportsHistory.BringToFront();
            dataGridViewSummary.ColumnHeadersVisible = true;
            dataGridViewReportsHistory.ColumnHeadersVisible = true;
            dataGridViewSummary.ColumnHeadersHeight = 32;
            dataGridViewReportsHistory.ColumnHeadersHeight = 32;

            lblMainGridTitle.Height = 0;
            lblHistoryTitle.Height = 0;

            ResumeLayout(true);
        }

        private void StyleReportButton(Button button, Color backColor)
        {
            if (button == null)
            {
                return;
            }

            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            button.TextAlign = ContentAlignment.MiddleCenter;
        }

        private void LayoutSummaryCard(TextBox card, int x, int y, int width, int height)
        {
            card.Location = new Point(x, y);
            card.Size = new Size(width, height);
            card.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            card.TextAlign = HorizontalAlignment.Left;
            card.BackColor = Color.White;
            card.ForeColor = Color.FromArgb(15, 23, 42);
            card.BorderStyle = BorderStyle.FixedSingle;
        }

        private void BuildReportDefinitions()
        {
            reportDefinitions.Clear();

            AddReport("sales_by_user", "المبيعات حسب المستخدم", BuildSalesByUserQuery());
            AddReport("sales_by_product", "المبيعات حسب المنتج", BuildSalesByProductQuery());
            AddReport("daily_sales", "المبيعات اليومية", BuildDailySalesQuery());
            AddReport("invoice_details", "تفاصيل الفواتير", BuildInvoiceDetailsQuery());
            AddReport("returns", "المرتجعات", BuildReturnsQuery());
            AddReport("customer_balances", "أرصدة الزبائن", BuildCustomerBalancesQuery());
            AddReport("low_stock", "المخزون المنخفض", BuildLowStockQuery());

            AddReport("sales_by_user", "\u0627\u0644\u0645\u0628\u064a\u0639\u0627\u062a \u062d\u0633\u0628 \u0627\u0644\u0645\u0633\u062a\u062e\u062f\u0645", BuildSalesByUserQuery());
            AddReport("sales_by_product", "\u0627\u0644\u0645\u0628\u064a\u0639\u0627\u062a \u062d\u0633\u0628 \u0627\u0644\u0645\u0646\u062a\u062c", BuildSalesByProductQuery());
            AddReport("daily_sales", "\u0627\u0644\u0645\u0628\u064a\u0639\u0627\u062a \u0627\u0644\u064a\u0648\u0645\u064a\u0629", BuildDailySalesQuery());
            AddReport("invoice_details", "\u062a\u0641\u0627\u0635\u064a\u0644 \u0627\u0644\u0641\u0648\u0627\u062a\u064a\u0631", BuildInvoiceDetailsQuery());
            AddReport("returns", "\u0627\u0644\u0645\u0631\u062a\u062c\u0639\u0627\u062a", BuildReturnsQuery());
            AddReport("customer_balances", "\u0623\u0631\u0635\u062f\u0629 \u0627\u0644\u0632\u0628\u0627\u0626\u0646", BuildCustomerBalancesQuery());
            AddReport("low_stock", "\u0627\u0644\u0645\u062e\u0632\u0648\u0646 \u0627\u0644\u0645\u0646\u062e\u0641\u0636", BuildLowStockQuery());
            AddReport("work_history", "Work History", BuildWorkHistoryQuery());

            comboReportType.DataSource = new BindingSource(reportDefinitions, null);
            comboReportType.DisplayMember = "Value";
            comboReportType.ValueMember = "Key";
            comboReportType.SelectedValue = currentReportKey;
        }

        private void AddReport(string key, string title, string query)
        {
            reportDefinitions[key] = new ReportDefinition(key, title, query);
        }

        private void LoadUserFilter()
        {
            if (comboUserFilter == null)
            {
                return;
            }

            string selectedUsername = GetSelectedUserFilter();
            List<UserFilterItem> users = new List<UserFilterItem>
            {
                new UserFilterItem(string.Empty, "All Users")
            };

            try
            {
                DataTable userTable = ExecuteDataTable(@"
                    SELECT DISTINCT username
                    FROM Users
                    WHERE username IS NOT NULL AND LTRIM(RTRIM(username)) <> N''
                    ORDER BY username;");

                foreach (DataRow row in userTable.Rows)
                {
                    string username = row["username"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(username))
                    {
                        users.Add(new UserFilterItem(username.Trim(), username.Trim()));
                    }
                }
            }
            catch
            {
                // Reports can still run without a populated user filter.
            }

            comboUserFilter.DataSource = users;
            comboUserFilter.DisplayMember = "DisplayName";
            comboUserFilter.ValueMember = "Username";

            if (!string.IsNullOrWhiteSpace(selectedUsername) && users.Any(user => user.Username.Equals(selectedUsername, StringComparison.OrdinalIgnoreCase)))
            {
                comboUserFilter.SelectedValue = selectedUsername;
            }
            else
            {
                comboUserFilter.SelectedIndex = 0;
            }
        }

        private string GetSelectedUserFilter()
        {
            if (comboUserFilter == null)
            {
                return string.Empty;
            }

            object selectedValue = comboUserFilter.SelectedValue;
            if (selectedValue is UserFilterItem selectedItem)
            {
                return selectedItem.Username;
            }

            return selectedValue == null ? string.Empty : selectedValue.ToString();
        }

        private void LoadSelectedReport(string rangeMode)
        {
            try
            {
                DateTime? startDate;
                DateTime? endDateExclusive;
                GetDateRange(rangeMode, out startDate, out endDateExclusive);

                string selectedKey = comboReportType?.SelectedValue?.ToString();
                if (string.IsNullOrWhiteSpace(selectedKey) || !reportDefinitions.ContainsKey(selectedKey))
                {
                    selectedKey = "sales_by_user";
                }

                ReportDefinition report = reportDefinitions[selectedKey];
                LoadReport(report, startDate, endDateExclusive);
            }
            catch (Exception ex)
            {
                MessageBox.Show("تعذر تحميل التقرير.\n" + ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetDateRange(string rangeMode, out DateTime? startDate, out DateTime? endDateExclusive)
        {
            DateTime today = DateTime.Today;
            startDate = null;
            endDateExclusive = null;

            if (string.Equals(rangeMode, "daily", StringComparison.OrdinalIgnoreCase))
            {
                startDate = datetimestart.Value.Date;
                endDateExclusive = startDate.Value.AddDays(1);
                return;
            }

            if (string.Equals(rangeMode, "monthly", StringComparison.OrdinalIgnoreCase))
            {
                startDate = new DateTime(datetimestart.Value.Year, datetimestart.Value.Month, 1);
                endDateExclusive = startDate.Value.AddMonths(1);
                return;
            }

            if (string.Equals(rangeMode, "custom", StringComparison.OrdinalIgnoreCase))
            {
                if (datetimeend.Value.Date < datetimestart.Value.Date)
                {
                    throw new InvalidOperationException("تاريخ النهاية يجب أن يكون بعد تاريخ البداية.");
                }

                startDate = datetimestart.Value.Date;
                endDateExclusive = datetimeend.Value.Date.AddDays(1);
                return;
            }

            if (string.Equals(rangeMode, "all", StringComparison.OrdinalIgnoreCase))
            {
                datetimestart.Value = new DateTime(today.Year, today.Month, 1);
                datetimeend.Value = today;
            }
        }

        private void LoadReport(ReportDefinition report, DateTime? startDate, DateTime? endDateExclusive)
        {
            if (string.Equals(report.Key, "work_history", StringComparison.OrdinalIgnoreCase))
            {
                WorkHistoryService.EnsureHistoryTable();
            }

            DataTable reportTable = ExecuteDataTable(
                report.Query,
                CreateDateTimeParameter("@StartDate", startDate),
                CreateDateTimeParameter("@EndDateExclusive", endDateExclusive),
                CreateParameter("@UserFilter", GetSelectedUserFilter()),
                CreateParameter("@LowStockThreshold", POS_System.Program.SettingsManager.GetIntSetting("low_stock_threshold", 5)));

            currentSummaryTable = reportTable;
            currentReportKey = report.Key;
            currentReportType = report.Key;
            currentReportTitle = report.Title;
            currentStartDate = startDate;
            currentEndDateExclusive = endDateExclusive;

            dataGridViewSummary.DataSource = currentSummaryTable;
            FormatSummaryGrid();

            if (string.Equals(report.Key, "work_history", StringComparison.OrdinalIgnoreCase))
            {
                UpdateWorkHistorySummaryCards(report.Title, startDate, endDateExclusive, currentSummaryTable);
            }
            else
            {
                UpdateSummaryCards(report.Title, startDate, endDateExclusive, currentSummaryTable);
            }
        }

        private void FormatSummaryGrid()
        {
            foreach (DataGridViewColumn column in dataGridViewSummary.Columns)
            {
                string name = column.Name.ToLowerInvariant();
                if (name.Contains("sales") || name.Contains("profit") || name.Contains("price") ||
                    name.Contains("amount") || name.Contains("balance") || name.Contains("total"))
                {
                    column.DefaultCellStyle.Format = "N2";
                }

                if (name.Contains("date") || name.Contains("created") || name.Contains("time"))
                {
                    column.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
            }
        }

        private void UpdateWorkHistorySummaryCards(string reportTitle, DateTime? startDate, DateTime? endDateExclusive, DataTable table)
        {
            decimal totalMinutes = SumColumn(table, "Total Minutes");
            int totalHours = ToInt(Math.Floor(totalMinutes / 60m));
            int remainingMinutes = ToInt(totalMinutes % 60m);
            int completedRows = table.Select("[End Time] IS NOT NULL").Length;

            lbtotalsales.Text = totalHours + "h " + remainingMinutes + "m";
            lbprofit.Text = "Rows: " + table.Rows.Count;
            lbtopuser.Text = GetTopValue(table);

            textBox1.Text = reportTitle + Environment.NewLine + BuildDateRangeText(startDate, endDateExclusive);
            textBox2.Text = "Users: " + CountDistinctValues(table, "User Name") + Environment.NewLine +
                            "Completed: " + completedRows + Environment.NewLine +
                            "Open: " + Math.Max(0, table.Rows.Count - completedRows);
            textBox3.Text = "Total Worked: " + totalHours + " hour(s), " + remainingMinutes + " minute(s)" + Environment.NewLine +
                            "Generated By: " + GetCurrentUsername() + Environment.NewLine +
                            "Generated At: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private void UpdateSummaryCards(string reportTitle, DateTime? startDate, DateTime? endDateExclusive, DataTable table)
        {
            decimal totalSales = SumColumn(table, "Total Sales");
            if (totalSales == 0m)
            {
                totalSales = SumColumn(table, "Amount");
            }

            decimal totalProfit = SumColumn(table, "Profit");
            int invoices = ToInt(SumColumn(table, "Invoices"));
            int items = ToInt(SumColumn(table, "Items Sold"));

            if (invoices == 0)
            {
                invoices = table.Columns.Contains("Invoice ID") ? table.Rows.Count : 0;
            }

            string topValue = GetTopValue(table);
            decimal averageSale = invoices > 0 ? totalSales / invoices : 0m;

            lbtotalsales.Text = "$" + totalSales.ToString("N2");
            lbprofit.Text = "$" + totalProfit.ToString("N2");
            lbtopuser.Text = string.IsNullOrWhiteSpace(topValue) ? "No Data" : topValue;

            textBox1.Text = reportTitle + Environment.NewLine + BuildDateRangeText(startDate, endDateExclusive);
            textBox2.Text = "Rows: " + table.Rows.Count + Environment.NewLine +
                            "Invoices: " + invoices + Environment.NewLine +
                            "Items Sold: " + items;
            textBox3.Text = "Average Sale: $" + averageSale.ToString("N2") + Environment.NewLine +
                            "Generated By: " + GetCurrentUsername() + Environment.NewLine +
                            "Generated At: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        private string GetTopValue(DataTable table)
        {
            if (table.Rows.Count == 0)
            {
                return string.Empty;
            }

            string[] preferredColumns = { "User Name", "Product", "Customer", "Day", "Invoice ID" };
            foreach (string columnName in preferredColumns)
            {
                if (table.Columns.Contains(columnName))
                {
                    return table.Rows[0][columnName]?.ToString();
                }
            }

            return table.Rows[0][0]?.ToString();
        }

        private int CountDistinctValues(DataTable table, string columnName)
        {
            if (table == null || !table.Columns.Contains(columnName))
            {
                return 0;
            }

            HashSet<string> values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in table.Rows)
            {
                string value = row[columnName]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add(value);
                }
            }

            return values.Count;
        }

        private decimal SumColumn(DataTable table, string columnName)
        {
            if (table == null || !table.Columns.Contains(columnName))
            {
                return 0m;
            }

            decimal total = 0m;
            foreach (DataRow row in table.Rows)
            {
                total += ConvertToDecimal(row[columnName]);
            }

            return total;
        }

        private int ToInt(decimal value)
        {
            if (value > int.MaxValue)
            {
                return int.MaxValue;
            }

            if (value < int.MinValue)
            {
                return int.MinValue;
            }

            return Convert.ToInt32(value);
        }

        private void ResetSummaryCards()
        {
            lbtotalsales.Text = "$0.00";
            lbprofit.Text = "$0.00";
            lbtopuser.Text = "No Data";
            textBox1.Text = "اختر نوع التقرير واضغط عرض.";
            textBox2.Text = "Rows: 0";
            textBox3.Text = "Generated By: " + GetCurrentUsername();
        }

        private void ShowReportStartupError(Exception ex)
        {
            ResetSummaryCards();
            dataGridViewSummary.DataSource = null;
            dataGridViewReportsHistory.DataSource = null;
            MessageBox.Show("تعذر فتح شاشة التقارير.\n" + ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void EnsureReportsTable()
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                IF OBJECT_ID(N'dbo.Reports', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.Reports
                    (
                        report_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        report_type NVARCHAR(100) NOT NULL,
                        generated_by INT NULL,
                        file_path NVARCHAR(500) NULL,
                        created_at DATETIME2 NOT NULL CONSTRAINT DF_Reports_created_at DEFAULT (GETDATE())
                    );
                END;

                IF COL_LENGTH('dbo.Reports', 'report_id') IS NULL
                    ALTER TABLE dbo.Reports ADD report_id INT IDENTITY(1,1) NOT NULL;

                IF COL_LENGTH('dbo.Reports', 'report_type') IS NULL
                    ALTER TABLE dbo.Reports ADD report_type NVARCHAR(100) NOT NULL CONSTRAINT DF_Reports_report_type DEFAULT ('custom');

                IF COL_LENGTH('dbo.Reports', 'generated_by') IS NULL
                    ALTER TABLE dbo.Reports ADD generated_by INT NULL;

                IF COL_LENGTH('dbo.Reports', 'file_path') IS NULL
                    ALTER TABLE dbo.Reports ADD file_path NVARCHAR(500) NULL;

                IF COL_LENGTH('dbo.Reports', 'created_at') IS NULL
                    ALTER TABLE dbo.Reports ADD created_at DATETIME2 NOT NULL CONSTRAINT DF_Reports_created_at DEFAULT (GETDATE());", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private string BuildDateRangeText(DateTime? startDate, DateTime? endDateExclusive)
        {
            if (!startDate.HasValue && !endDateExclusive.HasValue)
            {
                return "Date Range: All available sales";
            }

            if (startDate.HasValue && endDateExclusive.HasValue)
            {
                DateTime displayEndDate = endDateExclusive.Value.AddDays(-1);
                return "Date Range: " + startDate.Value.ToString("dd MMM yyyy") + " - " +
                       displayEndDate.ToString("dd MMM yyyy");
            }

            if (startDate.HasValue)
            {
                return "From: " + startDate.Value.ToString("dd MMM yyyy");
            }

            return "Until: " + endDateExclusive.Value.AddDays(-1).ToString("dd MMM yyyy");
        }

        private void SaveReport(string reportType, string filePath)
        {
            try
            {
                EnsureReportsTable();
                int? userId = GetCurrentUserId();

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                using (SqlCommand command = new SqlCommand(
                    @"INSERT INTO Reports (report_type, generated_by, file_path)
                      VALUES (@reportType, @generatedBy, @filePath);", connection))
                {
                    command.Parameters.Add("@reportType", SqlDbType.NVarChar, 100).Value = reportType ?? "custom";
                    command.Parameters.Add("@generatedBy", SqlDbType.Int).Value = userId.HasValue ? (object)userId.Value : DBNull.Value;
                    command.Parameters.Add("@filePath", SqlDbType.NVarChar, 500).Value =
                        string.IsNullOrWhiteSpace(filePath) ? (object)DBNull.Value : filePath;

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                AuditLogger.Log("ADD", "Reports", null, "Saved report: " + reportType + " / " + filePath);
            }
            catch
            {
                // Report history should never block the report screen itself.
            }
        }

        private int? GetCurrentUserId()
        {
            string username = GetCurrentUsername();
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            object result = ExecuteScalar(
                "SELECT TOP 1 user_id FROM Users WHERE username = @username;",
                CreateParameter("@username", username));

            if (result == null || result == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(result);
        }

        private string GetCurrentUsername()
        {
            return string.IsNullOrWhiteSpace(LoginForm.LoggedInUsername) ? "System" : LoginForm.LoggedInUsername;
        }

        private object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteScalar();
            }
        }

        private DataTable ExecuteDataTable(string query, params SqlParameter[] parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(query, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.CommandTimeout = 60;

                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        private SqlParameter CreateParameter(string parameterName, object value)
        {
            return new SqlParameter(parameterName, value ?? DBNull.Value);
        }

        private SqlParameter CreateDateTimeParameter(string parameterName, DateTime? value)
        {
            return new SqlParameter(parameterName, SqlDbType.DateTime2)
            {
                Value = value.HasValue ? (object)value.Value : DBNull.Value
            };
        }

        private decimal ConvertToDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0m;
            }

            decimal convertedValue;
            return decimal.TryParse(value.ToString(), out convertedValue) ? convertedValue : 0m;
        }

        private int ConvertToInt(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return 0;
            }

            int convertedValue;
            return int.TryParse(value.ToString(), out convertedValue) ? convertedValue : 0;
        }

        private void btndailyreport_Click(object sender, EventArgs e)
        {
            LoadSelectedReport("daily");
            SaveReport(currentReportType + "_daily", "Screen Report - " + datetimestart.Value.ToString("yyyy-MM-dd"));
            LoadReportsHistory();
        }

        private void btnmonthlyreport_Click(object sender, EventArgs e)
        {
            LoadSelectedReport("monthly");
            SaveReport(currentReportType + "_monthly", "Screen Report - " + datetimestart.Value.ToString("yyyy-MM"));
            LoadReportsHistory();
        }

        private void btnsave_Click(object sender, EventArgs e)
        {
            LoadSelectedReport("custom");
            SaveReport(
                currentReportType + "_custom",
                "Screen Report - " + datetimestart.Value.ToString("yyyy-MM-dd") + "_to_" + datetimeend.Value.ToString("yyyy-MM-dd"));
            LoadReportsHistory();
        }

        private void btnexcel_Click(object sender, EventArgs e)
        {
            if (currentSummaryTable == null || currentSummaryTable.Rows.Count == 0)
            {
                MessageBox.Show("There is no report data to export.", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Title = "Export Report";
                saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                saveDialog.FileName = BuildDefaultExportFileName();

                if (saveDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    ExportCurrentReportToExcel(saveDialog.FileName);
                    SaveReport(currentReportType + "_excel", saveDialog.FileName);
                    LoadReportsHistory();
                    MessageBox.Show("The report was exported successfully.", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to export the report.\n" + ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string BuildDefaultExportFileName()
        {
            string safeReportType = currentReportType.Replace(" ", "_");
            return "Report_" + safeReportType + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
        }

        private void ExportCurrentReportToExcel(string filePath)
        {
            Excel.Application excelApplication = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excelApplication = new Excel.Application();
                workbook = excelApplication.Workbooks.Add(Type.Missing);
                worksheet = (Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Reports";

                worksheet.Cells[1, 1] = "Report Title";
                worksheet.Cells[1, 2] = currentReportTitle;
                worksheet.Cells[2, 1] = "Generated By";
                worksheet.Cells[2, 2] = GetCurrentUsername();
                worksheet.Cells[3, 1] = "Generated At";
                worksheet.Cells[3, 2] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[4, 1] = "Date Range";
                worksheet.Cells[4, 2] = BuildDateRangeText(currentStartDate, currentEndDateExclusive);
                worksheet.Cells[5, 1] = "Total Sales";
                worksheet.Cells[5, 2] = lbtotalsales.Text;
                worksheet.Cells[6, 1] = "Profit";
                worksheet.Cells[6, 2] = lbprofit.Text;
                worksheet.Cells[7, 1] = "Top";
                worksheet.Cells[7, 2] = lbtopuser.Text;

                int headerRow = 9;
                for (int columnIndex = 0; columnIndex < currentSummaryTable.Columns.Count; columnIndex++)
                {
                    worksheet.Cells[headerRow, columnIndex + 1] = currentSummaryTable.Columns[columnIndex].ColumnName;
                }

                for (int rowIndex = 0; rowIndex < currentSummaryTable.Rows.Count; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < currentSummaryTable.Columns.Count; columnIndex++)
                    {
                        worksheet.Cells[rowIndex + headerRow + 1, columnIndex + 1] =
                            currentSummaryTable.Rows[rowIndex][columnIndex] == DBNull.Value
                                ? string.Empty
                                : currentSummaryTable.Rows[rowIndex][columnIndex].ToString();
                    }
                }

                worksheet.Columns.AutoFit();

                string directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                workbook.SaveAs(filePath);
                workbook.Close();
                excelApplication.Quit();
            }
            finally
            {
                ReleaseComObject(worksheet);
                ReleaseComObject(workbook);
                ReleaseComObject(excelApplication);
            }
        }

        private void ReleaseComObject(object comObject)
        {
            if (comObject != null && Marshal.IsComObject(comObject))
            {
                Marshal.ReleaseComObject(comObject);
            }
        }

        private void LoadReportsHistory()
        {
            try
            {
                EnsureReportsTable();
                DataTable historyTable = ExecuteDataTable(
                    @"SELECT
                          TRY_CONVERT(INT, r.report_id) AS [ID],
                          TRY_CONVERT(NVARCHAR(100), r.report_type) AS [Report Type],
                          ISNULL(u.username, 'Unknown') AS [Generated By],
                          TRY_CONVERT(DATETIME2, r.created_at) AS [Created At],
                          TRY_CONVERT(NVARCHAR(500), r.file_path) AS [File Path]
                      FROM Reports r
                      LEFT JOIN Users u ON TRY_CONVERT(INT, r.generated_by) = u.user_id
                      ORDER BY TRY_CONVERT(DATETIME2, r.created_at) DESC;");

                dataGridViewReportsHistory.DataSource = historyTable;

                if (dataGridViewReportsHistory.Columns.Contains("Created At"))
                {
                    dataGridViewReportsHistory.Columns["Created At"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }
            }
            catch
            {
                dataGridViewReportsHistory.DataSource = null;
            }
        }

        private string BuildSalesByUserQuery()
        {
            return @"
                SELECT
                    ISNULL(u.username, 'Unknown') AS [User Name],
                    COUNT(DISTINCT s.sale_id) AS [Invoices],
                    SUM(ISNULL(TRY_CONVERT(INT, si.quantity), 0)) AS [Items Sold],
                    CAST(SUM(ISNULL(TRY_CONVERT(DECIMAL(38,4), s.total_amount), 0)) AS DECIMAL(38,2)) AS [Total Sales],
                    CAST(SUM(
                        (ISNULL(TRY_CONVERT(DECIMAL(19,4), si.unit_price), 0) -
                         ISNULL(TRY_CONVERT(DECIMAL(19,4), p.price_usd), 0)) *
                        ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)
                    ) AS DECIMAL(38,2)) AS [Profit]
                FROM Sales s
                LEFT JOIN Users u ON TRY_CONVERT(INT, s.user_id) = u.user_id
                LEFT JOIN Sale_Items si ON s.sale_id = si.sale_id
                LEFT JOIN Products p ON si.product_id = p.product_id
                WHERE (@StartDate IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) >= @StartDate)
                  AND (@EndDateExclusive IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) < @EndDateExclusive)
                  AND (@UserFilter = N'' OR ISNULL(u.username, N'Unknown') = @UserFilter)
                  AND ISNULL(TRY_CONVERT(BIT, s.is_returned), 0) = 0
                GROUP BY ISNULL(u.username, 'Unknown')
                ORDER BY [Total Sales] DESC;";
        }

        private string BuildSalesByProductQuery()
        {
            return @"
                SELECT
                    ISNULL(p.name, si.name_product) AS [Product],
                    COUNT(DISTINCT si.sale_id) AS [Invoices],
                    SUM(ISNULL(TRY_CONVERT(INT, si.quantity), 0)) AS [Items Sold],
                    CAST(AVG(ISNULL(TRY_CONVERT(DECIMAL(38,4), si.unit_price), 0)) AS DECIMAL(38,2)) AS [Avg Price],
                    CAST(SUM(ISNULL(TRY_CONVERT(DECIMAL(38,4), si.unit_price), 0) *
                             ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)) AS DECIMAL(38,2)) AS [Total Sales],
                    CAST(SUM(
                        (ISNULL(TRY_CONVERT(DECIMAL(19,4), si.unit_price), 0) -
                         ISNULL(TRY_CONVERT(DECIMAL(19,4), p.price_usd), 0)) *
                        ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)
                    ) AS DECIMAL(38,2)) AS [Profit]
                FROM Sale_Items si
                LEFT JOIN Sales s ON s.sale_id = si.sale_id
                LEFT JOIN Users u ON TRY_CONVERT(INT, s.user_id) = u.user_id
                LEFT JOIN Products p ON p.product_id = si.product_id
                WHERE (@StartDate IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) >= @StartDate)
                  AND (@EndDateExclusive IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) < @EndDateExclusive)
                  AND (@UserFilter = N'' OR ISNULL(u.username, N'Unknown') = @UserFilter)
                  AND ISNULL(TRY_CONVERT(BIT, s.is_returned), 0) = 0
                GROUP BY ISNULL(p.name, si.name_product)
                ORDER BY [Items Sold] DESC;";
        }

        private string BuildDailySalesQuery()
        {
            return @"
                SELECT
                    CONVERT(DATE, TRY_CONVERT(DATETIME2, s.sale_date)) AS [Day],
                    COUNT(DISTINCT s.sale_id) AS [Invoices],
                    SUM(ISNULL(TRY_CONVERT(INT, si.quantity), 0)) AS [Items Sold],
                    CAST(SUM(ISNULL(TRY_CONVERT(DECIMAL(38,4), s.total_amount), 0)) AS DECIMAL(38,2)) AS [Total Sales],
                    CAST(SUM(
                        (ISNULL(TRY_CONVERT(DECIMAL(19,4), si.unit_price), 0) -
                         ISNULL(TRY_CONVERT(DECIMAL(19,4), p.price_usd), 0)) *
                        ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)
                    ) AS DECIMAL(38,2)) AS [Profit]
                FROM Sales s
                LEFT JOIN Sale_Items si ON s.sale_id = si.sale_id
                LEFT JOIN Users u ON TRY_CONVERT(INT, s.user_id) = u.user_id
                LEFT JOIN Products p ON si.product_id = p.product_id
                WHERE TRY_CONVERT(DATETIME2, s.sale_date) IS NOT NULL
                  AND (@StartDate IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) >= @StartDate)
                  AND (@EndDateExclusive IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) < @EndDateExclusive)
                  AND (@UserFilter = N'' OR ISNULL(u.username, N'Unknown') = @UserFilter)
                  AND ISNULL(TRY_CONVERT(BIT, s.is_returned), 0) = 0
                GROUP BY CONVERT(DATE, TRY_CONVERT(DATETIME2, s.sale_date))
                ORDER BY [Day] DESC;";
        }

        private string BuildInvoiceDetailsQuery()
        {
            return @"
                SELECT
                    s.sale_id AS [Invoice ID],
                    TRY_CONVERT(DATETIME2, s.sale_date) AS [Sale Date],
                    ISNULL(s.customer_name, c.name) AS [Customer],
                    ISNULL(u.username, s.created_by) AS [User Name],
                    TRY_CONVERT(NVARCHAR(50), s.payment_method) AS [Payment],
                    ISNULL(TRY_CONVERT(INT, s.quantity), 0) AS [Items Sold],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(38,4), s.total_amount), 0) AS DECIMAL(38,2)) AS [Total Sales],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(38,4), s.balance_usd), 0) AS DECIMAL(38,2)) AS [Balance USD],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(38,4), s.balance_lb), 0) AS DECIMAL(38,2)) AS [Balance LB],
                    CASE WHEN ISNULL(TRY_CONVERT(BIT, s.is_returned), 0) = 1 THEN N'Returned' ELSE ISNULL(s.status, N'Active') END AS [Status]
                FROM Sales s
                LEFT JOIN Users u ON TRY_CONVERT(INT, s.user_id) = u.user_id
                LEFT JOIN Customers c ON TRY_CONVERT(INT, s.customer_id) = c.customer_id
                WHERE (@StartDate IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) >= @StartDate)
                  AND (@EndDateExclusive IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) < @EndDateExclusive)
                  AND (@UserFilter = N'' OR COALESCE(u.username, s.created_by, N'Unknown') = @UserFilter)
                ORDER BY TRY_CONVERT(DATETIME2, s.sale_date) DESC, s.sale_id DESC;";
        }

        private string BuildReturnsQuery()
        {
            return @"
                SELECT
                    r.return_id AS [Return ID],
                    r.sale_id AS [Invoice ID],
                    ISNULL(p.name, N'Unknown') AS [Product],
                    ISNULL(TRY_CONVERT(INT, r.quantity), 0) AS [Items Sold],
                    TRY_CONVERT(DATETIME2, r.return_date) AS [Return Date]
                FROM Returns r
                LEFT JOIN Products p ON TRY_CONVERT(INT, r.product_id) = p.product_id
                WHERE (@StartDate IS NULL OR TRY_CONVERT(DATETIME2, r.return_date) >= @StartDate)
                  AND (@EndDateExclusive IS NULL OR TRY_CONVERT(DATETIME2, r.return_date) < @EndDateExclusive)
                ORDER BY TRY_CONVERT(DATETIME2, r.return_date) DESC, r.return_id DESC;";
        }

        private string BuildCustomerBalancesQuery()
        {
            return @"
                SELECT
                    c.name AS [Customer],
                    TRY_CONVERT(NVARCHAR(50), c.phone) AS [Phone],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(38,4), c.balance_usd), 0) AS DECIMAL(38,2)) AS [Balance USD],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(38,4), c.balance_lb), 0) AS DECIMAL(38,2)) AS [Balance LB],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(38,4), c.balance), 0) AS DECIMAL(38,2)) AS [Amount],
                    TRY_CONVERT(DATETIME2, c.balance_updated_at) AS [Updated At]
                FROM Customers c
                ORDER BY ABS(ISNULL(TRY_CONVERT(DECIMAL(38,4), c.balance_usd), 0)) +
                         ABS(ISNULL(TRY_CONVERT(DECIMAL(38,4), c.balance_lb), 0)) DESC;";
        }

        private string BuildLowStockQuery()
        {
            return @"
                SELECT
                    p.product_id AS [Product ID],
                    p.name AS [Product],
                    TRY_CONVERT(NVARCHAR(100), p.barcode) AS [Barcode],
                    ISNULL(TRY_CONVERT(INT, p.stock_quantity), 0) AS [Stock],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(38,4), p.price_usd), 0) AS DECIMAL(38,2)) AS [Cost USD],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(38,4), p.sale_price_usd), 0) AS DECIMAL(38,2)) AS [Sale Price USD],
                    TRY_CONVERT(DATETIME2, p.created_at) AS [Created At]
                FROM Products p
                WHERE ISNULL(TRY_CONVERT(INT, p.stock_quantity), 0) <= @LowStockThreshold
                ORDER BY ISNULL(TRY_CONVERT(INT, p.stock_quantity), 0), p.name;";
        }

        private string BuildWorkHistoryQuery()
        {
            return @"
                SELECT
                    h.history_id AS [ID],
                    ISNULL(u.username, h.username) AS [User Name],
                    TRY_CONVERT(DATE, h.work_date) AS [Work Date],
                    TRY_CONVERT(DATETIME2, h.start_time) AS [Start Time],
                    TRY_CONVERT(DATETIME2, h.end_time) AS [End Time],
                    ISNULL(TRY_CONVERT(INT, h.worked_hours), 0) AS [Hours],
                    ISNULL(TRY_CONVERT(INT, h.worked_minutes), 0) AS [Minutes],
                    ISNULL(TRY_CONVERT(INT, h.worked_duration_minutes), 0) AS [Total Minutes],
                    CASE WHEN h.end_time IS NULL THEN N'Open' ELSE N'Closed' END AS [Status]
                FROM dbo.History h
                LEFT JOIN Users u ON TRY_CONVERT(INT, h.user_id) = u.user_id
                WHERE (@StartDate IS NULL OR TRY_CONVERT(DATETIME2, h.start_time) >= @StartDate)
                  AND (@EndDateExclusive IS NULL OR TRY_CONVERT(DATETIME2, h.start_time) < @EndDateExclusive)
                  AND (@UserFilter = N'' OR ISNULL(u.username, h.username) = @UserFilter)
                ORDER BY TRY_CONVERT(DATETIME2, h.start_time) DESC, h.history_id DESC;";
        }

        private sealed class ReportDefinition
        {
            public ReportDefinition(string key, string title, string query)
            {
                Key = key;
                Title = title;
                Query = query;
            }

            public string Key { get; }
            public string Title { get; }
            public string Query { get; }

            public override string ToString()
            {
                return Title;
            }
        }

        private sealed class UserFilterItem
        {
            public UserFilterItem(string username, string displayName)
            {
                Username = username;
                DisplayName = displayName;
            }

            public string Username { get; }
            public string DisplayName { get; }
        }
    }
}
