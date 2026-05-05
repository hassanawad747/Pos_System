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
namespace Pos_System.Forms
{
    public partial class addsalesform : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;

        public addsalesform()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            LoadStock();
            CalculateTotals();


        }


        private void LoadStock(string productName, string supplierName)
        {
            string query = @"SELECT 
                        p.name AS ProductName,
                        p.stock_quantity AS Quantity,
                        p.price_usd AS UnitPriceUSD,
                        p.price_lb AS UnitPriceLB,
                        (p.stock_quantity * p.price_usd) AS TotalPriceUSD,
                        (p.stock_quantity * p.price_lb) AS TotalPriceLB,
                        s.name AS SupplierName
                     FROM Products p
                     INNER JOIN Suppliers s ON p.supplier_id = s.supplier_id
                     WHERE (@ProductName = '' OR p.name LIKE '%' + @ProductName + '%')
                       AND (@SupplierName = '' OR s.name = @SupplierName)";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ProductName", productName);
                cmd.Parameters.AddWithValue("@SupplierName", supplierName);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;

                // تنسيق الأعمدة
                dataGridView1.Columns["UnitPriceUSD"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["UnitPriceLB"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["TotalPriceUSD"].DefaultCellStyle.Format = "N2";
                dataGridView1.Columns["TotalPriceLB"].DefaultCellStyle.Format = "N2";
            }
        }


        private void CalculateTotals()
        {
            string query = @"SELECT 
                        SUM(p.stock_quantity * p.price_usd) AS TotalUSD,
                        SUM(p.stock_quantity * p.price_lb) AS TotalLB
                     FROM Products p";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    decimal totalUSD = reader["TotalUSD"] != DBNull.Value ? Convert.ToDecimal(reader["TotalUSD"]) : 0;
                    decimal totalLB = reader["TotalLB"] != DBNull.Value ? Convert.ToDecimal(reader["TotalLB"]) : 0;

                    lbdollar.Text = $"Total (USD): {totalUSD:N2}";
                    lblebanon.Text = $"Total (LB): {totalLB:N2}";
                }
            }
        }



        private void addsalesform_Load(object sender, EventArgs e)
        {
            LoginForm loginForm = new LoginForm();
            // TODO: This line of code loads data into the 'pos_systemDataSet16.Suppliers' table. You can move, or remove it, as needed.
            this.suppliersTableAdapter.Fill(this.pos_systemDataSet16.Suppliers);
            lbDate.Text = DateTime.Now.ToString("dd-MMM-yyyy");
            lbusername.Text = LoginForm.LoggedInUsername; // from login form
            LoadCustomers();
            LoadStock("", ""); // عرض كل البيانات بدون فلترة

            // تعبئة ComboBox بالموردين
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT name FROM Suppliers", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbSuppliers.DataSource = dt;
                cmbSuppliers.DisplayMember = "name";
                cmbSuppliers.SelectedIndex = -1; // لا اختيار افتراضي
            }

        }

        private void LoadCustomers()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT customer_id, name FROM Customers", con);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    combocustomer.Items.Add(new { ID = reader["customer_id"], Name = reader["name"].ToString() });
                }
            }
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string productName = txtsearch.Text.Trim();
            string supplierName = cmbSuppliers.SelectedItem != null ? cmbSuppliers.SelectedItem.ToString() : "";
            LoadStock(productName, supplierName);
        }

        private void LoadStock()
        {
            string query = @"SELECT 
                        p.name AS ProductName,
                        p.stock_quantity AS Quantity,
                        p.price_usd AS UnitPriceUSD,
                        p.price_lb AS UnitPriceLB,
                        (p.stock_quantity * p.price_usd) AS TotalPriceUSD,
                        (p.stock_quantity * p.price_lb) AS TotalPriceLB,
                        s.name AS SupplierName
                     FROM Products p
                     INNER JOIN Suppliers s ON p.supplier_id = s.supplier_id";

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt; // ربط البيانات مع DataGridView

                // تنسيق الأعمدة
                dataGridView1.Columns["UnitPriceUSD"].DefaultCellStyle.Format = "N2";   // سعر بالدولار
                dataGridView1.Columns["UnitPriceLB"].DefaultCellStyle.Format = "N2";    // سعر بالليرة
                dataGridView1.Columns["TotalPriceUSD"].DefaultCellStyle.Format = "N2";  // الإجمالي بالدولار
                dataGridView1.Columns["TotalPriceLB"].DefaultCellStyle.Format = "N2";   // الإجمالي بالليرة
            }
        }

        private void btnback_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void txtsearch_TextChanged(object sender, EventArgs e)
        {
            string productName = txtsearch.Text.Trim();
            string supplierName = cmbSuppliers.SelectedItem != null ? cmbSuppliers.SelectedItem.ToString() : "";
            LoadStock(productName, supplierName);
        }
    }
}
