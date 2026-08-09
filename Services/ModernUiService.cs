using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class ModernUiService
    {
        internal static readonly Color AppBackground = Color.FromArgb(244, 247, 250);
        internal static readonly Color Surface = Color.White;
        internal static readonly Color SurfaceMuted = Color.FromArgb(248, 250, 252);
        internal static readonly Color TextPrimary = Color.FromArgb(15, 23, 42);
        internal static readonly Color TextSecondary = Color.FromArgb(100, 116, 139);
        internal static readonly Color Border = Color.FromArgb(226, 232, 240);
        internal static readonly Color Primary = Color.FromArgb(37, 99, 235);
        internal static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);
        internal static readonly Color Accent = Color.FromArgb(13, 148, 136);
        internal static readonly Color Success = Color.FromArgb(5, 150, 105);
        internal static readonly Color Warning = Color.FromArgb(217, 119, 6);
        internal static readonly Color Danger = Color.FromArgb(220, 38, 38);
        internal static readonly Color Sidebar = Color.FromArgb(15, 23, 42);

        private static readonly HashSet<Form> ThemedForms = new HashSet<Form>();
        private static readonly HashSet<Control> HookedControls = new HashSet<Control>();
        private static bool globalThemeEnabled;
        private static Icon cachedAppIcon;

        public static void EnableGlobalTheme()
        {
            if (globalThemeEnabled) return;
            globalThemeEnabled = true;
            Application.Idle += Application_Idle;
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            for (int index = 0; index < Application.OpenForms.Count; index++)
            {
                Form form = Application.OpenForms[index];
                if (form == null || form.IsDisposed) continue;

                if (!ThemedForms.Contains(form))
                {
                    Apply(form);
                    ThemedForms.Add(form);
                    form.Disposed += Form_Disposed;
                }
            }
        }

        private static void DynamicControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control == null) return;
            StyleSingleControl(e.Control);
            HookControlTree(e.Control);
            if (e.Control.HasChildren)
                ApplyToControls(e.Control.Controls);

            Form embeddedForm = e.Control as Form;
            if (embeddedForm != null)
            {
                ApplyApplicationIcon(embeddedForm);
                embeddedForm.BackColor = AppBackground;
                embeddedForm.Font = new Font("Segoe UI", 9.5F);
                TryAttachSpecializedUi(embeddedForm);
            }
        }

        private static void Form_Disposed(object sender, EventArgs e)
        {
            Form form = sender as Form;
            if (form == null) return;
            ThemedForms.Remove(form);
            form.Disposed -= Form_Disposed;
        }

        public static void Apply(Form form)
        {
            if (form == null || form.IsDisposed) return;

            ApplyApplicationIcon(form);

            if (!(form is LoginForm))
            {
                form.BackColor = AppBackground;
                form.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
            }

            HookControlTree(form);
            ApplyToControls(form.Controls);
            TryAttachSpecializedUi(form);
        }

        private static void TryAttachSpecializedUi(Form form)
        {
            if (form is Pos_System.Forms.Sales)
                SalesAdvancedUiService.Attach(form);
        }

        private static void HookControlTree(Control control)
        {
            if (control == null || control.IsDisposed) return;

            if (!HookedControls.Contains(control))
            {
                HookedControls.Add(control);
                control.ControlAdded += DynamicControlAdded;
                control.Disposed += HookedControlDisposed;
            }

            foreach (Control child in control.Controls.Cast<Control>().ToList())
                HookControlTree(child);
        }

        private static void HookedControlDisposed(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control == null) return;
            HookedControls.Remove(control);
            control.ControlAdded -= DynamicControlAdded;
            control.Disposed -= HookedControlDisposed;
        }

        private static void ApplyApplicationIcon(Form form)
        {
            try
            {
                if (cachedAppIcon == null)
                {
                    string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BikeZonePOS.ico");
                    if (File.Exists(path))
                        cachedAppIcon = new Icon(path);
                }
                if (cachedAppIcon != null)
                    form.Icon = cachedAppIcon;
            }
            catch
            {
                // Branding icon is optional; never block the POS because an icon is unavailable.
            }
        }

        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls.Cast<Control>().ToList())
            {
                StyleSingleControl(control);
                HookControlTree(control);
                Form embeddedForm = control as Form;
                if (embeddedForm != null) TryAttachSpecializedUi(embeddedForm);
                if (control.HasChildren)
                    ApplyToControls(control.Controls);
            }
        }

        private static void StyleSingleControl(Control control)
        {
            if (control is DataGridView grid)
                StyleGrid(grid);
            else if (control is Button button)
                StyleButton(button);
            else if (control is TextBox textBox)
                StyleTextBox(textBox);
            else if (control is ComboBox comboBox)
                StyleComboBox(comboBox);
            else if (control is NumericUpDown numeric)
            {
                numeric.Font = new Font("Segoe UI", 10F);
                numeric.BackColor = Surface;
                numeric.ForeColor = TextPrimary;
                numeric.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is DateTimePicker picker)
            {
                picker.Font = new Font("Segoe UI", 9.5F);
                picker.CalendarForeColor = TextPrimary;
                picker.CalendarMonthBackground = Surface;
            }
            else if (control is Label label)
            {
                if (label.BackColor == Color.Black || label.BackColor == SystemColors.Control)
                    label.BackColor = Color.Transparent;
                if (label.ForeColor == Color.Black || label.ForeColor == SystemColors.ControlText)
                    label.ForeColor = TextPrimary;
                if (label.Font.Name != "Segoe UI")
                    label.Font = new Font("Segoe UI", Math.Max(9F, label.Font.Size), label.Font.Style);
            }
            else if (control is GroupBox groupBox)
            {
                groupBox.ForeColor = TextPrimary;
                groupBox.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            }
            else if (control is Panel panel)
            {
                if (panel.BackColor == SystemColors.Control || panel.BackColor == Color.Transparent)
                    panel.BackColor = Surface;
            }
            else if (control is TabControl tabs)
            {
                tabs.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                tabs.Padding = new Point(18, 7);
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.Font = new Font("Segoe UI", Math.Max(9F, checkBox.Font.Size));
                checkBox.ForeColor = TextPrimary;
                if (checkBox.BackColor == Color.Black || checkBox.BackColor == SystemColors.Control)
                    checkBox.BackColor = Color.Transparent;
            }
            else if (control is RadioButton radioButton)
            {
                radioButton.Font = new Font("Segoe UI", Math.Max(9F, radioButton.Font.Size));
                radioButton.ForeColor = TextPrimary;
                if (radioButton.BackColor == Color.Black || radioButton.BackColor == SystemColors.Control)
                    radioButton.BackColor = Color.Transparent;
            }
            else if (control is ToolStrip strip)
                StyleToolStrip(strip);
        }

        public static void StyleGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.RowHeadersVisible = false;
            grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            grid.RowTemplate.Height = 36;
            grid.ColumnHeadersHeight = 42;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 41, 59);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 41, 59);
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = TextPrimary;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
            grid.AlternatingRowsDefaultCellStyle.BackColor = SurfaceMuted;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        public static void StyleButton(Button button)
        {
            if (button.FlatStyle != FlatStyle.Flat)
                button.FlatStyle = FlatStyle.Flat;

            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            if (button.Font.Name != "Segoe UI")
                button.Font = new Font("Segoe UI", Math.Max(9F, button.Font.Size), FontStyle.Bold);

            string intent = ((button.Name ?? string.Empty) + " " + (button.Text ?? string.Empty)).ToLowerInvariant();
            if (intent.Contains("delete") || intent.Contains("remove") || intent.Contains("clear") ||
                intent.Contains("loss") || intent.Contains("cancel") || intent.Contains("void") || intent.Contains("مسح"))
                button.BackColor = Danger;
            else if (intent.Contains("save") || intent.Contains("add") || intent.Contains("record") ||
                     intent.Contains("start") || intent.Contains("open") || intent.Contains("دفع") || intent.Contains("اضافة"))
                button.BackColor = Success;
            else if (button.BackColor == SystemColors.Control || button.BackColor == Color.Transparent ||
                     button.BackColor == Color.White || button.BackColor == Color.Black)
                button.BackColor = Primary;

            button.ForeColor = Color.White;

            button.FlatAppearance.MouseOverBackColor = Lighten(button.BackColor, 0.08F);
            button.FlatAppearance.MouseDownBackColor = Darken(button.BackColor, 0.08F);
        }

        private static void StyleTextBox(TextBox textBox)
        {
            textBox.Font = new Font("Segoe UI", 10F);
            textBox.BackColor = Surface;
            textBox.ForeColor = TextPrimary;
            textBox.BorderStyle = BorderStyle.FixedSingle;
        }

        private static void StyleComboBox(ComboBox comboBox)
        {
            comboBox.Font = new Font("Segoe UI", 9.5F);
            comboBox.BackColor = Surface;
            comboBox.ForeColor = TextPrimary;
            if (comboBox.DropDownStyle == ComboBoxStyle.Simple)
                comboBox.DropDownStyle = ComboBoxStyle.DropDown;
        }

        private static void StyleToolStrip(ToolStrip strip)
        {
            strip.BackColor = Sidebar;
            strip.ForeColor = Color.White;
            strip.RenderMode = ToolStripRenderMode.System;
            strip.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            strip.Padding = new Padding(8, 4, 8, 4);
            foreach (ToolStripItem item in strip.Items)
            {
                item.ForeColor = Color.White;
                item.BackColor = Sidebar;
                item.Margin = new Padding(2);
                item.Padding = new Padding(8, 4, 8, 4);
            }
        }

        private static Color Lighten(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                Math.Min(255, color.R + (int)((255 - color.R) * factor)),
                Math.Min(255, color.G + (int)((255 - color.G) * factor)),
                Math.Min(255, color.B + (int)((255 - color.B) * factor)));
        }

        private static Color Darken(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                Math.Max(0, color.R - (int)(color.R * factor)),
                Math.Max(0, color.G - (int)(color.G * factor)),
                Math.Max(0, color.B - (int)(color.B * factor)));
        }
    }
}
