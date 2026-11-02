using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class PageChrome
    {
        public static void Apply(Form form)
        {
            PosPresenter.Watch(form);
        }
    }
}
