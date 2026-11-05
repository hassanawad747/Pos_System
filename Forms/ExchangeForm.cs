using System.Drawing;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public class ExchangeForm : Form
    {
        public ExchangeForm()
        {
            Text = "Exchange";
            POS_System.Program.SettingsManager.RegisterForm(this);
            Width = 460;
            Height = 280;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = PosPalette.Canvas;
            Font = PosPalette.Ui(10F, FontStyle.Regular);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Label title = new Label
            {
                Dock = DockStyle.Top,
                Height = 52,
                Text = "Exchange",
                Font = PosPalette.Ui(16F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = PosPalette.Header,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };

            Label body = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                Font = PosPalette.Ui(11F, FontStyle.Regular),
                ForeColor = PosPalette.Ink,
                Text = "Use Settings to change the dollar exchange rate stored on this register."
            };

            Controls.Add(body);
            Controls.Add(title);
        }
    }
}
