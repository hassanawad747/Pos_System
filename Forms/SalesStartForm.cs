using System;
using System.Drawing;
using System.Windows.Forms;
using Pos_System.Services;

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
            POS_System.Program.SettingsManager.RegisterForm(this);

            InitializeStartScreen();
        }

        private void InitializeStartScreen()
        {
            BackColor = PosPalette.Canvas;
            Dock = DockStyle.Fill;
            Font = PosPalette.Ui(11F, FontStyle.Regular);
            Text = "Start work";

            TableLayoutPanel layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(24),
                BackColor = PosPalette.Canvas
            };

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 28F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 72F));

            Label titleLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Start Sales Work",
                Font = PosPalette.Ui(24F, FontStyle.Bold),
                ForeColor = PosPalette.Ink,
                TextAlign = ContentAlignment.BottomCenter
            };

            Label userLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = string.IsNullOrWhiteSpace(username) ? DateTime.Now.ToString("yyyy-MM-dd HH:mm") : username,
                Font = PosPalette.Ui(13F, FontStyle.Regular),
                ForeColor = PosPalette.Muted,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Button startButton = new Button
            {
                Anchor = AnchorStyles.None,
                BackColor = PosPalette.Success,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = PosPalette.Ui(24F, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(360, 96),
                Text = "START"
            };

            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += startClicked;
            PosPresenter.SetRole(startButton, "success");

            layout.Controls.Add(titleLabel, 0, 1);
            layout.Controls.Add(userLabel, 0, 2);
            layout.Controls.Add(startButton, 0, 3);
            Controls.Add(layout);
        }
    }
}
