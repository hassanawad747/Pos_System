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

            int supplierIndex = dashboardMenuStrip.Items.IndexOf(menuSupplier);
            if (supplierIndex < 0) supplierIndex = Math.Min(3, dashboardMenuStrip.Items.Count);

            dashboardMenuStrip.Items.Insert(supplierIndex, menuPurchases);
            dashboardMenuStrip.Items.Insert(Math.Min(supplierIndex + 2, dashboardMenuStrip.Items.Count), menuSupplierLedger);
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
    }
}