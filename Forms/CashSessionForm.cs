using Pos_System.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class CashSessionForm : Form
    {
        private readonly CashSessionService service = new CashSessionService(POS_System.Program.SettingsManager.ConnectionString);
        private int registerId;
        private int? sessionId;
        private Label status;
        private NumericUpDown usd;
        private NumericUpDown lbp;
        private TextBox notes;
        private Button action;

        public CashSessionForm()
        {
            Text="Cash Register / Shift"; Size=new Size(650,430); StartPosition=FormStartPosition.CenterParent; BackColor=Color.White;
            BuildUi(); Load+=(s,e)=>LoadState();
        }

        private void BuildUi()
        {
            Controls.Add(new Label{Text="Cash Register & Shift",Location=new Point(25,20),AutoSize=true,Font=new Font("Segoe UI",18F,FontStyle.Bold)});
            status=new Label{Location=new Point(25,65),AutoSize=true,Font=new Font("Segoe UI",11F,FontStyle.Bold)};Controls.Add(status);
            Controls.Add(L("USD cash",25,120)); usd=N(170,115);Controls.Add(usd);
            Controls.Add(L("LBP cash",25,170)); lbp=N(170,165);Controls.Add(lbp);
            Controls.Add(L("Notes",25,220)); notes=new TextBox{Location=new Point(170,215),Width=390,Height=70,Multiline=true};Controls.Add(notes);
            action=new Button{Location=new Point(170,310),Width=230,Height=48,FlatStyle=FlatStyle.Flat,ForeColor=Color.White,Font=new Font("Segoe UI",11F,FontStyle.Bold)};action.Click+=Action_Click;Controls.Add(action);
        }
        private Label L(string t,int x,int y)=>new Label{Text=t,Location=new Point(x,y),AutoSize=true,Font=new Font("Segoe UI",10F,FontStyle.Bold)};
        private NumericUpDown N(int x,int y)=>new NumericUpDown{Location=new Point(x,y),Width=230,DecimalPlaces=2,Maximum=9999999999999999m,ThousandsSeparator=true};

        private void LoadState()
        {
            try
            {
                registerId=service.EnsureRegister(Environment.MachineName);
                sessionId=service.GetOpenSessionId(registerId);
                if(sessionId.HasValue)
                {
                    service.GetExpected(sessionId.Value,out decimal eUsd,out decimal eLbp);
                    status.Text=$"OPEN shift #{sessionId.Value} | Expected: {eUsd:N2} USD / {eLbp:N2} LBP";
                    action.Text="Close Shift";action.BackColor=Color.FromArgb(220,38,38);usd.Value=Safe(eUsd);lbp.Value=Safe(eLbp);
                }
                else
                {
                    status.Text="No open shift on this computer.";action.Text="Open Shift";action.BackColor=Color.FromArgb(22,163,74);usd.Value=0;lbp.Value=0;
                }
            }
            catch(Exception ex){MessageBox.Show("Could not load cash shift: "+ex.Message);}
        }

        private decimal Safe(decimal v){if(v<0)return 0;if(v>usd.Maximum)return usd.Maximum;return v;}

        private void Action_Click(object sender,EventArgs e)
        {
            try
            {
                if(AppSession.UserId<=0)throw new InvalidOperationException("No logged-in user.");
                if(sessionId.HasValue)
                {
                    service.CloseSession(sessionId.Value,usd.Value,lbp.Value,notes.Text);
                    AuditService.Log("CashSession","Close",sessionId.Value.ToString(),"Closed cashier shift");
                    MessageBox.Show("Shift closed successfully.");
                }
                else
                {
                    int id=service.OpenSession(registerId,AppSession.UserId,usd.Value,lbp.Value);
                    AuditService.Log("CashSession","Open",id.ToString(),"Opened cashier shift");
                    MessageBox.Show("Shift opened successfully. Shift #"+id);
                }
                notes.Clear();LoadState();
            }
            catch(Exception ex){MessageBox.Show("Shift operation failed: "+ex.Message);}
        }
    }
}
