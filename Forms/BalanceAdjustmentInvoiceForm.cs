using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    internal sealed class BalanceAdjustmentInvoiceForm : Form
    {
        private readonly string partyType;
        private readonly string partyName;
        private readonly string contactInfo;
        private readonly string actionTitle;
        private readonly string amountText;
        private readonly string balanceAfterText;
        private readonly string generatedBy;
        private readonly DateTime generatedAt;
        private readonly bool showCreatedBy;
        private PrintDocument printDocument;

        public BalanceAdjustmentInvoiceForm(
            string partyType,
            string partyName,
            string contactInfo,
            string actionTitle,
            string amountText,
            string balanceAfterText,
            string generatedBy,
            DateTime generatedAt,
            bool showCreatedBy = true)
        {
            this.partyType = partyType;
            this.partyName = string.IsNullOrWhiteSpace(partyName) ? "-" : partyName;
            this.contactInfo = string.IsNullOrWhiteSpace(contactInfo) ? "-" : contactInfo;
            this.actionTitle = actionTitle;
            this.amountText = amountText;
            this.balanceAfterText = string.IsNullOrWhiteSpace(balanceAfterText) ? "-" : balanceAfterText;
            this.generatedBy = string.IsNullOrWhiteSpace(generatedBy) ? "-" : generatedBy;
            this.generatedAt = generatedAt;
            this.showCreatedBy = showCreatedBy;

            InitializeInvoiceForm();
        }

        private void InitializeInvoiceForm()
        {
            Text = "Balance Invoice";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(720, 520);
            MinimumSize = new Size(640, 460);
            BackColor = Color.FromArgb(244, 247, 252);

            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            Label titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(24, 18),
                Text = "Balance Adjustment Invoice"
            };

            Label dateLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(191, 219, 254),
                Location = new Point(26, 54),
                Text = generatedAt.ToString("yyyy-MM-dd HH:mm")
            };

            headerPanel.Controls.Add(titleLabel);
            headerPanel.Controls.Add(dateLabel);

            Panel contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                BackColor = Color.White
            };

            int top = 24;
            contentPanel.Controls.Add(CreateValueLabel("Document Type", "Balance adjustment invoice", ref top));
            contentPanel.Controls.Add(CreateValueLabel("Party Type", partyType, ref top));
            contentPanel.Controls.Add(CreateValueLabel("Name", partyName, ref top));
            contentPanel.Controls.Add(CreateValueLabel("Phone / Contact", contactInfo, ref top));
            contentPanel.Controls.Add(CreateValueLabel("Action", actionTitle, ref top));
            contentPanel.Controls.Add(CreateValueLabel("Amount", amountText, ref top));
            contentPanel.Controls.Add(CreateValueLabel("Balance After", balanceAfterText, ref top));
            if (showCreatedBy)
            {
                contentPanel.Controls.Add(CreateValueLabel("Created By", generatedBy, ref top));
            }
            contentPanel.Controls.Add(CreateValueLabel("Created At", generatedAt.ToString("yyyy-MM-dd HH:mm"), ref top));

            Panel footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = Color.FromArgb(248, 250, 252)
            };

            Button btnPrint = new Button
            {
                Text = "Print Invoice",
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(150, 40),
                Location = new Point(24, 16)
            };
            btnPrint.FlatAppearance.BorderSize = 0;
            btnPrint.Click += BtnPrint_Click;

            Button btnClose = new Button
            {
                Text = "Close",
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(120, 40),
                Location = new Point(190, 16)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (sender, args) => Close();

            footerPanel.Controls.Add(btnPrint);
            footerPanel.Controls.Add(btnClose);

            Controls.Add(contentPanel);
            Controls.Add(footerPanel);
            Controls.Add(headerPanel);
        }

        private Control CreateValueLabel(string title, string value, ref int top)
        {
            Panel row = new Panel
            {
                Location = new Point(18, top),
                Size = new Size(640, 42)
            };

            Label titleLabel = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(0, 8),
                Size = new Size(180, 26),
                Text = title
            };

            Label valueLabel = new Label
            {
                AutoSize = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(190, 8),
                Size = new Size(430, 26),
                Text = value
            };

            row.Controls.Add(titleLabel);
            row.Controls.Add(valueLabel);
            top += 46;
            return row;
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
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
            using (Font titleFont = new Font("Segoe UI", 18F, FontStyle.Bold))
            using (Font headerFont = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (Font textFont = new Font("Segoe UI", 10F, FontStyle.Regular))
            {
                int y = 40;
                int left = 40;

                e.Graphics.DrawString("Balance Adjustment Invoice", titleFont, Brushes.Black, left, y);
                y += 42;
                e.Graphics.DrawString("Date: " + generatedAt.ToString("yyyy-MM-dd HH:mm"), textFont, Brushes.Black, left, y);
                y += 36;

                DrawLine(e, left, y);
                y += 18;

                DrawRow(e, headerFont, textFont, left, ref y, "Party Type", partyType);
                DrawRow(e, headerFont, textFont, left, ref y, "Name", partyName);
                DrawRow(e, headerFont, textFont, left, ref y, "Phone / Contact", contactInfo);
                DrawRow(e, headerFont, textFont, left, ref y, "Action", actionTitle);
                DrawRow(e, headerFont, textFont, left, ref y, "Amount", amountText);
                DrawRow(e, headerFont, textFont, left, ref y, "Balance After", balanceAfterText);
                if (showCreatedBy)
                {
                    DrawRow(e, headerFont, textFont, left, ref y, "Created By", generatedBy);
                }
                DrawRow(e, headerFont, textFont, left, ref y, "Created At", generatedAt.ToString("yyyy-MM-dd HH:mm"));

                y += 14;
                DrawLine(e, left, y);
                y += 30;
                e.Graphics.DrawString("Signature: ____________________", textFont, Brushes.Black, left, y);
            }
        }

        private static void DrawRow(PrintPageEventArgs e, Font headerFont, Font textFont, int left, ref int y, string label, string value)
        {
            e.Graphics.DrawString(label + ":", headerFont, Brushes.Black, left, y);
            e.Graphics.DrawString(value, textFont, Brushes.Black, left + 180, y);
            y += 28;
        }

        private static void DrawLine(PrintPageEventArgs e, int left, int y)
        {
            e.Graphics.DrawLine(Pens.Gray, left, y, left + 700, y);
        }
    }
}
