using Pos_System.Services;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    internal sealed class InventoryOperationsForm : Form
    {
        private readonly InventoryOperationsService service;
        private readonly DataGridView healthGrid;
        private readonly ComboBox countWarehouse, countProduct, transferFrom, transferTo, transferProduct, catalogWarehouse, catalogProduct, unitProduct, unitSelect;
        private readonly NumericUpDown actualQty, transferQty, minStock, maxStock, reorderPoint, batchQty, unitFactor, unitPrice;
        private readonly TextBox countReason, transferNotes, barcodeText, batchText, serialText;
        private readonly DateTimePicker expiryDate;
        private readonly CheckBox hasExpiry, primaryBarcode, baseUnit;

        public InventoryOperationsForm()
        {
            service = new InventoryOperationsService(POS_System.Program.SettingsManager.ConnectionString);
            Text = "Inventory Operations"; Dock = DockStyle.Fill; Font = new Font("Segoe UI", 9.5F); BackColor = ModernUiService.AppBackground;
            var tabs = new TabControl { Dock = DockStyle.Fill };
            healthGrid = Grid();
            tabs.TabPages.Add(HealthTab());
            tabs.TabPages.Add(CountTab());
            tabs.TabPages.Add(TransferTab());
            tabs.TabPages.Add(CatalogTab());
            tabs.TabPages.Add(UnitTab());
            Controls.Add(tabs);
            Load += (s,e) => LoadLookups();
        }

        private TabPage HealthTab()
        {
            var p=Page("Stock Health / Reorder"); var top=Bar(); var refresh=Btn("Refresh",12); refresh.Click+=(s,e)=>LoadHealth(); top.Controls.Add(refresh); p.Controls.Add(healthGrid);p.Controls.Add(top);return p;
        }

        private TabPage CountTab()
        {
            var p=Page("Stock Count"); var panel=FormPanel(); int y=18;
            countWarehouse=Combo(panel,"Warehouse",y); y+=48; countProduct=Combo(panel,"Product",y); y+=48;
            actualQty=Number(panel,"Actual Quantity",y,8); y+=48; countReason=Text(panel,"Reason",y); y+=55;
            var save=Btn("Complete Count",160);save.Top=y;save.Width=150;panel.Controls.Add(save);save.Click+=(s,e)=>DoCount();p.Controls.Add(panel);return p;
        }

        private TabPage TransferTab()
        {
            var p=Page("Warehouse Transfer");var panel=FormPanel();int y=18;
            transferFrom=Combo(panel,"From",y);y+=48;transferTo=Combo(panel,"To",y);y+=48;transferProduct=Combo(panel,"Product",y);y+=48;transferQty=Number(panel,"Quantity",y,8);y+=48;transferNotes=Text(panel,"Notes",y);y+=55;
            var save=Btn("Transfer Stock",160);save.Top=y;save.Width=150;panel.Controls.Add(save);save.Click+=(s,e)=>DoTransfer();p.Controls.Add(panel);return p;
        }

        private TabPage CatalogTab()
        {
            var p=Page("Barcode / Batch / Serial / Reorder"); var panel=FormPanel(760);int y=18;
            catalogWarehouse=Combo(panel,"Warehouse",y);y+=48;catalogProduct=Combo(panel,"Product",y);y+=48;
            barcodeText=Text(panel,"New Barcode",y); primaryBarcode=new CheckBox{Text="Primary",Left=520,Top=y+4,AutoSize=true};panel.Controls.Add(primaryBarcode);y+=48;
            var addBarcode=Btn("Add Barcode",160);addBarcode.Top=y;addBarcode.Click+=(s,e)=>DoBarcode();panel.Controls.Add(addBarcode);y+=52;
            batchText=Text(panel,"Batch Number",y);y+=48;batchQty=Number(panel,"Batch Quantity",y,8);y+=48;expiryDate=new DateTimePicker{Left=160,Top=y,Width=220};hasExpiry=new CheckBox{Text="Has Expiry",Left=390,Top=y+4,AutoSize=true};panel.Controls.Add(new Label{Text="Expiry",Left=20,Top=y+5,AutoSize=true});panel.Controls.Add(expiryDate);panel.Controls.Add(hasExpiry);y+=48;
            var addBatch=Btn("Save Batch",160);addBatch.Top=y;addBatch.Click+=(s,e)=>DoBatch();panel.Controls.Add(addBatch);y+=52;
            serialText=Text(panel,"Serial / IMEI",y);y+=48;var addSerial=Btn("Add Serial",160);addSerial.Top=y;addSerial.Click+=(s,e)=>DoSerial();panel.Controls.Add(addSerial);y+=55;
            minStock=Number(panel,"Minimum Stock",y,8);y+=48;maxStock=Number(panel,"Maximum Stock",y,8);y+=48;reorderPoint=Number(panel,"Reorder Point",y,8);y+=48;var saveReorder=Btn("Save Reorder",160);saveReorder.Top=y;saveReorder.Click+=(s,e)=>DoReorder();panel.Controls.Add(saveReorder);
            p.AutoScroll=true;p.Controls.Add(panel);return p;
        }

        private TabPage UnitTab()
        {
            var p=Page("Units & Conversion");var panel=FormPanel();int y=18;
            unitProduct=Combo(panel,"Product",y);y+=48;unitSelect=Combo(panel,"Unit",y);y+=48;unitFactor=Number(panel,"Conversion Factor",y,8);unitFactor.Value=1;y+=48;unitPrice=Number(panel,"Unit Selling Price",y,8);y+=48;baseUnit=new CheckBox{Text="Base unit",Left=160,Top=y,AutoSize=true};panel.Controls.Add(baseUnit);y+=40;var save=Btn("Add Product Unit",160);save.Top=y;save.Width=150;save.Click+=(s,e)=>DoUnit();panel.Controls.Add(save);p.Controls.Add(panel);return p;
        }

        private void LoadLookups()
        {
            DataTable wh=service.Warehouses();DataTable products=service.Products();DataTable units=service.Units();
            Bind(countWarehouse,wh.Copy(),"name","warehouse_id");Bind(transferFrom,wh.Copy(),"name","warehouse_id");Bind(transferTo,wh.Copy(),"name","warehouse_id");Bind(catalogWarehouse,wh.Copy(),"name","warehouse_id");
            Bind(countProduct,products.Copy(),"name","product_id");Bind(transferProduct,products.Copy(),"name","product_id");Bind(catalogProduct,products.Copy(),"name","product_id");Bind(unitProduct,products.Copy(),"name","product_id");Bind(unitSelect,units.Copy(),"name","unit_id");
            LoadHealth();
        }

        private void LoadHealth(){try{healthGrid.DataSource=service.StockHealth();}catch(Exception ex){Error(ex);}}
        private void DoCount(){try{int id=service.CompleteStockCount(Id(countWarehouse),Id(countProduct),actualQty.Value,countReason.Text,AppSession.UserId);MessageBox.Show("Stock count completed. #"+id);LoadHealth();}catch(Exception ex){Error(ex);}}
        private void DoTransfer(){try{int id=service.Transfer(Id(transferFrom),Id(transferTo),Id(transferProduct),transferQty.Value,transferNotes.Text,AppSession.UserId);MessageBox.Show("Transfer completed. #"+id);LoadHealth();}catch(Exception ex){Error(ex);}}
        private void DoBarcode(){try{service.AddBarcode(Id(catalogProduct),null,barcodeText.Text,primaryBarcode.Checked);MessageBox.Show("Barcode added.");barcodeText.Clear();}catch(Exception ex){Error(ex);}}
        private void DoBatch(){try{service.AddBatch(Id(catalogProduct),Id(catalogWarehouse),batchText.Text,hasExpiry.Checked?(DateTime?)expiryDate.Value.Date:null,batchQty.Value,null);MessageBox.Show("Batch saved.");}catch(Exception ex){Error(ex);}}
        private void DoSerial(){try{service.AddSerial(Id(catalogProduct),Id(catalogWarehouse),serialText.Text);MessageBox.Show("Serial added.");serialText.Clear();}catch(Exception ex){Error(ex);}}
        private void DoReorder(){try{service.SetReorder(Id(catalogWarehouse),Id(catalogProduct),minStock.Value,maxStock.Value,reorderPoint.Value);MessageBox.Show("Reorder settings saved.");LoadHealth();}catch(Exception ex){Error(ex);}}
        private void DoUnit(){try{int id=service.AddProductUnit(Id(unitProduct),Id(unitSelect),unitFactor.Value,baseUnit.Checked,unitPrice.Value>0?(decimal?)unitPrice.Value:null);MessageBox.Show("Product unit added. #"+id);}catch(Exception ex){Error(ex);}}

        private static int Id(ComboBox c){if(c.SelectedValue==null||!int.TryParse(Convert.ToString(c.SelectedValue),out int v))throw new InvalidOperationException("Select a valid value.");return v;}
        private static void Bind(ComboBox c,DataTable t,string display,string value){c.DataSource=t;c.DisplayMember=display;c.ValueMember=value;}
        private static void Error(Exception ex){MessageBox.Show(ex.Message,"Inventory",MessageBoxButtons.OK,MessageBoxIcon.Error);}
        private static TabPage Page(string text)=>new TabPage(text){BackColor=ModernUiService.AppBackground,Padding=new Padding(8)};
        private static Panel Bar()=>new Panel{Dock=DockStyle.Top,Height=58,BackColor=Color.White};
        private static Panel FormPanel(int width=680)=>new Panel{Dock=DockStyle.Top,Height=760,Width=width,BackColor=Color.White,Padding=new Padding(20)};
        private static DataGridView Grid()=>new DataGridView{Dock=DockStyle.Fill,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AllowUserToAddRows=false,ReadOnly=true};
        private static Button Btn(string text,int left){var b=new Button{Text=text,Left=left,Top=10,Width=110,Height=36,BackColor=ModernUiService.Primary,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};b.FlatAppearance.BorderSize=0;return b;}
        private static ComboBox Combo(Control p,string label,int y){p.Controls.Add(new Label{Text=label,Left=20,Top=y+5,Width=130});var c=new ComboBox{Left=160,Top=y,Width=340,DropDownStyle=ComboBoxStyle.DropDownList};p.Controls.Add(c);return c;}
        private static TextBox Text(Control p,string label,int y){p.Controls.Add(new Label{Text=label,Left=20,Top=y+5,Width=130});var t=new TextBox{Left=160,Top=y,Width=340};p.Controls.Add(t);return t;}
        private static NumericUpDown Number(Control p,string label,int y,int decimals){p.Controls.Add(new Label{Text=label,Left=20,Top=y+5,Width=130});var n=new NumericUpDown{Left=160,Top=y,Width=220,DecimalPlaces=decimals,Maximum=1000000000,Minimum=0};p.Controls.Add(n);return n;}
    }
}
