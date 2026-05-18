using ClosedXML.Excel;
using Pos_System.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class SupplierBalanceForm : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private ComboBox comboSupplier;
        private TextBox txtPhone;
        private TextBox txtEmail;
        private ComboBox comboBalanceType;
        private ComboBox comboCurrency;
        private TextBox txtBalanceAmount;
        private DateTimePicker dateFrom;
        private DateTimePicker dateTo;
        private DataGridView gridSuppliers;
        private DataGridView gridItems;
        private DataTable currentItemsTable;
        private PrintDocument printDocument;

        public SupplierBalanceForm()
        {
            InitializeSupplierBalanceForm();
        }

        private int SelectedSupplierId
        {
            get
            {
                return comboSupplier.SelectedValue is int value ? value : 0;
            }
        }

        private void InitializeSupplierBalanceForm()
        {
            Text = "Supplier";
            BackColor = Color.FromArgb(244, 247, 252);
            MinimumSize = new Size(1000, 620);
            Size = new Size(1200, 720);

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(12)
            };

            Label lblSupplier = CreateLabel("Supplier", 12, 15);
            comboSupplier = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(85, 11),
                Size = new Size(240, 30)
            };
            comboSupplier.SelectedIndexChanged += (sender, args) => LoadSelectedSupplier();

            Label lblPhone = CreateLabel("Phone", 340, 15);
            txtPhone = CreateTextBox(400, 11, 160);

            Label lblEmail = CreateLabel("Email", 575, 15);
            txtEmail = CreateTextBox(630, 11, 220);

            Button btnSaveContact = CreateButton("Save Info", 865, 10, 100, Color.FromArgb(37, 99, 235));
            btnSaveContact.Click += (sender, args) => SaveSupplierInfo();

            Label lblType = CreateLabel("Balance Type", 12, 58);
            comboBalanceType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(120, 54),
                Size = new Size(170, 30)
            };
            comboBalanceType.Items.AddRange(new object[] { "You owe supplier", "Supplier owes you" });
            comboBalanceType.SelectedIndex = 0;

            Label lblAmount = CreateLabel("Amount", 305, 58);
            txtBalanceAmount = CreateTextBox(370, 54, 120);
            txtBalanceAmount.KeyPress += NumericTextBox_KeyPress;

            comboCurrency = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(500, 54),
                Size = new Size(85, 30)
            };
            comboCurrency.Items.AddRange(new object[] { "USD", "L.L" });
            comboCurrency.SelectedIndex = 0;

            Button btnAdd = CreateButton("+ Add", 600, 52, 90, Color.FromArgb(22, 163, 74));
            btnAdd.Click += (sender, args) => AdjustSupplierBalance(true);

            Button btnLess = CreateButton("- Less", 700, 52, 90, Color.FromArgb(185, 28, 28));
            btnLess.Click += (sender, args) => AdjustSupplierBalance(false);

            Label lblFrom = CreateLabel("From", 805, 58);
            dateFrom = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Location = new Point(850, 54),
                Size = new Size(120, 30)
            };
            dateFrom.Value = DateTime.Today.AddMonths(-1);
            dateFrom.ValueChanged += (sender, args) => LoadSupplierItems();

            Label lblTo = CreateLabel("To", 980, 58);
            dateTo = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd",
                Location = new Point(1010, 54),
                Size = new Size(120, 30)
            };
            dateTo.ValueChanged += (sender, args) => LoadSupplierItems();

            Button btnPrint = CreateButton("Print", 12, 88, 90, Color.FromArgb(71, 85, 105));
            btnPrint.Click += (sender, args) => PrintSupplierInvoice();

            Button btnExcel = CreateButton("Excel", 112, 88, 90, Color.FromArgb(22, 163, 74));
            btnExcel.Click += (sender, args) => ExportSupplierInvoiceToExcel();

            topPanel.Controls.AddRange(new Control[]
            {
                lblSupplier, comboSupplier, lblPhone, txtPhone, lblEmail, txtEmail, btnSaveContact,
                lblType, comboBalanceType, lblAmount, txtBalanceAmount, comboCurrency, btnAdd, btnLess,
                lblFrom, dateFrom, lblTo, dateTo, btnPrint, btnExcel
            });

            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 210
            };

            gridSuppliers = CreateGrid();
            gridSuppliers.SelectionChanged += (sender, args) => SelectSupplierFromGrid();
            gridItems = CreateGrid();

            split.Panel1.Controls.Add(gridSuppliers);
            split.Panel2.Controls.Add(gridItems);

            Controls.Add(split);
            Controls.Add(topPanel);
            Load += SupplierBalanceForm_Load;
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(x, y),
                Text = text
            };
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Font = new Font("Segoe UI", 10F),
                Location = new Point(x, y),
                Size = new Size(width, 30)
            };
        }

        private Button CreateButton(string text, int x, int y, int width, Color color)
        {
            Button button = new Button
            {
                BackColor = color,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(x, y),
                Size = new Size(width, 32),
                Text = text
            };
            button.FlatAppearance.BorderSize = 0;
            return button;
        }

        private DataGridView CreateGrid()
        {
            return new DataGridView
            {
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
        }

        private void SupplierBalanceForm_Load(object sender, EventArgs e)
        {
            if (!PermissionService.EnsureScreenAccess(this, PermissionService.ScreenSuppliers))
            {
                return;
            }

            AuditLogger.EnsureSupplierBalanceColumns();
            LoadSuppliers();
            PermissionService.ApplyActionPermissions(this, PermissionService.ScreenSuppliers);
        }

        private void LoadSuppliers()
        {
            DataTable suppliers = ExecuteDataTable(@"
                SELECT supplier_id, name
                FROM Suppliers
                ORDER BY name;");

            comboSupplier.DisplayMember = "name";
            comboSupplier.ValueMember = "supplier_id";
            comboSupplier.DataSource = suppliers;

            LoadSupplierGrid();
            LoadSelectedSupplier();
        }

        private void LoadSupplierGrid()
        {
            gridSuppliers.DataSource = ExecuteDataTable(@"
                SELECT supplier_id AS [ID],
                       name AS [Supplier],
                       contact_info AS [Phone],
                       email AS [Email],
                       CASE
                           WHEN ISNULL(balance_usd, 0) > 0 OR ISNULL(balance_lb, 0) > 0 THEN N'You owe supplier'
                           WHEN ISNULL(balance_usd, 0) < 0 OR ISNULL(balance_lb, 0) < 0 THEN N'Supplier owes you'
                           ELSE N'No balance'
                       END AS [Balance Type],
                       ISNULL(balance_usd, 0) AS [USD Balance],
                       ISNULL(balance_lb, 0) AS [L.L Balance],
                       balance_updated_at AS [Balance DateTime]
                FROM Suppliers
                ORDER BY name;");
        }

        private void LoadSelectedSupplier()
        {
            if (SelectedSupplierId <= 0)
            {
                return;
            }

            DataTable table = ExecuteDataTable(@"
                SELECT contact_info, email
                FROM Suppliers
                WHERE supplier_id = " + SelectedSupplierId + ";");

            if (table.Rows.Count > 0)
            {
                txtPhone.Text = Convert.ToString(table.Rows[0]["contact_info"]);
                txtEmail.Text = Convert.ToString(table.Rows[0]["email"]);
            }

            LoadSupplierItems();
        }

        private void SelectSupplierFromGrid()
        {
            if (gridSuppliers.CurrentRow == null || !gridSuppliers.Columns.Contains("ID"))
            {
                return;
            }

            object value = gridSuppliers.CurrentRow.Cells["ID"].Value;
            if (value == null || value == DBNull.Value)
            {
                return;
            }

            comboSupplier.SelectedValue = Convert.ToInt32(value);
        }

        private void SaveSupplierInfo()
        {
            if (!PermissionService.CanSave(AppSession.UserId, AppSession.Role, PermissionService.ScreenSuppliers))
            {
                MessageBox.Show("You do not have permission to save suppliers.");
                return;
            }

            if (SelectedSupplierId <= 0)
            {
                MessageBox.Show("Select a supplier first.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Suppliers
                SET contact_info = @phone,
                    email = @email
                WHERE supplier_id = @id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", SelectedSupplierId);
                cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            LoadSupplierGrid();
            MessageBox.Show("Supplier info saved.");
        }

        private void AdjustSupplierBalance(bool add)
        {
            if (!PermissionService.CanSave(AppSession.UserId, AppSession.Role, PermissionService.ScreenSuppliers))
            {
                MessageBox.Show("You do not have permission to save suppliers.");
                return;
            }

            if (SelectedSupplierId <= 0)
            {
                MessageBox.Show("Select a supplier first.");
                return;
            }

            if (!TryParseDecimal(txtBalanceAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Enter a valid balance amount.");
                txtBalanceAmount.Focus();
                return;
            }

            bool supplierOwesYou = comboBalanceType.SelectedItem != null &&
                comboBalanceType.SelectedItem.ToString().Equals("Supplier owes you", StringComparison.OrdinalIgnoreCase);
            decimal signedAmount = supplierOwesYou ? -amount : amount;
            decimal delta = add ? signedAmount : -signedAmount;
            string currency = comboCurrency.SelectedItem != null ? comboCurrency.SelectedItem.ToString() : "USD";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Suppliers
                SET balance_usd = CASE WHEN @currency = 'USD' THEN ISNULL(balance_usd, 0) + @delta ELSE ISNULL(balance_usd, 0) END,
                    balance_lb = CASE WHEN @currency = 'LBP' THEN ISNULL(balance_lb, 0) + @delta ELSE ISNULL(balance_lb, 0) END,
                    balance_updated_at = GETDATE()
                WHERE supplier_id = @id;", conn))
            {
                cmd.Parameters.AddWithValue("@id", SelectedSupplierId);
                cmd.Parameters.AddWithValue("@currency", currency == "USD" ? "USD" : "LBP");
                cmd.Parameters.Add("@delta", SqlDbType.Decimal).Value = delta;
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            txtBalanceAmount.Clear();
            LoadSupplierGrid();
            LoadSelectedSupplier();
        }

        private void LoadSupplierItems()
        {
            if (SelectedSupplierId <= 0)
            {
                return;
            }

            DateTime from = dateFrom.Value.Date;
            DateTime to = dateTo.Value.Date.AddDays(1);

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT product_id AS [Product ID],
                       name AS [Product],
                       price_usd AS [Cost USD],
                       price_lb AS [Cost LBP],
                       stock_quantity AS [Stock],
                       barcode AS [Barcode],
                       created_at AS [DateTime]
                FROM Products
                WHERE TRY_CONVERT(INT, supplier_id) = @supplierId
                  AND created_at >= @from
                  AND created_at < @to
                ORDER BY created_at DESC;", conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("@supplierId", SelectedSupplierId);
                cmd.Parameters.Add("@from", SqlDbType.DateTime).Value = from;
                cmd.Parameters.Add("@to", SqlDbType.DateTime).Value = to;
                currentItemsTable = new DataTable();
                adapter.Fill(currentItemsTable);
                gridItems.DataSource = currentItemsTable;
            }
        }

        private void PrintSupplierInvoice()
        {
            if (currentItemsTable == null || currentItemsTable.Rows.Count == 0)
            {
                MessageBox.Show("No supplier items found for the selected date range.");
                return;
            }

            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = printDocument;
                preview.Width = 1000;
                preview.Height = 700;
                preview.ShowDialog(this);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font titleFont = new Font("Segoe UI", 16F, FontStyle.Bold);
            Font textFont = new Font("Segoe UI", 10F);
            Font headerFont = new Font("Segoe UI", 9F, FontStyle.Bold);
            int y = 40;

            string supplier = comboSupplier.Text;
            e.Graphics.DrawString("Supplier Invoice", titleFont, Brushes.Black, 40, y);
            y += 34;
            e.Graphics.DrawString("Supplier: " + supplier + "   Phone: " + txtPhone.Text + "   Email: " + txtEmail.Text, textFont, Brushes.Black, 40, y);
            y += 24;
            e.Graphics.DrawString("From: " + dateFrom.Value.ToString("yyyy-MM-dd") + "   To: " + dateTo.Value.ToString("yyyy-MM-dd"), textFont, Brushes.Black, 40, y);
            y += 36;

            e.Graphics.DrawString("Product", headerFont, Brushes.Black, 40, y);
            e.Graphics.DrawString("Cost USD", headerFont, Brushes.Black, 280, y);
            e.Graphics.DrawString("Cost LBP", headerFont, Brushes.Black, 390, y);
            e.Graphics.DrawString("Stock", headerFont, Brushes.Black, 520, y);
            e.Graphics.DrawString("DateTime", headerFont, Brushes.Black, 600, y);
            y += 24;

            foreach (DataRow row in currentItemsTable.Rows)
            {
                if (y > e.MarginBounds.Bottom - 30)
                {
                    e.HasMorePages = true;
                    return;
                }

                e.Graphics.DrawString(Convert.ToString(row["Product"]), textFont, Brushes.Black, 40, y);
                e.Graphics.DrawString(Convert.ToDecimal(row["Cost USD"]).ToString("N2"), textFont, Brushes.Black, 280, y);
                e.Graphics.DrawString(Convert.ToDecimal(row["Cost LBP"]).ToString("N0"), textFont, Brushes.Black, 390, y);
                e.Graphics.DrawString(Convert.ToString(row["Stock"]), textFont, Brushes.Black, 520, y);
                e.Graphics.DrawString(Convert.ToDateTime(row["DateTime"]).ToString("yyyy-MM-dd HH:mm"), textFont, Brushes.Black, 600, y);
                y += 22;
            }
        }

        private void ExportSupplierInvoiceToExcel()
        {
            if (currentItemsTable == null || currentItemsTable.Rows.Count == 0)
            {
                MessageBox.Show("No supplier items found for the selected date range.");
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                dialog.FileName = "SupplierInvoice_" + comboSupplier.Text.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                using (XLWorkbook workbook = new XLWorkbook())
                {
                    IXLWorksheet sheet = workbook.Worksheets.Add("Supplier Invoice");
                    sheet.Cell(1, 1).Value = "Supplier Invoice";
                    sheet.Cell(2, 1).Value = "Supplier";
                    sheet.Cell(2, 2).Value = comboSupplier.Text;
                    sheet.Cell(3, 1).Value = "Phone";
                    sheet.Cell(3, 2).Value = txtPhone.Text;
                    sheet.Cell(4, 1).Value = "Email";
                    sheet.Cell(4, 2).Value = txtEmail.Text;
                    sheet.Cell(5, 1).Value = "From";
                    sheet.Cell(5, 2).Value = dateFrom.Value.ToString("yyyy-MM-dd");
                    sheet.Cell(5, 3).Value = "To";
                    sheet.Cell(5, 4).Value = dateTo.Value.ToString("yyyy-MM-dd");
                    sheet.Cell(7, 1).InsertTable(currentItemsTable);
                    sheet.Columns().AdjustToContents();
                    workbook.SaveAs(dialog.FileName);
                }
            }

            MessageBox.Show("Supplier invoice exported to Excel.");
        }

        private DataTable ExecuteDataTable(string query)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
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
    }
}
