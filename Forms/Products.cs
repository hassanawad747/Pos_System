using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public partial class Products : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;

        public Products()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            ConfigureResponsiveLayout();
        }

        private void Products_Load(object sender, EventArgs e)
        {
            EnsureProductLookupColumns();
            RefreshAllGrids();
            LayoutInventoryControls();
        }

        private void ConfigureResponsiveLayout()
        {
            MinimumSize = new Size(900, 520);
            panel1.Resize += (sender, args) => LayoutInventoryControls();
            Resize += (sender, args) => LayoutInventoryControls();

            panel1.Dock = DockStyle.Fill;
            panel4.Dock = DockStyle.None;
            panel2.Dock = DockStyle.None;
            panel3.Dock = DockStyle.None;

            datagridProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            datagridCategories.Dock = DockStyle.Fill;
            datagridSuppliers.Dock = DockStyle.Fill;
        }

        private void EnsureProductLookupColumns()
        {
            if (!datagridProducts.Columns.Contains("category"))
            {
                datagridProducts.Columns.Insert(
                    Math.Min(2, datagridProducts.Columns.Count),
                    new DataGridViewTextBoxColumn
                    {
                        Name = "category",
                        DataPropertyName = "category",
                        HeaderText = "Category",
                        ReadOnly = true
                    });
            }

            if (!datagridProducts.Columns.Contains("supplier"))
            {
                datagridProducts.Columns.Insert(
                    Math.Min(3, datagridProducts.Columns.Count),
                    new DataGridViewTextBoxColumn
                    {
                        Name = "supplier",
                        DataPropertyName = "supplier",
                        HeaderText = "Supplier",
                        ReadOnly = true
                    });
            }
        }

        private void LayoutInventoryControls()
        {
            if (panel1 == null || panel1.ClientSize.Width <= 0 || panel1.ClientSize.Height <= 0)
            {
                return;
            }

            int margin = 12;
            int gap = 10;
            int buttonWidth = 115;
            int buttonHeight = 48;
            int topRowHeight = 66;

            button2.SetBounds(
                Math.Max(margin, panel1.ClientSize.Width - buttonWidth - margin),
                margin,
                buttonWidth,
                buttonHeight);

            button1.SetBounds(
                Math.Max(margin, button2.Left - buttonWidth - gap),
                margin,
                buttonWidth,
                buttonHeight);

            int searchLeft = margin;
            int searchWidth = Math.Max(220, button1.Left - searchLeft - gap);
            txtsearch.SetBounds(searchLeft, margin + 8, searchWidth, 40);

            int bottomHeight = Math.Max(180, Math.Min(280, panel1.ClientSize.Height / 3));
            panel4.SetBounds(
                0,
                Math.Max(topRowHeight + 180, panel1.ClientSize.Height - bottomHeight),
                panel1.ClientSize.Width,
                bottomHeight);

            datagridProducts.SetBounds(
                margin,
                topRowHeight + gap,
                Math.Max(0, panel1.ClientSize.Width - (margin * 2)),
                Math.Max(120, panel4.Top - topRowHeight - (gap * 2)));

            int halfWidth = Math.Max(0, (panel4.ClientSize.Width - gap) / 2);
            panel3.SetBounds(0, 0, halfWidth, panel4.ClientSize.Height);
            panel2.SetBounds(halfWidth + gap, 0, Math.Max(0, panel4.ClientSize.Width - halfWidth - gap), panel4.ClientSize.Height);
        }

        private void RefreshAllGrids()
        {
            string keyword = txtsearch.Text.Trim();
            RefreshProductsGrid(keyword);
            RefreshCategoriesGrid(keyword);
            RefreshSuppliersGrid(keyword);
        }

        private void RefreshProductsGrid(string keyword = "")
        {
            EnsureProductLookupColumns();

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(BuildProductsSearchQuery(keyword), conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@hasKeyword", !string.IsNullOrWhiteSpace(keyword));
                cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");

                if (int.TryParse(keyword, out int number))
                {
                    cmd.Parameters.AddWithValue("@number", number);
                }

                if (DateTime.TryParse(keyword, out DateTime searchDate))
                {
                    cmd.Parameters.AddWithValue("@searchDate", searchDate.Date);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    datagridProducts.DataSource = table;
                    FormatProductsGrid();
                }
            }
        }

        private static string BuildProductsSearchQuery(string keyword)
        {
            string query = @"
                SELECT
                    p.product_id,
                    p.name,
                    c.category_name AS category,
                    s.name AS supplier,
                    p.price_usd,
                    p.price_lb,
                    p.sale_price_usd,
                    p.sale_price_lb,
                    p.stock_quantity,
                    p.barcode,
                    p.exchange_rate,
                    p.category_id,
                    p.supplier_id,
                    p.created_at
                FROM Products p
                LEFT JOIN Categories c ON TRY_CONVERT(INT, p.category_id) = c.category_id
                LEFT JOIN Suppliers s ON TRY_CONVERT(INT, p.supplier_id) = s.supplier_id
                WHERE @hasKeyword = 0
                   OR p.name LIKE @keyword
                   OR p.barcode LIKE @keyword";

            if (int.TryParse(keyword, out _))
            {
                query += @"
                   OR DAY(p.created_at) = @number
                   OR MONTH(p.created_at) = @number
                   OR YEAR(p.created_at) = @number";
            }

            if (DateTime.TryParse(keyword, out _))
            {
                query += @"
                   OR CAST(p.created_at AS DATE) = @searchDate";
            }

            query += " ORDER BY p.product_id DESC";
            return query;
        }

        private void FormatProductsGrid()
        {
            SetColumnHeader("product_id", "Product ID");
            SetColumnHeader("name", "Product Name");
            SetColumnHeader("category", "Category");
            SetColumnHeader("supplier", "Supplier");
            SetColumnHeader("price_usd", "Cost USD");
            SetColumnHeader("price_lb", "Cost LBP");
            SetColumnHeader("sale_price_usd", "Sale Price USD");
            SetColumnHeader("sale_price_lb", "Sale Price LBP");
            SetColumnHeader("stock_quantity", "Stock");
            SetColumnHeader("barcode", "Barcode");
            SetColumnHeader("exchange_rate", "Exchange Rate");
            SetColumnHeader("created_at", "Created At");

            SetColumnVisible("category_id", false);
            SetColumnVisible("supplier_id", false);

            FormatMoneyColumn("price_usd");
            FormatMoneyColumn("price_lb");
            FormatMoneyColumn("sale_price_usd");
            FormatMoneyColumn("sale_price_lb");
            FormatMoneyColumn("exchange_rate");

            if (datagridProducts.Columns.Contains("created_at"))
            {
                datagridProducts.Columns["created_at"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            }
        }

        private void SetColumnHeader(string columnName, string headerText)
        {
            if (datagridProducts.Columns.Contains(columnName))
            {
                datagridProducts.Columns[columnName].HeaderText = headerText;
            }
        }

        private void SetColumnVisible(string columnName, bool visible)
        {
            if (datagridProducts.Columns.Contains(columnName))
            {
                datagridProducts.Columns[columnName].Visible = visible;
            }
        }

        private void FormatMoneyColumn(string columnName)
        {
            if (datagridProducts.Columns.Contains(columnName))
            {
                datagridProducts.Columns[columnName].DefaultCellStyle.Format = "N2";
                datagridProducts.Columns[columnName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void RefreshCategoriesGrid(string keyword = "")
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT DISTINCT c.category_id, c.category_name, c.description
                FROM Categories c
                WHERE @hasKeyword = 0
                   OR EXISTS
                   (
                       SELECT 1
                       FROM Products p
                       WHERE TRY_CONVERT(INT, p.category_id) = c.category_id
                         AND (p.name LIKE @keyword OR p.barcode LIKE @keyword)
                   )
                ORDER BY c.category_id DESC;", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@hasKeyword", !string.IsNullOrWhiteSpace(keyword));
                cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    datagridCategories.DataSource = table;
                }
            }
        }

        private void RefreshSuppliersGrid(string keyword = "")
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT DISTINCT s.supplier_id, s.name, s.contact_info, s.address
                FROM Suppliers s
                WHERE @hasKeyword = 0
                   OR EXISTS
                   (
                       SELECT 1
                       FROM Products p
                       WHERE TRY_CONVERT(INT, p.supplier_id) = s.supplier_id
                         AND (p.name LIKE @keyword OR p.barcode LIKE @keyword)
                   )
                ORDER BY s.supplier_id DESC;", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@hasKeyword", !string.IsNullOrWhiteSpace(keyword));
                cmd.Parameters.AddWithValue("@keyword", $"%{keyword}%");

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    datagridSuppliers.DataSource = table;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (CategoryForm categoryForm = new CategoryForm(true))
            {
                categoryForm.ShowDialog(GetDialogOwner());
            }

            RefreshAllGrids();
        }

        private IWin32Window GetDialogOwner()
        {
            return TopLevelControl as IWin32Window ?? this;
        }

        private void txtsearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtsearch.Text.Trim();
            RefreshProductsGrid(keyword);
            RefreshCategoriesGrid(keyword);
            RefreshSuppliersGrid(keyword);
        }

        private void datagridProducts_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = datagridProducts.Columns[e.ColumnIndex].Name;
            int selectedProductId = Convert.ToInt32(datagridProducts.Rows[e.RowIndex].Cells["product_id"].Value);

            if (columnName == "Edit")
            {
                using (AddProducts editForm = new AddProducts(selectedProductId))
                {
                    editForm.ShowDialog(GetDialogOwner());
                }

                AuditLogger.Log("DELETE", "Products", selectedProductId, "Deleted product");
                RefreshProductsGrid(txtsearch.Text.Trim());
                return;
            }

            if (columnName == "Delete")
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
                RefreshProductsGrid(txtsearch.Text.Trim());
            }
        }

        private void datagridCategories_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = datagridCategories.Columns[e.ColumnIndex].Name;
            int categoryId = Convert.ToInt32(datagridCategories.Rows[e.RowIndex].Cells["category_id1"].Value);

            if (columnName == "Edit1")
            {
                string categoryName = Convert.ToString(datagridCategories.Rows[e.RowIndex].Cells["category_name"].Value)?.Trim() ?? string.Empty;
                string description = Convert.ToString(datagridCategories.Rows[e.RowIndex].Cells["description"].Value)?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    MessageBox.Show("اسم الصنف لا يمكن أن يكون فارغاً.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Categories SET category_name = @name, description = @description WHERE category_id = @id",
                    conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", categoryId);
                    cmd.Parameters.AddWithValue("@name", categoryName);
                    cmd.Parameters.AddWithValue("@description", description);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ تم تعديل الصنف بنجاح");
                AuditLogger.Log("EDIT", "Categories", categoryId, "Updated category: " + categoryName);
                RefreshCategoriesGrid();
                return;
            }

            if (columnName == "Delete1")
            {
                DialogResult confirm = MessageBox.Show("هل تريد حذف الصنف؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Categories WHERE category_id = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", categoryId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ تم حذف الصنف بنجاح");
                AuditLogger.Log("DELETE", "Categories", categoryId, "Deleted category");
                RefreshCategoriesGrid();
            }
        }

        private void datagridSuppliers_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            string columnName = datagridSuppliers.Columns[e.ColumnIndex].Name;
            int supplierId = Convert.ToInt32(datagridSuppliers.Rows[e.RowIndex].Cells["supplier_id1"].Value);

            if (columnName == "Edit2")
            {
                string supplierName = Convert.ToString(datagridSuppliers.Rows[e.RowIndex].Cells["name1"].Value)?.Trim() ?? string.Empty;
                string contactInfo = Convert.ToString(datagridSuppliers.Rows[e.RowIndex].Cells["contact_info"].Value)?.Trim() ?? string.Empty;
                string address = Convert.ToString(datagridSuppliers.Rows[e.RowIndex].Cells["address"].Value)?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(supplierName))
                {
                    MessageBox.Show("اسم المورد لا يمكن أن يكون فارغاً.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Suppliers SET name = @name, contact_info = @contact, address = @address WHERE supplier_id = @id",
                    conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", supplierId);
                    cmd.Parameters.AddWithValue("@name", supplierName);
                    cmd.Parameters.AddWithValue("@contact", contactInfo);
                    cmd.Parameters.AddWithValue("@address", address);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ تم تعديل المورد بنجاح");
                AuditLogger.Log("EDIT", "Suppliers", supplierId, "Updated supplier: " + supplierName);
                RefreshSuppliersGrid();
                return;
            }

            if (columnName == "Delete2")
            {
                DialogResult confirm = MessageBox.Show("هل تريد حذف المورد؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                using (SqlConnection conn = new SqlConnection(connStr))
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Suppliers WHERE supplier_id = @id", conn))
                {
                    conn.Open();
                    cmd.Parameters.AddWithValue("@id", supplierId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ تم حذف المورد بنجاح");
                AuditLogger.Log("DELETE", "Suppliers", supplierId, "Deleted supplier");
                RefreshSuppliersGrid();
            }
        }

        private void datagridCategories_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void datagridSuppliers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void datagridProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void btnback_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
