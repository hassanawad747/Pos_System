using Pos_System.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class SalesAdvancedUiService
    {
        private static readonly HashSet<Form> Attached = new HashSet<Form>();
        private static int pendingHeldSaleId;
        private static List<SalesLifecycleService.CartLine> pendingResumeLines;

        public static void Attach(Form salesForm)
        {
            if (salesForm == null || salesForm.IsDisposed || Attached.Contains(salesForm)) return;
            DataGridView grid = Find<DataGridView>(salesForm, "datagridsales");
            Button save = Find<Button>(salesForm, "btnSaveSale");
            ComboBox customer = Find<ComboBox>(salesForm, "cmbCustomer");
            RadioButton usd = Find<RadioButton>(salesForm, "rbDollar");
            RadioButton lbp = Find<RadioButton>(salesForm, "rbLebanon");
            if (grid == null || save == null) return;

            Attached.Add(salesForm);
            salesForm.Disposed += (s, e) => Attached.Remove(salesForm);

            Control host = save.Parent;
            var hold = NewActionButton("Hold Sale", Color.FromArgb(245, 158, 11));
            var quote = NewActionButton("Quotation", Color.FromArgb(14, 116, 144));
            hold.Width = 110; quote.Width = 110;
            hold.Location = new Point(Math.Max(8, save.Left - hold.Width - 8), Math.Max(8, save.Top - hold.Height - 8));
            quote.Location = new Point(Math.Max(8, hold.Left - quote.Width - 8), hold.Top);

            hold.Click += (s, e) =>
            {
                try
                {
                    List<SalesLifecycleService.CartLine> lines = ReadCart(grid, usd != null && usd.Checked);
                    int? customerId = ReadSelectedId(customer);
                    string currency = usd != null && usd.Checked ? "USD" : "LBP";
                    decimal rate = POS_System.Program.SettingsManager.GetExchangeRate();
                    var service = new SalesLifecycleService(POS_System.Program.SettingsManager.ConnectionString);
                    int id = service.HoldSale(customerId, AppSession.UserId, currency, rate, lines, null);
                    grid.Rows.Clear();
                    MessageBox.Show("Sale placed on hold. Hold ID: " + id, "Hold Sale", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Hold Sale", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            quote.Click += (s, e) =>
            {
                try
                {
                    List<SalesLifecycleService.CartLine> lines = ReadCart(grid, usd != null && usd.Checked);
                    int? customerId = ReadSelectedId(customer);
                    string currency = usd != null && usd.Checked ? "USD" : "LBP";
                    decimal rate = POS_System.Program.SettingsManager.GetExchangeRate();
                    var service = new SalesLifecycleService(POS_System.Program.SettingsManager.ConnectionString);
                    int id = service.SaveQuotation(customerId, AppSession.UserId, currency, rate, lines, DateTime.Now.AddDays(7), null);
                    MessageBox.Show("Quotation saved. Quotation ID: " + id, "Quotation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Quotation", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            };

            host.Controls.Add(hold); host.Controls.Add(quote); hold.BringToFront(); quote.BringToFront();
            TryApplyPendingResume(grid);
        }

        public static void QueueResume(int heldSaleId, List<SalesLifecycleService.CartLine> lines)
        {
            pendingHeldSaleId = heldSaleId;
            pendingResumeLines = lines;
        }

        private static void TryApplyPendingResume(DataGridView grid)
        {
            if (pendingResumeLines == null || pendingResumeLines.Count == 0) return;
            try
            {
                grid.Rows.Clear();
                foreach (SalesLifecycleService.CartLine line in pendingResumeLines)
                {
                    int rowIndex = grid.Rows.Add();
                    DataGridViewRow row = grid.Rows[rowIndex];
                    SetCell(row, "product_id", line.ProductId);
                    SetCell(row, "product_name", line.ProductName);
                    SetCell(row, "quantity", line.Quantity);
                    SetCell(row, "price_usd", line.UnitPrice);
                    SetCell(row, "price_lb", line.UnitPrice);
                    SetCell(row, "original_price_usd", line.OriginalUnitPrice);
                    SetCell(row, "original_price_lb", line.OriginalUnitPrice);
                }
                new SalesLifecycleService(POS_System.Program.SettingsManager.ConnectionString).MarkHeldSaleResumed(pendingHeldSaleId);
                MessageBox.Show("Held sale restored into the cart.", "Resume Sale", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally { pendingHeldSaleId = 0; pendingResumeLines = null; }
        }

        private static List<SalesLifecycleService.CartLine> ReadCart(DataGridView grid, bool useUsd)
        {
            var lines = new List<SalesLifecycleService.CartLine>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                int productId = IntCell(row, "product_id"); int qty = IntCell(row, "quantity");
                if (productId <= 0 || qty <= 0) continue;
                decimal price = DecimalCell(row, useUsd ? "price_usd" : "price_lb");
                decimal original = DecimalCell(row, useUsd ? "original_price_usd" : "original_price_lb");
                if (original <= 0) original = price;
                decimal discount = Math.Max(0, (original - price) * qty);
                lines.Add(new SalesLifecycleService.CartLine { ProductId = productId, ProductName = StringCell(row, "product_name"), Quantity = qty, UnitPrice = price, OriginalUnitPrice = original, DiscountAmount = discount, TaxAmount = 0, LineTotal = price * qty });
            }
            if (lines.Count == 0) throw new InvalidOperationException("Add at least one product before using this action.");
            return lines;
        }

        private static Button NewActionButton(string text, Color color)
        {
            var b = new Button { Text = text, Height = 36, BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize = 0; return b;
        }

        private static int? ReadSelectedId(ComboBox box)
        {
            if (box == null || box.SelectedValue == null) return null;
            if (int.TryParse(Convert.ToString(box.SelectedValue), out int id)) return id;
            return null;
        }

        private static T Find<T>(Control root, string name) where T : Control
        {
            if (root is T match && string.Equals(root.Name, name, StringComparison.OrdinalIgnoreCase)) return match;
            foreach (Control child in root.Controls) { T result = Find<T>(child, name); if (result != null) return result; }
            return null;
        }

        private static int IntCell(DataGridViewRow row, string name) { return Has(row, name) && int.TryParse(Convert.ToString(row.Cells[name].Value), out int v) ? v : 0; }
        private static decimal DecimalCell(DataGridViewRow row, string name) { return Has(row, name) && decimal.TryParse(Convert.ToString(row.Cells[name].Value), out decimal v) ? v : 0m; }
        private static string StringCell(DataGridViewRow row, string name) { return Has(row, name) ? Convert.ToString(row.Cells[name].Value) : string.Empty; }
        private static bool Has(DataGridViewRow row, string name) { return row.DataGridView != null && row.DataGridView.Columns.Contains(name); }
        private static void SetCell(DataGridViewRow row, string name, object value) { if (Has(row, name)) row.Cells[name].Value = value; }
    }
}
