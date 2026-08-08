using Pos_System.Services;

namespace Pos_System
{
    public partial class LoginForm
    {
        static LoginForm()
        {
            ModernUiService.EnableGlobalTheme();
            ErrorLogService.Enable();
        }
    }
}
