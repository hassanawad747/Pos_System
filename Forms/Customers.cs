using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Pos_System.Forms
{
    public partial class Customers : Form
    {
        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";

        public Customers()
        {
            InitializeComponent();
            POS_System.Program.SettingsManager.RegisterForm(this);
        }

        private void Customers_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'pos_systemDataSet15.Customers' table. You can move, or remove it, as needed.
            this.customersTableAdapter1.Fill(this.pos_systemDataSet15.Customers);
            LoadCustomers("");

            //// إضافة زر Edit
            //DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
            //btnEdit.HeaderText = "Edit";
            //btnEdit.Text = "Edit";
            //btnEdit.UseColumnTextForButtonValue = true;
            //dataGridView1.Columns.Add(btnEdit);

            //// إضافة زر Delete
            //DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
            //btnDelete.HeaderText = "Delete";
            //btnDelete.Text = "Delete";
            //btnDelete.UseColumnTextForButtonValue = true;
            //dataGridView1.Columns.Add(btnDelete);
        }

        private void btnAddCustomer_Click(object sender, EventArgs e)
        {
            AddCustomers addForm = new AddCustomers();
            addForm.ShowDialog();
            LoadCustomers(""); // إعادة تحميل العملاء بعد الإضافة
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void LoadCustomers(string keyword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    SELECT customer_id, name, phone, email, loyalty_points, created_at,balance,created_by
                    FROM Customers
                    WHERE name LIKE @keyword OR phone LIKE @keyword OR email LIKE @keyword";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }

        private void datagridCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            AddCustomers addForm = new AddCustomers();
            addForm.ShowDialog();
            this.Hide();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                // Edit
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Edit")
                {
                    int customerId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["customer_id"].Value);
                    string newName = dataGridView1.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    string newPhone = dataGridView1.Rows[e.RowIndex].Cells["phone"].Value.ToString();
                    string newEmail = dataGridView1.Rows[e.RowIndex].Cells["email"].Value.ToString();
                    string createdby = dataGridView1.Rows[e.RowIndex].Cells["created_by"].Value.ToString();
                    string balance = dataGridView1.Rows[e.RowIndex].Cells["balance"].Value.ToString();


                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE Customers SET name=@name, phone=@phone, email=@email, created_by=@created_by, balance=@balance WHERE customer_id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", customerId);
                        cmd.Parameters.AddWithValue("@name", newName);
                        cmd.Parameters.AddWithValue("@phone", newPhone);
                        cmd.Parameters.AddWithValue("@email", newEmail);
                        cmd.Parameters.AddWithValue("@created_by", createdby);
                        cmd.Parameters.AddWithValue("@balance", balance);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("✅ تم تعديل العميل بنجاح");
                    LoadCustomers(txtsearch.Text);
                }

                // Delete
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
                {
                    int customerId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["customer_id"].Value);

                    var confirm = MessageBox.Show("هل تريد حذف العميل؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                    if (confirm == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM Customers WHERE customer_id=@id", conn);
                            cmd.Parameters.AddWithValue("@id", customerId);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("✅ تم حذف العميل بنجاح");
                        LoadCustomers(txtsearch.Text);
                    }
                }
            }
        }

        private void txtsearch_TextChanged_1(object sender, EventArgs e)
        {
            LoadCustomers(txtsearch.Text); // البحث التلقائي عند الكتابة
        }
    }
}
