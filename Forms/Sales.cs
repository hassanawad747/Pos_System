using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class Sales : Form
    {
        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";
        private string _username;
        private int currentUserId = 1; // مؤقتاً، لازم تجيب user_id من LoginForm
        private int currentCustomerId = 1; // مؤقتاً، أو تختار عميل من ComboBox

        public Sales(string username)
        {
            InitializeComponent();
            labeldate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            _username = username;
        }

        // البحث بالباركود
        private void txtsearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string barcode = txtsearch.Text;

                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT product_id, name, price FROM Products WHERE barcode=@barcode", conn);
                    cmd.Parameters.AddWithValue("@barcode", barcode);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        int productId = (int)reader["product_id"];
                        string itemName = reader["name"].ToString();
                        decimal price = (decimal)reader["price"];

                        datagridsales.Rows.Add(
                            productId,
                            itemName,
                            1,
                            price,
                            0,
                            price,
                            barcode,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            _username,
                            ""
                        );
                    }
                    else
                    {
                        MessageBox.Show("Item not found!");
                    }
                }
            }
        }

        // عند اختيار فئة من ComboBox
        private void comboProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            grpitems.Controls.Clear();
            string categoryName = comboProducts.SelectedItem.ToString();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT p.product_id, p.name, p.price, p.barcode
                    FROM Products p
                    INNER JOIN Categories c ON p.category_id = c.category_id
                    WHERE c.category_name = @categoryName", conn);

                cmd.Parameters.AddWithValue("@categoryName", categoryName);

                SqlDataReader reader = cmd.ExecuteReader();
                int x = 10, y = 20;

                while (reader.Read())
                {
                    int productId = (int)reader["product_id"];
                    string itemName = reader["name"].ToString();
                    decimal price = (decimal)reader["price"];
                    string barcode = reader["barcode"].ToString();

                    Button btn = new Button
                    {
                        Text = itemName,
                        Location = new Point(x, y),
                        Size = new Size(120, 40)
                    };

                    btn.Click += (s, ev) =>
                    {
                        datagridsales.Rows.Add(
                            productId,
                            itemName,
                            1,
                            price,
                            0,
                            price,
                            barcode,
                            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            _username,
                            ""
                        );
                    };

                    grpitems.Controls.Add(btn);

                    x += 130;
                    if (x > grpitems.Width - 130)
                    {
                        x = 10;
                        y += 50;
                    }
                }
            }
        }

        // زر الحفظ
        private void btnsave_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    SqlCommand cmdSale = new SqlCommand(
                        "INSERT INTO Sales (user_id, customer_id, total_amount, payment_method) OUTPUT INSERTED.sale_id VALUES (@user, @cust, @total, @method)", conn, tran);

                    cmdSale.Parameters.AddWithValue("@user", currentUserId);
                    cmdSale.Parameters.AddWithValue("@cust", currentCustomerId);
                    cmdSale.Parameters.AddWithValue("@total", CalculateTotal());
                    cmdSale.Parameters.AddWithValue("@method", "cash");

                    int saleId = (int)cmdSale.ExecuteScalar();

                    foreach (DataGridViewRow row in datagridsales.Rows)
                    {
                        if (row.IsNewRow) continue;

                        int productId = Convert.ToInt32(row.Cells["product_id"].Value);
                        int qty = Convert.ToInt32(row.Cells["quantity"].Value);
                        decimal price = Convert.ToDecimal(row.Cells["unit_price"].Value);
                        decimal discount = Convert.ToDecimal(row.Cells["discount"].Value);

                        SqlCommand cmdItem = new SqlCommand(
                            "INSERT INTO Sale_Items (sale_id, product_id, quantity, unit_price, discount) VALUES (@saleId, @pid, @qty, @price, @discount)", conn, tran);

                        cmdItem.Parameters.AddWithValue("@saleId", saleId);
                        cmdItem.Parameters.AddWithValue("@pid", productId);
                        cmdItem.Parameters.AddWithValue("@qty", qty);
                        cmdItem.Parameters.AddWithValue("@price", price);
                        cmdItem.Parameters.AddWithValue("@discount", discount);

                        cmdItem.ExecuteNonQuery();
                    }

                    tran.Commit();
                    MessageBox.Show("Sale saved successfully!");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    MessageBox.Show("Error saving sale: " + ex.Message);
                }
            }
        }

        private decimal CalculateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in datagridsales.Rows)
            {
                if (row.IsNewRow) continue;
                total += Convert.ToDecimal(row.Cells["quantity"].Value) * Convert.ToDecimal(row.Cells["unit_price"].Value);
            }
            return total;
        }

        private void Sales_Load(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT value FROM Settings WHERE key_name='default_price'", conn);
                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    txtdollar.Text = result.ToString(); // عرض السعر في TextBox
                }
            }



            // TODO: This line of code loads data into the 'pos_systemDataSet3.Customers' table. You can move, or remove it, as needed.
            this.customersTableAdapter1.Fill(this.pos_systemDataSet3.Customers);
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT category_name FROM Categories", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    comboProducts.Items.Add(reader["category_name"].ToString());
                }
            }

            datagridsales.Columns.Clear();
            datagridsales.Columns.Add("product_id", "رقم المنتج");
            datagridsales.Columns.Add("itemname", "اسم المنتج");
            datagridsales.Columns.Add("quantity", "الكمية");
            datagridsales.Columns.Add("unit_price", "سعر الوحدة");
            datagridsales.Columns.Add("discount", "الخصم");
            datagridsales.Columns.Add("total", "الإجمالي");
            datagridsales.Columns.Add("barcode", "الباركود");
            datagridsales.Columns.Add("sale_date", "تاريخ البيع");
            datagridsales.Columns.Add("user", "المستخدم");
            datagridsales.Columns.Add("customer", "العميل");

            datagridsales.Columns["unit_price"].DefaultCellStyle.Format = "C2";
            datagridsales.Columns["total"].DefaultCellStyle.Format = "C2";
            datagridsales.Columns["sale_date"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
        }

        private void btndollar_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Settings SET value=@val WHERE key_name='default_price'", conn);
                cmd.Parameters.AddWithValue("@val", txtdollar.Text);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Price updated successfully!");
            }

        }
    }
}