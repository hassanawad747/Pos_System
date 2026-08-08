using Pos_System.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class PosSystemDashboard
    {
        private Label modernWorkspaceTitle;
        private Label modernConnectionBadge;
        private bool modernDashboardApplied;

        private void ApplyModernDashboardChrome()
        {
            if (modernDashboardApplied) return;
            modernDashboardApplied = true;

            BackColor = ModernUiService.AppBackground;
            Font = new Font("Segoe UI", 9.5F);
            MinimumSize = new Size(1180, 720);

            if (panelContent != null)
                panelContent.BackColor = ModernUiService.AppBackground;

            if (panelheader != null)
            {
                panelheader.BackColor = Color.White;
                panelheader.Padding = new Padding(20, 8, 20, 8);

                modernWorkspaceTitle = new Label
                {
                    Text = "Bike Zone POS  /  Workspace",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    ForeColor = ModernUiService.TextPrimary,
                    Location = new Point(18, 10),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top
                };

                modernConnectionBadge = new Label
                {
                    Text = "●  Connected",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = ModernUiService.Success,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };

                panelheader.Controls.Add(modernWorkspaceTitle);
                panelheader.Controls.Add(modernConnectionBadge);
                panelheader.Resize += (s, e) =>
                {
                    modernConnectionBadge.Location = new Point(
                        Math.Max(10, panelheader.ClientSize.Width - modernConnectionBadge.Width - 22),
                        13);
                };
                modernConnectionBadge.Location = new Point(
                    Math.Max(10, panelheader.ClientSize.Width - modernConnectionBadge.Width - 22),
                    13);
            }

            if (dashboardMenuStrip != null)
            {
                dashboardMenuStrip.BackColor = ModernUiService.Sidebar;
                dashboardMenuStrip.ForeColor = Color.White;
                dashboardMenuStrip.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                dashboardMenuStrip.Padding = new Padding(12, 6, 12, 6);
                dashboardMenuStrip.RenderMode = ToolStripRenderMode.System;

                foreach (ToolStripItem item in dashboardMenuStrip.Items)
                {
                    item.ForeColor = Color.White;
                    item.BackColor = ModernUiService.Sidebar;
                    item.Padding = new Padding(10, 6, 10, 6);
                    item.Margin = new Padding(2, 1, 2, 1);
                }
            }

            ModernUiService.Apply(this);
        }

        private void SetWorkspaceTitle(string title)
        {
            if (modernWorkspaceTitle != null && !modernWorkspaceTitle.IsDisposed)
                modernWorkspaceTitle.Text = "Bike Zone POS  /  " + (string.IsNullOrWhiteSpace(title) ? "Workspace" : title);
        }
    }
}
