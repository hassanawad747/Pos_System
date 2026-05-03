using System;
using System.Data;
using System.Data.SqlClient;
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
        }

        private void Products_Load(object sender, EventArgs e)
        {
            RefreshAllGrids();
        }

        private void RefreshAllGrids()
        {
            RefreshProductsGrid(txtsearch.Text.Trim());
            RefreshCategoriesGrid();
            RefreshSuppliersGrid();
        }

        private void RefreshProductsGrid(string keyword = "")
        {
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
                }
            }
        }

        private static string BuildProductsSearchQuery(string keyword)
        {
            string query = @"
                SELECT product_id, name, price_usd, price_lb, sale_price_usd, sale_price_lb,
                       category_id, stock_quantity, barcode, exchange_rate, supplier_id, created_at
                FROM Products
                WHERE @hasKeyword = 0
                   OR name LIKE @keyword
                   OR barcode LIKE @keyword";

            if (int.TryParse(keyword, out _))
            {
                query += @"
                   OR DAY(created_at) = @number
                   OR MONTH(created_at) = @number
                   OR YEAR(created_at) = @number";
            }

            if (DateTime.TryParse(keyword, out _))
            {
                query += @"
                   OR CAST(created_at AS DATE) = @searchDate";
            }

            query += " ORDER BY product_id DESC";
            return query;
        }

        private void RefreshCategoriesGrid()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT category_id, category_name, description FROM Categories ORDER BY category_id DESC",
                conn))
            {
                conn.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    datagridCategories.DataSource = table;
                }
            }
        }

        private void RefreshSuppliersGrid()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT supplier_id, name, contact_info, address FROM Suppliers ORDER BY supplier_id DESC",
                conn))
            {
                conn.Open();

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
            RefreshProductsGrid(txtsearch.Text.Trim());
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

                AuditService.Log("Products", "Delete", selectedProductId.ToString(), "Deleted product ID " + selectedProductId);
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

                AuditService.Log("Categories", "Edit", categoryId.ToString(), "Updated category " + categoryName);
                MessageBox.Show("✅ تم تعديل الصنف بنجاح");
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

                AuditService.Log("Categories", "Delete", categoryId.ToString(), "Deleted category ID " + categoryId);
                MessageBox.Show("✅ تم حذف الصنف بنجاح");
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

                AuditService.Log("Suppliers", "Edit", supplierId.ToString(), "Updated supplier " + supplierName);
                MessageBox.Show("✅ تم تعديل المورد بنجاح");
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

                AuditService.Log("Suppliers", "Delete", supplierId.ToString(), "Deleted supplier ID " + supplierId);
                MessageBox.Show("✅ تم حذف المورد بنجاح");
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
