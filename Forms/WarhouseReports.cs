using ClosedXML.Excel;
using Pos_System.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class WarhouseReports : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;

        public WarhouseReports()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
        }

        private void WarhouseReports_Load(object sender, EventArgs e)
        {
            cmbStockFilter.SelectedIndex = 0;
            LoadWarehouseReport();
        }

        private void LoadWarehouseReport()
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, AppSession.Role, PermissionService.ScreenWarehouse))
            {
                MessageBox.Show("You do not have permission to open warehouse reports.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BeginInvoke(new Action(Close));
                return;
            }

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                using (SqlCommand cmdSummary = new SqlCommand(@"
                    SELECT 
                        ISNULL(SUM(ISNULL(p.sale_price_usd, 0) * ISNULL(p.stock_quantity, 0)), 0) AS CapitalValue,
                        ISNULL(SUM(ISNULL(p.price_usd, 0) * ISNULL(p.stock_quantity, 0)), 0) AS PurchaseCost,
                        COUNT(*) AS NumberOfItems,
                        ISNULL(SUM(ISNULL(p.stock_quantity, 0)), 0) AS TotalUnits,
                        ISNULL(SUM(CASE WHEN ISNULL(p.stock_quantity, 0) <= 10 THEN 1 ELSE 0 END), 0) AS LowStockItems
                    FROM Products p;", con))
                using (SqlDataReader reader = cmdSummary.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        lbkematr2slmal.Text = Convert.ToDecimal(reader["CapitalValue"]).ToString("$#,##0.00");
                        lbtaklefetshera2.Text = Convert.ToDecimal(reader["PurchaseCost"]).ToString("$#,##0.00");
                        lb3dadalsnef.Text = Convert.ToInt32(reader["NumberOfItems"]).ToString("N0");
                        lbejmale.Text = Convert.ToDecimal(reader["TotalUnits"]).ToString("N0");
                        lblLowStockValue.Text = Convert.ToInt32(reader["LowStockItems"]).ToString("N0");
                    }
                }

                using (SqlCommand cmdProducts = new SqlCommand(@"
                    SELECT 
                        p.product_id AS [ID],
                        p.name AS [Product],
                        ISNULL(c.category_name, '') AS [Category],
                        ISNULL(s.name, '') AS [Supplier],
                        ISNULL(p.stock_quantity, 0) AS [Stock],
                        ISNULL(p.barcode, '') AS [Barcode],
                        ISNULL(p.price_usd, 0) AS [Purchase USD],
                        ISNULL(p.price_lb, 0) AS [Purchase LBP],
                        ISNULL(p.sale_price_usd, 0) AS [Sale USD],
                        ISNULL(p.sale_price_lb, 0) AS [Sale LBP],
                        ISNULL(p.price_usd, 0) * ISNULL(p.stock_quantity, 0) AS [Cost Value USD],
                        ISNULL(p.sale_price_usd, 0) * ISNULL(p.stock_quantity, 0) AS [Sale Value USD],
                        p.created_at AS [Created At]
                    FROM Products p
                    LEFT JOIN Categories c ON c.category_id = p.category_id
                    LEFT JOIN Suppliers s ON s.supplier_id = p.supplier_id
                    WHERE (@search = '' OR p.name LIKE @searchLike OR p.barcode LIKE @searchLike OR c.category_name LIKE @searchLike OR s.name LIKE @searchLike)
                      AND (@stockMode = 'All' OR (@stockMode = 'Low' AND ISNULL(p.stock_quantity, 0) <= 10) OR (@stockMode = 'Out' AND ISNULL(p.stock_quantity, 0) = 0))
                    ORDER BY p.name;", con))
                using (SqlDataAdapter adapter = new SqlDataAdapter(cmdProducts))
                {
                    string search = txtSearch.Text.Trim();
                    cmdProducts.Parameters.Add("@search", SqlDbType.NVarChar, 200).Value = search;
                    cmdProducts.Parameters.Add("@searchLike", SqlDbType.NVarChar, 220).Value = "%" + search + "%";
                    cmdProducts.Parameters.Add("@stockMode", SqlDbType.NVarChar, 20).Value =
                        cmbStockFilter.SelectedItem == null ? "All" : cmbStockFilter.SelectedItem.ToString();

                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
                    FormatGrid();
                }
            }
        }

        private void FormatGrid()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            SetFillWeight("ID", 45);
            SetFillWeight("Product", 150);
            SetFillWeight("Category", 100);
            SetFillWeight("Supplier", 100);
            SetFillWeight("Stock", 65);
            SetFillWeight("Barcode", 100);

            string[] moneyColumns =
            {
                "Purchase USD",
                "Purchase LBP",
                "Sale USD",
                "Sale LBP",
                "Cost Value USD",
                "Sale Value USD"
            };

            foreach (string columnName in moneyColumns)
            {
                if (dataGridView1.Columns.Contains(columnName))
                {
                    dataGridView1.Columns[columnName].DefaultCellStyle.Format = "N2";
                    dataGridView1.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }

            if (dataGridView1.Columns.Contains("Stock"))
            {
                dataGridView1.Columns["Stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void SetFillWeight(string columnName, float weight)
        {
            if (dataGridView1.Columns.Contains(columnName))
            {
                dataGridView1.Columns[columnName].FillWeight = weight;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadWarehouseReport();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (IsHandleCreated)
            {
                LoadWarehouseReport();
            }
        }

        private void cmbStockFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsHandleCreated)
            {
                LoadWarehouseReport();
            }
        }

        private void btnexcel_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable table = dataGridView1.DataSource as DataTable;
                if (table == null || table.Rows.Count == 0)
                {
                    MessageBox.Show("No data to export.", "Warehouse Report");
                    return;
                }

                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                    dialog.FileName = "Warehouse_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";

                    if (dialog.ShowDialog(this) != DialogResult.OK)
                    {
                        return;
                    }

                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        IXLWorksheet summary = workbook.Worksheets.Add("Summary");
                        summary.Cell(1, 1).Value = "Capital Value";
                        summary.Cell(1, 2).Value = lbkematr2slmal.Text;
                        summary.Cell(2, 1).Value = "Purchase Cost";
                        summary.Cell(2, 2).Value = lbtaklefetshera2.Text;
                        summary.Cell(3, 1).Value = "Items";
                        summary.Cell(3, 2).Value = lb3dadalsnef.Text;
                        summary.Cell(4, 1).Value = "Units";
                        summary.Cell(4, 2).Value = lbejmale.Text;
                        summary.Cell(5, 1).Value = "Low Stock";
                        summary.Cell(5, 2).Value = lblLowStockValue.Text;
                        summary.Columns().AdjustToContents();

                        IXLWorksheet details = workbook.Worksheets.Add(table, "Products");
                        details.Columns().AdjustToContents();
                        workbook.SaveAs(dialog.FileName);
                    }
                }

                MessageBox.Show("Warehouse report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not export warehouse report: " + ex.Message, "Export");
            }
        }
    }
}
