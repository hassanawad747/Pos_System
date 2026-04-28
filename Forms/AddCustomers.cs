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

namespace Pos_System.Forms
{
    public partial class AddCustomers : Form
    {
        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";
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
            LoginForm loginForm = new LoginForm();
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Customers (name, phone, email,balance,created_by) 
                        VALUES (@name, @phone, @email,@balance,@created_by)", conn);

                    cmd.Parameters.AddWithValue("@name", txtname.Text);
                    cmd.Parameters.AddWithValue("@phone", txtnumber.Text);
                    cmd.Parameters.AddWithValue("@email", txtemail.Text);
                    cmd.Parameters.AddWithValue("@balance",txtprice.Text);
                    cmd.Parameters.AddWithValue("@created_by", LoginForm.LoggedInUsername); // يمكنك تعديل هذا حسب المستخدم الحالي
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("✅ تم إضافة العميل بنجاح بواسطة " + LoginForm.LoggedInUsername);
                this.Close(); // إغلاق الفورم بعد الإضافة
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ: " + ex.Message);
            }
            clearinput();
            Customers customers = new Customers();
            customers.ShowDialog();
            this.Close();
        }

        private void btncancle_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}
