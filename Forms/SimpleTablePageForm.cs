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
            Font = PosPalette.Ui(10F);
            POS_System.Program.SettingsManager.RegisterForm(this);
            BackColor = PosPalette.Canvas;

            Label titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = PosPalette.Canvas,
                Font = PosPalette.Ui(16F, FontStyle.Bold),
                ForeColor = PosPalette.Ink,
                Text = title,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label subtitleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = PosPalette.Canvas,
                Font = PosPalette.Ui(10F, FontStyle.Regular),
                ForeColor = PosPalette.Muted,
                Text = subtitle + " - " + tableName,
                TextAlign = ContentAlignment.MiddleCenter
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
