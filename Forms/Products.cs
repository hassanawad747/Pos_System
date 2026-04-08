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
    public partial class Products : Form
    {
        string connStr = "Server=HASSAN-AWWAD;Database=pos_system;Trusted_Connection=True;";
        public Products()
        {
            InitializeComponent();
        }

        private void Products_Load(object sender, EventArgs e)
        {
            this.suppliersTableAdapter.Fill(this.pos_systemDataSet13.Suppliers);
            this.categoriesTableAdapter.Fill(this.pos_systemDataSet12.Categories);
            this.productsTableAdapter1.Fill(this.pos_systemDataSet5.Products);

            // أزرار للمنتجات
            //DataGridViewButtonColumn btnEditProd = new DataGridViewButtonColumn();
            //btnEditProd.HeaderText = "Edit";
            //btnEditProd.Text = "Edit";
            //btnEditProd.UseColumnTextForButtonValue = true;
            //datagridProducts.Columns.Add(btnEditProd);

            //DataGridViewButtonColumn btnDeleteProd = new DataGridViewButtonColumn();
            //btnDeleteProd.HeaderText = "Delete";
            //btnDeleteProd.Text = "Delete";
            //btnDeleteProd.UseColumnTextForButtonValue = true;
            //datagridProducts.Columns.Add(btnDeleteProd);

            //// أزرار للأصناف
            //DataGridViewButtonColumn btnEditCat = new DataGridViewButtonColumn();
            //btnEditCat.HeaderText = "Edit";
            //btnEditCat.Text = "Edit";
            //btnEditCat.UseColumnTextForButtonValue = true;
            //datagridCategories.Columns.Add(btnEditCat);

            //DataGridViewButtonColumn btnDeleteCat = new DataGridViewButtonColumn();
            //btnDeleteCat.HeaderText = "Delete";
            //btnDeleteCat.Text = "Delete";
            //btnDeleteCat.UseColumnTextForButtonValue = true;
            //datagridCategories.Columns.Add(btnDeleteCat);

            //// أزرار للموردين
            //DataGridViewButtonColumn btnEditSup = new DataGridViewButtonColumn();
            //btnEditSup.HeaderText = "Edit";
            //btnEditSup.Text = "Edit";
            //btnEditSup.UseColumnTextForButtonValue = true;
            //datagridSuppliers.Columns.Add(btnEditSup);

            //DataGridViewButtonColumn btnDeleteSup = new DataGridViewButtonColumn();
            //btnDeleteSup.HeaderText = "Delete";
            //btnDeleteSup.Text = "Delete";
            //btnDeleteSup.UseColumnTextForButtonValue = true;
            //datagridSuppliers.Columns.Add(btnDeleteSup);


        }


        private void datagridCategories_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }



        private void datagridSuppliers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }




        private void datagridProducts_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            
        }


        private void button1_Click(object sender, EventArgs e)
        {
           CategoryForm categoryForm = new CategoryForm();
            categoryForm.ShowDialog();
            this.Close();
        }

        private void txtsearch_TextChanged(object sender, EventArgs e)
        {
            SearchProducts(txtsearch.Text);

        }


        private void SearchProducts(string keyword)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
            SELECT product_id, name, category_id, price_usd, price_lb, exchange_rate, 
                   sale_price_usd, sale_price_lb, stock_quantity, barcode, supplier_id, created_at
            FROM Products
            WHERE name LIKE @keyword OR barcode LIKE @keyword";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                // إذا النص ممكن يكون رقم (يوم أو شهر أو سنة)
                int number;
                if (int.TryParse(keyword, out number))
                {
                    // نضيف شرط إضافي للبحث بالتاريخ
                    query += @" OR DAY(created_at) = @num OR MONTH(created_at) = @num OR YEAR(created_at) = @num";
                    cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                    cmd.Parameters.AddWithValue("@num", number);
                }
                else
                {
                    // إذا النص تاريخ كامل (مثلاً 07/04/2026)
                    DateTime searchDate;
                    if (DateTime.TryParse(keyword, out searchDate))
                    {
                        query += @" OR CAST(created_at AS DATE) = @searchDate";
                        cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                        cmd.Parameters.AddWithValue("@searchDate", searchDate.Date);
                    }
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                datagridProducts.DataSource = dt;
            }
        }

        private void datagridProducts_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    if (e.RowIndex >= 0)
                    {
                        int productId = Convert.ToInt32(datagridProducts.Rows[e.RowIndex].Cells["product_id"].Value);

                        string newName = datagridProducts.Rows[e.RowIndex].Cells["name"].Value?.ToString() ?? "";

                        object priceUsdObj = datagridProducts.Rows[e.RowIndex].Cells["price_usd"].Value;
                        decimal newPriceUsd = priceUsdObj == DBNull.Value ? 0 : Convert.ToDecimal(priceUsdObj);

                        object priceLbObj = datagridProducts.Rows[e.RowIndex].Cells["price_lb"].Value;
                        decimal newPriceLb = priceLbObj == DBNull.Value ? 0 : Convert.ToDecimal(priceLbObj);

                        object exchangeRateObj = datagridProducts.Rows[e.RowIndex].Cells["exchange_rate"].Value;
                        decimal newExchangeRate = exchangeRateObj == DBNull.Value ? 0 : Convert.ToDecimal(exchangeRateObj);

                        object saleUsdObj = datagridProducts.Rows[e.RowIndex].Cells["sale_price_usd"].Value;
                        decimal newSaleUsd = saleUsdObj == DBNull.Value ? 0 : Convert.ToDecimal(saleUsdObj);

                        object saleLbObj = datagridProducts.Rows[e.RowIndex].Cells["sale_price_lb"].Value;
                        decimal newSaleLb = saleLbObj == DBNull.Value ? 0 : Convert.ToDecimal(saleLbObj);

                        object stockObj = datagridProducts.Rows[e.RowIndex].Cells["stock_quantity"].Value;
                        int newStock = stockObj == DBNull.Value ? 0 : Convert.ToInt32(stockObj);

                        string newBarcode = datagridProducts.Rows[e.RowIndex].Cells["barcode"].Value?.ToString() ?? "";

                        object catObj = datagridProducts.Rows[e.RowIndex].Cells["category_id"].Value;
                        int newCategoryId = catObj == DBNull.Value ? 0 : Convert.ToInt32(catObj);

                        object supObj = datagridProducts.Rows[e.RowIndex].Cells["supplier_id"].Value;
                        int newSupplierId = supObj == DBNull.Value ? 0 : Convert.ToInt32(supObj);

                        using (SqlConnection conn = new SqlConnection(connStr))
                        {
                            conn.Open();
                            SqlCommand cmd = new SqlCommand(@"
UPDATE Products 
SET name=@name,
    price_usd=@price_usd,
    price_lb=@price_lb,
    exchange_rate=@exchange_rate,
    sale_price_usd=@sale_usd,
    sale_price_lb=@sale_lb,
    stock_quantity=@stock,
    barcode=@barcode,
    category_id=@cat,
    supplier_id=@sup
WHERE product_id=@id", conn);

                            cmd.Parameters.AddWithValue("@id", productId);
                            cmd.Parameters.AddWithValue("@name", newName);
                            cmd.Parameters.AddWithValue("@price_usd", newPriceUsd);
                            cmd.Parameters.AddWithValue("@price_lb", newPriceLb);
                            cmd.Parameters.AddWithValue("@exchange_rate", newExchangeRate);
                            cmd.Parameters.AddWithValue("@sale_usd", newSaleUsd);
                            cmd.Parameters.AddWithValue("@sale_lb", newSaleLb);
                            cmd.Parameters.AddWithValue("@stock", newStock);
                            cmd.Parameters.AddWithValue("@barcode", newBarcode);
                            cmd.Parameters.AddWithValue("@cat", newCategoryId);
                            cmd.Parameters.AddWithValue("@sup", newSupplierId);

                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("✅ تم تعديل المنتج بنجاح");
                    }


                    //delete

                    if (e.RowIndex >= 0 && datagridProducts.Columns[e.ColumnIndex].Name == "Delete")
                        {
                            int productId = Convert.ToInt32(datagridProducts.Rows[e.RowIndex].Cells["product_id"].Value);

                            var confirm = MessageBox.Show("هل تريد حذف المنتج؟", "تأكيد الحذف", MessageBoxButtons.YesNo);
                            if (confirm == DialogResult.Yes)
                            {
                                using (SqlConnection conn = new SqlConnection(connStr))
                                {
                                    conn.Open();
                                    SqlCommand cmd = new SqlCommand("DELETE FROM Products WHERE product_id=@id", conn);
                                    cmd.Parameters.AddWithValue("@id", productId);
                                    cmd.ExecuteNonQuery();
                                }

                                MessageBox.Show("✅ تم حذف المنتج بنجاح");
                                datagridProducts.Rows.RemoveAt(e.RowIndex); // حذف الصف من الجدول مباشرة
                            }
                        }

                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء العملية: " + ex.Message);
            }
        }

        private void datagridCategories_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

            if (e.RowIndex >= 0)
            {
                if (datagridCategories.Columns[e.ColumnIndex].HeaderText == "Edit1")
                {
                    int categoryId = Convert.ToInt32(datagridCategories.Rows[e.RowIndex].Cells["category_id1"].Value);
                    string newName = datagridCategories.Rows[e.RowIndex].Cells["category_name"].Value.ToString();
                    string newDesc = datagridCategories.Rows[e.RowIndex].Cells["description"].Value.ToString();

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE Categories SET category_name=@name, description=@desc WHERE category_id=@id", conn);
                        cmd.Parameters.AddWithValue("@id", categoryId);
                        cmd.Parameters.AddWithValue("@name", newName);
                        cmd.Parameters.AddWithValue("@desc", newDesc);
                        cmd.ExecuteNonQuery();
                    }
                    MessageBox.Show("✅ تم تعديل الصنف بنجاح");
                    this.categoriesTableAdapter.Fill(this.pos_systemDataSet12.Categories);
                }

                if (datagridCategories.Columns[e.ColumnIndex].HeaderText == "Delete1")
                {
                    int categoryId = Convert.ToInt32(datagridCategories.Rows[e.RowIndex].Cells["category_id1"].Value);
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
                        this.categoriesTableAdapter.Fill(this.pos_systemDataSet12.Categories);
                    }
                }
            }
        }

        private void datagridSuppliers_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (datagridSuppliers.Columns[e.ColumnIndex].HeaderText == "Edit2")
                {
                    int supplierId = Convert.ToInt32(datagridSuppliers.Rows[e.RowIndex].Cells["supplier_id1"].Value);
                    string newName = datagridSuppliers.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    string newContact = datagridSuppliers.Rows[e.RowIndex].Cells["contact_info"].Value.ToString();
                    string newAddress = datagridSuppliers.Rows[e.RowIndex].Cells["address"].Value.ToString();

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
                    this.suppliersTableAdapter.Fill(this.pos_systemDataSet13.Suppliers);
                }

                if (datagridSuppliers.Columns[e.ColumnIndex].HeaderText == "Delete2")
                {
                    int supplierId = Convert.ToInt32(datagridSuppliers.Rows[e.RowIndex].Cells["supplier_id1"].Value);
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
                        this.suppliersTableAdapter.Fill(this.pos_systemDataSet13.Suppliers);
                    }
                }
            }
        }
    }
}
