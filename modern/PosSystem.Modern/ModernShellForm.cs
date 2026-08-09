namespace BikeZonePOS.Modern;

internal sealed class ModernShellForm : Form
{
    private readonly DatabaseHealthService healthService;
    private readonly Label connectionLabel;
    private readonly Label detailLabel;
    private readonly Button refreshButton;

    public ModernShellForm(DatabaseHealthService healthService)
    {
        this.healthService = healthService;
        Text = "Bike Zone POS - Modern .NET 10 Migration";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 560);
        Size = new Size(1100, 680);
        BackColor = Color.FromArgb(245,247,251);
        Font = new Font("Segoe UI", 10F);

        Panel sidebar = new() { Dock = DockStyle.Left, Width = 245, BackColor = Color.FromArgb(15,23,42), Padding = new Padding(22) };
        sidebar.Controls.Add(new Label { Text = "BIKE ZONE\nPOS", ForeColor = Color.White, Font = new Font("Segoe UI",22F,FontStyle.Bold), AutoSize = true, Location = new Point(22,28) });
        sidebar.Controls.Add(new Label { Text = ".NET 10 LTS migration shell", ForeColor = Color.FromArgb(148,163,184), AutoSize = true, Location = new Point(24,108) });
        string[] modules = { "Dashboard", "Sales", "Purchases", "Inventory", "Customers", "Suppliers", "Reports", "Settings" };
        int y=170;
        foreach(string module in modules)
        {
            Button b = new() { Text=module, Left=18, Top=y, Width=205, Height=42, FlatStyle=FlatStyle.Flat, ForeColor=Color.FromArgb(226,232,240), BackColor=Color.FromArgb(30,41,59), TextAlign=ContentAlignment.MiddleLeft, Padding=new Padding(12,0,0,0), Enabled=false };
            b.FlatAppearance.BorderSize=0; sidebar.Controls.Add(b); y+=48;
        }

        Panel header = new() { Dock=DockStyle.Top, Height=82, BackColor=Color.White, Padding=new Padding(28,16,28,12) };
        header.Controls.Add(new Label { Text="Modernization Workspace", Font=new Font("Segoe UI",18F,FontStyle.Bold), ForeColor=Color.FromArgb(15,23,42), AutoSize=true, Location=new Point(28,17) });
        header.Controls.Add(new Label { Text="Side-by-side validation before switching the production executable", ForeColor=Color.FromArgb(100,116,139), AutoSize=true, Location=new Point(30,50) });

        Panel content = new() { Dock=DockStyle.Fill, BackColor=Color.FromArgb(245,247,251), Padding=new Padding(32) };
        Panel card = new() { Dock=DockStyle.Top, Height=255, BackColor=Color.White, Padding=new Padding(28) };
        card.Controls.Add(new Label { Text="Migration readiness", Font=new Font("Segoe UI",16F,FontStyle.Bold), ForeColor=Color.FromArgb(15,23,42), AutoSize=true, Location=new Point(28,24) });
        card.Controls.Add(new Label { Text="This executable proves the new SDK-style .NET 10 WinForms runtime, DI composition root and Microsoft.Data.SqlClient connection path can run side-by-side with the current production app.", MaximumSize=new Size(700,0), AutoSize=true, ForeColor=Color.FromArgb(71,85,105), Location=new Point(30,66) });
        connectionLabel = new Label { Text="Database: checking...", Font=new Font("Segoe UI",11F,FontStyle.Bold), AutoSize=true, Location=new Point(30,125) };
        detailLabel = new Label { AutoSize=true, ForeColor=Color.FromArgb(100,116,139), Location=new Point(30,154) };
        refreshButton = new Button { Text="Check database", Left=30, Top=190, Width=145, Height=38, BackColor=Color.FromArgb(37,99,235), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
        refreshButton.FlatAppearance.BorderSize=0; refreshButton.Click += async (_,_) => await RefreshHealthAsync();
        card.Controls.Add(connectionLabel);card.Controls.Add(detailLabel);card.Controls.Add(refreshButton);

        Panel note = new() { Dock=DockStyle.Top, Height=140, BackColor=Color.FromArgb(239,246,255), Padding=new Padding(26), Margin=new Padding(0,18,0,0) };
        note.Controls.Add(new Label { Text="Safe migration rule", Font=new Font("Segoe UI",12F,FontStyle.Bold), ForeColor=Color.FromArgb(30,64,175), AutoSize=true, Location=new Point(26,20) });
        note.Controls.Add(new Label { Text="The existing .NET Framework POS remains the installer default until each production module is ported and parity-tested. The modern project is built in CI on every change so migration can proceed module-by-module without a risky big-bang rewrite.", MaximumSize=new Size(720,0), AutoSize=true, ForeColor=Color.FromArgb(30,64,175), Location=new Point(28,54) });

        content.Controls.Add(note);content.Controls.SetChildIndex(note,0);content.Controls.Add(card);content.Controls.SetChildIndex(card,0);
        Controls.Add(content);Controls.Add(header);Controls.Add(sidebar);
        Shown += async (_,_) => await RefreshHealthAsync();
    }

    private async Task RefreshHealthAsync()
    {
        refreshButton.Enabled=false;
        connectionLabel.Text="Database: checking...";
        try
        {
            DatabaseHealth health = await healthService.CheckAsync();
            connectionLabel.Text = health.IsConnected ? "Database: Connected" : "Database: Not connected";
            connectionLabel.ForeColor = health.IsConnected ? Color.FromArgb(22,163,74) : Color.FromArgb(220,38,38);
            detailLabel.Text = health.IsConnected ? $"Server: {health.Server}  |  Database: {health.Database}  |  {health.Message}" : health.Message;
        }
        finally { refreshButton.Enabled=true; }
    }
}
