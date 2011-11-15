namespace ZeroInstall.Central.WinForms
{
    partial class OptionsDialog
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
            this.buttonAdvanced = new System.Windows.Forms.Button();
            this.groupBoxSync = new System.Windows.Forms.GroupBox();
            this.linkSyncAccount = new System.Windows.Forms.LinkLabel();
            this.linkSyncRegister = new System.Windows.Forms.LinkLabel();
            this.buttonSyncCryptoKey = new System.Windows.Forms.Button();
            this.textBoxSyncCryptoKey = new System.Windows.Forms.TextBox();
            this.labelSyncCryptoKey = new System.Windows.Forms.Label();
            this.textBoxSyncPassword = new System.Windows.Forms.TextBox();
            this.labelSyncPassword = new System.Windows.Forms.Label();
            this.textBoxSyncUsername = new System.Windows.Forms.TextBox();
            this.labelSyncUsername = new System.Windows.Forms.Label();
            this.groupBoxSync.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(93, 150);
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(174, 150);
            // 
            // buttonAdvanced
            // 
            this.buttonAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAdvanced.Location = new System.Drawing.Point(12, 150);
            this.buttonAdvanced.Name = "buttonAdvanced";
            this.buttonAdvanced.Size = new System.Drawing.Size(75, 23);
            this.buttonAdvanced.TabIndex = 1;
            this.buttonAdvanced.Text = "&Advanced";
            this.buttonAdvanced.UseVisualStyleBackColor = true;
            this.buttonAdvanced.Click += new System.EventHandler(this.buttonAdvanced_Click);
            // 
            // groupBoxSync
            // 
            this.groupBoxSync.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSync.Controls.Add(this.linkSyncAccount);
            this.groupBoxSync.Controls.Add(this.linkSyncRegister);
            this.groupBoxSync.Controls.Add(this.buttonSyncCryptoKey);
            this.groupBoxSync.Controls.Add(this.textBoxSyncCryptoKey);
            this.groupBoxSync.Controls.Add(this.labelSyncCryptoKey);
            this.groupBoxSync.Controls.Add(this.textBoxSyncPassword);
            this.groupBoxSync.Controls.Add(this.labelSyncPassword);
            this.groupBoxSync.Controls.Add(this.textBoxSyncUsername);
            this.groupBoxSync.Controls.Add(this.labelSyncUsername);
            this.groupBoxSync.Location = new System.Drawing.Point(12, 12);
            this.groupBoxSync.Name = "groupBoxSync";
            this.groupBoxSync.Size = new System.Drawing.Size(237, 122);
            this.groupBoxSync.TabIndex = 0;
            this.groupBoxSync.TabStop = false;
            this.groupBoxSync.Text = "Synchronization";
            // 
            // linkSyncAccount
            // 
            this.linkSyncAccount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkSyncAccount.AutoSize = true;
            this.linkSyncAccount.Location = new System.Drawing.Point(143, 68);
            this.linkSyncAccount.Name = "linkSyncAccount";
            this.linkSyncAccount.Size = new System.Drawing.Size(88, 13);
            this.linkSyncAccount.TabIndex = 6;
            this.linkSyncAccount.TabStop = true;
            this.linkSyncAccount.Text = "Manage account";
            this.linkSyncAccount.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkSyncAccount_LinkClicked);
            // 
            // linkSyncRegister
            // 
            this.linkSyncRegister.AutoSize = true;
            this.linkSyncRegister.Location = new System.Drawing.Point(69, 68);
            this.linkSyncRegister.Name = "linkSyncRegister";
            this.linkSyncRegister.Size = new System.Drawing.Size(46, 13);
            this.linkSyncRegister.TabIndex = 5;
            this.linkSyncRegister.TabStop = true;
            this.linkSyncRegister.Text = "Register";
            this.linkSyncRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkSyncRegister_LinkClicked);
            // 
            // buttonSyncCryptoKey
            // 
            this.buttonSyncCryptoKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSyncCryptoKey.Location = new System.Drawing.Point(212, 93);
            this.buttonSyncCryptoKey.Name = "buttonSyncCryptoKey";
            this.buttonSyncCryptoKey.Size = new System.Drawing.Size(19, 20);
            this.buttonSyncCryptoKey.TabIndex = 9;
            this.buttonSyncCryptoKey.Text = "?";
            this.buttonSyncCryptoKey.UseVisualStyleBackColor = true;
            this.buttonSyncCryptoKey.Click += new System.EventHandler(this.buttonSyncCryptoKey_Click);
            // 
            // textBoxSyncCryptoKey
            // 
            this.textBoxSyncCryptoKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSyncCryptoKey.Location = new System.Drawing.Point(72, 93);
            this.textBoxSyncCryptoKey.Name = "textBoxSyncCryptoKey";
            this.textBoxSyncCryptoKey.Size = new System.Drawing.Size(134, 20);
            this.textBoxSyncCryptoKey.TabIndex = 8;
            this.textBoxSyncCryptoKey.UseSystemPasswordChar = true;
            // 
            // labelSyncCryptoKey
            // 
            this.labelSyncCryptoKey.AutoSize = true;
            this.labelSyncCryptoKey.Location = new System.Drawing.Point(6, 96);
            this.labelSyncCryptoKey.Name = "labelSyncCryptoKey";
            this.labelSyncCryptoKey.Size = new System.Drawing.Size(60, 13);
            this.labelSyncCryptoKey.TabIndex = 7;
            this.labelSyncCryptoKey.Text = "&Crypto key:";
            // 
            // textBoxSyncPassword
            // 
            this.textBoxSyncPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSyncPassword.Location = new System.Drawing.Point(72, 45);
            this.textBoxSyncPassword.Name = "textBoxSyncPassword";
            this.textBoxSyncPassword.Size = new System.Drawing.Size(159, 20);
            this.textBoxSyncPassword.TabIndex = 3;
            this.textBoxSyncPassword.UseSystemPasswordChar = true;
            // 
            // labelSyncPassword
            // 
            this.labelSyncPassword.AutoSize = true;
            this.labelSyncPassword.Location = new System.Drawing.Point(6, 48);
            this.labelSyncPassword.Name = "labelSyncPassword";
            this.labelSyncPassword.Size = new System.Drawing.Size(56, 13);
            this.labelSyncPassword.TabIndex = 2;
            this.labelSyncPassword.Text = "&Password:";
            // 
            // textBoxSyncUsername
            // 
            this.textBoxSyncUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSyncUsername.Location = new System.Drawing.Point(72, 19);
            this.textBoxSyncUsername.Name = "textBoxSyncUsername";
            this.textBoxSyncUsername.Size = new System.Drawing.Size(159, 20);
            this.textBoxSyncUsername.TabIndex = 1;
            // 
            // labelSyncUsername
            // 
            this.labelSyncUsername.AutoSize = true;
            this.labelSyncUsername.Location = new System.Drawing.Point(6, 22);
            this.labelSyncUsername.Name = "labelSyncUsername";
            this.labelSyncUsername.Size = new System.Drawing.Size(58, 13);
            this.labelSyncUsername.TabIndex = 0;
            this.labelSyncUsername.Text = "&Username:";
            // 
            // OptionsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 185);
            this.Controls.Add(this.groupBoxSync);
            this.Controls.Add(this.buttonAdvanced);
            this.Name = "OptionsDialog";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsDialog_Load);
            this.Controls.SetChildIndex(this.buttonAdvanced, 0);
            this.Controls.SetChildIndex(this.groupBoxSync, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.groupBoxSync.ResumeLayout(false);
            this.groupBoxSync.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonAdvanced;
        private System.Windows.Forms.GroupBox groupBoxSync;
        private System.Windows.Forms.LinkLabel linkSyncRegister;
        private System.Windows.Forms.Button buttonSyncCryptoKey;
        private System.Windows.Forms.TextBox textBoxSyncCryptoKey;
        private System.Windows.Forms.Label labelSyncCryptoKey;
        private System.Windows.Forms.TextBox textBoxSyncPassword;
        private System.Windows.Forms.Label labelSyncPassword;
        private System.Windows.Forms.TextBox textBoxSyncUsername;
        private System.Windows.Forms.Label labelSyncUsername;
        private System.Windows.Forms.LinkLabel linkSyncAccount;
    }
}