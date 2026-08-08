using Pos_System.Services;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    internal sealed class PricingAdminForm : Form
    {
        private readonly PricingService service;
        private readonly TabControl tabs;
        private DataGridView taxGrid, rateGrid, promoGrid, loyaltyGrid;
        private TextBox taxCode,taxName,fromCurrency,toCurrency,promoCode,promoName,loyaltyDescription;
        private NumericUpDown taxPercent,exchangeRate,promoDiscount,promoMinInvoice,loyaltyPoints;
        private CheckBox taxInclusive;
        private ComboBox loyaltyCustomer;

        public PricingAdminForm()
        {
            service=new PricingService(POS_System.Program.SettingsManager.ConnectionString);
            Text="Pricing / Tax / Currency / Loyalty";Dock=DockStyle.Fill;BackColor=ModernUiService.AppBackground;Font=new Font("Segoe UI",9.5F);
            tabs=new TabControl{Dock=DockStyle.Fill};tabs.TabPages.Add(TaxTab());tabs.TabPages.Add(RateTab());tabs.TabPages.Add(PromoTab());tabs.TabPages.Add(LoyaltyTab());Controls.Add(tabs);Load+=(s,e)=>RefreshAll();
        }

        private TabPage TaxTab()
        {
            var p=Page("Tax Rates");taxGrid=Grid();var top=Top(110);taxCode=Text(top,"Code",12,70,90);taxName=Text(top,"Name",180,230,170);taxPercent=Num(top,"Rate %",420,485,80,4);taxInclusive=new CheckBox{Text="Inclusive",Left=580,Top=16,AutoSize=true};top.Controls.Add(taxInclusive);var add=Btn("Add Tax",680,10);top.Controls.Add(add);add.Click+=(s,e)=>{Try(()=>{service.AddTaxRate(taxCode.Text,taxName.Text,taxPercent.Value,taxInclusive.Checked);taxGrid.DataSource=service.TaxRates();});};p.Controls.Add(taxGrid);p.Controls.Add(top);return p;
        }
        private TabPage RateTab()
        {
            var p=Page("Exchange Rates");rateGrid=Grid();var top=Top(110);fromCurrency=Text(top,"From",12,70,80);toCurrency=Text(top,"To",165,210,80);exchangeRate=Num(top,"Rate",310,360,130,8);var add=Btn("Add Rate",510,10);top.Controls.Add(add);add.Click+=(s,e)=>Try(()=>{service.AddExchangeRate(fromCurrency.Text,toCurrency.Text,exchangeRate.Value,DateTime.Now,AppSession.UserId);rateGrid.DataSource=service.ExchangeRates();});p.Controls.Add(rateGrid);p.Controls.Add(top);return p;
        }
        private TabPage PromoTab()
        {
            var p=Page("Promotions");promoGrid=Grid();var top=Top(130);promoCode=Text(top,"Code",12,70,90);promoName=Text(top,"Name",180,230,160);promoDiscount=Num(top,"Discount %",410,490,90,2);promoMinInvoice=Num(top,"Min Invoice",600,680,120,2);var add=Btn("Add Promotion",820,10);add.Width=130;top.Controls.Add(add);add.Click+=(s,e)=>Try(()=>{service.AddPromotion(promoCode.Text,promoName.Text,"INVOICE_DISCOUNT",promoDiscount.Value,promoMinInvoice.Value,DateTime.Now,null);promoGrid.DataSource=service.Promotions();});p.Controls.Add(promoGrid);p.Controls.Add(top);return p;
        }
        private TabPage LoyaltyTab()
        {
            var p=Page("Loyalty");loyaltyGrid=Grid();var top=Top(120);top.Controls.Add(new Label{Text="Customer",Left=12,Top=17,AutoSize=true});loyaltyCustomer=new ComboBox{Left=85,Top=12,Width=220,DropDownStyle=ComboBoxStyle.DropDownList};top.Controls.Add(loyaltyCustomer);loyaltyPoints=Num(top,"Points +/-",325,405,100,2);loyaltyDescription=Text(top,"Description",525,610,180);var add=Btn("Adjust",805,10);top.Controls.Add(add);add.Click+=(s,e)=>Try(()=>{if(loyaltyCustomer.SelectedValue==null)throw new InvalidOperationException("Choose a customer.");service.AdjustLoyalty(Convert.ToInt32(loyaltyCustomer.SelectedValue),loyaltyPoints.Value,loyaltyDescription.Text,AppSession.UserId);loyaltyGrid.DataSource=service.Loyalty();});p.Controls.Add(loyaltyGrid);p.Controls.Add(top);return p;
        }

        private void RefreshAll(){Try(()=>{taxGrid.DataSource=service.TaxRates();rateGrid.DataSource=service.ExchangeRates();promoGrid.DataSource=service.Promotions();loyaltyGrid.DataSource=service.Loyalty();DataTable c=service.Customers();loyaltyCustomer.DataSource=c;loyaltyCustomer.DisplayMember="name";loyaltyCustomer.ValueMember="customer_id";});}
        private static void Try(Action action){try{action();}catch(Exception ex){MessageBox.Show(ex.Message,"Pricing",MessageBoxButtons.OK,MessageBoxIcon.Error);}}
        private static TabPage Page(string text)=>new TabPage(text){BackColor=ModernUiService.AppBackground,Padding=new Padding(8)};
        private static DataGridView Grid()=>new DataGridView{Dock=DockStyle.Fill,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AllowUserToAddRows=false,ReadOnly=true};
        private static Panel Top(int height)=>new Panel{Dock=DockStyle.Top,Height=height,BackColor=Color.White};
        private static Button Btn(string text,int x,int y){var b=new Button{Text=text,Left=x,Top=y,Width=105,Height=36,BackColor=ModernUiService.Primary,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};b.FlatAppearance.BorderSize=0;return b;}
        private static TextBox Text(Control p,string label,int lx,int x,int width){p.Controls.Add(new Label{Text=label,Left=lx,Top=17,AutoSize=true});var t=new TextBox{Left=x,Top=12,Width=width};p.Controls.Add(t);return t;}
        private static NumericUpDown Num(Control p,string label,int lx,int x,int width,int decimals){p.Controls.Add(new Label{Text=label,Left=lx,Top=17,AutoSize=true});var n=new NumericUpDown{Left=x,Top=12,Width=width,DecimalPlaces=decimals,Maximum=1000000000,Minimum=-1000000000};p.Controls.Add(n);return n;}
    }
}
