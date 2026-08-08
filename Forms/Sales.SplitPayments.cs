using Pos_System.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class Sales
    {
        private Button btnSplitPayment;
        private bool splitPaymentButtonAdded;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            AddSplitPaymentButton();
        }

        private void AddSplitPaymentButton()
        {
            if (splitPaymentButtonAdded || btnSaveSale == null || btnSaveSale.Parent == null)
                return;

            btnSplitPayment = new Button
            {
                Name = "btnSplitPayment",
                Text = "Split Payment",
                Size = btnSaveSale.Size,
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Anchor = btnSaveSale.Anchor
            };
            btnSplitPayment.FlatAppearance.BorderSize = 0;

            Control host = btnSaveSale.Parent;
            int proposedLeft = btnSaveSale.Right + 8;
            if (proposedLeft + btnSplitPayment.Width <= host.ClientSize.Width - 8)
                btnSplitPayment.Location = new Point(proposedLeft, btnSaveSale.Top);
            else
                btnSplitPayment.Location = new Point(Math.Max(8, btnSaveSale.Left - btnSplitPayment.Width - 8), btnSaveSale.Top);

            btnSplitPayment.Click += BtnSplitPayment_Click;
            host.Controls.Add(btnSplitPayment);
            btnSplitPayment.BringToFront();
            splitPaymentButtonAdded = true;
        }

        private void BtnSplitPayment_Click(object sender, EventArgs e)
        {
            ProcessSplitPaymentSale();
        }

        private void ProcessSplitPaymentSale()
        {
            try
            {
                if (datagridsales == null || datagridsales.Rows.Cast<DataGridViewRow>().All(r => r.IsNewRow))
                {
                    MessageBox.Show("Add products before checkout.", "Split Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!rbDollar.Checked && !rbLebanon.Checked)
                {
                    MessageBox.Show("Choose USD or LBP before checkout.", "Split Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(cmbCustomer.SelectedValue?.ToString(), out int customerId))
                {
                    MessageBox.Show("Choose a valid customer.", "Split Payment", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int userId = LoginForm.LoggedInUserId;
                if (userId <= 0)
                    throw new InvalidOperationException("No logged-in user was found.");

                bool useUsd = rbDollar.Checked;
                string currency = useUsd ? "USD" : "LBP";
                decimal exchangeRate = POS_System.Program.SettingsManager.GetExchangeRate();
                decimal total = 0m;
                int totalQuantity = 0;

                foreach (DataGridViewRow row in datagridsales.Rows)
                {
                    if (row.IsNewRow) continue;
                    int qty = GetIntCellValue(row, "quantity");
                    if (qty <= 0)
                        throw new InvalidOperationException("All quantities must be greater than zero.");
                    decimal price = GetDecimalCellValue(row, useUsd ? "price_usd" : "price_lb");
                    total += price * qty;
                    totalQuantity += qty;
                }

                total = Math.Round(total, SqlMoneyScale);
                if (total <= 0)
                    throw new InvalidOperationException("Sale total must be greater than zero.");

                using (var dialog = new SplitPaymentDialog(total, currency))
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    List<SplitPaymentEntry> payments = dialog.Payments;
                    decimal paidTotal = Math.Round(payments.Sum(x => x.Amount), SqlMoneyScale);
                    decimal remaining = Math.Round(total - paidTotal, SqlMoneyScale);

                    if (remaining < 0)
                        throw new InvalidOperationException("Payments cannot exceed the sale total.");

                    int saleId;
                    string customerName = cmbCustomer.Text;

                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        using (var tx = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                        {
                            try
                            {
                                int? openCashSessionId = GetOpenCashSessionId(conn, tx, userId);
                                if (payments.Any(x => x.Method == "CASH") && !openCashSessionId.HasValue)
                                    throw new InvalidOperationException("Open a Cash Shift before using CASH in a split payment.");

                                saleId = InsertSplitSaleHeader(
                                    conn,
                                    tx,
                                    userId,
                                    customerId,
                                    customerName,
                                    total,
                                    totalQuantity,
                                    useUsd ? (decimal?)remaining : null,
                                    useUsd ? null : (decimal?)remaining);

                                InsertSplitSaleItems(conn, tx, saleId, customerName, useUsd);
                                InsertSplitSalePayments(conn, tx, saleId, payments, currency, exchangeRate, openCashSessionId);

                                UpdateCustomerBalance(
                                    conn,
                                    tx,
                                    customerId,
                                    useUsd ? (decimal?)remaining : null,
                                    useUsd ? null : (decimal?)remaining);

                                tx.Commit();
                            }
                            catch
                            {
                                tx.Rollback();
                                throw;
                            }
                        }
                    }

                    AuditService.Log("Sales", "Create", saleId.ToString(), "Created split-payment sale invoice " + saleId);

                    string result = "Split payment sale completed.\n\n" +
                                    "Invoice: " + saleId + "\n" +
                                    "Total: " + total.ToString("N2") + " " + currency + "\n" +
                                    "Paid: " + paidTotal.ToString("N2") + " " + currency + "\n" +
                                    "Remaining: " + remaining.ToString("N2") + " " + currency;
                    MessageBox.Show(result, "Split Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    clearinput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Split payment sale could not be completed:\n" + ex.Message,
                    "Split Payment", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int InsertSplitSaleHeader(
            SqlConnection conn,
            SqlTransaction tx,
            int userId,
            int customerId,
            string customerName,
            decimal total,
            int totalQuantity,
            decimal? balanceUsd,
            decimal? balanceLb)
        {
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Sales
(user_id, customer_id, total_amount, payment_method, created_by, customer_name, balance_usd, balance_lb, quantity)
VALUES
(@user_id, @customer_id, @total_amount, N'SPLIT', @created_by, @customer_name, @balance_usd, @balance_lb, @quantity);
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
            {
                cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
                cmd.Parameters.Add("@customer_id", SqlDbType.Int).Value = customerId;
                AddMoneyParameter(cmd, "@total_amount", total);
                cmd.Parameters.Add("@created_by", SqlDbType.NVarChar, 50).Value = LoginForm.LoggedInUsername;
                cmd.Parameters.Add("@customer_name", SqlDbType.NVarChar, 100).Value = customerName;
                AddNullableMoneyParameter(cmd, "@balance_usd", balanceUsd);
                AddNullableMoneyParameter(cmd, "@balance_lb", balanceLb);
                cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = totalQuantity;
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void InsertSplitSaleItems(SqlConnection conn, SqlTransaction tx, int saleId, string customerName, bool useUsd)
        {
            foreach (DataGridViewRow row in datagridsales.Rows)
            {
                if (row.IsNewRow) continue;

                int productId = GetIntCellValue(row, "product_id");
                int qty = GetIntCellValue(row, "quantity");
                string productName = row.Cells["product_name"].Value?.ToString() ?? string.Empty;
                decimal unitPrice = GetDecimalCellValue(row, useUsd ? "price_usd" : "price_lb");
                decimal originalUnitPrice = GetDecimalCellValue(row, useUsd ? "original_price_usd" : "original_price_lb");
                if (originalUnitPrice <= 0m) originalUnitPrice = unitPrice;
                decimal discountAmount = Math.Max(0m, (originalUnitPrice - unitPrice) * qty);
                decimal discountValue = originalUnitPrice > 0m
                    ? (discountAmount / Math.Max(1, qty)) * 100m / originalUnitPrice
                    : 0m;
                string discountType = discountAmount > 0m ? "SplitCheckout" : "None";

                int currentStock;
                using (var stockCmd = new SqlCommand(
                    "SELECT stock_quantity FROM dbo.Products WITH (UPDLOCK, ROWLOCK) WHERE product_id=@id", conn, tx))
                {
                    stockCmd.Parameters.Add("@id", SqlDbType.Int).Value = productId;
                    object stockValue = stockCmd.ExecuteScalar();
                    if (stockValue == null || stockValue == DBNull.Value)
                        throw new InvalidOperationException("Product not found: " + productName);
                    currentStock = Convert.ToInt32(stockValue);
                }

                if (qty > currentStock)
                    throw new InvalidOperationException("Insufficient stock for " + productName + ". Available: " + currentStock);

                using (var itemCmd = new SqlCommand(@"
INSERT INTO dbo.Sale_Items
(sale_id, product_id, quantity, unit_price, original_unit_price, discount_amount, discount_type, discount_value, discount_by, name_product, customer_name, created_by)
VALUES
(@sale_id, @product_id, @quantity, @unit_price, @original_unit_price, @discount_amount, @discount_type, @discount_value, @discount_by, @name_product, @customer_name, @created_by);", conn, tx))
                {
                    itemCmd.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                    itemCmd.Parameters.Add("@product_id", SqlDbType.Int).Value = productId;
                    itemCmd.Parameters.Add("@quantity", SqlDbType.Int).Value = qty;
                    AddMoneyParameter(itemCmd, "@unit_price", unitPrice);
                    AddMoneyParameter(itemCmd, "@original_unit_price", originalUnitPrice);
                    AddMoneyParameter(itemCmd, "@discount_amount", discountAmount);
                    itemCmd.Parameters.Add("@discount_type", SqlDbType.NVarChar, 30).Value = discountType;
                    var discountParam = itemCmd.Parameters.Add("@discount_value", SqlDbType.Decimal);
                    discountParam.Precision = 18;
                    discountParam.Scale = 4;
                    discountParam.Value = Math.Round(discountValue, 4);
                    itemCmd.Parameters.Add("@discount_by", SqlDbType.NVarChar, 100).Value = discountAmount > 0m ? LoginForm.LoggedInUsername : (object)DBNull.Value;
                    itemCmd.Parameters.Add("@name_product", SqlDbType.NVarChar, 100).Value = productName;
                    itemCmd.Parameters.Add("@customer_name", SqlDbType.NVarChar, 100).Value = customerName;
                    itemCmd.Parameters.Add("@created_by", SqlDbType.NVarChar, 50).Value = LoginForm.LoggedInUsername;
                    itemCmd.ExecuteNonQuery();
                }

                using (var stockUpdate = new SqlCommand(@"
UPDATE dbo.Products
SET stock_quantity = stock_quantity - @qty
WHERE product_id = @id AND stock_quantity >= @qty;", conn, tx))
                {
                    stockUpdate.Parameters.Add("@qty", SqlDbType.Int).Value = qty;
                    stockUpdate.Parameters.Add("@id", SqlDbType.Int).Value = productId;
                    if (stockUpdate.ExecuteNonQuery() != 1)
                        throw new InvalidOperationException("Stock changed while completing sale for " + productName + ". Try again.");
                }
            }
        }

        private void InsertSplitSalePayments(
            SqlConnection conn,
            SqlTransaction tx,
            int saleId,
            List<SplitPaymentEntry> payments,
            string currency,
            decimal exchangeRate,
            int? openCashSessionId)
        {
            foreach (SplitPaymentEntry payment in payments)
            {
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.SalePayments
(sale_id, payment_method, amount, currency, exchange_rate, reference_number, cash_session_id, is_legacy_auto, created_at)
VALUES
(@sale_id, @method, @amount, @currency, @exchange_rate, @reference, @cash_session_id, 0, SYSUTCDATETIME());", conn, tx))
                {
                    cmd.Parameters.Add("@sale_id", SqlDbType.Int).Value = saleId;
                    cmd.Parameters.Add("@method", SqlDbType.NVarChar, 50).Value = payment.Method;
                    AddMoneyParameter(cmd, "@amount", payment.Amount);
                    cmd.Parameters.Add("@currency", SqlDbType.NVarChar, 10).Value = currency;
                    AddNullableMoneyParameter(cmd, "@exchange_rate", exchangeRate > 0m ? (decimal?)exchangeRate : null);
                    cmd.Parameters.Add("@reference", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(payment.Reference) ? (object)DBNull.Value : payment.Reference.Trim();
                    cmd.Parameters.Add("@cash_session_id", SqlDbType.Int).Value = payment.Method == "CASH" && openCashSessionId.HasValue
                        ? (object)openCashSessionId.Value
                        : DBNull.Value;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private int? GetOpenCashSessionId(SqlConnection conn, SqlTransaction tx, int userId)
        {
            using (var cmd = new SqlCommand(@"
SELECT TOP (1) cash_session_id
FROM dbo.CashSessions
WHERE user_id=@user_id AND status=N'OPEN'
ORDER BY opened_at DESC;", conn, tx))
            {
                cmd.Parameters.Add("@user_id", SqlDbType.Int).Value = userId;
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? (int?)null : Convert.ToInt32(value);
            }
        }

        private static void AddMoneyParameter(SqlCommand cmd, string name, decimal value)
        {
            var parameter = cmd.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = SqlMoneyPrecision;
            parameter.Scale = SqlMoneyScale;
            parameter.Value = Math.Round(value, SqlMoneyScale);
        }

        private static void AddNullableMoneyParameter(SqlCommand cmd, string name, decimal? value)
        {
            var parameter = cmd.Parameters.Add(name, SqlDbType.Decimal);
            parameter.Precision = SqlMoneyPrecision;
            parameter.Scale = SqlMoneyScale;
            parameter.Value = value.HasValue ? (object)Math.Round(value.Value, SqlMoneyScale) : DBNull.Value;
        }

        private sealed class SplitPaymentEntry
        {
            public string Method { get; set; }
            public decimal Amount { get; set; }
            public string Reference { get; set; }
        }

        private sealed class SplitPaymentDialog : Form
        {
            private readonly decimal saleTotal;
            private readonly string currency;
            private readonly DataGridView grid;
            private readonly Label paidLabel;
            private readonly Label remainingLabel;

            public List<SplitPaymentEntry> Payments { get; private set; } = new List<SplitPaymentEntry>();

            public SplitPaymentDialog(decimal saleTotal, string currency)
            {
                this.saleTotal = saleTotal;
                this.currency = currency;

                Text = "Split Payment";
                StartPosition = FormStartPosition.CenterParent;
                Size = new Size(760, 500);
                MinimumSize = new Size(680, 430);
                BackColor = Color.FromArgb(244, 247, 252);
                Font = new Font("Segoe UI", 10F);

                var header = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = Color.FromArgb(15, 23, 42) };
                header.Controls.Add(new Label
                {
                    Text = "Split Payment Checkout",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(18, 13)
                });
                header.Controls.Add(new Label
                {
                    Text = "Sale total: " + saleTotal.ToString("N2") + " " + currency,
                    ForeColor = Color.FromArgb(203, 213, 225),
                    AutoSize = true,
                    Location = new Point(20, 47)
                });

                grid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    BackgroundColor = Color.White,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = true,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect
                };

                var methodColumn = new DataGridViewComboBoxColumn
                {
                    Name = "Method",
                    HeaderText = "Method",
                    FlatStyle = FlatStyle.Flat
                };
                methodColumn.Items.AddRange("CASH", "CARD", "BANK", "OTHER");
                grid.Columns.Add(methodColumn);
                grid.Columns.Add("Amount", "Amount " + currency);
                grid.Columns.Add("Reference", "Reference / Note");
                grid.CellValueChanged += (s, e) => UpdateSummary();
                grid.RowsRemoved += (s, e) => UpdateSummary();
                grid.CurrentCellDirtyStateChanged += (s, e) =>
                {
                    if (grid.IsCurrentCellDirty)
                        grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                };

                var bottom = new Panel { Dock = DockStyle.Bottom, Height = 118, BackColor = Color.White, Padding = new Padding(12) };
                var add = CreateButton("Add Payment", 12, 12, Color.FromArgb(37, 99, 235));
                var exactCash = CreateButton("Full Cash", 150, 12, Color.FromArgb(22, 163, 74));
                var remove = CreateButton("Remove", 288, 12, Color.FromArgb(220, 38, 38));
                var complete = CreateButton("Complete Sale", 560, 62, Color.FromArgb(79, 70, 229));
                complete.Width = 160;

                paidLabel = new Label { AutoSize = true, Location = new Point(12, 68), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
                remainingLabel = new Label { AutoSize = true, Location = new Point(255, 68), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };

                add.Click += (s, e) => AddPaymentRow("CASH", 0m, null);
                exactCash.Click += (s, e) =>
                {
                    grid.Rows.Clear();
                    AddPaymentRow("CASH", saleTotal, null);
                };
                remove.Click += (s, e) =>
                {
                    if (grid.CurrentRow != null && !grid.CurrentRow.IsNewRow)
                        grid.Rows.Remove(grid.CurrentRow);
                };
                complete.Click += Complete_Click;

                bottom.Controls.AddRange(new Control[] { add, exactCash, remove, paidLabel, remainingLabel, complete });
                Controls.Add(grid);
                Controls.Add(bottom);
                Controls.Add(header);

                AddPaymentRow("CASH", 0m, null);
                UpdateSummary();
            }

            private void AddPaymentRow(string method, decimal amount, string reference)
            {
                int index = grid.Rows.Add();
                grid.Rows[index].Cells["Method"].Value = method;
                grid.Rows[index].Cells["Amount"].Value = amount == 0m ? string.Empty : amount.ToString("0.##", CultureInfo.CurrentCulture);
                grid.Rows[index].Cells["Reference"].Value = reference ?? string.Empty;
                UpdateSummary();
            }

            private void Complete_Click(object sender, EventArgs e)
            {
                var result = new List<SplitPaymentEntry>();
                decimal paid = 0m;

                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;

                    string method = Convert.ToString(row.Cells["Method"].Value)?.Trim().ToUpperInvariant();
                    string amountText = Convert.ToString(row.Cells["Amount"].Value)?.Trim();
                    string reference = Convert.ToString(row.Cells["Reference"].Value)?.Trim();

                    if (string.IsNullOrWhiteSpace(method) && string.IsNullOrWhiteSpace(amountText))
                        continue;
                    if (string.IsNullOrWhiteSpace(method))
                    {
                        MessageBox.Show("Choose a payment method for every payment row.");
                        return;
                    }
                    if (!TryParseAmount(amountText, out decimal amount) || amount <= 0m)
                    {
                        MessageBox.Show("Every payment amount must be greater than zero.");
                        return;
                    }

                    paid += amount;
                    result.Add(new SplitPaymentEntry { Method = method, Amount = amount, Reference = reference });
                }

                paid = Math.Round(paid, SqlMoneyScale);
                if (result.Count == 0)
                {
                    MessageBox.Show("Add at least one payment.");
                    return;
                }
                if (paid > saleTotal)
                {
                    MessageBox.Show("Paid amount cannot exceed the sale total.");
                    return;
                }

                Payments = result;
                DialogResult = DialogResult.OK;
                Close();
            }

            private void UpdateSummary()
            {
                decimal paid = 0m;
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (row.IsNewRow) continue;
                    if (TryParseAmount(Convert.ToString(row.Cells["Amount"].Value), out decimal amount) && amount > 0m)
                        paid += amount;
                }
                decimal remaining = saleTotal - paid;
                paidLabel.Text = "Paid: " + paid.ToString("N2") + " " + currency;
                remainingLabel.Text = "Remaining: " + remaining.ToString("N2") + " " + currency;
                remainingLabel.ForeColor = remaining < 0m ? Color.FromArgb(185, 28, 28) : Color.FromArgb(180, 83, 9);
            }

            private static bool TryParseAmount(string text, out decimal value)
            {
                return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
                       decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
            }

            private static Button CreateButton(string text, int x, int y, Color color)
            {
                var button = new Button
                {
                    Text = text,
                    Location = new Point(x, y),
                    Width = 128,
                    Height = 38,
                    BackColor = color,
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                };
                button.FlatAppearance.BorderSize = 0;
                return button;
            }
        }
    }
}
