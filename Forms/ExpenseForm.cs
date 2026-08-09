using Pos_System.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class ExpenseForm : Form
    {
        private readonly string cs = POS_System.Program.SettingsManager.ConnectionString;
        private ComboBox category;
        private ComboBox currency;
        private ComboBox method;
        private NumericUpDown amount;
        private TextBox description;
        private TextBox reference;
        private DateTimePicker date;
        private DataGridView grid;

        public ExpenseForm()
        {
            Text = "Expenses";
            MinimumSize = new Size(1000, 650);
            BackColor = Color.FromArgb(244,247,252);
            BuildUi();
            Load += (s,e) => { LoadCategories(); LoadExpenses(); };
        }

        private void BuildUi()
        {
            var top = new Panel { Dock=DockStyle.Top, Height=165, BackColor=Color.White, Padding=new Padding(14) };
            top.Controls.Add(new Label { Text="Expenses", Location=new Point(14,10), AutoSize=true, Font=new Font("Segoe UI",16F,FontStyle.Bold) });
            category = C(90,55,180); currency=C(365,55,90); currency.Items.AddRange(new object[]{"USD","LBP"}); currency.SelectedIndex=0;
            amount = new NumericUpDown { Location=new Point(535,55), Width=145, DecimalPlaces=2, Maximum=9999999999999999m, ThousandsSeparator=true };
            method=C(785,55,120); method.Items.AddRange(new object[]{"CASH","CARD","BANK","OTHER"}); method.SelectedIndex=0;
            date = new DateTimePicker { Location=new Point(90,105), Width=180, Format=DateTimePickerFormat.Short };
            description = new TextBox { Location=new Point(365,105), Width=315 };
            reference = new TextBox { Location=new Point(785,105), Width=120 };
            var save = new Button { Text="Save Expense", Location=new Point(920,72), Width=120, Height=45, BackColor=Color.FromArgb(220,38,38), ForeColor=Color.White, FlatStyle=FlatStyle.Flat };
            save.Click += Save_Click;
            top.Controls.AddRange(new Control[]{L("Category",14,59),category,L("Currency",295,59),currency,L("Amount",475,59),amount,L("Method",720,59),method,L("Date",14,109),date,L("Description",285,109),description,L("Reference",700,109),reference,save});
            grid = new DataGridView { Dock=DockStyle.Fill, ReadOnly=true, AllowUserToAddRows=false, RowHeadersVisible=false, BackgroundColor=Color.White, AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill };
            Controls.Add(grid); Controls.Add(top);
        }

        private ComboBox C(int x,int y,int w) => new ComboBox { Location=new Point(x,y), Width=w, DropDownStyle=ComboBoxStyle.DropDownList };
        private Label L(string t,int x,int y) => new Label { Text=t, Location=new Point(x,y), AutoSize=true, Font=new Font("Segoe UI",9F,FontStyle.Bold) };

        private void LoadCategories()
        {
            using(var a=new SqlDataAdapter("SELECT expense_category_id,name FROM dbo.ExpenseCategories WHERE is_active=1 ORDER BY name",cs))
            { var dt=new DataTable(); a.Fill(dt); category.DataSource=dt; category.DisplayMember="name"; category.ValueMember="expense_category_id"; }
        }

        private void LoadExpenses()
        {
            using(var a=new SqlDataAdapter(@"SELECT TOP 300 e.expense_id AS Id,c.name AS Category,e.amount AS Amount,e.currency AS Currency,e.expense_date AS Date,e.payment_method AS Method,e.reference_number AS Reference,e.description AS Description FROM dbo.Expenses e INNER JOIN dbo.ExpenseCategories c ON c.expense_category_id=e.expense_category_id ORDER BY e.expense_date DESC,e.expense_id DESC",cs))
            { var dt=new DataTable(); a.Fill(dt); grid.DataSource=dt; }
        }

        private void Save_Click(object sender,EventArgs e)
        {
            try
            {
                ActionPermissionService.Demand("EXPENSE.CREATE");
                if(AppSession.UserId<=0) throw new InvalidOperationException("No logged-in user.");
                if(category.SelectedValue==null) throw new InvalidOperationException("Select expense category.");
                if(amount.Value<=0) throw new InvalidOperationException("Amount must be greater than zero.");
                using(var conn=new SqlConnection(cs))
                {
                    conn.Open();using(var tx=conn.BeginTransaction(IsolationLevel.Serializable))
                    try
                    {
                        int expenseId;
                        using(var cmd=new SqlCommand(@"INSERT dbo.Expenses(expense_category_id,amount,currency,expense_date,description,payment_method,reference_number,user_id,exchange_rate) VALUES(@c,@a,@cur,@d,@desc,@m,@r,@u,@rate);SELECT CAST(SCOPE_IDENTITY() AS INT);",conn,tx))
                        {
                            cmd.Parameters.Add("@c",SqlDbType.Int).Value=Convert.ToInt32(category.SelectedValue);
                            var p=cmd.Parameters.Add("@a",SqlDbType.Decimal);p.Precision=24;p.Scale=8;p.Value=amount.Value;
                            cmd.Parameters.Add("@cur",SqlDbType.NVarChar,10).Value=Convert.ToString(currency.SelectedItem);
                            cmd.Parameters.Add("@d",SqlDbType.DateTime2).Value=date.Value;
                            cmd.Parameters.Add("@desc",SqlDbType.NVarChar,1000).Value=string.IsNullOrWhiteSpace(description.Text)?(object)DBNull.Value:description.Text.Trim();
                            cmd.Parameters.Add("@m",SqlDbType.NVarChar,50).Value=Convert.ToString(method.SelectedItem);
                            cmd.Parameters.Add("@r",SqlDbType.NVarChar,100).Value=string.IsNullOrWhiteSpace(reference.Text)?(object)DBNull.Value:reference.Text.Trim();
                            cmd.Parameters.Add("@u",SqlDbType.Int).Value=AppSession.UserId;
                            var rate=cmd.Parameters.Add("@rate",SqlDbType.Decimal);rate.Precision=24;rate.Scale=8;rate.Value=POS_System.Program.SettingsManager.GetExchangeRate();
                            expenseId=Convert.ToInt32(cmd.ExecuteScalar());
                        }
                        if(string.Equals(Convert.ToString(method.SelectedItem),"CASH",StringComparison.OrdinalIgnoreCase))
                        using(var cmd=new SqlCommand(@"DECLARE @session INT=(SELECT TOP(1) cash_session_id FROM dbo.CashSessions WITH(UPDLOCK,HOLDLOCK) WHERE user_id=@u AND status=N'OPEN' ORDER BY opened_at DESC);IF @session IS NULL THROW 52920,'Open a cash shift before recording a cash expense.',1;INSERT dbo.CashMovements(cash_session_id,movement_type,amount,currency,reference_type,reference_id,description,user_id) VALUES(@session,N'EXPENSE',@a,@cur,N'EXPENSE',@id,@desc,@u);",conn,tx))
                        {cmd.Parameters.AddWithValue("@u",AppSession.UserId);var p=cmd.Parameters.Add("@a",SqlDbType.Decimal);p.Precision=24;p.Scale=8;p.Value=amount.Value;cmd.Parameters.AddWithValue("@cur",Convert.ToString(currency.SelectedItem));cmd.Parameters.AddWithValue("@id",expenseId);cmd.Parameters.AddWithValue("@desc",string.IsNullOrWhiteSpace(description.Text)?"Expense":description.Text.Trim());cmd.ExecuteNonQuery();}
                        tx.Commit();
                    }
                    catch{tx.Rollback();throw;}
                }
                AuditService.Log("Expenses","Create",null,"Created expense " + amount.Value.ToString("N2") + " " + Convert.ToString(currency.SelectedItem));
                amount.Value=0;description.Clear();reference.Clear();LoadExpenses();
                MessageBox.Show("Expense saved successfully.");
            }
            catch(Exception ex){MessageBox.Show("Expense could not be saved: "+ex.Message);}
        }
    }
}
