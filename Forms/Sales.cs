using ClosedXML.Excel;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
//using ClosedXML.Excel;  //l7atta 7afez data do8re be file excel 
//using System.IO;
using Excel = Microsoft.Office.Interop.Excel;

namespace Pos_System.Forms
{
    public partial class Sales : Form
    {
        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";
        private const decimal SqlMoneyMax = 9999999999999999.99999999m;
        private const byte SqlMoneyPrecision = 24;
        private const byte SqlMoneyScale = 8;
        //private int currentSaleId = 0;

        private string _role;
        private string _username;
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
            StylePrimaryButton(btndollar, Color.FromArgb(37, 99, 235));
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
                        unit_price,
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
            worksheet.Cells[headerRow, 3] = "سعر الوحدة";
            worksheet.Cells[headerRow, 4] = "الإجمالي";
            worksheet.Cells[headerRow, 5] = "أضيف بواسطة";
            worksheet.Cells[headerRow, 6] = "وقت الإضافة";

            Excel.Range tableHeaderRange = worksheet.Range["A10", "F10"];
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
                worksheet.Cells[rowIndex, 3] = item["unit_price"] != DBNull.Value ? Convert.ToDecimal(item["unit_price"]) : 0m;
                worksheet.Cells[rowIndex, 4] = item["line_total"] != DBNull.Value ? Convert.ToDecimal(item["line_total"]) : 0m;
                worksheet.Cells[rowIndex, 5] = item["created_by"]?.ToString();
                worksheet.Cells[rowIndex, 6] = item["sale_date"] != DBNull.Value
                    ? Convert.ToDateTime(item["sale_date"]).ToString("yyyy/MM/dd HH:mm")
                    : "";
                rowIndex++;
            }

            int lastItemRow = Math.Max(headerRow + 1, rowIndex - 1);
            Excel.Range itemsRange = worksheet.Range[$"A{headerRow}", $"F{lastItemRow}"];
            itemsRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            itemsRange.Font.Name = "Segoe UI";
            itemsRange.Font.Size = 10;
            itemsRange.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

            if (lastItemRow >= headerRow + 1)
            {
                Excel.Range moneyRange = worksheet.Range[$"C{headerRow + 1}", $"D{lastItemRow}"];
                moneyRange.NumberFormat = "#,##0.00000000";

                Excel.Range alternatingRange = worksheet.Range[$"A{headerRow + 1}", $"F{lastItemRow}"];
                alternatingRange.FormatConditions.Add(Type: Excel.XlFormatConditionType.xlExpression, Formula1: "=MOD(ROW(),2)=0");
                alternatingRange.FormatConditions[1].Interior.Color = ColorTranslator.ToOle(Color.FromArgb(247, 249, 252));
            }

            int totalsRow = lastItemRow + 2;
            worksheet.Cells[totalsRow, 3] = "الإجمالي النهائي";
            worksheet.Cells[totalsRow, 4] = totalAmount;
            Excel.Range totalsRange = worksheet.Range[$"C{totalsRow}", $"D{totalsRow}"];
            totalsRange.Font.Bold = true;
            totalsRange.Font.Name = "Segoe UI";
            totalsRange.Font.Size = 12;
            totalsRange.Interior.Color = ColorTranslator.ToOle(Color.FromArgb(232, 242, 255));
            totalsRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            worksheet.Range[$"D{totalsRow}"].NumberFormat = "#,##0.00000000";

            int noteRow = totalsRow + 2;
            Excel.Range thanksRange = worksheet.Range[$"A{noteRow}", $"F{noteRow}"];
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
                detailsGrid.Columns["unit_price"].HeaderText = "سعر الوحدة";
                detailsGrid.Columns["unit_price"].DefaultCellStyle.Format = "N8";
            }

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

            invoiceForm.Controls.Add(bodyPanel);
            invoiceForm.Controls.Add(summaryPanel);
            invoiceForm.Controls.Add(headerPanel);
            invoiceForm.ShowDialog();
        }

        private void Sales_Load(object sender, EventArgs e)
        {
            StyleSalesForm();
            txtquantity.Text = "1"; // القيمة الافتراضية
            LoadCustomers();

            // ممكن تستدعي هنا أيضاً لو تحب
            LoadCategories();

            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand("SELECT value FROM Settings WHERE key_name='default_price'", conn);
            conn.Open();
            string rate = cmd.ExecuteScalar().ToString();
            conn.Close();

            txtdollar.Text = rate;
            //  _role = reader["role"].ToString().Trim().ToLower();

            // التحقق من صلاحية المستخدم
            if (_role.Trim().ToLower() == "admin" || _role.Trim().ToLower() == "manager")
            {
                txtdollar.ReadOnly = false;
                btndollar.Enabled = true;
                btndelete.Enabled = true;
                btnexcel.Enabled = true;
                btnmortaja3.Enabled = true;

            }
            else
            {
                txtdollar.ReadOnly = true;
                btndollar.Enabled = false;
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
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand("UPDATE Settings SET value=@rate WHERE key_name='default_price'", conn);
            cmd.Parameters.AddWithValue("@rate", txtdollar.Text);
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();

            MessageBox.Show("تم حفظ سعر الدولار بنجاح");
        }


        private void cmbCategory_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // نتأكد إن القيمة ID صحيحة
            if (cmbCategory.SelectedValue == null || cmbCategory.SelectedValue is DataRowView)
                return;

            int categoryId = Convert.ToInt32(cmbCategory.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    @"SELECT p.product_id, p.name, p.price_usd, p.price_lb, p.exchange_rate 
              FROM Products p
              WHERE p.category_id = @catId", conn);

                // ✅ أفضل من AddWithValue
                da.SelectCommand.Parameters.Add("@catId", SqlDbType.Int).Value = categoryId;

                DataTable dt = new DataTable();
                da.Fill(dt);

                pnlProducts.Controls.Clear();

                // ✅ إذا ما في منتجات
                if (dt.Rows.Count == 0)
                {
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

                        decimal priceUsd = productRow["price_usd"] == DBNull.Value ? 0 : Convert.ToDecimal(productRow["price_usd"]);
                        decimal priceLb = productRow["price_lb"] == DBNull.Value ? 0 : Convert.ToDecimal(productRow["price_lb"]);
                        decimal exchangeRate = productRow["exchange_rate"] == DBNull.Value ? 0 : Convert.ToDecimal(productRow["exchange_rate"]);

                        DateTime now = DateTime.Now;

                        decimal totalUsd = priceUsd * qty;

                        datagridsales.Rows.Add(
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

                        UpdateTotals();
                    };

                    pnlProducts.Controls.Add(btn);
                }
            }
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
                string query = @"SELECT product_id, name, price_usd, price_lb, exchange_rate
                     FROM Products
                     WHERE name = @keyword OR barcode = @keyword";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", keyword);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int productId = Convert.ToInt32(reader["product_id"]);
                    string productName = reader["name"].ToString();
                    decimal priceUsd = reader["price_usd"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price_usd"]);
                    decimal priceLb = reader["price_lb"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price_lb"]);
                    decimal exchangeRate = reader["exchange_rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["exchange_rate"]);
                    DateTime now = DateTime.Now;

                    decimal totalUsd = priceUsd * qty;

                    // ✅ إضافة الصف مع العمود product_id أولاً
                    datagridsales.Rows.Add(productId, productName, priceUsd, priceLb, qty, totalUsd, exchangeRate, now, "");

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

                e.Graphics.DrawString($"{product} - الكمية: {qty} - السعر: {price}", font, Brushes.Black, 20, y);
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
            ProcessSale(true);
        }

        private void ProcessSale(bool printReceipt)
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

            // ✅ Validate payment
            if (!decimal.TryParse(txtPaidAmount.Text, out decimal customerPaid))
            {
                MessageBox.Show("أدخل المبلغ المدفوع بشكل صحيح");
                return;
            }

            int userId = LoginForm.LoggedInUserId;

            if (!int.TryParse(cmbCustomer.SelectedValue?.ToString(), out int customerId))
            {
                MessageBox.Show("اختر زبون صحيح");
                return;
            }

            string customerName = cmbCustomer.Text;
            string paymentMethod = comboPaymentMethod.Text;

            if (comboPaymentMethod.SelectedIndex < 0 || string.IsNullOrWhiteSpace(paymentMethod))
            {
                MessageBox.Show("اختر طريقة الدفع أولاً");
                return;
            }

            // ✅ Balances
           decimal? balanceUsd = rbDollar.Checked ? (decimal?)(customerPaid - totalUsd) : null;
            decimal? balanceLb = rbLebanon.Checked ? (decimal?)(customerPaid - totalLb) : null;

            // ✅ Safe rounding
            decimal safeTotalUsd = Math.Round(totalUsd, SqlMoneyScale);
            decimal safeTotalLb = Math.Round(totalLb, SqlMoneyScale);
            decimal? safeBalanceUsd = balanceUsd.HasValue ? (decimal?)Math.Round(balanceUsd.Value, SqlMoneyScale) : null;
            decimal? safeBalanceLb = balanceLb.HasValue ? (decimal?)Math.Round(balanceLb.Value, SqlMoneyScale) : null;

            if (!rbDollar.Checked && !rbLebanon.Checked)
            {
                MessageBox.Show("اختر العملة قبل إتمام عملية البيع");
                return;
            }

            decimal selectedTotalAmount = rbDollar.Checked ? safeTotalUsd : safeTotalLb;
            decimal? selectedBalanceAmount = rbDollar.Checked ? safeBalanceUsd : safeBalanceLb;

            if (!ValidateSaleAmounts(selectedTotalAmount, selectedBalanceAmount, out string validationMessage))
            {
                MessageBox.Show(validationMessage);
                return;
            }

            int saleId;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
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
(sale_id, product_id, quantity, unit_price, name_product, customer_name, created_by)
VALUES (@sale_id, @product_id, @quantity, @unit_price, @name_product, @customer_name, @created_by)", conn, transaction);

                        cmdItem.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                        cmdItem.Parameters.Add("@product_id", SqlDbType.Int).Value = productId;
                        cmdItem.Parameters.Add("@quantity", SqlDbType.Int).Value = qty;

                        cmdItem.Parameters.Add(new SqlParameter("@unit_price", SqlDbType.Decimal)
                        {
                            Precision = SqlMoneyPrecision,
                            Scale = SqlMoneyScale,
                            Value = Math.Round(unitPrice, SqlMoneyScale)
                        });

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

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("خطأ: " + ex.Message);
                    return;
                }
            }

            MessageBox.Show("تمت العملية بنجاح - رقم الفاتورة: " + saleId);

            if (printReceipt)
            {
                PrintReceipt(saleId, customerName, paymentMethod);
            }

            clearinput();
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
            ProcessSale(false);
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

        private void button2_Click(object sender, EventArgs e)
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

                // أولاً: جيب المنتجات من جدول Sale_Items الصحيح
                string queryDetails = @"SELECT product_id, quantity 
                            FROM Sale_Items 
                            WHERE sale_id = @sale_id";

                SqlCommand cmdDetails = new SqlCommand(queryDetails, conn);
                cmdDetails.Parameters.AddWithValue("@sale_id", saleId);

                SqlDataReader reader = cmdDetails.ExecuteReader();

                List<(int productId, int qty)> returnedProducts = new List<(int, int)>();

                while (reader.Read())
                {
                    int productId = Convert.ToInt32(reader["product_id"]);
                    int qty = Convert.ToInt32(reader["quantity"]);
                    returnedProducts.Add((productId, qty));
                }
                reader.Close();

                // ثانياً: زيد الكمية في جدول Products
                foreach (var item in returnedProducts)
                {
                    string updateQuery = @"UPDATE Products 
                               SET stock_quantity = stock_quantity + @qty 
                               WHERE product_id = @product_id";   // ✅ استخدم stock_quantity بدل stock

                    SqlCommand cmdUpdate = new SqlCommand(updateQuery, conn);
                    cmdUpdate.Parameters.AddWithValue("@qty", item.qty);
                    cmdUpdate.Parameters.AddWithValue("@product_id", item.productId);
                    cmdUpdate.ExecuteNonQuery();

                    // ثالثاً: سجل العملية في جدول Returns مع الكمية والمنتج
                    string insertReturn = @"INSERT INTO Returns (sale_id, product_id, quantity, return_date) 
                                VALUES (@sale_id, @product_id, @quantity, GETDATE())";

                    SqlCommand cmdReturn = new SqlCommand(insertReturn, conn);
                    cmdReturn.Parameters.AddWithValue("@sale_id", saleId);
                    cmdReturn.Parameters.AddWithValue("@product_id", item.productId);
                    cmdReturn.Parameters.AddWithValue("@quantity", item.qty);
                    cmdReturn.ExecuteNonQuery();
                }

                // رابعاً: حدّث جدول Sales ليظهر أن الفاتورة مرتجعة
                string updateSale = "UPDATE Sales SET is_returned = 1, status = N'مرتجع' WHERE sale_id = @sale_id";
                SqlCommand cmdUpdateSale = new SqlCommand(updateSale, conn);
                cmdUpdateSale.Parameters.AddWithValue("@sale_id", saleId);
                cmdUpdateSale.ExecuteNonQuery();

                conn.Close();
            }

            MessageBox.Show("تم تسجيل المرتجع وإضافة المنتجات مرة أخرى للمخزون");

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


    
