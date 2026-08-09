using Microsoft.EntityFrameworkCore;
using Pos_System.Data;
using Pos_System.Models;
using Pos_System.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class PurchaseForm : Form
    {
        private readonly POSDbContext context;
        private readonly PurchaseService purchaseService;
        private readonly BindingList<PurchaseRow> rows = new BindingList<PurchaseRow>();
        private ComboBox supplierCombo;
        private ComboBox productCombo;
        private NumericUpDown quantityInput;
        private NumericUpDown costInput;
        private NumericUpDown discountInput;
        private NumericUpDown taxInput;
        private NumericUpDown paidInput;
        private ComboBox paymentMethodCombo;
        private TextBox notesText;
        private DataGridView grid;
        private Label subtotalLabel;
        private Label totalLabel;
        private Label remainingLabel;

        public PurchaseForm()
        {
            var options = new DbContextOptionsBuilder<POSDbContext>()
                .UseSqlServer(POS_System.Program.SettingsManager.ConnectionString)
                .Options;
            context = new POSDbContext(options);
            purchaseService = new PurchaseService(context);
            BuildUi();
            Load += PurchaseForm_Load;
            FormClosed += (s, e) => context.Dispose();
        }

        private void BuildUi()
        {
            Text = "Purchases";
            BackColor = Color.FromArgb(244, 247, 252);
            MinimumSize = new Size(1050, 650);

            var header = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White, Padding = new Padding(16) };
            var title = new Label { Text = "Purchase Invoice", AutoSize = true, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(16, 18) };
            supplierCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(190, 20), Width = 280, Font = new Font("Segoe UI", 10F) };
            header.Controls.Add(title);
            header.Controls.Add(new Label { Text = "Supplier", AutoSize = true, Location = new Point(130, 24), Font = new Font("Segoe UI", 9F, FontStyle.Bold) });
            header.Controls.Add(supplierCombo);

            var itemPanel = new Panel { Dock = DockStyle.Top, Height = 90, BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(14) };
            productCombo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(80, 20), Width = 250 };
            quantityInput = Number(350, 20, 80, 0);
            quantityInput.Minimum = 1; quantityInput.Value = 1;
            costInput = Number(505, 20, 105, 2);
            discountInput = Number(690, 20, 90, 2);
            taxInput = Number(845, 20, 90, 2);
            var addButton = Button("Add Item", 950, 18, 90, Color.FromArgb(37, 99, 235));
            addButton.Click += AddItem_Click;
            itemPanel.Controls.AddRange(new Control[] {
                Label("Product", 14, 24), productCombo,
                Label("Qty", 315, 24), quantityInput,
                Label("Unit Cost", 440, 24), costInput,
                Label("Discount", 620, 24), discountInput,
                Label("Tax", 805, 24), taxInput, addButton
            });

            grid = new DataGridView {
                Dock = DockStyle.Fill, BackgroundColor = Color.White, AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, AllowUserToAddRows = false,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, DataSource = rows
            };
            grid.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) { rows.RemoveAt(e.RowIndex); RefreshTotals(); } };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 155, BackColor = Color.White, Padding = new Padding(14) };
            paidInput = Number(100, 18, 120, 2);
            paymentMethodCombo = new ComboBox { Location = new Point(330, 18), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            paymentMethodCombo.Items.AddRange(new object[] { "CASH", "CARD", "BANK", "OTHER" });
            paymentMethodCombo.SelectedIndex = 0;
            notesText = new TextBox { Location = new Point(100, 58), Width = 380, Height = 55, Multiline = true };
            subtotalLabel = Summary("Subtotal: $0.00", 560, 15);
            totalLabel = Summary("Total: $0.00", 560, 45);
            remainingLabel = Summary("Remaining: $0.00", 560, 75);
            var save = Button("Save Purchase", 820, 30, 180, Color.FromArgb(22, 163, 74));
            save.Height = 55;
            save.Click += SavePurchase_Click;
            paidInput.ValueChanged += (s, e) => RefreshTotals();
            bottom.Controls.AddRange(new Control[] {
                Label("Paid USD", 20, 22), paidInput, Label("Method", 265, 22), paymentMethodCombo,
                Label("Notes", 20, 62), notesText, subtotalLabel, totalLabel, remainingLabel, save
            });

            Controls.Add(grid); Controls.Add(bottom); Controls.Add(itemPanel); Controls.Add(header);
        }

        private void PurchaseForm_Load(object sender, EventArgs e)
        {
            try
            {
                supplierCombo.DataSource = context.Suppliers.OrderBy(x => x.Name).ToList();
                supplierCombo.DisplayMember = "Name"; supplierCombo.ValueMember = "SupplierId";
                productCombo.DataSource = context.Products.OrderBy(x => x.Name).ToList();
                productCombo.DisplayMember = "Name"; productCombo.ValueMember = "ProductId";
            }
            catch (Exception ex) { MessageBox.Show("Could not load purchase data: " + ex.Message); }
        }

        private void AddItem_Click(object sender, EventArgs e)
        {
            var product = productCombo.SelectedItem as Product;
            if (product == null) return;
            int quantity = (int)quantityInput.Value;
            decimal cost = costInput.Value;
            decimal discount = discountInput.Value;
            decimal tax = taxInput.Value;
            decimal lineTotal = (quantity * cost) - discount + tax;
            if (discount > quantity * cost) { MessageBox.Show("Discount cannot exceed the item value."); return; }

            rows.Add(new PurchaseRow { ProductId = product.ProductId, Product = product.Name, Quantity = quantity, UnitCost = cost, Discount = discount, Tax = tax, LineTotal = lineTotal });
            RefreshTotals();
        }

        private void SavePurchase_Click(object sender, EventArgs e)
        {
            try
            {
                if (AppSession.UserId <= 0) throw new InvalidOperationException("No logged-in user was found.");
                if (!(supplierCombo.SelectedValue is int supplierId) || supplierId <= 0) throw new InvalidOperationException("Select a supplier.");
                if (rows.Count == 0) throw new InvalidOperationException("Add at least one product.");

                var purchase = new Purchase {
                    SupplierId = supplierId, UserId = AppSession.UserId, PurchaseDate = DateTime.UtcNow,
                    PaidAmount = paidInput.Value, PaymentMethod = Convert.ToString(paymentMethodCombo.SelectedItem), Notes = notesText.Text.Trim(),
                    Currency="USD",ExchangeRate=POS_System.Program.SettingsManager.GetExchangeRate(),OperationKey=Guid.NewGuid(),
                    PurchaseItems = rows.Select(x => new PurchaseItem {
                        ProductId = x.ProductId, Quantity = x.Quantity, UnitCost = x.UnitCost,
                        DiscountAmount = x.Discount, TaxAmount = x.Tax, LineTotal = x.LineTotal
                    }).ToList()
                };

                purchaseService.CreatePurchase(purchase);
                MessageBox.Show("Purchase saved successfully.\nInvoice: " + purchase.InvoiceNumber + "\nRemaining supplier balance added: $" + purchase.RemainingAmount.ToString("N2"), "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Information);
                rows.Clear(); paidInput.Value = 0; notesText.Clear(); RefreshTotals();
            }
            catch (Exception ex) { MessageBox.Show("Purchase could not be saved: " + ex.Message, "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void RefreshTotals()
        {
            decimal subtotal = rows.Sum(x => x.Quantity * x.UnitCost);
            decimal total = rows.Sum(x => x.LineTotal);
            decimal remaining = Math.Max(0m, total - paidInput.Value);
            subtotalLabel.Text = "Subtotal: $" + subtotal.ToString("N2");
            totalLabel.Text = "Total: $" + total.ToString("N2");
            remainingLabel.Text = "Remaining: $" + remaining.ToString("N2");
        }

        private static NumericUpDown Number(int x, int y, int width, int decimals) => new NumericUpDown { Location = new Point(x, y), Width = width, DecimalPlaces = decimals, Maximum = 1000000000m, ThousandsSeparator = true };
        private static Label Label(string text, int x, int y) => new Label { Text = text, AutoSize = true, Location = new Point(x, y), Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
        private static Label Summary(string text, int x, int y) => new Label { Text = text, AutoSize = true, Location = new Point(x, y), Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
        private static Button Button(string text, int x, int y, int width, Color color) { var b = new Button { Text = text, Location = new Point(x, y), Width = width, Height = 34, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) }; b.FlatAppearance.BorderSize = 0; return b; }

        private class PurchaseRow
        {
            [Browsable(false)] public int ProductId { get; set; }
            public string Product { get; set; }
            public int Quantity { get; set; }
            public decimal UnitCost { get; set; }
            public decimal Discount { get; set; }
            public decimal Tax { get; set; }
            public decimal LineTotal { get; set; }
        }
    }
}
