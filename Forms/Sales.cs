using ClosedXML.Excel;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;
using Pos_System.Services;
//using ClosedXML.Excel;  //l7atta 7afez data do8re be file excel 
//using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace Pos_System.Forms
{
    public partial class Sales : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        private const decimal SqlMoneyMax = 9999999999999999.99999999m;
        private const byte SqlMoneyPrecision = 24;
        private const byte SqlMoneyScale = 8;
        //private int currentSaleId = 0;

        private string _role;
        private string _username;
        private CheckBox chkUseDiscount;
        private TextBox txtDiscountAmount;
        private Label lblDiscountAmount;
        public static int LoggedInUserId; // متغير عام يخزن الـ user_id



        public Sales(string username, string role)
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
            labeldate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            _username = username;
            _role = role;

        }

        private decimal GetDecimalCellValue(DataGridViewRow row, string columnName)
        {
            return decimal.TryParse(row.Cells[columnName].Value?.ToString(), out decimal value)
                ? Math.Round(value, SqlMoneyScale)
                : 0m;
        }

        private int GetIntCellValue(DataGridViewRow row, string columnName)
        {
            return int.TryParse(row.Cells[columnName].Value?.ToString(), out int value)
                ? value
                : 0;
        }

        private bool ValidateSaleAmounts(decimal totalAmount, decimal? balanceAmount, out string validationMessage)
        {
            validationMessage = null;

            if (totalAmount > SqlMoneyMax || totalAmount < -SqlMoneyMax)
            {
                validationMessage = "إجمالي الفاتورة أكبر من الحد المسموح في قاعدة البيانات. عدّل نوع العمود total_amount إلى DECIMAL(24,8).";
                return false;
            }

            if (balanceAmount.HasValue &&
                (balanceAmount.Value > SqlMoneyMax || balanceAmount.Value < -SqlMoneyMax))
            {
                validationMessage = "الرصيد أكبر من الحد المسموح في قاعدة البيانات الحالية.";
                return false;
            }

            return true;
        }

        private void StyleSalesForm()
        {
            //BackColor = Color.FromArgb(244, 247, 252);
            //Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            panelheader.BackColor = Color.FromArgb(15, 23, 42);
            panel1.BackColor = Color.White;
            panel2.BackColor = Color.FromArgb(244, 247, 252);
            panel4.BackColor = Color.White;
            pnlProducts.BackColor = Color.White;

            label1.ForeColor = Color.FromArgb(234, 88, 12);
            label2.ForeColor = Color.White;
            label3.ForeColor = Color.FromArgb(191, 219, 254);
            labeldate.ForeColor = Color.FromArgb(226, 232, 240);

            StyleTextBox(txtsearch, 12F, false);
            StyleTextBox(txtquantity, 12F, true);
            StyleTextBox(txtdollar, 11F, true);
            StyleTextBox(txtPaidAmount, 11F, false);
            StyleTextBox(txtSaleId, 11F, false);

            StyleComboBox(cmbCategory);
            StyleComboBox(cmbCustomer);
            StyleComboBox(comboPaymentMethod);

            StylePrimaryButton(button5, Color.FromArgb(37, 99, 235));
            StylePrimaryButton(btnSaveSale, Color.FromArgb(22, 163, 74));
            StylePrimaryButton(btnsare3, Color.FromArgb(59, 130, 246));
            StylePrimaryButton(btnexcel, Color.FromArgb(5, 150, 105));
            StylePrimaryButton(btn3rdfetora, Color.FromArgb(245, 158, 11));
            StylePrimaryButton(btnmortaja3, Color.FromArgb(100, 116, 139));
            StylePrimaryButton(btndelete, Color.FromArgb(220, 38, 38));

            StyleRadioButton(rbDollar);
            StyleRadioButton(rbLebanon);
            rbLebanon.ForeColor = Color.FromArgb(30, 41, 59);
            rbDollar.ForeColor = Color.FromArgb(30, 41, 59);

            label9.ForeColor = Color.FromArgb(30, 41, 59);
            label11.ForeColor = Color.FromArgb(30, 41, 59);
            label9.BackColor = Color.Transparent;
            label11.BackColor = Color.Transparent;
            label9.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            label11.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);

            lblTotal.ForeColor = Color.FromArgb(22, 101, 52);
            lbtotal_lebanon.ForeColor = Color.FromArgb(30, 64, 175);
            lblTotal.BackColor = Color.FromArgb(240, 253, 244);
            lbtotal_lebanon.BackColor = Color.FromArgb(239, 246, 255);
            lblTotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lbtotal_lebanon.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            // lblTotal.Padding = new Padding(10, 0, 0, 0);
            //lbtotal_lebanon.Padding = new Padding(10, 0, 0, 0);
            lblTotal.BorderStyle = BorderStyle.FixedSingle;
            lbtotal_lebanon.BorderStyle = BorderStyle.FixedSingle;

            StyleDataGridView();

            btnSaveSale.Text = "بيع";
            btnsare3.Visible = false;
            txtdollar.Visible = false;
            btndollar.Visible = false;
            label4.Visible = false;

            AddDiscountControls();
            ConfigureSalesResponsiveLayout();
        }

        private void AddDiscountControls()
        {
            if (chkUseDiscount == null)
            {
                chkUseDiscount = new CheckBox
                {
                    Name = "chkUseDiscount",
                    Text = "Use Discount",
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59)
                };
                chkUseDiscount.CheckedChanged += (s, e) => ReapplyDiscountsToRows();
                panel1.Controls.Add(chkUseDiscount);
            }

            if (lblDiscountAmount == null)
            {
                lblDiscountAmount = new Label
                {
                    Name = "lblDiscountAmount",
                    Text = "Discount %:",
                    AutoSize = false,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(30, 41, 59),
                    TextAlign = ContentAlignment.MiddleRight
                };
                panel1.Controls.Add(lblDiscountAmount);
            }

            if (txtDiscountAmount == null)
            {
                txtDiscountAmount = new TextBox
                {
                    Name = "txtDiscountAmount",
                    BorderStyle = BorderStyle.FixedSingle,
                    Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                    TextAlign = HorizontalAlignment.Center
                };
                txtDiscountAmount.TextChanged += (s, e) => ReapplyDiscountsToRows();
                txtDiscountAmount.KeyPress += (s, e) =>
                {
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                        e.Handled = true;
                    if (e.KeyChar == '.' && txtDiscountAmount.Text.Contains("."))
                        e.Handled = true;
                };
                panel1.Controls.Add(txtDiscountAmount);
            }

            ApplyDiscountPermission();
        }

        private void ApplyDiscountPermission()
        {
            bool canUseDiscount = PermissionService.CanViewScreen(AppSession.UserId, AppSession.Role, PermissionService.ScreenUseDiscount);
            bool canUseManualDiscount = PermissionService.CanViewScreen(AppSession.UserId, AppSession.Role, PermissionService.ScreenManualDiscount);

            if (chkUseDiscount != null)
            {
                chkUseDiscount.Enabled = canUseDiscount;
                chkUseDiscount.Visible = canUseDiscount;
                if (!canUseDiscount)
                    chkUseDiscount.Checked = false;
            }

            if (lblDiscountAmount != null)
            {
                lblDiscountAmount.Visible = canUseDiscount && canUseManualDiscount;
            }

            if (txtDiscountAmount != null)
            {
                txtDiscountAmount.Enabled = canUseDiscount && canUseManualDiscount;
                txtDiscountAmount.Visible = canUseDiscount && canUseManualDiscount;
                if (!canUseDiscount || !canUseManualDiscount)
                    txtDiscountAmount.Clear();
            }
        }

        private void ConfigureSalesResponsiveLayout()
        {
            MinimumSize = new Size(760, 560);
            AutoScroll = false;
            panel1.MinimumSize = new Size(240, 0);
            panel1.AutoScroll = true;
            pnlProducts.AutoScroll = true;
            panelheader.Dock = DockStyle.None;
            panel1.Dock = DockStyle.None;
            panel2.Dock = DockStyle.None;
            pnlProducts.Dock = DockStyle.None;
            panel4.Dock = DockStyle.None;

            Resize -= Sales_Resize;
            Resize += Sales_Resize;
            panel2.Resize -= SalesTopPanel_Resize;
            panel2.Resize += SalesTopPanel_Resize;
            pnlProducts.Resize -= ProductsPanel_Resize;
            pnlProducts.Resize += ProductsPanel_Resize;

            ApplySalesResponsiveLayout();
        }

        private void Sales_Resize(object sender, EventArgs e)
        {
            ApplySalesResponsiveLayout();
        }

        private void SalesTopPanel_Resize(object sender, EventArgs e)
        {
            LayoutSalesTopPanel();
        }

        private void ProductsPanel_Resize(object sender, EventArgs e)
        {
            LayoutProductButtons();
        }

        private void ApplySalesResponsiveLayout()
        {
            int formWidth = Math.Max(1, ClientSize.Width);
            int formHeight = Math.Max(1, ClientSize.Height);
            int headerHeight = panelheader.Visible ? 60 : 0;
            int rightPanelWidth = formWidth < 1150 ? 250 : formWidth < 1320 ? 280 : 300;
            rightPanelWidth = Math.Min(rightPanelWidth, Math.Max(220, formWidth - 420));
            int leftWidth = formWidth - rightPanelWidth;
            int bodyHeight = Math.Max(1, formHeight - headerHeight);
            int productsHeight = Math.Max(86, Math.Min(175, bodyHeight / 4));

            panelheader.SetBounds(0, 0, formWidth, headerHeight);
            panel1.SetBounds(leftWidth, headerHeight, rightPanelWidth, bodyHeight);
            panel2.SetBounds(0, headerHeight, leftWidth, 58);
            pnlProducts.SetBounds(0, panel2.Bottom, leftWidth, productsHeight);
            panel4.SetBounds(0, pnlProducts.Bottom, leftWidth, Math.Max(1, formHeight - pnlProducts.Bottom));

            LayoutSalesTopPanel();
            LayoutRightSalesPanel();
            LayoutProductButtons();
            panel1.BringToFront();
        }

        private void LayoutSalesTopPanel()
        {
            int padding = 12;
            int right = panel2.ClientSize.Width - padding;
            if (right <= padding)
                return;

            int categoryWidth = Math.Min(190, Math.Max(125, panel2.ClientSize.Width / 6));
            cmbCategory.SetBounds(right - categoryWidth, 17, categoryWidth, cmbCategory.Height);

            label8.Location = new Point(cmbCategory.Left - label8.Width - 18, 15);
            txtquantity.SetBounds(label8.Left - 105, 13, 92, 32);

            int addWidth = 84;
            button5.SetBounds(Math.Max(padding + 160, txtquantity.Left - addWidth - 14), 13, addWidth, 32);

            int searchRight = button5.Left - 8;
            txtsearch.SetBounds(padding, 13, Math.Max(180, searchRight - padding), 33);
        }

        private void LayoutRightSalesPanel()
        {
            int margin = 14;
            int width = Math.Max(0, panel1.ClientSize.Width - (margin * 2));
            int bottom = panel1.ClientSize.Height - 14;
            int labelHeight = 22;
            int inputHeight = 32;
            int buttonHeight = 39;
            int gap = 6;

            txtPaidAmount.Width = width;
            cmbCustomer.Width = width;
            comboPaymentMethod.Width = width;
            txtSaleId.Width = width;
            lblTotal.Width = width;
            lbtotal_lebanon.Width = width;
            label9.Width = width;
            label11.Width = width;
            btnSaveSale.Width = width;
            btnsare3.Width = width;
            btnmortaja3.Width = width;
            btndelete.Width = width;

            label5.AutoSize = false;
            label6.AutoSize = false;
            label7.AutoSize = false;
            label10.AutoSize = false;
            label5.TextAlign = ContentAlignment.MiddleRight;
            label6.TextAlign = ContentAlignment.MiddleRight;
            label7.TextAlign = ContentAlignment.MiddleRight;
            label10.TextAlign = ContentAlignment.MiddleRight;

            int y = 14;
            label5.SetBounds(margin, y, width, labelHeight);
            y += labelHeight + 3;
            cmbCustomer.SetBounds(margin, y, width, inputHeight);
            y += inputHeight + gap;

            label6.SetBounds(margin, y, width, labelHeight);
            y += labelHeight + 3;
            txtPaidAmount.SetBounds(margin, y, width, inputHeight);
            y += inputHeight + gap;

            rbDollar.SetBounds(margin, y, 74, 25);
            rbLebanon.SetBounds(margin + 80, y, 105, 25);
            y += 28;

            if (chkUseDiscount != null && lblDiscountAmount != null && txtDiscountAmount != null)
            {
                chkUseDiscount.SetBounds(margin, y, width, 25);
                y += 28;
                int discountBoxWidth = Math.Min(88, Math.Max(64, width / 3));
                lblDiscountAmount.SetBounds(margin + discountBoxWidth + 8, y, Math.Max(80, width - discountBoxWidth - 8), 28);
                txtDiscountAmount.SetBounds(margin, y, discountBoxWidth, 28);
                y += 34;
            }

            label7.SetBounds(margin, y, width, labelHeight);
            y += labelHeight + 3;
            comboPaymentMethod.SetBounds(margin, y, width, inputHeight);
            y += inputHeight + gap;

            label10.SetBounds(margin, y, width, labelHeight);
            y += labelHeight + 3;
            txtSaleId.SetBounds(margin, y, width, inputHeight);
            y += inputHeight + 8;

            label9.SetBounds(margin, y, width, labelHeight);
            y += labelHeight + 2;
            lblTotal.SetBounds(margin, y, width, 27);
            y += 27 + 3;
            label11.SetBounds(margin, y, width, labelHeight);
            y += labelHeight + 2;
            lbtotal_lebanon.SetBounds(margin, y, width, 27);
            y += 27 + 10;

            int halfButtonWidth = Math.Max(120, (width - 6) / 2);
            int actionHeight = (buttonHeight * 4) + (gap * 3);
            int actionTop = Math.Max(y, bottom - actionHeight);

            btnSaveSale.SetBounds(margin, actionTop, width, buttonHeight);
            btnsare3.SetBounds(margin, btnSaveSale.Bottom + gap, width, buttonHeight);
            btnexcel.SetBounds(margin, btnSaveSale.Bottom + gap, halfButtonWidth, buttonHeight);
            btn3rdfetora.SetBounds(margin + halfButtonWidth + 6, btnexcel.Top, halfButtonWidth, buttonHeight);
            btnmortaja3.SetBounds(margin, btnexcel.Bottom + gap, width, buttonHeight);
            btndelete.SetBounds(margin, btnmortaja3.Bottom + gap, width, buttonHeight);

            panel1.AutoScrollMinSize = new Size(0, btndelete.Bottom + margin);
        }

        private void LayoutProductButtons()
        {
            if (pnlProducts.Controls.Count == 0)
                return;

            int margin = 10;
            int x = 10;
            int y = 10;
            int availableWidth = Math.Max(140, pnlProducts.ClientSize.Width - 20);
            int buttonWidth = Math.Max(110, Math.Min(170, availableWidth / Math.Max(1, availableWidth / 135)));

            foreach (Control control in pnlProducts.Controls)
            {
                if (!(control is Button button))
                    continue;

                button.Size = new Size(buttonWidth, 50);
                if (x + button.Width > availableWidth + 10)
                {
                    x = 10;
                    y += button.Height + margin;
                }

                button.Location = new Point(x, y);
                x += button.Width + margin;
            }
        }

        private void StyleTextBox(TextBox textBox, float fontSize, bool centered)
        {
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = Color.White;
            textBox.ForeColor = Color.FromArgb(30, 41, 59);
            textBox.Font = new Font("Segoe UI", fontSize, FontStyle.Regular);
            textBox.TextAlign = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left;
        }

        private void StyleComboBox(ComboBox comboBox)
        {
            comboBox.FlatStyle = FlatStyle.Flat;
            comboBox.BackColor = Color.White;
            comboBox.ForeColor = Color.FromArgb(30, 41, 59);
            comboBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Height = 34;
        }

        private void StylePrimaryButton(Button button, Color backColor)
        {
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        }

        private void StyleRadioButton(RadioButton radioButton)
        {
            radioButton.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            radioButton.AutoSize = true;
        }

        private void StyleDataGridView()
        {
            datagridsales.BackgroundColor = Color.White;
            datagridsales.BorderStyle = BorderStyle.None;
            datagridsales.EnableHeadersVisualStyles = false;
            datagridsales.GridColor = Color.FromArgb(226, 232, 240);
            datagridsales.RowHeadersVisible = false;
            datagridsales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            datagridsales.MultiSelect = false;
            datagridsales.AllowUserToResizeRows = false;
            datagridsales.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            datagridsales.DefaultCellStyle.BackColor = Color.White;
            datagridsales.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            datagridsales.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            datagridsales.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            datagridsales.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            datagridsales.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(15, 23, 42);
            datagridsales.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            datagridsales.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            datagridsales.ColumnHeadersHeight = 38;
            datagridsales.RowTemplate.Height = 30;
            datagridsales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            datagridsales.ScrollBars = ScrollBars.Vertical;
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
        allowed_user_id INT NULL,
        discount_type NVARCHAR(20) NOT NULL,
        discount_value DECIMAL(18,4) NOT NULL,
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

        private void EnsureSaleItemsDiscountColumns()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
IF COL_LENGTH('dbo.Sale_Items', 'original_unit_price') IS NULL
    ALTER TABLE dbo.Sale_Items ADD original_unit_price DECIMAL(24,8) NULL;
IF COL_LENGTH('dbo.Sale_Items', 'discount_amount') IS NULL
    ALTER TABLE dbo.Sale_Items ADD discount_amount DECIMAL(24,8) NULL;
IF COL_LENGTH('dbo.Sale_Items', 'discount_type') IS NULL
    ALTER TABLE dbo.Sale_Items ADD discount_type NVARCHAR(30) NULL;
IF COL_LENGTH('dbo.Sale_Items', 'discount_value') IS NULL
    ALTER TABLE dbo.Sale_Items ADD discount_value DECIMAL(18,4) NULL;
IF COL_LENGTH('dbo.Sale_Items', 'discount_by') IS NULL
    ALTER TABLE dbo.Sale_Items ADD discount_by NVARCHAR(100) NULL;", conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string FormatMoney(object value, string suffix = "")
        {
            if (value == null || value == DBNull.Value)
                return "-";

            if (!decimal.TryParse(value.ToString(), out decimal amount))
                return value.ToString();

            return amount.ToString("N" + SqlMoneyScale) + (string.IsNullOrWhiteSpace(suffix) ? "" : " " + suffix);
        }

        private Label CreateInvoiceValueLabel(string text, Point location, Size size, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", bold ? 11F : 10F, bold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = Color.FromArgb(45, 52, 54),
                TextAlign = ContentAlignment.MiddleRight
            };
        }

        private Panel CreateInvoiceInfoCard(string title, string value, Point location)
        {
            Panel card = new Panel
            {
                Location = location,
                Size = new Size(250, 60),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            Label titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 24,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 110, 114),
                TextAlign = ContentAlignment.MiddleRight
            };

            Label valueLabel = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                TextAlign = ContentAlignment.MiddleRight
            };

            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabel);

            return card;
        }

        private bool TryLoadInvoiceData(int saleId, out DataRow saleRow, out DataTable itemsTable)
        {
            saleRow = null;
            itemsTable = new DataTable();
            DataTable saleTable = new DataTable();
            EnsureSaleItemsDiscountColumns();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT sale_id, customer_name, payment_method, total_amount, balance_usd, balance_lb, sale_date
                      FROM Sales
                      WHERE sale_id = @sale_id", conn))
                {
                    cmd.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(saleTable);
                    }
                }

                if (saleTable.Rows.Count == 0)
                    return false;

                using (SqlCommand itemsCmd = new SqlCommand(
                    @"SELECT
                        product_id,
                        name_product AS product_name,
                        quantity,
                        original_unit_price,
                        unit_price,
                        ISNULL(discount_amount, 0) AS discount_amount,
                        ISNULL(discount_type, N'None') AS discount_type,
                        ISNULL(discount_value, 0) AS discount_value,
                        discount_by,
                        CAST(quantity * unit_price AS DECIMAL(24,8)) AS line_total,
                        created_by,
                        sale_date
                      FROM Sale_Items
                      WHERE sale_id = @sale_id
                      ORDER BY sale_item_id", conn))
                {
                    itemsCmd.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                    using (SqlDataAdapter adapter = new SqlDataAdapter(itemsCmd))
                    {
                        adapter.Fill(itemsTable);
                    }
                }
            }

            saleRow = saleTable.Rows[0];
            return true;
        }

        private void ExportInvoiceToExcel(DataRow saleRow, DataTable itemsTable)
        {
            int saleId = Convert.ToInt32(saleRow["sale_id"]);
            string customerName = saleRow["customer_name"].ToString();
            string paymentMethod = saleRow["payment_method"].ToString();
            decimal totalAmount = saleRow["total_amount"] != DBNull.Value ? Convert.ToDecimal(saleRow["total_amount"]) : 0m;
            decimal? balanceUsd = saleRow["balance_usd"] != DBNull.Value ? (decimal?)Convert.ToDecimal(saleRow["balance_usd"]) : null;
            decimal? balanceLb = saleRow["balance_lb"] != DBNull.Value ? (decimal?)Convert.ToDecimal(saleRow["balance_lb"]) : null;
            DateTime saleDate = saleRow["sale_date"] != DBNull.Value ? Convert.ToDateTime(saleRow["sale_date"]) : DateTime.Now;

            int totalQty = 0;
            foreach (DataRow row in itemsTable.Rows)
            {
                if (row["quantity"] != DBNull.Value)
                    totalQty += Convert.ToInt32(row["quantity"]);
            }

            Excel.Application excelApp = new Excel.Application();
            excelApp.Visible = true;
            excelApp.DisplayAlerts = false;

            Excel.Workbook workbook = excelApp.Workbooks.Add();
            Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];
            worksheet.Name = $"Invoice_{saleId}";
            worksheet.DisplayRightToLeft = true;

            Excel.Range titleRange = worksheet.Range["A1", "G2"];
            titleRange.Merge();
            titleRange.Value2 = "فاتورة المبيعات";
            titleRange.Font.Bold = true;
            titleRange.Font.Size = 22;
            titleRange.Font.Name = "Segoe UI";
            titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            titleRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            titleRange.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(18, 52, 86));
            titleRange.Font.Color = ColorTranslator.ToOle(Color.White);
            titleRange.RowHeight = 34;

            Excel.Range subTitleRange = worksheet.Range["A3", "G3"];
            subTitleRange.Merge();
            subTitleRange.Value2 = $"Invoice #{saleId}   |   {saleDate:yyyy/MM/dd HH:mm}";
            subTitleRange.Font.Size = 11;
            subTitleRange.Font.Name = "Segoe UI";
            subTitleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            subTitleRange.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(225, 233, 242));

            worksheet.Range["A5"].Value2 = "الزبون";
            worksheet.Range["B5"].Value2 = customerName;
            worksheet.Range["D5"].Value2 = "طريقة الدفع";
            worksheet.Range["E5"].Value2 = paymentMethod;
            worksheet.Range["A6"].Value2 = "إجمالي الفاتورة";
            worksheet.Range["B6"].Value2 = totalAmount;
            worksheet.Range["D6"].Value2 = "إجمالي الكمية";
            worksheet.Range["E6"].Value2 = totalQty;
            worksheet.Range["A7"].Value2 = "الرصيد بالدولار";
            worksheet.Range["B7"].Value2 = balanceUsd.HasValue ? (object)balanceUsd.Value : "-";
            worksheet.Range["D7"].Value2 = "الرصيد بالليرة";
            worksheet.Range["E7"].Value2 = balanceLb.HasValue ? (object)balanceLb.Value : "-";

            Excel.Range summaryRange = worksheet.Range["A5", "E7"];
            summaryRange.Font.Name = "Segoe UI";
            summaryRange.Font.Size = 11;
            summaryRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            summaryRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            summaryRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;
            summaryRange.RowHeight = 24;
            worksheet.Range["A5", "A7"].Interior.Color = ColorTranslator.ToOle(Color.FromArgb(239, 243, 248));
            worksheet.Range["D5", "D7"].Interior.Color = ColorTranslator.ToOle(Color.FromArgb(239, 243, 248));

            int headerRow = 10;
            worksheet.Cells[headerRow, 1] = "الصنف";
            worksheet.Cells[headerRow, 2] = "الكمية";
            worksheet.Cells[headerRow, 3] = "السعر قبل الخصم";
            worksheet.Cells[headerRow, 4] = "السعر بعد الخصم";
            worksheet.Cells[headerRow, 5] = "قيمة الخصم";
            worksheet.Cells[headerRow, 6] = "الإجمالي";
            worksheet.Cells[headerRow, 7] = "الخصم بواسطة";
            worksheet.Cells[headerRow, 8] = "وقت الإضافة";

            Excel.Range tableHeaderRange = worksheet.Range["A10", "H10"];
            tableHeaderRange.Font.Bold = true;
            tableHeaderRange.Font.Name = "Segoe UI";
            tableHeaderRange.Font.Size = 11;
            tableHeaderRange.Font.Color = ColorTranslator.ToOle(Color.White);
            tableHeaderRange.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(18, 52, 86));
            tableHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            tableHeaderRange.RowHeight = 26;

            int rowIndex = headerRow + 1;
            foreach (DataRow item in itemsTable.Rows)
            {
                worksheet.Cells[rowIndex, 1] = item["product_name"]?.ToString();
                worksheet.Cells[rowIndex, 2] = item["quantity"] != DBNull.Value ? Convert.ToInt32(item["quantity"]) : 0;
                worksheet.Cells[rowIndex, 3] = item["original_unit_price"] != DBNull.Value ? Convert.ToDecimal(item["original_unit_price"]) : 0m;
                worksheet.Cells[rowIndex, 4] = item["unit_price"] != DBNull.Value ? Convert.ToDecimal(item["unit_price"]) : 0m;
                worksheet.Cells[rowIndex, 5] = item["discount_amount"] != DBNull.Value ? Convert.ToDecimal(item["discount_amount"]) : 0m;
                worksheet.Cells[rowIndex, 6] = item["line_total"] != DBNull.Value ? Convert.ToDecimal(item["line_total"]) : 0m;
                worksheet.Cells[rowIndex, 7] = item["discount_by"]?.ToString();
                worksheet.Cells[rowIndex, 8] = item["sale_date"] != DBNull.Value
                    ? Convert.ToDateTime(item["sale_date"]).ToString("yyyy/MM/dd HH:mm")
                    : "";
                rowIndex++;
            }

            int lastItemRow = Math.Max(headerRow + 1, rowIndex - 1);
            Excel.Range itemsRange = worksheet.Range[$"A{headerRow}", $"H{lastItemRow}"];
            itemsRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            itemsRange.Font.Name = "Segoe UI";
            itemsRange.Font.Size = 10;
            itemsRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

            if (lastItemRow >= headerRow + 1)
            {
                Excel.Range moneyRange = worksheet.Range[$"C{headerRow + 1}", $"F{lastItemRow}"];
                moneyRange.NumberFormat = "#,##0.00000000";

                Excel.Range alternatingRange = worksheet.Range[$"A{headerRow + 1}", $"H{lastItemRow}"];
                alternatingRange.FormatConditions.Add(Type: Excel.XlFormatConditionType.xlExpression, Formula1: "=MOD(ROW(),2)=0");
                alternatingRange.FormatConditions[1].Interior.Color = ColorTranslator.ToOle(Color.FromArgb(247, 249, 252));
            }

            int totalsRow = lastItemRow + 2;
            worksheet.Cells[totalsRow, 5] = "الإجمالي النهائي";
            worksheet.Cells[totalsRow, 6] = totalAmount;
            Excel.Range totalsRange = worksheet.Range[$"E{totalsRow}", $"F{totalsRow}"];
            totalsRange.Font.Bold = true;
            totalsRange.Font.Name = "Segoe UI";
            totalsRange.Font.Size = 12;
            totalsRange.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(232, 242, 255));
            totalsRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            worksheet.Range[$"F{totalsRow}"].NumberFormat = "#,##0.00000000";

            int noteRow = totalsRow + 2;
            Excel.Range thanksRange = worksheet.Range[$"A{noteRow}", $"H{noteRow}"];
            thanksRange.Merge();
            thanksRange.Value2 = "شكراً لثقتكم";
            thanksRange.Font.Bold = true;
            thanksRange.Font.Name = "Segoe UI";
            thanksRange.Font.Size = 12;
            thanksRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            thanksRange.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(248, 250, 252));

            worksheet.Columns["A:F"].AutoFit();
            worksheet.Columns["A"].ColumnWidth = 24;
            worksheet.Columns["E"].ColumnWidth = 18;
            worksheet.Columns["F"].ColumnWidth = 19;

            Excel.Range pageRange = worksheet.Range["A1", $"F{noteRow}"];
            pageRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            Excel.PageSetup pageSetup = worksheet.PageSetup;
            pageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
            pageSetup.Zoom = false;
            pageSetup.FitToPagesWide = 1;
            pageSetup.FitToPagesTall = false;
            pageSetup.LeftMargin = excelApp.InchesToPoints(0.3);
            pageSetup.RightMargin = excelApp.InchesToPoints(0.3);
            pageSetup.TopMargin = excelApp.InchesToPoints(0.4);
            pageSetup.BottomMargin = excelApp.InchesToPoints(0.4);
            pageSetup.CenterHorizontally = true;
            pageSetup.PrintTitleRows = "$10:$10";
            pageSetup.CenterHeader = $"Invoice #{saleId}";

            worksheet.Activate();
        }

        private void ShowInvoiceForm(int saleId, string customerName, string paymentMethod, decimal totalAmount,
            decimal? balanceUsd, decimal? balanceLb, DateTime saleDate, DataTable itemsTable)
        {
            Form invoiceForm = new Form
            {
                Text = $"Invoice #{saleId}",
                Size = new Size(1080, 760),
                StartPosition = FormStartPosition.CenterScreen,
                BackColor = Color.FromArgb(245, 247, 250),
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.FromArgb(18, 52, 86)
            };

            Label titleLabel = new Label
            {
                Text = "فاتورة المبيعات",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                Location = new Point(760, 18),
                AutoSize = true
            };

            Label subtitleLabel = new Label
            {
                Text = $"Invoice #{saleId}   |   {saleDate:yyyy/MM/dd HH:mm}",
                ForeColor = Color.FromArgb(220, 230, 240),
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Location = new Point(640, 62),
                AutoSize = true
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(subtitleLabel);

            Panel bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                AutoScroll = true
            };

            Panel summaryPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            DataGridView detailsGrid = new DataGridView
            {
                Location = new Point(18, 168),
                Size = new Size(1010, 340),
                DataSource = itemsTable,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes
            };

            detailsGrid.EnableHeadersVisualStyles = false;
            detailsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(18, 52, 86);
            detailsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            detailsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            detailsGrid.ColumnHeadersHeight = 38;
            detailsGrid.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            detailsGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            detailsGrid.DefaultCellStyle.SelectionForeColor = Color.Black;
            detailsGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
            detailsGrid.RowTemplate.Height = 32;

            if (detailsGrid.Columns.Contains("product_id"))
                detailsGrid.Columns["product_id"].Visible = false;

            if (detailsGrid.Columns.Contains("product_name"))
                detailsGrid.Columns["product_name"].HeaderText = "الصنف";

            if (detailsGrid.Columns.Contains("quantity"))
                detailsGrid.Columns["quantity"].HeaderText = "الكمية";

            if (detailsGrid.Columns.Contains("unit_price"))
            {
                detailsGrid.Columns["unit_price"].HeaderText = "السعر بعد الخصم";
                detailsGrid.Columns["unit_price"].DefaultCellStyle.Format = "N8";
            }
            if (detailsGrid.Columns.Contains("original_unit_price"))
            {
                detailsGrid.Columns["original_unit_price"].HeaderText = "السعر قبل الخصم";
                detailsGrid.Columns["original_unit_price"].DefaultCellStyle.Format = "N8";
            }
            if (detailsGrid.Columns.Contains("discount_amount"))
            {
                detailsGrid.Columns["discount_amount"].HeaderText = "قيمة الخصم";
                detailsGrid.Columns["discount_amount"].DefaultCellStyle.Format = "N8";
            }
            if (detailsGrid.Columns.Contains("discount_type"))
                detailsGrid.Columns["discount_type"].HeaderText = "نوع الخصم";
            if (detailsGrid.Columns.Contains("discount_value"))
                detailsGrid.Columns["discount_value"].HeaderText = "نسبة/قيمة الخصم";
            if (detailsGrid.Columns.Contains("discount_by"))
                detailsGrid.Columns["discount_by"].HeaderText = "الخصم بواسطة";

            if (detailsGrid.Columns.Contains("line_total"))
            {
                detailsGrid.Columns["line_total"].HeaderText = "الإجمالي";
                detailsGrid.Columns["line_total"].DefaultCellStyle.Format = "N8";
            }

            if (detailsGrid.Columns.Contains("created_by"))
                detailsGrid.Columns["created_by"].HeaderText = "أضيف بواسطة";

            if (detailsGrid.Columns.Contains("sale_date"))
            {
                detailsGrid.Columns["sale_date"].HeaderText = "وقت الإضافة";
                detailsGrid.Columns["sale_date"].DefaultCellStyle.Format = "yyyy/MM/dd HH:mm";
            }

            bodyPanel.Controls.Add(CreateInvoiceInfoCard("الزبون", customerName, new Point(778, 18)));
            bodyPanel.Controls.Add(CreateInvoiceInfoCard("طريقة الدفع", paymentMethod, new Point(510, 18)));
            bodyPanel.Controls.Add(CreateInvoiceInfoCard("إجمالي الفاتورة", FormatMoney(totalAmount), new Point(242, 18)));
            bodyPanel.Controls.Add(CreateInvoiceInfoCard("عدد الأصناف", itemsTable.Rows.Count.ToString(), new Point(18, 18)));

            Label detailsTitle = new Label
            {
                Text = "تفاصيل الأصناف",
                Location = new Point(918, 130),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(18, 52, 86)
            };

            bodyPanel.Controls.Add(detailsTitle);
            bodyPanel.Controls.Add(detailsGrid);

            int totalQty = 0;
            foreach (DataRow row in itemsTable.Rows)
            {
                if (row["quantity"] != DBNull.Value)
                    totalQty += Convert.ToInt32(row["quantity"]);
            }

            summaryPanel.Controls.Add(CreateInvoiceValueLabel("ملخص الفاتورة", new Point(880, 10), new Size(130, 28), true));
            summaryPanel.Controls.Add(CreateInvoiceValueLabel("إجمالي الكمية: " + totalQty, new Point(740, 50), new Size(270, 28)));
            summaryPanel.Controls.Add(CreateInvoiceValueLabel("الإجمالي النهائي: " + FormatMoney(totalAmount), new Point(690, 85), new Size(320, 28), true));
            summaryPanel.Controls.Add(CreateInvoiceValueLabel("الرصيد بالدولار: " + (balanceUsd.HasValue ? FormatMoney(balanceUsd.Value, "$") : "-"), new Point(360, 50), new Size(300, 28)));
            summaryPanel.Controls.Add(CreateInvoiceValueLabel("الرصيد بالليرة: " + (balanceLb.HasValue ? FormatMoney(balanceLb.Value, "ل.ل") : "-"), new Point(360, 85), new Size(300, 28)));
            summaryPanel.Controls.Add(CreateInvoiceValueLabel("شكراً لثقتكم", new Point(30, 67), new Size(220, 28), true));

            Button closeButton = new Button
            {
                Text = "إغلاق",
                Size = new Size(120, 36),
                Location = new Point(18, 95),
                BackColor = Color.FromArgb(18, 52, 86),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.Click += (s, e) => invoiceForm.Close();
            summaryPanel.Controls.Add(closeButton);

            Button printButton = new Button
            {
                Text = "طباعة",
                Size = new Size(120, 36),
                Location = new Point(148, 95),
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            printButton.FlatAppearance.BorderSize = 0;
            printButton.Click += (s, e) => PrintViewedInvoice(saleId, customerName, paymentMethod, totalAmount, balanceUsd, balanceLb, saleDate, itemsTable, invoiceForm);
            summaryPanel.Controls.Add(printButton);

            invoiceForm.Controls.Add(bodyPanel);
            invoiceForm.Controls.Add(summaryPanel);
            invoiceForm.Controls.Add(headerPanel);
            invoiceForm.ShowDialog();
        }

        private void PrintViewedInvoice(int saleId, string customerName, string paymentMethod, decimal totalAmount,
            decimal? balanceUsd, decimal? balanceLb, DateTime saleDate, DataTable itemsTable, IWin32Window owner)
        {
            if (itemsTable == null || itemsTable.Rows.Count == 0)
            {
                MessageBox.Show("لا توجد أصناف في هذه الفاتورة.");
                return;
            }

            int printRowIndex = 0;
            PrintDocument printDocument = new PrintDocument();
            printDocument.BeginPrint += (sender, args) => printRowIndex = 0;
            printDocument.PrintPage += (sender, args) =>
            {
                PrintViewedInvoicePage(
                    args,
                    saleId,
                    customerName,
                    paymentMethod,
                    totalAmount,
                    balanceUsd,
                    balanceLb,
                    saleDate,
                    itemsTable,
                    ref printRowIndex);
            };

            using (PrintPreviewDialog preview = new PrintPreviewDialog())
            {
                preview.Document = printDocument;
                preview.Width = 1000;
                preview.Height = 700;
                preview.ShowDialog(owner);
            }
        }

        private void PrintViewedInvoicePage(PrintPageEventArgs e, int saleId, string customerName, string paymentMethod,
            decimal totalAmount, decimal? balanceUsd, decimal? balanceLb, DateTime saleDate, DataTable itemsTable,
            ref int printRowIndex)
        {
            using (Font titleFont = new Font("Segoe UI", 16F, FontStyle.Bold))
            using (Font textFont = new Font("Segoe UI", 10F))
            using (Font headerFont = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                int y = 40;
                int left = 40;
                int bottom = e.MarginBounds.Bottom;

                e.Graphics.DrawString("فاتورة المبيعات", titleFont, Brushes.Black, left, y);
                y += 34;
                e.Graphics.DrawString("رقم الفاتورة: " + saleId + "   التاريخ: " + saleDate.ToString("yyyy/MM/dd HH:mm"), textFont, Brushes.Black, left, y);
                y += 24;
                e.Graphics.DrawString("الزبون: " + customerName + "   طريقة الدفع: " + paymentMethod, textFont, Brushes.Black, left, y);
                y += 34;

                    e.Graphics.DrawString("الصنف", headerFont, Brushes.Black, left, y);
                    e.Graphics.DrawString("الكمية", headerFont, Brushes.Black, left + 260, y);
                    e.Graphics.DrawString("بعد الخصم", headerFont, Brushes.Black, left + 350, y);
                    e.Graphics.DrawString("الخصم", headerFont, Brushes.Black, left + 455, y);
                    e.Graphics.DrawString("الإجمالي", headerFont, Brushes.Black, left + 540, y);
                y += 24;

                while (printRowIndex < itemsTable.Rows.Count)
                {
                    if (y > bottom - 90)
                    {
                        e.HasMorePages = true;
                        return;
                    }

                    DataRow row = itemsTable.Rows[printRowIndex];
                    string productName = Convert.ToString(row["product_name"]);
                    if (productName.Length > 34)
                        productName = productName.Substring(0, 34);

                    int quantity = row["quantity"] != DBNull.Value ? Convert.ToInt32(row["quantity"]) : 0;
                    decimal unitPrice = row["unit_price"] != DBNull.Value ? Convert.ToDecimal(row["unit_price"]) : 0m;
                    decimal discountAmount = row["discount_amount"] != DBNull.Value ? Convert.ToDecimal(row["discount_amount"]) : 0m;
                    decimal lineTotal = row["line_total"] != DBNull.Value ? Convert.ToDecimal(row["line_total"]) : 0m;

                    e.Graphics.DrawString(productName, textFont, Brushes.Black, left, y);
                    e.Graphics.DrawString(quantity.ToString(), textFont, Brushes.Black, left + 260, y);
                    e.Graphics.DrawString(unitPrice.ToString("N2"), textFont, Brushes.Black, left + 350, y);
                    e.Graphics.DrawString(discountAmount.ToString("N2"), textFont, Brushes.Black, left + 455, y);
                    e.Graphics.DrawString(lineTotal.ToString("N2"), textFont, Brushes.Black, left + 540, y);
                    y += 22;
                    printRowIndex++;
                }

                y += 22;
                e.Graphics.DrawString("الإجمالي النهائي: " + FormatMoney(totalAmount), headerFont, Brushes.Black, left, y);
                y += 22;
                e.Graphics.DrawString("الرصيد بالدولار: " + (balanceUsd.HasValue ? FormatMoney(balanceUsd.Value, "$") : "-"), textFont, Brushes.Black, left, y);
                y += 22;
                e.Graphics.DrawString("الرصيد بالليرة: " + (balanceLb.HasValue ? FormatMoney(balanceLb.Value, "ل.ل") : "-"), textFont, Brushes.Black, left, y);
            }

            e.HasMorePages = false;
        }

        private void Sales_Load(object sender, EventArgs e)
        {
            StyleSalesForm();
            txtquantity.Text = "1"; // القيمة الافتراضية
            EnsureDiscountRulesTable();
            EnsureSaleItemsDiscountColumns();
            LoadCustomers();

            // ممكن تستدعي هنا أيضاً لو تحب
            LoadCategories();

            txtdollar.Text = POS_System.Program.SettingsManager.GetExchangeRate().ToString("0.####", CultureInfo.InvariantCulture);
            //  _role = reader["role"].ToString().Trim().ToLower();

            // التحقق من صلاحية المستخدم
            if (_role.Trim().ToLower() == "admin" || _role.Trim().ToLower() == "manager")
            {
                btndelete.Enabled = true;
                btnexcel.Enabled = true;
                btnmortaja3.Enabled = true;

            }
            else
            {
                btndelete.Enabled = false;
                btnexcel.Enabled = false;
                btnmortaja3.Enabled = false;
            }

           
            DataGridViewTextBoxColumn colProductId = new DataGridViewTextBoxColumn();
            colProductId.Name = "product_id";
            colProductId.HeaderText = "Product ID";
            colProductId.Visible = false;
            datagridsales.Columns.Add(colProductId);


            // باقي الأعمدة
            datagridsales.Columns.Add("product_name", "Product Name");
            datagridsales.Columns.Add("price_usd", "Price USD");
            datagridsales.Columns.Add("price_lb", "Price LB");
            datagridsales.Columns.Add("quantity", "Quantity");
            datagridsales.Columns.Add("total", "Total");
            datagridsales.Columns.Add("exchange_dollar", "Exchange Dollar");
            datagridsales.Columns.Add("date_time", "Date/Time");
            datagridsales.Columns.Add("balance", "Balance");
            datagridsales.Columns.Add("original_price_usd", "Original Price USD");
            datagridsales.Columns["original_price_usd"].Visible = false;
            datagridsales.Columns.Add("original_price_lb", "Original Price LB");
            datagridsales.Columns["original_price_lb"].Visible = false;
            datagridsales.Columns.Add("category_id", "Category ID");
            datagridsales.Columns["category_id"].Visible = false;
            datagridsales.Columns.Add("barcode", "Barcode");
            datagridsales.Columns["barcode"].Visible = false;
            datagridsales.Columns.Add("product_unit_id", "Product Unit ID");
            datagridsales.Columns["product_unit_id"].Visible = false;
            datagridsales.Columns.Add("warehouse_id", "Warehouse ID");
            datagridsales.Columns["warehouse_id"].Visible = false;
            datagridsales.Columns.Add("batch_number", "Batch");
            datagridsales.Columns["batch_number"].Visible = false;
            datagridsales.Columns.Add("serial_number", "Serial / IMEI");
            datagridsales.Columns["serial_number"].Visible = false;

            // زر Edit
            DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
            btnEdit.Name = "Edit";
            btnEdit.HeaderText = "Edit";
            btnEdit.Text = "Edit";
            btnEdit.UseColumnTextForButtonValue = true;
            datagridsales.Columns.Add(btnEdit);

            // زر Delete
            DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
            btnDelete.Name = "Delete";
            btnDelete.HeaderText = "Delete";
            btnDelete.Text = "Delete";
            btnDelete.UseColumnTextForButtonValue = true;
            datagridsales.Columns.Add(btnDelete);
            ConfigureSalesGridColumns();

            BeginInvoke(new Action(ApplySalesResponsiveLayout));

        }

        private void ConfigureSalesGridColumns()
        {
            SetColumnFill("product_name", 155, 120);
            SetColumnFill("price_usd", 82, 68);
            SetColumnFill("price_lb", 82, 68);
            SetColumnFill("quantity", 72, 58);
            SetColumnFill("total", 82, 68);
            SetColumnFill("exchange_dollar", 82, 68);
            SetColumnFill("date_time", 98, 78);
            SetColumnFill("balance", 78, 65);
            SetColumnFill("Edit", 58, 50);
            SetColumnFill("Delete", 62, 55);

            void SetColumnFill(string columnName, float fillWeight, int minimumWidth)
            {
                if (!datagridsales.Columns.Contains(columnName))
                    return;

                DataGridViewColumn column = datagridsales.Columns[columnName];
                column.FillWeight = fillWeight;
                column.MinimumWidth = minimumWidth;
            }
        }



        private void LoadCustomers()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT customer_id, name FROM Customers";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbCustomer.DataSource = dt;
                cmbCustomer.DisplayMember = "name"; // يظهر الاسم
                cmbCustomer.ValueMember = "customer_id";     // القيمة المخفية هي ID
            }
        }

        private void LoadCategories()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
        SELECT c.category_id, 
               c.category_name + ' (' + CAST(COUNT(p.product_id) AS NVARCHAR) + ')' AS display_name
        FROM Categories c
        LEFT JOIN Products p ON c.category_id = p.category_id
            AND (p.barcode IS NULL OR LTRIM(RTRIM(p.barcode)) = '')
        GROUP BY c.category_id, c.category_name";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                cmbCategory.DataSource = dt;
                cmbCategory.DisplayMember = "display_name"; // يظهر الاسم مع العدد
                cmbCategory.ValueMember = "category_id";    // يخزن الـ ID الصحيح
            }




        }


        private void btndollar_Click(object sender, EventArgs e)
        {
            MessageBox.Show("يتم تعديل سعر الدولار من شاشة Settings فقط.");
        }


        private void LoadProducts(int? categoryId = null)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT p.product_id,
                             p.name,
                             p.category_id,
                             p.barcode,
                             COALESCE(NULLIF(p.sale_price_usd, 0), p.price_usd, 0) AS price_usd,
                             COALESCE(NULLIF(p.sale_price_lb, 0), p.price_lb, 0) AS price_lb,
                             p.exchange_rate 
              FROM Products p
              WHERE (@catId IS NULL OR p.category_id = @catId)
                AND (p.barcode IS NULL OR LTRIM(RTRIM(p.barcode)) = '')", conn);

                // ✅ أفضل من AddWithValue
                da.SelectCommand.Parameters.Add("@catId", SqlDbType.Int).Value = categoryId.HasValue ? (object)categoryId.Value : DBNull.Value;

                DataTable dt = new DataTable();
                da.Fill(dt);

                pnlProducts.Controls.Clear();

                // ✅ إذا ما في منتجات
                if (dt.Rows.Count == 0)
                {
                    if (categoryId.HasValue)
                        MessageBox.Show("لا يوجد منتجات في هذه الفئة");
                    return;
                }

                Random rnd = new Random();

                int x = 10;
                int y = 10;
                int margin = 10;
                int maxWidth = pnlProducts.Width;

                foreach (DataRow row in dt.Rows)
                {
                    Button btn = new Button();
                    btn.Text = row["name"].ToString();
                    btn.Tag = row;
                    btn.Width = 120;
                    btn.Height = 50;

                    btn.Location = new Point(x, y);

                    btn.BackColor = Color.FromArgb(
                        rnd.Next(50, 200),
                        rnd.Next(50, 200),
                        rnd.Next(50, 200)
                    );
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;

                    // 🔁 ترتيب الأزرار
                    x += btn.Width + margin;

                    if (x + btn.Width > maxWidth)
                    {
                        x = 10;
                        y += btn.Height + margin;
                    }

                    // ✅ Click Event
                    btn.Click += (s, ev) =>
                    {
                        if (string.IsNullOrWhiteSpace(txtquantity.Text))
                        {
                            txtquantity.Text = "1";
                        }

                        if (!int.TryParse(txtquantity.Text, out int qty) || qty <= 0)
                        {
                            MessageBox.Show("الكمية غير صحيحة");
                            return;
                        }

                        DataRow productRow = (DataRow)((Button)s).Tag;

                        int productId = Convert.ToInt32(productRow["product_id"]);
                        string productName = productRow["name"].ToString();
                        int productCategoryId = productRow["category_id"] == DBNull.Value ? 0 : Convert.ToInt32(productRow["category_id"]);
                        string barcode = productRow["barcode"] == DBNull.Value ? "" : productRow["barcode"].ToString();

                        decimal priceUsd = productRow["price_usd"] == DBNull.Value ? 0 : Convert.ToDecimal(productRow["price_usd"]);
                        decimal exchangeRate = POS_System.Program.SettingsManager.GetExchangeRate();
                        priceUsd = ApplyDiscountToUsdPrice(productId, productCategoryId, productName, barcode, priceUsd, exchangeRate);
                        decimal priceLb = priceUsd * exchangeRate;

                        DateTime now = DateTime.Now;

                        decimal totalUsd = priceUsd * qty;

                        int rowIndex = datagridsales.Rows.Add(
                            productId,
                            productName,
                            priceUsd,
                            priceLb,
                            qty,
                            totalUsd,
                            exchangeRate,
                            now,
                            ""
                        );
                        DataGridViewRow addedRow = datagridsales.Rows[rowIndex];
                        addedRow.Cells["original_price_usd"].Value = productRow["price_usd"];
                        addedRow.Cells["original_price_lb"].Value = Convert.ToDecimal(productRow["price_usd"]) * exchangeRate;
                        addedRow.Cells["category_id"].Value = productCategoryId;
                        addedRow.Cells["barcode"].Value = barcode;

                        UpdateTotals();
                    };

                    pnlProducts.Controls.Add(btn);
                }

                LayoutProductButtons();
            }
        }

        private void cmbCategory_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // نتأكد إن القيمة ID صحيحة
            if (cmbCategory.SelectedValue == null || cmbCategory.SelectedValue is DataRowView)
                return;

            int categoryId = Convert.ToInt32(cmbCategory.SelectedValue);
            LoadProducts(categoryId);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string keyword = txtsearch.Text.Trim();

            if (string.IsNullOrEmpty(keyword))
            {
                MessageBox.Show("من فضلك أدخل اسم المنتج أو الباركود");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtquantity.Text))
            {
                MessageBox.Show("من فضلك أدخل الكمية");
                return;
            }

            int qty;
            if (!int.TryParse(txtquantity.Text, out qty) || qty <= 0)
            {
                MessageBox.Show("الكمية غير صحيحة");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"SELECT TOP(1) p.product_id,p.name,p.category_id,
                                        COALESCE(pb.barcode,p.barcode,N'') barcode,
                                        COALESCE(NULLIF(pu.selling_price,0),NULLIF(p.sale_price_usd,0),p.price_usd,0) AS price_usd,
                                        COALESCE(NULLIF(p.sale_price_lb,0),p.price_lb,0) AS price_lb,
                                        p.exchange_rate,pb.product_unit_id
                     FROM dbo.Products p
                     OUTER APPLY(
                       SELECT TOP(1) b.barcode,b.product_unit_id
                       FROM dbo.ProductBarcodes b
                       WHERE b.product_id=p.product_id AND b.barcode=@keyword
                       ORDER BY b.is_primary DESC,b.product_barcode_id
                     )pb
                     LEFT JOIN dbo.ProductUnits pu ON pu.product_unit_id=pb.product_unit_id
                     WHERE p.name=@keyword OR p.barcode=@keyword OR pb.barcode=@keyword
                     ORDER BY CASE WHEN pb.barcode=@keyword THEN 0 WHEN p.barcode=@keyword THEN 1 ELSE 2 END";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", keyword);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int productId = Convert.ToInt32(reader["product_id"]);
                    string productName = reader["name"].ToString();
                    int categoryId = reader["category_id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["category_id"]);
                    string barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    decimal priceUsd = reader["price_usd"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price_usd"]);
                    decimal exchangeRate = POS_System.Program.SettingsManager.GetExchangeRate();
                    priceUsd = ApplyDiscountToUsdPrice(productId, categoryId, productName, barcode, priceUsd, exchangeRate);
                    decimal priceLb = priceUsd * exchangeRate;
                    DateTime now = DateTime.Now;

                    decimal totalUsd = priceUsd * qty;

                    // ✅ إضافة الصف مع العمود product_id أولاً
                    int rowIndex = datagridsales.Rows.Add(productId, productName, priceUsd, priceLb, qty, totalUsd, exchangeRate, now, "");
                    DataGridViewRow addedRow = datagridsales.Rows[rowIndex];
                    addedRow.Cells["original_price_usd"].Value = reader["price_usd"];
                    addedRow.Cells["original_price_lb"].Value = Convert.ToDecimal(reader["price_usd"]) * exchangeRate;
                    addedRow.Cells["category_id"].Value = categoryId;
                    addedRow.Cells["barcode"].Value = barcode;
                    addedRow.Cells["product_unit_id"].Value = reader["product_unit_id"] == DBNull.Value ? (object)null : Convert.ToInt32(reader["product_unit_id"]);

                    UpdateTotals();
                }
                else
                {
                    MessageBox.Show("المنتج غير موجود");
                }

                conn.Close();
            }

            clearinput();

        }

        private decimal ApplyDiscountToUsdPrice(int productId, int categoryId, string productName, string barcode, decimal priceUsd, decimal exchangeRate)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, AppSession.Role, PermissionService.ScreenUseDiscount) ||
                chkUseDiscount == null || !chkUseDiscount.Checked || priceUsd <= 0)
                return priceUsd;

            decimal maxDiscount = POS_System.Program.SettingsManager.GetDecimalSetting("max_discount", 20m);
            decimal discountValue;
            bool canUseManualDiscount = PermissionService.CanViewScreen(AppSession.UserId, AppSession.Role, PermissionService.ScreenManualDiscount);
            if (canUseManualDiscount &&
                (decimal.TryParse(txtDiscountAmount?.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out discountValue) ||
                 decimal.TryParse(txtDiscountAmount?.Text?.Trim(), out discountValue)))
            {
                if (discountValue > 0)
                {
                    decimal safePercent = Math.Min(discountValue, maxDiscount);
                    return Math.Max(0m, Math.Round(priceUsd - (priceUsd * safePercent / 100m), SqlMoneyScale));
                }
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 discount_type, discount_value
FROM Discount_Rules
WHERE active = 1
  AND (allowed_user_id IS NULL OR allowed_user_id = @currentUserId)
  AND (
        (target_type = 'Product' AND (product_id = @productId OR barcode = @barcode OR rule_name = @productName))
        OR (target_type = 'Category' AND category_id = @categoryId)
      )
ORDER BY CASE WHEN target_type = 'Product' THEN 0 ELSE 1 END, discount_rule_id DESC;", conn))
            {
                cmd.Parameters.Add("@productId", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@categoryId", SqlDbType.Int).Value = categoryId;
                cmd.Parameters.Add("@barcode", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(barcode) ? (object)DBNull.Value : barcode;
                cmd.Parameters.Add("@productName", SqlDbType.NVarChar, 120).Value = string.IsNullOrWhiteSpace(productName) ? (object)DBNull.Value : productName;
                cmd.Parameters.Add("@currentUserId", SqlDbType.Int).Value = AppSession.UserId;
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return priceUsd;

                    string discountType = reader["discount_type"].ToString();
                    discountValue = Convert.ToDecimal(reader["discount_value"]);
                    if (discountType.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
                    {
                        return Math.Max(0m, Math.Round(priceUsd - discountValue, SqlMoneyScale));
                    }

                    decimal safePercent = Math.Min(discountValue, maxDiscount);
                    return Math.Max(0m, Math.Round(priceUsd - (priceUsd * safePercent / 100m), SqlMoneyScale));
                }
            }
        }

        private void ReapplyDiscountsToRows()
        {
            if (datagridsales == null || datagridsales.Columns.Count == 0 || !datagridsales.Columns.Contains("original_price_usd"))
                return;

            foreach (DataGridViewRow row in datagridsales.Rows)
            {
                if (row.IsNewRow)
                    continue;

                decimal originalUsd = GetDecimalCellValue(row, "original_price_usd");
                if (originalUsd <= 0)
                    originalUsd = GetDecimalCellValue(row, "price_usd");

                decimal exchangeRate = GetDecimalCellValue(row, "exchange_dollar");
                if (exchangeRate <= 0)
                    exchangeRate = POS_System.Program.SettingsManager.GetExchangeRate();

                int productId = GetIntCellValue(row, "product_id");
                int categoryId = GetIntCellValue(row, "category_id");
                string productName = row.Cells["product_name"].Value?.ToString() ?? "";
                string barcode = row.Cells["barcode"].Value?.ToString() ?? "";
                int qty = GetIntCellValue(row, "quantity");

                decimal discountedUsd = ApplyDiscountToUsdPrice(productId, categoryId, productName, barcode, originalUsd, exchangeRate);
                decimal discountedLb = discountedUsd * exchangeRate;
                row.Cells["price_usd"].Value = discountedUsd;
                row.Cells["price_lb"].Value = discountedLb;
                row.Cells["total"].Value = discountedUsd * qty;
            }

            UpdateTotals();
        }

        private decimal GetRowDiscountAmount(DataGridViewRow row, bool usd)
        {
            decimal original = GetDecimalCellValue(row, usd ? "original_price_usd" : "original_price_lb");
            decimal current = GetDecimalCellValue(row, usd ? "price_usd" : "price_lb");
            int qty = GetIntCellValue(row, "quantity");
            return Math.Round(Math.Max(0m, original - current) * qty, SqlMoneyScale);
        }

        private void datagridsales_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (datagridsales.Columns[e.ColumnIndex].Name == "Edit")
                {
                    // فتح الخلية الخاصة بالكمية للتعديل
                    datagridsales.CurrentCell = datagridsales.Rows[e.RowIndex].Cells["quantity"];
                    datagridsales.BeginEdit(true);
                }
                else if (datagridsales.Columns[e.ColumnIndex].Name == "Delete")
                {
                    // حذف الصف
                    datagridsales.Rows.RemoveAt(e.RowIndex);
                    UpdateTotals();
                }
                UpdateTotals();
            }
        }

        private void datagridsales_CellEndEdit_1(object sender, DataGridViewCellEventArgs e)
        {
            if (datagridsales.Columns[e.ColumnIndex].Name == "quantity")
            {
                try
                {
                    // قراءة السعر والكمية
                    decimal priceUsd = Convert.ToDecimal(datagridsales.Rows[e.RowIndex].Cells["price_usd"].Value);
                    decimal priceLb = Convert.ToDecimal(datagridsales.Rows[e.RowIndex].Cells["price_lb"].Value);
                    decimal exchangeRate = Convert.ToDecimal(datagridsales.Rows[e.RowIndex].Cells["exchange_dollar"].Value);

                    int qty = Convert.ToInt32(datagridsales.Rows[e.RowIndex].Cells["quantity"].Value);

                    // تحديث الإجمالي بالدولار
                    datagridsales.Rows[e.RowIndex].Cells["total"].Value = priceUsd * qty;

                    // تحديث قيمة exchange_dollar (حسب منطقك: إما السعر بالليرة ÷ السعر بالدولار أو العمود exchange_rate)
                    datagridsales.Rows[e.RowIndex].Cells["exchange_dollar"].Value = exchangeRate * qty;
                    // أو مثلاً: datagridsales.Rows[e.RowIndex].Cells["exchange_dollar"].Value = priceLb / priceUsd;

                }
                catch
                {
                    MessageBox.Show("الكمية غير صحيحة");
                    datagridsales.Rows[e.RowIndex].Cells["quantity"].Value = 1;
                }
                UpdateTotals();
            }
        }

        private void UpdateTotals()
        {
            decimal totalUsd = 0;
            decimal totalLb = 0;

            foreach (DataGridViewRow row in datagridsales.Rows)
            {
                if (row.IsNewRow) continue;

                try
                {
                    decimal priceUsd = Convert.ToDecimal(row.Cells["price_usd"].Value);
                    decimal priceLb = Convert.ToDecimal(row.Cells["price_lb"].Value);
                    int qty = Convert.ToInt32(row.Cells["quantity"].Value);

                    totalUsd += priceUsd * qty;
                    totalLb += priceLb * qty;
                }
                catch
                {
                    // تجاهل أي صف فيه قيم غير صحيحة
                }
            }

            lblTotal.Text = $"{totalUsd:F2}";
            lbtotal_lebanon.Text = $"{totalLb:F2}";
        }

        private void PrintReceipt(int saleId, string customerName, string paymentMethod)
        {
            PrintDocument printDoc = new PrintDocument();

            // لاحقاً لما تركب الطابعة الحرارية، غيّر اسم الطابعة هنا
            // printDoc.PrinterSettings.PrinterName = "اسم الطابعة الحرارية";

            printDoc.PrintPage += (sender, e) => PrintDoc_PrintPage(sender, e, saleId, customerName, paymentMethod);
            printDoc.Print();
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e, int saleId, string customerName, string paymentMethod)
        {
            Font font = new Font("Arial", 10);
            float y = 20;

            // رأس الفاتورة
            e.Graphics.DrawString("فاتورة المبيعات", new Font("Arial", 14, FontStyle.Bold), Brushes.Black, 20, y);
            y += 30;
            e.Graphics.DrawString("رقم الفاتورة: " + saleId, font, Brushes.Black, 20, y);
            y += 20;
            e.Graphics.DrawString("التاريخ: " + DateTime.Now.ToString("yyyy/MM/dd HH:mm"), font, Brushes.Black, 20, y);
            y += 20;
            e.Graphics.DrawString("اسم الزبون: " + customerName, font, Brushes.Black, 20, y);
            y += 20;
            e.Graphics.DrawString("طريقة الدفع: " + paymentMethod, font, Brushes.Black, 20, y);
            y += 40;

            // تفاصيل المنتجات
            foreach (DataGridViewRow row in datagridsales.Rows)
            {
                if (row.IsNewRow) continue;

                string product = row.Cells["product_name"].Value?.ToString();
                string qty = row.Cells["quantity"].Value?.ToString();
                string price = rbDollar.Checked
                    ? row.Cells["price_usd"].Value?.ToString() + " $"
                    : row.Cells["price_lb"].Value?.ToString() + " ل.ل";
                decimal discountAmount = GetRowDiscountAmount(row, rbDollar.Checked);
                string discountText = discountAmount > 0
                    ? (rbDollar.Checked ? $" - الخصم: {discountAmount:N2} $" : $" - الخصم: {discountAmount:N2} ل.ل")
                    : "";

                e.Graphics.DrawString($"{product} - الكمية: {qty} - السعر بعد الخصم: {price}{discountText}", font, Brushes.Black, 20, y);
                y += 20;
            }

            y += 20;
            string balanceText = rbDollar.Checked
                ? "الرصيد: " + (txtPaidAmount.Text) + " $"
                : "الرصيد: " + (txtPaidAmount.Text) + " ل.ل";

            e.Graphics.DrawString(balanceText, font, Brushes.Black, 20, y);
        }

        private void btnSaveSale_Click_1(object sender, EventArgs e)
        {
            DialogResult saleType = MessageBox.Show(
                "هل تريد البيع مع فاتورة؟",
                "نوع البيع",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (saleType == DialogResult.Cancel)
                return;

            bool printReceipt = saleType == DialogResult.Yes;
            if (ShowSaleReview(printReceipt))
            {
                ProcessSale(printReceipt);
            }
        }

        private bool TryReadPaidAmount(out decimal customerPaid)
        {
            if (!decimal.TryParse(txtPaidAmount.Text, out customerPaid))
            {
                MessageBox.Show("أدخل المبلغ المدفوع بشكل صحيح");
                return false;
            }

            return true;
        }

        private bool HasSaleItems()
        {
            foreach (DataGridViewRow row in datagridsales.Rows)
            {
                if (!row.IsNewRow)
                    return true;
            }

            return false;
        }

        private bool ShowSaleReview(bool withInvoice)
        {
            if (!HasSaleItems())
            {
                MessageBox.Show("أضف منتجات قبل البيع");
                return false;
            }

            if (!TryReadPaidAmount(out decimal customerPaid))
                return false;

            if (!rbDollar.Checked && !rbLebanon.Checked)
            {
                MessageBox.Show("اختر العملة قبل إتمام عملية البيع");
                return false;
            }

            if (!int.TryParse(cmbCustomer.SelectedValue?.ToString(), out int customerId))
            {
                MessageBox.Show("اختر زبون صحيح");
                return false;
            }

            decimal exchangeRate = POS_System.Program.SettingsManager.GetExchangeRate();
            GetCustomerBalance(customerId, out decimal currentBalanceUsd, out decimal currentBalanceLb);

            Form reviewForm = new Form
            {
                Text = withInvoice ? "مراجعة البيع مع فاتورة" : "مراجعة البيع بدون فاتورة",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(1120, 760),
                MinimumSize = new Size(940, 640),
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                BackColor = Color.FromArgb(244, 247, 252),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(24, 14, 24, 12)
            };

            Label titleLabel = new Label
            {
                Text = "مراجعة البيع قبل الحفظ",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleRight
            };

            Label subtitleLabel = new Label
            {
                Text = (withInvoice ? "مع فاتورة" : "بدون فاتورة") + "   |   " + cmbCustomer.Text + "   |   " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                Dock = DockStyle.Top,
                Height = 28,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(203, 213, 225),
                TextAlign = ContentAlignment.MiddleRight
            };

            DataGridView reviewGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                Font = new Font("Segoe UI", 10.5F),
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(226, 232, 240),
                AllowUserToDeleteRows = false
            };
            reviewGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            reviewGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            reviewGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            reviewGrid.ColumnHeadersHeight = 42;
            reviewGrid.DefaultCellStyle.BackColor = Color.White;
            reviewGrid.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            reviewGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            reviewGrid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            reviewGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            reviewGrid.RowTemplate.Height = 34;

            reviewGrid.Columns.Add("product_id", "Product ID");
            reviewGrid.Columns["product_id"].Visible = false;
            reviewGrid.Columns.Add("product_name", "المنتج");
            reviewGrid.Columns.Add("original_price", "قبل الخصم");
            reviewGrid.Columns.Add("price_usd", "السعر $");
            reviewGrid.Columns.Add("price_lb", "السعر ل.ل");
            reviewGrid.Columns.Add("quantity", "الكمية");
            reviewGrid.Columns.Add("discount_amount", "قيمة الخصم");
            reviewGrid.Columns.Add("total", "المجموع $");
            reviewGrid.Columns.Add("exchange_dollar", "Exchange");
            reviewGrid.Columns["exchange_dollar"].Visible = false;
            reviewGrid.Columns.Add("date_time", "الوقت");

            DataGridViewButtonColumn deleteColumn = new DataGridViewButtonColumn
            {
                Name = "Delete",
                HeaderText = "حذف",
                Text = "حذف",
                UseColumnTextForButtonValue = true,
                FlatStyle = FlatStyle.Flat,
                Width = 70
            };
            reviewGrid.Columns.Add(deleteColumn);

            foreach (DataGridViewRow row in datagridsales.Rows)
            {
                if (row.IsNewRow) continue;

                reviewGrid.Rows.Add(
                    row.Cells["product_id"].Value,
                    row.Cells["product_name"].Value,
                    row.Cells["original_price_usd"].Value,
                    row.Cells["price_usd"].Value,
                    row.Cells["price_lb"].Value,
                    row.Cells["quantity"].Value,
                    GetRowDiscountAmount(row, true),
                    row.Cells["total"].Value,
                    row.Cells["exchange_dollar"].Value,
                    row.Cells["date_time"].Value);
            }

            reviewGrid.Columns["product_name"].ReadOnly = true;
            reviewGrid.Columns["original_price"].ReadOnly = true;
            reviewGrid.Columns["discount_amount"].ReadOnly = true;
            reviewGrid.Columns["total"].ReadOnly = true;
            reviewGrid.Columns["date_time"].ReadOnly = true;

            Panel bodyPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(18)
            };

            Panel summaryPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 226,
                BackColor = Color.FromArgb(244, 247, 252),
                Padding = new Padding(18, 14, 18, 14)
            };

            Label totalValueLabel = CreateSummaryValueLabel();
            Label paidValueLabel = CreateSummaryValueLabel();
            Label changeValueLabel = CreateSummaryColoredValueLabel(Color.FromArgb(22, 101, 52));
            Label invoiceBalanceValueLabel = CreateSummaryColoredValueLabel(Color.FromArgb(180, 83, 9));
            Label currentBalanceValueLabel = CreateSummaryValueLabel();
            Label afterBalanceValueLabel = CreateSummaryColoredValueLabel(Color.FromArgb(37, 99, 235));

            TableLayoutPanel summaryCards = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 138,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            summaryCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            summaryCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            summaryCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
            summaryCards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            summaryCards.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            summaryCards.Controls.Add(CreateSummaryCard("المجموع", totalValueLabel, Color.FromArgb(37, 99, 235)), 0, 0);
            summaryCards.Controls.Add(CreateSummaryCard("المدفوع", paidValueLabel, Color.FromArgb(15, 118, 110)), 1, 0);
            summaryCards.Controls.Add(CreateSummaryCard("ارد له", changeValueLabel, Color.FromArgb(22, 163, 74)), 2, 0);
            summaryCards.Controls.Add(CreateSummaryCard("باقي هذه الفاتورة", invoiceBalanceValueLabel, Color.FromArgb(245, 158, 11)), 0, 1);
            summaryCards.Controls.Add(CreateSummaryCard("رصيد الزبون الحالي", currentBalanceValueLabel, Color.FromArgb(100, 116, 139)), 1, 1);
            summaryCards.Controls.Add(CreateSummaryCard("الرصيد بعد البيع", afterBalanceValueLabel, Color.FromArgb(79, 70, 229)), 2, 1);

            Panel actionPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 58,
                BackColor = Color.Transparent
            };

            Button okButton = new Button
            {
                Text = "بيع",
                DialogResult = DialogResult.OK,
                Width = 168,
                Height = 44,
                Left = 188,
                Top = 7,
                BackColor = Color.FromArgb(22, 163, 74),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            okButton.FlatAppearance.BorderSize = 0;

            Button noButton = new Button
            {
                Text = "عدم البيع",
                DialogResult = DialogResult.Cancel,
                Width = 168,
                Height = 44,
                Left = 10,
                Top = 7,
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            noButton.FlatAppearance.BorderSize = 0;

            void RefreshReviewTotals()
            {
                decimal totalUsd = 0m;
                decimal totalLb = 0m;

                foreach (DataGridViewRow row in reviewGrid.Rows)
                {
                    if (row.IsNewRow) continue;

                    int qty = GetReviewIntCellValue(row, "quantity");
                    decimal priceUsd = GetReviewDecimalCellValue(row, "price_usd");
                    decimal priceLb = GetReviewDecimalCellValue(row, "price_lb");
                    decimal originalPrice = GetReviewDecimalCellValue(row, "original_price");

                    row.Cells["discount_amount"].Value = Math.Round(Math.Max(0m, originalPrice - priceUsd) * qty, 2);
                    row.Cells["total"].Value = Math.Round(priceUsd * qty, 2);
                    totalUsd += priceUsd * qty;
                    totalLb += priceLb * qty;
                }

                decimal selectedTotal = rbDollar.Checked ? totalUsd : totalLb;
                decimal balance = selectedTotal - customerPaid;
                decimal customerOwes = balance > 0 ? balance : 0m;
                decimal change = balance < 0 ? Math.Abs(balance) : 0m;
                decimal saleBalanceUsd = rbDollar.Checked
                    ? balance
                    : exchangeRate > 0 ? balance / exchangeRate : 0m;
                decimal saleBalanceLb = rbDollar.Checked
                    ? balance * exchangeRate
                    : balance;
                decimal afterBalanceUsd = currentBalanceUsd + saleBalanceUsd;
                decimal afterBalanceLb = currentBalanceLb + saleBalanceLb;

                decimal customerOwesUsd = rbDollar.Checked
                    ? customerOwes
                    : exchangeRate > 0 ? customerOwes / exchangeRate : 0m;
                decimal customerOwesLb = rbDollar.Checked
                    ? customerOwes * exchangeRate
                    : customerOwes;
                decimal changeUsd = rbDollar.Checked
                    ? change
                    : exchangeRate > 0 ? change / exchangeRate : 0m;
                decimal changeLb = rbDollar.Checked
                    ? change * exchangeRate
                    : change;

                totalValueLabel.Text = $"{totalUsd:N2} $ / {totalLb:N2} ل.ل";
                paidValueLabel.Text = $"{customerPaid:N2} {(rbDollar.Checked ? "$" : "ل.ل")}";
                changeValueLabel.Text = $"{changeUsd:N2} $ / {changeLb:N2} ل.ل";
                invoiceBalanceValueLabel.Text = $"{customerOwesUsd:N2} $ / {customerOwesLb:N2} ل.ل";
                currentBalanceValueLabel.Text = $"{currentBalanceUsd:N2} $ / {currentBalanceLb:N2} ل.ل";
                afterBalanceValueLabel.Text = $"{afterBalanceUsd:N2} $ / {afterBalanceLb:N2} ل.ل";
            }

            reviewGrid.CellContentClick += (s, e) =>
            {
                if (e.RowIndex >= 0 && reviewGrid.Columns[e.ColumnIndex].Name == "Delete")
                {
                    reviewGrid.Rows.RemoveAt(e.RowIndex);
                    RefreshReviewTotals();
                }
            };

            reviewGrid.CellEndEdit += (s, e) => RefreshReviewTotals();
            reviewGrid.RowsRemoved += (s, e) => RefreshReviewTotals();

            actionPanel.Controls.Add(okButton);
            actionPanel.Controls.Add(noButton);
            summaryPanel.Controls.Add(summaryCards);
            summaryPanel.Controls.Add(actionPanel);
            bodyPanel.Controls.Add(reviewGrid);
            headerPanel.Controls.Add(subtitleLabel);
            headerPanel.Controls.Add(titleLabel);
            reviewForm.Controls.Add(bodyPanel);
            reviewForm.Controls.Add(summaryPanel);
            reviewForm.Controls.Add(headerPanel);
            reviewForm.AcceptButton = okButton;
            reviewForm.CancelButton = noButton;

            RefreshReviewTotals();

            if (reviewForm.ShowDialog(this) != DialogResult.OK)
                return false;

            if (reviewGrid.Rows.Count == 0)
            {
                MessageBox.Show("لا يوجد منتجات للبيع");
                return false;
            }

            datagridsales.Rows.Clear();

            foreach (DataGridViewRow row in reviewGrid.Rows)
            {
                if (row.IsNewRow) continue;

                int qty = GetReviewIntCellValue(row, "quantity");
                if (qty <= 0)
                {
                    MessageBox.Show("الكمية غير صحيحة");
                    return false;
                }

                decimal priceUsd = GetReviewDecimalCellValue(row, "price_usd");
                decimal priceLb = GetReviewDecimalCellValue(row, "price_lb");
                decimal originalUsd = GetReviewDecimalCellValue(row, "original_price");
                decimal rowExchangeRate = GetReviewDecimalCellValue(row, "exchange_dollar");
                decimal total = priceUsd * qty;

                int copiedRowIndex = datagridsales.Rows.Add(
                    row.Cells["product_id"].Value,
                    row.Cells["product_name"].Value,
                    priceUsd,
                    priceLb,
                    qty,
                    total,
                    row.Cells["exchange_dollar"].Value,
                    row.Cells["date_time"].Value,
                    "",
                    originalUsd,
                    originalUsd * rowExchangeRate,
                    0,
                    "");
                DataGridViewRow copiedRow = datagridsales.Rows[copiedRowIndex];
                copiedRow.Cells["original_price_usd"].Value = originalUsd;
                copiedRow.Cells["original_price_lb"].Value = originalUsd * rowExchangeRate;
                copiedRow.Cells["category_id"].Value = 0;
                copiedRow.Cells["barcode"].Value = "";
            }

            UpdateTotals();
            return true;

            Label CreateSummaryValueLabel()
            {
                return CreateSummaryColoredValueLabel(Color.FromArgb(15, 23, 42));
            }

            Label CreateSummaryColoredValueLabel(Color color)
            {
                return new Label
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                    ForeColor = color,
                    TextAlign = ContentAlignment.MiddleRight
                };
            }

            Panel CreateSummaryCard(string title, Label valueLabel, Color accentColor)
            {
                Panel card = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Margin = new Padding(6),
                    Padding = new Padding(12, 8, 12, 8)
                };

                Panel accent = new Panel
                {
                    Dock = DockStyle.Right,
                    Width = 5,
                    BackColor = accentColor
                };

                Label titleLabelInner = new Label
                {
                    Dock = DockStyle.Top,
                    Height = 22,
                    Text = title,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    TextAlign = ContentAlignment.MiddleRight
                };

                card.Controls.Add(valueLabel);
                card.Controls.Add(titleLabelInner);
                card.Controls.Add(accent);
                return card;
            }
        }

        private void GetCustomerBalance(int customerId, out decimal balanceUsd, out decimal balanceLb)
        {
            balanceUsd = 0m;
            balanceLb = 0m;

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(balance_usd, 0) AS balance_usd, ISNULL(balance_lb, 0) AS balance_lb FROM Customers WHERE customer_id = @customer_id",
                conn))
            {
                cmd.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        balanceUsd = Convert.ToDecimal(reader["balance_usd"]);
                        balanceLb = Convert.ToDecimal(reader["balance_lb"]);
                    }
                }
            }
        }

        private decimal GetReviewDecimalCellValue(DataGridViewRow row, string columnName)
        {
            return decimal.TryParse(row.Cells[columnName].Value?.ToString(), out decimal value) ? value : 0m;
        }

        private int GetReviewIntCellValue(DataGridViewRow row, string columnName)
        {
            return int.TryParse(row.Cells[columnName].Value?.ToString(), out int value) ? value : 0;
        }

        private bool ProcessSale(bool printReceipt)
        {
            decimal totalUsd = 0;
            decimal totalLb = 0;
            int totalQuantity = 0;

            // ✅ Calculate totals
            foreach (DataGridViewRow row in datagridsales.Rows)
            {
                if (row.IsNewRow) continue;

                int qty = int.TryParse(row.Cells["quantity"].Value?.ToString(), out int q) ? q : 0;
                decimal priceUsd = decimal.TryParse(row.Cells["price_usd"].Value?.ToString(), out decimal pu) ? pu : 0;
                decimal priceLb = decimal.TryParse(row.Cells["price_lb"].Value?.ToString(), out decimal pl) ? pl : 0;

                totalUsd += priceUsd * qty;
                totalLb += priceLb * qty;
                totalQuantity += qty;
            }

            if (totalQuantity <= 0)
            {
                MessageBox.Show("أضف منتجات قبل البيع");
                return false;
            }

            // ✅ Validate payment
            if (!TryReadPaidAmount(out decimal customerPaid))
                return false;

            int userId = LoginForm.LoggedInUserId;

            if (!int.TryParse(cmbCustomer.SelectedValue?.ToString(), out int customerId))
            {
                MessageBox.Show("اختر زبون صحيح");
                return false;
            }

            string customerName = cmbCustomer.Text;
            string paymentMethod = comboPaymentMethod.Text;

            if (comboPaymentMethod.SelectedIndex < 0 || string.IsNullOrWhiteSpace(paymentMethod))
            {
                MessageBox.Show("اختر طريقة الدفع أولاً");
                return false;
            }

            // Positive balance means the customer owes the business.
            // Negative balance means the business owes the customer.
            decimal? balanceUsd = rbDollar.Checked ? (decimal?)(totalUsd - customerPaid) : null;
            decimal? balanceLb = rbLebanon.Checked ? (decimal?)(totalLb - customerPaid) : null;

            // ✅ Safe rounding
            decimal safeTotalUsd = Math.Round(totalUsd, SqlMoneyScale);
            decimal safeTotalLb = Math.Round(totalLb, SqlMoneyScale);
            decimal? safeBalanceUsd = balanceUsd.HasValue ? (decimal?)Math.Round(balanceUsd.Value, SqlMoneyScale) : null;
            decimal? safeBalanceLb = balanceLb.HasValue ? (decimal?)Math.Round(balanceLb.Value, SqlMoneyScale) : null;

            if (!rbDollar.Checked && !rbLebanon.Checked)
            {
                MessageBox.Show("اختر العملة قبل إتمام عملية البيع");
                return false;
            }

            decimal selectedTotalAmount = rbDollar.Checked ? safeTotalUsd : safeTotalLb;
            decimal? selectedBalanceAmount = rbDollar.Checked ? safeBalanceUsd : safeBalanceLb;

            if (!ValidateSaleAmounts(selectedTotalAmount, selectedBalanceAmount, out string validationMessage))
            {
                MessageBox.Show(validationMessage);
                return false;
            }



            bool useUnifiedCheckout = true;
            if (useUnifiedCheckout)
            {
                try
                {
                    if (string.Equals(paymentMethod, "WHISH", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Use Split Payment for WHISH so the payer phone and provider authorization can be verified before checkout.");

                    CheckoutService.Request request = BuildCheckoutRequest(rbDollar.Checked, userId, customerId, customerName, rbDollar.Checked ? "USD" : "LBP", POS_System.Program.SettingsManager.GetExchangeRate());
                    if (customerPaid > 0m)
                        request.Payments.Add(new CheckoutService.Payment { Method = paymentMethod, Amount = customerPaid });
                    CheckoutService.Result checkout = new CheckoutService(connStr).Complete(request);
                    SalesAdvancedUiService.ClearCheckoutSource();
                    AuditService.Log("Sales", "Create", checkout.SaleId.ToString(), "Created unified checkout invoice " + checkout.SaleId);
                    MessageBox.Show("تمت العملية بنجاح - رقم الفاتورة: " + checkout.SaleId);
                    if (printReceipt) PrintReceipt(checkout.SaleId, customerName, paymentMethod);
                    clearinput();
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorLogService.Log(ex, "UNIFIED_CHECKOUT", Name);
                    MessageBox.Show("خطأ: " + ex.Message);
                    return false;
                }
            }

            int saleId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                EnsureSaleItemsDiscountColumns();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // ✅ INSERT SALES
                    string query = @"INSERT INTO Sales 
                                (user_id, customer_id, total_amount, payment_method, created_by, customer_name, balance_usd, balance_lb, quantity)
                                VALUES (@user_id, @customer_id, @total_amount, @payment_method, @created_by, @customer_name, @balance_usd, @balance_lb, @quantity);
                                SELECT CAST(SCOPE_IDENTITY() AS int);";

                    SqlCommand cmd = new SqlCommand(query, conn, transaction);

                    cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
                    cmd.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;

                    cmd.Parameters.Add(new SqlParameter("@total_amount", SqlDbType.Decimal)
                    {
                        Precision = SqlMoneyPrecision,
                        Scale = SqlMoneyScale,
                        Value = selectedTotalAmount
                    });

                    cmd.Parameters.Add("@payment_method", SqlDbType.NVarChar, 50).Value = paymentMethod;
                    cmd.Parameters.Add("@created_by", SqlDbType.NVarChar, 50).Value = LoginForm.LoggedInUsername;
                    cmd.Parameters.Add("@customer_name", SqlDbType.NVarChar, 100).Value = customerName;

                    cmd.Parameters.Add(new SqlParameter("@balance_usd", SqlDbType.Decimal)
                    {
                        Precision = SqlMoneyPrecision,
                        Scale = SqlMoneyScale,
                        Value = safeBalanceUsd.HasValue ? safeBalanceUsd.Value : (object)DBNull.Value
                    });

                    cmd.Parameters.Add(new SqlParameter("@balance_lb", SqlDbType.Decimal)
                    {
                        Precision = SqlMoneyPrecision,
                        Scale = SqlMoneyScale,
                        Value = safeBalanceLb.HasValue ? safeBalanceLb.Value : (object)DBNull.Value
                    });

                    cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = totalQuantity;

                    saleId = (int)cmd.ExecuteScalar();

                    // ✅ INSERT ITEMS + UPDATE STOCK
                    foreach (DataGridViewRow row in datagridsales.Rows)
                    {
                        if (row.IsNewRow) continue;

                        int qty = GetIntCellValue(row, "quantity");
                        decimal unitPrice = rbDollar.Checked
                            ? GetDecimalCellValue(row, "price_usd")
                            : GetDecimalCellValue(row, "price_lb");
                        decimal originalUnitPrice = rbDollar.Checked
                            ? GetDecimalCellValue(row, "original_price_usd")
                            : GetDecimalCellValue(row, "original_price_lb");
                        if (originalUnitPrice <= 0)
                            originalUnitPrice = unitPrice;
                        decimal discountAmount = Math.Max(0m, (originalUnitPrice - unitPrice) * qty);
                        decimal discountValue = originalUnitPrice > 0 ? (discountAmount / Math.Max(1, qty)) * 100m / originalUnitPrice : 0m;
                        string discountType = discountAmount > 0m
                            ? (PermissionService.CanViewScreen(AppSession.UserId, AppSession.Role, PermissionService.ScreenManualDiscount) &&
                               decimal.TryParse(txtDiscountAmount?.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal manualDiscount) &&
                               manualDiscount > 0m ? "Manual" : "Rule")
                            : "None";
                        int productId = GetIntCellValue(row, "product_id");
                        string productName = row.Cells["product_name"].Value?.ToString() ?? "";

                        if (unitPrice > SqlMoneyMax || unitPrice < -SqlMoneyMax)
                        {
                            throw new Exception($"سعر المنتج {productName} أكبر من الحد المسموح في قاعدة البيانات. عدّل العمود unit_price إلى DECIMAL(24,8).");
                        }

                        // Check stock
                        SqlCommand check = new SqlCommand("SELECT stock_quantity FROM Products WHERE product_id=@id", conn, transaction);
                        check.Parameters.Add("@id", SqlDbType.Int).Value = productId;

                        object result = check.ExecuteScalar();
                        if (result == null) continue;

                        int stock = Convert.ToInt32(result);
                        if (qty > stock)
                        {
                            throw new Exception($"المخزون غير كافي للمنتج {productName}");
                        }

                        // Insert item
                        SqlCommand cmdItem = new SqlCommand(@"INSERT INTO Sale_Items 
                            (sale_id, product_id, quantity, unit_price, original_unit_price, discount_amount, discount_type, discount_value, discount_by, name_product, customer_name, created_by)
                            VALUES (@sale_id, @product_id, @quantity, @unit_price, @original_unit_price, @discount_amount, @discount_type, @discount_value, @discount_by, @name_product, @customer_name, @created_by)", conn, transaction);

                        cmdItem.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                        cmdItem.Parameters.Add("@product_id", SqlDbType.Int).Value = productId;
                        cmdItem.Parameters.Add("@quantity", SqlDbType.Int).Value = qty;

                        cmdItem.Parameters.Add(new SqlParameter("@unit_price", SqlDbType.Decimal)
                        {
                            Precision = SqlMoneyPrecision,
                            Scale = SqlMoneyScale,
                            Value = Math.Round(unitPrice, SqlMoneyScale)
                        });
                        cmdItem.Parameters.Add(new SqlParameter("@original_unit_price", SqlDbType.Decimal)
                        {
                            Precision = SqlMoneyPrecision,
                            Scale = SqlMoneyScale,
                            Value = Math.Round(originalUnitPrice, SqlMoneyScale)
                        });
                        cmdItem.Parameters.Add(new SqlParameter("@discount_amount", SqlDbType.Decimal)
                        {
                            Precision = SqlMoneyPrecision,
                            Scale = SqlMoneyScale,
                            Value = Math.Round(discountAmount, SqlMoneyScale)
                        });
                        cmdItem.Parameters.Add("@discount_type", SqlDbType.NVarChar, 30).Value = discountType;
                        cmdItem.Parameters.Add(new SqlParameter("@discount_value", SqlDbType.Decimal)
                        {
                            Precision = 18,
                            Scale = 4,
                            Value = Math.Round(discountValue, 4)
                        });
                        cmdItem.Parameters.Add("@discount_by", SqlDbType.NVarChar, 100).Value = discountAmount > 0m ? LoginForm.LoggedInUsername : (object)DBNull.Value;

                        cmdItem.Parameters.Add("@name_product", SqlDbType.NVarChar, 100).Value = productName;
                        cmdItem.Parameters.Add("@customer_name", SqlDbType.NVarChar, 100).Value = customerName;
                        cmdItem.Parameters.Add("@created_by", SqlDbType.NVarChar, 50).Value = LoginForm.LoggedInUsername;

                        cmdItem.ExecuteNonQuery();

                        // Update stock
                        SqlCommand update = new SqlCommand(
                            "UPDATE Products SET stock_quantity = stock_quantity - @qty WHERE product_id = @id",
                            conn, transaction);

                        update.Parameters.Add("@qty", SqlDbType.Int).Value = qty;
                        update.Parameters.Add("@id", SqlDbType.Int).Value = productId;
                        update.ExecuteNonQuery();
                    }

                    UpdateCustomerBalance(conn, transaction, customerId, safeBalanceUsd, safeBalanceLb);

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("خطأ: " + ex.Message);
                    return false;
                }
            }

            AuditService.Log("Sales", "Create", saleId.ToString(), "Created sale invoice " + saleId);
            MessageBox.Show("تمت العملية بنجاح - رقم الفاتورة: " + saleId);

            if (printReceipt)
            {
                PrintReceipt(saleId, customerName, paymentMethod);
            }

            clearinput();
            return true;
        }

        private void UpdateCustomerBalance(
            SqlConnection conn,
            SqlTransaction transaction,
            int customerId,
            decimal? balanceUsdDelta,
            decimal? balanceLbDelta)
        {
            using (SqlCommand command = new SqlCommand(@"
                UPDATE Customers
                SET balance_usd = ISNULL(balance_usd, 0) + @balanceUsdDelta,
                    balance_lb = ISNULL(balance_lb, 0) + @balanceLbDelta,
                    balance = CASE
                        WHEN @balanceUsdDelta <> 0 THEN ISNULL(balance_usd, 0) + @balanceUsdDelta
                        WHEN @balanceLbDelta <> 0 THEN ISNULL(balance_lb, 0) + @balanceLbDelta
                        ELSE ISNULL(balance, 0)
                    END,
                    balance_updated_at = GETDATE()
                WHERE customer_id = @customerId;", conn, transaction))
            {
                command.Parameters.Add("@customerId", SqlDbType.Int).Value = customerId;
                command.Parameters.Add(new SqlParameter("@balanceUsdDelta", SqlDbType.Decimal)
                {
                    Precision = SqlMoneyPrecision,
                    Scale = SqlMoneyScale,
                    Value = balanceUsdDelta.HasValue ? balanceUsdDelta.Value : 0m
                });
                command.Parameters.Add(new SqlParameter("@balanceLbDelta", SqlDbType.Decimal)
                {
                    Precision = SqlMoneyPrecision,
                    Scale = SqlMoneyScale,
                    Value = balanceLbDelta.HasValue ? balanceLbDelta.Value : 0m
                });

                command.ExecuteNonQuery();
            }
        }

        private void clearinput()

        {
            txtPaidAmount.Clear();
            txtquantity.Clear();
            txtsearch.Clear();
            cmbCategory.SelectedIndex = 0;
            cmbCustomer.SelectedIndex = 0;
            comboPaymentMethod.SelectedIndex = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (ShowSaleReview(false))
            {
                ProcessSale(false);
            }
        }

        private void btn3rdfetora_Click(object sender, EventArgs e)
        {
            int saleId;
            if (!int.TryParse(txtSaleId.Text, out saleId))
            {
                MessageBox.Show("أدخل رقم فاتورة صحيح");
                return;
            }

            if (!TryLoadInvoiceData(saleId, out DataRow saleRow, out DataTable itemsTable))
            {
                MessageBox.Show("لم يتم العثور على الفاتورة");
                return;
            }

            ShowInvoiceForm(
                Convert.ToInt32(saleRow["sale_id"]),
                saleRow["customer_name"].ToString(),
                saleRow["payment_method"].ToString(),
                saleRow["total_amount"] != DBNull.Value ? Convert.ToDecimal(saleRow["total_amount"]) : 0m,
                saleRow["balance_usd"] != DBNull.Value ? (decimal?)Convert.ToDecimal(saleRow["balance_usd"]) : null,
                saleRow["balance_lb"] != DBNull.Value ? (decimal?)Convert.ToDecimal(saleRow["balance_lb"]) : null,
                saleRow["sale_date"] != DBNull.Value ? Convert.ToDateTime(saleRow["sale_date"]) : DateTime.Now,
                itemsTable);
        }

        private void button3_Click(object sender, EventArgs e)
        {

            int saleId;
            if (!int.TryParse(txtSaleId.Text, out saleId))
            {
                MessageBox.Show("أدخل رقم فاتورة صحيح");
                return;
            }

            if (!TryLoadInvoiceData(saleId, out DataRow saleRow, out DataTable itemsTable))
            {
                MessageBox.Show("لم يتم العثور على الفاتورة");
                return;
            }

            ExportInvoiceToExcel(saleRow, itemsTable);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int saleId;
            if (!int.TryParse(txtSaleId.Text, out saleId))
            {
                MessageBox.Show("أدخل رقم فاتورة صحيح");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string queryReturns = "DELETE FROM Returns WHERE sale_id = @sale_id";
                    string queryItems = "DELETE FROM Sale_Items WHERE sale_id = @sale_id";
                    string querySale = "DELETE FROM Sales WHERE sale_id = @sale_id";

                    using (SqlCommand cmdReturns = new SqlCommand(queryReturns, conn, transaction))
                    using (SqlCommand cmdItems = new SqlCommand(queryItems, conn, transaction))
                    using (SqlCommand cmdSale = new SqlCommand(querySale, conn, transaction))
                    {
                        cmdReturns.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                        cmdItems.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                        cmdSale.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;

                        cmdReturns.ExecuteNonQuery();
                        cmdItems.ExecuteNonQuery();
                        int rowsAffected = cmdSale.ExecuteNonQuery();

                        transaction.Commit();

                        if (rowsAffected > 0)
                        {
                            AuditService.Log("Sales", "Delete", saleId.ToString(), "Deleted sale invoice " + saleId);
                            MessageBox.Show("تم حذف الفاتورة بنجاح");
                        }
                        else
                        {
                            MessageBox.Show("لم يتم العثور على الفاتورة");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();

                    MessageBox.Show(
                        "تعذر حذف الفاتورة لأن هناك بيانات مرتبطة بها.\n" + ex.Message,
                        "خطأ في الحذف",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private class ReturnItemSelection
        {
            public int SaleId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public int AvailableQuantity { get; set; }
        }

        private bool TryGetReturnQuantity(out int quantity)
        {
            quantity = 1;

            if (string.IsNullOrWhiteSpace(txtquantity.Text))
                return true;

            if (!int.TryParse(txtquantity.Text.Trim(), out quantity) || quantity <= 0)
            {
                MessageBox.Show("الكمية غير صحيحة");
                return false;
            }

            return true;
        }

        private bool TryGetProductIdByBarcodeOrName(SqlConnection conn, SqlTransaction transaction, string keyword, out int productId)
        {
            productId = 0;

            using (SqlCommand cmd = new SqlCommand(
                @"SELECT TOP 1 p.product_id
                  FROM dbo.Products p
                  LEFT JOIN dbo.ProductBarcodes b ON b.product_id=p.product_id AND b.barcode=@keyword
                  WHERE p.barcode=@keyword OR p.name=@keyword OR b.barcode=@keyword
                  ORDER BY CASE WHEN b.barcode=@keyword THEN 0 WHEN p.barcode=@keyword THEN 1 ELSE 2 END", conn, transaction))
            {
                cmd.Parameters.Add("@keyword", SqlDbType.NVarChar, 100).Value = keyword;

                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                    return false;

                productId = Convert.ToInt32(result);
                return true;
            }
        }

        private ReturnItemSelection FindReturnableSaleItem(SqlConnection conn, SqlTransaction transaction, int productId, int quantity, int? saleId)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT TOP 1
                         si.sale_id,
                         si.product_id,
                         p.name,
                         si.quantity - ISNULL((
                             SELECT SUM(r.quantity)
                             FROM Returns r
                             WHERE r.sale_id = si.sale_id
                               AND r.product_id = si.product_id
                         ), 0) AS available_quantity
                  FROM Sale_Items si
                  INNER JOIN Sales s ON s.sale_id = si.sale_id
                  INNER JOIN Products p ON p.product_id = si.product_id
                  WHERE si.product_id = @product_id
                    AND (@sale_id IS NULL OR si.sale_id = @sale_id)
                    AND si.quantity - ISNULL((
                        SELECT SUM(r.quantity)
                        FROM Returns r
                        WHERE r.sale_id = si.sale_id
                          AND r.product_id = si.product_id
                    ), 0) >= @quantity
                  ORDER BY s.sale_date DESC, si.sale_item_id DESC", conn, transaction))
            {
                cmd.Parameters.Add("@product_id", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = quantity;
                cmd.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId.HasValue ? (object)saleId.Value : DBNull.Value;

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new ReturnItemSelection
                    {
                        SaleId = Convert.ToInt32(reader["sale_id"]),
                        ProductId = Convert.ToInt32(reader["product_id"]),
                        ProductName = reader["name"].ToString(),
                        AvailableQuantity = Convert.ToInt32(reader["available_quantity"])
                    };
                }
            }
        }

        private void InsertReturnAndRestoreStock(SqlConnection conn, SqlTransaction transaction, int saleId, int productId, int quantity)
        {
            using (SqlCommand cmdUpdate = new SqlCommand(
                @"UPDATE Products
                  SET stock_quantity = stock_quantity + @qty
                  WHERE product_id = @product_id", conn, transaction))
            {
                cmdUpdate.Parameters.Add("@qty", SqlDbType.Int).Value = quantity;
                cmdUpdate.Parameters.Add("@product_id", SqlDbType.Int).Value = productId;
                cmdUpdate.ExecuteNonQuery();
            }

            using (SqlCommand cmdReturn = new SqlCommand(
                @"INSERT INTO Returns (sale_id, product_id, quantity, return_date)
                  VALUES (@sale_id, @product_id, @quantity, GETDATE())", conn, transaction))
            {
                cmdReturn.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                cmdReturn.Parameters.Add("@product_id", SqlDbType.Int).Value = productId;
                cmdReturn.Parameters.Add("@quantity", SqlDbType.Int).Value = quantity;
                cmdReturn.ExecuteNonQuery();
            }
        }

        private void UpdateSaleReturnStatus(SqlConnection conn, SqlTransaction transaction, int saleId)
        {
            using (SqlCommand cmd = new SqlCommand(
                @"IF NOT EXISTS (
                      SELECT 1
                      FROM Sale_Items si
                      WHERE si.sale_id = @sale_id
                        AND si.quantity - ISNULL((
                            SELECT SUM(r.quantity)
                            FROM Returns r
                            WHERE r.sale_id = si.sale_id
                              AND r.product_id = si.product_id
                        ), 0) > 0
                  )
                  BEGIN
                      UPDATE Sales SET is_returned = 1, status = N'مرتجع' WHERE sale_id = @sale_id
                  END
                  ELSE
                  BEGIN
                      UPDATE Sales SET is_returned = 0, status = N'مرتجع جزئي' WHERE sale_id = @sale_id
                  END", conn, transaction))
            {
                cmd.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                cmd.ExecuteNonQuery();
            }
        }

        private bool ReturnWholeInvoice(int saleId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    List<(int productId, int qty)> returnedProducts = new List<(int, int)>();

                    using (SqlCommand cmdDetails = new SqlCommand(
                        @"SELECT
                                 si.product_id,
                                 si.quantity - ISNULL((
                                     SELECT SUM(r.quantity)
                                     FROM Returns r
                                     WHERE r.sale_id = si.sale_id
                                       AND r.product_id = si.product_id
                                 ), 0) AS remaining_quantity
                          FROM Sale_Items si
                          WHERE si.sale_id = @sale_id
                            AND si.quantity - ISNULL((
                                SELECT SUM(r.quantity)
                                FROM Returns r
                                WHERE r.sale_id = si.sale_id
                                  AND r.product_id = si.product_id
                            ), 0) > 0", conn, transaction))
                    {
                        cmdDetails.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;

                        using (SqlDataReader reader = cmdDetails.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                returnedProducts.Add((
                                    Convert.ToInt32(reader["product_id"]),
                                    Convert.ToInt32(reader["remaining_quantity"])));
                            }
                        }
                    }

                    if (returnedProducts.Count == 0)
                    {
                        transaction.Rollback();
                        MessageBox.Show("لم يتم العثور على الفاتورة أو أنها مرتجعة بالكامل");
                        return false;
                    }

                    foreach (var item in returnedProducts)
                    {
                        InsertReturnAndRestoreStock(conn, transaction, saleId, item.productId, item.qty);
                    }

                    UpdateSaleReturnStatus(conn, transaction, saleId);
                    transaction.Commit();
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("خطأ أثناء تسجيل المرتجع: " + ex.Message);
                    return false;
                }
            }
        }

        private bool ReturnProductByBarcodeOrName(string keyword, int quantity, int? saleId)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    if (!TryGetProductIdByBarcodeOrName(conn, transaction, keyword, out int productId))
                    {
                        transaction.Rollback();
                        MessageBox.Show("المنتج غير موجود");
                        return false;
                    }

                    ReturnItemSelection item = FindReturnableSaleItem(conn, transaction, productId, quantity, saleId);
                    if (item == null)
                    {
                        transaction.Rollback();
                        MessageBox.Show(saleId.HasValue
                            ? "هذا المنتج غير موجود في الفاتورة أو تم إرجاعه سابقاً"
                            : "لم يتم العثور على عملية بيع لهذا المنتج أو تم إرجاع الكمية سابقاً");
                        return false;
                    }

                    InsertReturnAndRestoreStock(conn, transaction, item.SaleId, item.ProductId, quantity);
                    UpdateSaleReturnStatus(conn, transaction, item.SaleId);
                    transaction.Commit();

                    AuditLogger.Log("EDIT", "Sales", item.SaleId, "Returned product " + item.ProductId + " / quantity: " + quantity);
                    MessageBox.Show("تم تسجيل مرتجع المنتج: " + item.ProductName + "\nالكمية: " + quantity);
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("خطأ أثناء تسجيل المرتجع: " + ex.Message);
                    return false;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool hasSaleId = int.TryParse(txtSaleId.Text.Trim(), out int saleId);
            string keyword = txtsearch.Text.Trim();
            bool hasKeyword = !string.IsNullOrWhiteSpace(keyword) &&
                              !keyword.Equals("search by name product or barcode", StringComparison.OrdinalIgnoreCase);

            if (hasKeyword)
            {
                if (!TryGetReturnQuantity(out int quantity))
                    return;

                bool returnedByItem = ReturnProductByBarcodeOrName(keyword, quantity, hasSaleId ? (int?)saleId : null);
                if (returnedByItem)
                    clearinput();

                return;
            }

            if (!hasSaleId)
            {
                MessageBox.Show("أدخل رقم الفاتورة أو باركود المنتج في خانة البحث");
                return;
            }

            if (ReturnWholeInvoice(saleId))
            {
                AuditLogger.Log("EDIT", "Sales", saleId, "Marked invoice as returned and restored stock");
                MessageBox.Show("تم تسجيل المرتجع وإضافة المنتجات مرة أخرى للمخزون");
                clearinput();
            }
        }


        ////////////////////////////////
        /// لحفظ الفاتورة في ملف Excel مباشرة على سطح المكتب
        //////////////////////////////////


        //string query = @"SELECT sale_id, customer_name, payment_method, total_amount, 
        //                    balance_usd, balance_lb, sale_date 
        //             FROM Sales 
        //             WHERE sale_id = @sale_id";

        //SqlCommand cmd = new SqlCommand(query, conn);
        //cmd.Parameters.AddWithValue("@sale_id", saleId);

        //conn.Open();
        //SqlDataReader reader = cmd.ExecuteReader();

        //if (reader.Read())
        //{
        //    // إنشاء ملف Excel جديد
        //    using (var workbook = new XLWorkbook())
        //    {
        //        var worksheet = workbook.Worksheets.Add("فاتورة");

        //        worksheet.Cell(1, 1).Value = "فاتورة المبيعات";
        //        worksheet.Cell(2, 1).Value = "رقم الفاتورة";
        //        worksheet.Cell(2, 2).Value = reader["sale_id"].ToString();

        //        worksheet.Cell(3, 1).Value = "اسم الزبون";
        //        worksheet.Cell(3, 2).Value = reader["customer_name"].ToString();

        //        worksheet.Cell(4, 1).Value = "طريقة الدفع";
        //        worksheet.Cell(4, 2).Value = reader["payment_method"].ToString();

        //        worksheet.Cell(5, 1).Value = "المبلغ الإجمالي";
        //        worksheet.Cell(5, 2).Value = reader["total_amount"].ToString();

        //        if (reader["balance_usd"] != DBNull.Value)
        //        {
        //            worksheet.Cell(6, 1).Value = "الرصيد بالدولار";
        //            worksheet.Cell(6, 2).Value = reader["balance_usd"].ToString();
        //        }

        //        if (reader["balance_lb"] != DBNull.Value)
        //        {
        //            worksheet.Cell(7, 1).Value = "الرصيد بالليرة";
        //            worksheet.Cell(7, 2).Value = reader["balance_lb"].ToString();
        //        }

        //        worksheet.Cell(8, 1).Value = "التاريخ";
        //        worksheet.Cell(8, 2).Value = Convert.ToDateTime(reader["sale_date"]).ToString("yyyy/MM/dd HH:mm");

        //        // حفظ الملف
        //        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"فاتورة_{saleId}.xlsx");
        //        workbook.SaveAs(filePath);

        //        MessageBox.Show("تم تصدير الفاتورة إلى Excel:\n" + filePath, "نجاح");
        //    }
        //}
        //else
        //{
        //    MessageBox.Show("لم يتم العثور على الفاتورة");
        //}

        //conn.Close();
    }

    }


    
