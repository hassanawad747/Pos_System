using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Globalization;
using Pos_System.Services;

namespace Pos_System.Forms
{
    public partial class AddCustomers : Form
    {
        private readonly string connStr = POS_System.Program.SettingsManager.ConnectionString;
        public AddCustomers()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
        }

        private void clearinput()
        {
            txtname.Clear();
            txtnumber.Clear();
            txtemail.Clear();
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            if (!PermissionService.CanCreate(AppSession.UserId, AppSession.Role, PermissionService.ScreenCustomers))
            {
                MessageBox.Show("You do not have permission to create customers.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    AuditLogger.EnsureCustomerBalanceColumns();

                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Customers (name, phone, email, balance, balance_usd, balance_lb, balance_updated_at, created_by) 
                        VALUES (@name, @phone, @email, @balance, @balanceUsd, 0, CASE WHEN @balanceUsd = 0 THEN NULL ELSE GETDATE() END, @created_by)", conn);

                    decimal openingBalance = 0m;
                    if (!string.IsNullOrWhiteSpace(txtprice.Text) &&
                        !decimal.TryParse(txtprice.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out openingBalance) &&
                        !decimal.TryParse(txtprice.Text.Trim(), out openingBalance))
                    {
                        MessageBox.Show("Balance must be a valid number.");
                        txtprice.Focus();
                        return;
                    }

                    cmd.Parameters.AddWithValue("@name", txtname.Text);
                    cmd.Parameters.AddWithValue("@phone", txtnumber.Text);
                    cmd.Parameters.AddWithValue("@email", txtemail.Text);
                    cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = openingBalance;
                    cmd.Parameters.Add("@balanceUsd", SqlDbType.Decimal).Value = openingBalance;
                    cmd.Parameters.AddWithValue("@created_by", AppSession.Username);
                    cmd.ExecuteNonQuery();
                }

                AuditService.Log("Customers", "Create", txtnumber.Text.Trim(), "Created customer " + txtname.Text.Trim());
                MessageBox.Show("✅ تم إضافة العميل بنجاح بواسطة " + AppSession.Username);
                DialogResult = DialogResult.OK;
                //Close();
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
            clearinput();
        }

        private void btncancle_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}
