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
            POS_System.Program.SettingsManager.RegisterForm(this);
            BackColor = PosPalette.Canvas;
            Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Font = PosPalette.Ui(16F, FontStyle.Bold),
                ForeColor = PosPalette.Ink,
                Text = "Dashboard Home",
                TextAlign = ContentAlignment.MiddleCenter
            });
        }
    }
}
