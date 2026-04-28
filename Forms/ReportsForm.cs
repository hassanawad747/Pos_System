using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;

namespace Pos_System.Forms
{
    public partial class ReportsForm : Form
    {
        private const string ConnectionString = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";

        private DataTable currentSummaryTable = new DataTable();
        private string currentReportType = "all";
        private string currentReportTitle = "All Reports Summary";
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
            ConfigureReportControls();
            LoadDashboard();
        }

        private void ConfigureReportControls()
        {
            labeldate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy  hh:mm tt", CultureInfo.InvariantCulture);

            datetimestart.Format = DateTimePickerFormat.Custom;
            datetimestart.CustomFormat = "dd/MM/yyyy";
            datetimeend.Format = DateTimePickerFormat.Custom;
            datetimeend.CustomFormat = "dd/MM/yyyy";

            DateTime today = DateTime.Today;
            datetimestart.Value = new DateTime(today.Year, today.Month, 1);
            datetimeend.Value = today;

            btnsave.Text = "Custom Report";

            ConfigureGrid(dataGridViewSummary);
            ConfigureGrid(dataGridViewReportsHistory);
            ResetSummaryCards();
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
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        private void LoadDashboard()
        {
            LoadReportsHistory();
            LoadAllReportsSummary();
        }

        private void LoadAllReportsSummary()
        {
            LoadSummaryReport(null, null, "all", "All Reports Summary");
        }

        private void LoadDailyReport(DateTime date)
        {
            DateTime startDate = date.Date;
            DateTime endDateExclusive = startDate.AddDays(1);
            string title = "Daily Report - " + startDate.ToString("dd MMM yyyy");

            LoadSummaryReport(startDate, endDateExclusive, "daily", title);
        }

        private void LoadMonthlyReport(int month, int year)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDateExclusive = startDate.AddMonths(1);
            string title = "Monthly Report - " + startDate.ToString("MMMM yyyy");

            LoadSummaryReport(startDate, endDateExclusive, "monthly", title);
        }

        private void LoadCustomRangeReport(DateTime startDate, DateTime endDate)
        {
            DateTime safeStartDate = startDate.Date;
            DateTime safeEndDateExclusive = endDate.Date.AddDays(1);
            string title = string.Format(
                "Custom Report - {0} to {1}",
                safeStartDate.ToString("dd MMM yyyy"),
                endDate.Date.ToString("dd MMM yyyy"));

            LoadSummaryReport(safeStartDate, safeEndDateExclusive, "custom", title);
        }

        private void LoadSummaryReport(DateTime? startDate, DateTime? endDateExclusive, string reportType, string reportTitle)
        {
            DataTable summaryTable = ExecuteDataTable(
                @"SELECT
                      u.username AS [User Name],
                      COUNT(DISTINCT s.sale_id) AS [Invoices],
                      SUM(ISNULL(si.quantity, 0)) AS [Items Sold],
                      CAST(SUM(ISNULL(s.total_amount, 0)) AS DECIMAL(18,2)) AS [Total Sales],
                      CAST(SUM((ISNULL(si.unit_price, 0) - ISNULL(p.price_usd, 0)) * ISNULL(si.quantity, 0)) AS DECIMAL(18,2)) AS [Profit]
                  FROM Sales s
                  INNER JOIN Users u ON s.user_id = u.user_id
                  LEFT JOIN Sale_Items si ON s.sale_id = si.sale_id
                  LEFT JOIN Products p ON si.product_id = p.product_id
                  WHERE (@StartDate IS NULL OR s.sale_date >= @StartDate)
                    AND (@EndDateExclusive IS NULL OR s.sale_date < @EndDateExclusive)
                  GROUP BY u.username
                  ORDER BY [Total Sales] DESC;",
                CreateParameter("@StartDate", startDate),
                CreateParameter("@EndDateExclusive", endDateExclusive));

            currentSummaryTable = summaryTable;
            currentReportType = reportType;
            currentReportTitle = reportTitle;
            currentStartDate = startDate;
            currentEndDateExclusive = endDateExclusive;

            dataGridViewSummary.DataSource = currentSummaryTable;
            FormatSummaryGrid();
            UpdateSummaryCards(reportTitle, startDate, endDateExclusive, currentSummaryTable);
        }

        private void FormatSummaryGrid()
        {
            if (dataGridViewSummary.Columns.Contains("Total Sales"))
            {
                dataGridViewSummary.Columns["Total Sales"].DefaultCellStyle.Format = "N2";
            }

            if (dataGridViewSummary.Columns.Contains("Profit"))
            {
                dataGridViewSummary.Columns["Profit"].DefaultCellStyle.Format = "N2";
            }
        }

        private void UpdateSummaryCards(string reportTitle, DateTime? startDate, DateTime? endDateExclusive, DataTable summaryTable)
        {
            if (summaryTable.Rows.Count == 0)
            {
                lbtotalsales.Text = "Total Sales: $0.00";
                lbprofit.Text = "Profit: $0.00";
                lbtopuser.Text = "Top User: No Data";

                textBox1.Text = reportTitle + Environment.NewLine + "No sales found for the selected period.";
                textBox2.Text = "Transactions: 0" + Environment.NewLine + "Items Sold: 0";
                textBox3.Text = "Average Sale: $0.00" + Environment.NewLine + "Generated By: " + GetCurrentUsername();
                return;
            }

            decimal totalSales = ConvertToDecimal(summaryTable.Compute("SUM([Total Sales])", string.Empty));
            decimal totalProfit = ConvertToDecimal(summaryTable.Compute("SUM([Profit])", string.Empty));
            int totalInvoices = ConvertToInt(summaryTable.Compute("SUM([Invoices])", string.Empty));
            int totalItemsSold = ConvertToInt(summaryTable.Compute("SUM([Items Sold])", string.Empty));
            decimal averageSale = totalInvoices > 0 ? totalSales / totalInvoices : 0m;

            lbtotalsales.Text = "$" + totalSales.ToString("N2");
            lbprofit.Text = "$" + totalProfit.ToString("N2");
            lbtopuser.Text = summaryTable.Rows[0]["User Name"].ToString();

            //textBox1.Text = reportTitle + Environment.NewLine + BuildDateRangeText(startDate, endDateExclusive);
            //textBox2.Text = "Transactions: " + totalInvoices + Environment.NewLine + "Items Sold: " + totalItemsSold;
            //textBox3.Text = "Average Sale: $" + averageSale.ToString("N2") + Environment.NewLine + "Generated By: " + GetCurrentUsername();
        }

        private void ResetSummaryCards()
        {
            lbtotalsales.Text = "$0.00";
            lbprofit.Text = "$0.00";
            lbtopuser.Text = "No Data";
           // textBox1.Text = "Report summary will appear here.";
            //textBox2.Text = "Transactions: 0" + Environment.NewLine + "Items Sold: 0";
            //textBox3.Text = "Average Sale: $0.00" + Environment.NewLine + "Generated By: " + GetCurrentUsername();
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
                return string.Format(
                    "Date Range: {0} - {1}",
                    startDate.Value.ToString("dd MMM yyyy"),
                    displayEndDate.ToString("dd MMM yyyy"));
            }

            if (startDate.HasValue)
            {
                return "From: " + startDate.Value.ToString("dd MMM yyyy");
            }

            return "Until: " + endDateExclusive.Value.AddDays(-1).ToString("dd MMM yyyy");
        }

        private void SaveReport(string reportType, string filePath)
        {
            int? userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return;
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(
                @"INSERT INTO Reports (report_type, generated_by, file_path)
                  VALUES (@reportType, @generatedBy, @filePath);", connection))
            {
                command.Parameters.AddWithValue("@reportType", reportType);
                command.Parameters.AddWithValue("@generatedBy", userId.Value);
                command.Parameters.AddWithValue("@filePath", filePath);

                connection.Open();
                command.ExecuteNonQuery();
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
            try
            {
                DateTime selectedDate = datetimestart.Value.Date;
                LoadDailyReport(selectedDate);
                SaveReport("daily", "Screen Report - " + selectedDate.ToString("yyyy-MM-dd"));
                LoadReportsHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to generate the daily report.\n" + ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnmonthlyreport_Click(object sender, EventArgs e)
        {
            try
            {
                int month = datetimestart.Value.Month;
                int year = datetimestart.Value.Year;

                LoadMonthlyReport(month, year);
                SaveReport("monthly", "Screen Report - " + year + "-" + month.ToString("00"));
                LoadReportsHistory();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to generate the monthly report.\n" + ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnsave_Click(object sender, EventArgs e)
        {
            try
            {
                if (datetimeend.Value.Date < datetimestart.Value.Date)
                {
                    MessageBox.Show("End date must be greater than or equal to the start date.", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    datetimeend.Focus();
                    return;
                }

                LoadCustomRangeReport(datetimestart.Value.Date, datetimeend.Value.Date);
                SaveReport(
                    "custom",
                    string.Format(
                        "Screen Report - {0}_to_{1}",
                        datetimestart.Value.ToString("yyyy-MM-dd"),
                        datetimeend.Value.ToString("yyyy-MM-dd")));

                LoadReportsHistory();
                MessageBox.Show("Custom report generated successfully.", "Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to generate the custom report.\n" + ex.Message, "Reports", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            return string.Format("Report_{0}_{1}.xlsx", safeReportType, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
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
                worksheet.Cells[3, 2] = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");
                worksheet.Cells[4, 1] = "Date Range";
                worksheet.Cells[4, 2] = BuildDateRangeText(currentStartDate, currentEndDateExclusive);
                worksheet.Cells[5, 1] = "Total Sales";
                worksheet.Cells[5, 2] = lbtotalsales.Text;
                worksheet.Cells[6, 1] = "Profit";
                worksheet.Cells[6, 2] = lbprofit.Text;
                worksheet.Cells[7, 1] = "Top User";
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
            DataTable historyTable = ExecuteDataTable(
                @"SELECT
                      r.report_id AS [ID],
                      r.report_type AS [Report Type],
                      ISNULL(u.username, 'Unknown') AS [Generated By],
                      r.created_at AS [Created At],
                      r.file_path AS [File Path]
                  FROM Reports r
                  LEFT JOIN Users u ON r.generated_by = u.user_id
                  ORDER BY r.created_at DESC;");

            dataGridViewReportsHistory.DataSource = historyTable;

            if (dataGridViewReportsHistory.Columns.Contains("Created At"))
            {
                dataGridViewReportsHistory.Columns["Created At"].DefaultCellStyle.Format = "dd/MM/yyyy hh:mm tt";
            }
        }
    }
}
