using Pos_System.Services;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    internal sealed class BusinessIntelligenceForm : Form
    {
        private readonly BusinessIntelligenceService service;
        private readonly TabControl tabs;

        public BusinessIntelligenceForm() : this(new BusinessIntelligenceService(POS_System.Program.SettingsManager.ConnectionString)) { }

        public BusinessIntelligenceForm(BusinessIntelligenceService service)
        {
            this.service=service ?? throw new ArgumentNullException(nameof(service));
            Text="Business Intelligence";Dock=DockStyle.Fill;BackColor=ModernUiService.AppBackground;Font=new Font("Segoe UI",9.5F);
            tabs=new TabControl{Dock=DockStyle.Fill};Controls.Add(tabs);
            AddTab("Profit",()=>this.service.DailyProfit());AddTab("Products",()=>this.service.ProductPerformance());AddTab("Stock",()=>this.service.StockHealth());AddTab("Customers",()=>this.service.Customers());AddTab("Suppliers",()=>this.service.Suppliers());AddTab("Hourly Sales",()=>this.service.Hourly());AddTab("Returns",()=>this.service.Returns());
        }
        private void AddTab(string name,Func<DataTable> loader)
        {
            var page=new TabPage(name){BackColor=ModernUiService.AppBackground,Padding=new Padding(8)};var grid=new DataGridView{Dock=DockStyle.Fill,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AllowUserToAddRows=false,ReadOnly=true};var bar=new Panel{Dock=DockStyle.Top,Height=56,BackColor=Color.White};var refresh=new Button{Text="Refresh",Left=12,Top=10,Width=105,Height=36,BackColor=ModernUiService.Primary,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};refresh.FlatAppearance.BorderSize=0;refresh.Click+=(s,e)=>LoadGrid(grid,loader);bar.Controls.Add(refresh);page.Controls.Add(grid);page.Controls.Add(bar);tabs.TabPages.Add(page);page.Enter+=(s,e)=>{if(grid.DataSource==null)LoadGrid(grid,loader);};
        }
        private static void LoadGrid(DataGridView grid,Func<DataTable> loader){try{grid.DataSource=loader();}catch(Exception ex){ErrorLogService.Log(ex,"BI_REPORT",nameof(BusinessIntelligenceForm));MessageBox.Show(ex.Message,"Reports",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
    }
}
