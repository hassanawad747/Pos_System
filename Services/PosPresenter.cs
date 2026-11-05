using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Pos_System.Services
{
    /// <summary>
    /// Replaces legacy Bike/Zone chrome with one readable Smart POS presentation.
    /// Functional controls are kept. Decorative leftovers are removed, not covered.
    /// Applies to every Form in Forms/.
    /// </summary>
    internal static class PosPresenter
    {
        private static readonly HashSet<Form> Watched = new HashSet<Form>();
        private static readonly HashSet<Form> Mounted = new HashSet<Form>();
        private static readonly Dictionary<Control, string> Roles = new Dictionary<Control, string>();

        public static void Watch(Form form)
        {
            if (form == null || Watched.Contains(form))
            {
                return;
            }

            Watched.Add(form);
            form.HandleCreated += Form_HandleCreated;
            form.Load += Form_Load;
            form.Disposed += Form_Disposed;
            if (form.IsHandleCreated)
            {
                Mount(form);
            }
        }

        public static bool IsMounted(Form form)
        {
            return form != null && Mounted.Contains(form);
        }

        public static void ApplyTheme(Form form, string theme)
        {
            PosPalette.Dark = string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase);
            if (IsMounted(form))
            {
                Recolor(form);
            }
        }

        public static void SetRole(Control control, string role)
        {
            if (control == null || string.IsNullOrEmpty(role))
            {
                return;
            }

            Roles[control] = role;
            ApplyRole(control, role);
        }

        public static ToolStripRenderer CreateRenderer()
        {
            return new SmartToolStripRenderer();
        }

        private static void Form_HandleCreated(object sender, EventArgs e)
        {
            Mount(sender as Form);
        }

        private static void Form_Load(object sender, EventArgs e)
        {
            Mount(sender as Form);
        }

        private static void Form_Disposed(object sender, EventArgs e)
        {
            Form form = sender as Form;
            if (form == null)
            {
                return;
            }

            Watched.Remove(form);
            Mounted.Remove(form);
            form.HandleCreated -= Form_HandleCreated;
            form.Load -= Form_Load;
            form.Disposed -= Form_Disposed;
        }

        private static void Mount(Form form)
        {
            if (form == null || form.IsDisposed)
            {
                return;
            }

            PosPalette.Dark = string.Equals(
                POS_System.Program.SettingsManager.GetSetting("theme", "default"),
                "dark",
                StringComparison.OrdinalIgnoreCase);

            if (Mounted.Contains(form))
            {
                Recolor(form);
                return;
            }

            string name = form.GetType().Name;
            form.SuspendLayout();
            try
            {
                form.Font = PosPalette.Ui(10F, FontStyle.Regular);
                if (name == "PosSystemDashboard")
                {
                    RestyleTree(form);
                }
                else if (name == "LoginForm")
                {
                    MountLogin(form);
                }
                else if (name == "Sales")
                {
                    MountSales(form);
                }
                else
                {
                    ReplaceLegacyChrome(form);
                    RestyleTree(form);
                }

                form.BackColor = PosPalette.Canvas;
                form.ForeColor = PosPalette.Ink;
                Mounted.Add(form);
            }
            finally
            {
                form.ResumeLayout(true);
            }
        }

        private static void MountLogin(Form form)
        {
            Control username = Find(form, "txtusername");
            Control password = Find(form, "txtpassword");
            Control login = Find(form, "btnlogin");
            Control toggle = Find(form, "btnTogglePassword");
            RemoveBrandLabels(form);

            form.Controls.Clear();
            form.Text = "Smart POS";
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ClientSize = new Size(900, 540);
            form.BackColor = PosPalette.Canvas;

            Panel root = new Panel { Dock = DockStyle.Fill, BackColor = PosPalette.Canvas };
            SetRole(root, "canvas");

            Panel brand = new Panel { Dock = DockStyle.Left, Width = 360, BackColor = PosPalette.Sidebar };
            SetRole(brand, "sidebar");
            Label brandBody = new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = PosPalette.SidebarMuted,
                Font = PosPalette.Ui(12F, FontStyle.Regular),
                Text = "Sales, inventory, customers, and reports\r\nstay on this register.\r\n\r\nالمبيعات والمخزون والزبائن على هذا الصندوق.",
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(32, 16, 32, 36)
            };
            SetRole(brandBody, "sidebar-muted");
            Label brandTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 170,
                ForeColor = Color.White,
                Font = PosPalette.Ui(30F, FontStyle.Bold),
                Text = "Smart POS",
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(32, 0, 32, 8)
            };
            SetRole(brandTitle, "sidebar-text");
            Label brandMark = new Label
            {
                Dock = DockStyle.Top,
                Height = 56,
                ForeColor = Color.White,
                Font = PosPalette.Ui(11F, FontStyle.Bold),
                Text = "POINT OF SALE",
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(32, 28, 32, 0)
            };
            SetRole(brandMark, "sidebar-muted");
            brand.Controls.Add(brandBody);
            brand.Controls.Add(brandTitle);
            brand.Controls.Add(brandMark);

            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PosPalette.Surface,
                Padding = new Padding(52, 72, 52, 40)
            };
            SetRole(card, "surface");

            Label heading = new Label
            {
                Dock = DockStyle.Top,
                Height = 46,
                Text = "Sign in",
                Font = PosPalette.Ui(24F, FontStyle.Bold),
                ForeColor = PosPalette.Ink
            };
            SetRole(heading, "ink");
            Label sub = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                Text = "Use the account stored on this register.",
                Font = PosPalette.Ui(10.5F, FontStyle.Regular),
                ForeColor = PosPalette.Muted
            };
            SetRole(sub, "muted");

            if (username is TextBox usernameBox)
            {
                usernameBox.Multiline = false;
            }

            if (password is TextBox passwordBox)
            {
                passwordBox.Multiline = false;
            }

            StyleInput(username);
            StyleInput(password);
            if (login is Button loginButton)
            {
                loginButton.Text = "Sign in";
                SetRole(loginButton, "accent");
                loginButton.Height = 46;
                loginButton.Dock = DockStyle.Top;
            }

            if (toggle is Button toggleButton)
            {
                toggleButton.Width = 46;
                toggleButton.Dock = DockStyle.Right;
                SetRole(toggleButton, "secondary");
            }

            Panel userRow = FieldRow("Username", username);
            Panel passRow = FieldRow("Password", password);
            if (toggle != null)
            {
                passRow.Controls.Add(toggle);
                toggle.BringToFront();
            }

            card.Controls.Add(login);
            card.Controls.Add(passRow);
            card.Controls.Add(userRow);
            card.Controls.Add(sub);
            card.Controls.Add(heading);

            root.Controls.Add(card);
            root.Controls.Add(brand);
            form.Controls.Add(root);
        }

        private static Panel FieldRow(string caption, Control input)
        {
            Panel row = new Panel
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = PosPalette.Surface,
                Padding = new Padding(0, 8, 0, 8)
            };
            SetRole(row, "surface");
            Label label = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                Text = caption,
                ForeColor = PosPalette.Muted,
                Font = PosPalette.Ui(9F, FontStyle.Bold)
            };
            SetRole(label, "muted");
            if (input != null)
            {
                input.Dock = DockStyle.Top;
                input.Height = 34;
            }

            row.Controls.Add(input);
            row.Controls.Add(label);
            return row;
        }

        private static void MountSales(Form form)
        {
            Control search = Find(form, "txtsearch");
            Control quantity = Find(form, "txtquantity");
            Control quantityLabel = Find(form, "label8");
            Control category = Find(form, "cmbCategory");
            Control add = Find(form, "button5");
            Control products = Find(form, "pnlProducts");
            Control grid = Find(form, "datagridsales");
            Control customerLabel = Find(form, "label5");
            Control customer = Find(form, "cmbCustomer");
            Control paidLabel = Find(form, "label6");
            Control paid = Find(form, "txtPaidAmount");
            Control dollar = Find(form, "rbDollar");
            Control lebanon = Find(form, "rbLebanon");
            Control discount = Find(form, "chkUseDiscount");
            Control discountLabel = Find(form, "lblDiscountAmount");
            Control discountAmount = Find(form, "txtDiscountAmount");
            Control paymentLabel = Find(form, "label7");
            Control payment = Find(form, "comboPaymentMethod");
            Control saleIdLabel = Find(form, "label10");
            Control saleId = Find(form, "txtSaleId");
            Control totalCaption = Find(form, "label9");
            Control total = Find(form, "lblTotal");
            Control liraCaption = Find(form, "label11");
            Control lira = Find(form, "lbtotal_lebanon");
            Control rateLabel = Find(form, "label4");
            Control rate = Find(form, "txtdollar");
            Control rateSave = Find(form, "btndollar");
            Control save = Find(form, "btnSaveSale");
            Control quick = Find(form, "btnsare3");
            Control excel = Find(form, "btnexcel");
            Control invoice = Find(form, "btn3rdfetora");
            Control returns = Find(form, "btnmortaja3");
            Control delete = Find(form, "btndelete");

            RemoveBrandLabels(form);
            form.Controls.Clear();
            form.Text = "Sales";
            form.BackColor = PosPalette.Canvas;

            Panel root = new Panel { Dock = DockStyle.Fill, BackColor = PosPalette.Canvas };
            SetRole(root, "canvas");

            Panel rail = new Panel
            {
                Dock = DockStyle.Right,
                Width = 320,
                BackColor = PosPalette.Surface,
                Padding = new Padding(14)
            };
            SetRole(rail, "surface");
            Panel railStack = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = PosPalette.Surface };
            SetRole(railStack, "surface");

            StackTop(railStack, delete, 42);
            StackTop(railStack, returns, 42);
            StackTop(railStack, invoice, 42);
            StackTop(railStack, excel, 42);
            if (quick != null && quick.Visible)
            {
                StackTop(railStack, quick, 42);
            }

            StackTop(railStack, save, 48);
            StackTop(railStack, lira, 30);
            StackTop(railStack, liraCaption, 22);
            StackTop(railStack, total, 30);
            StackTop(railStack, totalCaption, 22);
            StackTop(railStack, saleId, 32);
            StackCaption(railStack, saleIdLabel);
            StackTop(railStack, payment, 32);
            StackCaption(railStack, paymentLabel);
            StackTop(railStack, discountAmount, 32);
            StackCaption(railStack, discountLabel);
            StackTop(railStack, discount, 28);
            Panel currency = new Panel { Height = 32, BackColor = PosPalette.Surface };
            SetRole(currency, "surface");
            if (dollar != null)
            {
                dollar.Dock = DockStyle.Left;
                dollar.Width = 120;
                currency.Controls.Add(dollar);
            }

            if (lebanon != null)
            {
                lebanon.Dock = DockStyle.Fill;
                currency.Controls.Add(lebanon);
            }

            StackTop(railStack, currency, 32);
            StackTop(railStack, paid, 32);
            StackCaption(railStack, paidLabel);
            StackTop(railStack, customer, 32);
            StackCaption(railStack, customerLabel);

            Panel rateRow = new Panel { Height = 36, BackColor = PosPalette.Surface };
            SetRole(rateRow, "surface");
            if (rateSave != null)
            {
                rateSave.Dock = DockStyle.Right;
                rateSave.Width = 72;
                rateRow.Controls.Add(rateSave);
            }

            if (rate != null)
            {
                rate.Dock = DockStyle.Fill;
                rateRow.Controls.Add(rate);
            }

            StackTop(railStack, rateRow, 36);
            StackCaption(railStack, rateLabel);
            rail.Controls.Add(railStack);

            Panel workspace = new Panel { Dock = DockStyle.Fill, BackColor = PosPalette.Canvas, Padding = new Padding(12) };
            SetRole(workspace, "canvas");
            Panel filters = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = PosPalette.Surface, Padding = new Padding(10, 10, 10, 10) };
            SetRole(filters, "surface");
            if (category != null)
            {
                category.Dock = DockStyle.Right;
                category.Width = 170;
                filters.Controls.Add(category);
            }

            if (quantity != null)
            {
                quantity.Dock = DockStyle.Right;
                quantity.Width = 72;
                filters.Controls.Add(quantity);
            }

            if (quantityLabel != null)
            {
                quantityLabel.Dock = DockStyle.Right;
                quantityLabel.AutoSize = false;
                quantityLabel.Width = 88;
                quantityLabel.TextAlign = ContentAlignment.MiddleRight;
                filters.Controls.Add(quantityLabel);
            }

            if (add != null)
            {
                add.Dock = DockStyle.Right;
                add.Width = 96;
                filters.Controls.Add(add);
            }

            if (search != null)
            {
                search.Dock = DockStyle.Fill;
                filters.Controls.Add(search);
            }

            TableLayoutPanel body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = PosPalette.Canvas
            };
            SetRole(body, "canvas");
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            if (products != null)
            {
                products.Dock = DockStyle.Fill;
                products.BackColor = PosPalette.Canvas;
                SetRole(products, "canvas");
                body.Controls.Add(products, 0, 0);
            }

            if (grid != null)
            {
                grid.Dock = DockStyle.Fill;
                body.Controls.Add(grid, 0, 1);
            }

            workspace.Controls.Add(body);
            workspace.Controls.Add(filters);

            root.Controls.Add(workspace);
            root.Controls.Add(rail);
            if (form.TopLevel)
            {
                root.Controls.Add(HeaderBar("Sales"));
            }

            form.Controls.Add(root);

            if (products != null)
            {
                products.ControlAdded -= ProductHost_ControlAdded;
                products.ControlAdded += ProductHost_ControlAdded;
                foreach (Control child in products.Controls)
                {
                    if (child is Button productButton)
                    {
                        StyleProductButton(productButton);
                    }
                }
            }

            RestyleTree(form);
        }

        private static void StackCaption(Control parent, Control label)
        {
            if (label is Label caption)
            {
                caption.AutoSize = false;
                caption.TextAlign = ContentAlignment.MiddleLeft;
                SetRole(caption, "muted");
            }

            StackTop(parent, label, 20);
        }

        private static void ProductHost_ControlAdded(object sender, ControlEventArgs e)
        {
            if (e.Control is Button button)
            {
                StyleProductButton(button);
            }
        }

        public static void StyleProductButton(Button button)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = PosPalette.Line;
            button.UseVisualStyleBackColor = false;
            button.BackColor = PosPalette.Surface;
            button.ForeColor = PosPalette.Ink;
            button.Font = PosPalette.Ui(9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.TextAlign = ContentAlignment.MiddleCenter;
        }

        private static void StackTop(Control parent, Control child, int height)
        {
            if (child == null)
            {
                return;
            }

            child.Dock = DockStyle.Top;
            child.Height = height;
            child.Margin = new Padding(0, 4, 0, 4);
            parent.Controls.Add(child);
            child.BringToFront();
        }

        private static Panel HeaderBar(string title)
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = PosPalette.Header,
                Padding = new Padding(16, 0, 16, 0)
            };
            SetRole(header, "header");
            Label label = new Label
            {
                Dock = DockStyle.Fill,
                Text = title,
                ForeColor = Color.White,
                Font = PosPalette.Ui(16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            SetRole(label, "header-text");
            header.Controls.Add(label);
            return header;
        }

        private static void ReplaceLegacyChrome(Form form)
        {
            RemoveBrandLabels(form);
            NormalizeFonts(form);

            foreach (Panel panel in All(form).OfType<Panel>().ToList())
            {
                if (!IsLegacyHeader(panel))
                {
                    continue;
                }

                if (!form.TopLevel)
                {
                    panel.Visible = false;
                    continue;
                }

                panel.BackColor = PosPalette.Header;
                panel.ForeColor = Color.White;
                panel.Height = Math.Min(panel.Height, 56);
                SetRole(panel, "header");
                foreach (Control child in panel.Controls)
                {
                    if (child is Label label)
                    {
                        if (IsBrand(label.Text))
                        {
                            label.Visible = false;
                            continue;
                        }

                        label.ForeColor = Color.White;
                        label.BackColor = Color.Transparent;
                        label.Font = PosPalette.Ui(label.Font.Size > 14F ? 16F : 11F, FontStyle.Bold);
                        SetRole(label, "header-text");
                    }
                }
            }

            string cleaned = CleanTitle(form.Text, form.GetType().Name);
            if (!string.IsNullOrWhiteSpace(cleaned))
            {
                form.Text = cleaned;
            }
        }

        private static bool IsLegacyHeader(Panel panel)
        {
            if (panel == null)
            {
                return false;
            }

            if (string.Equals(panel.Name, "panelheader", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return panel.Dock == DockStyle.Top &&
                   panel.Height > 0 &&
                   panel.Height <= 120 &&
                   IsDark(panel.BackColor);
        }

        private static void RemoveBrandLabels(Form form)
        {
            foreach (Label label in All(form).OfType<Label>().ToList())
            {
                if (!IsBrand(label.Text))
                {
                    continue;
                }

                label.Visible = false;
                label.Text = string.Empty;
                label.Parent?.Controls.Remove(label);
            }
        }

        private static bool IsBrand(string text)
        {
            string value = (text ?? string.Empty).Trim();
            return value.Equals("bike", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("zone", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("bike zone", StringComparison.OrdinalIgnoreCase);
        }

        private static void NormalizeFonts(Control root)
        {
            foreach (Control control in All(root))
            {
                ApplyUiFont(control);
            }

            ApplyUiFont(root);
        }

        private static void ApplyUiFont(Control control)
        {
            if (control == null || control is PictureBox || control.Font == null)
            {
                return;
            }

            string family = control.Font.FontFamily.Name;
            if (family.Equals("Segoe UI", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            float size = control.Font.Size;
            if (size < 8F)
            {
                size = 9F;
            }
            else if (size > 28F)
            {
                size = 22F;
            }

            control.Font = PosPalette.Ui(size, control.Font.Style);
        }

        private static void RestyleTree(Control root)
        {
            if (root == null)
            {
                return;
            }

            if (!Roles.ContainsKey(root))
            {
                string inferred = InferRole(root);
                if (!string.IsNullOrEmpty(inferred))
                {
                    Roles[root] = inferred;
                    ApplyRole(root, inferred);
                }
            }
            else
            {
                ApplyRole(root, Roles[root]);
            }

            ApplyUiFont(root);

            foreach (Control child in root.Controls.Cast<Control>().ToList())
            {
                RestyleTree(child);
            }
        }

        private static void Recolor(Form form)
        {
            if (form == null || form.IsDisposed)
            {
                return;
            }

            form.SuspendLayout();
            try
            {
                RestyleTree(form);
            }
            finally
            {
                form.ResumeLayout(true);
            }
        }

        private static string InferRole(Control control)
        {
            if (control is Form)
            {
                return "canvas";
            }

            if (control is Button button)
            {
                return ClassifyButton(button);
            }

            if (control is TextBox || control is ComboBox || control is DateTimePicker ||
                control is NumericUpDown || control is MaskedTextBox || control is RichTextBox)
            {
                return "input";
            }

            if (control is DataGridView)
            {
                return "grid";
            }

            if (control is Chart)
            {
                return "chart";
            }

            if (control is Label)
            {
                string parent = ParentRole(control);
                if (parent == "header")
                {
                    return "header-text";
                }

                if (parent == "sidebar")
                {
                    return "sidebar-text";
                }

                return "ink";
            }

            if (control is CheckBox || control is RadioButton || control is GroupBox || control is LinkLabel)
            {
                string parent = ParentRole(control);
                return parent == "sidebar" ? "sidebar-text" : "ink";
            }

            if (control is Panel || control is FlowLayoutPanel || control is TableLayoutPanel || control is SplitContainer)
            {
                if (IsLegacyHeader(control as Panel) || ParentRole(control) == "header")
                {
                    return "header";
                }

                if (ParentRole(control) == "sidebar")
                {
                    return "sidebar";
                }

                if (control is Panel panel &&
                    (string.Equals(panel.Name, "panel1", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(panel.Name, "panelContent", StringComparison.OrdinalIgnoreCase)))
                {
                    return panel.Dock == DockStyle.Left ? "sidebar" : "canvas";
                }

                return "surface";
            }

            if (control is MenuStrip || control is ToolStrip || control is StatusStrip)
            {
                return "surface";
            }

            if (control is TabControl || control is TabPage)
            {
                return "surface";
            }

            if (control is ListBox || control is ListView || control is TreeView)
            {
                return "input";
            }

            return null;
        }

        private static string ParentRole(Control control)
        {
            Control parent = control?.Parent;
            while (parent != null)
            {
                if (Roles.TryGetValue(parent, out string role))
                {
                    return role;
                }

                parent = parent.Parent;
            }

            return null;
        }

        private static string ClassifyButton(Button button)
        {
            string name = (button.Name ?? string.Empty).ToLowerInvariant();
            string text = (button.Text ?? string.Empty).Trim().ToLowerInvariant();
            string blob = name + " " + text;

            if (ContainsAny(blob, "delete", "remove", "logout", "log out", "حذف", "مسح"))
            {
                return "danger";
            }

            if (ContainsAny(blob, "save", "add", "login", "signin", "sign in", "start", "بيع", "اضافة", "إضافة", "حفظ"))
            {
                return "accent";
            }

            if (ContainsAny(blob, "excel", "export", "print", "طباعة", "طبع"))
            {
                return "success";
            }

            return "secondary";
        }

        private static bool ContainsAny(string blob, params string[] tokens)
        {
            foreach (string token in tokens)
            {
                if (blob.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyRole(Control control, string role)
        {
            if (control == null || control.IsDisposed)
            {
                return;
            }

            string parent = ParentRole(control);
            switch (role)
            {
                case "canvas":
                    control.BackColor = PosPalette.Canvas;
                    control.ForeColor = PosPalette.Ink;
                    break;
                case "surface":
                    control.BackColor = PosPalette.Surface;
                    control.ForeColor = PosPalette.Ink;
                    break;
                case "sidebar":
                    control.BackColor = PosPalette.Sidebar;
                    control.ForeColor = PosPalette.SidebarInk;
                    break;
                case "header":
                    control.BackColor = PosPalette.Header;
                    control.ForeColor = Color.White;
                    break;
                case "header-text":
                    control.ForeColor = Color.White;
                    control.BackColor = Color.Transparent;
                    break;
                case "sidebar-text":
                    control.ForeColor = PosPalette.SidebarInk;
                    control.BackColor = Color.Transparent;
                    break;
                case "sidebar-muted":
                    control.ForeColor = PosPalette.SidebarMuted;
                    control.BackColor = Color.Transparent;
                    break;
                case "ink":
                    control.ForeColor = parent == "sidebar" ? PosPalette.SidebarInk : PosPalette.Ink;
                    if (!(control is Button))
                    {
                        control.BackColor = Color.Transparent;
                    }
                    break;
                case "muted":
                    control.ForeColor = parent == "sidebar" ? PosPalette.SidebarMuted : PosPalette.Muted;
                    control.BackColor = Color.Transparent;
                    break;
                case "input":
                    StyleInput(control);
                    break;
                case "grid":
                    if (control is DataGridView grid)
                    {
                        StyleGrid(grid);
                    }
                    break;
                case "chart":
                    if (control is Chart chart)
                    {
                        StyleChart(chart);
                    }
                    break;
                case "nav":
                    StyleNavButton(control as Button, false);
                    break;
                case "nav-active":
                    StyleNavButton(control as Button, true);
                    break;
                case "product":
                    if (control is Button product)
                    {
                        StyleProductButton(product);
                    }
                    break;
                default:
                    if (control is Button action)
                    {
                        StyleActionButton(action, role);
                    }
                    break;
            }
        }

        private static void StyleInput(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.BackColor = PosPalette.InputFill;
            control.ForeColor = PosPalette.Ink;
            control.Font = PosPalette.Ui(10F, FontStyle.Regular);
            if (control is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
            }

            if (control is ComboBox combo)
            {
                combo.FlatStyle = FlatStyle.Flat;
            }
        }

        public static void StyleGrid(DataGridView grid)
        {
            if (grid == null)
            {
                return;
            }

            grid.BackgroundColor = PosPalette.Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = PosPalette.Line;
            grid.EnableHeadersVisualStyles = false;
            if (grid.ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.AutoSize)
            {
                grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            }

            grid.ColumnHeadersHeight = Math.Max(36, grid.ColumnHeadersHeight);
            grid.ColumnHeadersDefaultCellStyle.BackColor = PosPalette.Sidebar;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = PosPalette.Ui(9.5F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = PosPalette.Sidebar;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.BackColor = PosPalette.Surface;
            grid.DefaultCellStyle.ForeColor = PosPalette.Ink;
            grid.DefaultCellStyle.Font = PosPalette.Ui(9.5F, FontStyle.Regular);
            grid.DefaultCellStyle.SelectionBackColor = PosPalette.Selection;
            grid.DefaultCellStyle.SelectionForeColor = PosPalette.Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = PosPalette.Dark
                ? Color.FromArgb(30, 41, 59)
                : Color.FromArgb(248, 250, 252);
            grid.AlternatingRowsDefaultCellStyle.ForeColor = PosPalette.Ink;
            grid.RowHeadersVisible = false;
            grid.RowTemplate.Height = 32;
        }

        public static void StyleChart(Chart chart)
        {
            if (chart == null)
            {
                return;
            }

            chart.BackColor = PosPalette.Surface;
            chart.ForeColor = PosPalette.Ink;
            foreach (ChartArea area in chart.ChartAreas)
            {
                area.BackColor = PosPalette.Surface;
                area.AxisX.MajorGrid.LineColor = PosPalette.Line;
                area.AxisY.MajorGrid.LineColor = PosPalette.Line;
                area.AxisX.LineColor = PosPalette.Line;
                area.AxisY.LineColor = PosPalette.Line;
                area.AxisX.LabelStyle.ForeColor = PosPalette.Muted;
                area.AxisY.LabelStyle.ForeColor = PosPalette.Muted;
                area.AxisX.TitleForeColor = PosPalette.Ink;
                area.AxisY.TitleForeColor = PosPalette.Ink;
            }

            foreach (Legend legend in chart.Legends)
            {
                legend.BackColor = PosPalette.Surface;
                legend.ForeColor = PosPalette.Ink;
            }

            foreach (Title title in chart.Titles)
            {
                title.ForeColor = PosPalette.Ink;
                title.Font = PosPalette.Ui(11F, FontStyle.Bold);
            }
        }

        private static void StyleActionButton(Button button, string role)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.ForeColor = Color.White;
            button.Font = PosPalette.Ui(9.5F, FontStyle.Bold);
            switch (role)
            {
                case "danger":
                    button.BackColor = PosPalette.Danger;
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(185, 28, 28);
                    break;
                case "success":
                    button.BackColor = PosPalette.Success;
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(4, 120, 87);
                    break;
                case "accent":
                    button.BackColor = PosPalette.Accent;
                    button.FlatAppearance.MouseOverBackColor = PosPalette.AccentDeep;
                    break;
                default:
                    button.BackColor = PosPalette.Secondary;
                    button.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59);
                    break;
            }
        }

        private static void StyleNavButton(Button button, bool active)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(14, 0, 8, 0);
            button.Font = PosPalette.Ui(10.5F, active ? FontStyle.Bold : FontStyle.Regular);
            button.BackColor = active ? PosPalette.Accent : PosPalette.Sidebar;
            button.ForeColor = Color.White;
            button.FlatAppearance.MouseOverBackColor = active ? PosPalette.AccentDeep : PosPalette.NavHover;
        }

        private static string CleanTitle(string text, string typeName)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                text.Equals("Form1", StringComparison.OrdinalIgnoreCase) ||
                text.IndexOf("PosSystem", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.EndsWith("Form", StringComparison.OrdinalIgnoreCase) ||
                text.IndexOf("form", StringComparison.OrdinalIgnoreCase) >= 0 ||
                IsBrand(text))
            {
                return FriendlyTypeName(typeName);
            }

            return text;
        }

        private static string FriendlyTypeName(string typeName)
        {
            switch (typeName)
            {
                case "Products":
                case "ProductsPageForm":
                case "InventoryPageForm":
                    return "Inventory";
                case "Customers":
                case "CustomersPageForm":
                    return "Customers";
                case "AddCustomers":
                    return "Customer";
                case "AddProducts":
                    return "Product";
                case "AddUsers":
                case "UsersPageForm":
                    return "Users";
                case "SupplierForm":
                case "SuppliersPageForm":
                case "SupplierBalanceForm":
                    return "Suppliers";
                case "CategoryForm":
                    return "Categories";
                case "Settings":
                case "SettingsPageForm":
                    return "Settings";
                case "ReportsForm":
                case "ReportsOverviewPageForm":
                    return "Reports";
                case "EarningReports":
                    return "Earnings";
                case "WarhouseReports":
                    return "Warehouse";
                case "addsalesform":
                    return "Add sale";
                case "SalesStartForm":
                    return "Start work";
                case "DiscountSettingsForm":
                    return "Discounts";
                case "UserPermissionsForm":
                    return "Permissions";
                case "ActivityLogForm":
                    return "Activity";
                case "BalanceAdjustmentInvoiceForm":
                    return "Balance invoice";
                case "SimpleTablePageForm":
                    return "Records";
                case "ExchangeForm":
                    return "Exchange";
                case "Dashboard":
                case "AdminDashboardHomeForm":
                    return "Dashboard";
                case "LoginForm":
                    return "Smart POS";
                default:
                    return "Smart POS";
            }
        }

        private static bool IsDark(Color color)
        {
            return color.GetBrightness() < 0.35F && color.A > 200;
        }

        private static Control Find(Control root, string name)
        {
            foreach (Control control in All(root))
            {
                if (string.Equals(control.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return control;
                }
            }

            return null;
        }

        private static IEnumerable<Control> All(Control root)
        {
            if (root == null)
            {
                yield break;
            }

            foreach (Control child in root.Controls.Cast<Control>().ToList())
            {
                yield return child;
                foreach (Control nested in All(child))
                {
                    yield return nested;
                }
            }
        }

        private sealed class SmartToolStripRenderer : ToolStripProfessionalRenderer
        {
            public SmartToolStripRenderer()
                : base(new SmartColorTable())
            {
                RoundedEdges = false;
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = PosPalette.Ink;
                if (string.Equals(e.Item?.Text, "Logout", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(e.Item?.Text, "Log out", StringComparison.OrdinalIgnoreCase))
                {
                    e.TextColor = Color.White;
                }

                base.OnRenderItemText(e);
            }
        }

        private sealed class SmartColorTable : ProfessionalColorTable
        {
            public override Color ToolStripGradientBegin => PosPalette.Surface;
            public override Color ToolStripGradientMiddle => PosPalette.Surface;
            public override Color ToolStripGradientEnd => PosPalette.Surface;
            public override Color MenuStripGradientBegin => PosPalette.Surface;
            public override Color MenuStripGradientEnd => PosPalette.Surface;
            public override Color ToolStripBorder => PosPalette.Line;
            public override Color ButtonSelectedGradientBegin => PosPalette.AccentSoft;
            public override Color ButtonSelectedGradientEnd => PosPalette.AccentSoft;
            public override Color ButtonPressedGradientBegin => PosPalette.Accent;
            public override Color ButtonPressedGradientEnd => PosPalette.Accent;
            public override Color SeparatorDark => PosPalette.Line;
            public override Color SeparatorLight => PosPalette.Surface;
            public override Color MenuItemSelected => PosPalette.AccentSoft;
            public override Color MenuItemSelectedGradientBegin => PosPalette.AccentSoft;
            public override Color MenuItemSelectedGradientEnd => PosPalette.AccentSoft;
        }
    }
}
