using ClosedXML.Excel;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public partial class EarningReports : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private DataTable currentReportTable;
        private ComboBox comboReportMode;
        private TextBox txtSearch;
        private Label lblReportMode;
        private Label lblSearch;

        public EarningReports()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            ConfigureForm();
        }

        private void EarningReports_Load(object sender, EventArgs e)
        {
            try
            {
                AuditLogger.EnsureSalesColumns();
                LoadUsers();
                LoadReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open earning reports: " + ex.Message, "Earning Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureForm()
        {
            MinimumSize = new Size(980, 560);
            BackColor = Color.FromArgb(244, 247, 252);

            panelheader.BackColor = Color.FromArgb(15, 23, 42);
            panel1.BackColor = Color.White;
            panel2.BackColor = Color.FromArgb(244, 247, 252);
            panel3.BackColor = Color.White;

            panel1.Dock = DockStyle.Top;
            panel2.Dock = DockStyle.Top;
            panel3.Dock = DockStyle.Fill;

            dtpStart.ShowCheckBox = true;
            dtpEnd.ShowCheckBox = true;
            dtpStart.Checked = true;
            dtpEnd.Checked = true;
            dtpStart.Value = DateTime.Today;
            dtpEnd.Value = DateTime.Today;

            combouser.DropDownStyle = ComboBoxStyle.DropDownList;

            AddAdvancedFilterControls();

            StyleButton(btnadd, Color.FromArgb(37, 99, 235), "بحث");
            StyleButton(btnclear, Color.FromArgb(100, 116, 139), "مسح");
            StyleButton(btnexcel, Color.FromArgb(22, 163, 74), "Excel");
            StyleTextBox(txtSearch);
            StyleGrid();
            StyleSummaryCards();

            txtdollar.Visible = false;
            label7.Visible = false;

            panel1.Resize += (sender, args) => LayoutFilterPanel();
            panel2.Resize += (sender, args) => LayoutSummaryCards();
            Resize += (sender, args) =>
            {
                LayoutFilterPanel();
                LayoutSummaryCards();
            };
        }

        private void AddAdvancedFilterControls()
        {
            if (comboReportMode == null)
            {
                comboReportMode = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
                };
                comboReportMode.Items.AddRange(new object[]
                {
                    "تفصيلي",
                    "حسب البائع",
                    "حسب المنتج",
                    "حسب اليوم"
                });
                comboReportMode.SelectedIndex = 0;
                panel1.Controls.Add(comboReportMode);
            }

            if (txtSearch == null)
            {
                txtSearch = new TextBox
                {
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular)
                };
                txtSearch.KeyDown += (sender, args) =>
                {
                    if (args.KeyCode == Keys.Enter)
                    {
                        args.SuppressKeyPress = true;
                        LoadReport();
                    }
                };
                panel1.Controls.Add(txtSearch);
            }

            if (lblReportMode == null)
            {
                lblReportMode = CreateFilterLabel("نوع التقرير");
                panel1.Controls.Add(lblReportMode);
            }

            if (lblSearch == null)
            {
                lblSearch = CreateFilterLabel("بحث");
                panel1.Controls.Add(lblSearch);
            }

            label4.Text = "من تاريخ";
            label5.Text = "الى تاريخ";
            label6.Text = "البائع";
            label8.Text = "إجمالي البيع $";
            label9.Text = "إجمالي البيع ل.ل";
            label10.Text = "صافي الربح $";
            label11.Text = "صافي الربح ل.ل";
        }

        private Label CreateFilterLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(51, 65, 85),
                Text = text
            };
        }

        private void StyleButton(Button button, Color color, string text)
        {
            button.Text = text;
            button.BackColor = color;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        private void StyleTextBox(TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            textBox.TextAlign = HorizontalAlignment.Center;
        }

        private void StyleGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.White;
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.GridColor = Color.FromArgb(226, 232, 240);
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        }

        private void StyleSummaryCards()
        {
            foreach (TextBox box in new[] { textBox1, textBox2, textBox3, textBox4 })
            {
                box.BackColor = Color.White;
                box.BorderStyle = BorderStyle.FixedSingle;
                box.ReadOnly = true;
                box.TabStop = false;
            }

            foreach (Label label in new[] { label8, label9, label10, label11 })
            {
                label.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                label.ForeColor = Color.FromArgb(71, 85, 105);
            }

            foreach (Label label in new[] { lbbe3dollar, lbbe3lebanon, lbrebe7dollar, lbrebe7lebanon })
            {
                label.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                label.ForeColor = Color.FromArgb(15, 23, 42);
            }
        }

        private void LayoutFilterPanel()
        {
            if (panel1.ClientSize.Width <= 0)
            {
                return;
            }

            int margin = 12;
            int gap = 10;
            int labelTop = 8;
            int inputTop = 30;
            int buttonTop = 28;
            int buttonWidth = 90;
            int buttonHeight = 38;
            int inputWidth = Math.Max(120, Math.Min(180, (panel1.ClientSize.Width - 720) / 5));

            label4.SetBounds(margin, labelTop, inputWidth, 20);
            dtpStart.SetBounds(margin, inputTop, inputWidth, 28);

            int x = dtpStart.Right + gap;
            label5.SetBounds(x, labelTop, inputWidth, 20);
            dtpEnd.SetBounds(x, inputTop, inputWidth, 28);

            x = dtpEnd.Right + gap;
            label6.SetBounds(x, labelTop, inputWidth, 20);
            combouser.SetBounds(x, inputTop, inputWidth, 28);

            x = combouser.Right + gap;
            lblReportMode.SetBounds(x, labelTop, inputWidth, 20);
            comboReportMode.SetBounds(x, inputTop, inputWidth, 28);

            x = comboReportMode.Right + gap;
            lblSearch.SetBounds(x, labelTop, inputWidth, 20);
            txtSearch.SetBounds(x, inputTop, inputWidth, 28);

            btnexcel.SetBounds(panel1.ClientSize.Width - buttonWidth - margin, buttonTop, buttonWidth, buttonHeight);
            btnclear.SetBounds(btnexcel.Left - buttonWidth - gap, buttonTop, buttonWidth, buttonHeight);
            btnadd.SetBounds(btnclear.Left - buttonWidth - gap, buttonTop, buttonWidth, buttonHeight);
        }

        private void LayoutSummaryCards()
        {
            int gap = 12;
            int cardHeight = Math.Max(70, panel2.ClientSize.Height - (gap * 2));
            int cardWidth = Math.Max(160, (panel2.ClientSize.Width - (gap * 5)) / 4);

            LayoutSummaryCard(textBox1, label8, lbbe3dollar, gap, gap, cardWidth, cardHeight);
            LayoutSummaryCard(textBox2, label9, lbbe3lebanon, gap * 2 + cardWidth, gap, cardWidth, cardHeight);
            LayoutSummaryCard(textBox3, label10, lbrebe7dollar, gap * 3 + cardWidth * 2, gap, cardWidth, cardHeight);
            LayoutSummaryCard(textBox4, label11, lbrebe7lebanon, gap * 4 + cardWidth * 3, gap, cardWidth, cardHeight);
        }

        private void LayoutSummaryCard(TextBox box, Label title, Label value, int left, int top, int width, int height)
        {
            box.SetBounds(left, top, width, height);
            title.SetBounds(left + 12, top + 10, width - 24, 22);
            value.SetBounds(left + 12, top + 38, width - 24, 28);
            title.BringToFront();
            value.BringToFront();
        }

        private void LoadUsers()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT user_id, username FROM Users ORDER BY username;", con))
            {
                DataTable users = new DataTable();
                adapter.Fill(users);

                DataRow allUsers = users.NewRow();
                allUsers["user_id"] = DBNull.Value;
                allUsers["username"] = "All sellers";
                users.Rows.InsertAt(allUsers, 0);

                combouser.DataBindings.Clear();
                combouser.DataSource = users;
                combouser.DisplayMember = "username";
                combouser.ValueMember = "user_id";
                combouser.SelectedIndex = 0;
            }
        }

        private void LoadReport()
        {
            try
            {
                ReportFilter filter;
                if (!TryGetFilter(out filter))
                {
                    return;
                }

                currentReportTable = GetEarningReport(filter);
                dataGridView1.DataSource = currentReportTable;
                FormatReportGrid();
                UpdateTotals(filter.ExchangeRate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load earning report: " + ex.Message, "Earning Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryGetFilter(out ReportFilter filter)
        {
            filter = new ReportFilter();

            filter.ExchangeRate = POS_System.Program.SettingsManager.GetExchangeRate();
            filter.StartDate = dtpStart.Checked ? dtpStart.Value.Date : (DateTime?)null;
            filter.EndDate = dtpEnd.Checked ? dtpEnd.Value.Date.AddDays(1) : (DateTime?)null;

            if (filter.StartDate.HasValue && filter.EndDate.HasValue && filter.StartDate.Value >= filter.EndDate.Value)
            {
                MessageBox.Show("Start date must be before end date.", "Earning Report", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (combouser.SelectedValue != null && combouser.SelectedValue != DBNull.Value &&
                int.TryParse(combouser.SelectedValue.ToString(), out int userId))
            {
                filter.UserId = userId;
            }

            filter.SearchText = txtSearch == null ? string.Empty : txtSearch.Text.Trim();
            filter.ReportMode = comboReportMode == null || comboReportMode.SelectedItem == null
                ? "تفصيلي"
                : comboReportMode.SelectedItem.ToString();

            return true;
        }

        private DataTable GetEarningReport(ReportFilter filter)
        {
            DataTable details = GetEarningDetails(filter);

            if (filter.ReportMode == "حسب البائع")
            {
                return BuildGroupedReport(details, "Seller", "البائع");
            }

            if (filter.ReportMode == "حسب المنتج")
            {
                return BuildGroupedReport(details, "Product", "المنتج");
            }

            if (filter.ReportMode == "حسب اليوم")
            {
                return BuildGroupedReport(details, "Day", "اليوم");
            }

            return details;
        }

        private DataTable GetEarningDetails(ReportFilter filter)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT
                    s.sale_id AS [Invoice],
                    TRY_CONVERT(DATETIME2, s.sale_date) AS [Date],
                    CONVERT(DATE, TRY_CONVERT(DATETIME2, s.sale_date)) AS [Day],
                    ISNULL(u.username, s.created_by) AS [Seller],
                    ISNULL(s.customer_name, c.name) AS [Customer],
                    ISNULL(si.name_product, p.name) AS [Product],
                    ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0) AS [Quantity],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_usd), 0) AS DECIMAL(18,2)) AS [Cost USD],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0) AS DECIMAL(18,0)) AS [Cost LBP],
                    CAST(ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) AS DECIMAL(18,2)) AS [Sold Unit Price],
                    CASE
                        WHEN ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) >= ISNULL(NULLIF(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0), 0) / 2
                             AND ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0) > 0
                            THEN N'LBP'
                        ELSE N'USD'
                    END AS [Currency],
                    CAST(CASE
                        WHEN ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) >= ISNULL(NULLIF(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0), 0) / 2
                             AND ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0) > 0
                            THEN (ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) * ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)) / @ExchangeRate
                        ELSE ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) * ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)
                    END AS DECIMAL(18,2)) AS [Sales USD],
                    CAST(CASE
                        WHEN ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) >= ISNULL(NULLIF(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0), 0) / 2
                             AND ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0) > 0
                            THEN ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) * ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)
                        ELSE ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) * ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0) * @ExchangeRate
                    END AS DECIMAL(18,0)) AS [Sales LBP],
                    CAST(CASE
                        WHEN ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) >= ISNULL(NULLIF(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0), 0) / 2
                             AND ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0) > 0
                            THEN ((ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) - ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0)) * ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)) / @ExchangeRate
                        ELSE (ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) - ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_usd), 0)) * ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)
                    END AS DECIMAL(18,2)) AS [Profit USD],
                    CAST(CASE
                        WHEN ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) >= ISNULL(NULLIF(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0), 0) / 2
                             AND ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0) > 0
                            THEN (ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) - ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_lb), 0)) * ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)
                        ELSE ((ISNULL(TRY_CONVERT(DECIMAL(18,4), si.unit_price), 0) - ISNULL(TRY_CONVERT(DECIMAL(18,4), p.price_usd), 0)) * ISNULL(TRY_CONVERT(DECIMAL(18,4), si.quantity), 0)) * @ExchangeRate
                    END AS DECIMAL(18,0)) AS [Profit LBP]
                FROM Sales s
                LEFT JOIN Users u ON TRY_CONVERT(INT, s.user_id) = u.user_id
                LEFT JOIN Customers c ON TRY_CONVERT(INT, s.customer_id) = c.customer_id
                INNER JOIN Sale_Items si ON s.sale_id = si.sale_id
                LEFT JOIN Products p ON si.product_id = p.product_id
                WHERE (@UserId IS NULL OR TRY_CONVERT(INT, s.user_id) = @UserId)
                  AND (@StartDate IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) >= @StartDate)
                  AND (@EndDate IS NULL OR TRY_CONVERT(DATETIME2, s.sale_date) < @EndDate)
                  AND (@Search = N''
                       OR ISNULL(si.name_product, p.name) LIKE N'%' + @Search + N'%'
                       OR ISNULL(u.username, s.created_by) LIKE N'%' + @Search + N'%'
                       OR ISNULL(s.customer_name, c.name) LIKE N'%' + @Search + N'%'
                       OR TRY_CONVERT(NVARCHAR(50), s.sale_id) LIKE N'%' + @Search + N'%')
                  AND ISNULL(TRY_CONVERT(BIT, s.is_returned), 0) = 0
                ORDER BY TRY_CONVERT(DATETIME2, s.sale_date) DESC, s.sale_id DESC;", con))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = filter.UserId.HasValue ? (object)filter.UserId.Value : DBNull.Value;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime2).Value = filter.StartDate.HasValue ? (object)filter.StartDate.Value : DBNull.Value;
                cmd.Parameters.Add("@EndDate", SqlDbType.DateTime2).Value = filter.EndDate.HasValue ? (object)filter.EndDate.Value : DBNull.Value;
                cmd.Parameters.Add("@ExchangeRate", SqlDbType.Decimal).Value = filter.ExchangeRate;
                cmd.Parameters["@ExchangeRate"].Precision = 18;
                cmd.Parameters["@ExchangeRate"].Scale = 4;
                cmd.Parameters.Add("@Search", SqlDbType.NVarChar, 100).Value = filter.SearchText ?? string.Empty;

                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        private DataTable BuildGroupedReport(DataTable details, string groupColumn, string caption)
        {
            DataTable grouped = new DataTable();
            grouped.Columns.Add(caption, typeof(string));
            grouped.Columns.Add("Invoices", typeof(int));
            grouped.Columns.Add("Quantity", typeof(decimal));
            grouped.Columns.Add("Sales USD", typeof(decimal));
            grouped.Columns.Add("Sales LBP", typeof(decimal));
            grouped.Columns.Add("Profit USD", typeof(decimal));
            grouped.Columns.Add("Profit LBP", typeof(decimal));
            grouped.Columns.Add("Margin %", typeof(decimal));

            var rows = details.AsEnumerable()
                .GroupBy(row => Convert.ToString(row[groupColumn]))
                .OrderByDescending(group => group.Sum(row => GetDecimal(row, "Profit USD")));

            foreach (var group in rows)
            {
                decimal salesUsd = group.Sum(row => GetDecimal(row, "Sales USD"));
                decimal profitUsd = group.Sum(row => GetDecimal(row, "Profit USD"));
                decimal margin = salesUsd == 0m ? 0m : profitUsd / salesUsd * 100m;

                grouped.Rows.Add(
                    string.IsNullOrWhiteSpace(group.Key) ? "Unknown" : group.Key,
                    group.Select(row => Convert.ToString(row["Invoice"])).Distinct().Count(),
                    group.Sum(row => GetDecimal(row, "Quantity")),
                    salesUsd,
                    group.Sum(row => GetDecimal(row, "Sales LBP")),
                    profitUsd,
                    group.Sum(row => GetDecimal(row, "Profit LBP")),
                    margin);
            }

            return grouped;
        }

        private void FormatReportGrid()
        {
            foreach (string columnName in new[] { "Cost USD", "Cost LBP", "Sold Unit Price", "Sales USD", "Sales LBP", "Profit USD", "Profit LBP", "Margin %" })
            {
                if (dataGridView1.Columns.Contains(columnName))
                {
                    dataGridView1.Columns[columnName].DefaultCellStyle.Format = "N2";
                    dataGridView1.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            if (dataGridView1.Columns.Contains("Date"))
            {
                dataGridView1.Columns["Date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            }
        }

        private void UpdateTotals(decimal exchangeRate)
        {
            decimal totalUsd = 0m;
            decimal totalLbp = 0m;
            decimal profitUsd = 0m;
            decimal profitLbp = 0m;

            if (currentReportTable != null)
            {
                foreach (DataRow row in currentReportTable.Rows)
                {
                    totalUsd += GetDecimal(row, "Sales USD");
                    totalLbp += GetDecimal(row, "Sales LBP");
                    profitUsd += GetDecimal(row, "Profit USD");
                    profitLbp += GetDecimal(row, "Profit LBP");
                }
            }

            lbbe3dollar.Text = "$ " + totalUsd.ToString("N2");
            lbbe3lebanon.Text = "L.L " + totalLbp.ToString("N0");
            lbrebe7dollar.Text = "$ " + profitUsd.ToString("N2");
            lbrebe7lebanon.Text = "L.L " + profitLbp.ToString("N0");

            decimal margin = totalUsd == 0m ? 0m : profitUsd / totalUsd * 100m;
            label10.Text = "صافي الربح $ (" + margin.ToString("N1") + "%)";
        }

        private decimal GetDecimal(DataRow row, string columnName)
        {
            object value = row[columnName];
            return value == DBNull.Value ? 0m : Convert.ToDecimal(value);
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            LoadReport();
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            combouser.SelectedIndex = 0;
            dtpStart.Checked = true;
            dtpEnd.Checked = true;
            dtpStart.Value = DateTime.Today;
            dtpEnd.Value = DateTime.Today;
            if (comboReportMode != null)
            {
                comboReportMode.SelectedIndex = 0;
            }

            if (txtSearch != null)
            {
                txtSearch.Clear();
            }

            LoadReport();
        }

        private void btnexcel_Click(object sender, EventArgs e)
        {
            if (currentReportTable == null || currentReportTable.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Earning Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                dialog.FileName = "earning-report-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".xlsx";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    IXLWorksheet sheet = workbook.Worksheets.Add(currentReportTable, "Earning Report");
                    sheet.Columns().AdjustToContents();
                    sheet.Row(1).Style.Font.Bold = true;
                    sheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#0F172A");
                    sheet.Row(1).Style.Font.FontColor = XLColor.White;
                    workbook.SaveAs(dialog.FileName);
                }
            }

            MessageBox.Show("Excel report saved successfully.", "Earning Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private sealed class ReportFilter
        {
            public int? UserId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal ExchangeRate { get; set; }
            public string SearchText { get; set; }
            public string ReportMode { get; set; }
        }
    }
}
