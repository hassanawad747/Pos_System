using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class ResponsiveLayoutService
    {
        private static readonly Dictionary<Form, FormLayoutSnapshot> snapshots =
            new Dictionary<Form, FormLayoutSnapshot>();

        public static void Register(Form form)
        {
            if (form == null || snapshots.ContainsKey(form))
            {
                return;
            }

            form.Shown += Form_Shown;
            form.Disposed += Form_Disposed;
        }

        private static void Form_Shown(object sender, EventArgs e)
        {
            Form form = sender as Form;
            if (form == null || form.IsDisposed || snapshots.ContainsKey(form))
            {
                return;
            }

            snapshots[form] = CaptureForm(form);
            form.Resize += Form_Resize;
            ApplyLayout(form);
        }

        private static void Form_Resize(object sender, EventArgs e)
        {
            ApplyLayout(sender as Form);
        }

        private static void Form_Disposed(object sender, EventArgs e)
        {
            Form form = sender as Form;
            if (form == null)
            {
                return;
            }

            form.Shown -= Form_Shown;
            form.Resize -= Form_Resize;
            form.Disposed -= Form_Disposed;
            snapshots.Remove(form);
        }

        private static FormLayoutSnapshot CaptureForm(Form form)
        {
            FormLayoutSnapshot snapshot = new FormLayoutSnapshot(form.ClientSize);
            CaptureChildren(form, snapshot);
            return snapshot;
        }

        private static void CaptureChildren(Control parent, FormLayoutSnapshot snapshot)
        {
            foreach (Control child in parent.Controls)
            {
                snapshot.Controls[child] = new ControlLayoutSnapshot(
                    child.Bounds,
                    child.Font,
                    parent.ClientSize,
                    child.Dock);

                if (child.HasChildren)
                {
                    CaptureChildren(child, snapshot);
                }
            }
        }

        private static void ApplyLayout(Form form)
        {
            if (form == null || form.IsDisposed || !snapshots.TryGetValue(form, out FormLayoutSnapshot snapshot))
            {
                return;
            }

            if (form.ClientSize.Width <= 0 || form.ClientSize.Height <= 0)
            {
                return;
            }

            form.SuspendLayout();

            foreach (KeyValuePair<Control, ControlLayoutSnapshot> item in snapshot.Controls.ToList())
            {
                Control control = item.Key;
                ControlLayoutSnapshot original = item.Value;

                if (control.IsDisposed || control.Parent == null || original.Dock != DockStyle.None)
                {
                    continue;
                }

                Size parentSize = control.Parent.ClientSize;
                if (parentSize.Width <= 0 || parentSize.Height <= 0 ||
                    original.ParentSize.Width <= 0 || original.ParentSize.Height <= 0)
                {
                    continue;
                }

                float scaleX = parentSize.Width / (float)original.ParentSize.Width;
                float scaleY = parentSize.Height / (float)original.ParentSize.Height;
                float fontScale = Math.Max(0.75f, Math.Min(1.35f, Math.Min(scaleX, scaleY)));

                Rectangle scaledBounds = new Rectangle(
                    Scale(original.Bounds.X, scaleX),
                    Scale(original.Bounds.Y, scaleY),
                    Math.Max(1, Scale(original.Bounds.Width, scaleX)),
                    Math.Max(1, Scale(original.Bounds.Height, scaleY)));

                control.Bounds = scaledBounds;
                ApplyScaledFont(control, original.Font, fontScale);
            }

            form.ResumeLayout(true);
        }

        private static void ApplyScaledFont(Control control, Font originalFont, float fontScale)
        {
            if (originalFont == null)
            {
                return;
            }

            float newSize = Math.Max(6F, Math.Min(24F, originalFont.Size * fontScale));
            if (Math.Abs(control.Font.Size - newSize) < 0.25F)
            {
                return;
            }

            control.Font = new Font(originalFont.FontFamily, newSize, originalFont.Style);
        }

        private static int Scale(int value, float scale)
        {
            return (int)Math.Round(value * scale);
        }

        private sealed class FormLayoutSnapshot
        {
            public FormLayoutSnapshot(Size clientSize)
            {
                ClientSize = clientSize;
            }

            public Size ClientSize { get; }
            public Dictionary<Control, ControlLayoutSnapshot> Controls { get; } =
                new Dictionary<Control, ControlLayoutSnapshot>();
        }

        private sealed class ControlLayoutSnapshot
        {
            public ControlLayoutSnapshot(Rectangle bounds, Font font, Size parentSize, DockStyle dock)
            {
                Bounds = bounds;
                Font = font;
                ParentSize = parentSize;
                Dock = dock;
            }

            public Rectangle Bounds { get; }
            public Font Font { get; }
            public Size ParentSize { get; }
            public DockStyle Dock { get; }
        }
    }
}
