using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public class DiscountSettingsForm : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private ComboBox cmbTargetType;
        private ComboBox cmbProduct;
        private ComboBox cmbCategory;
        private ComboBox cmbDiscountType;
        private ComboBox cmbAllowedUser;
        private TextBox txtRuleName;
        private TextBox txtBarcode;
        private TextBox txtDiscountValue;
        private CheckBox chkActive;
        private DataGridView gridRules;

        public DiscountSettingsForm()
        {
            Text = "Discount Settings";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 620);
            MinimumSize = new Size(820, 520);
            BackColor = Color.FromArgb(244, 247, 252);

            BuildUi();
            Load += (s, e) =>
            {
                if (!PermissionService.EnsureScreenAccess(this, PermissionService.ScreenDiscountSettings))
                    return;

                EnsureDiscountRulesTable();
                LoadProducts();
                LoadCategories();
                LoadUsers();
                LoadRules();
                UpdateTargetInputs();
            };
        }

        private void BuildUi()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(18, 12, 18, 12)
            };
            Label title = new Label
            {
                Text = "Discount Settings",
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            header.Controls.Add(title);

            Panel editor = new Panel
            {
                Dock = DockStyle.Top,
                Height = 230,
                BackColor = Color.White,
                Padding = new Padding(16)
            };

            txtRuleName = CreateTextBox();
            cmbTargetType = CreateComboBox();
            cmbTargetType.Items.AddRange(new object[] { "Product", "Category" });
            cmbTargetType.SelectedIndex = 0;
            cmbTargetType.SelectedIndexChanged += (s, e) => UpdateTargetInputs();

            cmbProduct = CreateComboBox();
            cmbCategory = CreateComboBox();
            txtBarcode = CreateTextBox();
            cmbDiscountType = CreateComboBox();
            cmbDiscountType.Items.AddRange(new object[] { "Percent", "Fixed" });
            cmbDiscountType.SelectedIndex = 0;
            cmbAllowedUser = CreateComboBox();
            txtDiscountValue = CreateTextBox();
            chkActive = new CheckBox
            {
                Text = "Active",
                Checked = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true
            };

            Button btnSave = CreateButton("Save Rule", Color.FromArgb(22, 163, 74));
            btnSave.Click += (s, e) => SaveRule();
            Button btnDelete = CreateButton("Delete Selected", Color.FromArgb(220, 38, 38));
            btnDelete.Click += (s, e) => DeleteSelectedRule();
            Button btnClear = CreateButton("Clear", Color.FromArgb(100, 116, 139));
            btnClear.Click += (s, e) => ClearEditor();

            AddField(editor, "Rule Name", txtRuleName, 16, 18, 190);
            AddField(editor, "Target", cmbTargetType, 260, 18, 150);
            AddField(editor, "Product", cmbProduct, 450, 18, 240);
            AddField(editor, "Category", cmbCategory, 450, 18, 240);
            AddField(editor, "Barcode", txtBarcode, 720, 18, 190);

            AddField(editor, "Discount Type", cmbDiscountType, 16, 95, 190);
            AddField(editor, "Value", txtDiscountValue, 260, 95, 150);
            AddField(editor, "Allowed User", cmbAllowedUser, 450, 95, 240);
            chkActive.Location = new Point(720, 122);
            btnSave.SetBounds(16, 172, 120, 38);
            btnDelete.SetBounds(150, 172, 130, 38);
            btnClear.SetBounds(294, 172, 90, 38);
            editor.Controls.Add(chkActive);
            editor.Controls.Add(btnSave);
            editor.Controls.Add(btnDelete);
            editor.Controls.Add(btnClear);

            gridRules = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
            gridRules.CellDoubleClick += (s, e) => LoadSelectedRuleIntoEditor();

            Controls.Add(gridRules);
            Controls.Add(editor);
            Controls.Add(header);
        }

        private TextBox CreateTextBox()
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private ComboBox CreateComboBox()
        {
            return new ComboBox
            {
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
        }

        private Button CreateButton(string text, Color color)
        {
            Button button = new Button
            {
                Text = text,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private void AddField(Control parent, string labelText, Control input, int x, int y, int width)
        {
            Label label = new Label
            {
                Text = labelText,
                Location = new Point(x, y),
                Size = new Size(width, 22),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            input.SetBounds(x, y + 26, width, 28);
            parent.Controls.Add(label);
            parent.Controls.Add(input);
        }

        private void EnsureDiscountRulesTable()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
IF OBJECT_ID('dbo.Discount_Rules', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Discount_Rules
    (
        discount_rule_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        rule_name NVARCHAR(120) NOT NULL,
        target_type NVARCHAR(20) NOT NULL,
        product_id INT NULL,
        category_id INT NULL,
        barcode NVARCHAR(100) NULL,
        discount_type NVARCHAR(20) NOT NULL,
        discount_value DECIMAL(18,4) NOT NULL,
        allowed_user_id INT NULL,
        active BIT NOT NULL CONSTRAINT DF_Discount_Rules_Active DEFAULT(1),
        created_at DATETIME NOT NULL CONSTRAINT DF_Discount_Rules_CreatedAt DEFAULT(GETDATE())
    );
END
IF COL_LENGTH('dbo.Discount_Rules', 'allowed_user_id') IS NULL
    ALTER TABLE dbo.Discount_Rules ADD allowed_user_id INT NULL;", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void LoadProducts()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT product_id, name + ISNULL(' / ' + NULLIF(barcode, ''), '') AS display_name FROM Products ORDER BY name", conn))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                cmbProduct.DataSource = table;
                cmbProduct.DisplayMember = "display_name";
                cmbProduct.ValueMember = "product_id";
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter adapter = new SqlDataAdapter("SELECT category_id, category_name FROM Categories ORDER BY category_name", conn))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                cmbCategory.DataSource = table;
                cmbCategory.DisplayMember = "category_name";
                cmbCategory.ValueMember = "category_id";
            }
        }

        private void LoadUsers()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter adapter = new SqlDataAdapter(@"
SELECT CAST(NULL AS INT) AS user_id, N'All Users' AS username
UNION ALL
SELECT user_id, username FROM Users
ORDER BY username", conn))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                cmbAllowedUser.DataSource = table;
                cmbAllowedUser.DisplayMember = "username";
                cmbAllowedUser.ValueMember = "user_id";
            }
        }

        private void LoadRules()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlDataAdapter adapter = new SqlDataAdapter(@"
SELECT r.discount_rule_id,
       r.rule_name,
       r.target_type,
       p.name AS product_name,
       c.category_name,
       r.barcode,
       ISNULL(u.username, N'All Users') AS allowed_user,
       r.discount_type,
       r.discount_value,
       r.active
FROM Discount_Rules r
LEFT JOIN Products p ON p.product_id = r.product_id
LEFT JOIN Categories c ON c.category_id = r.category_id
LEFT JOIN Users u ON u.user_id = r.allowed_user_id
ORDER BY r.discount_rule_id DESC", conn))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                gridRules.DataSource = table;
                if (gridRules.Columns.Contains("discount_rule_id"))
                    gridRules.Columns["discount_rule_id"].Visible = false;
            }
        }

        private void UpdateTargetInputs()
        {
            bool productTarget = cmbTargetType.SelectedItem?.ToString() == "Product";
            cmbProduct.Visible = productTarget;
            txtBarcode.Visible = productTarget;
            cmbCategory.Visible = !productTarget;
        }

        private void SaveRule()
        {
            if (string.IsNullOrWhiteSpace(txtRuleName.Text))
            {
                MessageBox.Show("Enter rule name.");
                return;
            }

            if (!decimal.TryParse(txtDiscountValue.Text.Trim(), out decimal value) || value <= 0)
            {
                MessageBox.Show("Enter a valid discount value.");
                return;
            }

            string targetType = cmbTargetType.SelectedItem?.ToString() ?? "Product";
            object productId = targetType == "Product" && cmbProduct.SelectedValue != null ? cmbProduct.SelectedValue : (object)DBNull.Value;
            object categoryId = targetType == "Category" && cmbCategory.SelectedValue != null ? cmbCategory.SelectedValue : (object)DBNull.Value;
            object allowedUserId = cmbAllowedUser.SelectedValue == null || cmbAllowedUser.SelectedValue == DBNull.Value
                ? (object)DBNull.Value
                : cmbAllowedUser.SelectedValue;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
INSERT INTO Discount_Rules
    (rule_name, target_type, product_id, category_id, barcode, allowed_user_id, discount_type, discount_value, active)
VALUES
    (@rule_name, @target_type, @product_id, @category_id, @barcode, @allowed_user_id, @discount_type, @discount_value, @active)", conn))
            {
                cmd.Parameters.Add("@rule_name", SqlDbType.NVarChar, 120).Value = txtRuleName.Text.Trim();
                cmd.Parameters.Add("@target_type", SqlDbType.NVarChar, 20).Value = targetType;
                cmd.Parameters.Add("@product_id", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@category_id", SqlDbType.Int).Value = categoryId;
                cmd.Parameters.Add("@barcode", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(txtBarcode.Text) ? (object)DBNull.Value : txtBarcode.Text.Trim();
                cmd.Parameters.Add("@allowed_user_id", SqlDbType.Int).Value = allowedUserId;
                cmd.Parameters.Add("@discount_type", SqlDbType.NVarChar, 20).Value = cmbDiscountType.SelectedItem?.ToString() ?? "Percent";
                cmd.Parameters.Add("@discount_value", SqlDbType.Decimal).Value = value;
                cmd.Parameters.Add("@active", SqlDbType.Bit).Value = chkActive.Checked;
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            ClearEditor();
            LoadRules();
        }

        private void DeleteSelectedRule()
        {
            if (gridRules.CurrentRow == null || gridRules.CurrentRow.Cells["discount_rule_id"].Value == null)
                return;

            int id = Convert.ToInt32(gridRules.CurrentRow.Cells["discount_rule_id"].Value);
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("DELETE FROM Discount_Rules WHERE discount_rule_id = @id", conn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            LoadRules();
        }

        private void LoadSelectedRuleIntoEditor()
        {
            if (gridRules.CurrentRow == null)
                return;

            txtRuleName.Text = gridRules.CurrentRow.Cells["rule_name"].Value?.ToString();
            cmbTargetType.SelectedItem = gridRules.CurrentRow.Cells["target_type"].Value?.ToString();
            cmbDiscountType.SelectedItem = gridRules.CurrentRow.Cells["discount_type"].Value?.ToString();
            txtDiscountValue.Text = gridRules.CurrentRow.Cells["discount_value"].Value?.ToString();
            txtBarcode.Text = gridRules.CurrentRow.Cells["barcode"].Value?.ToString();
            if (gridRules.CurrentRow.Cells["allowed_user"].Value != null)
                cmbAllowedUser.Text = gridRules.CurrentRow.Cells["allowed_user"].Value.ToString();
            chkActive.Checked = gridRules.CurrentRow.Cells["active"].Value != DBNull.Value && Convert.ToBoolean(gridRules.CurrentRow.Cells["active"].Value);
            UpdateTargetInputs();
        }

        private void ClearEditor()
        {
            txtRuleName.Clear();
            txtBarcode.Clear();
            txtDiscountValue.Clear();
            chkActive.Checked = true;
            cmbTargetType.SelectedIndex = 0;
            cmbDiscountType.SelectedIndex = 0;
            if (cmbAllowedUser.Items.Count > 0)
                cmbAllowedUser.SelectedIndex = 0;
        }
    }
}
