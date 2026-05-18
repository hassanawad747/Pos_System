using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public partial class AddProducts : Form
    {
        private const string EditColumnName = "ProductEditButton";
        private const string DeleteColumnName = "ProductDeleteButton";

        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private readonly int? productId;
        private readonly bool openProductsAfterSave;
        private int? editingProductId;
        private bool loadingProduct;
        private bool allowGridSelectionLoad;

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
            editingProductId = productId;

            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
            txtpricedollar.TextChanged += PriceInput_TextChanged;
            txtdollar.TextChanged += PriceInput_TextChanged;
            txtsaledollar.TextChanged += PriceInput_TextChanged;
            txtname.TextChanged += txtname_TextChanged;
            panel1.Resize += (s, e) => LayoutBottomButtons();
            Resize += (s, e) => LayoutBottomButtons();
        }

        private void AddProducts_Load(object sender, EventArgs e)
        {
            if (!PermissionService.EnsureScreenAccess(this, PermissionService.ScreenProducts))
            {
                return;
            }

            EnsureActionColumns();
            LoadCategories();
            LoadSuppliers();
            RefreshProductsGrid();
            UseSettingsExchangeRate();
            HideLocalExchangeRateInput();
            SetupBarcodeMode();
            LayoutBottomButtons();

            if (editingProductId.HasValue)
            {
                LoadProductData(editingProductId.Value);
                btnadd.Text = "Update Product";
            }

            UpdateConvertedPrices();
            PermissionService.ApplyActionPermissions(this, PermissionService.ScreenProducts);
            allowGridSelectionLoad = true;
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            if (editingProductId.HasValue)
            {
                if (!PermissionService.CanEdit(AppSession.UserId, AppSession.Role, PermissionService.ScreenProducts))
                {
                    MessageBox.Show("You do not have permission to update products.");
                    return;
                }
            }
            else if (!PermissionService.CanCreate(AppSession.UserId, AppSession.Role, PermissionService.ScreenProducts))
            {
                MessageBox.Show("You do not have permission to create products.");
                return;
            }

            if (!TryBuildProductValues(out ProductValues values))
            {
                return;
            }

            bool wasEditing = editingProductId.HasValue;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                using (SqlCommand cmd = conn.CreateCommand())
                {
                    if (editingProductId.HasValue)
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
                        cmd.Parameters.AddWithValue("@id", editingProductId.Value);
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
                    cmd.Parameters.AddWithValue("@barcode", (object)values.Barcode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@supplierId", values.SupplierId);
                    cmd.ExecuteNonQuery();
                }
            }

            AuditService.Log(
                "Products",
                editingProductId.HasValue ? "Edit" : "Create",
                editingProductId.HasValue ? editingProductId.Value.ToString() : (values.Barcode ?? values.Name),
                (editingProductId.HasValue ? "Updated product " : "Created product ") + values.Name);
            if (wasEditing && !productId.HasValue)
            {
                MessageBox.Show("Product updated successfully.");
                RefreshProductsGrid();
                return;
            }
            MessageBox.Show(productId.HasValue ? "✅ تم تعديل المنتج بنجاح" : "✅ تم إضافة المنتج بنجاح");
            RefreshProductsGrid();

            if (wasEditing && !productId.HasValue)
            {
                return;
            }

            if (productId.HasValue)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            if (!openProductsAfterSave)
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
            bool requireBarcode = BarcodeIsRequired();

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

            if (requireBarcode && string.IsNullOrWhiteSpace(barcode))
            {
                MessageBox.Show("Please enter barcode, or choose No if this product has no barcode.");
                txtbarcode.Focus();
                return false;
            }

            if (!decimal.TryParse(txtpricedollar.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal priceUsd) &&
                !decimal.TryParse(txtpricedollar.Text.Trim(), out priceUsd))
            {
                MessageBox.Show("يرجى إدخال سعر شراء صحيح بالدولار.");
                txtpricedollar.Focus();
                return false;
            }

            decimal exchangeRate = POS_System.Program.SettingsManager.GetExchangeRate();

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
                Barcode = string.IsNullOrWhiteSpace(barcode) ? null : barcode,
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

                    FillProductInputs(reader);
                }
            }
        }

        private void LoadProductDataByBarcode(string barcode)
        {
            if (loadingProduct || string.IsNullOrWhiteSpace(barcode))
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

                    FillProductInputs(reader);
                }
            }
        }

        private void LoadProductDataByName(string productName)
        {
            if (loadingProduct || string.IsNullOrWhiteSpace(productName))
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM Products WHERE name = @name ORDER BY product_id DESC", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@name", productName.Trim());

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        FillProductInputs(reader);
                    }
                }
            }
        }

        private void FillProductInputs(SqlDataReader reader)
        {
            loadingProduct = true;
            try
            {
                editingProductId = Convert.ToInt32(reader["product_id"]);
                txtname.Text = Convert.ToString(reader["name"]);
                comboitem.SelectedValue = Convert.ToInt32(reader["category_id"]);
                txtpricedollar.Text = Convert.ToString(reader["price_usd"]);
                txtpriceLebanon.Text = Convert.ToString(reader["price_lb"]);
                UseSettingsExchangeRate();
                txtsaledollar.Text = Convert.ToString(reader["sale_price_usd"]);
                txtsalelebanon.Text = Convert.ToString(reader["sale_price_lb"]);
                txtquentity.Text = Convert.ToString(reader["stock_quantity"]);
                txtbarcode.Text = Convert.ToString(reader["barcode"]);
                combobarcode.SelectedIndex = string.IsNullOrWhiteSpace(txtbarcode.Text) ? 1 : 0;
                comboSupplier.SelectedValue = Convert.ToInt32(reader["supplier_id"]);
                btnadd.Text = "Update Product";
            }
            finally
            {
                loadingProduct = false;
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
            UseSettingsExchangeRate();
            txtsaledollar.Clear();
            txtsalelebanon.Clear();
            txtquentity.Clear();
            txtbarcode.Clear();
            combobarcode.SelectedIndex = 0;
            comboitem.SelectedIndex = -1;
            comboSupplier.SelectedIndex = -1;
            editingProductId = null;
            btnadd.Text = "اضافة المنتج";
            txtname.Focus();
        }

        private void UpdateConvertedPrices()
        {
            decimal exchangeRate = POS_System.Program.SettingsManager.GetExchangeRate();

            if (TryParseDecimal(txtpricedollar.Text, out decimal priceUsd))
            {
                txtpriceLebanon.Text = (priceUsd * exchangeRate).ToString("0.##");
            }
            else
            {
                txtpriceLebanon.Clear();
            }

            if (TryParseDecimal(txtsaledollar.Text, out decimal salePriceUsd))
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

        private void UseSettingsExchangeRate()
        {
            txtdollar.Text = POS_System.Program.SettingsManager.GetExchangeRate().ToString("0.####", CultureInfo.InvariantCulture);
        }

        private void HideLocalExchangeRateInput()
        {
            txtdollar.Visible = false;
            label11.Visible = false;
        }

        private void txtbarcode_TextChanged(object sender, EventArgs e)
        {
            LoadProductDataByBarcode(txtbarcode.Text);
        }

        private void txtname_TextChanged(object sender, EventArgs e)
        {
            LoadProductDataByName(txtname.Text);
        }

        private void SetupBarcodeMode()
        {
            combobarcode.DropDownStyle = ComboBoxStyle.DropDownList;
            if (combobarcode.Items.Count == 0)
            {
                combobarcode.Items.Add("Yes");
                combobarcode.Items.Add("No");
            }

            if (combobarcode.SelectedIndex < 0)
            {
                combobarcode.SelectedIndex = 0;
            }
        }

        private bool BarcodeIsRequired()
        {
            string selected = Convert.ToString(combobarcode.SelectedItem).Trim();
            return selected.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || selected.Equals("نعم", StringComparison.OrdinalIgnoreCase);
        }

        private void LayoutBottomButtons()
        {
            if (panel1 == null || btnadd == null || btnexit == null)
            {
                return;
            }

            const int margin = 12;
            int top = Math.Max(8, (panel1.ClientSize.Height - btnadd.Height) / 2);

            btnadd.Location = new Point(Math.Max(margin, panel1.ClientSize.Width - btnadd.Width - margin), top);
            btnexit.Location = new Point(Math.Max(margin, btnadd.Left - btnexit.Width - 8), top);
            btnadd.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnexit.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
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
                if (!PermissionService.CanDelete(AppSession.UserId, AppSession.Role, PermissionService.ScreenProducts))
                {
                    MessageBox.Show("You do not have permission to delete products.");
                    return;
                }
                DialogResult confirm = MessageBox.Show("هل تريد حذف المنتج؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(@"
                    SET XACT_ABORT ON;
                    BEGIN TRANSACTION;

                    IF OBJECT_ID(N'dbo.Returns', N'U') IS NOT NULL
                        DELETE FROM dbo.Returns WHERE product_id = @id;

                    IF OBJECT_ID(N'dbo.Sale_Items', N'U') IS NOT NULL
                        DELETE FROM dbo.Sale_Items WHERE product_id = @id;

                    IF OBJECT_ID(N'dbo.InventoryLogs', N'U') IS NOT NULL
                       AND COL_LENGTH('dbo.InventoryLogs', 'product_id') IS NOT NULL
                        DELETE FROM dbo.InventoryLogs WHERE product_id = @id;

                    DELETE FROM dbo.Products WHERE product_id = @id;

                    COMMIT TRANSACTION;", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", selectedProductId);
                    cmd.ExecuteNonQuery();
                }

                AuditService.Log("Products", "Delete", selectedProductId.ToString(), "Deleted product ID " + selectedProductId);
                MessageBox.Show("✅ تم حذف المنتج بنجاح");
                RefreshProductsGrid();
                return;
            }

            LoadProductData(selectedProductId);
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (!allowGridSelectionLoad || loadingProduct || dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.IsNewRow)
            {
                return;
            }

            if (!dataGridView1.Columns.Contains("product_id"))
            {
                return;
            }

            object value = dataGridView1.CurrentRow.Cells["product_id"].Value;
            if (value == null || value == DBNull.Value)
            {
                return;
            }

            if (int.TryParse(Convert.ToString(value), out int selectedProductId))
            {
                LoadProductData(selectedProductId);
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
