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
    public partial class WarhouseReports : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        public WarhouseReports()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
        }


        private void LoadWarehouseReport()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                // Load summary
                string summaryQuery = @"SELECT 
                                    SUM(p.sale_price_usd * p.stock_quantity) AS CapitalValue,
                                    SUM(p.price_usd * p.stock_quantity) AS PurchaseCost,
                                    COUNT(*) AS NumberOfItems,
                                    SUM(p.stock_quantity) AS TotalUnits
                                FROM Products p;";
                SqlCommand cmdSummary = new SqlCommand(summaryQuery, con);
                con.Open();
                SqlDataReader reader = cmdSummary.ExecuteReader();
                if (reader.Read())
                {
                    lbkematr2slmal.Text = $"قيمة رأس المال: ${reader["CapitalValue"]:N2}";
                    lbtaklefetshera2.Text = $"تكلفة الشراء: ${reader["PurchaseCost"]:N2}";
                    lb3dadalsnef.Text = $"عدد الأصناف: {reader["NumberOfItems"]}";
                    lbejmale.Text = $"إجمالي الوحدات: {reader["TotalUnits"]}";
                }
                reader.Close();

                // Load low stock products
                string productsQuery = @"SELECT 
                                    p.name AS ProductName,
                                    p.barcode,
                                    p.stock_quantity,
                                    p.sale_price_usd AS Price
                                FROM Products p;";
                //WHERE p.stock_quantity < 10
                SqlDataAdapter da = new SqlDataAdapter(productsQuery, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;

                // Add action button column
                if (!dataGridView1.Columns.Contains("Action"))
                {
                    DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                    btn.HeaderText = "إجراء";
                    btn.Text = "طلب شراء";
                    btn.UseColumnTextForButtonValue = true;
                    dataGridView1.Columns.Add(btn);
                }
            }
        }

        private void WarhouseReports_Load(object sender, EventArgs e)
        {
            LoadWarehouseReport();
        }

        private void btnexcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Create Excel application
                Excel.Application excelApp = new Excel.Application();
                excelApp.Visible = true;

                // Create a new workbook
                Excel.Workbook workbook = excelApp.Workbooks.Add(Type.Missing);
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Warehouse Report";

                // Export summary labels
                worksheet.Cells[1, 1] = "قيمة رأس المال";
                worksheet.Cells[1, 2] = lbkematr2slmal.Text;

                worksheet.Cells[2, 1] = "تكلفة الشراء";
                worksheet.Cells[2, 2] = lbtaklefetshera2.Text;

                worksheet.Cells[3, 1] = "عدد الأصناف";
                worksheet.Cells[3, 2] = lb3dadalsnef.Text;

                worksheet.Cells[4, 1] = "إجمالي الوحدات";
                worksheet.Cells[4, 2] = lbejmale.Text;

                // Export table headers
                for (int i = 0; i < dataGridView1.Columns.Count; i++)
                {
                    worksheet.Cells[6, i + 1] = dataGridView1.Columns[i].HeaderText;
                }

                // Export table rows
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value != null)
                        {
                            worksheet.Cells[i + 7, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                        }
                    }
                }

                worksheet.Columns.AutoFit();
                MessageBox.Show("تم تصدير التقرير إلى Excel بنجاح!", "تصدير", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التصدير: " + ex.Message);
            }
        }
    }
}
