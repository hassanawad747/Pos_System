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
                    // Shell is built in PosSystemDashboard; it calls SetRole itself.
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
            Control usernameLabel = Find(form, "label1");
            Control passwordLabel = Find(form, "label2");
            Control brandTitle = Find(form, "label3");
            Control brandMark = Find(form, "label4");
            Control error = Find(form, "lblError");
            Control exit = Find(form, "btnExit");
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
                Name = "lblBrandBody",
                Dock = DockStyle.Fill,
                ForeColor = PosPalette.SidebarMuted,
                Font = PosPalette.Ui(12F, FontStyle.Regular),
                Text = "Sales, inventory, customers, and reports\r\nstay on this register.\r\n\r\nالمبيعات والمخزون والزبائن على هذا الصندوق.",
                TextAlign = ContentAlignment.BottomLeft,
                Padding = new Padding(32, 16, 32, 36)
            };
            SetRole(brandBody, "sidebar-muted");

            if (brandTitle is Label titleLabel)
            {
                titleLabel.AutoSize = false;
                titleLabel.Dock = DockStyle.Top;
                titleLabel.Height = 170;
                titleLabel.ForeColor = Color.White;
                titleLabel.Font = PosPalette.Ui(30F, FontStyle.Bold);
                titleLabel.Text = "Smart POS";
                titleLabel.TextAlign = ContentAlignment.BottomLeft;
                titleLabel.Padding = new Padding(32, 0, 32, 8);
                SetRole(titleLabel, "sidebar-text");
            }

            if (brandMark is Label markLabel)
            {
                markLabel.AutoSize = false;
                markLabel.Dock = DockStyle.Top;
                markLabel.Height = 56;
                markLabel.ForeColor = PosPalette.SidebarMuted;
                markLabel.Font = PosPalette.Ui(11F, FontStyle.Bold);
                markLabel.Text = "POINT OF SALE";
                markLabel.TextAlign = ContentAlignment.BottomLeft;
                markLabel.Padding = new Padding(32, 28, 32, 0);
                SetRole(markLabel, "sidebar-muted");
            }

            brand.Controls.Add(brandBody);
            if (brandTitle != null)
            {
                brand.Controls.Add(brandTitle);
            }

            if (brandMark != null)
            {
                brand.Controls.Add(brandMark);
            }

            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = PosPalette.Surface,
                Padding = new Padding(52, 64, 52, 36)
            };
            SetRole(card, "surface");

            Label heading = new Label
            {
                Name = "lblSignInHeading",
                Dock = DockStyle.Top,
                Height = 46,
                Text = "Sign in",
                Font = PosPalette.Ui(24F, FontStyle.Bold),
                ForeColor = PosPalette.Ink
            };
            SetRole(heading, "ink");
            Label sub = new Label
            {
                Name = "lblSignInSub",
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
            if (username != null)
            {
                username.TabIndex = 0;
            }

            if (password != null)
            {
                password.TabIndex = 1;
            }

            if (login is Button loginButton)
            {
                loginButton.Text = "Sign in";
                SetRole(loginButton, "accent");
                loginButton.Height = 46;
                loginButton.Dock = DockStyle.Top;
                loginButton.Margin = new Padding(0, 8, 0, 0);
                loginButton.TabIndex = 2;
                form.AcceptButton = loginButton;
            }

            if (toggle is Button toggleButton)
            {
                toggleButton.Text = string.Empty;
                toggleButton.Width = 46;
                toggleButton.Dock = DockStyle.Right;
                toggleButton.FlatAppearance.BorderSize = 0;
                toggleButton.TabStop = false;
                SetRole(toggleButton, "secondary");
            }

            if (error is Label errorLabel)
            {
                errorLabel.AutoSize = false;
                errorLabel.Dock = DockStyle.Top;
                errorLabel.Height = 48;
                errorLabel.ForeColor = PosPalette.Danger;
                errorLabel.Font = PosPalette.Ui(9.5F, FontStyle.Regular);
                errorLabel.TextAlign = ContentAlignment.MiddleLeft;
                SetRole(errorLabel, "danger-text");
            }

            if (exit is Button exitButton)
            {
                exitButton.Text = "Exit";
                SetRole(exitButton, "secondary");
                exitButton.Height = 40;
                exitButton.Dock = DockStyle.Top;
                exitButton.TabIndex = 3;
                form.CancelButton = exitButton;
            }

            Panel userRow = FieldRow("Username", username, usernameLabel, null);
            Panel passRow = FieldRow("Password", password, passwordLabel, toggle);

            AddDocked(card, exit);
            AddDocked(card, login);
            AddDocked(card, error);
            card.Controls.Add(passRow);
            card.Controls.Add(userRow);
            card.Controls.Add(sub);
            card.Controls.Add(heading);

            root.Controls.Add(card);
            root.Controls.Add(brand);
            form.Controls.Add(root);
            RestyleTree(form);
        }

        private static Panel FieldRow(string caption, Control input, Control existingLabel, Control trailing)
        {
            Panel row = new Panel
            {
                Dock = DockStyle.Top,
                Height = 78,
                BackColor = PosPalette.Surface,
                Padding = new Padding(0, 8, 0, 8)
            };
            SetRole(row, "surface");

            Label label = existingLabel as Label ?? new Label();
            label.AutoSize = false;
            label.Dock = DockStyle.Top;
            label.Height = 20;
            label.Text = caption;
            label.ForeColor = PosPalette.Muted;
            label.Font = PosPalette.Ui(9F, FontStyle.Bold);
            SetRole(label, "muted");

            Panel inputHost = new Panel
            {
                Dock = DockStyle.Top,
                Height = 34,
                BackColor = PosPalette.Surface
            };
            SetRole(inputHost, "surface");
            if (input != null)
            {
                input.Dock = DockStyle.Fill;
                input.Height = 34;
                inputHost.Controls.Add(input);
            }

            if (trailing != null)
            {
                trailing.Dock = DockStyle.Right;
                trailing.Width = 46;
                trailing.Height = 34;
                inputHost.Controls.Add(trailing);
                trailing.BringToFront();
            }

            row.Controls.Add(inputHost);
            row.Controls.Add(label);
            return row;
        }

        private static void AddDocked(Control parent, Control child)
        {
            if (parent == null || child == null)
            {
                return;
            }

            parent.Controls.Add(child);
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
                   value.Equals("zone", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("bike zone", StringComparison.OrdinalIgnoreCase) ||
                   value.Equals("bikezone", StringComparison.OrdinalIgnoreCase);
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

            string parent = ParentRole(control);
            if (control is Label)
            {
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

            if (control is CheckBox || control is RadioButton || control is GroupBox)
            {
                return parent == "sidebar" ? "sidebar-text" : "ink";
            }

            if (control is Panel || control is FlowLayoutPanel || control is TableLayoutPanel)
            {
                if (IsLegacyHeader(control as Panel) || parent == "header")
                {
                    return "header";
                }

                if (parent == "sidebar")
                {
                    return "sidebar";
                }

                Panel panel = control as Panel;
                if (panel != null && panel.Dock == DockStyle.Left && IsDark(panel.BackColor))
                {
                    return "sidebar";
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

            if (ContainsAny(blob, "delete", "remove", "logout", "log out", "حذف", "مسح"))
            {
                return "danger";
            }

            if (ContainsAny(blob, "save", "add", "login", "signin", "sign in", "start", "بيع", "اضافة", "إضافة", "حفظ"))
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
                case "danger-text":
                    control.ForeColor = PosPalette.Danger;
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
