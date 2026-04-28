using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

using Excel = Microsoft.Office.Interop.Excel;



namespace Pos_System.Forms
{



    public partial class EarningReports : Form
    {
        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";

        public EarningReports()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
        }


        private void LoadSalesReport()
        {
            string query = @"SELECT 
                        s.sale_date AS [Date],
                        u.username AS [Seller],
                        p.name AS [Product],
                        si.quantity AS [Quantity],
                        p.price_usd AS [PurchasePriceUSD],
                        si.unit_price AS [SellingPriceUSD],
                        (si.quantity * si.unit_price) AS [TotalUSD],
                        ((si.unit_price - p.price_usd) * si.quantity) AS [ProfitUSD]
                     FROM Sales s
                     INNER JOIN Users u ON s.user_id = u.user_id
                     INNER JOIN Sale_Items si ON s.sale_id = si.sale_id
                     INNER JOIN Products p ON si.product_id = p.product_id
                     ORDER BY s.sale_date DESC;";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;

                // Format numbers
                dataGridView1.Columns["PurchasePriceUSD"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["SellingPriceUSD"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["TotalUSD"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["ProfitUSD"].DefaultCellStyle.Format = "N2";
            }
        }

        private void EarningReports_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'pos_systemDataSet17.Users' table. You can move, or remove it, as needed.
            this.usersTableAdapter.Fill(this.pos_systemDataSet17.Users);
            LoadSalesReport();
        }


        private void LoadUsers()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT user_id, username FROM Users", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                combouser.DataSource = dt;
                combouser.DisplayMember = "username";
                combouser.ValueMember = "user_id";
                combouser.SelectedIndex = -1; // no default selection
            }
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            // Validate user selection
            int? userId = null;
            if (combouser.SelectedIndex >= 0 && combouser.SelectedValue != null)
            {
                int parsedId;
                if (int.TryParse(combouser.SelectedValue.ToString(), out parsedId))
                {
                    userId = parsedId;
                }
            }

            // Date filters
            DateTime? startDate = dtpStart.Checked ? (DateTime?)dtpStart.Value.Date : null;
            DateTime? endDate = dtpEnd.Checked ? (DateTime?)dtpEnd.Value.Date : null;

            // Dollar rate
            decimal dollarRate = 0;
            if (!decimal.TryParse(txtdollar.Text, out dollarRate))
            {
                MessageBox.Show("Please enter a valid dollar rate.");
                return;
            }

            string query = @"SELECT 
                        s.sale_date,
                        u.username,
                        p.name,
                        si.quantity,
                        p.price_usd,
                        si.unit_price,
                        (si.quantity * si.unit_price) AS TotalUSD,
                        ((si.unit_price - p.price_usd) * si.quantity) AS ProfitUSD
                     FROM Sales s
                     INNER JOIN Users u ON s.user_id = u.user_id
                     INNER JOIN Sale_Items si ON s.sale_id = si.sale_id
                     INNER JOIN Products p ON si.product_id = p.product_id
                     WHERE (@UserId IS NULL OR s.user_id = @UserId)
                       AND (@StartDate IS NULL OR s.sale_date >= @StartDate)
                       AND (@EndDate IS NULL OR s.sale_date <= @EndDate)
                     ORDER BY s.sale_date DESC;";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserId", (object)userId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StartDate", (object)startDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object)endDate ?? DBNull.Value);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;

                // Calculate totals safely
                decimal totalUSD = dt.AsEnumerable()
                                     .Where(row => row["TotalUSD"] != DBNull.Value)
                                     .Sum(row => row.Field<decimal>("TotalUSD"));

                decimal profitUSD = dt.AsEnumerable()
                                      .Where(row => row["ProfitUSD"] != DBNull.Value)
                                      .Sum(row => row.Field<decimal>("ProfitUSD"));

                decimal totalLB = totalUSD * dollarRate;
                decimal profitLB = profitUSD * dollarRate;

                // Show in labels
                lbbe3dollar.Text = $"قيمة البيع $: {totalUSD:N2}";
                lbbe3lebanon.Text = $"قيمة البيع ل.ل: {totalLB:N2}";
                lbrebe7dollar.Text = $"$ الربح: {profitUSD:N2}";
                lbrebe7lebanon.Text = $"ل.ل الربح: {profitLB:N2}";
            }
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            // Clear filters
            combouser.SelectedIndex = -1;   // reset ComboBox
            txtdollar.Clear();              // clear TextBox
            dtpStart.Value = DateTime.Now;  // reset DateTimePicker
            dtpEnd.Value = DateTime.Now;    // reset DateTimePicker
            dtpStart.Checked = false;       // uncheck date filter
            dtpEnd.Checked = false;

            // Reload all data without filters
            string query = @"SELECT 
                        s.sale_date,
                        u.username,
                        p.name,
                        si.quantity,
                        p.price_usd,
                        si.unit_price,
                        (si.quantity * si.unit_price) AS TotalUSD,
                        ((si.unit_price - p.price_usd) * si.quantity) AS ProfitUSD
                     FROM Sales s
                     INNER JOIN Users u ON s.user_id = u.user_id
                     INNER JOIN Sale_Items si ON s.sale_id = si.sale_id
                     INNER JOIN Products p ON si.product_id = p.product_id
                     ORDER BY s.sale_date DESC;";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;

                // Reset labels to totals for all data
                decimal totalUSD = dt.AsEnumerable()
                                     .Where(row => row["TotalUSD"] != DBNull.Value)
                                     .Sum(row => row.Field<decimal>("TotalUSD"));

                decimal profitUSD = dt.AsEnumerable()
                                      .Where(row => row["ProfitUSD"] != DBNull.Value)
                                      .Sum(row => row.Field<decimal>("ProfitUSD"));

                lbbe3dollar.Text = $"قيمة البيع $: {totalUSD:N2}";
                lbbe3lebanon.Text = $"قيمة البيع ل.ل: 0.00"; // no rate entered
                lbrebe7dollar.Text = $"$ الربح: {profitUSD:N2}";
                lbrebe7lebanon.Text = $"ل.ل الربح: 0.00";
            }
        }

        private void btnexcel_Click(object sender, EventArgs e)
        {
            try
            {
                Excel.Application excelApp = new Excel.Application();
                excelApp.Visible = true;

                Excel.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Sales Report";

                // Export column headers
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText;
                }

                // Export rows
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value != null)
                        {
                            worksheet.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                        }
                    }
                }

                worksheet.Columns.AutoFit();
                MessageBox.Show("Data exported successfully to Excel!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting to Excel: " + ex.Message);
            }
        }
    }
}

