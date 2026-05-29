using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public partial class AddUsers : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;

        public AddUsers()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
        }


        private void clearinputs()
        {
            txtusername.Clear();
            txtpassword.Clear();
            txtConfirmPassword.Clear();
            ApplyDefaultRole();
        }

        private void ApplyDefaultRole()
        {
            string defaultRole = POS_System.Program.SettingsManager.GetSetting("default_role", "cashier")
                .Trim()
                .ToLowerInvariant();

            if (!CanShowAdminRoleOption() && defaultRole == "admin")
            {
                defaultRole = "cashier";
            }

            rdCashier.Checked = defaultRole == "cashier";
            rdManger.Checked = defaultRole == "manager";
            rdAdmin.Checked = defaultRole == "admin";

            if (!rdCashier.Checked && !rdManger.Checked && !rdAdmin.Checked)
            {
                rdCashier.Checked = true;
            }
        }

        private string GetSelectedRoleOrDefault()
        {
            if (rdCashier.Checked) return "cashier";
            if (rdManger.Checked) return "manager";
            if (rdAdmin.Checked && CanShowAdminRoleOption()) return "admin";

            string defaultRole = POS_System.Program.SettingsManager.GetSetting("default_role", "cashier").Trim().ToLowerInvariant();
            if (!CanShowAdminRoleOption() && defaultRole == "admin")
            {
                return "cashier";
            }

            return defaultRole;
        }

        private bool ValidatePasswordPolicy(string password, bool requirePassword)
        {
            int maxLength = POS_System.Program.SettingsManager.GetIntSetting("password_max_length", 12);

            if (string.IsNullOrEmpty(password))
            {
                if (requirePassword)
                {
                    MessageBox.Show("Password is required.");
                    return false;
                }

                return true;
            }

            if (password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters.");
                return false;
            }

            if (password.Length > maxLength)
            {
                MessageBox.Show($"Password cannot be longer than {maxLength} characters.");
                return false;
            }

            return true;
        }

        private void LoadUsers()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = CanShowAdminRoleOption()
                    ? "SELECT user_id, username, role, created_at, created_by FROM Users"
                    : "SELECT user_id, username, role, created_at, created_by FROM Users WHERE username <> 'admin'";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;

                if (dataGridView1.Columns.Contains("password_hash"))
                {
                    dataGridView1.Columns["password_hash"].Visible = false;
                }
            }
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanCreate(AppSession.UserId, AppSession.Role, PermissionService.ScreenUsers))
            {
                MessageBox.Show("You do not have permission to create users.");
                return;
            }

            string username = txtusername.Text.Trim();
            string password = txtpassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }

            if (!ValidatePasswordPolicy(password, true))
            {
                return;
            }

            string role = GetSelectedRoleOrDefault();

            if (string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Please select a role!");
                return;
            }

            if (role == "admin" && !CanShowAdminRoleOption())
            {
                MessageBox.Show("Only admin can create admin users.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "INSERT INTO Users (username, password_hash, role, created_by) VALUES (@username, @password, @role, @createdBy)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", PasswordHasher.Hash(password));
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@createdBy", LoginForm.LoggedInUsername);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        AuditService.Log("Users", "Create", username, "Created user " + username + " with role " + role);
                        MessageBox.Show("User created successfully!");
                        LoadUsers(); // refresh DataGridView
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
            clearinputs();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanDelete(AppSession.UserId, AppSession.Role, PermissionService.ScreenUsers))
            {
                MessageBox.Show("You do not have permission to delete users.");
                return;
            }

            if (dataGridView1.SelectedRows.Count > 0)
            {
                int userId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["user_id"].Value);

                // Show confirmation dialog
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this user?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("DELETE FROM Users WHERE user_id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", userId);
                        cmd.ExecuteNonQuery();

                        AuditService.Log("Users", "Delete", userId.ToString(), "Deleted user ID " + userId);
                        MessageBox.Show("User deleted successfully!", "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadUsers();   // refresh DataGridView
                        clearinputs(); // clear textboxes
                    }
                }
                else
                {
                    MessageBox.Show("Delete operation cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select a user to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                txtusername.Text = row.Cells["username"].Value.ToString();
                string role = row.Cells["role"].Value.ToString();

                rdCashier.Checked = role == "cashier";
                rdManger.Checked = role == "manager";
                rdAdmin.Checked = role == "admin" && CanShowAdminRoleOption();

                if (role == "admin" && !CanShowAdminRoleOption())
                {
                    rdCashier.Checked = false;
                    rdManger.Checked = false;
                    rdAdmin.Checked = false;
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanEdit(AppSession.UserId, AppSession.Role, PermissionService.ScreenUsers))
            {
                MessageBox.Show("You do not have permission to update users.");
                return;
            }

            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to edit.");
                return;
            }

            int userId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["user_id"].Value);
            string username = txtusername.Text.Trim();
            string password = txtpassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            if (!string.IsNullOrEmpty(password) && password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }

            if (!ValidatePasswordPolicy(password, false))
            {
                return;
            }

            string role = GetSelectedRoleOrDefault();

            if (string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Please select a role!");
                return;
            }

            if (role == "admin" && !CanShowAdminRoleOption())
            {
                MessageBox.Show("Only admin can assign admin role.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query;
                if (!string.IsNullOrEmpty(password))
                {
                    query = "UPDATE Users SET username=@username, password_hash=@password, role=@role WHERE user_id=@id";
                }
                else
                {
                    query = "UPDATE Users SET username=@username, role=@role WHERE user_id=@id";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@id", userId);

                    if (!string.IsNullOrEmpty(password))
                    {
                        cmd.Parameters.AddWithValue("@password", PasswordHasher.Hash(password));
                    }

                    try
                    {
                        cmd.ExecuteNonQuery();
                        AuditService.Log("Users", "Edit", userId.ToString(), "Updated user " + username + " with role " + role);
                        MessageBox.Show("User updated successfully!");
                        LoadUsers(); // refresh grid
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
            clearinputs();
        }

        private void AddUsers_Load(object sender, EventArgs e)
        {
            if (!PermissionService.EnsureScreenAccess(this, PermissionService.ScreenUsers))
            {
                return;
            }

            int maxLength = POS_System.Program.SettingsManager.GetIntSetting("password_max_length", 12);
            txtpassword.MaxLength = maxLength;
            txtConfirmPassword.MaxLength = maxLength;
            ConfigureAdminRoleVisibility();
            ApplyDefaultRole();
            LoadUsers();
            PermissionService.ApplyActionPermissions(this, PermissionService.ScreenUsers);
        }

        private void ConfigureAdminRoleVisibility()
        {
            bool canShowAdminRole = CanShowAdminRoleOption();
            rdAdmin.Visible = canShowAdminRole;
            rdAdmin.Enabled = canShowAdminRole;

            if (!canShowAdminRole && rdAdmin.Checked)
            {
                rdAdmin.Checked = false;
                rdCashier.Checked = true;
            }
        }

        private bool CanShowAdminRoleOption()
        {
            return AppSession.IsAdministrator;
        }
    }
}

