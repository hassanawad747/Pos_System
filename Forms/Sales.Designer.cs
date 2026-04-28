namespace Pos_System.Forms
{
    partial class Sales
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
            this.panelheader = new System.Windows.Forms.Panel();
            this.labeldate = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.txtsearch = new System.Windows.Forms.TextBox();
            this.btndollar = new System.Windows.Forms.Button();
            this.btnsare3 = new System.Windows.Forms.Button();
            this.btn3rdfetora = new System.Windows.Forms.Button();
            this.btnmortaja3 = new System.Windows.Forms.Button();
            this.btndelete = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPaidAmount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbCustomer = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtdollar = new System.Windows.Forms.TextBox();
            this.btnSaveSale = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnexcel = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.txtSaleId = new System.Windows.Forms.TextBox();
            this.rbLebanon = new System.Windows.Forms.RadioButton();
            this.rbDollar = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.comboPaymentMethod = new System.Windows.Forms.ComboBox();
            this.lbtotal_lebanon = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.txtquantity = new System.Windows.Forms.TextBox();
            this.cmbCategory = new System.Windows.Forms.ComboBox();
            this.pos_systemDataSet21 = new Pos_System.pos_systemDataSet21();
            this.pnlProducts = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.datagridsales = new System.Windows.Forms.DataGridView();
            this.pos_systemDataSet20 = new Pos_System.pos_systemDataSet20();
            this.pos_systemDataSet22 = new Pos_System.pos_systemDataSet22();
            this.panelheader.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet21)).BeginInit();
            this.panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.datagridsales)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet20)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet22)).BeginInit();
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
            this.panelheader.Size = new System.Drawing.Size(1776, 74);
            this.panelheader.TabIndex = 0;
            // 
            // labeldate
            // 
            this.labeldate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labeldate.AutoSize = true;
            this.labeldate.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labeldate.Location = new System.Drawing.Point(1630, 18);
            this.labeldate.Name = "labeldate";
            this.labeldate.Size = new System.Drawing.Size(48, 23);
            this.labeldate.TabIndex = 6;
            this.labeldate.Text = "Date";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(14, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 23);
            this.label3.TabIndex = 2;
            this.label3.Text = "Sales";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(976, 8);
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
            this.label1.Location = new System.Drawing.Point(888, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "Bike";
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.Blue;
            this.button5.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button5.ForeColor = System.Drawing.Color.White;
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(443, 16);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(112, 40);
            this.button5.TabIndex = 8;
            this.button5.Text = "اضافة";
            this.button5.UseVisualStyleBackColor = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // txtsearch
            // 
            this.txtsearch.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtsearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtsearch.Location = new System.Drawing.Point(16, 16);
            this.txtsearch.Multiline = true;
            this.txtsearch.Name = "txtsearch";
            this.txtsearch.Size = new System.Drawing.Size(421, 40);
            this.txtsearch.TabIndex = 4;
            this.txtsearch.Text = "search by name product or barcode";
            // 
            // btndollar
            // 
            this.btndollar.BackColor = System.Drawing.Color.Blue;
            this.btndollar.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btndollar.ForeColor = System.Drawing.Color.White;
            this.btndollar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.btndollar.Location = new System.Drawing.Point(18, 18);
            this.btndollar.Name = "btndollar";
            this.btndollar.Size = new System.Drawing.Size(120, 40);
            this.btndollar.TabIndex = 11;
            this.btndollar.Text = "حفظ";
            this.btndollar.UseVisualStyleBackColor = false;
            this.btndollar.Click += new System.EventHandler(this.btndollar_Click);
            // 
            // btnsare3
            // 
            this.btnsare3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnsare3.BackColor = System.Drawing.Color.Blue;
            this.btnsare3.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnsare3.ForeColor = System.Drawing.Color.White;
            this.btnsare3.Location = new System.Drawing.Point(18, 590);
            this.btnsare3.Name = "btnsare3";
            this.btnsare3.Size = new System.Drawing.Size(392, 50);
            this.btnsare3.TabIndex = 10;
            this.btnsare3.Text = "بيع سريع";
            this.btnsare3.UseVisualStyleBackColor = false;
            this.btnsare3.Click += new System.EventHandler(this.button4_Click);
            // 
            // btn3rdfetora
            // 
            this.btn3rdfetora.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn3rdfetora.BackColor = System.Drawing.Color.Orange;
            this.btn3rdfetora.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn3rdfetora.ForeColor = System.Drawing.Color.White;
            this.btn3rdfetora.Location = new System.Drawing.Point(214, 648);
            this.btn3rdfetora.Name = "btn3rdfetora";
            this.btn3rdfetora.Size = new System.Drawing.Size(196, 50);
            this.btn3rdfetora.TabIndex = 9;
            this.btn3rdfetora.Text = "عرض الفاتورة";
            this.btn3rdfetora.UseVisualStyleBackColor = false;
            this.btn3rdfetora.Click += new System.EventHandler(this.btn3rdfetora_Click);
            // 
            // btnmortaja3
            // 
            this.btnmortaja3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnmortaja3.BackColor = System.Drawing.Color.Gray;
            this.btnmortaja3.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnmortaja3.ForeColor = System.Drawing.Color.White;
            this.btnmortaja3.Location = new System.Drawing.Point(18, 706);
            this.btnmortaja3.Name = "btnmortaja3";
            this.btnmortaja3.Size = new System.Drawing.Size(392, 50);
            this.btnmortaja3.TabIndex = 8;
            this.btnmortaja3.Text = "مرتجع";
            this.btnmortaja3.UseVisualStyleBackColor = false;
            this.btnmortaja3.Click += new System.EventHandler(this.button2_Click);
            // 
            // btndelete
            // 
            this.btndelete.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btndelete.BackColor = System.Drawing.Color.Red;
            this.btndelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btndelete.ForeColor = System.Drawing.Color.White;
            this.btndelete.Location = new System.Drawing.Point(18, 764);
            this.btndelete.Name = "btndelete";
            this.btndelete.Size = new System.Drawing.Size(392, 50);
            this.btndelete.TabIndex = 7;
            this.btndelete.Text = "مسح الفاتورة";
            this.btndelete.UseVisualStyleBackColor = false;
            this.btndelete.Click += new System.EventHandler(this.button1_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.Location = new System.Drawing.Point(271, 137);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(141, 23);
            this.label6.TabIndex = 6;
            this.label6.Text = " : المبلغ المدفوع ";
            // 
            // txtPaidAmount
            // 
            this.txtPaidAmount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPaidAmount.Location = new System.Drawing.Point(18, 163);
            this.txtPaidAmount.Multiline = true;
            this.txtPaidAmount.Name = "txtPaidAmount";
            this.txtPaidAmount.Size = new System.Drawing.Size(392, 38);
            this.txtPaidAmount.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Location = new System.Drawing.Point(318, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 23);
            this.label5.TabIndex = 4;
            this.label5.Text = " : الزبون ";
            // 
            // cmbCustomer
            // 
            this.cmbCustomer.FormattingEnabled = true;
            this.cmbCustomer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCustomer.Location = new System.Drawing.Point(18, 98);
            this.cmbCustomer.Name = "cmbCustomer";
            this.cmbCustomer.Size = new System.Drawing.Size(392, 24);
            this.cmbCustomer.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.Location = new System.Drawing.Point(286, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 23);
            this.label4.TabIndex = 2;
            this.label4.Text = " : سعر الدولار ";
            // 
            // txtdollar
            // 
            this.txtdollar.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtdollar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtdollar.Location = new System.Drawing.Point(154, 18);
            this.txtdollar.Multiline = true;
            this.txtdollar.Name = "txtdollar";
            this.txtdollar.Size = new System.Drawing.Size(126, 38);
            this.txtdollar.TabIndex = 1;
            // 
            // btnSaveSale
            // 
            this.btnSaveSale.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveSale.BackColor = System.Drawing.Color.Green;
            this.btnSaveSale.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSaveSale.ForeColor = System.Drawing.Color.White;
            this.btnSaveSale.Location = new System.Drawing.Point(18, 532);
            this.btnSaveSale.Name = "btnSaveSale";
            this.btnSaveSale.Size = new System.Drawing.Size(392, 50);
            this.btnSaveSale.TabIndex = 0;
            this.btnSaveSale.Text = "بيع مع فاتورة";
            this.btnSaveSale.UseVisualStyleBackColor = false;
            this.btnSaveSale.Click += new System.EventHandler(this.btnSaveSale_Click_1);
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.btnexcel);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.txtSaleId);
            this.panel1.Controls.Add(this.rbLebanon);
            this.panel1.Controls.Add(this.rbDollar);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.comboPaymentMethod);
            this.panel1.Controls.Add(this.lbtotal_lebanon);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.lblTotal);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.btndelete);
            this.panel1.Controls.Add(this.btnmortaja3);
            this.panel1.Controls.Add(this.btndollar);
            this.panel1.Controls.Add(this.btnSaveSale);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.btn3rdfetora);
            this.panel1.Controls.Add(this.txtPaidAmount);
            this.panel1.Controls.Add(this.btnsare3);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.txtdollar);
            this.panel1.Controls.Add(this.cmbCustomer);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(1334, 74);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(12);
            this.panel1.Size = new System.Drawing.Size(442, 832);
            this.panel1.TabIndex = 13;
            // 
            // btnexcel
            // 
            this.btnexcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnexcel.BackColor = System.Drawing.Color.Green;
            this.btnexcel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnexcel.ForeColor = System.Drawing.Color.White;
            this.btnexcel.Location = new System.Drawing.Point(18, 648);
            this.btnexcel.Name = "btnexcel";
            this.btnexcel.Size = new System.Drawing.Size(188, 50);
            this.btnexcel.TabIndex = 24;
            this.btnexcel.Text = "طبع الفاتورة excel";
            this.btnexcel.UseVisualStyleBackColor = false;
            this.btnexcel.Click += new System.EventHandler(this.button3_Click);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.Location = new System.Drawing.Point(270, 315);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(141, 23);
            this.label10.TabIndex = 23;
            this.label10.Text = ": ادخل رقم الفاتورة";
            // 
            // txtSaleId
            // 
            this.txtSaleId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSaleId.Location = new System.Drawing.Point(18, 346);
            this.txtSaleId.Multiline = true;
            this.txtSaleId.Name = "txtSaleId";
            this.txtSaleId.Size = new System.Drawing.Size(392, 38);
            this.txtSaleId.TabIndex = 22;
            // 
            // rbLebanon
            // 
            this.rbLebanon.AutoSize = true;
            this.rbLebanon.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbLebanon.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.rbLebanon.Location = new System.Drawing.Point(126, 221);
            this.rbLebanon.Name = "rbLebanon";
            this.rbLebanon.Size = new System.Drawing.Size(117, 29);
            this.rbLebanon.TabIndex = 21;
            this.rbLebanon.TabStop = true;
            this.rbLebanon.Text = "Lebanon";
            this.rbLebanon.UseVisualStyleBackColor = true;
            // 
            // rbDollar
            // 
            this.rbDollar.AutoSize = true;
            this.rbDollar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbDollar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.rbDollar.Location = new System.Drawing.Point(28, 221);
            this.rbDollar.Name = "rbDollar";
            this.rbDollar.Size = new System.Drawing.Size(89, 29);
            this.rbDollar.TabIndex = 20;
            this.rbDollar.TabStop = true;
            this.rbDollar.Text = "Dollar";
            this.rbDollar.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Yi Baiti", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Location = new System.Drawing.Point(319, 252);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 23);
            this.label7.TabIndex = 19;
            this.label7.Text = ": الدفع عبر";
            // 
            // comboPaymentMethod
            // 
            this.comboPaymentMethod.FormattingEnabled = true;
            this.comboPaymentMethod.Items.AddRange(new object[] {
            "cash",
            "card",
            "wallet"});
            this.comboPaymentMethod.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboPaymentMethod.Location = new System.Drawing.Point(18, 281);
            this.comboPaymentMethod.Name = "comboPaymentMethod";
            this.comboPaymentMethod.Size = new System.Drawing.Size(392, 24);
            this.comboPaymentMethod.TabIndex = 18;
            // 
            // lbtotal_lebanon
            // 
            this.lbtotal_lebanon.AutoSize = false;
            this.lbtotal_lebanon.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbtotal_lebanon.Location = new System.Drawing.Point(18, 494);
            this.lbtotal_lebanon.Name = "lbtotal_lebanon";
            this.lbtotal_lebanon.Size = new System.Drawing.Size(392, 32);
            this.lbtotal_lebanon.TabIndex = 17;
            this.lbtotal_lebanon.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label11
            // 
            this.label11.AutoSize = false;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(18, 462);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(392, 28);
            this.label11.TabIndex = 16;
            this.label11.Text = "Total Lebanon:";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = false;
            this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotal.Location = new System.Drawing.Point(18, 432);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(392, 32);
            this.lblTotal.TabIndex = 15;
            this.lblTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.AutoSize = false;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(18, 400);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(392, 28);
            this.label9.TabIndex = 14;
            this.label9.Text = "Total Dollar:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.txtquantity);
            this.panel2.Controls.Add(this.cmbCategory);
            this.panel2.Controls.Add(this.button5);
            this.panel2.Controls.Add(this.txtsearch);
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 74);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
            this.panel2.Size = new System.Drawing.Size(1334, 70);
            this.panel2.TabIndex = 14;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft YaHei", 13.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.Location = new System.Drawing.Point(803, 19);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 31);
            this.label8.TabIndex = 12;
            this.label8.Text = ": العدد";
            // 
            // txtquantity
            // 
            this.txtquantity.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtquantity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtquantity.Location = new System.Drawing.Point(678, 16);
            this.txtquantity.Multiline = true;
            this.txtquantity.Name = "txtquantity";
            this.txtquantity.Size = new System.Drawing.Size(120, 38);
            this.txtquantity.TabIndex = 10;
            // 
            // cmbCategory
            // 
            this.cmbCategory.DisplayMember = "category_name";
            this.cmbCategory.FormattingEnabled = true;
            this.cmbCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCategory.Location = new System.Drawing.Point(906, 21);
            this.cmbCategory.Name = "cmbCategory";
            this.cmbCategory.Size = new System.Drawing.Size(210, 24);
            this.cmbCategory.TabIndex = 9;
            this.cmbCategory.ValueMember = "category_name";
            this.cmbCategory.SelectedIndexChanged += new System.EventHandler(this.cmbCategory_SelectedIndexChanged_1);
            // 
            // pos_systemDataSet21
            // 
            this.pos_systemDataSet21.DataSetName = "pos_systemDataSet21";
            this.pos_systemDataSet21.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // pnlProducts
            // 
            this.pnlProducts.BackColor = System.Drawing.Color.White;
            this.pnlProducts.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlProducts.Location = new System.Drawing.Point(0, 144);
            this.pnlProducts.Name = "pnlProducts";
            this.pnlProducts.Padding = new System.Windows.Forms.Padding(16, 14, 16, 10);
            this.pnlProducts.Size = new System.Drawing.Size(1334, 300);
            this.pnlProducts.TabIndex = 15;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(252)))));
            this.panel4.Controls.Add(this.datagridsales);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 444);
            this.panel4.Name = "panel4";
            this.panel4.Padding = new System.Windows.Forms.Padding(16, 0, 16, 16);
            this.panel4.Size = new System.Drawing.Size(1334, 462);
            this.panel4.TabIndex = 16;
            // 
            // datagridsales
            // 
            this.datagridsales.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.datagridsales.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.datagridsales.Dock = System.Windows.Forms.DockStyle.Fill;
            this.datagridsales.Location = new System.Drawing.Point(16, 0);
            this.datagridsales.Name = "datagridsales";
            this.datagridsales.RowHeadersWidth = 51;
            this.datagridsales.RowTemplate.Height = 24;
            this.datagridsales.Size = new System.Drawing.Size(1302, 446);
            this.datagridsales.TabIndex = 0;
            this.datagridsales.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.datagridsales_CellContentClick);
            this.datagridsales.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.datagridsales_CellEndEdit_1);
            // 
            // pos_systemDataSet20
            // 
            this.pos_systemDataSet20.DataSetName = "pos_systemDataSet20";
            this.pos_systemDataSet20.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // pos_systemDataSet22
            // 
            this.pos_systemDataSet22.DataSetName = "pos_systemDataSet22";
            this.pos_systemDataSet22.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // Sales
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(244)))), ((int)(((byte)(247)))), ((int)(((byte)(252)))));
            this.ClientSize = new System.Drawing.Size(1776, 906);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.pnlProducts);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelheader);
            this.Name = "Sales";
            this.Text = "Sales";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Sales_Load);
            this.panelheader.ResumeLayout(false);
            this.panelheader.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet21)).EndInit();
            this.panel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.datagridsales)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet20)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pos_systemDataSet22)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelheader;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labeldate;
        private System.Windows.Forms.TextBox txtsearch;
        private System.Windows.Forms.Button btnSaveSale;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbCustomer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtdollar;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPaidAmount;
        private System.Windows.Forms.Button btnsare3;
        private System.Windows.Forms.Button btn3rdfetora;
        private System.Windows.Forms.Button btnmortaja3;
        private System.Windows.Forms.Button btndelete;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button btndollar;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel pnlProducts;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.DataGridView datagridsales;
        private System.Windows.Forms.ComboBox cmbCategory;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtquantity;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label label9;
        private pos_systemDataSet20 pos_systemDataSet20;
        private pos_systemDataSet21 pos_systemDataSet21;
        private System.Windows.Forms.Label lbtotal_lebanon;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboPaymentMethod;
        private pos_systemDataSet22 pos_systemDataSet22;
        private System.Windows.Forms.RadioButton rbLebanon;
        private System.Windows.Forms.RadioButton rbDollar;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtSaleId;
        private System.Windows.Forms.Button btnexcel;
    }
}
