namespace Pos_System.Forms
{
    partial class Products
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
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.datagridSuppliers = new System.Windows.Forms.DataGridView();
            this.supplier_id1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contact_info = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.address = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Edit2 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Delete2 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.suppliersBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.pos_systemDataSet13 = new Pos_System.pos_systemDataSet13();
            this.panel3 = new System.Windows.Forms.Panel();
            this.datagridCategories = new System.Windows.Forms.DataGridView();
            this.category_id1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.category_name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Edit1 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Delete1 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.categoriesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.pos_systemDataSet12 = new Pos_System.pos_systemDataSet12();
            this.button2 = new System.Windows.Forms.Button();
            this.datagridProducts = new System.Windows.Forms.DataGridView();
            this.product_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.price_usd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.price_lb = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sale_price_usd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sale_price_lb = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.category_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.stock_quantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barcode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.exchange_rate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.supplier_id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.created_at = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Edit = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Delete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.productsBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.pos_systemDataSet5 = new Pos_System.pos_systemDataSet5();
            this.button1 = new System.Windows.Forms.Button();
            this.txtsearch = new System.Windows.Forms.TextBox();
            this.pos_systemDataSet4 = new Pos_System.pos_systemDataSet4();
            this.productsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.productsTableAdapter = new Pos_System.pos_systemDataSet4TableAdapters.ProductsTableAdapter();
            this.productsTableAdapter1 = new Pos_System.pos_systemDataSet5TableAdapters.ProductsTableAdapter();
            this.categoriesTableAdapter = new Pos_System.pos_systemDataSet12TableAdapters.CategoriesTableAdapter();
            this.suppliersTableAdapter = new Pos_System.pos_systemDataSet13TableAdapters.SuppliersTableAdapter();
            this.panelheader.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datagridSuppliers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.suppliersBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet13)).BeginInit();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datagridCategories)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.categoriesBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.datagridProducts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.productsBindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.productsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panelheader
            // 
            this.panelheader.BackColor = System.Drawing.Color.Black;
            this.panelheader.Controls.Add(this.labeldate);
            this.panelheader.Controls.Add(this.label3);
            this.panelheader.Controls.Add(this.label2);
            this.panelheader.Controls.Add(this.label1);
            this.panelheader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelheader.ForeColor = System.Drawing.Color.White;
            this.panelheader.Location = new System.Drawing.Point(0, 0);
            this.panelheader.Name = "panelheader";
            this.panelheader.Size = new System.Drawing.Size(1544, 82);
            this.panelheader.TabIndex = 1;
            // 
            // labeldate
            // 
            this.labeldate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labeldate.AutoSize = true;
            this.labeldate.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labeldate.Location = new System.Drawing.Point(1405, 20);
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
            this.label3.Size = new System.Drawing.Size(89, 23);
            this.label3.TabIndex = 2;
            this.label3.Text = "Products";
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
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(884, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bike";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel4);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.datagridProducts);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.txtsearch);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 82);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1544, 694);
            this.panel1.TabIndex = 2;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.panel2);
            this.panel4.Controls.Add(this.panel3);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(0, 437);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1544, 257);
            this.panel4.TabIndex = 5;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.datagridSuppliers);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(748, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(796, 257);
            this.panel2.TabIndex = 4;
            // 
            // datagridSuppliers
            // 
            this.datagridSuppliers.AutoGenerateColumns = false;
            this.datagridSuppliers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.datagridSuppliers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datagridSuppliers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.supplier_id1,
            this.name1,
            this.contact_info,
            this.address,
            this.Edit2,
            this.Delete2});
            this.datagridSuppliers.DataSource = this.suppliersBindingSource;
            this.datagridSuppliers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datagridSuppliers.Location = new System.Drawing.Point(0, 0);
            this.datagridSuppliers.Name = "datagridSuppliers";
            this.datagridSuppliers.RowHeadersWidth = 51;
            this.datagridSuppliers.RowTemplate.Height = 24;
            this.datagridSuppliers.Size = new System.Drawing.Size(796, 257);
            this.datagridSuppliers.TabIndex = 1;
            this.datagridSuppliers.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.datagridSuppliers_CellContentClick_1);
            // 
            // supplier_id1
            // 
            this.supplier_id1.DataPropertyName = "supplier_id";
            this.supplier_id1.HeaderText = "supplier_id";
            this.supplier_id1.MinimumWidth = 6;
            this.supplier_id1.Name = "supplier_id1";
            this.supplier_id1.ReadOnly = true;
            // 
            // name1
            // 
            this.name1.DataPropertyName = "name";
            this.name1.HeaderText = "name";
            this.name1.MinimumWidth = 6;
            this.name1.Name = "name1";
            // 
            // contact_info
            // 
            this.contact_info.DataPropertyName = "contact_info";
            this.contact_info.HeaderText = "contact_info";
            this.contact_info.MinimumWidth = 6;
            this.contact_info.Name = "contact_info";
            // 
            // address
            // 
            this.address.DataPropertyName = "address";
            this.address.HeaderText = "address";
            this.address.MinimumWidth = 6;
            this.address.Name = "address";
            // 
            // Edit2
            // 
            this.Edit2.DataPropertyName = "Edit";
            this.Edit2.HeaderText = "Edit";
            this.Edit2.MinimumWidth = 6;
            this.Edit2.Name = "Edit2";
            this.Edit2.ReadOnly = true;
            // 
            // Delete2
            // 
            this.Delete2.DataPropertyName = "Delete";
            this.Delete2.HeaderText = "Delete";
            this.Delete2.MinimumWidth = 6;
            this.Delete2.Name = "Delete2";
            this.Delete2.ReadOnly = true;
            // 
            // suppliersBindingSource
            // 
            this.suppliersBindingSource.DataMember = "Suppliers";
            this.suppliersBindingSource.DataSource = this.pos_systemDataSet13;
            // 
            // pos_systemDataSet13
            // 
            this.pos_systemDataSet13.DataSetName = "pos_systemDataSet13";
            this.pos_systemDataSet13.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.datagridCategories);
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(749, 257);
            this.panel3.TabIndex = 5;
            // 
            // datagridCategories
            // 
            this.datagridCategories.AutoGenerateColumns = false;
            this.datagridCategories.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.datagridCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datagridCategories.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.category_id1,
            this.category_name,
            this.description,
            this.Edit1,
            this.Delete1});
            this.datagridCategories.DataSource = this.categoriesBindingSource;
            this.datagridCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datagridCategories.Location = new System.Drawing.Point(0, 0);
            this.datagridCategories.Name = "datagridCategories";
            this.datagridCategories.RowHeadersWidth = 51;
            this.datagridCategories.RowTemplate.Height = 24;
            this.datagridCategories.Size = new System.Drawing.Size(749, 257);
            this.datagridCategories.TabIndex = 0;
            this.datagridCategories.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.datagridCategories_CellContentClick_1);
            // 
            // category_id1
            // 
            this.category_id1.DataPropertyName = "category_id";
            this.category_id1.HeaderText = "category_id";
            this.category_id1.MinimumWidth = 6;
            this.category_id1.Name = "category_id1";
            this.category_id1.ReadOnly = true;
            // 
            // category_name
            // 
            this.category_name.DataPropertyName = "category_name";
            this.category_name.HeaderText = "category_name";
            this.category_name.MinimumWidth = 6;
            this.category_name.Name = "category_name";
            // 
            // description
            // 
            this.description.DataPropertyName = "description";
            this.description.HeaderText = "description";
            this.description.MinimumWidth = 6;
            this.description.Name = "description";
            // 
            // Edit1
            // 
            this.Edit1.DataPropertyName = "Edit";
            this.Edit1.HeaderText = "Edit";
            this.Edit1.MinimumWidth = 6;
            this.Edit1.Name = "Edit1";
            this.Edit1.ReadOnly = true;
            // 
            // Delete1
            // 
            this.Delete1.DataPropertyName = "Delete";
            this.Delete1.HeaderText = "Delete";
            this.Delete1.MinimumWidth = 6;
            this.Delete1.Name = "Delete1";
            this.Delete1.ReadOnly = true;
            // 
            // categoriesBindingSource
            // 
            this.categoriesBindingSource.DataMember = "Categories";
            this.categoriesBindingSource.DataSource = this.pos_systemDataSet12;
            // 
            // pos_systemDataSet12
            // 
            this.pos_systemDataSet12.DataSetName = "pos_systemDataSet12";
            this.pos_systemDataSet12.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.Green;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(1417, 7);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(115, 60);
            this.button2.TabIndex = 3;
            this.button2.Text = "Excel";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // datagridProducts
            // 
            this.datagridProducts.AutoGenerateColumns = false;
            this.datagridProducts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.datagridProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datagridProducts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.product_id,
            this.name,
            this.price_usd,
            this.price_lb,
            this.sale_price_usd,
            this.sale_price_lb,
            this.category_id,
            this.stock_quantity,
            this.barcode,
            this.exchange_rate,
            this.supplier_id,
            this.created_at,
            this.Edit,
            this.Delete});
            this.datagridProducts.DataSource = this.productsBindingSource1;
            this.datagridProducts.Location = new System.Drawing.Point(12, 106);
            this.datagridProducts.Name = "datagridProducts";
            this.datagridProducts.RowHeadersWidth = 51;
            this.datagridProducts.RowTemplate.Height = 24;
            this.datagridProducts.Size = new System.Drawing.Size(1520, 333);
            this.datagridProducts.TabIndex = 2;
            this.datagridProducts.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.datagridProducts_CellContentClick_1);
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
            // exchange_rate
            // 
            this.exchange_rate.DataPropertyName = "exchange_rate";
            this.exchange_rate.HeaderText = "exchange_rate";
            this.exchange_rate.MinimumWidth = 6;
            this.exchange_rate.Name = "exchange_rate";
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
            // Edit
            // 
            this.Edit.DataPropertyName = "Edit";
            this.Edit.HeaderText = "Edit";
            this.Edit.MinimumWidth = 6;
            this.Edit.Name = "Edit";
            this.Edit.ReadOnly = true;
            // 
            // Delete
            // 
            this.Delete.DataPropertyName = "Delete";
            this.Delete.HeaderText = "Delete";
            this.Delete.MinimumWidth = 6;
            this.Delete.Name = "Delete";
            this.Delete.ReadOnly = true;
            // 
            // productsBindingSource1
            // 
            this.productsBindingSource1.DataMember = "Products";
            this.productsBindingSource1.DataSource = this.pos_systemDataSet5;
            // 
            // pos_systemDataSet5
            // 
            this.pos_systemDataSet5.DataSetName = "pos_systemDataSet5";
            this.pos_systemDataSet5.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Blue;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(954, 7);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 60);
            this.button1.TabIndex = 1;
            this.button1.Text = "اضافة منتج";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtsearch
            // 
            this.txtsearch.Location = new System.Drawing.Point(399, 15);
            this.txtsearch.Multiline = true;
            this.txtsearch.Name = "txtsearch";
            this.txtsearch.Size = new System.Drawing.Size(543, 40);
            this.txtsearch.TabIndex = 0;
            this.txtsearch.TextChanged += new System.EventHandler(this.txtsearch_TextChanged);
            // 
            // pos_systemDataSet4
            // 
            this.pos_systemDataSet4.DataSetName = "pos_systemDataSet4";
            this.pos_systemDataSet4.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // productsBindingSource
            // 
            this.productsBindingSource.DataMember = "Products";
            this.productsBindingSource.DataSource = this.pos_systemDataSet4;
            // 
            // productsTableAdapter
            // 
            this.productsTableAdapter.ClearBeforeFill = true;
            // 
            // productsTableAdapter1
            // 
            this.productsTableAdapter1.ClearBeforeFill = true;
            // 
            // categoriesTableAdapter
            // 
            this.categoriesTableAdapter.ClearBeforeFill = true;
            // 
            // suppliersTableAdapter
            // 
            this.suppliersTableAdapter.ClearBeforeFill = true;
            // 
            // Products
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1544, 776);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelheader);
            this.Name = "Products";
            this.Text = "Products";
            this.Load += new System.EventHandler(this.Products_Load);
            this.panelheader.ResumeLayout(false);
            this.panelheader.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datagridSuppliers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.suppliersBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet13)).EndInit();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datagridCategories)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.categoriesBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.datagridProducts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.productsBindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.productsBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelheader;
        private System.Windows.Forms.Label labeldate;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtsearch;
        private System.Windows.Forms.DataGridView datagridProducts;
        private pos_systemDataSet4 pos_systemDataSet4;
        private System.Windows.Forms.BindingSource productsBindingSource;
        private pos_systemDataSet4TableAdapters.ProductsTableAdapter productsTableAdapter;
        private System.Windows.Forms.Button button2;
        private pos_systemDataSet5 pos_systemDataSet5;
        private System.Windows.Forms.BindingSource productsBindingSource1;
        private pos_systemDataSet5TableAdapters.ProductsTableAdapter productsTableAdapter1;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView datagridSuppliers;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DataGridView datagridCategories;
        private pos_systemDataSet12 pos_systemDataSet12;
        private System.Windows.Forms.BindingSource categoriesBindingSource;
        private pos_systemDataSet12TableAdapters.CategoriesTableAdapter categoriesTableAdapter;
        private pos_systemDataSet13 pos_systemDataSet13;
        private System.Windows.Forms.BindingSource suppliersBindingSource;
        private pos_systemDataSet13TableAdapters.SuppliersTableAdapter suppliersTableAdapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn category_id1;
        private System.Windows.Forms.DataGridViewTextBoxColumn category_name;
        private System.Windows.Forms.DataGridViewTextBoxColumn description;
        private System.Windows.Forms.DataGridViewButtonColumn Edit1;
        private System.Windows.Forms.DataGridViewButtonColumn Delete1;
        private System.Windows.Forms.DataGridViewTextBoxColumn supplier_id1;
        private System.Windows.Forms.DataGridViewTextBoxColumn name1;
        private System.Windows.Forms.DataGridViewTextBoxColumn contact_info;
        private System.Windows.Forms.DataGridViewTextBoxColumn address;
        private System.Windows.Forms.DataGridViewButtonColumn Edit2;
        private System.Windows.Forms.DataGridViewButtonColumn Delete2;
        private System.Windows.Forms.DataGridViewTextBoxColumn product_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn price_usd;
        private System.Windows.Forms.DataGridViewTextBoxColumn price_lb;
        private System.Windows.Forms.DataGridViewTextBoxColumn sale_price_usd;
        private System.Windows.Forms.DataGridViewTextBoxColumn sale_price_lb;
        private System.Windows.Forms.DataGridViewTextBoxColumn category_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn stock_quantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn barcode;
        private System.Windows.Forms.DataGridViewTextBoxColumn exchange_rate;
        private System.Windows.Forms.DataGridViewTextBoxColumn supplier_id;
        private System.Windows.Forms.DataGridViewTextBoxColumn created_at;
        private System.Windows.Forms.DataGridViewButtonColumn Edit;
        private System.Windows.Forms.DataGridViewButtonColumn Delete;
    }
}