using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class SimpleTablePageForm : Form
    {
        public SimpleTablePageForm(string title, string subtitle, string tableName, string message)
        {
            Text = title;
            BackColor = Color.White;

            Label titleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 50,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Text = title,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label subtitleLabel = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.DimGray,
                Text = subtitle + " - " + tableName,
                TextAlign = ContentAlignment.MiddleCenter
            };

            TextBox messageBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 11F),
                Text = message
            };

            Controls.Add(messageBox);
            Controls.Add(subtitleLabel);
            Controls.Add(titleLabel);
        }
    }
}
