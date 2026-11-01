using System.Drawing;

namespace Pos_System.Services
{
    /// <summary>
    /// Warm atelier palette. Deliberately not the navy / teal shell.
    /// </summary>
    internal static class PosPalette
    {
        public static bool Dark { get; set; }

        public static Color Canvas => Dark ? Color.FromArgb(32, 24, 20) : Color.FromArgb(243, 236, 226);
        public static Color Surface => Dark ? Color.FromArgb(48, 36, 30) : Color.FromArgb(255, 251, 246);
        public static Color Sidebar => Dark ? Color.FromArgb(40, 28, 22) : Color.FromArgb(255, 248, 241);
        public static Color SidebarInk => Dark ? Color.FromArgb(248, 240, 230) : Color.FromArgb(62, 40, 28);
        public static Color Ink => Dark ? Color.FromArgb(248, 241, 232) : Color.FromArgb(48, 32, 24);
        public static Color Muted => Dark ? Color.FromArgb(196, 170, 150) : Color.FromArgb(122, 96, 78);
        public static Color Line => Dark ? Color.FromArgb(92, 70, 56) : Color.FromArgb(228, 214, 198);
        public static Color Accent => Color.FromArgb(196, 82, 45);
        public static Color AccentDeep => Color.FromArgb(140, 52, 28);
        public static Color AccentSoft => Dark ? Color.FromArgb(92, 52, 38) : Color.FromArgb(246, 222, 208);
        public static Color Header => Dark ? Color.FromArgb(28, 18, 14) : Color.FromArgb(62, 40, 28);
        public static Color Success => Color.FromArgb(30, 122, 82);
        public static Color Danger => Color.FromArgb(176, 48, 42);
        public static Color Gold => Color.FromArgb(176, 122, 42);
        public static Color Selection => Dark ? Color.FromArgb(92, 58, 44) : Color.FromArgb(242, 214, 196);

        public static Font Ui(float size, FontStyle style)
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Point);
        }
    }
}
