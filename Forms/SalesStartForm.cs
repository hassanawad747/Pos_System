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
        private Button startButton;
        private bool starting;

        public SalesStartForm(string username, EventHandler startClicked)
        {
            this.username = username;
            this.startClicked = startClicked;
            InitializeStartScreen();
            POS_System.Program.SettingsManager.RegisterForm(this);
        }

        private void InitializeStartScreen()
        {
            Text = "Start work";
            BackColor = PosPalette.Canvas;
            Dock = DockStyle.Fill;
            Font = PosPalette.Ui(11F, FontStyle.Regular);
            PosPresenter.SetRole(this, "canvas");

            Panel card = new Panel
            {
                Size = new Size(520, 340),
                BackColor = PosPalette.Surface,
                Padding = new Padding(36, 32, 36, 32)
            };
            PosPresenter.SetRole(card, "surface");
            card.Anchor = AnchorStyles.None;

            Label titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 48,
                Text = "Start Sales Work",
                Font = PosPalette.Ui(24F, FontStyle.Bold),
                ForeColor = PosPalette.Ink,
                TextAlign = ContentAlignment.MiddleCenter
            };
            PosPresenter.SetRole(titleLabel, "ink");

            Label userLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = string.IsNullOrWhiteSpace(username)
                    ? DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                    : username + "  ·  " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                Font = PosPalette.Ui(11F, FontStyle.Regular),
                ForeColor = PosPalette.Muted,
                TextAlign = ContentAlignment.MiddleCenter
            };
            PosPresenter.SetRole(userLabel, "muted");

            Label hintLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 48,
                Text = "Open the ticket for this shift.\r\nابدأ وردية المبيعات على هذا الصندوق.",
                Font = PosPalette.Ui(10.5F, FontStyle.Regular),
                ForeColor = PosPalette.Muted,
                TextAlign = ContentAlignment.MiddleCenter
            };
            PosPresenter.SetRole(hintLabel, "muted");

            startButton = new Button
            {
                Name = "btnStartWork",
                Dock = DockStyle.Bottom,
                Height = 56,
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Font = PosPalette.Ui(18F, FontStyle.Bold),
                Text = "START"
            };
            startButton.FlatAppearance.BorderSize = 0;
            startButton.Click += StartButton_Click;
            PosPresenter.SetRole(startButton, "accent");
            AcceptButton = startButton;

            card.Controls.Add(startButton);
            card.Controls.Add(hintLabel);
            card.Controls.Add(userLabel);
            card.Controls.Add(titleLabel);

            Controls.Add(card);
            Resize += (sender, args) => CenterCard(card);
            Shown += (sender, args) =>
            {
                CenterCard(card);
                startButton.Focus();
            };
        }

        private void CenterCard(Control card)
        {
            if (card == null)
            {
                return;
            }

            card.Location = new Point(
                Math.Max(16, (ClientSize.Width - card.Width) / 2),
                Math.Max(16, (ClientSize.Height - card.Height) / 2));
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (starting)
            {
                return;
            }

            starting = true;
            startButton.Enabled = false;
            try
            {
                startClicked?.Invoke(sender, e);
            }
            finally
            {
                starting = false;
                if (!IsDisposed && startButton != null)
                {
                    startButton.Enabled = true;
                }
            }
        }
    }
}
