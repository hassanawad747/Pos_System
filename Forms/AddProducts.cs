using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class AddProducts : Form
    {
        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";
        private int? productId = null; // إذا null → إضافة، إذا فيه قيمة → تعديل

        // Constructor للإضافة
        public AddProducts()
        {
            InitializeComponent();
        }

        // Constructor للتعديل
        public AddProducts(int id)
        {
            InitializeComponent();
            productId = id;
        }

        private void AddProducts_Load(object sender, EventArgs e)
        {


            LoadCategories();
            LoadSuppliers();

            this.productsTableAdapter.Fill(this.pos_systemDataSet11.Products);
            dataGridView1.DataSource = pos_systemDataSet11.Products;

            // إذا تعديل → حمّل بيانات المنتج
            if (productId.HasValue)
            {
                LoadProductData(productId.Value);
                btnadd.Text = "Update Product"; // غيّر نص الزر
            }

            // تحميل المنتجات وربطها بالـ DataGridView
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT product_id, name, category_id, price_usd, price_lb, exchange_rate, sale_price_usd, sale_price_lb, stock_quantity, barcode, supplier_id FROM Products", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }

            // إضافة زر Edit
            DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
            btnEdit.HeaderText = "Edit";
            btnEdit.Text = "Edit";
            btnEdit.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(btnEdit);

            // إضافة زر Delete
            DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
            btnDelete.HeaderText = "Delete";
            btnDelete.Text = "Delete";
            btnDelete.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(btnDelete);
        }
        

        private void btnadd_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd;

                if (productId.HasValue)
                {
                    // تعديل منتج
                    cmd = new SqlCommand(@"
                    UPDATE Products SET
                        name=@name,
                        category_id=@cat,
                        price_usd=@price_usd,
                        price_lb=@price_lb,
                        exchange_rate=@exchange_rate,
                        sale_price_usd=@sale_usd,
                        sale_price_lb=@sale_lb,
                        stock_quantity=@stock,
                        barcode=@barcode,
                        supplier_id=@supplier
                    WHERE product_id=@id", conn);

                    cmd.Parameters.AddWithValue("@id", productId.Value);
                }
                else
                {
                    // إضافة منتج جديد
                    cmd = new SqlCommand(@"
                    INSERT INTO Products 
                    (name, category_id, price_usd, price_lb, exchange_rate, sale_price_usd, sale_price_lb, stock_quantity, barcode, supplier_id)
                    VALUES (@name, @cat, @price_usd, @price_lb, @exchange_rate, @sale_usd, @sale_lb, @stock, @barcode, @supplier)", conn);
                }

                // القيم من TextBox و ComboBox
                cmd.Parameters.AddWithValue("@name", txtname.Text);
                cmd.Parameters.AddWithValue("@cat", (int)comboitem.SelectedValue);

                decimal priceUsd = Convert.ToDecimal(txtpricedollar.Text);
                decimal exchangeRate = Convert.ToDecimal(txtdollar.Text);
                decimal priceLb = priceUsd * exchangeRate;
                decimal saleUsd = Convert.ToDecimal(txtsaledollar.Text);
                decimal saleLb = saleUsd * exchangeRate;

                cmd.Parameters.AddWithValue("@price_usd", priceUsd);
                cmd.Parameters.AddWithValue("@exchange_rate", exchangeRate);
                cmd.Parameters.AddWithValue("@price_lb", priceLb);
                cmd.Parameters.AddWithValue("@sale_usd", saleUsd);
                cmd.Parameters.AddWithValue("@sale_lb", saleLb);

                cmd.Parameters.AddWithValue("@stock", Convert.ToInt32(txtquentity.Text));
                cmd.Parameters.AddWithValue("@barcode", txtbarcode.Text);
                cmd.Parameters.AddWithValue("@supplier", (int)comboSupplier.SelectedValue);

                cmd.ExecuteNonQuery();

                if (productId.HasValue)
                    MessageBox.Show("✅ تم تعديل المنتج بنجاح");
                else
                    MessageBox.Show("✅ تم إضافة المنتج بنجاح");

                Products productsForm = new Products();
                productsForm.Show();
                this.Close();
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT category_id, category_name FROM Categories", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(reader);

                comboitem.DataSource = dt;
                comboitem.DisplayMember = "category_name";
                comboitem.ValueMember = "category_id";
            }
        }

        private void LoadSuppliers()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT supplier_id, name FROM Suppliers", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(reader);

                comboSupplier.DataSource = dt;
                comboSupplier.DisplayMember = "name";
                comboSupplier.ValueMember = "supplier_id";
            }
        }

        private void LoadProductData(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Products WHERE product_id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtname.Text = reader["name"].ToString();
                    comboitem.SelectedValue = reader["category_id"];
                    txtpricedollar.Text = reader["price_usd"].ToString();
                    txtdollar.Text = reader["exchange_rate"].ToString();
                    txtsaledollar.Text = reader["sale_price_usd"].ToString();
                    txtquentity.Text = reader["stock_quantity"].ToString();
                    txtbarcode.Text = reader["barcode"].ToString();
                    comboSupplier.SelectedValue = reader["supplier_id"];
                }
            }
        }

        // ✅ تحميل بيانات المنتج بالباركود
        private void LoadProductDataByBarcode(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Products WHERE barcode=@barcode", conn);
                cmd.Parameters.AddWithValue("@barcode", barcode);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtname.Text = reader["name"].ToString();
                    comboitem.SelectedValue = reader["category_id"];
                    txtpricedollar.Text = reader["price_usd"].ToString();
                    txtdollar.Text = reader["exchange_rate"].ToString();
                    txtsaledollar.Text = reader["sale_price_usd"].ToString();
                    txtquentity.Text = reader["stock_quantity"].ToString();
                    txtbarcode.Text = reader["barcode"].ToString();
                    comboSupplier.SelectedValue = reader["supplier_id"];
                }
                else
                {
                    // إذا ما وجد المنتج
                    MessageBox.Show("⚠️ المنتج غير موجود بهذا الباركود");
                }
            }
        }

        private void txtbarcode_TextChanged(object sender, EventArgs e)
        {
            LoadProductDataByBarcode(txtbarcode.Text);
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm();
            supplierForm.ShowDialog();
            this.Close();
        }

        private void btnskip_Click(object sender, EventArgs e)
        {
            Products productsForm = new Products();
            productsForm.ShowDialog();
            this.Close();
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // زر Edit
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Edit")
                {
                    int productId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["product_id"].Value);

                    string newName = dataGridView1.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    decimal newPriceUsd = Convert.ToDecimal(dataGridView1.Rows[e.RowIndex].Cells["price_usd"].Value);
                    int newCategoryId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["category_id"].Value);
                    int newSupplierId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["supplier_id"].Value);

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(@"UPDATE Products 
                                                  SET name=@name, price_usd=@price_usd, category_id=@cat, supplier_id=@sup 
                                                  WHERE product_id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", productId);
                        cmd.Parameters.AddWithValue("@name", newName);
                        cmd.Parameters.AddWithValue("@price_usd", newPriceUsd);
                        cmd.Parameters.AddWithValue("@cat", newCategoryId);
                        cmd.Parameters.AddWithValue("@sup", newSupplierId);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("✅ تم تعديل المنتج بنجاح");
                }

                // زر Delete
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
                {
                    int productId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["product_id"].Value);

                    var confirm = MessageBox.Show("هل تريد حذف المنتج؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                    if (confirm == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM Products WHERE product_id=@id", conn);
                            cmd.Parameters.AddWithValue("@id", productId);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("✅ تم حذف المنتج بنجاح");

                        // إعادة تحميل البيانات
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            SqlDataAdapter da = new SqlDataAdapter("SELECT product_id, name, category_id, price_usd, price_lb, exchange_rate, sale_price_usd, sale_price_lb, stock_quantity, barcode, supplier_id FROM Products", conn);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dataGridView1.DataSource = dt;
                        }
                    }
                }
            }
        }



    }
}
