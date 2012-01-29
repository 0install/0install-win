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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsDialog));
            this.buttonAdvanced = new System.Windows.Forms.Button();
            this.groupBoxSync = new System.Windows.Forms.GroupBox();
            this.buttonSyncReset = new System.Windows.Forms.Button();
            this.buttonSyncSetupWizard = new System.Windows.Forms.Button();
            this.linkSyncAccount = new System.Windows.Forms.LinkLabel();
            this.buttonSyncCryptoKey = new System.Windows.Forms.Button();
            this.textBoxSyncCryptoKey = new System.Windows.Forms.TextBox();
            this.labelSyncCryptoKey = new System.Windows.Forms.Label();
            this.textBoxSyncPassword = new System.Windows.Forms.TextBox();
            this.labelSyncPassword = new System.Windows.Forms.Label();
            this.textBoxSyncServer = new Common.Controls.UriTextBox();
            this.labelServer = new System.Windows.Forms.Label();
            this.textBoxSyncUsername = new System.Windows.Forms.TextBox();
            this.labelSyncUsername = new System.Windows.Forms.Label();
            this.groupBoxSync.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            // 
            // buttonAdvanced
            // 
            resources.ApplyResources(this.buttonAdvanced, "buttonAdvanced");
            this.buttonAdvanced.Name = "buttonAdvanced";
            this.buttonAdvanced.UseVisualStyleBackColor = true;
            this.buttonAdvanced.Click += new System.EventHandler(this.buttonAdvanced_Click);
            // 
            // groupBoxSync
            // 
            resources.ApplyResources(this.groupBoxSync, "groupBoxSync");
            this.groupBoxSync.Controls.Add(this.buttonSyncReset);
            this.groupBoxSync.Controls.Add(this.buttonSyncSetupWizard);
            this.groupBoxSync.Controls.Add(this.linkSyncAccount);
            this.groupBoxSync.Controls.Add(this.buttonSyncCryptoKey);
            this.groupBoxSync.Controls.Add(this.textBoxSyncCryptoKey);
            this.groupBoxSync.Controls.Add(this.labelSyncCryptoKey);
            this.groupBoxSync.Controls.Add(this.textBoxSyncPassword);
            this.groupBoxSync.Controls.Add(this.labelSyncPassword);
            this.groupBoxSync.Controls.Add(this.textBoxSyncServer);
            this.groupBoxSync.Controls.Add(this.labelServer);
            this.groupBoxSync.Controls.Add(this.textBoxSyncUsername);
            this.groupBoxSync.Controls.Add(this.labelSyncUsername);
            this.groupBoxSync.Name = "groupBoxSync";
            this.groupBoxSync.TabStop = false;
            // 
            // buttonSyncReset
            // 
            resources.ApplyResources(this.buttonSyncReset, "buttonSyncReset");
            this.buttonSyncReset.Name = "buttonSyncReset";
            this.buttonSyncReset.UseVisualStyleBackColor = true;
            this.buttonSyncReset.Click += new System.EventHandler(this.buttonSyncReset_Click);
            // 
            // buttonSyncSetupWizard
            // 
            resources.ApplyResources(this.buttonSyncSetupWizard, "buttonSyncSetupWizard");
            this.buttonSyncSetupWizard.Name = "buttonSyncSetupWizard";
            this.buttonSyncSetupWizard.UseVisualStyleBackColor = true;
            this.buttonSyncSetupWizard.Click += new System.EventHandler(this.buttonSyncSetupWizard_Click);
            // 
            // linkSyncAccount
            // 
            resources.ApplyResources(this.linkSyncAccount, "linkSyncAccount");
            this.linkSyncAccount.Name = "linkSyncAccount";
            this.linkSyncAccount.TabStop = true;
            this.linkSyncAccount.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkSyncAccount_LinkClicked);
            // 
            // buttonSyncCryptoKey
            // 
            resources.ApplyResources(this.buttonSyncCryptoKey, "buttonSyncCryptoKey");
            this.buttonSyncCryptoKey.Name = "buttonSyncCryptoKey";
            this.buttonSyncCryptoKey.UseVisualStyleBackColor = true;
            this.buttonSyncCryptoKey.Click += new System.EventHandler(this.buttonSyncCryptoKey_Click);
            // 
            // textBoxSyncCryptoKey
            // 
            resources.ApplyResources(this.textBoxSyncCryptoKey, "textBoxSyncCryptoKey");
            this.textBoxSyncCryptoKey.Name = "textBoxSyncCryptoKey";
            this.textBoxSyncCryptoKey.UseSystemPasswordChar = true;
            // 
            // labelSyncCryptoKey
            // 
            resources.ApplyResources(this.labelSyncCryptoKey, "labelSyncCryptoKey");
            this.labelSyncCryptoKey.Name = "labelSyncCryptoKey";
            // 
            // textBoxSyncPassword
            // 
            resources.ApplyResources(this.textBoxSyncPassword, "textBoxSyncPassword");
            this.textBoxSyncPassword.Name = "textBoxSyncPassword";
            this.textBoxSyncPassword.UseSystemPasswordChar = true;
            // 
            // labelSyncPassword
            // 
            resources.ApplyResources(this.labelSyncPassword, "labelSyncPassword");
            this.labelSyncPassword.Name = "labelSyncPassword";
            // 
            // textBoxSyncServer
            // 
            this.textBoxSyncServer.AllowDrop = true;
            resources.ApplyResources(this.textBoxSyncServer, "textBoxSyncServer");
            this.textBoxSyncServer.HttpOnly = true;
            this.textBoxSyncServer.Name = "textBoxSyncServer";
            // 
            // labelServer
            // 
            resources.ApplyResources(this.labelServer, "labelServer");
            this.labelServer.Name = "labelServer";
            // 
            // textBoxSyncUsername
            // 
            resources.ApplyResources(this.textBoxSyncUsername, "textBoxSyncUsername");
            this.textBoxSyncUsername.Name = "textBoxSyncUsername";
            // 
            // labelSyncUsername
            // 
            resources.ApplyResources(this.labelSyncUsername, "labelSyncUsername");
            this.labelSyncUsername.Name = "labelSyncUsername";
            // 
            // OptionsDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxSync);
            this.Controls.Add(this.buttonAdvanced);
            this.Name = "OptionsDialog";
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
        private System.Windows.Forms.Button buttonSyncCryptoKey;
        private System.Windows.Forms.TextBox textBoxSyncCryptoKey;
        private System.Windows.Forms.Label labelSyncCryptoKey;
        private System.Windows.Forms.TextBox textBoxSyncPassword;
        private System.Windows.Forms.Label labelSyncPassword;
        private System.Windows.Forms.TextBox textBoxSyncUsername;
        private System.Windows.Forms.Label labelSyncUsername;
        private System.Windows.Forms.LinkLabel linkSyncAccount;
        private System.Windows.Forms.Button buttonSyncReset;
        private System.Windows.Forms.Button buttonSyncSetupWizard;
        private Common.Controls.UriTextBox textBoxSyncServer;
        private System.Windows.Forms.Label labelServer;
    }
}