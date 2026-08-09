using Pos_System.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    internal sealed class SalesLifecycleForm : Form
    {
        private readonly SalesLifecycleService service;
        private readonly TabControl tabs;
        private readonly DataGridView heldGrid;
        private readonly DataGridView quotationGrid;
        private TextBox saleIdText;
        private readonly DataGridView returnGrid;
        private ComboBox returnType;
        private ComboBox refundMethod;
        private TextBox reasonText;

        public SalesLifecycleForm()
        {
            service = new SalesLifecycleService(POS_System.Program.SettingsManager.ConnectionString);
            this.Text = "Sales Operations"; Dock = DockStyle.Fill; BackColor = ModernUiService.AppBackground; Font = new Font("Segoe UI", 9.5F);
            tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(14, 8) };
            heldGrid = CreateGrid(); quotationGrid = CreateGrid(); returnGrid = CreateGrid();
            tabs.TabPages.Add(BuildHeldTab()); tabs.TabPages.Add(BuildQuotationTab()); tabs.TabPages.Add(BuildReturnTab());
            Controls.Add(tabs); Load += (s, e) => RefreshAll();
        }

        private TabPage BuildHeldTab(){var page=NewPage("Held Sales");var bar=NewBar();var refresh=Button("Refresh",12);var resume=Button("Resume Selected",130);resume.Width=150;refresh.Click+=(s,e)=>LoadHeld();resume.Click+=(s,e)=>ResumeSelected();bar.Controls.Add(refresh);bar.Controls.Add(resume);page.Controls.Add(heldGrid);page.Controls.Add(bar);return page;}
        private TabPage BuildQuotationTab(){var page=NewPage("Quotations");var bar=NewBar();var refresh=Button("Refresh",12);var convert=Button("Convert to Sale",130);convert.Width=150;refresh.Click+=(s,e)=>LoadQuotations();convert.Click+=(s,e)=>ConvertQuotation();bar.Controls.Add(refresh);bar.Controls.Add(convert);page.Controls.Add(quotationGrid);page.Controls.Add(bar);return page;}
        private TabPage BuildReturnTab()
        {
            var page=NewPage("Returns / Exchange");var top=new Panel{Dock=DockStyle.Top,Height=106,BackColor=Color.White,Padding=new Padding(12)};
            top.Controls.Add(new Label{Text="Sale ID",AutoSize=true,Location=new Point(12,15)});saleIdText=new TextBox{Location=new Point(70,10),Width=120};var load=Button("Load Sale",200);load.Top=8;
            returnType=new ComboBox{Location=new Point(325,10),Width=120,DropDownStyle=ComboBoxStyle.DropDownList};returnType.Items.AddRange(new object[]{"RETURN","EXCHANGE"});returnType.SelectedIndex=0;
            refundMethod=new ComboBox{Location=new Point(455,10),Width=140,DropDownStyle=ComboBoxStyle.DropDownList};refundMethod.Items.AddRange(new object[]{"CASH","CARD","BANK","CUSTOMER_CREDIT","OTHER"});refundMethod.SelectedIndex=0;
            reasonText=new TextBox{Location=new Point(70,53),Width=525};top.Controls.Add(new Label{Text="Reason",AutoSize=true,Location=new Point(12,58)});var complete=Button("Complete Return",610);complete.Top=51;complete.Width=150;
            top.Controls.AddRange(new Control[]{saleIdText,load,returnType,refundMethod,reasonText,complete});load.Click+=(s,e)=>LoadReturnItems();complete.Click+=(s,e)=>CompleteReturn();page.Controls.Add(returnGrid);page.Controls.Add(top);return page;
        }

        private void ResumeSelected(){try{if(heldGrid.CurrentRow==null)return;int id=Convert.ToInt32(heldGrid.CurrentRow.Cells["held_sale_id"].Value);SalesLifecycleService.CheckoutSourceData source=service.LoadHeldSale(id);SalesAdvancedUiService.QueueResume(source);using(var sales=new Sales(LoginForm.LoggedInUsername,AppSession.Role))sales.ShowDialog(this);LoadHeld();}catch(Exception ex){HandleError(ex,"RESUME_SALE");}}
        private void ConvertQuotation(){try{if(quotationGrid.CurrentRow==null)return;int id=Convert.ToInt32(quotationGrid.CurrentRow.Cells["quotation_id"].Value);SalesLifecycleService.CheckoutSourceData source=service.LoadQuotation(id);SalesAdvancedUiService.QueueResume(source);using(var sales=new Sales(LoginForm.LoggedInUsername,AppSession.Role))sales.ShowDialog(this);LoadQuotations();}catch(Exception ex){HandleError(ex,"CONVERT_QUOTATION");}}
        private void LoadReturnItems()
        {
            if(!int.TryParse(saleIdText.Text.Trim(),out int saleId)||saleId<=0){MessageBox.Show("Enter a valid Sale ID.");return;}
            try{List<SalesLifecycleService.ReturnLine> rows=service.GetReturnableSaleItems(saleId);var table=new DataTable();table.Columns.Add("sale_item_id",typeof(int));table.Columns.Add("product_id",typeof(int));table.Columns.Add("product",typeof(string));table.Columns.Add("sold_qty",typeof(int));table.Columns.Add("returned_qty",typeof(int));table.Columns.Add("available_qty",typeof(int));table.Columns.Add("return_qty",typeof(int));table.Columns.Add("unit_price",typeof(decimal));table.Columns.Add("restock",typeof(bool));foreach(var line in rows)table.Rows.Add(line.SaleItemId,line.ProductId,line.ProductName,line.SoldQuantity,line.AlreadyReturned,Math.Max(0,line.SoldQuantity-line.AlreadyReturned),0,line.UnitPrice,true);returnGrid.DataSource=table;if(returnGrid.Columns.Contains("sale_item_id"))returnGrid.Columns["sale_item_id"].Visible=false;if(returnGrid.Columns.Contains("product_id"))returnGrid.Columns["product_id"].Visible=false;foreach(DataGridViewColumn c in returnGrid.Columns)c.ReadOnly=c.Name!="return_qty"&&c.Name!="restock";}catch(Exception ex){HandleError(ex,"LOAD_RETURN");}
        }
        private void CompleteReturn()
        {
            try
            {
                ActionPermissionService.Demand("SALE.RETURN");
                if(!int.TryParse(saleIdText.Text.Trim(),out int saleId)||saleId<=0)throw new InvalidOperationException("Enter a valid Sale ID.");
                var lines=new List<SalesLifecycleService.ReturnLine>();foreach(DataGridViewRow row in returnGrid.Rows){if(row.IsNewRow)continue;int qty=ToInt(row.Cells["return_qty"].Value);if(qty<=0)continue;lines.Add(new SalesLifecycleService.ReturnLine{SaleItemId=ToInt(row.Cells["sale_item_id"].Value),ProductId=ToInt(row.Cells["product_id"].Value),ProductName=Convert.ToString(row.Cells["product"].Value),SoldQuantity=ToInt(row.Cells["sold_qty"].Value),AlreadyReturned=ToInt(row.Cells["returned_qty"].Value),ReturnQuantity=qty,UnitPrice=Convert.ToDecimal(row.Cells["unit_price"].Value),Restock=Convert.ToBoolean(row.Cells["restock"].Value)});}
                if(string.Equals(Convert.ToString(returnType.SelectedItem),"EXCHANGE",StringComparison.OrdinalIgnoreCase))
                {
                    CheckoutService.Request replacement=PromptReplacement();if(replacement==null)return;
                    SalesLifecycleService.ExchangeResult exchange=service.ProcessExchange(saleId,AppSession.UserId,reasonText.Text,lines,replacement);
                    MessageBox.Show("Exchange completed atomically.\nReturn: "+exchange.ReturnId+"\nReplacement sale: "+exchange.ReplacementSaleId+"\nDifference: "+exchange.Difference.ToString("N2"),"Exchange",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                else
                {
                    int id=service.ProcessReturn(saleId,AppSession.UserId,"RETURN",Convert.ToString(refundMethod.SelectedItem),reasonText.Text,lines);MessageBox.Show("Return completed. Return ID: "+id,"Return",MessageBoxButtons.OK,MessageBoxIcon.Information);
                }
                LoadReturnItems();
            }catch(Exception ex){HandleError(ex,"COMPLETE_RETURN");}
        }

        private CheckoutService.Request PromptReplacement()
        {
            DataTable products=new InventoryOperationsService(POS_System.Program.SettingsManager.ConnectionString).Products();
            using(var dialog=new Form{Text="Exchange Replacement",StartPosition=FormStartPosition.CenterParent,Width=520,Height=310,Font=new Font("Segoe UI",10F),BackColor=Color.White,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false})
            {
                var product=new ComboBox{Left=150,Top=25,Width=320,DropDownStyle=ComboBoxStyle.DropDownList,DataSource=products,DisplayMember="name",ValueMember="product_id"};
                var quantity=new NumericUpDown{Left=150,Top=70,Width=130,Minimum=1,Maximum=100000,Value=1};
                var method=new ComboBox{Left=150,Top=115,Width=180,DropDownStyle=ComboBoxStyle.DropDownList};method.Items.AddRange(new object[]{"CASH","CARD","BANK","OTHER"});method.SelectedIndex=0;
                var paid=new NumericUpDown{Left=150,Top=160,Width=180,DecimalPlaces=2,Maximum=1000000000};
                var ok=Button("Complete Exchange",300);ok.Top=210;ok.Width=170;ok.DialogResult=DialogResult.OK;
                dialog.Controls.AddRange(new Control[]{new Label{Text="Replacement product",Left=18,Top=30,AutoSize=true},product,new Label{Text="Quantity",Left=18,Top=75,AutoSize=true},quantity,new Label{Text="Extra payment method",Left=18,Top=120,AutoSize=true},method,new Label{Text="Extra amount paid",Left=18,Top=165,AutoSize=true},paid,ok});dialog.AcceptButton=ok;
                if(dialog.ShowDialog(this)!=DialogResult.OK)return null;
                int productId=Convert.ToInt32(product.SelectedValue);DataRowView selected=product.SelectedItem as DataRowView;
                var request=new CheckoutService.Request{Username=LoginForm.LoggedInUsername,OperationKey=Guid.NewGuid()};
                request.Lines.Add(new CheckoutService.Line{ProductId=productId,ProductName=selected==null?product.Text:Convert.ToString(selected["name"]),Quantity=Convert.ToInt32(quantity.Value),UnitPrice=0m,OriginalUnitPrice=0m});
                if(paid.Value>0m)request.Payments.Add(new CheckoutService.Payment{Method=Convert.ToString(method.SelectedItem),Amount=paid.Value});
                return request;
            }
        }

        private void RefreshAll(){LoadHeld();LoadQuotations();}
        private void LoadHeld(){try{heldGrid.DataSource=service.GetHeldSales();}catch(Exception ex){HandleError(ex,"LOAD_HELD");}}
        private void LoadQuotations(){try{quotationGrid.DataSource=service.GetQuotations();}catch(Exception ex){HandleError(ex,"LOAD_QUOTATIONS");}}
        private static void HandleError(Exception ex,string op){ErrorLogService.Log(ex,op,nameof(SalesLifecycleForm));MessageBox.Show(ex.Message,"Sales Operations",MessageBoxButtons.OK,MessageBoxIcon.Error);}
        private static TabPage NewPage(string text){return new TabPage(text){BackColor=ModernUiService.AppBackground,Padding=new Padding(8)};}
        private static Panel NewBar(){return new Panel{Dock=DockStyle.Top,Height=58,BackColor=Color.White};}
        private static DataGridView CreateGrid(){return new DataGridView{Dock=DockStyle.Fill,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AllowUserToAddRows=false,AllowUserToDeleteRows=false,BackgroundColor=Color.White};}
        private static Button Button(string text,int left){var b=new Button{Text=text,Left=left,Top=10,Width=105,Height=36,BackColor=ModernUiService.Primary,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};b.FlatAppearance.BorderSize=0;return b;}
        private static int ToInt(object value){return int.TryParse(Convert.ToString(value),out int v)?v:0;}
    }
}
