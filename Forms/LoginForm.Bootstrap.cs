using Pos_System.Services;
using Pos_System.Models;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System
{
    public partial class LoginForm
    {
        static LoginForm()
        {
            ModernUiService.EnableGlobalTheme();
            ErrorLogService.Enable();
        }

        private void OfferFirstAdministratorSetup()
        {
            try
            {
                if(_context.Users.Any()||btnlogin.Parent.Controls.Find("firstAdminLink",false).Length>0)return;
                var link=new LinkLabel{Name="firstAdminLink",Text="Create the first administrator account",AutoSize=true,Left=btnlogin.Left,Top=btnlogin.Bottom+7,LinkColor=Color.FromArgb(37,99,235)};
                link.Click+=(s,e)=>{try{CreateFirstAdministrator(link);}catch(Exception ex){ErrorLogService.Log(ex,"FIRST_ADMIN_CREATE",nameof(LoginForm));MessageBox.Show(ex.Message,"First Administrator",MessageBoxButtons.OK,MessageBoxIcon.Error);}};btnlogin.Parent.Controls.Add(link);link.BringToFront();
            }
            catch(Exception ex){ErrorLogService.Log(ex,"FIRST_ADMIN_CHECK",nameof(LoginForm));}
        }

        private void CreateFirstAdministrator(Control link)
        {
            using(var dialog=new Form{Text="First Administrator",StartPosition=FormStartPosition.CenterParent,Width=460,Height=330,Font=new Font("Segoe UI",10F),BackColor=Color.White,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false})
            {
                var username=new TextBox{Left=145,Top=45,Width=260,Text="admin"};var password=new TextBox{Left=145,Top=95,Width=260,UseSystemPasswordChar=true};var confirm=new TextBox{Left=145,Top=145,Width=260,UseSystemPasswordChar=true};var save=new Button{Text="Create Administrator",Left=225,Top=210,Width=180,Height=42,DialogResult=DialogResult.OK,BackColor=Color.FromArgb(37,99,235),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};save.FlatAppearance.BorderSize=0;
                dialog.Controls.AddRange(new Control[]{new Label{Text="Username",Left=30,Top=50,AutoSize=true},username,new Label{Text="Password",Left=30,Top=100,AutoSize=true},password,new Label{Text="Confirm",Left=30,Top=150,AutoSize=true},confirm,new Label{Text="Use at least 8 characters.",Left=145,Top=178,AutoSize=true,ForeColor=Color.DimGray},save});dialog.AcceptButton=save;
                if(dialog.ShowDialog(this)!=DialogResult.OK)return;
                string name=username.Text.Trim();if(name.Length<3)throw new InvalidOperationException("Administrator username must contain at least 3 characters.");if(password.Text.Length<8)throw new InvalidOperationException("Administrator password must contain at least 8 characters.");if(password.Text!=confirm.Text)throw new InvalidOperationException("Passwords do not match.");if(_context.Users.Any())throw new InvalidOperationException("An application user already exists; first-run setup is no longer available.");
                var user=new User{Username=name,Password_Hash=PasswordHasher.Hash(password.Text),Role="admin",Created_At=DateTime.UtcNow};_context.Users.Add(user);_context.SaveChanges();txtusername.Text=name;txtpassword.Clear();link.Visible=false;MessageBox.Show("Administrator created securely. Enter the new password to sign in.","First Administrator",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }
    }
}
