using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Pos_System.Services
{
    internal static class PermissionService
    {
        internal const string GlobalScreenKey = "GLOBAL";
        internal const string ScreenDashboard = "Dashboard";
        internal const string ScreenSales = "Sales";
        internal const string ScreenProducts = "Products";
        internal const string ScreenSuppliers = "Suppliers";
        internal const string ScreenCustomers = "Customers";
        internal const string ScreenReports = "Reports";
        internal const string ScreenEarningReports = "EarningReports";
        internal const string ScreenWarehouse = "Warehouse";
        internal const string ScreenSettings = "Settings";
        internal const string ScreenUsers = "Users";
        internal const string ScreenOptions = "Options";
        internal const string ScreenUseDiscount = "UseDiscount";
        internal const string ScreenManualDiscount = "ManualDiscount";
        internal const string ScreenDiscountSettings = "DiscountSettings";
        internal const string ScreenDashboardBalances = "DashboardBalances";

        private sealed class ScreenDefinition
        {
            public ScreenDefinition(string key, string title)
            {
                Key = key;
                Title = title;
            }

            public string Key { get; }
            public string Title { get; }
        }

        private static readonly ScreenDefinition[] ScreenDefinitions =
        {
            new ScreenDefinition(ScreenDashboard, "Dashboard"),
            new ScreenDefinition(ScreenSales, "Sales"),
            new ScreenDefinition(ScreenProducts, "Products"),
            new ScreenDefinition(ScreenSuppliers, "Suppliers"),
            new ScreenDefinition(ScreenCustomers, "Customers"),
            new ScreenDefinition(ScreenReports, "Reports"),
            new ScreenDefinition(ScreenEarningReports, "Earning Reports"),
            new ScreenDefinition(ScreenWarehouse, "Warehouse"),
            new ScreenDefinition(ScreenSettings, "Settings"),
            new ScreenDefinition(ScreenUsers, "Users"),
            new ScreenDefinition(ScreenOptions, "Options"),
            new ScreenDefinition(ScreenUseDiscount, "Use Discount"),
            new ScreenDefinition(ScreenManualDiscount, "Manual Discount Textbox"),
            new ScreenDefinition(ScreenDiscountSettings, "Discount Settings"),
            new ScreenDefinition(ScreenDashboardBalances, "Dashboard Balance Tables")
        };

        public static void EnsurePermissionsTable()
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                IF OBJECT_ID(N'dbo.UserPermissions', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.UserPermissions
                    (
                        permission_id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        user_id INT NOT NULL,
                        screen_key NVARCHAR(100) NOT NULL,
                        can_view BIT NOT NULL CONSTRAINT DF_UserPermissions_can_view DEFAULT (0),
                        can_create BIT NOT NULL CONSTRAINT DF_UserPermissions_can_create DEFAULT (0),
                        can_edit BIT NOT NULL CONSTRAINT DF_UserPermissions_can_edit DEFAULT (0),
                        can_save BIT NOT NULL CONSTRAINT DF_UserPermissions_can_save DEFAULT (0),
                        can_delete BIT NOT NULL CONSTRAINT DF_UserPermissions_can_delete DEFAULT (0),
                        can_view_notifications BIT NOT NULL CONSTRAINT DF_UserPermissions_can_view_notifications DEFAULT (0),
                        can_delete_notifications BIT NOT NULL CONSTRAINT DF_UserPermissions_can_delete_notifications DEFAULT (0),
                        is_customized BIT NOT NULL CONSTRAINT DF_UserPermissions_is_customized DEFAULT (0),
                        updated_at DATETIME2 NOT NULL CONSTRAINT DF_UserPermissions_updated_at DEFAULT SYSUTCDATETIME(),
                        updated_by NVARCHAR(100) NULL
                    );

                    CREATE UNIQUE INDEX UX_UserPermissions_User_Screen
                    ON dbo.UserPermissions(user_id, screen_key);
                END;

                IF COL_LENGTH('dbo.UserPermissions', 'is_customized') IS NULL
                    ALTER TABLE dbo.UserPermissions ADD is_customized BIT NOT NULL CONSTRAINT DF_UserPermissions_is_customized_upgrade DEFAULT (0);

                IF COL_LENGTH('dbo.UserPermissions', 'can_delete_notifications') IS NULL
                    ALTER TABLE dbo.UserPermissions ADD can_delete_notifications BIT NOT NULL CONSTRAINT DF_UserPermissions_can_delete_notifications_upgrade DEFAULT (0);

                IF COL_LENGTH('dbo.UserPermissions', 'can_edit') IS NULL
                    ALTER TABLE dbo.UserPermissions ADD can_edit BIT NOT NULL CONSTRAINT DF_UserPermissions_can_edit_upgrade DEFAULT (0);", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public static DataTable GetUsersForPermissions()
        {
            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(
                @"SELECT user_id, username, role
                  FROM Users
                  WHERE username <> 'admin'
                  ORDER BY username;", connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                connection.Open();
                DataTable table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static void EnsureUserPermissionRows(int userId, string role)
        {
            if (userId <= 0)
            {
                return;
            }

            EnsurePermissionsTable();

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            {
                connection.Open();

                foreach (ScreenDefinition screen in ScreenDefinitions)
                {
                    EnsureSinglePermissionRow(
                        connection,
                        userId,
                        screen.Key,
                        GetDefaultCanView(role, screen.Key),
                        GetDefaultCanCreate(role, screen.Key),
                        GetDefaultCanEdit(role, screen.Key),
                        GetDefaultCanSave(role, screen.Key),
                        GetDefaultCanDelete(role, screen.Key),
                        GetDefaultCanViewNotifications(role, screen.Key),
                        false);
                }

                EnsureSinglePermissionRow(
                    connection,
                    userId,
                    GlobalScreenKey,
                    true,
                    false,
                    false,
                    false,
                    false,
                    GetDefaultCanViewNotifications(role, GlobalScreenKey),
                    GetDefaultCanDeleteNotifications(role, GlobalScreenKey));

                EnsureDefaultGlobalNotificationPermissions(connection, userId, role);
            }
        }

        public static DataTable GetPermissionTableForUser(int userId, string role)
        {
            string normalizedRole = NormalizeRole(role);
            EnsureUserPermissionRows(userId, role);

            DataTable table = new DataTable();
            table.Columns.Add("screen_key", typeof(string));
            table.Columns.Add("screen_name", typeof(string));
            table.Columns.Add("can_view", typeof(bool));
            table.Columns.Add("can_create", typeof(bool));
            table.Columns.Add("can_edit", typeof(bool));
            table.Columns.Add("can_save", typeof(bool));
            table.Columns.Add("can_delete", typeof(bool));

            Dictionary<string, DataRow> rowsByScreen = new Dictionary<string, DataRow>(StringComparer.OrdinalIgnoreCase);

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                SELECT screen_key, can_view, can_create, can_edit, can_save, can_delete, is_customized
                FROM dbo.UserPermissions
                WHERE user_id = @userId
                  AND screen_key <> @global
                ORDER BY screen_key;", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@global", SqlDbType.NVarChar, 100).Value = GlobalScreenKey;
                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DataRow row = table.NewRow();
                        row["screen_key"] = Convert.ToString(reader["screen_key"]);
                        row["screen_name"] = GetScreenTitle(Convert.ToString(reader["screen_key"]));
                        row["can_view"] = Convert.ToBoolean(reader["can_view"]);
                        row["can_create"] = Convert.ToBoolean(reader["can_create"]);
                        row["can_edit"] = Convert.ToBoolean(reader["can_edit"]);
                        row["can_save"] = Convert.ToBoolean(reader["can_save"]);
                        row["can_delete"] = Convert.ToBoolean(reader["can_delete"]);
                        table.Rows.Add(row);
                        rowsByScreen[Convert.ToString(row["screen_key"])] = row;
                    }
                }
            }

            foreach (ScreenDefinition screen in ScreenDefinitions)
            {
                if (rowsByScreen.ContainsKey(screen.Key))
                {
                    continue;
                }

                DataRow row = table.NewRow();
                row["screen_key"] = screen.Key;
                row["screen_name"] = screen.Title;
                row["can_view"] = GetDefaultCanView(role, screen.Key);
                row["can_create"] = GetDefaultCanCreate(role, screen.Key);
                row["can_edit"] = GetDefaultCanEdit(role, screen.Key);
                row["can_save"] = GetDefaultCanSave(role, screen.Key);
                row["can_delete"] = GetDefaultCanDelete(role, screen.Key);
                table.Rows.Add(row);
            }

            return table;
        }

        public static bool CanViewScreen(int userId, string role, string screenKey)
        {
            return GetPermissionValue(userId, role, screenKey, "can_view", GetDefaultCanView(role, screenKey));
        }

        public static bool CanCreate(int userId, string role, string screenKey)
        {
            return GetPermissionValue(userId, role, screenKey, "can_create", GetDefaultCanCreate(role, screenKey));
        }

        public static bool CanSave(int userId, string role, string screenKey)
        {
            return GetPermissionValue(userId, role, screenKey, "can_save", GetDefaultCanSave(role, screenKey));
        }

        public static bool CanEdit(int userId, string role, string screenKey)
        {
            return GetPermissionValue(userId, role, screenKey, "can_edit", GetDefaultCanEdit(role, screenKey));
        }

        public static bool CanDelete(int userId, string role, string screenKey)
        {
            return GetPermissionValue(userId, role, screenKey, "can_delete", GetDefaultCanDelete(role, screenKey));
        }

        public static bool CanViewNotifications(int userId, string role)
        {
            return GetPermissionValue(
                userId,
                role,
                GlobalScreenKey,
                "can_view_notifications",
                GetDefaultCanViewNotifications(role, GlobalScreenKey));
        }

        public static bool CanDeleteNotifications(int userId, string role)
        {
            if (userId <= 0 || !IsAdministratorIdentity(role))
            {
                return false;
            }

            EnsureUserPermissionRows(userId, role);

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(@"
                SELECT can_delete_notifications
                FROM dbo.UserPermissions
                WHERE user_id = @userId
                  AND screen_key = @screenKey;", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@screenKey", SqlDbType.NVarChar, 100).Value = GlobalScreenKey;
                connection.Open();
                object result = command.ExecuteScalar();
                return result == null || result == DBNull.Value
                    ? GetDefaultCanDeleteNotifications(role, GlobalScreenKey)
                    : Convert.ToBoolean(result);
            }
        }

        public static bool CanGrantNotificationDeletePermission()
        {
            return IsAdministratorIdentity(AppSession.Role);
        }

        public static bool CanManagePermissions(int userId, string role)
        {
            if (IsAdministratorIdentity(role))
            {
                return true;
            }

            return CanViewScreen(userId, role, ScreenOptions);
        }

        public static void SavePermissions(int userId, string role, DataTable permissions, bool canViewNotifications, bool? canDeleteNotifications)
        {
            EnsureUserPermissionRows(userId, role);

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            {
                connection.Open();

                foreach (DataRow row in permissions.Rows)
                {
                    using (SqlCommand command = new SqlCommand(@"
                        UPDATE dbo.UserPermissions
                        SET can_view = @canView,
                            can_create = @canCreate,
                            can_edit = @canEdit,
                            can_save = @canSave,
                            can_delete = @canDelete,
                            is_customized = 1,
                            updated_at = SYSUTCDATETIME(),
                            updated_by = @updatedBy
                        WHERE user_id = @userId
                          AND screen_key = @screenKey;", connection))
                    {
                        command.Parameters.Add("@canView", SqlDbType.Bit).Value = GetRowBool(row, "can_view");
                        command.Parameters.Add("@canCreate", SqlDbType.Bit).Value = GetRowBool(row, "can_create");
                        command.Parameters.Add("@canEdit", SqlDbType.Bit).Value = GetRowBool(row, "can_edit");
                        command.Parameters.Add("@canSave", SqlDbType.Bit).Value = GetRowBool(row, "can_save");
                        command.Parameters.Add("@canDelete", SqlDbType.Bit).Value = GetRowBool(row, "can_delete");
                        command.Parameters.Add("@updatedBy", SqlDbType.NVarChar, 100).Value =
                            string.IsNullOrWhiteSpace(AppSession.Username) ? (object)DBNull.Value : AppSession.Username;
                        command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                        command.Parameters.Add("@screenKey", SqlDbType.NVarChar, 100).Value = Convert.ToString(row["screen_key"]);
                        command.ExecuteNonQuery();
                    }
                }

                using (SqlCommand globalCommand = new SqlCommand(@"
                    UPDATE dbo.UserPermissions
                    SET can_view_notifications = @canViewNotifications,
                        can_delete_notifications = COALESCE(@canDeleteNotifications, can_delete_notifications),
                        is_customized = 1,
                        updated_at = SYSUTCDATETIME(),
                        updated_by = @updatedBy
                    WHERE user_id = @userId
                      AND screen_key = @screenKey;", connection))
                {
                    globalCommand.Parameters.Add("@canViewNotifications", SqlDbType.Bit).Value = canViewNotifications;
                    globalCommand.Parameters.Add("@canDeleteNotifications", SqlDbType.Bit).Value =
                        canDeleteNotifications.HasValue ? (object)canDeleteNotifications.Value : DBNull.Value;
                    globalCommand.Parameters.Add("@updatedBy", SqlDbType.NVarChar, 100).Value =
                        string.IsNullOrWhiteSpace(AppSession.Username) ? (object)DBNull.Value : AppSession.Username;
                    globalCommand.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                    globalCommand.Parameters.Add("@screenKey", SqlDbType.NVarChar, 100).Value = GlobalScreenKey;
                    globalCommand.ExecuteNonQuery();
                }
            }
        }

        public static bool EnsureScreenAccess(Form form, string screenKey)
        {
            if (CanViewScreen(AppSession.UserId, AppSession.Role, screenKey))
            {
                return true;
            }

            MessageBox.Show("You do not have permission to open this form.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            form.BeginInvoke(new Action(form.Close));
            return false;
        }

        public static void ApplyActionPermissions(Form form, string screenKey)
        {
            bool canCreate = CanCreate(AppSession.UserId, AppSession.Role, screenKey);
            bool canEdit = CanEdit(AppSession.UserId, AppSession.Role, screenKey);
            bool canSave = CanSave(AppSession.UserId, AppSession.Role, screenKey);
            bool canDelete = CanDelete(AppSession.UserId, AppSession.Role, screenKey);

            foreach (Control control in GetAllControls(form))
            {
                Button button = control as Button;
                if (button == null)
                {
                    continue;
                }

                PermissionAction action = DetectAction(button.Name, button.Text);
                switch (action)
                {
                    case PermissionAction.Create:
                        button.Enabled = canCreate;
                        button.Visible = canCreate || !IsActionOnlyButton(button);
                        break;
                    case PermissionAction.Edit:
                        button.Enabled = canEdit;
                        button.Visible = canEdit || !IsActionOnlyButton(button);
                        break;
                    case PermissionAction.Save:
                        button.Enabled = canSave;
                        button.Visible = canSave || !IsActionOnlyButton(button);
                        break;
                    case PermissionAction.Delete:
                        button.Enabled = canDelete;
                        button.Visible = canDelete || !IsActionOnlyButton(button);
                        break;
                }
            }

            foreach (DataGridView grid in GetAllControls(form).OfType<DataGridView>())
            {
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    DataGridViewButtonColumn buttonColumn = column as DataGridViewButtonColumn;
                    if (buttonColumn == null)
                    {
                        continue;
                    }

                    PermissionAction action = DetectAction(buttonColumn.Name, buttonColumn.HeaderText + " " + buttonColumn.Text);
                    switch (action)
                    {
                        case PermissionAction.Edit:
                            buttonColumn.Visible = canEdit;
                            break;
                        case PermissionAction.Save:
                            buttonColumn.Visible = canSave;
                            break;
                        case PermissionAction.Delete:
                            buttonColumn.Visible = canDelete;
                            break;
                    }
                }
            }
        }

        private static bool GetPermissionValue(int userId, string role, string screenKey, string columnName, bool defaultValue)
        {
            if (userId <= 0)
            {
                return false;
            }

            EnsureUserPermissionRows(userId, role);

            using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
            using (SqlCommand command = new SqlCommand(
                $"SELECT {columnName}, is_customized FROM dbo.UserPermissions WHERE user_id = @userId AND screen_key = @screenKey;",
                connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@screenKey", SqlDbType.NVarChar, 100).Value = screenKey;
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return false;
                    }

                    object result = reader[columnName];
                    return result != null && result != DBNull.Value && Convert.ToBoolean(result);
                }
            }
        }

        private static void EnsureSinglePermissionRow(
            SqlConnection connection,
            int userId,
            string screenKey,
            bool canView,
            bool canCreate,
            bool canEdit,
            bool canSave,
            bool canDelete,
            bool canViewNotifications,
            bool canDeleteNotifications)
        {
            using (SqlCommand command = new SqlCommand(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM dbo.UserPermissions
                    WHERE user_id = @userId
                      AND screen_key = @screenKey)
                BEGIN
                    INSERT INTO dbo.UserPermissions
                    (user_id, screen_key, can_view, can_create, can_edit, can_save, can_delete, can_view_notifications, can_delete_notifications, is_customized, updated_by)
                    VALUES
                    (@userId, @screenKey, @canView, @canCreate, @canEdit, @canSave, @canDelete, @canViewNotifications, @canDeleteNotifications, 0, @updatedBy);
                END;", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@screenKey", SqlDbType.NVarChar, 100).Value = screenKey;
                command.Parameters.Add("@canView", SqlDbType.Bit).Value = canView;
                command.Parameters.Add("@canCreate", SqlDbType.Bit).Value = canCreate;
                command.Parameters.Add("@canEdit", SqlDbType.Bit).Value = canEdit;
                command.Parameters.Add("@canSave", SqlDbType.Bit).Value = canSave;
                command.Parameters.Add("@canDelete", SqlDbType.Bit).Value = canDelete;
                command.Parameters.Add("@canViewNotifications", SqlDbType.Bit).Value = canViewNotifications;
                command.Parameters.Add("@canDeleteNotifications", SqlDbType.Bit).Value = canDeleteNotifications;
                command.Parameters.Add("@updatedBy", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(AppSession.Username) ? (object)DBNull.Value : AppSession.Username;
                command.ExecuteNonQuery();
            }
        }

        private static void EnsureDefaultGlobalNotificationPermissions(SqlConnection connection, int userId, string role)
        {
            if (!IsAdministratorIdentity(role))
            {
                return;
            }

            using (SqlCommand command = new SqlCommand(@"
                UPDATE dbo.UserPermissions
                SET can_view_notifications = 1,
                    can_delete_notifications = 1
                WHERE user_id = @userId
                  AND screen_key = @screenKey;", connection))
            {
                command.Parameters.Add("@userId", SqlDbType.Int).Value = userId;
                command.Parameters.Add("@screenKey", SqlDbType.NVarChar, 100).Value = GlobalScreenKey;
                command.ExecuteNonQuery();
            }
        }

        private static IEnumerable<Control> GetAllControls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;

                foreach (Control nestedChild in GetAllControls(child))
                {
                    yield return nestedChild;
                }
            }
        }

        private static string NormalizeRole(string role)
        {
            string normalized = (role ?? string.Empty).Trim().ToLowerInvariant();
            return normalized == "manger" ? "manager" : normalized;
        }

        private static bool IsAdministratorIdentity(string role)
        {
            return NormalizeRole(role) == "admin";
        }

        private static string GetScreenTitle(string screenKey)
        {
            ScreenDefinition definition = ScreenDefinitions.FirstOrDefault(
                screen => string.Equals(screen.Key, screenKey, StringComparison.OrdinalIgnoreCase));
            return definition == null ? screenKey : definition.Title;
        }

        private static bool GetDefaultCanView(string role, string screenKey)
        {
            return IsAdministratorIdentity(role);
        }

        private static bool GetDefaultCanCreate(string role, string screenKey)
        {
            return IsAdministratorIdentity(role);
        }

        private static bool GetDefaultCanEdit(string role, string screenKey)
        {
            return IsAdministratorIdentity(role);
        }

        private static bool GetDefaultCanSave(string role, string screenKey)
        {
            return IsAdministratorIdentity(role);
        }

        private static bool GetDefaultCanDelete(string role, string screenKey)
        {
            return IsAdministratorIdentity(role);
        }

        private static bool GetDefaultCanViewNotifications(string role, string screenKey)
        {
            return IsAdministratorIdentity(role);
        }

        private static bool GetDefaultCanDeleteNotifications(string role, string screenKey)
        {
            return IsAdministratorIdentity(role);
        }

        private static bool GetRowBool(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) &&
                   row[columnName] != DBNull.Value &&
                   Convert.ToBoolean(row[columnName]);
        }

        private static bool IsActionOnlyButton(Button button)
        {
            PermissionAction action = DetectAction(button.Name, button.Text);
            return action != PermissionAction.None;
        }

        private static PermissionAction DetectAction(string name, string text)
        {
            string value = ((name ?? string.Empty) + " " + (text ?? string.Empty)).ToLowerInvariant();

            if (value.Contains("delete") || value.Contains("remove"))
            {
                return PermissionAction.Delete;
            }

            if (value.Contains("edit") || value.Contains("update"))
            {
                return PermissionAction.Edit;
            }

            if (value.Contains("save") || value.Contains("plus") || value.Contains("minus"))
            {
                return PermissionAction.Save;
            }

            if (value.Contains("add") || value.Contains("new") || value.Contains("create"))
            {
                return PermissionAction.Create;
            }

            return PermissionAction.None;
        }

        private enum PermissionAction
        {
            None,
            Create,
            Edit,
            Save,
            Delete
        }
    }
}
