using System.Drawing;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public class SimpleTablePageForm : Form
    {
        public SimpleTablePageForm(string title, string subtitle, string tableName, string message)
        {
            Text = title;
            POS_System.Program.SettingsManager.RegisterForm(this);
            BackColor = PosPalette.Canvas;
            Font = PosPalette.Ui(10F, FontStyle.Regular);

            Label titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 52,
                Font = PosPalette.Ui(16F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = PosPalette.Header,
                Text = title,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };

            Label subtitleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                Font = PosPalette.Ui(10F, FontStyle.Regular),
                ForeColor = PosPalette.Muted,
                BackColor = PosPalette.Surface,
                Text = subtitle + " - " + tableName,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 8, 16, 0)
            };

            TextBox messageBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = PosPalette.Surface,
                ForeColor = PosPalette.Ink,
                Font = PosPalette.Ui(11F, FontStyle.Regular),
                Text = message
            };

            Controls.Add(messageBox);
            Controls.Add(subtitleLabel);
            Controls.Add(titleLabel);
        }
    }
}
