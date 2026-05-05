using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class ArabicToolTipService
    {
        private static readonly Dictionary<Form, ToolTip> toolTips = new Dictionary<Form, ToolTip>();
        private static readonly Dictionary<Form, Control> lastDisabledControl = new Dictionary<Form, Control>();

        public static void Register(Form form)
        {
            if (form == null || form.IsDisposed)
            {
                return;
            }

            if (!toolTips.ContainsKey(form))
            {
                toolTips[form] = new ToolTip
                {
                    AutoPopDelay = 5000,
                    InitialDelay = 200,
                    ReshowDelay = 80,
                    ShowAlways = true
                };

                form.Disposed += Form_Disposed;
            }

            Apply(form, form);
            form.MouseMove -= Parent_MouseMove;
            form.MouseMove += Parent_MouseMove;
        }

        private static void Apply(Form form, Control parent)
        {
            parent.ControlAdded -= Parent_ControlAdded;
            parent.ControlAdded += Parent_ControlAdded;
            parent.MouseMove -= Parent_MouseMove;
            parent.MouseMove += Parent_MouseMove;

            foreach (Control control in parent.Controls)
            {
                ApplyToControl(form, control);

                if (control.HasChildren)
                {
                    Apply(form, control);
                }
            }
        }

        private static void ApplyToControl(Form form, Control control)
        {
            if (!toolTips.TryGetValue(form, out ToolTip toolTip))
            {
                return;
            }

            string hint = BuildHint(control);
            if (!string.IsNullOrWhiteSpace(hint))
            {
                toolTip.SetToolTip(control, hint);
            }
        }

        private static string BuildHint(Control control)
        {
            string name = (control.Name ?? string.Empty).ToLowerInvariant();
            string text = (control.Text ?? string.Empty).Trim();

            if (control is Button)
            {
                return string.IsNullOrWhiteSpace(text) ? "اضغط هنا" : "اضغط: " + text;
            }

            if (control is TextBox)
            {
                if (name.Contains("search") || name.Contains("barcode"))
                    return "اكتب أو امسح الباركود";
                if (name.Contains("quantity") || name.Contains("qty"))
                    return "اكتب الكمية";
                if (name.Contains("dollar") || name.Contains("rate"))
                    return "اكتب سعر الدولار";
                if (name.Contains("paid") || name.Contains("amount"))
                    return "اكتب المبلغ";
                if (name.Contains("saleid") || name.Contains("invoice"))
                    return "اكتب رقم الفاتورة";
                if (name.Contains("phone"))
                    return "اكتب رقم الهاتف";
                if (name.Contains("email"))
                    return "اكتب البريد الإلكتروني";
                if (name.Contains("password"))
                    return "اكتب كلمة المرور";
                if (name.Contains("user"))
                    return "اكتب اسم المستخدم";
                if (name.Contains("price"))
                    return "اكتب السعر";
                if (name.Contains("stock"))
                    return "اكتب كمية المخزون";
                if (name.Contains("name"))
                    return "اكتب الاسم";

                return "اكتب هنا";
            }

            if (control is ComboBox)
            {
                return "اختر من القائمة";
            }

            if (control is CheckBox || control is RadioButton)
            {
                return string.IsNullOrWhiteSpace(text) ? "اختر الخيار" : "اختر: " + text;
            }

            if (control is DataGridView)
            {
                return "جدول البيانات";
            }

            return string.Empty;
        }

        private static void Parent_ControlAdded(object sender, ControlEventArgs e)
        {
            Control parent = sender as Control;
            Form form = parent?.FindForm();
            if (form == null)
            {
                return;
            }

            ApplyToControl(form, e.Control);

            if (e.Control.HasChildren)
            {
                Apply(form, e.Control);
            }
        }

        private static void Parent_MouseMove(object sender, MouseEventArgs e)
        {
            Control parent = sender as Control;
            Form form = parent?.FindForm();
            if (form == null || !toolTips.TryGetValue(form, out ToolTip toolTip))
            {
                return;
            }

            Control disabledControl = FindDisabledControlAt(parent, e.Location);
            if (disabledControl == null || string.IsNullOrWhiteSpace(BuildHint(disabledControl)))
            {
                lastDisabledControl[form] = null;
                return;
            }

            if (lastDisabledControl.TryGetValue(form, out Control lastControl) && lastControl == disabledControl)
            {
                return;
            }

            lastDisabledControl[form] = disabledControl;
            Point showPoint = parent.PointToClient(Cursor.Position);
            toolTip.Show(BuildHint(disabledControl), parent, showPoint.X + 12, showPoint.Y + 12, 2500);
        }

        private static Control FindDisabledControlAt(Control parent, Point parentPoint)
        {
            foreach (Control child in parent.Controls)
            {
                Rectangle bounds = child.Bounds;
                if (!bounds.Contains(parentPoint))
                {
                    continue;
                }

                Point childPoint = new Point(parentPoint.X - child.Left, parentPoint.Y - child.Top);
                Control nested = FindDisabledControlAt(child, childPoint);
                if (nested != null)
                {
                    return nested;
                }

                return child.Enabled ? null : child;
            }

            return null;
        }

        private static void Form_Disposed(object sender, EventArgs e)
        {
            Form form = sender as Form;
            if (form == null)
            {
                return;
            }

            if (toolTips.TryGetValue(form, out ToolTip toolTip))
            {
                toolTip.Dispose();
            }

            toolTips.Remove(form);
            lastDisabledControl.Remove(form);
            form.Disposed -= Form_Disposed;
        }
    }
}
