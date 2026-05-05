using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public partial class Customers : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private TextBox txtBalanceAmount;
        private ComboBox cmbBalanceCurrency;
        private Button btnBalancePlus;
        private Button btnBalanceMinus;

        public Customers()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            CreateBalanceAdjustmentControls();
            ConfigureCustomerGrid();
        }

        private void Customers_Load(object sender, EventArgs e)
        {
            AuditLogger.EnsureCustomerBalanceColumns();
            // TODO: This line of code loads data into the 'pos_systemDataSet15.Customers' table. You can move, or remove it, as needed.
            this.customersTableAdapter1.Fill(this.pos_systemDataSet15.Customers);
            LoadCustomers("");

            //// إضافة زر Edit
            //DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
            //btnEdit.HeaderText = "Edit";
            //btnEdit.Text = "Edit";
            //btnEdit.UseColumnTextForButtonValue = true;
            //dataGridView1.Columns.Add(btnEdit);

            //// إضافة زر Delete
            //DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
            //btnDelete.HeaderText = "Delete";
            //btnDelete.Text = "Delete";
            //btnDelete.UseColumnTextForButtonValue = true;
            //dataGridView1.Columns.Add(btnDelete);
        }

        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            AddCustomers addForm = new AddCustomers();
            addForm.ShowDialog();
            LoadCustomers(""); // إعادة تحميل العملاء بعد الإضافة
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void LoadCustomers(string keyword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    SELECT customer_id,
                           name,
                           phone,
                           email,
                           loyalty_points,
                           created_at,
                           balance,
                           CASE
                               WHEN ISNULL(balance_usd, 0) = 0 THEN N'$ 0.00'
                               ELSE CONCAT(CASE WHEN balance_usd < 0 THEN N'-' ELSE N'' END, N'$ ', FORMAT(ABS(balance_usd), 'N2'))
                           END AS balance_usd,
                           CASE
                               WHEN ISNULL(balance_lb, 0) = 0 THEN N'0 L.L'
                               ELSE CONCAT(CASE WHEN balance_lb < 0 THEN N'-' ELSE N'' END, FORMAT(ABS(balance_lb), 'N0'), N' L.L')
                           END AS balance_lb,
                           CASE
                               WHEN ISNULL(balance_usd, 0) > 0 THEN CONCAT(N'Customer owes you: $', FORMAT(balance_usd, 'N2'))
                               WHEN ISNULL(balance_usd, 0) < 0 THEN CONCAT(N'You owe customer: $', FORMAT(ABS(balance_usd), 'N2'))
                               WHEN ISNULL(balance_lb, 0) > 0 THEN CONCAT(N'Customer owes you: ', FORMAT(balance_lb, 'N0'), N' L.L')
                               WHEN ISNULL(balance_lb, 0) < 0 THEN CONCAT(N'You owe customer: ', FORMAT(ABS(balance_lb), 'N0'), N' L.L')
                               ELSE N'No balance'
                           END AS balance_status,
                           balance_updated_at,
                           created_by
                    FROM Customers
                    WHERE name LIKE @keyword OR phone LIKE @keyword OR email LIKE @keyword";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }

        private void CreateBalanceAdjustmentControls()
        {
            panel3.BackColor = Color.WhiteSmoke;

            Label lblBalanceAmount = new Label
            {
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold),
                Location = new Point(18, 22),
                Text = "Balance amount"
            };

            txtBalanceAmount = new TextBox
            {
                Font = new Font("Microsoft Sans Serif", 12F),
                Location = new Point(160, 15),
                Name = "txtBalanceAmount",
                Size = new Size(170, 34),
                TabIndex = 20
            };

            cmbBalanceCurrency = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold),
                Location = new Point(344, 15),
                Name = "cmbBalanceCurrency",
                Size = new Size(100, 34),
                TabIndex = 21
            };
            cmbBalanceCurrency.Items.AddRange(new object[] { "USD", "L.L" });
            cmbBalanceCurrency.SelectedIndex = 0;

            btnBalancePlus = new Button
            {
                BackColor = Color.SeaGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(462, 10),
                Name = "btnBalancePlus",
                Size = new Size(130, 44),
                TabIndex = 22,
                Text = "+ Add",
                UseVisualStyleBackColor = false
            };
            btnBalancePlus.Click += btnBalancePlus_Click;

            btnBalanceMinus = new Button
            {
                BackColor = Color.Firebrick,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(604, 10),
                Name = "btnBalanceMinus",
                Size = new Size(130, 44),
                TabIndex = 23,
                Text = "- Less",
                UseVisualStyleBackColor = false
            };
            btnBalanceMinus.Click += btnBalanceMinus_Click;

            Label lblHint = new Label
            {
                AutoSize = true,
                Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular),
                ForeColor = Color.DimGray,
                Location = new Point(752, 24),
                Text = "Select customer row first. Plus adds debt, minus reduces paid balance."
            };

            panel3.Controls.Add(lblBalanceAmount);
            panel3.Controls.Add(txtBalanceAmount);
            panel3.Controls.Add(cmbBalanceCurrency);
            panel3.Controls.Add(btnBalancePlus);
            panel3.Controls.Add(btnBalanceMinus);
            panel3.Controls.Add(lblHint);
        }

        private void ConfigureCustomerGrid()
        {
            dataGridView1.AutoGenerateColumns = false;

            if (!dataGridView1.Columns.Contains("balance_usd"))
            {
                dataGridView1.Columns.Insert(5, new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "balance_usd",
                    HeaderText = "Balance $",
                    MinimumWidth = 6,
                    Name = "balance_usd",
                    ReadOnly = true
                });
            }

            if (!dataGridView1.Columns.Contains("balance_lb"))
            {
                dataGridView1.Columns.Insert(6, new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "balance_lb",
                    HeaderText = "Balance L.L",
                    MinimumWidth = 6,
                    Name = "balance_lb",
                    ReadOnly = true
                });
            }

            if (!dataGridView1.Columns.Contains("balance_status"))
            {
                dataGridView1.Columns.Insert(7, new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "balance_status",
                    HeaderText = "Balance Status",
                    MinimumWidth = 6,
                    Name = "balance_status",
                    ReadOnly = true
                });
            }
        }

        private void btnBalancePlus_Click(object sender, EventArgs e)
        {
            AdjustSelectedCustomerBalance(1m);
        }

        private void btnBalanceMinus_Click(object sender, EventArgs e)
        {
            AdjustSelectedCustomerBalance(-1m);
        }

        private void AdjustSelectedCustomerBalance(decimal direction)
        {
            if (dataGridView1.CurrentRow == null || dataGridView1.CurrentRow.IsNewRow)
            {
                MessageBox.Show("اختر العميل من الجدول اولا");
                return;
            }

            if (!TryParseDecimal(txtBalanceAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("اكتب رقم صحيح في خانة الرصيد");
                txtBalanceAmount.Focus();
                return;
            }

            int customerId = Convert.ToInt32(dataGridView1.CurrentRow.Cells["customer_id"].Value);
            string customerName = Convert.ToString(dataGridView1.CurrentRow.Cells["name"].Value);
            string currency = cmbBalanceCurrency.SelectedItem != null ? cmbBalanceCurrency.SelectedItem.ToString() : "USD";
            decimal delta = amount * direction;
            string selectedColumn = currency == "USD" ? "balance_usd" : "balance_lb";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = @"
                    UPDATE Customers
                    SET balance_usd = CASE WHEN @currency = 'USD' THEN ISNULL(balance_usd, 0) + @delta ELSE ISNULL(balance_usd, 0) END,
                        balance_lb = CASE WHEN @currency = 'LBP' THEN ISNULL(balance_lb, 0) + @delta ELSE ISNULL(balance_lb, 0) END,
                        balance = CASE
                            WHEN @currency = 'USD' THEN ISNULL(balance_usd, 0) + @delta
                            ELSE ISNULL(balance_lb, 0) + @delta
                        END,
                        balance_updated_at = GETDATE()
                    WHERE customer_id = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", customerId);
                    cmd.Parameters.AddWithValue("@currency", currency == "USD" ? "USD" : "LBP");
                    SqlParameter deltaParameter = cmd.Parameters.Add("@delta", SqlDbType.Decimal);
                    deltaParameter.Precision = 24;
                    deltaParameter.Scale = 8;
                    deltaParameter.Value = delta;
                    cmd.ExecuteNonQuery();
                }
            }

            string actionWord = direction > 0 ? "Added" : "Reduced";
            string amountText = currency == "USD" ? "$ " + amount.ToString("N2") : amount.ToString("N0") + " L.L";
            AuditLogger.Log("EDIT", "Customers", customerId, actionWord + " customer balance " + amountText + " for " + customerName);

            txtBalanceAmount.Clear();
            LoadCustomers(txtsearch.Text);

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["customer_id"].Value != null && Convert.ToInt32(row.Cells["customer_id"].Value) == customerId)
                {
                    row.Selected = true;
                    dataGridView1.CurrentCell = row.Cells[selectedColumn];
                    break;
                }
            }

            MessageBox.Show("تم تحديث رصيد العميل بنجاح");
        }

        private void datagridCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddCustomers addForm = new AddCustomers();
            addForm.ShowDialog();
            this.Hide();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                // Edit
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Edit")
                {
                    int customerId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["customer_id"].Value);
                    string newName = dataGridView1.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    string newPhone = dataGridView1.Rows[e.RowIndex].Cells["phone"].Value.ToString();
                    string newEmail = dataGridView1.Rows[e.RowIndex].Cells["email"].Value.ToString();
                    string createdby = dataGridView1.Rows[e.RowIndex].Cells["created_by"].Value.ToString();
                    string balance = dataGridView1.Rows[e.RowIndex].Cells["balance"].Value.ToString();


                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE Customers SET name=@name, phone=@phone, email=@email, created_by=@created_by, balance=@balance WHERE customer_id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", customerId);
                        cmd.Parameters.AddWithValue("@name", newName);
                        cmd.Parameters.AddWithValue("@phone", newPhone);
                        cmd.Parameters.AddWithValue("@email", newEmail);
                        cmd.Parameters.AddWithValue("@created_by", createdby);
                        cmd.Parameters.AddWithValue("@balance", TryParseDecimal(balance, out decimal parsedBalance) ? parsedBalance : 0m);
                        cmd.ExecuteNonQuery();
                    }

                    AuditLogger.Log("EDIT", "Customers", customerId, "Updated customer: " + newName);
                    MessageBox.Show("✅ تم تعديل العميل بنجاح");
                    LoadCustomers(txtsearch.Text);
                }

                // Delete
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
                {
                    int customerId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["customer_id"].Value);

                    var confirm = MessageBox.Show("هل تريد حذف العميل؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                    if (confirm == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM Customers WHERE customer_id=@id", conn);
                            cmd.Parameters.AddWithValue("@id", customerId);
                            cmd.ExecuteNonQuery();
                        }

                        AuditLogger.Log("DELETE", "Customers", customerId, "Deleted customer");
                        MessageBox.Show("✅ تم حذف العميل بنجاح");
                        LoadCustomers(txtsearch.Text);
                    }
                }
            }
        }

        private void txtsearch_TextChanged_1(object sender, EventArgs e)
        {
            LoadCustomers(txtsearch.Text); // البحث التلقائي عند الكتابة
        }

        private static bool TryParseDecimal(string value, out decimal parsedValue)
        {
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out parsedValue)
                || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedValue);
        }
    }
}
