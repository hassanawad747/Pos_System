using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class LoadingOverlayService
    {
        public static IDisposable Show(Form form, string message = "Loading...")
        {
            if (form == null || form.IsDisposed)
            {
                return new EmptyLoadingScope();
            }

            Panel overlay = new Panel
            {
                BackColor = Color.FromArgb(245, 247, 250),
                Dock = DockStyle.Fill,
                Cursor = Cursors.WaitCursor
            };

            Label label = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Text = message,
                TextAlign = ContentAlignment.MiddleCenter
            };

            overlay.Controls.Add(label);
            form.Controls.Add(overlay);
            overlay.BringToFront();
            form.UseWaitCursor = true;
            overlay.Refresh();
            Application.DoEvents();

            return new LoadingScope(form, overlay);
        }

        private sealed class LoadingScope : IDisposable
        {
            private readonly Form form;
            private readonly Panel overlay;
            private bool disposed;

            public LoadingScope(Form form, Panel overlay)
            {
                this.form = form;
                this.overlay = overlay;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    return;
                }

                disposed = true;

                if (form != null && !form.IsDisposed)
                {
                    form.UseWaitCursor = false;
                }

                if (overlay != null && !overlay.IsDisposed)
                {
                    Control parent = overlay.Parent;
                    parent?.Controls.Remove(overlay);
                    overlay.Dispose();
                }
            }
        }

        private sealed class EmptyLoadingScope : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
