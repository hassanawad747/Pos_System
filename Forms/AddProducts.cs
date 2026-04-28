using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class AddProducts : Form
    {
        private const string EditColumnName = "ProductEditButton";
        private const string DeleteColumnName = "ProductDeleteButton";

        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private readonly int? productId;
        private readonly bool openProductsAfterSave;

        public AddProducts() : this(null, false)
        {
        }

        public AddProducts(bool openProductsAfterSave) : this(null, openProductsAfterSave)
        {
        }

        public AddProducts(int productId) : this(productId, false)
        {
        }

        private AddProducts(int? productId, bool openProductsAfterSave)
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);

            this.productId = productId;
            this.openProductsAfterSave = openProductsAfterSave;

            dataGridView1.CellClick += dataGridView1_CellClick;
            txtpricedollar.TextChanged += PriceInput_TextChanged;
            txtdollar.TextChanged += PriceInput_TextChanged;
            txtsaledollar.TextChanged += PriceInput_TextChanged;
        }

        private void AddProducts_Load(object sender, EventArgs e)
        {
            EnsureActionColumns();
            LoadCategories();
            LoadSuppliers();
            RefreshProductsGrid();

            if (productId.HasValue)
            {
                LoadProductData(productId.Value);
                btnadd.Text = "Update Product";
            }

            UpdateConvertedPrices();
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            if (!TryBuildProductValues(out ProductValues values))
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (productId.HasValue)
                    {
                        cmd.CommandText = @"
                            UPDATE Products
                            SET name = @name,
                                category_id = @categoryId,
                                price_usd = @priceUsd,
                                price_lb = @priceLb,
                                exchange_rate = @exchangeRate,
                                sale_price_usd = @salePriceUsd,
                                sale_price_lb = @salePriceLb,
                                stock_quantity = @stockQuantity,
                                barcode = @barcode,
                                supplier_id = @supplierId
                            WHERE product_id = @id";
                        cmd.Parameters.AddWithValue("@id", productId.Value);
                    }
                    else
                    {
                        cmd.CommandText = @"
                            INSERT INTO Products
                            (name, category_id, price_usd, price_lb, exchange_rate, sale_price_usd, sale_price_lb, stock_quantity, barcode, supplier_id)
                            VALUES
                            (@name, @categoryId, @priceUsd, @priceLb, @exchangeRate, @salePriceUsd, @salePriceLb, @stockQuantity, @barcode, @supplierId)";
                    }

                    cmd.Parameters.AddWithValue("@name", values.Name);
                    cmd.Parameters.AddWithValue("@categoryId", values.CategoryId);
                    cmd.Parameters.AddWithValue("@priceUsd", values.PriceUsd);
                    cmd.Parameters.AddWithValue("@priceLb", values.PriceLb);
                    cmd.Parameters.AddWithValue("@exchangeRate", values.ExchangeRate);
                    cmd.Parameters.AddWithValue("@salePriceUsd", values.SalePriceUsd);
                    cmd.Parameters.AddWithValue("@salePriceLb", values.SalePriceLb);
                    cmd.Parameters.AddWithValue("@stockQuantity", values.StockQuantity);
                    cmd.Parameters.AddWithValue("@barcode", values.Barcode);
                    cmd.Parameters.AddWithValue("@supplierId", values.SupplierId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show(productId.HasValue ? "✅ تم تعديل المنتج بنجاح" : "✅ تم إضافة المنتج بنجاح");
            RefreshProductsGrid();

            //if (openProductsAfterSave)
            //{
            //    OpenProductsFormAndClose();
            //    return;
            //}

            if (productId.HasValue)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            ClearInputs();
        }

        private bool TryBuildProductValues(out ProductValues values)
        {
            values = new ProductValues();

            string productName = txtname.Text.Trim();
            string barcode = txtbarcode.Text.Trim();

            if (string.IsNullOrWhiteSpace(productName))
            {
                MessageBox.Show("يرجى إدخال اسم المنتج.");
                txtname.Focus();
                return false;
            }

            if (!(comboitem.SelectedValue is int categoryId))
            {
                MessageBox.Show("يرجى اختيار الصنف.");
                comboitem.Focus();
                return false;
            }

            if (!(comboSupplier.SelectedValue is int supplierId))
            {
                MessageBox.Show("يرجى اختيار المورد.");
                comboSupplier.Focus();
                return false;
            }

            if (!decimal.TryParse(txtpricedollar.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal priceUsd) &&
                !decimal.TryParse(txtpricedollar.Text.Trim(), out priceUsd))
            {
                MessageBox.Show("يرجى إدخال سعر شراء صحيح بالدولار.");
                txtpricedollar.Focus();
                return false;
            }

            if (!decimal.TryParse(txtdollar.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal exchangeRate) &&
                !decimal.TryParse(txtdollar.Text.Trim(), out exchangeRate))
            {
                MessageBox.Show("يرجى إدخال سعر صرف صحيح.");
                txtdollar.Focus();
                return false;
            }

            if (!decimal.TryParse(txtsaledollar.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal salePriceUsd) &&
                !decimal.TryParse(txtsaledollar.Text.Trim(), out salePriceUsd))
            {
                MessageBox.Show("يرجى إدخال سعر بيع صحيح بالدولار.");
                txtsaledollar.Focus();
                return false;
            }

            if (!int.TryParse(txtquentity.Text.Trim(), out int stockQuantity))
            {
                MessageBox.Show("يرجى إدخال كمية صحيحة.");
                txtquentity.Focus();
                return false;
            }

            values = new ProductValues
            {
                Name = productName,
                CategoryId = categoryId,
                PriceUsd = priceUsd,
                ExchangeRate = exchangeRate,
                PriceLb = priceUsd * exchangeRate,
                SalePriceUsd = salePriceUsd,
                SalePriceLb = salePriceUsd * exchangeRate,
                StockQuantity = stockQuantity,
                Barcode = barcode,
                SupplierId = supplierId
            };

            txtpriceLebanon.Text = values.PriceLb.ToString("0.##");
            txtsalelebanon.Text = values.SalePriceLb.ToString("0.##");
            return true;
        }

        private void RefreshProductsGrid()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT product_id, name, category_id, stock_quantity, barcode, supplier_id,
                         created_at, price_usd, price_lb, exchange_rate, sale_price_usd, sale_price_lb
                  FROM Products
                  ORDER BY product_id DESC",
                conn))
            {
                conn.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
                }
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT category_id, category_name FROM Categories ORDER BY category_name",
                conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    comboitem.DataSource = table;
                    comboitem.DisplayMember = "category_name";
                    comboitem.ValueMember = "category_id";
                    comboitem.SelectedIndex = -1;
                }
            }
        }

        private void LoadSuppliers()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT supplier_id, name FROM Suppliers ORDER BY name",
                conn))
            {
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    DataTable table = new DataTable();
                    table.Load(reader);
                    comboSupplier.DataSource = table;
                    comboSupplier.DisplayMember = "name";
                    comboSupplier.ValueMember = "supplier_id";
                    comboSupplier.SelectedIndex = -1;
                }
            }
        }

        private void LoadProductData(int id)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Products WHERE product_id = @id", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@id", id);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return;
                    }

                    txtname.Text = Convert.ToString(reader["name"]);
                    comboitem.SelectedValue = Convert.ToInt32(reader["category_id"]);
                    txtpricedollar.Text = Convert.ToString(reader["price_usd"]);
                    txtpriceLebanon.Text = Convert.ToString(reader["price_lb"]);
                    txtdollar.Text = Convert.ToString(reader["exchange_rate"]);
                    txtsaledollar.Text = Convert.ToString(reader["sale_price_usd"]);
                    txtsalelebanon.Text = Convert.ToString(reader["sale_price_lb"]);
                    txtquentity.Text = Convert.ToString(reader["stock_quantity"]);
                    txtbarcode.Text = Convert.ToString(reader["barcode"]);
                    comboSupplier.SelectedValue = Convert.ToInt32(reader["supplier_id"]);
                }
            }
        }

        private void LoadProductDataByBarcode(string barcode)
        {
            if (productId.HasValue || string.IsNullOrWhiteSpace(barcode))
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Products WHERE barcode = @barcode", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@barcode", barcode.Trim());

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return;
                    }

                    txtname.Text = Convert.ToString(reader["name"]);
                    comboitem.SelectedValue = Convert.ToInt32(reader["category_id"]);
                    txtpricedollar.Text = Convert.ToString(reader["price_usd"]);
                    txtpriceLebanon.Text = Convert.ToString(reader["price_lb"]);
                    txtdollar.Text = Convert.ToString(reader["exchange_rate"]);
                    txtsaledollar.Text = Convert.ToString(reader["sale_price_usd"]);
                    txtsalelebanon.Text = Convert.ToString(reader["sale_price_lb"]);
                    txtquentity.Text = Convert.ToString(reader["stock_quantity"]);
                    comboSupplier.SelectedValue = Convert.ToInt32(reader["supplier_id"]);
                }
            }
        }

        private void EnsureActionColumns()
        {
            if (dataGridView1.Columns[EditColumnName] == null)
            {
                dataGridView1.Columns.Add(new DataGridViewButtonColumn
                {
                    Name = EditColumnName,
                    HeaderText = "Edit",
                    Text = "Edit",
                    UseColumnTextForButtonValue = true
                });
            }

            if (dataGridView1.Columns[DeleteColumnName] == null)
            {
                dataGridView1.Columns.Add(new DataGridViewButtonColumn
                {
                    Name = DeleteColumnName,
                    HeaderText = "Delete",
                    Text = "Delete",
                    UseColumnTextForButtonValue = true
                });
            }
        }

        private void ClearInputs()
        {
            txtname.Clear();
            txtpricedollar.Clear();
            txtpriceLebanon.Clear();
            txtdollar.Clear();
            txtsaledollar.Clear();
            txtsalelebanon.Clear();
            txtquentity.Clear();
            txtbarcode.Clear();
            combobarcode.SelectedIndex = -1;
            comboitem.SelectedIndex = -1;
            comboSupplier.SelectedIndex = -1;
            txtname.Focus();
        }

        private void UpdateConvertedPrices()
        {
            if (TryParseDecimal(txtpricedollar.Text, out decimal priceUsd) &&
                TryParseDecimal(txtdollar.Text, out decimal exchangeRate))
            {
                txtpriceLebanon.Text = (priceUsd * exchangeRate).ToString("0.##");
            }
            else
            {
                txtpriceLebanon.Clear();
            }

            if (TryParseDecimal(txtsaledollar.Text, out decimal salePriceUsd) &&
                TryParseDecimal(txtdollar.Text, out exchangeRate))
            {
                txtsalelebanon.Text = (salePriceUsd * exchangeRate).ToString("0.##");
            }
            else
            {
                txtsalelebanon.Clear();
            }
        }

        private static bool TryParseDecimal(string input, out decimal value)
        {
            return decimal.TryParse(input?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value) ||
                   decimal.TryParse(input?.Trim(), out value);
        }

        private void PriceInput_TextChanged(object sender, EventArgs e)
        {
            UpdateConvertedPrices();
        }

        private void txtbarcode_TextChanged(object sender, EventArgs e)
        {
            LoadProductDataByBarcode(txtbarcode.Text);
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnskip_Click(object sender, EventArgs e)
        {
            OpenProductsFormAndClose();
        }

        private void OpenProductsFormAndClose()
        {
            using (Products productsForm = new Products())
            {
                Hide();
                productsForm.ShowDialog(GetDialogOwner());
            }

            Close();
        }

        private IWin32Window GetDialogOwner()
        {
            return TopLevelControl as IWin32Window ?? this;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            int selectedProductId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["product_id"].Value);

            if (columnName == EditColumnName)
            {
                using (AddProducts editForm = new AddProducts(selectedProductId))
                {
                    editForm.ShowDialog(GetDialogOwner());
                }

                RefreshProductsGrid();
                return;
            }

            if (columnName == DeleteColumnName)
            {
                DialogResult confirm = MessageBox.Show("هل تريد حذف المنتج؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Products WHERE product_id = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", selectedProductId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ تم حذف المنتج بنجاح");
                RefreshProductsGrid();
            }
        }

        private struct ProductValues
        {
            public string Name;
            public int CategoryId;
            public decimal PriceUsd;
            public decimal PriceLb;
            public decimal ExchangeRate;
            public decimal SalePriceUsd;
            public decimal SalePriceLb;
            public int StockQuantity;
            public string Barcode;
            public int SupplierId;
        }
    }
}
