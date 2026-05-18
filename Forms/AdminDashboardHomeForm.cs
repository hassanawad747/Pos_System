using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class AdminDashboardHomeForm : Form
    {
        public AdminDashboardHomeForm()
        {
            BackColor = Color.White;
            Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Text = "Dashboard Home",
                TextAlign = ContentAlignment.MiddleCenter
            });
        }
    }
}
