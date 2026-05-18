using Pos_System.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public class ActivityLogForm : Form
    {
        private readonly DataGridView grid = new DataGridView();

        public ActivityLogForm()
        {
            Text = "Activity Log";
            BackColor = Color.White;
            Width = 900;
            Height = 520;

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            Controls.Add(grid);

            Load += ActivityLogForm_Load;
        }

        private void ActivityLogForm_Load(object sender, EventArgs e)
        {
            LoadAuditRows();
        }

        private void LoadAuditRows()
        {
            try
            {
                AuditLogger.EnsureAuditTable();

                using (SqlConnection connection = new SqlConnection(POS_System.Program.SettingsManager.ConnectionString))
                using (SqlCommand command = new SqlCommand(@"
                    SELECT TOP 200
                        created_at AS [Time],
                        username AS [User],
                        action_type AS [Action],
                        entity_name AS [Entity],
                        entity_id AS [Record],
                        details AS [Details]
                    FROM dbo.AuditLogs
                    ORDER BY audit_log_id DESC;", connection))
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    grid.DataSource = table;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load activity log: " + ex.Message, "Activity Log");
            }
        }
    }
}
