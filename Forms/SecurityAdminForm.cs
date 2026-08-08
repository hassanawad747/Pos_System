using Pos_System.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    internal sealed class SecurityAdminForm : Form
    {
        private readonly DataGridView usersGrid;
        private readonly DataGridView permissionsGrid;
        private readonly Label legacyLabel;

        public SecurityAdminForm()
        {
            Text="Security & Reliability";Dock=DockStyle.Fill;BackColor=ModernUiService.AppBackground;Font=new Font("Segoe UI",9.5F);
            var tabs=new TabControl{Dock=DockStyle.Fill};
            usersGrid=Grid();permissionsGrid=Grid();legacyLabel=new Label{AutoSize=true,Left=15,Top=17,Font=new Font("Segoe UI",10F,FontStyle.Bold)};
            tabs.TabPages.Add(PasswordTab());tabs.TabPages.Add(PermissionTab());tabs.TabPages.Add(ErrorLogTab());Controls.Add(tabs);Load+=(s,e)=>RefreshAll();
        }

        private TabPage PasswordTab()
        {
            var p=Page("Password Security");var top=Bar(90);var refresh=Btn("Refresh",15,45);var disable=Btn("Disable Legacy Fallback",140,45);disable.Width=185;top.Controls.Add(legacyLabel);top.Controls.Add(refresh);top.Controls.Add(disable);refresh.Click+=(s,e)=>LoadUsers();disable.Click+=(s,e)=>DisableFallback();p.Controls.Add(usersGrid);p.Controls.Add(top);return p;
        }
        private TabPage PermissionTab()
        {
            var p=Page("Action Permissions");var top=Bar(58);var refresh=Btn("Refresh",15,10);var save=Btn("Save Selected",140,10);top.Controls.Add(refresh);top.Controls.Add(save);refresh.Click+=(s,e)=>LoadPermissions();save.Click+=(s,e)=>SavePermission();p.Controls.Add(permissionsGrid);p.Controls.Add(top);return p;
        }
        private TabPage ErrorLogTab()
        {
            var p=Page("Error Logs");var grid=Grid();var top=Bar(58);var refresh=Btn("Refresh",15,10);refresh.Click+=(s,e)=>{try{grid.DataSource=Query("SELECT TOP(500) error_log_id,occurred_at,user_id,machine_name,form_name,operation,message,severity FROM dbo.ErrorLogs ORDER BY occurred_at DESC;");}catch(Exception ex){ShowError(ex);}};top.Controls.Add(refresh);p.Controls.Add(grid);p.Controls.Add(top);return p;
        }

        private void RefreshAll(){LoadUsers();LoadPermissions();}
        private void LoadUsers()
        {
            try
            {
                DataTable t=Query("SELECT user_id,username,role,CASE WHEN password_hash LIKE N'PBKDF2$%' THEN N'PBKDF2' ELSE N'LEGACY' END password_format,created_at FROM dbo.Users ORDER BY username;");usersGrid.DataSource=t;
                int legacy=0;foreach(DataRow r in t.Rows)if(Convert.ToString(r["password_format"])=="LEGACY")legacy++;
                legacyLabel.Text="Legacy password accounts remaining: "+legacy+" | Fallback enabled: "+POS_System.Program.SettingsManager.GetBoolSetting("legacy_password_fallback_enabled",true);
            }catch(Exception ex){ShowError(ex);}
        }
        private void DisableFallback()
        {
            try
            {
                int legacy=Convert.ToInt32(Scalar("SELECT COUNT(*) FROM dbo.Users WHERE password_hash IS NULL OR password_hash NOT LIKE N'PBKDF2$%';"));
                if(legacy>0){MessageBox.Show("Cannot disable legacy fallback yet. "+legacy+" account(s) still need to log in once or have their password reset so they can be migrated to PBKDF2.","Password Security",MessageBoxButtons.OK,MessageBoxIcon.Warning);return;}
                var values=new Dictionary<string,string>{{"legacy_password_fallback_enabled","false"}};POS_System.Program.SettingsManager.SaveSettings(values);MessageBox.Show("Legacy plaintext/SHA256 password fallback is now disabled.");LoadUsers();
            }catch(Exception ex){ShowError(ex);}
        }
        private void LoadPermissions(){try{permissionsGrid.DataSource=Query("SELECT action_permission_id,role_name,action_key,is_allowed,updated_at FROM dbo.ActionPermissions ORDER BY role_name,action_key;");if(permissionsGrid.Columns.Contains("action_permission_id"))permissionsGrid.Columns["action_permission_id"].ReadOnly=true;if(permissionsGrid.Columns.Contains("role_name"))permissionsGrid.Columns["role_name"].ReadOnly=true;if(permissionsGrid.Columns.Contains("action_key"))permissionsGrid.Columns["action_key"].ReadOnly=true;}catch(Exception ex){ShowError(ex);}}
        private void SavePermission()
        {
            try{if(permissionsGrid.CurrentRow==null)return;int id=Convert.ToInt32(permissionsGrid.CurrentRow.Cells["action_permission_id"].Value);bool allowed=Convert.ToBoolean(permissionsGrid.CurrentRow.Cells["is_allowed"].Value);using(var c=Open())using(var cmd=new SqlCommand("UPDATE dbo.ActionPermissions SET is_allowed=@a,updated_by=@u,updated_at=SYSUTCDATETIME() WHERE action_permission_id=@id;",c)){cmd.Parameters.AddWithValue("@a",allowed);cmd.Parameters.AddWithValue("@u",AppSession.UserId);cmd.Parameters.AddWithValue("@id",id);cmd.ExecuteNonQuery();}MessageBox.Show("Permission saved.");}catch(Exception ex){ShowError(ex);}
        }

        private static SqlConnection Open(){var c=new SqlConnection(POS_System.Program.SettingsManager.ConnectionString);c.Open();return c;}
        private static DataTable Query(string sql){var t=new DataTable();using(var c=new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))using(var da=new SqlDataAdapter(sql,c))da.Fill(t);return t;}
        private static object Scalar(string sql){using(var c=Open())using(var cmd=new SqlCommand(sql,c))return cmd.ExecuteScalar();}
        private static void ShowError(Exception ex){ErrorLogService.Log(ex,"SECURITY_ADMIN",nameof(SecurityAdminForm));MessageBox.Show(ex.Message,"Security",MessageBoxButtons.OK,MessageBoxIcon.Error);}
        private static TabPage Page(string text)=>new TabPage(text){BackColor=ModernUiService.AppBackground,Padding=new Padding(8)};
        private static DataGridView Grid()=>new DataGridView{Dock=DockStyle.Fill,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,AllowUserToAddRows=false};
        private static Panel Bar(int h)=>new Panel{Dock=DockStyle.Top,Height=h,BackColor=Color.White};
        private static Button Btn(string text,int x,int y){var b=new Button{Text=text,Left=x,Top=y,Width=110,Height=34,BackColor=ModernUiService.Primary,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};b.FlatAppearance.BorderSize=0;return b;}
    }
}
