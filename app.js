const views = {
  dashboard: {
    title: "Dashboard",
    subtitle: "Live-style overview for daily POS activity.",
    heading: "Dashboard Activity",
    action: "New Sale",
    rows: [
      ["09:10", "Sale invoice #1841", "cashier01", "Completed", "green"],
      ["09:26", "Customer balance updated", "admin", "Synced", "green"],
      ["10:02", "Product stock warning", "system", "Review", "gold"],
      ["10:18", "Return processed", "manager", "Approved", "green"],
      ["10:45", "Supplier payment added", "admin", "Saved", "green"]
    ],
    quick: [
      ["Best seller", "Barcode scanner bundle"],
      ["Cash drawer", "$1,275 available"],
      ["Next task", "Review 12 low-stock products"]
    ]
  },
  sales: {
    title: "Sales",
    subtitle: "Create invoices, manage returns, and track daily totals.",
    heading: "Recent Sales",
    action: "Add Invoice",
    rows: [
      ["#1845", "Walk-in customer", "$84.00", "Paid", "green"],
      ["#1844", "Ali Market", "$215.50", "Pending", "gold"],
      ["#1843", "Cash customer", "$41.25", "Paid", "green"],
      ["#1842", "Return invoice", "-$16.00", "Returned", "red"]
    ],
    quick: [
      ["Payment methods", "Cash, card, customer balance"],
      ["Return control", "Approval workflow supported"],
      ["Currency", "USD and LBP totals"]
    ]
  },
  products: {
    title: "Products",
    subtitle: "Manage barcodes, prices, stock, categories, and warehouses.",
    heading: "Inventory Watch",
    action: "Add Product",
    rows: [
      ["PRD-1009", "Receipt printer paper", "4 left", "Low stock", "gold"],
      ["PRD-1014", "USB barcode scanner", "18 left", "Available", "green"],
      ["PRD-1032", "Thermal printer", "0 left", "Out", "red"],
      ["PRD-1048", "Cash drawer", "7 left", "Available", "green"]
    ],
    quick: [
      ["Inventory tools", "Categories, suppliers, warehouse reports"],
      ["Pricing", "Default price and discount limits"],
      ["Alerts", "Low-stock threshold monitoring"]
    ]
  },
  customers: {
    title: "Customers",
    subtitle: "Track customer profiles, purchase history, and balances.",
    heading: "Customer Balances",
    action: "Add Customer",
    rows: [
      ["CUS-021", "Samir Trading", "$420.00", "Due", "gold"],
      ["CUS-044", "Nour Shop", "$0.00", "Clear", "green"],
      ["CUS-058", "Maya Market", "$95.50", "Due", "gold"],
      ["CUS-063", "Cash customer", "$0.00", "Clear", "green"]
    ],
    quick: [
      ["Balance tracking", "USD and LBP customer balances"],
      ["Search", "Find by name, phone, or account"],
      ["History", "Customer activity and sales records"]
    ]
  },
  suppliers: {
    title: "Suppliers",
    subtitle: "Manage supplier accounts, payments, and balance reporting.",
    heading: "Supplier Overview",
    action: "Add Supplier",
    rows: [
      ["SUP-012", "Tech Supply Co.", "$1,120.00", "Open", "gold"],
      ["SUP-016", "Paper House", "$0.00", "Clear", "green"],
      ["SUP-023", "Lebanon Wholesale", "$640.75", "Open", "gold"],
      ["SUP-031", "Office Line", "$0.00", "Clear", "green"]
    ],
    quick: [
      ["Balance reports", "Supplier balance screen included"],
      ["Contact data", "Phone, email, address"],
      ["Payments", "Track paid and remaining amounts"]
    ]
  },
  reports: {
    title: "Reports",
    subtitle: "View sales, earnings, warehouse, and audit reports.",
    heading: "Report Center",
    action: "Export",
    rows: [
      ["Today", "Sales summary", "PDF", "Ready", "green"],
      ["This week", "Earnings report", "Excel", "Ready", "green"],
      ["Warehouse", "Stock movement", "PDF", "Ready", "green"],
      ["Audit", "User activity", "Screen", "Live", "gold"]
    ],
    quick: [
      ["Export formats", "PDF and Excel-style reporting"],
      ["Audit logs", "User actions and changes"],
      ["Filters", "Date, user, product, customer"]
    ]
  },
  settings: {
    title: "Settings",
    subtitle: "Control permissions, currency, discounts, and system behavior.",
    heading: "System Settings",
    action: "Save",
    rows: [
      ["Currency", "USD", "Active", "Enabled", "green"],
      ["Exchange rate", "89,000", "Editable", "Enabled", "green"],
      ["Max discount", "20%", "Protected", "Enabled", "green"],
      ["Failed login lock", "3 attempts", "Security", "Enabled", "green"]
    ],
    quick: [
      ["Permissions", "Role-based user access"],
      ["Security", "Login lock and session timeout"],
      ["Defaults", "Tax, reports, prices, backup schedule"]
    ]
  }
};

const title = document.getElementById("view-title");
const subtitle = document.getElementById("view-subtitle");
const table = document.getElementById("data-table");
const quickView = document.getElementById("quick-view");
const panelHeading = document.querySelector("#main-panel .panel-heading h2");
const action = document.querySelector(".primary-action");

function renderView(name) {
  const view = views[name];
  title.textContent = view.title;
  subtitle.textContent = view.subtitle;
  panelHeading.textContent = view.heading;
  action.textContent = view.action;
  table.innerHTML = view.rows
    .map(row => `
      <tr>
        <td>${row[0]}</td>
        <td>${row[1]}</td>
        <td>${row[2]}</td>
        <td><span class="badge ${row[4]}">${row[3]}</span></td>
      </tr>
    `)
    .join("");
  quickView.innerHTML = view.quick
    .map(item => `
      <div class="quick-item">
        <span>${item[0]}</span>
        <strong>${item[1]}</strong>
      </div>
    `)
    .join("");
}

document.querySelectorAll(".nav-item").forEach(button => {
  button.addEventListener("click", () => {
    document.querySelectorAll(".nav-item").forEach(item => item.classList.remove("active"));
    button.classList.add("active");
    renderView(button.dataset.view);
  });
});

renderView("dashboard");
