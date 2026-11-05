using System.Drawing;
using System.Windows.Forms;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public class AdminDashboardHomeForm : Form
    {
        public AdminDashboardHomeForm()
        {
            POS_System.Program.SettingsManager.RegisterForm(this);
            Text = "Dashboard";
            BackColor = PosPalette.Canvas;
            Font = PosPalette.Ui(10F, FontStyle.Regular);
            Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Font = PosPalette.Ui(16F, FontStyle.Bold),
                ForeColor = PosPalette.Ink,
                Text = "Dashboard",
                TextAlign = ContentAlignment.MiddleCenter
            });
        }
    }
}
