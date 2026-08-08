using Pos_System.Services;
using System;
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
        private ToolStripButton menuPricingAdmin;
        private ToolStripButton menuBusinessIntelligence;
        private ToolStripButton menuSecurityAdmin;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            AddPurchaseMenus();
            ApplyModernDashboardChrome();
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
            menuSalesOperations = CreateDashboardMenuButton("Sales Ops", OpenSalesOperations_Click);
            menuInventoryOperations = CreateDashboardMenuButton("Stock Ops", OpenInventoryOperations_Click);
            menuPricingAdmin = CreateDashboardMenuButton("Pricing", OpenPricingAdmin_Click);
            menuBusinessIntelligence = CreateDashboardMenuButton("BI", OpenBusinessIntelligence_Click);
            menuSecurityAdmin = CreateDashboardMenuButton("Security", OpenSecurityAdmin_Click);

            int supplierIndex = dashboardMenuStrip.Items.IndexOf(menuSupplier);
            if (supplierIndex < 0) supplierIndex = Math.Min(3, dashboardMenuStrip.Items.Count);
            dashboardMenuStrip.Items.Insert(supplierIndex, menuPurchases);
            dashboardMenuStrip.Items.Insert(Math.Min(supplierIndex + 2, dashboardMenuStrip.Items.Count), menuSupplierLedger);

            int customerIndex = dashboardMenuStrip.Items.IndexOf(menuCustomers);
            if (customerIndex < 0) customerIndex = Math.Min(supplierIndex + 3, dashboardMenuStrip.Items.Count);
            dashboardMenuStrip.Items.Insert(Math.Min(customerIndex + 1, dashboardMenuStrip.Items.Count), menuCustomerLedger);

            int reportsIndex = dashboardMenuStrip.Items.IndexOf(menuReports);
            if (reportsIndex < 0) reportsIndex = dashboardMenuStrip.Items.Count;
            dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex, dashboardMenuStrip.Items.Count), menuSalesOperations);
            dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex + 1, dashboardMenuStrip.Items.Count), menuInventoryOperations);
            dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex + 2, dashboardMenuStrip.Items.Count), menuExpenses);
            dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex + 3, dashboardMenuStrip.Items.Count), menuCashShift);
            dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex + 4, dashboardMenuStrip.Items.Count), menuCashReports);
            dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex + 5, dashboardMenuStrip.Items.Count), menuPricingAdmin);
            dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex + 6, dashboardMenuStrip.Items.Count), menuBusinessIntelligence);
            if (AppSession.IsAdministrator)
                dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex + 7, dashboardMenuStrip.Items.Count), menuSecurityAdmin);

            purchaseMenusAdded = true;
            LayoutDashboardMenuButtons();
        }

        private void OpenPurchases_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSuppliers,"purchases")) return; SetWorkspaceTitle("Purchases"); LoadForm(new PurchaseForm()); }
        private void OpenSupplierLedger_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSuppliers,"supplier ledger")) return; SetWorkspaceTitle("Supplier Ledger"); LoadForm(new SupplierLedgerForm()); }
        private void OpenCustomerLedger_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenCustomers,"customer ledger")) return; SetWorkspaceTitle("Customer Ledger"); LoadForm(new CustomerLedgerForm()); }
        private void OpenExpenses_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenReports,"expenses")) return; SetWorkspaceTitle("Expenses"); LoadForm(new ExpenseForm()); }
        private void OpenCashShift_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSales,"cash shift")) return; SetWorkspaceTitle("Cash Shift"); LoadForm(new CashSessionForm()); }
        private void OpenCashReports_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenReports,"X/Z reports")) return; SetWorkspaceTitle("X / Z Reports"); LoadForm(new CashReportForm()); }
        private void OpenSalesOperations_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSales,"sales operations")) return; SetWorkspaceTitle("Sales Operations"); LoadForm(new SalesLifecycleForm()); }
        private void OpenInventoryOperations_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenProducts,"inventory operations")) return; SetWorkspaceTitle("Inventory Operations"); LoadForm(new InventoryOperationsForm()); }
        private void OpenPricingAdmin_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenSettings,"pricing administration")) return; SetWorkspaceTitle("Pricing / Tax / Loyalty"); LoadForm(new PricingAdminForm()); }
        private void OpenBusinessIntelligence_Click(object sender, EventArgs e) { if (!RequireScreen(PermissionService.ScreenReports,"business intelligence")) return; SetWorkspaceTitle("Business Intelligence"); LoadForm(new BusinessIntelligenceForm()); }
        private void OpenSecurityAdmin_Click(object sender, EventArgs e) { if (!AppSession.IsAdministrator) { MessageBox.Show("Administrator access is required.","Permission",MessageBoxButtons.OK,MessageBoxIcon.Warning); return; } SetWorkspaceTitle("Security & Reliability"); LoadForm(new SecurityAdminForm()); }

        private bool RequireScreen(string screen, string label)
        {
            if (PermissionService.CanViewScreen(AppSession.UserId, _role, screen)) return true;
            MessageBox.Show("You do not have permission to open " + label + ".", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
    }
}
