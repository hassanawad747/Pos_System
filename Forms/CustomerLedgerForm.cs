using Pos_System.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class CustomerLedgerForm : Form
    {
        private readonly string connectionString = POS_System.Program.SettingsManager.ConnectionString;
        private readonly CustomerLedgerService ledgerService;
        private ComboBox customerCombo;
        private ComboBox currencyCombo;
        private Label balanceLabel;
        private NumericUpDown amountInput;
        private ComboBox methodCombo;
        private TextBox referenceText;
        private TextBox notesText;
        private DataGridView grid;

        public CustomerLedgerForm()
        {
            ledgerService = new CustomerLedgerService(connectionString);
            BuildUi();
            Load += CustomerLedgerForm_Load;
        }

        private string SelectedCurrency => Convert.ToString(currencyCombo.SelectedItem) == "LBP" ? "LBP" : "USD";

        private void BuildUi()
        {
            Text = "Customer Ledger";
            BackColor = Color.FromArgb(244, 247, 252);
            MinimumSize = new Size(1040, 620);

            var top = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.White, Padding = new Padding(14) };
            var title = new Label { Text = "Customer Ledger & Payments", AutoSize = true, Location = new Point(14, 12), Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
            customerCombo = new ComboBox { Location = new Point(105, 55), Width = 270, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            customerCombo.SelectedIndexChanged += (s, e) => RefreshLedger();
            currencyCombo = new ComboBox { Location = new Point(445, 55), Width = 90, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            currencyCombo.Items.AddRange(new object[] { "USD", "LBP" });
            currencyCombo.SelectedIndex = 0;
            currencyCombo.SelectedIndexChanged += (s, e) => RefreshLedger();
            balanceLabel = new Label { Text = "Balance: $0.00", AutoSize = true, Location = new Point(585, 58), Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28) };
            top.Controls.AddRange(new Control[] { L("Customer", 14, 60), customerCombo, L("Currency", 385, 60), currencyCombo, balanceLabel, title });

            var payment = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(14) };
            amountInput = new NumericUpDown { Location = new Point(80, 18), Width = 140, DecimalPlaces = 2, Maximum = 9999999999999999m, ThousandsSeparator = true };
            methodCombo = new ComboBox { Location = new Point(305, 18), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            methodCombo.Items.AddRange(new object[] { "CASH", "CARD", "BANK", "OTHER" });
            methodCombo.SelectedIndex = 0;
            referenceText = new TextBox { Location = new Point(525, 18), Width = 155 };
            notesText = new TextBox { Location = new Point(80, 56), Width = 600 };
            var pay = B("Record Payment", 720, 26, 165, Color.FromArgb(22, 163, 74));
            pay.Click += RecordPayment_Click;
            payment.Controls.AddRange(new Control[] { L("Amount", 14, 22), amountInput, L("Method", 245, 22), methodCombo, L("Reference", 450, 22), referenceText, L("Notes", 14, 60), notesText, pay });

            grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            Controls.Add(grid);
            Controls.Add(payment);
            Controls.Add(top);
        }

        private void CustomerLedgerForm_Load(object sender, EventArgs e)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var adapter = new SqlDataAdapter("SELECT customer_id, name FROM dbo.Customers ORDER BY name", conn))
                {
                    var table = new DataTable();
                    adapter.Fill(table);
                    customerCombo.DataSource = table;
                    customerCombo.DisplayMember = "name";
                    customerCombo.ValueMember = "customer_id";
                }
                RefreshLedger();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load customer ledger: " + ex.Message, "Customer Ledger", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int SelectedCustomerId
        {
            get
            {
                if (customerCombo.SelectedValue == null || customerCombo.SelectedValue == DBNull.Value) return 0;
                return Convert.ToInt32(customerCombo.SelectedValue);
            }
        }

        private void RefreshLedger()
        {
            if (SelectedCustomerId <= 0 || currencyCombo.SelectedIndex < 0) return;
            try
            {
                string currency = SelectedCurrency;
                decimal balance = ledgerService.GetBalance(SelectedCustomerId, currency);
                string symbol = currency == "USD" ? "$" : "LBP ";
                balanceLabel.Text = "Customer owes: " + symbol + balance.ToString("N2");
                balanceLabel.ForeColor = balance > 0 ? Color.FromArgb(185, 28, 28) : Color.FromArgb(22, 101, 52);
                grid.DataSource = ledgerService.GetTransactions(SelectedCustomerId, currency)
                    .Select(x => new
                    {
                        Date = x.CreatedAt.ToLocalTime(),
                        Type = x.TransactionType,
                        Reference = x.ReferenceType + (x.ReferenceId.HasValue ? " #" + x.ReferenceId.Value : string.Empty),
                        Debit = x.Debit,
                        Credit = x.Credit,
                        Balance = x.BalanceAfter,
                        Currency = x.Currency,
                        x.Description
                    }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not refresh customer ledger: " + ex.Message, "Customer Ledger", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RecordPayment_Click(object sender, EventArgs e)
        {
            try
            {
                if (AppSession.UserId <= 0) throw new InvalidOperationException("No logged-in user was found.");
                ledgerService.RecordPayment(
                    SelectedCustomerId,
                    amountInput.Value,
                    Convert.ToString(methodCombo.SelectedItem),
                    referenceText.Text,
                    notesText.Text,
                    AppSession.UserId,
                    SelectedCurrency);

                MessageBox.Show("Customer payment saved successfully.", "Customer Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                amountInput.Value = 0;
                referenceText.Clear();
                notesText.Clear();
                RefreshLedger();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Payment could not be saved: " + ex.Message, "Customer Payment", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static Label L(string text, int x, int y)
        {
            return new Label { Text = text, AutoSize = true, Location = new Point(x, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        }

        private static Button B(string text, int x, int y, int width, Color color)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Width = width,
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
