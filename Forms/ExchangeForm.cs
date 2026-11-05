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
            Width = 420;
            Height = 240;
            BackColor = PosPalette.Canvas;
            StartPosition = FormStartPosition.CenterParent;
        }
    }
}
