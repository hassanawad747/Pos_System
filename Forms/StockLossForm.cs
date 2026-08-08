using Pos_System.Services;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    internal sealed class StockLossForm : Form
    {
        private readonly StockLossService service;
        private readonly ComboBox warehouse;
        private readonly ComboBox product;
        private readonly ComboBox type;
        private readonly ComboBox batch;
        private readonly NumericUpDown quantity;
        private readonly TextBox reason;
        private readonly DataGridView recent;

        public StockLossForm() : this(new StockLossService(POS_System.Program.SettingsManager.ConnectionString)) { }

        public StockLossForm(StockLossService service)
        {
            this.service=service ?? throw new ArgumentNullException(nameof(service));
            Text="Damage / Expired Stock";Dock=DockStyle.Fill;BackColor=ModernUiService.AppBackground;Font=new Font("Segoe UI",9.5F);
            var top=new Panel{Dock=DockStyle.Top,Height=225,BackColor=Color.White,Padding=new Padding(18)};
            warehouse=Combo(top,"Warehouse",20);product=Combo(top,"Product",60);type=Combo(top,"Type",100);type.Items.AddRange(new object[]{"DAMAGE","EXPIRED"});type.SelectedIndex=0;
            batch=Combo(top,"Batch (optional)",140);batch.DisplayMember="batch_number";batch.ValueMember="batch_number";
            quantity=new NumericUpDown{Left=540,Top=20,Width=130,Minimum=1,Maximum=1000000};top.Controls.Add(new Label{Text="Quantity",Left=465,Top=25,AutoSize=true});top.Controls.Add(quantity);
            reason=new TextBox{Left=540,Top=60,Width=300};top.Controls.Add(new Label{Text="Reason",Left=465,Top=65,AutoSize=true});top.Controls.Add(reason);
            var save=new Button{Text="Record Stock Loss",Left=540,Top=108,Width=160,Height=38,BackColor=Color.FromArgb(220,38,38),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};save.FlatAppearance.BorderSize=0;top.Controls.Add(save);
            recent=new DataGridView{Dock=DockStyle.Fill,AllowUserToAddRows=false,ReadOnly=true,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
            Controls.Add(recent);Controls.Add(top);
            Load+=(s,e)=>LoadLookups();product.SelectedValueChanged+=(s,e)=>LoadBatches();warehouse.SelectedValueChanged+=(s,e)=>LoadBatches();save.Click+=(s,e)=>SaveLoss();
        }

        private void LoadLookups()
        {
            try{DataTable w=service.Warehouses();warehouse.DataSource=w;warehouse.DisplayMember="name";warehouse.ValueMember="warehouse_id";DataTable p=service.Products();product.DataSource=p;product.DisplayMember="name";product.ValueMember="product_id";LoadBatches();LoadRecent();}catch(Exception ex){HandleError(ex);}
        }
        private void LoadBatches()
        {
            if(warehouse.SelectedValue==null||product.SelectedValue==null)return;
            if(!int.TryParse(Convert.ToString(warehouse.SelectedValue),out int w)||!int.TryParse(Convert.ToString(product.SelectedValue),out int p))return;
            try{DataTable t=service.Batches(p,w);DataRow empty=t.NewRow();empty["batch_number"]="";t.Rows.InsertAt(empty,0);batch.DataSource=t;}catch{}
        }
        private void SaveLoss()
        {
            try
            {
                int id=service.RecordLoss(Convert.ToInt32(warehouse.SelectedValue),Convert.ToInt32(product.SelectedValue),Convert.ToInt32(quantity.Value),Convert.ToString(type.SelectedItem),Convert.ToString(batch.SelectedValue),reason.Text,AppSession.UserId);
                MessageBox.Show("Stock loss recorded. Transaction #"+id,"Inventory",MessageBoxButtons.OK,MessageBoxIcon.Information);reason.Clear();LoadBatches();LoadRecent();
            }catch(Exception ex){HandleError(ex);}
        }
        private void LoadRecent()
        {
            try
            {
                using(var c=new System.Data.SqlClient.SqlConnection(POS_System.Program.SettingsManager.ConnectionString))using(var da=new System.Data.SqlClient.SqlDataAdapter(@"SELECT TOP(200) i.inventory_transaction_id,p.name product,i.transaction_type,i.quantity_change,w.name warehouse,i.batch_number,i.notes,i.created_at
FROM dbo.InventoryTransactions i JOIN dbo.Products p ON p.product_id=i.product_id LEFT JOIN dbo.Warehouses w ON w.warehouse_id=i.warehouse_id
WHERE i.transaction_type IN(N'DAMAGE',N'EXPIRED') ORDER BY i.created_at DESC;",c)){var t=new DataTable();da.Fill(t);recent.DataSource=t;}
            }catch(Exception ex){HandleError(ex);}
        }
        private static ComboBox Combo(Control parent,string label,int y){parent.Controls.Add(new Label{Text=label,Left=20,Top=y+5,Width=125});var c=new ComboBox{Left=150,Top=y,Width=280,DropDownStyle=ComboBoxStyle.DropDownList};parent.Controls.Add(c);return c;}
        private static void HandleError(Exception ex){ErrorLogService.Log(ex,"STOCK_LOSS",nameof(StockLossForm));MessageBox.Show(ex.Message,"Inventory",MessageBoxButtons.OK,MessageBoxIcon.Error);}
    }
}
