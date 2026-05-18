namespace Pos_System.Forms
{
    partial class AddProducts
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelheader = new System.Windows.Forms.Panel();
            this.labeldate = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnexit = new System.Windows.Forms.Button();
            this.btnadd = new System.Windows.Forms.Button();
            this.suppliersBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.pos_systemDataSet6 = new Pos_System.pos_systemDataSet6();
            this.suppliersTableAdapter = new Pos_System.pos_systemDataSet6TableAdapters.SuppliersTableAdapter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboSupplier = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.combobarcode = new System.Windows.Forms.ComboBox();
            this.comboitem = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtsalelebanon = new System.Windows.Forms.TextBox();
            this.txtsaledollar = new System.Windows.Forms.TextBox();
            this.txtdollar = new System.Windows.Forms.TextBox();
            this.txtquentity = new System.Windows.Forms.TextBox();
            this.txtpriceLebanon = new System.Windows.Forms.TextBox();
            this.txtpricedollar = new System.Windows.Forms.TextBox();
            this.txtbarcode = new System.Windows.Forms.TextBox();
            this.txtname = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.pos_systemDataSet11 = new Pos_System.pos_systemDataSet11();
            this.productsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.productsTableAdapter = new Pos_System.pos_systemDataSet11TableAdapters.ProductsTableAdapter();
            this.product_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.category_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stock_quantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barcode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.supplier_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.created_at = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.price_usd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.price_lb = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.exchange_rate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sale_price_usd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sale_price_lb = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelheader.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.suppliersBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet6)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.productsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panelheader
            // 
            this.panelheader.BackColor = System.Drawing.Color.Black;
            this.panelheader.Controls.Add(this.labeldate);
            this.panelheader.Controls.Add(this.label3);
            this.panelheader.Controls.Add(this.label2);
            this.panelheader.Controls.Add(this.label4);
            this.panelheader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelheader.ForeColor = System.Drawing.Color.White;
            this.panelheader.Location = new System.Drawing.Point(0, 0);
            this.panelheader.Name = "panelheader";
            this.panelheader.Size = new System.Drawing.Size(1491, 82);
            this.panelheader.TabIndex = 2;
            // 
            // labeldate
            // 
            this.labeldate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labeldate.AutoSize = true;
            this.labeldate.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labeldate.Location = new System.Drawing.Point(1352, 20);
            this.labeldate.Name = "labeldate";
            this.labeldate.Size = new System.Drawing.Size(0, 23);
            this.labeldate.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(184, 23);
            this.label3.TabIndex = 2;
            this.label3.Text = "Add New Products";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(972, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 38);
            this.label2.TabIndex = 1;
            this.label2.Text = "Zone";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(884, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 38);
            this.label4.TabIndex = 0;
            this.label4.Text = "Bike";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnexit);
            this.panel1.Controls.Add(this.btnadd);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 597);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1491, 69);
            this.panel1.TabIndex = 13;
            // 
            // btnexit
            // 
            this.btnexit.BackColor = System.Drawing.Color.Transparent;
            this.btnexit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnexit.Location = new System.Drawing.Point(1318, 6);
            this.btnexit.Name = "btnexit";
            this.btnexit.Size = new System.Drawing.Size(65, 49);
            this.btnexit.TabIndex = 1;
            this.btnexit.Text = "الغاء";
            this.btnexit.UseVisualStyleBackColor = false;
            this.btnexit.Click += new System.EventHandler(this.btnexit_Click);
            // 
            // btnadd
            // 
            this.btnadd.BackColor = System.Drawing.Color.Green;
            this.btnadd.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnadd.ForeColor = System.Drawing.Color.White;
            this.btnadd.Location = new System.Drawing.Point(1389, 6);
            this.btnadd.Name = "btnadd";
            this.btnadd.Size = new System.Drawing.Size(99, 49);
            this.btnadd.TabIndex = 0;
            this.btnadd.Text = "اضافة المنتج";
            this.btnadd.UseVisualStyleBackColor = false;
            this.btnadd.Click += new System.EventHandler(this.btnadd_Click);
            // 
            // suppliersBindingSource
            // 
            this.suppliersBindingSource.DataMember = "Suppliers";
            this.suppliersBindingSource.DataSource = this.pos_systemDataSet6;
            // 
            // pos_systemDataSet6
            // 
            this.pos_systemDataSet6.DataSetName = "pos_systemDataSet6";
            this.pos_systemDataSet6.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // suppliersTableAdapter
            // 
            this.suppliersTableAdapter.ClearBeforeFill = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.comboSupplier);
            this.panel2.Controls.Add(this.label14);
            this.panel2.Controls.Add(this.combobarcode);
            this.panel2.Controls.Add(this.comboitem);
            this.panel2.Controls.Add(this.label13);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.label10);
            this.panel2.Controls.Add(this.label9);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.txtsalelebanon);
            this.panel2.Controls.Add(this.txtsaledollar);
            this.panel2.Controls.Add(this.txtdollar);
            this.panel2.Controls.Add(this.txtquentity);
            this.panel2.Controls.Add(this.txtpriceLebanon);
            this.panel2.Controls.Add(this.txtpricedollar);
            this.panel2.Controls.Add(this.txtbarcode);
            this.panel2.Controls.Add(this.txtname);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(831, 82);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(660, 515);
            this.panel2.TabIndex = 29;
            // 
            // comboSupplier
            // 
            this.comboSupplier.DataSource = this.suppliersBindingSource;
            this.comboSupplier.DisplayMember = "name";
            this.comboSupplier.FormattingEnabled = true;
            this.comboSupplier.Location = new System.Drawing.Point(359, 439);
            this.comboSupplier.Name = "comboSupplier";
            this.comboSupplier.Size = new System.Drawing.Size(291, 24);
            this.comboSupplier.TabIndex = 50;
            this.comboSupplier.ValueMember = "name";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(571, 406);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(72, 23);
            this.label14.TabIndex = 49;
            this.label14.Text = "اسم الشركة";
            // 
            // combobarcode
            // 
            this.combobarcode.FormattingEnabled = true;
            this.combobarcode.Items.AddRange(new object[] {
            "نعم",
            "لا"});
            this.combobarcode.Location = new System.Drawing.Point(359, 277);
            this.combobarcode.Name = "combobarcode";
            this.combobarcode.Size = new System.Drawing.Size(287, 24);
            this.combobarcode.TabIndex = 48;
            // 
            // comboitem
            // 
            this.comboitem.FormattingEnabled = true;
            this.comboitem.Items.AddRange(new object[] {
            "1"});
            this.comboitem.Location = new System.Drawing.Point(359, 133);
            this.comboitem.Name = "comboitem";
            this.comboitem.Size = new System.Drawing.Size(288, 24);
            this.comboitem.TabIndex = 47;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(14, 406);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(97, 23);
            this.label13.TabIndex = 46;
            this.label13.Text = "سعر البيع(ل.ل)";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(538, 175);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(109, 23);
            this.label12.TabIndex = 45;
            this.label12.Text = "سعر الشراء(ل.ل)";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(14, 331);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(88, 23);
            this.label11.TabIndex = 44;
            this.label11.Text = "صرف الدولار";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(571, 251);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(77, 23);
            this.label10.TabIndex = 43;
            this.label10.Text = "Barcode required";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(14, 251);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(42, 23);
            this.label9.TabIndex = 42;
            this.label9.Text = "الكمية";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(14, 175);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(109, 23);
            this.label8.TabIndex = 41;
            this.label8.Text = "سعر الشراء($)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(546, 331);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(97, 23);
            this.label7.TabIndex = 40;
            this.label7.Text = "سعر البيع($)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(594, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(49, 23);
            this.label6.TabIndex = 39;
            this.label6.Text = "الصنف";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(14, 100);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 23);
            this.label5.TabIndex = 38;
            this.label5.Text = "الباركود";
            // 
            // txtsalelebanon
            // 
            this.txtsalelebanon.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtsalelebanon.Location = new System.Drawing.Point(9, 432);
            this.txtsalelebanon.Multiline = true;
            this.txtsalelebanon.Name = "txtsalelebanon";
            this.txtsalelebanon.Size = new System.Drawing.Size(289, 37);
            this.txtsalelebanon.TabIndex = 37;
            // 
            // txtsaledollar
            // 
            this.txtsaledollar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtsaledollar.Location = new System.Drawing.Point(359, 357);
            this.txtsaledollar.Multiline = true;
            this.txtsaledollar.Name = "txtsaledollar";
            this.txtsaledollar.Size = new System.Drawing.Size(289, 37);
            this.txtsaledollar.TabIndex = 36;
            // 
            // txtdollar
            // 
            this.txtdollar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtdollar.Location = new System.Drawing.Point(9, 357);
            this.txtdollar.Multiline = true;
            this.txtdollar.Name = "txtdollar";
            this.txtdollar.Size = new System.Drawing.Size(291, 37);
            this.txtdollar.TabIndex = 35;
            // 
            // txtquentity
            // 
            this.txtquentity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtquentity.Location = new System.Drawing.Point(10, 277);
            this.txtquentity.Multiline = true;
            this.txtquentity.Name = "txtquentity";
            this.txtquentity.Size = new System.Drawing.Size(291, 37);
            this.txtquentity.TabIndex = 34;
            // 
            // txtpriceLebanon
            // 
            this.txtpriceLebanon.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtpriceLebanon.Location = new System.Drawing.Point(359, 201);
            this.txtpriceLebanon.Multiline = true;
            this.txtpriceLebanon.Name = "txtpriceLebanon";
            this.txtpriceLebanon.Size = new System.Drawing.Size(289, 37);
            this.txtpriceLebanon.TabIndex = 33;
            // 
            // txtpricedollar
            // 
            this.txtpricedollar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtpricedollar.Location = new System.Drawing.Point(9, 201);
            this.txtpricedollar.Multiline = true;
            this.txtpricedollar.Name = "txtpricedollar";
            this.txtpricedollar.Size = new System.Drawing.Size(291, 37);
            this.txtpricedollar.TabIndex = 32;
            // 
            // txtbarcode
            // 
            this.txtbarcode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtbarcode.Location = new System.Drawing.Point(11, 126);
            this.txtbarcode.Multiline = true;
            this.txtbarcode.Name = "txtbarcode";
            this.txtbarcode.Size = new System.Drawing.Size(291, 37);
            this.txtbarcode.TabIndex = 31;
            this.txtbarcode.TextChanged += new System.EventHandler(this.txtbarcode_TextChanged);
            // 
            // txtname
            // 
            this.txtname.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtname.Location = new System.Drawing.Point(11, 40);
            this.txtname.Multiline = true;
            this.txtname.Name = "txtname";
            this.txtname.Size = new System.Drawing.Size(639, 37);
            this.txtname.TabIndex = 30;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(14, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 23);
            this.label1.TabIndex = 29;
            this.label1.Text = "اسم المنتج";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.product_id,
            this.name,
            this.category_id,
            this.stock_quantity,
            this.barcode,
            this.supplier_id,
            this.created_at,
            this.price_usd,
            this.price_lb,
            this.exchange_rate,
            this.sale_price_usd,
            this.sale_price_lb});
            this.dataGridView1.DataSource = this.productsBindingSource;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 82);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(831, 515);
            this.dataGridView1.TabIndex = 30;
            // 
            // pos_systemDataSet11
            // 
            this.pos_systemDataSet11.DataSetName = "pos_systemDataSet11";
            this.pos_systemDataSet11.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // productsBindingSource
            // 
            this.productsBindingSource.DataMember = "Products";
            this.productsBindingSource.DataSource = this.pos_systemDataSet11;
            // 
            // productsTableAdapter
            // 
            this.productsTableAdapter.ClearBeforeFill = true;
            // 
            // product_id
            // 
            this.product_id.DataPropertyName = "product_id";
            this.product_id.HeaderText = "product_id";
            this.product_id.MinimumWidth = 6;
            this.product_id.Name = "product_id";
            this.product_id.ReadOnly = true;
            // 
            // name
            // 
            this.name.DataPropertyName = "name";
            this.name.HeaderText = "name";
            this.name.MinimumWidth = 6;
            this.name.Name = "name";
            // 
            // category_id
            // 
            this.category_id.DataPropertyName = "category_id";
            this.category_id.HeaderText = "category_id";
            this.category_id.MinimumWidth = 6;
            this.category_id.Name = "category_id";
            // 
            // stock_quantity
            // 
            this.stock_quantity.DataPropertyName = "stock_quantity";
            this.stock_quantity.HeaderText = "stock_quantity";
            this.stock_quantity.MinimumWidth = 6;
            this.stock_quantity.Name = "stock_quantity";
            // 
            // barcode
            // 
            this.barcode.DataPropertyName = "barcode";
            this.barcode.HeaderText = "barcode";
            this.barcode.MinimumWidth = 6;
            this.barcode.Name = "barcode";
            // 
            // supplier_id
            // 
            this.supplier_id.DataPropertyName = "supplier_id";
            this.supplier_id.HeaderText = "supplier_id";
            this.supplier_id.MinimumWidth = 6;
            this.supplier_id.Name = "supplier_id";
            // 
            // created_at
            // 
            this.created_at.DataPropertyName = "created_at";
            this.created_at.HeaderText = "created_at";
            this.created_at.MinimumWidth = 6;
            this.created_at.Name = "created_at";
            // 
            // price_usd
            // 
            this.price_usd.DataPropertyName = "price_usd";
            this.price_usd.HeaderText = "price_usd";
            this.price_usd.MinimumWidth = 6;
            this.price_usd.Name = "price_usd";
            // 
            // price_lb
            // 
            this.price_lb.DataPropertyName = "price_lb";
            this.price_lb.HeaderText = "price_lb";
            this.price_lb.MinimumWidth = 6;
            this.price_lb.Name = "price_lb";
            // 
            // exchange_rate
            // 
            this.exchange_rate.DataPropertyName = "exchange_rate";
            this.exchange_rate.HeaderText = "exchange_rate";
            this.exchange_rate.MinimumWidth = 6;
            this.exchange_rate.Name = "exchange_rate";
            // 
            // sale_price_usd
            // 
            this.sale_price_usd.DataPropertyName = "sale_price_usd";
            this.sale_price_usd.HeaderText = "sale_price_usd";
            this.sale_price_usd.MinimumWidth = 6;
            this.sale_price_usd.Name = "sale_price_usd";
            // 
            // sale_price_lb
            // 
            this.sale_price_lb.DataPropertyName = "sale_price_lb";
            this.sale_price_lb.HeaderText = "sale_price_lb";
            this.sale_price_lb.MinimumWidth = 6;
            this.sale_price_lb.Name = "sale_price_lb";
            // 
            // AddProducts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1491, 666);
            this.MinimumSize = new System.Drawing.Size(1050, 650);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelheader);
            this.Name = "AddProducts";
            this.Text = "AddProducts";
            this.Load += new System.EventHandler(this.AddProducts_Load);
            this.panelheader.ResumeLayout(false);
            this.panelheader.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.suppliersBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet6)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.productsBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelheader;
        private System.Windows.Forms.Label labeldate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnexit;
        private System.Windows.Forms.Button btnadd;
        private pos_systemDataSet6 pos_systemDataSet6;
        private System.Windows.Forms.BindingSource suppliersBindingSource;
        private pos_systemDataSet6TableAdapters.SuppliersTableAdapter suppliersTableAdapter;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox comboSupplier;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox combobarcode;
        private System.Windows.Forms.ComboBox comboitem;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtsalelebanon;
        private System.Windows.Forms.TextBox txtsaledollar;
        private System.Windows.Forms.TextBox txtdollar;
        private System.Windows.Forms.TextBox txtquentity;
        private System.Windows.Forms.TextBox txtpriceLebanon;
        private System.Windows.Forms.TextBox txtpricedollar;
        private System.Windows.Forms.TextBox txtbarcode;
        private System.Windows.Forms.TextBox txtname;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private pos_systemDataSet11 pos_systemDataSet11;
        private System.Windows.Forms.BindingSource productsBindingSource;
        private pos_systemDataSet11TableAdapters.ProductsTableAdapter productsTableAdapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn product_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn category_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn stock_quantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn barcode;
        private System.Windows.Forms.DataGridViewTextBoxColumn supplier_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn created_at;
        private System.Windows.Forms.DataGridViewTextBoxColumn price_usd;
        private System.Windows.Forms.DataGridViewTextBoxColumn price_lb;
        private System.Windows.Forms.DataGridViewTextBoxColumn exchange_rate;
        private System.Windows.Forms.DataGridViewTextBoxColumn sale_price_usd;
        private System.Windows.Forms.DataGridViewTextBoxColumn sale_price_lb;
    }
}
