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
            this.buttonSyncReset = new System.Windows.Forms.Button();
            this.buttonSyncSetup = new System.Windows.Forms.Button();
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
            this.tabOptions = new System.Windows.Forms.TabControl();
            this.tabPageUpdates = new System.Windows.Forms.TabPage();
            this.checkBoxHelpWithTesting = new System.Windows.Forms.CheckBox();
            this.groupNetworkUse = new System.Windows.Forms.GroupBox();
            this.radioNetworkUseOffline = new System.Windows.Forms.RadioButton();
            this.radioNetworkUseMinimal = new System.Windows.Forms.RadioButton();
            this.radioNetworkUseFull = new System.Windows.Forms.RadioButton();
            this.tabPageStorage = new System.Windows.Forms.TabPage();
            this.groupImplDirs = new System.Windows.Forms.GroupBox();
            this.buttonRemoveImplDir = new System.Windows.Forms.Button();
            this.buttonGoToImplDir = new System.Windows.Forms.Button();
            this.buttonAddImplDir = new System.Windows.Forms.Button();
            this.listBoxImplDirs = new System.Windows.Forms.ListBox();
            this.tabPageCatalog = new System.Windows.Forms.TabPage();
            this.groupCatalogSources = new System.Windows.Forms.GroupBox();
            this.buttonResetCatalogSources = new System.Windows.Forms.Button();
            this.buttonRemoveCatalogSource = new System.Windows.Forms.Button();
            this.buttonGoToCatalogSource = new System.Windows.Forms.Button();
            this.buttonAddCatalogSource = new System.Windows.Forms.Button();
            this.listBoxCatalogSources = new System.Windows.Forms.ListBox();
            this.tabPageTrust = new System.Windows.Forms.TabPage();
            this.checkBoxAutoApproveKeys = new System.Windows.Forms.CheckBox();
            this.groupTrustedKeys = new System.Windows.Forms.GroupBox();
            this.panelTrustedKeys = new System.Windows.Forms.Panel();
            this.buttonRemoveTrustedKey = new System.Windows.Forms.Button();
            this.tabPageSync = new System.Windows.Forms.TabPage();
            this.implDirBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonApply = new System.Windows.Forms.Button();
            this.tabOptions.SuspendLayout();
            this.tabPageUpdates.SuspendLayout();
            this.groupNetworkUse.SuspendLayout();
            this.tabPageStorage.SuspendLayout();
            this.groupImplDirs.SuspendLayout();
            this.tabPageCatalog.SuspendLayout();
            this.groupCatalogSources.SuspendLayout();
            this.tabPageTrust.SuspendLayout();
            this.groupTrustedKeys.SuspendLayout();
            this.tabPageSync.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.Click += new System.EventHandler(this.buttonApplyOK_Click);
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
            // buttonSyncReset
            // 
            resources.ApplyResources(this.buttonSyncReset, "buttonSyncReset");
            this.buttonSyncReset.Name = "buttonSyncReset";
            this.buttonSyncReset.UseVisualStyleBackColor = true;
            this.buttonSyncReset.Click += new System.EventHandler(this.buttonSyncReset_Click);
            // 
            // buttonSyncSetup
            // 
            resources.ApplyResources(this.buttonSyncSetup, "buttonSyncSetup");
            this.buttonSyncSetup.Name = "buttonSyncSetup";
            this.buttonSyncSetup.UseVisualStyleBackColor = true;
            this.buttonSyncSetup.Click += new System.EventHandler(this.buttonSyncSetup_Click);
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
            this.textBoxSyncPassword.TextChanged += new System.EventHandler(this.textBoxSync_TextChanged);
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
            this.textBoxSyncServer.TextChanged += new System.EventHandler(this.textBoxSync_TextChanged);
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
            this.textBoxSyncUsername.TextChanged += new System.EventHandler(this.textBoxSync_TextChanged);
            // 
            // labelSyncUsername
            // 
            resources.ApplyResources(this.labelSyncUsername, "labelSyncUsername");
            this.labelSyncUsername.Name = "labelSyncUsername";
            // 
            // tabOptions
            // 
            resources.ApplyResources(this.tabOptions, "tabOptions");
            this.tabOptions.Controls.Add(this.tabPageUpdates);
            this.tabOptions.Controls.Add(this.tabPageStorage);
            this.tabOptions.Controls.Add(this.tabPageCatalog);
            this.tabOptions.Controls.Add(this.tabPageTrust);
            this.tabOptions.Controls.Add(this.tabPageSync);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.SelectedIndex = 0;
            // 
            // tabPageUpdates
            // 
            this.tabPageUpdates.Controls.Add(this.checkBoxHelpWithTesting);
            this.tabPageUpdates.Controls.Add(this.groupNetworkUse);
            resources.ApplyResources(this.tabPageUpdates, "tabPageUpdates");
            this.tabPageUpdates.Name = "tabPageUpdates";
            this.tabPageUpdates.UseVisualStyleBackColor = true;
            // 
            // checkBoxHelpWithTesting
            // 
            resources.ApplyResources(this.checkBoxHelpWithTesting, "checkBoxHelpWithTesting");
            this.checkBoxHelpWithTesting.Name = "checkBoxHelpWithTesting";
            this.checkBoxHelpWithTesting.UseVisualStyleBackColor = true;
            // 
            // groupNetworkUse
            // 
            resources.ApplyResources(this.groupNetworkUse, "groupNetworkUse");
            this.groupNetworkUse.Controls.Add(this.radioNetworkUseOffline);
            this.groupNetworkUse.Controls.Add(this.radioNetworkUseMinimal);
            this.groupNetworkUse.Controls.Add(this.radioNetworkUseFull);
            this.groupNetworkUse.Name = "groupNetworkUse";
            this.groupNetworkUse.TabStop = false;
            // 
            // radioNetworkUseOffline
            // 
            resources.ApplyResources(this.radioNetworkUseOffline, "radioNetworkUseOffline");
            this.radioNetworkUseOffline.Name = "radioNetworkUseOffline";
            this.radioNetworkUseOffline.TabStop = true;
            this.radioNetworkUseOffline.UseVisualStyleBackColor = true;
            // 
            // radioNetworkUseMinimal
            // 
            resources.ApplyResources(this.radioNetworkUseMinimal, "radioNetworkUseMinimal");
            this.radioNetworkUseMinimal.Name = "radioNetworkUseMinimal";
            this.radioNetworkUseMinimal.TabStop = true;
            this.radioNetworkUseMinimal.UseVisualStyleBackColor = true;
            // 
            // radioNetworkUseFull
            // 
            resources.ApplyResources(this.radioNetworkUseFull, "radioNetworkUseFull");
            this.radioNetworkUseFull.Name = "radioNetworkUseFull";
            this.radioNetworkUseFull.TabStop = true;
            this.radioNetworkUseFull.UseVisualStyleBackColor = true;
            // 
            // tabPageStorage
            // 
            this.tabPageStorage.Controls.Add(this.groupImplDirs);
            resources.ApplyResources(this.tabPageStorage, "tabPageStorage");
            this.tabPageStorage.Name = "tabPageStorage";
            this.tabPageStorage.UseVisualStyleBackColor = true;
            // 
            // groupImplDirs
            // 
            resources.ApplyResources(this.groupImplDirs, "groupImplDirs");
            this.groupImplDirs.Controls.Add(this.buttonRemoveImplDir);
            this.groupImplDirs.Controls.Add(this.buttonGoToImplDir);
            this.groupImplDirs.Controls.Add(this.buttonAddImplDir);
            this.groupImplDirs.Controls.Add(this.listBoxImplDirs);
            this.groupImplDirs.Name = "groupImplDirs";
            this.groupImplDirs.TabStop = false;
            // 
            // buttonRemoveImplDir
            // 
            resources.ApplyResources(this.buttonRemoveImplDir, "buttonRemoveImplDir");
            this.buttonRemoveImplDir.Name = "buttonRemoveImplDir";
            this.buttonRemoveImplDir.UseVisualStyleBackColor = true;
            this.buttonRemoveImplDir.Click += new System.EventHandler(this.buttonRemoveImplDir_Click);
            // 
            // buttonGoToImplDir
            // 
            resources.ApplyResources(this.buttonGoToImplDir, "buttonGoToImplDir");
            this.buttonGoToImplDir.Name = "buttonGoToImplDir";
            this.buttonGoToImplDir.UseVisualStyleBackColor = true;
            this.buttonGoToImplDir.Click += new System.EventHandler(this.buttonGoToImplDir_Click);
            // 
            // buttonAddImplDir
            // 
            resources.ApplyResources(this.buttonAddImplDir, "buttonAddImplDir");
            this.buttonAddImplDir.Name = "buttonAddImplDir";
            this.buttonAddImplDir.UseVisualStyleBackColor = true;
            this.buttonAddImplDir.Click += new System.EventHandler(this.buttonAddImplDir_Click);
            // 
            // listBoxImplDirs
            // 
            this.listBoxImplDirs.AllowDrop = true;
            resources.ApplyResources(this.listBoxImplDirs, "listBoxImplDirs");
            this.listBoxImplDirs.DisplayMember = "Source";
            this.listBoxImplDirs.FormattingEnabled = true;
            this.listBoxImplDirs.Name = "listBoxImplDirs";
            this.listBoxImplDirs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxImplDirs.ValueMember = "Source";
            this.listBoxImplDirs.SelectedIndexChanged += new System.EventHandler(this.listBoxImplDirs_SelectedIndexChanged);
            // 
            // tabPageCatalog
            // 
            this.tabPageCatalog.Controls.Add(this.groupCatalogSources);
            resources.ApplyResources(this.tabPageCatalog, "tabPageCatalog");
            this.tabPageCatalog.Name = "tabPageCatalog";
            this.tabPageCatalog.UseVisualStyleBackColor = true;
            // 
            // groupCatalogSources
            // 
            resources.ApplyResources(this.groupCatalogSources, "groupCatalogSources");
            this.groupCatalogSources.Controls.Add(this.buttonResetCatalogSources);
            this.groupCatalogSources.Controls.Add(this.buttonRemoveCatalogSource);
            this.groupCatalogSources.Controls.Add(this.buttonGoToCatalogSource);
            this.groupCatalogSources.Controls.Add(this.buttonAddCatalogSource);
            this.groupCatalogSources.Controls.Add(this.listBoxCatalogSources);
            this.groupCatalogSources.Name = "groupCatalogSources";
            this.groupCatalogSources.TabStop = false;
            // 
            // buttonResetCatalogSources
            // 
            resources.ApplyResources(this.buttonResetCatalogSources, "buttonResetCatalogSources");
            this.buttonResetCatalogSources.Name = "buttonResetCatalogSources";
            this.buttonResetCatalogSources.UseVisualStyleBackColor = true;
            this.buttonResetCatalogSources.Click += new System.EventHandler(this.buttonResetCatalogSources_Click);
            // 
            // buttonRemoveCatalogSource
            // 
            resources.ApplyResources(this.buttonRemoveCatalogSource, "buttonRemoveCatalogSource");
            this.buttonRemoveCatalogSource.Name = "buttonRemoveCatalogSource";
            this.buttonRemoveCatalogSource.UseVisualStyleBackColor = true;
            this.buttonRemoveCatalogSource.Click += new System.EventHandler(this.buttonRemoveCatalogSource_Click);
            // 
            // buttonGoToCatalogSource
            // 
            resources.ApplyResources(this.buttonGoToCatalogSource, "buttonGoToCatalogSource");
            this.buttonGoToCatalogSource.Name = "buttonGoToCatalogSource";
            this.buttonGoToCatalogSource.UseVisualStyleBackColor = true;
            this.buttonGoToCatalogSource.Click += new System.EventHandler(this.buttonGoToCatalogSource_Click);
            // 
            // buttonAddCatalogSource
            // 
            resources.ApplyResources(this.buttonAddCatalogSource, "buttonAddCatalogSource");
            this.buttonAddCatalogSource.Name = "buttonAddCatalogSource";
            this.buttonAddCatalogSource.UseVisualStyleBackColor = true;
            this.buttonAddCatalogSource.Click += new System.EventHandler(this.buttonAddCatalogSource_Click);
            // 
            // listBoxCatalogSources
            // 
            this.listBoxCatalogSources.AllowDrop = true;
            resources.ApplyResources(this.listBoxCatalogSources, "listBoxCatalogSources");
            this.listBoxCatalogSources.DisplayMember = "Source";
            this.listBoxCatalogSources.FormattingEnabled = true;
            this.listBoxCatalogSources.Name = "listBoxCatalogSources";
            this.listBoxCatalogSources.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxCatalogSources.ValueMember = "Source";
            this.listBoxCatalogSources.SelectedIndexChanged += new System.EventHandler(this.listBoxCatalogSources_SelectedIndexChanged);
            // 
            // tabPageTrust
            // 
            this.tabPageTrust.Controls.Add(this.checkBoxAutoApproveKeys);
            this.tabPageTrust.Controls.Add(this.groupTrustedKeys);
            resources.ApplyResources(this.tabPageTrust, "tabPageTrust");
            this.tabPageTrust.Name = "tabPageTrust";
            this.tabPageTrust.UseVisualStyleBackColor = true;
            // 
            // checkBoxAutoApproveKeys
            // 
            resources.ApplyResources(this.checkBoxAutoApproveKeys, "checkBoxAutoApproveKeys");
            this.checkBoxAutoApproveKeys.Name = "checkBoxAutoApproveKeys";
            this.checkBoxAutoApproveKeys.UseVisualStyleBackColor = true;
            // 
            // groupTrustedKeys
            // 
            resources.ApplyResources(this.groupTrustedKeys, "groupTrustedKeys");
            this.groupTrustedKeys.Controls.Add(this.panelTrustedKeys);
            this.groupTrustedKeys.Controls.Add(this.buttonRemoveTrustedKey);
            this.groupTrustedKeys.Name = "groupTrustedKeys";
            this.groupTrustedKeys.TabStop = false;
            // 
            // panelTrustedKeys
            // 
            resources.ApplyResources(this.panelTrustedKeys, "panelTrustedKeys");
            this.panelTrustedKeys.Name = "panelTrustedKeys";
            // 
            // buttonRemoveTrustedKey
            // 
            resources.ApplyResources(this.buttonRemoveTrustedKey, "buttonRemoveTrustedKey");
            this.buttonRemoveTrustedKey.Name = "buttonRemoveTrustedKey";
            this.buttonRemoveTrustedKey.UseVisualStyleBackColor = true;
            this.buttonRemoveTrustedKey.Click += new System.EventHandler(this.buttonRemoveTrustedKey_Click);
            // 
            // tabPageSync
            // 
            this.tabPageSync.Controls.Add(this.buttonSyncReset);
            this.tabPageSync.Controls.Add(this.textBoxSyncServer);
            this.tabPageSync.Controls.Add(this.buttonSyncSetup);
            this.tabPageSync.Controls.Add(this.labelSyncUsername);
            this.tabPageSync.Controls.Add(this.linkSyncAccount);
            this.tabPageSync.Controls.Add(this.textBoxSyncUsername);
            this.tabPageSync.Controls.Add(this.buttonSyncCryptoKey);
            this.tabPageSync.Controls.Add(this.labelServer);
            this.tabPageSync.Controls.Add(this.textBoxSyncCryptoKey);
            this.tabPageSync.Controls.Add(this.labelSyncPassword);
            this.tabPageSync.Controls.Add(this.labelSyncCryptoKey);
            this.tabPageSync.Controls.Add(this.textBoxSyncPassword);
            resources.ApplyResources(this.tabPageSync, "tabPageSync");
            this.tabPageSync.Name = "tabPageSync";
            this.tabPageSync.UseVisualStyleBackColor = true;
            // 
            // implDirBrowserDialog
            // 
            resources.ApplyResources(this.implDirBrowserDialog, "implDirBrowserDialog");
            this.implDirBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // buttonApply
            // 
            resources.ApplyResources(this.buttonApply, "buttonApply");
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApplyOK_Click);
            // 
            // OptionsDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonAdvanced);
            this.Controls.Add(this.tabOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "OptionsDialog";
            this.Load += new System.EventHandler(this.OptionsDialog_Load);
            this.Controls.SetChildIndex(this.tabOptions, 0);
            this.Controls.SetChildIndex(this.buttonAdvanced, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.buttonApply, 0);
            this.tabOptions.ResumeLayout(false);
            this.tabPageUpdates.ResumeLayout(false);
            this.tabPageUpdates.PerformLayout();
            this.groupNetworkUse.ResumeLayout(false);
            this.groupNetworkUse.PerformLayout();
            this.tabPageStorage.ResumeLayout(false);
            this.groupImplDirs.ResumeLayout(false);
            this.tabPageCatalog.ResumeLayout(false);
            this.groupCatalogSources.ResumeLayout(false);
            this.tabPageTrust.ResumeLayout(false);
            this.tabPageTrust.PerformLayout();
            this.groupTrustedKeys.ResumeLayout(false);
            this.tabPageSync.ResumeLayout(false);
            this.tabPageSync.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonAdvanced;
        private System.Windows.Forms.Button buttonSyncCryptoKey;
        private System.Windows.Forms.TextBox textBoxSyncCryptoKey;
        private System.Windows.Forms.Label labelSyncCryptoKey;
        private System.Windows.Forms.TextBox textBoxSyncPassword;
        private System.Windows.Forms.Label labelSyncPassword;
        private System.Windows.Forms.TextBox textBoxSyncUsername;
        private System.Windows.Forms.Label labelSyncUsername;
        private System.Windows.Forms.LinkLabel linkSyncAccount;
        private System.Windows.Forms.Button buttonSyncReset;
        private System.Windows.Forms.Button buttonSyncSetup;
        private Common.Controls.UriTextBox textBoxSyncServer;
        private System.Windows.Forms.Label labelServer;
        private System.Windows.Forms.TabControl tabOptions;
        private System.Windows.Forms.TabPage tabPageUpdates;
        private System.Windows.Forms.TabPage tabPageStorage;
        private System.Windows.Forms.TabPage tabPageCatalog;
        private System.Windows.Forms.TabPage tabPageTrust;
        private System.Windows.Forms.TabPage tabPageSync;
        private System.Windows.Forms.GroupBox groupNetworkUse;
        private System.Windows.Forms.RadioButton radioNetworkUseOffline;
        private System.Windows.Forms.RadioButton radioNetworkUseMinimal;
        private System.Windows.Forms.RadioButton radioNetworkUseFull;
        private System.Windows.Forms.CheckBox checkBoxHelpWithTesting;
        private System.Windows.Forms.CheckBox checkBoxAutoApproveKeys;
        private System.Windows.Forms.GroupBox groupTrustedKeys;
        private System.Windows.Forms.GroupBox groupImplDirs;
        private System.Windows.Forms.ListBox listBoxImplDirs;
        private System.Windows.Forms.Button buttonRemoveImplDir;
        private System.Windows.Forms.Button buttonAddImplDir;
        private System.Windows.Forms.Button buttonRemoveTrustedKey;
        private System.Windows.Forms.FolderBrowserDialog implDirBrowserDialog;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Panel panelTrustedKeys;
        private System.Windows.Forms.Button buttonGoToImplDir;
        private System.Windows.Forms.GroupBox groupCatalogSources;
        private System.Windows.Forms.Button buttonRemoveCatalogSource;
        private System.Windows.Forms.Button buttonGoToCatalogSource;
        private System.Windows.Forms.Button buttonAddCatalogSource;
        private System.Windows.Forms.ListBox listBoxCatalogSources;
        private System.Windows.Forms.Button buttonResetCatalogSources;
    }
}