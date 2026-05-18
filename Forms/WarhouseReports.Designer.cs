namespace Pos_System.Forms
{
    partial class WarhouseReports
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelheader = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.summaryPanel = new System.Windows.Forms.TableLayoutPanel();
            this.cardCapital = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.lbkematr2slmal = new System.Windows.Forms.Label();
            this.cardPurchase = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.lbtaklefetshera2 = new System.Windows.Forms.Label();
            this.cardItems = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.lb3dadalsnef = new System.Windows.Forms.Label();
            this.cardUnits = new System.Windows.Forms.Panel();
            this.label8 = new System.Windows.Forms.Label();
            this.lbejmale = new System.Windows.Forms.Label();
            this.cardLowStock = new System.Windows.Forms.Panel();
            this.lblLowStockTitle = new System.Windows.Forms.Label();
            this.lblLowStockValue = new System.Windows.Forms.Label();
            this.toolbarPanel = new System.Windows.Forms.Panel();
            this.lblSearch = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblStockFilter = new System.Windows.Forms.Label();
            this.cmbStockFilter = new System.Windows.Forms.ComboBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnexcel = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panelheader.SuspendLayout();
            this.summaryPanel.SuspendLayout();
            this.cardCapital.SuspendLayout();
            this.cardPurchase.SuspendLayout();
            this.cardItems.SuspendLayout();
            this.cardUnits.SuspendLayout();
            this.cardLowStock.SuspendLayout();
            this.toolbarPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panelheader
            // 
            this.panelheader.BackColor = System.Drawing.Color.Black;
            this.panelheader.Controls.Add(this.label3);
            this.panelheader.Controls.Add(this.label2);
            this.panelheader.Controls.Add(this.label1);
            this.panelheader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelheader.ForeColor = System.Drawing.Color.White;
            this.panelheader.Location = new System.Drawing.Point(0, 0);
            this.panelheader.Name = "panelheader";
            this.panelheader.Padding = new System.Windows.Forms.Padding(16, 0, 16, 0);
            this.panelheader.Size = new System.Drawing.Size(1280, 78);
            this.panelheader.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(16, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(198, 32);
            this.label3.TabIndex = 0;
            this.label3.Text = "Warehouse Reports";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 21F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic);
            this.label2.Location = new System.Drawing.Point(675, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 47);
            this.label2.TabIndex = 2;
            this.label2.Text = "ZONE";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 21F, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic);
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(583, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 47);
            this.label1.TabIndex = 1;
            this.label1.Text = "BIKE";
            // 
            // summaryPanel
            // 
            this.summaryPanel.BackColor = System.Drawing.Color.White;
            this.summaryPanel.ColumnCount = 5;
            this.summaryPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.summaryPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.summaryPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.summaryPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.summaryPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.summaryPanel.Controls.Add(this.cardCapital, 0, 0);
            this.summaryPanel.Controls.Add(this.cardPurchase, 1, 0);
            this.summaryPanel.Controls.Add(this.cardItems, 2, 0);
            this.summaryPanel.Controls.Add(this.cardUnits, 3, 0);
            this.summaryPanel.Controls.Add(this.cardLowStock, 4, 0);
            this.summaryPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.summaryPanel.Location = new System.Drawing.Point(0, 78);
            this.summaryPanel.Name = "summaryPanel";
            this.summaryPanel.Padding = new System.Windows.Forms.Padding(12);
            this.summaryPanel.RowCount = 1;
            this.summaryPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.summaryPanel.Size = new System.Drawing.Size(1280, 118);
            this.summaryPanel.TabIndex = 1;
            // 
            // cardCapital
            // 
            this.cardCapital.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.cardCapital.Controls.Add(this.label4);
            this.cardCapital.Controls.Add(this.lbkematr2slmal);
            this.cardCapital.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardCapital.Margin = new System.Windows.Forms.Padding(6);
            this.cardCapital.Padding = new System.Windows.Forms.Padding(12);
            this.cardCapital.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.label4.Location = new System.Drawing.Point(12, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 21);
            this.label4.TabIndex = 0;
            this.label4.Text = "Capital Value";
            // 
            // lbkematr2slmal
            // 
            this.lbkematr2slmal.AutoSize = true;
            this.lbkematr2slmal.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lbkematr2slmal.Location = new System.Drawing.Point(12, 43);
            this.lbkematr2slmal.Name = "lbkematr2slmal";
            this.lbkematr2slmal.Size = new System.Drawing.Size(74, 35);
            this.lbkematr2slmal.TabIndex = 1;
            this.lbkematr2slmal.Text = "$0.00";
            // 
            // cardPurchase
            // 
            this.cardPurchase.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.cardPurchase.Controls.Add(this.label7);
            this.cardPurchase.Controls.Add(this.lbtaklefetshera2);
            this.cardPurchase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardPurchase.Margin = new System.Windows.Forms.Padding(6);
            this.cardPurchase.Padding = new System.Windows.Forms.Padding(12);
            this.cardPurchase.TabIndex = 1;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.label7.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.label7.Location = new System.Drawing.Point(12, 11);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 21);
            this.label7.TabIndex = 0;
            this.label7.Text = "Purchase Cost";
            // 
            // lbtaklefetshera2
            // 
            this.lbtaklefetshera2.AutoSize = true;
            this.lbtaklefetshera2.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lbtaklefetshera2.Location = new System.Drawing.Point(12, 43);
            this.lbtaklefetshera2.Name = "lbtaklefetshera2";
            this.lbtaklefetshera2.Size = new System.Drawing.Size(74, 35);
            this.lbtaklefetshera2.TabIndex = 1;
            this.lbtaklefetshera2.Text = "$0.00";
            // 
            // cardItems
            // 
            this.cardItems.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.cardItems.Controls.Add(this.label6);
            this.cardItems.Controls.Add(this.lb3dadalsnef);
            this.cardItems.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardItems.Margin = new System.Windows.Forms.Padding(6);
            this.cardItems.Padding = new System.Windows.Forms.Padding(12);
            this.cardItems.TabIndex = 2;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.label6.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.label6.Location = new System.Drawing.Point(12, 11);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(51, 21);
            this.label6.TabIndex = 0;
            this.label6.Text = "Items";
            // 
            // lb3dadalsnef
            // 
            this.lb3dadalsnef.AutoSize = true;
            this.lb3dadalsnef.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lb3dadalsnef.Location = new System.Drawing.Point(12, 43);
            this.lb3dadalsnef.Name = "lb3dadalsnef";
            this.lb3dadalsnef.Size = new System.Drawing.Size(29, 35);
            this.lb3dadalsnef.TabIndex = 1;
            this.lb3dadalsnef.Text = "0";
            // 
            // cardUnits
            // 
            this.cardUnits.BackColor = System.Drawing.Color.FromArgb(248, 250, 252);
            this.cardUnits.Controls.Add(this.label8);
            this.cardUnits.Controls.Add(this.lbejmale);
            this.cardUnits.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardUnits.Margin = new System.Windows.Forms.Padding(6);
            this.cardUnits.Padding = new System.Windows.Forms.Padding(12);
            this.cardUnits.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.label8.ForeColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.label8.Location = new System.Drawing.Point(12, 11);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(94, 21);
            this.label8.TabIndex = 0;
            this.label8.Text = "Total Units";
            // 
            // lbejmale
            // 
            this.lbejmale.AutoSize = true;
            this.lbejmale.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lbejmale.Location = new System.Drawing.Point(12, 43);
            this.lbejmale.Name = "lbejmale";
            this.lbejmale.Size = new System.Drawing.Size(29, 35);
            this.lbejmale.TabIndex = 1;
            this.lbejmale.Text = "0";
            // 
            // cardLowStock
            // 
            this.cardLowStock.BackColor = System.Drawing.Color.FromArgb(254, 242, 242);
            this.cardLowStock.Controls.Add(this.lblLowStockTitle);
            this.cardLowStock.Controls.Add(this.lblLowStockValue);
            this.cardLowStock.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cardLowStock.Margin = new System.Windows.Forms.Padding(6);
            this.cardLowStock.Padding = new System.Windows.Forms.Padding(12);
            this.cardLowStock.TabIndex = 4;
            // 
            // lblLowStockTitle
            // 
            this.lblLowStockTitle.AutoSize = true;
            this.lblLowStockTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblLowStockTitle.ForeColor = System.Drawing.Color.FromArgb(153, 27, 27);
            this.lblLowStockTitle.Location = new System.Drawing.Point(12, 11);
            this.lblLowStockTitle.Name = "lblLowStockTitle";
            this.lblLowStockTitle.Size = new System.Drawing.Size(87, 21);
            this.lblLowStockTitle.TabIndex = 0;
            this.lblLowStockTitle.Text = "Low Stock";
            // 
            // lblLowStockValue
            // 
            this.lblLowStockValue.AutoSize = true;
            this.lblLowStockValue.Font = new System.Drawing.Font("Segoe UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblLowStockValue.ForeColor = System.Drawing.Color.FromArgb(153, 27, 27);
            this.lblLowStockValue.Location = new System.Drawing.Point(12, 43);
            this.lblLowStockValue.Name = "lblLowStockValue";
            this.lblLowStockValue.Size = new System.Drawing.Size(29, 35);
            this.lblLowStockValue.TabIndex = 1;
            this.lblLowStockValue.Text = "0";
            // 
            // toolbarPanel
            // 
            this.toolbarPanel.BackColor = System.Drawing.Color.White;
            this.toolbarPanel.Controls.Add(this.lblSearch);
            this.toolbarPanel.Controls.Add(this.txtSearch);
            this.toolbarPanel.Controls.Add(this.lblStockFilter);
            this.toolbarPanel.Controls.Add(this.cmbStockFilter);
            this.toolbarPanel.Controls.Add(this.btnRefresh);
            this.toolbarPanel.Controls.Add(this.btnexcel);
            this.toolbarPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.toolbarPanel.Location = new System.Drawing.Point(0, 196);
            this.toolbarPanel.Name = "toolbarPanel";
            this.toolbarPanel.Padding = new System.Windows.Forms.Padding(16, 10, 16, 10);
            this.toolbarPanel.Size = new System.Drawing.Size(1280, 66);
            this.toolbarPanel.TabIndex = 2;
            // 
            // lblSearch
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSearch.Location = new System.Drawing.Point(16, 22);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(61, 23);
            this.lblSearch.TabIndex = 0;
            this.lblSearch.Text = "Search";
            // 
            // txtSearch
            // 
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.txtSearch.Location = new System.Drawing.Point(85, 18);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(330, 30);
            this.txtSearch.TabIndex = 1;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblStockFilter
            // 
            this.lblStockFilter.AutoSize = true;
            this.lblStockFilter.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStockFilter.Location = new System.Drawing.Point(438, 22);
            this.lblStockFilter.Name = "lblStockFilter";
            this.lblStockFilter.Size = new System.Drawing.Size(50, 23);
            this.lblStockFilter.TabIndex = 2;
            this.lblStockFilter.Text = "Stock";
            // 
            // cmbStockFilter
            // 
            this.cmbStockFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbStockFilter.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbStockFilter.FormattingEnabled = true;
            this.cmbStockFilter.Items.AddRange(new object[] {
            "All",
            "Low",
            "Out"});
            this.cmbStockFilter.Location = new System.Drawing.Point(496, 18);
            this.cmbStockFilter.Name = "cmbStockFilter";
            this.cmbStockFilter.Size = new System.Drawing.Size(150, 31);
            this.cmbStockFilter.TabIndex = 3;
            this.cmbStockFilter.SelectedIndexChanged += new System.EventHandler(this.cmbStockFilter_SelectedIndexChanged);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.FromArgb(71, 85, 105);
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(665, 15);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(110, 36);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnexcel
            // 
            this.btnexcel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnexcel.BackColor = System.Drawing.Color.FromArgb(22, 163, 74);
            this.btnexcel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnexcel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnexcel.ForeColor = System.Drawing.Color.White;
            this.btnexcel.Location = new System.Drawing.Point(1154, 15);
            this.btnexcel.Name = "btnexcel";
            this.btnexcel.Size = new System.Drawing.Size(110, 36);
            this.btnexcel.TabIndex = 5;
            this.btnexcel.Text = "Excel";
            this.btnexcel.UseVisualStyleBackColor = false;
            this.btnexcel.Click += new System.EventHandler(this.btnexcel_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.dataGridView1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 262);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(12);
            this.panel2.Size = new System.Drawing.Size(1280, 458);
            this.panel2.TabIndex = 3;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(15, 23, 42);
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeight = 34;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(219, 234, 254);
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(15, 23, 42);
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 28;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(1256, 434);
            this.dataGridView1.TabIndex = 0;
            // 
            // WarhouseReports
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(241, 245, 249);
            this.ClientSize = new System.Drawing.Size(1280, 720);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.toolbarPanel);
            this.Controls.Add(this.summaryPanel);
            this.Controls.Add(this.panelheader);
            this.MinimumSize = new System.Drawing.Size(1000, 640);
            this.Name = "WarhouseReports";
            this.Text = "Warehouse Reports";
            this.Load += new System.EventHandler(this.WarhouseReports_Load);
            this.panelheader.ResumeLayout(false);
            this.panelheader.PerformLayout();
            this.summaryPanel.ResumeLayout(false);
            this.cardCapital.ResumeLayout(false);
            this.cardCapital.PerformLayout();
            this.cardPurchase.ResumeLayout(false);
            this.cardPurchase.PerformLayout();
            this.cardItems.ResumeLayout(false);
            this.cardItems.PerformLayout();
            this.cardUnits.ResumeLayout(false);
            this.cardUnits.PerformLayout();
            this.cardLowStock.ResumeLayout(false);
            this.cardLowStock.PerformLayout();
            this.toolbarPanel.ResumeLayout(false);
            this.toolbarPanel.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelheader;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel summaryPanel;
        private System.Windows.Forms.Panel cardCapital;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbkematr2slmal;
        private System.Windows.Forms.Panel cardPurchase;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lbtaklefetshera2;
        private System.Windows.Forms.Panel cardItems;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lb3dadalsnef;
        private System.Windows.Forms.Panel cardUnits;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lbejmale;
        private System.Windows.Forms.Panel cardLowStock;
        private System.Windows.Forms.Label lblLowStockTitle;
        private System.Windows.Forms.Label lblLowStockValue;
        private System.Windows.Forms.Panel toolbarPanel;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblStockFilter;
        private System.Windows.Forms.ComboBox cmbStockFilter;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnexcel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dataGridView1;
    }
}
