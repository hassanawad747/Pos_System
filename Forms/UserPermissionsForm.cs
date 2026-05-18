using Pos_System.Services;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class UserPermissionsForm : Form
    {
        private ComboBox comboUsers;
        private Label lblRoleValue;
        private CheckBox chkNotifications;
        private GroupBox grpDeleteNotifications;
        private RadioButton rdoDeleteNotificationsYes;
        private RadioButton rdoDeleteNotificationsNo;
        private DataGridView permissionGrid;
        private Button btnSave;
        private Button btnRefresh;
        private Button btnClose;
        private DataTable usersTable;

        public UserPermissionsForm()
        {
            InitializePermissionForm();
        }

        private void InitializePermissionForm()
        {
            Text = "User Permissions";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(920, 620);
            Size = new Size(1100, 700);
            BackColor = Color.FromArgb(245, 247, 250);

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(16),
                BackColor = Color.White
            };

            Label lblUser = new Label
            {
                AutoSize = true,
                Location = new Point(16, 18),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "User"
            };

            comboUsers = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Regular),
                Location = new Point(80, 14),
                Width = 260
            };
            comboUsers.SelectedIndexChanged += ComboUsers_SelectedIndexChanged;

            Label lblRole = new Label
            {
                AutoSize = true,
                Location = new Point(360, 18),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Text = "Role"
            };

            lblRoleValue = new Label
            {
                AutoSize = true,
                Location = new Point(410, 18),
                Font = new Font("Segoe UI", 11F, FontStyle.Regular),
                Text = "-"
            };

            chkNotifications = new CheckBox
            {
                AutoSize = true,
                Location = new Point(16, 52),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                Text = "Can see notifications"
            };

            grpDeleteNotifications = new GroupBox
            {
                Text = "Delete notifications",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(250, 45),
                Size = new Size(260, 40),
                Visible = false
            };

            rdoDeleteNotificationsYes = new RadioButton
            {
                AutoSize = true,
                Location = new Point(15, 16),
                Text = "Yes"
            };

            rdoDeleteNotificationsNo = new RadioButton
            {
                AutoSize = true,
                Checked = true,
                Location = new Point(82, 16),
                Text = "No"
            };

            grpDeleteNotifications.Controls.Add(rdoDeleteNotificationsYes);
            grpDeleteNotifications.Controls.Add(rdoDeleteNotificationsNo);

            btnRefresh = new Button
            {
                Text = "Reload",
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 120,
                Height = 36,
                Location = new Point(770, 14)
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;

            topPanel.Controls.Add(lblUser);
            topPanel.Controls.Add(comboUsers);
            topPanel.Controls.Add(lblRole);
            topPanel.Controls.Add(lblRoleValue);
            topPanel.Controls.Add(chkNotifications);
            topPanel.Controls.Add(grpDeleteNotifications);
            topPanel.Controls.Add(btnRefresh);

            permissionGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };

            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                Padding = new Padding(16),
                BackColor = Color.White
            };

            btnSave = new Button
            {
                Text = "Save Permissions",
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 180,
                Height = 40,
                Location = new Point(16, 14)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            btnClose = new Button
            {
                Text = "Close",
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Width = 120,
                Height = 40,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(940, 14)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (sender, args) => Close();

            bottomPanel.Controls.Add(btnSave);
            bottomPanel.Controls.Add(btnClose);

            Controls.Add(permissionGrid);
            Controls.Add(bottomPanel);
            Controls.Add(topPanel);

            Load += UserPermissionsForm_Load;
        }

        private void UserPermissionsForm_Load(object sender, EventArgs e)
        {
            if (!PermissionService.CanManagePermissions(AppSession.UserId, AppSession.Role))
            {
                MessageBox.Show("You do not have permission to manage user permissions.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            LoadUsers();
        }

        private void LoadUsers()
        {
            usersTable = PermissionService.GetUsersForPermissions();
            comboUsers.DataSource = usersTable;
            comboUsers.DisplayMember = "username";
            comboUsers.ValueMember = "user_id";

            if (comboUsers.Items.Count > 0)
            {
                comboUsers.SelectedIndex = 0;
                LoadSelectedUserPermissions();
            }
        }

        private void ComboUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedUserPermissions();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            using (LoadingOverlayService.Show(this, "Refreshing permissions..."))
            {
                LoadSelectedUserPermissions();
            }
        }

        private void LoadSelectedUserPermissions()
        {
            DataRowView selectedRow = comboUsers.SelectedItem as DataRowView;
            if (selectedRow == null)
            {
                permissionGrid.DataSource = null;
                lblRoleValue.Text = "-";
                chkNotifications.Checked = false;
                rdoDeleteNotificationsNo.Checked = true;
                return;
            }

            int userId = Convert.ToInt32(selectedRow["user_id"]);
            string role = Convert.ToString(selectedRow["role"]);

            lblRoleValue.Text = role;

            DataTable permissionTable = PermissionService.GetPermissionTableForUser(userId, role);
            permissionGrid.DataSource = permissionTable;

            if (permissionGrid.Columns.Contains("screen_key"))
            {
                permissionGrid.Columns["screen_key"].ReadOnly = true;
                permissionGrid.Columns["screen_key"].HeaderText = "Key";
            }

            if (permissionGrid.Columns.Contains("screen_name"))
            {
                permissionGrid.Columns["screen_name"].ReadOnly = true;
                permissionGrid.Columns["screen_name"].HeaderText = "Form";
            }

            SetPermissionColumnHeader("can_view", "View");
            SetPermissionColumnHeader("can_create", "Create");
            SetPermissionColumnHeader("can_edit", "Edit");
            SetPermissionColumnHeader("can_save", "Save");
            SetPermissionColumnHeader("can_delete", "Delete");

            chkNotifications.Checked = PermissionService.CanViewNotifications(userId, role);
            grpDeleteNotifications.Visible = PermissionService.CanGrantNotificationDeletePermission();
            rdoDeleteNotificationsYes.Checked = PermissionService.CanDeleteNotifications(userId, role);
            rdoDeleteNotificationsNo.Checked = !rdoDeleteNotificationsYes.Checked;
        }

        private void SetPermissionColumnHeader(string columnName, string headerText)
        {
            if (!permissionGrid.Columns.Contains(columnName))
            {
                return;
            }

            permissionGrid.Columns[columnName].HeaderText = headerText;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            DataRowView selectedRow = comboUsers.SelectedItem as DataRowView;
            DataTable permissionTable = permissionGrid.DataSource as DataTable;

            if (selectedRow == null || permissionTable == null)
            {
                MessageBox.Show("Please select a user first.");
                return;
            }

            int userId = Convert.ToInt32(selectedRow["user_id"]);
            string role = Convert.ToString(selectedRow["role"]);

            bool? canDeleteNotifications = grpDeleteNotifications.Visible ? (bool?)rdoDeleteNotificationsYes.Checked : null;
            using (LoadingOverlayService.Show(this, "Saving permissions..."))
            {
                PermissionService.SavePermissions(userId, role, permissionTable, chkNotifications.Checked, canDeleteNotifications);
            }
            MessageBox.Show("Permissions saved successfully.", "Permissions", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // UserPermissionsForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "UserPermissionsForm";
            this.Load += new System.EventHandler(this.UserPermissionsForm_Load_1);
            this.ResumeLayout(false);

        }

        private void UserPermissionsForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}
