using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
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
            if (rdAdmin.Checked) return "admin";
            return POS_System.Program.SettingsManager.GetSetting("default_role", "cashier").Trim().ToLowerInvariant();
        }

        private bool ValidatePasswordPolicy(string password)
        {
            int maxLength = POS_System.Program.SettingsManager.GetIntSetting("password_max_length", 12);

            if (!string.IsNullOrEmpty(password) && password.Length > maxLength)
            {
                MessageBox.Show($"Password cannot be longer than {maxLength} characters.");
                return false;
            }

            return true;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void LoadUsers()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT user_id, username, role, created_at, created_by, password_hash FROM Users", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }


        private void btnAdd_Click(object sender, EventArgs e)
        {
            string username = txtusername.Text.Trim();
            string password = txtpassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }

            if (!ValidatePasswordPolicy(password))
            {
                return;
            }

            string role = GetSelectedRoleOrDefault();

            if (string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Please select a role!");
                return;
            }

            //string hashedPassword = HashPassword(password);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "INSERT INTO Users (username, password_hash, role, created_by) VALUES (@username, @password, @role, @createdBy)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@createdBy", LoginForm.LoggedInUsername); // track who created the account

                    try
                    {
                        cmd.ExecuteNonQuery();
<<<<<<< HEAD
                        AuditLogger.Log("ADD", "Users", null, "Created user: " + username + " / role: " + role);
=======
                        AuditService.Log("Users", "Create", username, "Created user " + username + " with role " + role);
>>>>>>> 19f309a5c7fd8647b5ac2d407bba710bbfe790f1
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

<<<<<<< HEAD
                        AuditLogger.Log("DELETE", "Users", userId, "Deleted user id: " + userId);
=======
                        AuditService.Log("Users", "Delete", userId.ToString(), "Deleted user ID " + userId);
>>>>>>> 19f309a5c7fd8647b5ac2d407bba710bbfe790f1
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
                rdAdmin.Checked = role == "admin";
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to edit.");
                return;
            }

            int userId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["user_id"].Value);
            string username = txtusername.Text.Trim();
            string password = txtpassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (!string.IsNullOrEmpty(password) && password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }

            if (!ValidatePasswordPolicy(password))
            {
                return;
            }

            string role = GetSelectedRoleOrDefault();

            if (string.IsNullOrEmpty(role))
            {
                MessageBox.Show("Please select a role!");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query;
                if (!string.IsNullOrEmpty(password))
                {
                    string hashedPassword = HashPassword(password);
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
                        cmd.Parameters.AddWithValue("@password",password);
                    }

                    try
                    {
                        cmd.ExecuteNonQuery();
<<<<<<< HEAD
                        AuditLogger.Log("EDIT", "Users", userId, "Updated user: " + username + " / role: " + role);
=======
                        AuditService.Log("Users", "Edit", userId.ToString(), "Updated user " + username + " with role " + role);
>>>>>>> 19f309a5c7fd8647b5ac2d407bba710bbfe790f1
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
            int maxLength = POS_System.Program.SettingsManager.GetIntSetting("password_max_length", 12);
            txtpassword.MaxLength = maxLength;
            txtConfirmPassword.MaxLength = maxLength;
            ApplyDefaultRole();
            LoadUsers();
        }
    }
}

