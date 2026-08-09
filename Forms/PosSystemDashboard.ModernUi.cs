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
        private Label modernBrandTitle;
        private Label modernUserBadge;
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

                foreach (Control control in panelheader.Controls)
                    control.Visible = false;

                modernBrandTitle = new Label
                {
                    Text = "BIKE ZONE  POS",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                    ForeColor = ModernUiService.TextPrimary,
                    Location = new Point(24, 11)
                };

                modernWorkspaceTitle = new Label
                {
                    Text = "Dashboard  •  Workspace",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5F),
                    ForeColor = ModernUiService.TextSecondary,
                    Location = new Point(26, 48)
                };

                modernUserBadge = new Label
                {
                    Text = (_username ?? "User") + "  •  " + (_role ?? string.Empty),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = ModernUiService.TextPrimary,
                    BackColor = ModernUiService.SurfaceMuted,
                    Padding = new Padding(14, 8, 14, 8),
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };

                modernConnectionBadge = new Label
                {
                    Text = "●  Connected",
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = ModernUiService.Success,
                    Anchor = AnchorStyles.Right | AnchorStyles.Top
                };

                panelheader.Controls.Add(modernBrandTitle);
                panelheader.Controls.Add(modernWorkspaceTitle);
                panelheader.Controls.Add(modernUserBadge);
                panelheader.Controls.Add(modernConnectionBadge);
                if (auditNotificationButton != null)
                {
                    auditNotificationButton.Visible = AppSession.IsAdministrator;
                    panelheader.Controls.Add(auditNotificationButton);
                }

                panelheader.Resize += (sender, args) => LayoutModernHeader();
                LayoutModernHeader();
            }

            if (dashboardMenuStrip != null)
            {
                dashboardMenuStrip.BackColor = ModernUiService.Sidebar;
                dashboardMenuStrip.ForeColor = Color.White;
                dashboardMenuStrip.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                dashboardMenuStrip.Padding = new Padding(18, 8, 18, 8);
                dashboardMenuStrip.RenderMode = ToolStripRenderMode.System;

                foreach (ToolStripItem item in dashboardMenuStrip.Items)
                {
                    item.ForeColor = Color.White;
                    item.BackColor = ModernUiService.Sidebar;
                    item.Padding = new Padding(12, 7, 12, 7);
                    item.Margin = new Padding(2, 1, 2, 1);
                }
            }

            ModernUiService.Apply(this);
        }

        private void LayoutModernHeader()
        {
            if (panelheader == null || modernConnectionBadge == null || modernUserBadge == null) return;
            int right = panelheader.ClientSize.Width - 24;
            if (auditNotificationButton != null && auditNotificationButton.Visible)
            {
                auditNotificationButton.SetBounds(right - 44, 18, 42, 42);
                right = auditNotificationButton.Left - 14;
            }
            modernConnectionBadge.Location = new Point(Math.Max(10, right - modernConnectionBadge.Width), 34);
            modernUserBadge.Location = new Point(Math.Max(10, modernConnectionBadge.Left - modernUserBadge.Width - 22), 22);
        }

        private void SetWorkspaceTitle(string title)
        {
            if (modernWorkspaceTitle != null && !modernWorkspaceTitle.IsDisposed)
                modernWorkspaceTitle.Text = (string.IsNullOrWhiteSpace(title) ? "Dashboard" : title) + "  •  Workspace";
        }
    }
}
