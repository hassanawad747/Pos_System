using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public partial class SupplierForm : Form
    {
        private const string EditColumnName = "SupplierEditButton";
        private const string DeleteColumnName = "SupplierDeleteButton";

        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private readonly bool openNextAfterSave;
        private TextBox txtSupplierBalance;
        private ComboBox cmbSupplierBalanceCurrency;
        private Button btnSupplierBalancePlus;
        private Button btnSupplierBalanceMinus;

        public SupplierForm() : this(true)
        {
        }

        public SupplierForm(bool openNextAfterSave)
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);

            this.openNextAfterSave = openNextAfterSave;
            ConfigureSupplierBalanceControls();
            dataGridView1.CellClick += dataGridView1_CellClick;
        }

        private void SupplierForm_Load(object sender, EventArgs e)
        {
            if (!PermissionService.EnsureScreenAccess(this, PermissionService.ScreenSuppliers))
            {
                return;
            }

            AuditLogger.EnsureSupplierBalanceColumns();
            EnsureBalanceColumns();
            EnsureActionColumns();
            RefreshSuppliers();
            PermissionService.ApplyActionPermissions(this, PermissionService.ScreenSuppliers);
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanCreate(AppSession.UserId, AppSession.Role, PermissionService.ScreenSuppliers))
            {
                MessageBox.Show("You do not have permission to create suppliers.");
                return;
            }

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

            AuditService.Log("Suppliers", "Create", supplierName, "Created supplier " + supplierName);
            MessageBox.Show("✅ تم إضافة المورد بنجاح");
            AuditLogger.Log("ADD", "Suppliers", null, "Added supplier: " + supplierName);
            RefreshSuppliers();

            if (openNextAfterSave)
            {
                OpenAddProductsFormAndClose();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
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
                @"SELECT supplier_id,
                         name,
                         contact_info,
                         address,
                         balance_usd,
                         balance_lb,
                         CASE
                             WHEN ISNULL(balance_usd, 0) > 0 OR ISNULL(balance_lb, 0) > 0 THEN N'You owe supplier'
                             WHEN ISNULL(balance_usd, 0) < 0 OR ISNULL(balance_lb, 0) < 0 THEN N'Supplier owes you'
                             ELSE N'No balance'
                         END AS balance_status,
                         balance_updated_at
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
                    FormatSupplierGrid();
                }
            }
        }

        private void ConfigureSupplierBalanceControls()
        {
            panel3.AutoScroll = true;
            panel3.Height = Math.Max(panel3.Height, 320);

            Label lblBalance = new Label
            {
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Bold),
                Location = new Point(15, 238),
                Text = "Balance"
            };

            txtSupplierBalance = new TextBox
            {
                Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold),
                Location = new Point(15, 260),
                Name = "txtSupplierBalance",
                Size = new Size(112, 27),
                TextAlign = HorizontalAlignment.Center
            };
            txtSupplierBalance.KeyPress += NumericTextBox_KeyPress;

            cmbSupplierBalanceCurrency = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Bold),
                Location = new Point(133, 260),
                Name = "cmbSupplierBalanceCurrency",
                Size = new Size(121, 26)
            };
            cmbSupplierBalanceCurrency.Items.AddRange(new object[] { "USD", "L.L" });
            cmbSupplierBalanceCurrency.SelectedIndex = 0;

            btnSupplierBalancePlus = new Button
            {
                BackColor = Color.FromArgb(22, 163, 74),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Location = new Point(15, 294),
                Name = "btnSupplierBalancePlus",
                Size = new Size(112, 32),
                Text = "Add Balance"
            };
            btnSupplierBalancePlus.FlatAppearance.BorderSize = 0;
            btnSupplierBalancePlus.Click += (sender, args) => AdjustSelectedSupplierBalance(1m);

            btnSupplierBalanceMinus = new Button
            {
                BackColor = Color.FromArgb(220, 38, 38),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Location = new Point(133, 294),
                Name = "btnSupplierBalanceMinus",
                Size = new Size(121, 32),
                Text = "Less Balance"
            };
            btnSupplierBalanceMinus.FlatAppearance.BorderSize = 0;
            btnSupplierBalanceMinus.Click += (sender, args) => AdjustSelectedSupplierBalance(-1m);

            panel3.Controls.Add(lblBalance);
            panel3.Controls.Add(txtSupplierBalance);
            panel3.Controls.Add(cmbSupplierBalanceCurrency);
            panel3.Controls.Add(btnSupplierBalancePlus);
            panel3.Controls.Add(btnSupplierBalanceMinus);
        }

        private void EnsureBalanceColumns()
        {
            AddTextColumnIfMissing("balance_usd", "balance_usd", "Balance $", true);
            AddTextColumnIfMissing("balance_lb", "balance_lb", "Balance L.L", true);
            AddTextColumnIfMissing("balance_status", "balance_status", "Balance Status", true);
            AddTextColumnIfMissing("balance_updated_at", "balance_updated_at", "Balance DateTime", true);
        }

        private void AddTextColumnIfMissing(string name, string dataPropertyName, string headerText, bool readOnly)
        {
            if (dataGridView1.Columns.Contains(name))
            {
                return;
            }

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = dataPropertyName,
                HeaderText = headerText,
                ReadOnly = readOnly
            });
        }

        private void FormatSupplierGrid()
        {
            if (dataGridView1.Columns.Contains("balance_usd"))
            {
                dataGridView1.Columns["balance_usd"].DefaultCellStyle.Format = "N2";
            }

            if (dataGridView1.Columns.Contains("balance_lb"))
            {
                dataGridView1.Columns["balance_lb"].DefaultCellStyle.Format = "N0";
            }

            if (dataGridView1.Columns.Contains("balance_updated_at"))
            {
                dataGridView1.Columns["balance_updated_at"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            }
        }

        private void AdjustSelectedSupplierBalance(decimal direction)
        {
            if (!PermissionService.CanSave(AppSession.UserId, AppSession.Role, PermissionService.ScreenSuppliers))
            {
                MessageBox.Show("You do not have permission to change supplier balance.");
                return;
            }

            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Select a supplier row first.");
                return;
            }

            if (!TryParseDecimal(txtSupplierBalance.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Enter a valid balance amount.");
                txtSupplierBalance.Focus();
                return;
            }

            int supplierId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["supplier_id"].Value);
            string supplierName = Convert.ToString(dataGridView1.CurrentRow.Cells["name"].Value);
            string currency = cmbSupplierBalanceCurrency.SelectedItem != null
                ? cmbSupplierBalanceCurrency.SelectedItem.ToString()
                : "USD";
            decimal delta = amount * direction;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Suppliers
                SET balance_usd = CASE WHEN @currency = 'USD' THEN ISNULL(balance_usd, 0) + @delta ELSE ISNULL(balance_usd, 0) END,
                    balance_lb = CASE WHEN @currency = 'LBP' THEN ISNULL(balance_lb, 0) + @delta ELSE ISNULL(balance_lb, 0) END,
                    balance_updated_at = GETDATE()
                WHERE supplier_id = @id;", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@id", supplierId);
                cmd.Parameters.AddWithValue("@currency", currency == "USD" ? "USD" : "LBP");
                cmd.Parameters.AddWithValue("@delta", delta);
                cmd.ExecuteNonQuery();
            }

            string amountText = currency == "USD"
                ? "$" + amount.ToString("N2", CultureInfo.InvariantCulture)
                : amount.ToString("N0", CultureInfo.InvariantCulture) + " L.L";
            string actionWord = direction > 0 ? "Added" : "Less";

            AuditLogger.Log("EDIT", "Suppliers", supplierId, actionWord + " supplier balance " + amountText + " for " + supplierName);
            txtSupplierBalance.Clear();
            RefreshSuppliers();
        }

        private static bool TryParseDecimal(string value, out decimal result)
        {
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result) ||
                   decimal.TryParse(value, out result);
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            TextBox textBox = sender as TextBox;
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == '.' && textBox != null && textBox.Text.Contains("."))
            {
                e.Handled = true;
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
                if (!PermissionService.CanEdit(AppSession.UserId, AppSession.Role, PermissionService.ScreenSuppliers))
                {
                    MessageBox.Show("You do not have permission to edit suppliers.");
                    return;
                }

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

                AuditService.Log("Suppliers", "Edit", supplierId.ToString(), "Updated supplier " + supplierName);
                MessageBox.Show("✅ تم تعديل المورد بنجاح");
                AuditLogger.Log("EDIT", "Suppliers", supplierId, "Updated supplier: " + supplierName);
                RefreshSuppliers();
                return;
            }

            if (columnName == DeleteColumnName)
            {
                if (!PermissionService.CanDelete(AppSession.UserId, AppSession.Role, PermissionService.ScreenSuppliers))
                {
                    MessageBox.Show("You do not have permission to delete suppliers.");
                    return;
                }
                DialogResult confirm = MessageBox.Show("هل تريد حذف المورد؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                try
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM Suppliers WHERE supplier_id = @id", conn))
                    {
                        conn.Open();
                        cmd.Parameters.AddWithValue("@id", supplierId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException)
                {
                    MessageBox.Show("Cannot delete this supplier because products or sales are using it. Keep it in the system or edit its details instead.", "Delete Supplier");
                    return;
                }

                AuditService.Log("Suppliers", "Delete", supplierId.ToString(), "Deleted supplier ID " + supplierId);
                MessageBox.Show("✅ تم حذف المورد بنجاح");
                AuditLogger.Log("DELETE", "Suppliers", supplierId, "Deleted supplier");
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
