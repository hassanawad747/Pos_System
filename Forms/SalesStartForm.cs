using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class SalesStartForm : Form
    {
        private readonly string username;
        private readonly EventHandler startClicked;

        public SalesStartForm(string username, EventHandler startClicked)
        {
            this.username = username;
            this.startClicked = startClicked;

            InitializeStartScreen();
        }

        private void InitializeStartScreen()
        {
            BackColor = Color.FromArgb(244, 247, 252);
            Dock = DockStyle.Fill;
            Font = new Font("Segoe UI", 11F, FontStyle.Regular);

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(24),
                BackColor = Color.FromArgb(244, 247, 252)
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));

            Label titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Start Sales Work",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                TextAlign = ContentAlignment.BottomCenter
            };

            Label userLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = string.IsNullOrWhiteSpace(username) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm") : username,
                Font = new Font("Segoe UI", 13F, FontStyle.Regular),
                ForeColor = Color.FromArgb(71, 85, 105),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button startButton = new Button
            {
                Anchor = AnchorStyles.None,
                BackColor = Color.FromArgb(22, 163, 74),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(360, 96),
                Text = "START"
            };

            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += startClicked;

            layout.Controls.Add(titleLabel, 0, 1);
            layout.Controls.Add(userLabel, 0, 2);
            layout.Controls.Add(startButton, 0, 3);
            Controls.Add(layout);
        }
    }
}
