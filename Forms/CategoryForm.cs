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
    public partial class CategoryForm : Form
    {

        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";
        public CategoryForm()
        {
            InitializeComponent();
        }

        private void btnadd_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO Categories (category_name,description) VALUES (@name,@description)", conn);
                cmd.Parameters.AddWithValue("@name", txttypeitem.Text);
                cmd.Parameters.AddWithValue("@description", txtdescreption.Text);
                cmd.ExecuteNonQuery();
            }

            MessageBox.Show("✅ تم إضافة الصنف بنجاح");

            // افتح فورم الموردين مباشرة
            SupplierForm supplierForm = new SupplierForm();
            supplierForm.ShowDialog();
            this.Close();

        }


        private void LoadCategories(string keyword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
            SELECT category_id, category_name, description
            FROM Categories
            WHERE category_name LIKE @keyword OR description LIKE @keyword";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dataGridView1.DataSource = dt; // تأكد أن اسم الـ DataGridView عندك هو datagridCategories
            }
        }



        private void btndelete_Click(object sender, EventArgs e)
        {
            Products productsForm = new Products();
            productsForm.ShowDialog();
            this.Close();
        }

        private void CategoryForm_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'pos_systemDataSet9.Suppliers' table. You can move, or remove it, as needed.
            //this.suppliersTableAdapter.Fill(this.pos_systemDataSet9.Suppliers);
            LoadCategories("");

            // TODO: This line of code loads data into the 'pos_systemDataSet8.Categories' table. You can move, or remove it, as needed.
            this.categoriesTableAdapter.Fill(this.pos_systemDataSet8.Categories);
           // LoadCategories("");

            // إضافة زر Edit
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

        private void txttypeitem_TextChanged(object sender, EventArgs e)
        {
            LoadCategories(txttypeitem.Text);
        }

        private void btnskip_Click(object sender, EventArgs e)
        {
            SupplierForm supplierForm = new SupplierForm();
            supplierForm.ShowDialog();
            this.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // زر Edit
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Edit")
                {
                    int categoryId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["category_id"].Value);

                    string newName = dataGridView1.Rows[e.RowIndex].Cells["category_name"].Value.ToString();
                    string newDescription = dataGridView1.Rows[e.RowIndex].Cells["description"].Value.ToString();

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE Categories SET category_name=@name, description=@desc WHERE category_id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", categoryId);
                        cmd.Parameters.AddWithValue("@name", newName);
                        cmd.Parameters.AddWithValue("@desc", newDescription);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("✅ تم تعديل الصنف بنجاح");
                    LoadCategories("");
                }

                // زر Delete
                if (dataGridView1.Columns[e.ColumnIndex].HeaderText == "Delete")
                {
                    int categoryId = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells["category_id"].Value);

                    var confirm = MessageBox.Show("هل تريد حذف الصنف؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                    if (confirm == DialogResult.Yes)
                    {
                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand("DELETE FROM Categories WHERE category_id=@id", conn);
                            cmd.Parameters.AddWithValue("@id", categoryId);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("✅ تم حذف الصنف بنجاح");
                        LoadCategories("");
                    }
                }
            }
        }


    }
}
