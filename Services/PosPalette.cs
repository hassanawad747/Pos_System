using System.Drawing;

namespace Pos_System.Services
{
    /// <summary>
    /// Professional Smart POS palette.
    /// Dark readable sidebar, bright workspace, white cards, one blue accent.
    /// High-contrast text only: dark ink on light surfaces, white on the sidebar.
    /// </summary>
    internal static class PosPalette
    {
        public static bool Dark { get; set; }

        public static Color Canvas => Dark ? Color.FromArgb(11, 18, 32) : Color.FromArgb(241, 245, 249);
        public static Color Surface => Dark ? Color.FromArgb(22, 32, 51) : Color.White;
        public static Color Sidebar => Dark ? Color.FromArgb(8, 12, 22) : Color.FromArgb(15, 23, 42);
        public static Color SidebarInk => Color.White;
        public static Color SidebarMuted => Color.FromArgb(148, 163, 184);
        public static Color NavHover => Color.FromArgb(30, 41, 59);
        public static Color Ink => Dark ? Color.FromArgb(248, 250, 252) : Color.FromArgb(15, 23, 42);
        public static Color Muted => Dark ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85);
        public static Color Line => Dark ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240);
        public static Color Accent => Color.FromArgb(37, 99, 235);
        public static Color AccentDeep => Color.FromArgb(29, 78, 216);
        public static Color AccentSoft => Dark ? Color.FromArgb(30, 58, 95) : Color.FromArgb(219, 234, 254);
        public static Color Header => Sidebar;
        public static Color Success => Color.FromArgb(5, 150, 105);
        public static Color Danger => Color.FromArgb(220, 38, 38);
        public static Color Warning => Color.FromArgb(217, 119, 6);
        public static Color Gold => Warning;
        public static Color Selection => Dark ? Color.FromArgb(30, 64, 175) : Color.FromArgb(219, 234, 254);
        public static Color Secondary => Color.FromArgb(51, 65, 85);
        public static Color InputFill => Dark ? Color.FromArgb(15, 23, 42) : Color.White;

        public static Font Ui(float size, FontStyle style)
        {
            return new Font("Segoe UI", size, style, GraphicsUnit.Point);
        }
    }
}
