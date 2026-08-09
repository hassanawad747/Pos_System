using Microsoft.EntityFrameworkCore;
using Pos_System.Data;
using Pos_System.Models;
using Pos_System.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class SupplierLedgerForm : Form
    {
        private readonly POSDbContext context;
        private readonly SupplierLedgerService ledgerService;
        private ComboBox supplierCombo;
        private Label balanceLabel;
        private NumericUpDown amountInput;
        private ComboBox methodCombo;
        private TextBox referenceText;
        private TextBox notesText;
        private DataGridView grid;

        public SupplierLedgerForm()
        {
            var options = new DbContextOptionsBuilder<POSDbContext>()
                .UseSqlServer(POS_System.Program.SettingsManager.ConnectionString)
                .Options;
            context = new POSDbContext(options);
            ledgerService = new SupplierLedgerService(context);
            BuildUi();
            Load += SupplierLedgerForm_Load;
            FormClosed += (s, e) => context.Dispose();
        }

        private void BuildUi()
        {
            Text = "Supplier Ledger";
            BackColor = Color.FromArgb(244, 247, 252);
            MinimumSize = new Size(980, 620);

            var top = new Panel { Dock = DockStyle.Top, Height = 105, BackColor = Color.White, Padding = new Padding(14) };
            var title = new Label { Text = "Supplier Ledger & Payments", AutoSize = true, Location = new Point(14, 12), Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
            supplierCombo = new ComboBox { Location = new Point(105, 55), Width = 270, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            supplierCombo.SelectedIndexChanged += (s, e) => RefreshLedger();
            balanceLabel = new Label { Text = "Balance: $0.00", AutoSize = true, Location = new Point(430, 58), Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28) };
            top.Controls.AddRange(new Control[] { title, L("Supplier", 14, 60), supplierCombo, balanceLabel });

            var payment = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(14) };
            amountInput = new NumericUpDown { Location = new Point(80, 18), Width = 120, DecimalPlaces = 2, Maximum = 1000000000m, ThousandsSeparator = true };
            methodCombo = new ComboBox { Location = new Point(285, 18), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList };
            methodCombo.Items.AddRange(new object[] { "CASH", "CARD", "BANK", "OTHER" }); methodCombo.SelectedIndex = 0;
            referenceText = new TextBox { Location = new Point(505, 18), Width = 155 };
            notesText = new TextBox { Location = new Point(80, 56), Width = 580 };
            var pay = B("Record Payment", 700, 26, 165, Color.FromArgb(22, 163, 74));
            pay.Click += RecordPayment_Click;
            payment.Controls.AddRange(new Control[] { L("Amount", 14, 22), amountInput, L("Method", 225, 22), methodCombo, L("Reference", 430, 22), referenceText, L("Notes", 14, 60), notesText, pay });

            grid = new DataGridView {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, ReadOnly = true,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            Controls.Add(grid); Controls.Add(payment); Controls.Add(top);
        }

        private void SupplierLedgerForm_Load(object sender, EventArgs e)
        {
            try
            {
                supplierCombo.DataSource = context.Suppliers.OrderBy(x => x.Name).ToList();
                supplierCombo.DisplayMember = "Name";
                supplierCombo.ValueMember = "SupplierId";
                RefreshLedger();
            }
            catch (Exception ex) { MessageBox.Show("Could not load supplier ledger: " + ex.Message); }
        }

        private int SelectedSupplierId
        {
            get
            {
                if (supplierCombo.SelectedValue is int id) return id;
                var supplier = supplierCombo.SelectedItem as Supplier;
                return supplier == null ? 0 : supplier.SupplierId;
            }
        }

        private void RefreshLedger()
        {
            if (SelectedSupplierId <= 0) return;
            try
            {
                decimal balance = ledgerService.GetBalance(SelectedSupplierId, "USD");
                balanceLabel.Text = "You owe supplier: $" + balance.ToString("N2");
                balanceLabel.ForeColor = balance > 0 ? Color.FromArgb(185, 28, 28) : Color.FromArgb(22, 101, 52);
                grid.DataSource = ledgerService.GetTransactions(SelectedSupplierId, "USD")
                    .Select(x => new {
                        Date = x.CreatedAt.ToLocalTime(),
                        Type = x.TransactionType,
                        Reference = x.ReferenceType + (x.ReferenceId.HasValue ? " #" + x.ReferenceId.Value : string.Empty),
                        Debit = x.Debit,
                        Credit = x.Credit,
                        Balance = x.BalanceAfter,
                        x.Description
                    }).ToList();
            }
            catch (Exception ex) { MessageBox.Show("Could not refresh ledger: " + ex.Message); }
        }

        private void RecordPayment_Click(object sender, EventArgs e)
        {
            try
            {
                if (AppSession.UserId <= 0) throw new InvalidOperationException("No logged-in user was found.");
                ledgerService.RecordPayment(SelectedSupplierId, amountInput.Value, Convert.ToString(methodCombo.SelectedItem), referenceText.Text, notesText.Text, AppSession.UserId, "USD");
                MessageBox.Show("Supplier payment saved successfully.", "Supplier Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                amountInput.Value = 0; referenceText.Clear(); notesText.Clear(); RefreshLedger();
            }
            catch (Exception ex) { MessageBox.Show("Payment could not be saved: " + ex.Message, "Supplier Payment", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private static Label L(string text, int x, int y) => new Label { Text = text, AutoSize = true, Location = new Point(x, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        private static Button B(string text, int x, int y, int width, Color color) { var b = new Button { Text = text, Location = new Point(x, y), Width = width, Height = 38, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }; b.FlatAppearance.BorderSize = 0; return b; }
    }
}