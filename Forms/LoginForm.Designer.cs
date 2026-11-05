namespace Pos_System
{
    partial class LoginForm
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
            this.panelLoginRoot = new System.Windows.Forms.Panel();
            this.panelLoginCard = new System.Windows.Forms.Panel();
            this.lblSignInHeading = new System.Windows.Forms.Label();
            this.lblSignInSub = new System.Windows.Forms.Label();
            this.panelUsername = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtusername = new System.Windows.Forms.TextBox();
            this.panelPassword = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtpassword = new System.Windows.Forms.TextBox();
            this.btnTogglePassword = new System.Windows.Forms.Button();
            this.lblError = new System.Windows.Forms.Label();
            this.btnlogin = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.panelLoginBrand = new System.Windows.Forms.Panel();
            this.lblBrandBody = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.panelLoginRoot.SuspendLayout();
            this.panelLoginCard.SuspendLayout();
            this.panelUsername.SuspendLayout();
            this.panelPassword.SuspendLayout();
            this.panelLoginBrand.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelLoginRoot
            // 
            this.panelLoginRoot.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.panelLoginRoot.Controls.Add(this.panelLoginCard);
            this.panelLoginRoot.Controls.Add(this.panelLoginBrand);
            this.panelLoginRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLoginRoot.Name = "panelLoginRoot";
            this.panelLoginRoot.Size = new System.Drawing.Size(900, 540);
            this.panelLoginRoot.TabIndex = 0;
            // 
            // panelLoginCard
            // 
            this.panelLoginCard.BackColor = System.Drawing.Color.White;
            this.panelLoginCard.Controls.Add(this.btnExit);
            this.panelLoginCard.Controls.Add(this.btnlogin);
            this.panelLoginCard.Controls.Add(this.lblError);
            this.panelLoginCard.Controls.Add(this.panelPassword);
            this.panelLoginCard.Controls.Add(this.panelUsername);
            this.panelLoginCard.Controls.Add(this.lblSignInSub);
            this.panelLoginCard.Controls.Add(this.lblSignInHeading);
            this.panelLoginCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLoginCard.Name = "panelLoginCard";
            this.panelLoginCard.Padding = new System.Windows.Forms.Padding(52, 64, 52, 36);
            this.panelLoginCard.TabIndex = 1;
            // 
            // lblSignInHeading
            // 
            this.lblSignInHeading.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSignInHeading.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblSignInHeading.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.lblSignInHeading.Name = "lblSignInHeading";
            this.lblSignInHeading.Size = new System.Drawing.Size(436, 46);
            this.lblSignInHeading.TabIndex = 0;
            this.lblSignInHeading.Text = "Sign in";
            // 
            // lblSignInSub
            // 
            this.lblSignInSub.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblSignInSub.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.lblSignInSub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblSignInSub.Name = "lblSignInSub";
            this.lblSignInSub.Size = new System.Drawing.Size(436, 36);
            this.lblSignInSub.TabIndex = 1;
            this.lblSignInSub.Text = "Use the account stored on this register.";
            // 
            // panelUsername
            // 
            this.panelUsername.Controls.Add(this.txtusername);
            this.panelUsername.Controls.Add(this.label1);
            this.panelUsername.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelUsername.Name = "panelUsername";
            this.panelUsername.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.panelUsername.Size = new System.Drawing.Size(436, 78);
            this.panelUsername.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(436, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "Username";
            // 
            // txtusername
            // 
            this.txtusername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtusername.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtusername.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtusername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtusername.Name = "txtusername";
            this.txtusername.Size = new System.Drawing.Size(436, 29);
            this.txtusername.TabIndex = 1;
            // 
            // panelPassword
            // 
            this.panelPassword.Controls.Add(this.txtpassword);
            this.panelPassword.Controls.Add(this.btnTogglePassword);
            this.panelPassword.Controls.Add(this.label2);
            this.panelPassword.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelPassword.Name = "panelPassword";
            this.panelPassword.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.panelPassword.Size = new System.Drawing.Size(436, 78);
            this.panelPassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(436, 22);
            this.label2.TabIndex = 0;
            this.label2.Text = "Password";
            // 
            // txtpassword
            // 
            this.txtpassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtpassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtpassword.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtpassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.txtpassword.Name = "txtpassword";
            this.txtpassword.PasswordChar = '*';
            this.txtpassword.TabIndex = 2;
            // 
            // btnTogglePassword
            // 
            this.btnTogglePassword.AccessibleName = "Show password";
            this.btnTogglePassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnTogglePassword.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTogglePassword.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnTogglePassword.FlatAppearance.BorderSize = 0;
            this.btnTogglePassword.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTogglePassword.ForeColor = System.Drawing.Color.White;
            this.btnTogglePassword.Name = "btnTogglePassword";
            this.btnTogglePassword.Size = new System.Drawing.Size(46, 62);
            this.btnTogglePassword.TabIndex = 5;
            this.btnTogglePassword.TabStop = false;
            this.btnTogglePassword.UseVisualStyleBackColor = false;
            this.btnTogglePassword.Click += new System.EventHandler(this.btnTogglePassword_Click);
            this.btnTogglePassword.Paint += new System.Windows.Forms.PaintEventHandler(this.btnTogglePassword_Paint);
            // 
            // lblError
            // 
            this.lblError.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblError.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblError.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(38)))), ((int)(((byte)(38)))));
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(436, 48);
            this.lblError.TabIndex = 4;
            this.lblError.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnlogin
            // 
            this.btnlogin.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnlogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnlogin.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnlogin.FlatAppearance.BorderSize = 0;
            this.btnlogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnlogin.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnlogin.ForeColor = System.Drawing.Color.White;
            this.btnlogin.Name = "btnlogin";
            this.btnlogin.Size = new System.Drawing.Size(436, 46);
            this.btnlogin.TabIndex = 3;
            this.btnlogin.Text = "Sign in";
            this.btnlogin.UseVisualStyleBackColor = false;
            this.btnlogin.Click += new System.EventHandler(this.btnlogin_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.btnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(436, 40);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // panelLoginBrand
            // 
            this.panelLoginBrand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(15)))), ((int)(((byte)(23)))), ((int)(((byte)(42)))));
            this.panelLoginBrand.Controls.Add(this.lblBrandBody);
            this.panelLoginBrand.Controls.Add(this.label3);
            this.panelLoginBrand.Controls.Add(this.label4);
            this.panelLoginBrand.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLoginBrand.Name = "panelLoginBrand";
            this.panelLoginBrand.Size = new System.Drawing.Size(360, 540);
            this.panelLoginBrand.TabIndex = 0;
            // 
            // lblBrandBody
            // 
            this.lblBrandBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblBrandBody.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblBrandBody.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.lblBrandBody.Name = "lblBrandBody";
            this.lblBrandBody.Padding = new System.Windows.Forms.Padding(32, 16, 32, 36);
            this.lblBrandBody.Text = "Sales, inventory, customers, and reports\r\nstay on this register.\r\n\r\nالمبيعات والمخزون والزبائن على هذا الصندوق.";
            this.lblBrandBody.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Top;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 30F, System.Drawing.FontStyle.Bold);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Name = "label3";
            this.label3.Padding = new System.Windows.Forms.Padding(32, 0, 32, 8);
            this.label3.Size = new System.Drawing.Size(360, 88);
            this.label3.Text = "Smart POS";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label4
            // 
            this.label4.Dock = System.Windows.Forms.DockStyle.Top;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(163)))), ((int)(((byte)(184)))));
            this.label4.Name = "label4";
            this.label4.Padding = new System.Windows.Forms.Padding(32, 28, 32, 0);
            this.label4.Size = new System.Drawing.Size(360, 56);
            this.label4.Text = "POINT OF SALE";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // LoginForm
            // 
            this.AcceptButton = this.btnlogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(245)))), ((int)(((byte)(249)))));
            this.CancelButton = this.btnExit;
            this.ClientSize = new System.Drawing.Size(900, 540);
            this.Controls.Add(this.panelLoginRoot);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Smart POS";
            this.panelLoginRoot.ResumeLayout(false);
            this.panelLoginCard.ResumeLayout(false);
            this.panelUsername.ResumeLayout(false);
            this.panelUsername.PerformLayout();
            this.panelPassword.ResumeLayout(false);
            this.panelPassword.PerformLayout();
            this.panelLoginBrand.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelLoginRoot;
        private System.Windows.Forms.Panel panelLoginBrand;
        private System.Windows.Forms.Panel panelLoginCard;
        private System.Windows.Forms.Panel panelUsername;
        private System.Windows.Forms.Panel panelPassword;
        private System.Windows.Forms.Label lblSignInHeading;
        private System.Windows.Forms.Label lblSignInSub;
        private System.Windows.Forms.Label lblBrandBody;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtusername;
        private System.Windows.Forms.TextBox txtpassword;
        private System.Windows.Forms.Button btnlogin;
        private System.Windows.Forms.Button btnTogglePassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Button btnExit;
    }
}
