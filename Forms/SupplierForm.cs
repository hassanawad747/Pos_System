using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class SupplierForm : Form
    {
        private const string EditColumnName = "SupplierEditButton";
        private const string DeleteColumnName = "SupplierDeleteButton";

        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private readonly bool openNextAfterSave;

        public SupplierForm() : this(true)
        {
        }

        public SupplierForm(bool openNextAfterSave)
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);

            this.openNextAfterSave = openNextAfterSave;
            dataGridView1.CellClick += dataGridView1_CellClick;
        }

        private void SupplierForm_Load(object sender, EventArgs e)
        {
            EnsureActionColumns();
            RefreshSuppliers();
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            if (!TryValidateInput(out string supplierName, out string contactInfo, out string address))
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Suppliers (name, contact_info, address) VALUES (@name, @contact, @address)",
                conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@name", supplierName);
                cmd.Parameters.AddWithValue("@contact", contactInfo);
                cmd.Parameters.AddWithValue("@address", address);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("✅ تم إضافة المورد بنجاح");
            RefreshSuppliers();

            if (openNextAfterSave)
            {
                OpenAddProductsFormAndClose();
                return;
            }

            ClearInputs();
        }

        private bool TryValidateInput(out string supplierName, out string contactInfo, out string address)
        {
            supplierName = txtname.Text.Trim();
            contactInfo = txtnumber.Text.Trim();
            address = txtplace.Text.Trim();

            if (string.IsNullOrWhiteSpace(supplierName))
            {
                MessageBox.Show("يرجى إدخال اسم المورد.");
                txtname.Focus();
                return false;
            }

            return true;
        }

        private void RefreshSuppliers(string keyword = "")
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT supplier_id, name, contact_info, address
                  FROM Suppliers
                  WHERE @keyword = ''
                     OR name LIKE @search
                     OR contact_info LIKE @search
                     OR address LIKE @search
                  ORDER BY supplier_id DESC",
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
            txtname.Clear();
            txtnumber.Clear();
            txtplace.Clear();
            txtname.Focus();
        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnskip_Click(object sender, EventArgs e)
        {
            OpenAddProductsFormAndClose();
        }

        private void OpenAddProductsFormAndClose()
        {
            using (AddProducts productForm = new AddProducts(true))
            {
                Hide();
                productForm.ShowDialog(GetDialogOwner());
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
            int supplierId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["supplier_id"].Value);

            if (columnName == EditColumnName)
            {
                string supplierName = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["name"].Value)?.Trim() ?? string.Empty;
                string contactInfo = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["contact_info"].Value)?.Trim() ?? string.Empty;
                string address = Convert.ToString(dataGridView1.Rows[e.RowIndex].Cells["address"].Value)?.Trim() ?? string.Empty;

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
                RefreshSuppliers();
                return;
            }

            if (columnName == DeleteColumnName)
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
                RefreshSuppliers();
            }
        }

        private void txtname_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtnumber_TextChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
