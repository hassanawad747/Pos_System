using System;
using System.Drawing;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public class AdminDashboardHomeForm : Form
    {
        public AdminDashboardHomeForm()
        {
            Text = "Dashboard";
            Font = PosPalette.Ui(10F);
            POS_System.Program.SettingsManager.RegisterForm(this);
            BackColor = PosPalette.Canvas;

            Panel card = new Panel
            {
                BackColor = PosPalette.Surface,
                Size = new Size(480, 160)
            };
            card.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Font = PosPalette.Ui(11F),
                ForeColor = PosPalette.Muted,
                Text = "Choose a section from the sidebar.",
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(24, 8, 24, 16)
            });
            card.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 64,
                Font = PosPalette.Ui(18F, FontStyle.Bold),
                ForeColor = PosPalette.Ink,
                Text = "Dashboard",
                TextAlign = ContentAlignment.BottomCenter
            });

            void CenterCard(object sender, EventArgs e)
            {
                card.Left = Math.Max(0, (ClientSize.Width - card.Width) / 2);
                card.Top = Math.Max(0, (ClientSize.Height - card.Height) / 2);
            }

            Resize += CenterCard;
            Controls.Add(card);
            Load += CenterCard;
        }
    }
}
