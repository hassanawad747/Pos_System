using Pos_System.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class CashReportForm : Form
    {
        private readonly string cs = POS_System.Program.SettingsManager.ConnectionString;
        private DataGridView grid;
        private Label title;

        public CashReportForm()
        {
            Text = "X / Z Cash Reports";
            MinimumSize = new Size(1100,650);
            BackColor = Color.FromArgb(244,247,252);
            BuildUi();
            Load += (s,e) => LoadReport(false);
        }

        private void BuildUi()
        {
            var top = new Panel { Dock=DockStyle.Top,Height=90,BackColor=Color.White };
            title = new Label { Text="X Report - Open Shift",Location=new Point(20,15),AutoSize=true,Font=new Font("Segoe UI",17F,FontStyle.Bold) };
            var x = B("X Report (Open)",650,20,160,Color.FromArgb(37,99,235)); x.Click+=(s,e)=>LoadReport(false);
            var z = B("Z History (Closed)",825,20,170,Color.FromArgb(79,70,229)); z.Click+=(s,e)=>LoadReport(true);
            top.Controls.AddRange(new Control[]{title,x,z});
            grid = new DataGridView { Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,RowHeadersVisible=false,BackgroundColor=Color.White,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,SelectionMode=DataGridViewSelectionMode.FullRowSelect };
            Controls.Add(grid);Controls.Add(top);
        }

        private Button B(string t,int x,int y,int w,Color c)
        {
            var b=new Button{Text=t,Location=new Point(x,y),Width=w,Height=42,BackColor=c,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};b.FlatAppearance.BorderSize=0;return b;
        }

        private void LoadReport(bool closed)
        {
            try
            {
                title.Text = closed ? "Z Report History - Closed Shifts" : "X Report - Current Open Shifts";
                string sql = @"SELECT cash_session_id AS Shift,register_name AS Register,computer_name AS Computer,username AS Cashier,opened_at AS Opened,closed_at AS Closed,status AS Status,opening_usd AS OpeningUSD,cash_in_usd AS InUSD,cash_out_usd AS OutUSD,calculated_expected_usd AS ExpectedUSD,actual_usd AS ActualUSD,difference_usd AS DifferenceUSD,opening_lbp AS OpeningLBP,cash_in_lbp AS InLBP,cash_out_lbp AS OutLBP,calculated_expected_lbp AS ExpectedLBP,actual_lbp AS ActualLBP,difference_lbp AS DifferenceLBP,movement_count AS Movements FROM dbo.vw_CashSessionSummary WHERE status=@status ORDER BY opened_at DESC";
                using(var conn=new SqlConnection(cs))
                using(var cmd=new SqlCommand(sql,conn))
                using(var adapter=new SqlDataAdapter(cmd))
                {
                    cmd.Parameters.Add("@status",SqlDbType.NVarChar,20).Value=closed?"CLOSED":"OPEN";
                    var dt=new DataTable();adapter.Fill(dt);grid.DataSource=dt;
                }
            }
            catch(Exception ex){MessageBox.Show("Could not load X/Z report: "+ex.Message);}
        }
    }
}
