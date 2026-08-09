using Pos_System.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class PosSystemDashboard
    {
        private bool purchaseMenusAdded;
        private ToolStripButton menuPurchases;
        private ToolStripButton menuSupplierLedger;
        private ToolStripButton menuCustomerLedger;
        private ToolStripButton menuExpenses;
        private ToolStripButton menuCashShift;
        private ToolStripButton menuCashReports;
        private ToolStripButton menuSalesOperations;
        private ToolStripButton menuInventoryOperations;
        private ToolStripButton menuStockLoss;
        private ToolStripButton menuPricingAdmin;
        private ToolStripButton menuBusinessIntelligence;
        private ToolStripButton menuSecurityAdmin;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            AddPurchaseMenus();
            ApplyModernDashboardChrome();
            LayoutDashboardHome();
        }

        private void AddPurchaseMenus()
        {
            if (purchaseMenusAdded || dashboardMenuStrip == null) return;

            menuPurchases = CreateDashboardMenuButton("Purchases", OpenPurchases_Click);
            menuSupplierLedger = CreateDashboardMenuButton("Supplier Ledger", OpenSupplierLedger_Click);
            menuCustomerLedger = CreateDashboardMenuButton("Customer Ledger", OpenCustomerLedger_Click);
            menuExpenses = CreateDashboardMenuButton("Expenses", OpenExpenses_Click);
            menuCashShift = CreateDashboardMenuButton("Cash Shift", OpenCashShift_Click);
            menuCashReports = CreateDashboardMenuButton("X / Z Reports", OpenCashReports_Click);
            menuSalesOperations = CreateDashboardMenuButton("Sales Operations", OpenSalesOperations_Click);
            menuInventoryOperations = CreateDashboardMenuButton("Inventory Operations", OpenInventoryOperations_Click);
            menuStockLoss = CreateDashboardMenuButton("Damage / Expiry", OpenStockLoss_Click);
            menuPricingAdmin = CreateDashboardMenuButton("Pricing / Tax / Loyalty", OpenPricingAdmin_Click);
            menuBusinessIntelligence = CreateDashboardMenuButton("Business Intelligence", OpenBusinessIntelligence_Click);
            menuSecurityAdmin = CreateDashboardMenuButton("Security", OpenSecurityAdmin_Click);

            ToolStripDropDownButton partners = CreateDashboardGroup("Partners");
            partners.DropDownItems.Add(CreateDashboardMenuItem("Suppliers", menuSupplier_Click));
            partners.DropDownItems.Add(CreateDashboardMenuItem("Supplier Ledger", OpenSupplierLedger_Click));
            partners.DropDownItems.Add(new ToolStripSeparator());
            partners.DropDownItems.Add(CreateDashboardMenuItem("Customers", btnCustomers_Click));
            partners.DropDownItems.Add(CreateDashboardMenuItem("Customer Ledger", OpenCustomerLedger_Click));

            ToolStripDropDownButton operations = CreateDashboardGroup("Operations");
            operations.DropDownItems.Add(CreateDashboardMenuItem("Sales Operations", OpenSalesOperations_Click));
            operations.DropDownItems.Add(CreateDashboardMenuItem("Inventory Operations", OpenInventoryOperations_Click));
            operations.DropDownItems.Add(CreateDashboardMenuItem("Damage / Expiry", OpenStockLoss_Click));
            operations.DropDownItems.Add(new ToolStripSeparator());
            operations.DropDownItems.Add(CreateDashboardMenuItem("Expenses", OpenExpenses_Click));
            operations.DropDownItems.Add(CreateDashboardMenuItem("Cash Shift", OpenCashShift_Click));

            ToolStripDropDownButton reports = CreateDashboardGroup("Reports");
            reports.DropDownItems.Add(CreateDashboardMenuItem("Earning Report", btnEarningReports_Click));
            reports.DropDownItems.Add(CreateDashboardMenuItem("Warehouse Report", btnWarhouseReports_Click));
            reports.DropDownItems.Add(CreateDashboardMenuItem("Reports Overview", button8_Click));
            reports.DropDownItems.Add(CreateDashboardMenuItem("X / Z Reports", OpenCashReports_Click));
            reports.DropDownItems.Add(CreateDashboardMenuItem("Business Intelligence", OpenBusinessIntelligence_Click));

            ToolStripDropDownButton administration = CreateDashboardGroup("Administration");
            administration.DropDownItems.Add(CreateDashboardMenuItem("Pricing / Tax / Loyalty", OpenPricingAdmin_Click));
            administration.DropDownItems.Add(CreateDashboardMenuItem("Users", btnAddUsers_Click));
            administration.DropDownItems.Add(CreateDashboardMenuItem("User Permissions", OpenUserPermissions_Click));
            administration.DropDownItems.Add(CreateDashboardMenuItem("Activity Log", OpenActivityLog_Click));
            administration.DropDownItems.Add(CreateDashboardMenuItem("Settings", button10_Click));
            if (AppSession.IsAdministrator)
                administration.DropDownItems.Add(CreateDashboardMenuItem("Security & Reliability", OpenSecurityAdmin_Click));

            ToolStripDropDownButton actions = CreateDashboardGroup("Actions");
            actions.DropDownItems.Add(CreateDashboardMenuItem("Save", (sender, args) => ExecuteActiveFormCommand("save")));
            actions.DropDownItems.Add(CreateDashboardMenuItem("Save As", (sender, args) => ExecuteActiveFormCommand("saveas")));
            actions.DropDownItems.Add(CreateDashboardMenuItem("Edit", (sender, args) => ExecuteActiveFormCommand("edit")));
            actions.DropDownItems.Add(CreateDashboardMenuItem("Delete", (sender, args) => ExecuteActiveFormCommand("delete")));
            actions.DropDownItems.Add(CreateDashboardMenuItem("Print", (sender, args) => ExecuteActiveFormCommand("print")));

            dashboardMenuStrip.Items.Clear();
            dashboardMenuStrip.Items.AddRange(new ToolStripItem[]
            {
                menuDashboard,
                menuSales,
                menuProducts,
                menuPurchases,
                partners,
                operations,
                reports,
                administration,
                actions,
                menuLogout
            });

            purchaseMenusAdded = true;
            LayoutDashboardMenuButtons();
        }

        private ToolStripDropDownButton CreateDashboardGroup(string text)
        {
            return new ToolStripDropDownButton(text)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                ForeColor = Color.White,
                BackColor = ModernUiService.Sidebar,
                Padding = new Padding(12, 7, 12, 7),
                ShowDropDownArrow = true
            };
        }

        private ToolStripMenuItem CreateDashboardMenuItem(string text, EventHandler handler)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text)
            {
                BackColor = Color.White,
                ForeColor = ModernUiService.TextPrimary,
                Font = new Font("Segoe UI", 9.5F),
                Padding = new Padding(10, 7, 16, 7)
            };
            item.Click += handler;
            return item;
        }

        private void OpenPurchases_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSuppliers,"purchases")) return; SetWorkspaceTitle("Purchases"); LoadForm(new PurchaseForm()); }
        private void OpenSupplierLedger_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSuppliers,"supplier ledger")) return; SetWorkspaceTitle("Supplier Ledger"); LoadForm(new SupplierLedgerForm()); }
        private void OpenCustomerLedger_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenCustomers,"customer ledger")) return; SetWorkspaceTitle("Customer Ledger"); LoadForm(new CustomerLedgerForm()); }
        private void OpenExpenses_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenReports,"expenses")) return; SetWorkspaceTitle("Expenses"); LoadForm(new ExpenseForm()); }
        private void OpenCashShift_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSales,"cash shift")) return; SetWorkspaceTitle("Cash Shift"); LoadForm(new CashSessionForm()); }
        private void OpenCashReports_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenReports,"X/Z reports")) return; SetWorkspaceTitle("X / Z Reports"); LoadForm(new CashReportForm()); }
        private void OpenSalesOperations_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSales,"sales operations")) return; SetWorkspaceTitle("Sales Operations"); LoadForm(AppServices.Get<SalesLifecycleForm>()); }
        private void OpenInventoryOperations_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenProducts,"inventory operations")) return; SetWorkspaceTitle("Inventory Operations"); LoadForm(AppServices.Get<InventoryOperationsForm>()); }
        private void OpenStockLoss_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenProducts,"damage/expired stock")) return; SetWorkspaceTitle("Damage / Expired Stock"); LoadForm(AppServices.Get<StockLossForm>()); }
        private void OpenPricingAdmin_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSettings,"pricing administration")) return; SetWorkspaceTitle("Pricing / Tax / Loyalty"); LoadForm(AppServices.Get<PricingAdminForm>()); }
        private void OpenBusinessIntelligence_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenReports,"business intelligence")) return; SetWorkspaceTitle("Business Intelligence"); LoadForm(AppServices.Get<BusinessIntelligenceForm>()); }
        private void OpenSecurityAdmin_Click(object sender, EventArgs e) { if (!AppSession.IsAdministrator) { MessageBox.Show("Administrator access is required.","Permission",MessageBoxButtons.OK,MessageBoxIcon.Warning); return; } SetWorkspaceTitle("Security & Reliability"); LoadForm(AppServices.Get<SecurityAdminForm>()); }
        private void OpenActivityLog_Click(object sender, EventArgs e) { if (!AppSession.IsAdministrator) { MessageBox.Show("Administrator access is required.","Permission",MessageBoxButtons.OK,MessageBoxIcon.Warning); return; } SetWorkspaceTitle("Activity Log"); LoadForm(new ActivityLogForm()); }
        private void OpenUserPermissions_Click(object sender, EventArgs e) { if (!AppSession.IsAdministrator) { MessageBox.Show("Administrator access is required.","Permission",MessageBoxButtons.OK,MessageBoxIcon.Warning); return; } SetWorkspaceTitle("User Permissions"); LoadForm(new UserPermissionsForm()); }

        private bool RequireScreen(string screen, string label)
        {
            if (PermissionService.CanViewScreen(AppSession.UserId, _role, screen)) return true;
            MessageBox.Show("You do not have permission to open " + label + ".", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }
}
