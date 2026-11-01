using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Services
{
    /// <summary>
    /// Replaces legacy Bike/Zone chrome with one Smart POS presentation.
    /// Functional controls (inputs, grids, buttons, charts) are kept and reparented.
    /// Old decorative panels are removed from the visible tree, not covered up.
    ///
    /// Covered windows:
    /// LoginForm, PosSystemDashboard, Dashboard, Sales, SalesStartForm,
    /// Products, ProductsPageForm, InventoryPageForm, AddProducts,
    /// Customers, CustomersPageForm, AddCustomers,
    /// SupplierForm, SuppliersPageForm, SupplierBalanceForm,
    /// CategoryForm, Settings, SettingsPageForm, DiscountSettingsForm, UserPermissionsForm,
    /// AddUsers, UsersPageForm, ReportsForm, ReportsOverviewPageForm,
    /// EarningReports, WarhouseReports, addsalesform, ActivityLogForm,
    /// AdminDashboardHomeForm, BalanceAdjustmentInvoiceForm, SimpleTablePageForm,
    /// plus the sales invoice and sale-review dialogs created inside Sales.
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
            form.Load += Form_Load;
            form.Disposed += Form_Disposed;
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
            if (name == "PosSystemDashboard")
            {
                Mounted.Add(form);
                return;
            }

            form.SuspendLayout();
            try
            {
                if (name == "LoginForm")
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

                form.Font = PosPalette.Ui(10F, FontStyle.Regular);
                form.BackColor = PosPalette.Canvas;
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
            form.ClientSize = new Size(860, 520);

            Panel root = new Panel { Dock = DockStyle.Fill, BackColor = PosPalette.Canvas };
            SetRole(root, "canvas");

            Panel brand = new Panel { Dock = DockStyle.Left, Width = 340, BackColor = PosPalette.Header };
            SetRole(brand, "header");
            brand.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                Font = PosPalette.Ui(14F, FontStyle.Regular),
                Text = "Sign in to the shop floor.\r\nSales, stock, and customers\r\nstay on this counter.",
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(28)
            });
            brand.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 160,
                ForeColor = Color.White,
                Font = PosPalette.Ui(28F, FontStyle.Bold),
                Text = "Smart POS",
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(28, 0, 28, 0)
            });

            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PosPalette.Surface,
                Padding = new Padding(48, 64, 48, 40)
            };
            SetRole(card, "surface");

            Label heading = new Label
            {
                Dock = DockStyle.Top,
                Height = 48,
                Text = "Welcome back",
                Font = PosPalette.Ui(22F, FontStyle.Bold),
                ForeColor = PosPalette.Ink
            };
            Label sub = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = "Use the account already stored on this register.",
                Font = PosPalette.Ui(10F, FontStyle.Regular),
                ForeColor = PosPalette.Muted
            };

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
                loginButton.Height = 44;
                loginButton.Dock = DockStyle.Top;
            }

            if (toggle is Button toggleButton)
            {
                toggleButton.Width = 44;
                toggleButton.Dock = DockStyle.Right;
                SetRole(toggleButton, "header");
            }

            Panel userRow = FieldRow("Username", username);
            Panel passRow = FieldRow("Password", password);
            if (toggle != null)
            {
                passRow.Controls.Add(toggle);
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
                Height = 72,
                BackColor = PosPalette.Surface,
                Padding = new Padding(0, 8, 0, 8)
            };
            Label label = new Label
            {
                Dock = DockStyle.Top,
                Height = 18,
                Text = caption,
                ForeColor = PosPalette.Muted,
                Font = PosPalette.Ui(9F, FontStyle.Bold)
            };
            if (input != null)
            {
                input.Dock = DockStyle.Top;
                input.Height = 32;
            }

            row.Controls.Add(input);
            row.Controls.Add(label);
            return row;
        }

        private static void MountSales(Form form)
        {
            Control search = Find(form, "txtsearch");
            Control quantity = Find(form, "txtquantity");
            Control category = Find(form, "cmbCategory");
            Control add = Find(form, "button5");
            Control products = Find(form, "pnlProducts");
            Control grid = Find(form, "datagridsales");
            Control customer = Find(form, "cmbCustomer");
            Control paid = Find(form, "txtPaidAmount");
            Control dollar = Find(form, "rbDollar");
            Control lebanon = Find(form, "rbLebanon");
            Control discount = Find(form, "chkUseDiscount");
            Control discountLabel = Find(form, "lblDiscountAmount");
            Control discountAmount = Find(form, "txtDiscountAmount");
            Control payment = Find(form, "comboPaymentMethod");
            Control saleId = Find(form, "txtSaleId");
            Control totalCaption = Find(form, "label9");
            Control total = Find(form, "lblTotal");
            Control liraCaption = Find(form, "label11");
            Control lira = Find(form, "lbtotal_lebanon");
            Control save = Find(form, "btnSaveSale");
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
                Width = 300,
                BackColor = PosPalette.Surface,
                Padding = new Padding(12)
            };
            SetRole(rail, "surface");
            Panel railStack = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = PosPalette.Surface };
            StackTop(railStack, delete, 40);
            StackTop(railStack, returns, 40);
            StackTop(railStack, invoice, 40);
            StackTop(railStack, excel, 40);
            StackTop(railStack, save, 46);
            StackTop(railStack, lira, 32);
            StackTop(railStack, liraCaption, 20);
            StackTop(railStack, total, 32);
            StackTop(railStack, totalCaption, 20);
            StackTop(railStack, saleId, 32);
            StackTop(railStack, payment, 32);
            StackTop(railStack, discountAmount, 32);
            StackTop(railStack, discountLabel, 20);
            StackTop(railStack, discount, 28);
            Panel currency = new Panel { Height = 32, BackColor = PosPalette.Surface };
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
            StackTop(railStack, customer, 32);
            rail.Controls.Add(railStack);

            Panel workspace = new Panel { Dock = DockStyle.Fill, BackColor = PosPalette.Canvas, Padding = new Padding(12) };
            Panel filters = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = PosPalette.Canvas, Padding = new Padding(0, 8, 0, 8) };
            if (quantity != null)
            {
                quantity.Dock = DockStyle.Right;
                quantity.Width = 72;
                filters.Controls.Add(quantity);
            }

            if (category != null)
            {
                category.Dock = DockStyle.Right;
                category.Width = 180;
                filters.Controls.Add(category);
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
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
            if (products != null)
            {
                products.Dock = DockStyle.Fill;
                products.BackColor = PosPalette.Canvas;
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
                Height = 56,
                BackColor = PosPalette.Header,
                Padding = new Padding(16, 0, 16, 0)
            };
            SetRole(header, "header");
            Label label = new Label
            {
                Dock = DockStyle.Fill,
                Text = title,
                ForeColor = Color.White,
                Font = PosPalette.Ui(18F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            SetRole(label, "header-text");
            header.Controls.Add(label);
            return header;
        }

        private static void ReplaceLegacyChrome(Form form)
        {
            RemoveBrandLabels(form);

            foreach (Panel panel in All(form).OfType<Panel>().ToList())
            {
                if (!IsLegacyHeader(panel))
                {
                    continue;
                }

                panel.BackColor = PosPalette.Header;
                panel.ForeColor = Color.White;
                SetRole(panel, "header");
                foreach (Control child in panel.Controls)
                {
                    if (child is Label label)
                    {
                        label.ForeColor = Color.White;
                        label.BackColor = Color.Transparent;
                        label.Font = PosPalette.Ui(label.Font.Size > 16F ? 16F : 12F, FontStyle.Bold);
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

                label.Parent?.Controls.Remove(label);
            }
        }

        private static bool IsBrand(string text)
        {
            string value = (text ?? string.Empty).Trim();
            return value.Equals("bike", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("zone", StringComparison.OrdinalIgnoreCase);
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

            if (control is TextBox || control is ComboBox || control is DateTimePicker || control is NumericUpDown || control is MaskedTextBox)
            {
                return "input";
            }

            if (control is DataGridView)
            {
                return "grid";
            }

            if (control is Label)
            {
                return ParentRole(control) == "header" ? "header-text" : "ink";
            }

            if (control is CheckBox || control is RadioButton || control is GroupBox)
            {
                return "ink";
            }

            if (control is Panel || control is FlowLayoutPanel || control is TableLayoutPanel)
            {
                if (IsLegacyHeader(control as Panel) || ParentRole(control) == "header")
                {
                    return "header";
                }

                return "surface";
            }

            if (control is MenuStrip || control is ToolStrip)
            {
                return "surface";
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

            if (ContainsAny(blob, "delete", "remove", "logout", "حذف", "مسح"))
            {
                return "danger";
            }

            if (ContainsAny(blob, "save", "add", "login", "signin", "start", "بيع", "اضافة", "إضافة", "حفظ"))
            {
                return "accent";
            }

            if (ContainsAny(blob, "excel", "export", "print", "طباعة"))
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

            switch (role)
            {
                case "canvas":
                    control.BackColor = PosPalette.Canvas;
                    control.ForeColor = PosPalette.Ink;
                    break;
                case "surface":
                case "sidebar":
                    control.BackColor = role == "sidebar" ? PosPalette.Sidebar : PosPalette.Surface;
                    control.ForeColor = PosPalette.Ink;
                    break;
                case "header":
                    control.BackColor = PosPalette.Header;
                    control.ForeColor = Color.White;
                    break;
                case "header-text":
                    control.ForeColor = Color.White;
                    control.BackColor = Color.Transparent;
                    break;
                case "ink":
                    control.ForeColor = PosPalette.Ink;
                    if (!(control is Button))
                    {
                        control.BackColor = Color.Transparent;
                    }
                    break;
                case "muted":
                    control.ForeColor = PosPalette.Muted;
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
                    if (control is Button button)
                    {
                        StyleActionButton(button, role);
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

            control.BackColor = PosPalette.Dark ? PosPalette.Canvas : Color.White;
            control.ForeColor = PosPalette.Ink;
            control.Font = PosPalette.Ui(10F, FontStyle.Regular);
            if (control is TextBox textBox)
            {
                textBox.BorderStyle = BorderStyle.FixedSingle;
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
            grid.GridColor = PosPalette.Line;
            grid.EnableHeadersVisualStyles = false;
            if (grid.ColumnHeadersHeightSizeMode == DataGridViewColumnHeadersHeightSizeMode.AutoSize)
            {
                grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            }

            grid.ColumnHeadersHeight = Math.Max(36, grid.ColumnHeadersHeight);
            grid.ColumnHeadersDefaultCellStyle.BackColor = PosPalette.Header;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = PosPalette.Ui(9.5F, FontStyle.Bold);
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = PosPalette.Header;
            grid.DefaultCellStyle.BackColor = PosPalette.Surface;
            grid.DefaultCellStyle.ForeColor = PosPalette.Ink;
            grid.DefaultCellStyle.Font = PosPalette.Ui(9.5F, FontStyle.Regular);
            grid.DefaultCellStyle.SelectionBackColor = PosPalette.Selection;
            grid.DefaultCellStyle.SelectionForeColor = PosPalette.Ink;
            grid.AlternatingRowsDefaultCellStyle.BackColor = PosPalette.Dark
                ? Color.FromArgb(56, 42, 34)
                : Color.FromArgb(248, 242, 234);
            grid.AlternatingRowsDefaultCellStyle.ForeColor = PosPalette.Ink;
            grid.RowHeadersVisible = false;
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
                    break;
                case "success":
                    button.BackColor = PosPalette.Success;
                    break;
                case "accent":
                    button.BackColor = PosPalette.Accent;
                    break;
                default:
                    button.BackColor = PosPalette.AccentDeep;
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
            button.ForeColor = active ? Color.White : PosPalette.SidebarInk;
            button.FlatAppearance.MouseOverBackColor = active ? PosPalette.AccentDeep : PosPalette.AccentSoft;
        }

        private static string CleanTitle(string text, string typeName)
        {
            if (string.IsNullOrWhiteSpace(text) ||
                text.Equals("Form1", StringComparison.OrdinalIgnoreCase) ||
                text.IndexOf("PosSystem", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.EndsWith("Form", StringComparison.OrdinalIgnoreCase) ||
                text.IndexOf("form", StringComparison.OrdinalIgnoreCase) >= 0)
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
                case "Dashboard":
                case "AdminDashboardHomeForm":
                    return "Dashboard";
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
        }
    }
}
