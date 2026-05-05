using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public partial class CategoryForm : Form
    {
        private const string EditColumnName = "CategoryEditButton";
        private const string DeleteColumnName = "CategoryDeleteButton";

        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private readonly bool openNextAfterSave;

        public CategoryForm() : this(true)
        {
        }

        public CategoryForm(bool openNextAfterSave)
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);

            this.openNextAfterSave = openNextAfterSave;
            dataGridView1.CellClick += dataGridView1_CellClick;
        }

        private void CategoryForm_Load(object sender, EventArgs e)
        {
            EnsureActionColumns();
            RefreshCategories();
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            if (!TryValidateInput(out string categoryName, out string description))
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Categories (category_name, description) VALUES (@name, @description)",
                conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@name", categoryName);
                cmd.Parameters.AddWithValue("@description", description);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("✅ تم إضافة الصنف بنجاح");
            AuditLogger.Log("ADD", "Categories", null, "Added category: " + categoryName);
            RefreshCategories();

            if (openNextAfterSave)
            {
                OpenSupplierFormAndClose();
                return;
            }

            ClearInputs();
        }

        private bool TryValidateInput(out string categoryName, out string description)
        {
            categoryName = txttypeitem.Text.Trim();
            description = txtdescreption.Text.Trim();

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                MessageBox.Show("يرجى إدخال اسم الصنف.");
                txttypeitem.Focus();
                return false;
            }

            return true;
        }

        private void RefreshCategories(string keyword = "")
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT category_id, category_name, description
                  FROM Categories
                  WHERE @keyword = ''
                     OR category_name LIKE @search
                     OR description LIKE @search
                  ORDER BY category_id DESC",
                conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@keyword", keyword);
                cmd.Parameters.AddWithValue("@search", $"%{keyword}%");

                using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    dataGridView1.DataSource = table;
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
            txttypeitem.Clear();
            txtdescreption.Clear();
            txttypeitem.Focus();
        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnskip_Click(object sender, EventArgs e)
        {
            OpenSupplierFormAndClose();
        }

        private void OpenSupplierFormAndClose()
        {
            using (SupplierForm supplierForm = new SupplierForm(true))
            {
                Hide();
                supplierForm.ShowDialog(GetDialogOwner());
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
            int categoryId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["category_id"].Value);

            if (columnName == EditColumnName)
            {
                string categoryName = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["category_name"].Value)?.Trim() ?? string.Empty;
                string description = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["description"].Value)?.Trim() ?? string.Empty;

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
                RefreshCategories();
                return;
            }

            if (columnName == DeleteColumnName)
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
                RefreshCategories();
            }
        }

        private void txttypeitem_TextChanged(object sender, EventArgs e)
        {
        }

        private void btnback_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
