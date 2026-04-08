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

namespace Pos_System.Forms
{
    public partial class SupplierForm : Form
    {
        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";
        public SupplierForm()
        {
            InitializeComponent();
        }

        

        private void btnadd_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Suppliers (name,contact_info,address) VALUES (@name,@contact_info,@address)", conn);
                cmd.Parameters.AddWithValue("@name", txtname.Text);
                cmd.Parameters.AddWithValue("@contact_info", txtnumber.Text);
                cmd.Parameters.AddWithValue("@address", txtplace.Text);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("✅ تم إضافة المورد بنجاح");

            // افتح فورم إضافة المنتجات مباشرة
            AddProducts productForm = new AddProducts();
            productForm.ShowDialog();
            this.Close();

        }

        private void btndelete_Click(object sender, EventArgs e)
        {
            CategoryForm categoryForm = new CategoryForm();
            categoryForm.ShowDialog();
            this.Close();
        }


        private void LoadSuppliers(string keyword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    SELECT supplier_id, name, contact_info, address
                    FROM Suppliers
                    WHERE name LIKE @keyword OR contact_info LIKE @keyword";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt;
            }
        }


        private void SupplierForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'pos_systemDataSet10.Suppliers' table. You can move, or remove it, as needed.
            this.suppliersTableAdapter.Fill(this.pos_systemDataSet10.Suppliers);
            // TODO: This line of code loads data into the 'pos_systemDataSet7.Categories' table. You can move, or remove it, as needed.
            // this.categoriesTableAdapter.Fill(this.pos_systemDataSet7.Categories);

            LoadSuppliers(""); // عند التحميل يعرض كل الموردين


            DataGridViewButtonColumn btnEdit = new DataGridViewButtonColumn();
            btnEdit.HeaderText = "Edit";
            btnEdit.Text = "Edit";
            btnEdit.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(btnEdit);

            // إضافة زر Delete
            DataGridViewButtonColumn btnDelete = new DataGridViewButtonColumn();
            btnDelete.HeaderText = "Delete";
            btnDelete.Text = "Delete";
            btnDelete.UseColumnTextForButtonValue = true;
            dataGridView1.Columns.Add(btnDelete);

        }

        private void txtname_TextChanged(object sender, EventArgs e)
        {
            LoadSuppliers(txtname.Text);
        }

        private void txtnumber_TextChanged(object sender, EventArgs e)
        {
            LoadSuppliers(txtnumber.Text);
        }

        private void btnskip_Click(object sender, EventArgs e)
        {
            AddProducts addProducts = new AddProducts();
            addProducts.ShowDialog();
            this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // زر Edit
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Edit")
                {
                    int supplierId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["supplier_id"].Value);

                    string newName = dataGridView1.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    string newContact = dataGridView1.Rows[e.RowIndex].Cells["contact_info"].Value.ToString();
                    string newAddress = dataGridView1.Rows[e.RowIndex].Cells["address"].Value.ToString();

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE Suppliers SET name=@name, contact_info=@contact, address=@address WHERE supplier_id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", supplierId);
                        cmd.Parameters.AddWithValue("@name", newName);
                        cmd.Parameters.AddWithValue("@contact", newContact);
                        cmd.Parameters.AddWithValue("@address", newAddress);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("✅ تم تعديل المورد بنجاح");
                    LoadSuppliers("");
                }

                // زر Delete
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
                {
                    int supplierId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["supplier_id"].Value);

                    var confirm = MessageBox.Show("هل تريد حذف المورد؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                    if (confirm == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM Suppliers WHERE supplier_id=@id", conn);
                            cmd.Parameters.AddWithValue("@id", supplierId);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("✅ تم حذف المورد بنجاح");
                        LoadSuppliers("");
                    }
                }
            }
        }



    }
}
