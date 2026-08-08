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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            AddPurchaseMenus();
        }

        private void AddPurchaseMenus()
        {
            if (purchaseMenusAdded || dashboardMenuStrip == null) return;

            menuPurchases = CreateDashboardMenuButton("Purchases", OpenPurchases_Click);
            menuSupplierLedger = CreateDashboardMenuButton("Supplier Ledger", OpenSupplierLedger_Click);
            menuCustomerLedger = CreateDashboardMenuButton("Customer Ledger", OpenCustomerLedger_Click);
            menuExpenses = CreateDashboardMenuButton("Expenses", OpenExpenses_Click);

            int supplierIndex = dashboardMenuStrip.Items.IndexOf(menuSupplier);
            if (supplierIndex < 0) supplierIndex = Math.Min(3, dashboardMenuStrip.Items.Count);

            dashboardMenuStrip.Items.Insert(supplierIndex, menuPurchases);
            dashboardMenuStrip.Items.Insert(Math.Min(supplierIndex + 2, dashboardMenuStrip.Items.Count), menuSupplierLedger);

            int customerIndex = dashboardMenuStrip.Items.IndexOf(menuCustomers);
            if (customerIndex < 0) customerIndex = Math.Min(supplierIndex + 3, dashboardMenuStrip.Items.Count);
            dashboardMenuStrip.Items.Insert(Math.Min(customerIndex + 1, dashboardMenuStrip.Items.Count), menuCustomerLedger);

            int reportsIndex = dashboardMenuStrip.Items.IndexOf(menuReports);
            if (reportsIndex < 0) reportsIndex = dashboardMenuStrip.Items.Count;
            dashboardMenuStrip.Items.Insert(Math.Min(reportsIndex, dashboardMenuStrip.Items.Count), menuExpenses);

            purchaseMenusAdded = true;
            LayoutDashboardMenuButtons();
        }

        private void OpenPurchases_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenSuppliers))
            {
                MessageBox.Show("You do not have permission to open purchases.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadForm(new PurchaseForm());
        }

        private void OpenSupplierLedger_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenSuppliers))
            {
                MessageBox.Show("You do not have permission to open supplier ledger.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadForm(new SupplierLedgerForm());
        }

        private void OpenCustomerLedger_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenCustomers))
            {
                MessageBox.Show("You do not have permission to open customer ledger.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadForm(new CustomerLedgerForm());
        }

        private void OpenExpenses_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanViewScreen(AppSession.UserId, _role, PermissionService.ScreenReports))
            {
                MessageBox.Show("You do not have permission to open expenses.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            LoadForm(new ExpenseForm());
        }
    }
}
