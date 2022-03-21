using NanoByte.Common.Controls;

namespace ZeroInstall.Commands.WinForms
{
    partial class ConfigDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigDialog));
            this.linkSyncAccount = new System.Windows.Forms.LinkLabel();
            this.buttonSyncCryptoKey = new System.Windows.Forms.Button();
            this.textBoxSyncCryptoKey = new System.Windows.Forms.TextBox();
            this.labelSyncCryptoKey = new System.Windows.Forms.Label();
            this.textBoxSyncPassword = new System.Windows.Forms.TextBox();
            this.labelSyncPassword = new System.Windows.Forms.Label();
            this.textBoxSyncServer = new NanoByte.Common.Controls.UriTextBox();
            this.labelServer = new System.Windows.Forms.Label();
            this.textBoxSyncUsername = new System.Windows.Forms.TextBox();
            this.labelSyncUsername = new System.Windows.Forms.Label();
            this.tabOptions = new System.Windows.Forms.TabControl();
            this.tabPageUpdates = new System.Windows.Forms.TabPage();
            this.groupStability = new System.Windows.Forms.GroupBox();
            this.checkBoxHelpWithTesting = new System.Windows.Forms.CheckBox();
            this.groupFreshness = new System.Windows.Forms.GroupBox();
            this.timespanFreshness = new NanoByte.Common.Controls.TimeSpanControl();
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
            this.tabPageLanguage = new System.Windows.Forms.TabPage();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.buttonAdvancedShow = new System.Windows.Forms.Button();
            this.labelAdvancedWarning = new System.Windows.Forms.Label();
            this.propertyGridAdvanced = new NanoByte.Common.Controls.ResettablePropertyGrid();
            this.implDirBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.tabOptions.SuspendLayout();
            this.tabPageUpdates.SuspendLayout();
            this.groupStability.SuspendLayout();
            this.groupFreshness.SuspendLayout();
            this.groupNetworkUse.SuspendLayout();
            this.tabPageStorage.SuspendLayout();
            this.groupImplDirs.SuspendLayout();
            this.tabPageCatalog.SuspendLayout();
            this.groupCatalogSources.SuspendLayout();
            this.tabPageTrust.SuspendLayout();
            this.groupTrustedKeys.SuspendLayout();
            this.tabPageSync.SuspendLayout();
            this.tabPageLanguage.SuspendLayout();
            this.tabPageAdvanced.SuspendLayout();
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
            this.textBoxSyncCryptoKey.TextChanged += new System.EventHandler(this.textBoxSyncCryptoKey_TextChanged);
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
            this.textBoxSyncPassword.TextChanged += new System.EventHandler(this.textBoxSyncPassword_TextChanged);
            // 
            // labelSyncPassword
            // 
            resources.ApplyResources(this.labelSyncPassword, "labelSyncPassword");
            this.labelSyncPassword.Name = "labelSyncPassword";
            // 
            // textBoxSyncServer
            // 
            resources.ApplyResources(this.textBoxSyncServer, "textBoxSyncServer");
            this.textBoxSyncServer.Name = "textBoxSyncServer";
            this.textBoxSyncServer.TextChanged += new System.EventHandler(this.textBoxSyncServer_TextChanged);
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
            this.textBoxSyncUsername.TextChanged += new System.EventHandler(this.textBoxSyncUsername_TextChanged);
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
            this.tabOptions.Controls.Add(this.tabPageLanguage);
            this.tabOptions.Controls.Add(this.tabPageAdvanced);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.SelectedIndex = 0;
            // 
            // tabPageUpdates
            // 
            this.tabPageUpdates.Controls.Add(this.groupStability);
            this.tabPageUpdates.Controls.Add(this.groupFreshness);
            this.tabPageUpdates.Controls.Add(this.groupNetworkUse);
            resources.ApplyResources(this.tabPageUpdates, "tabPageUpdates");
            this.tabPageUpdates.Name = "tabPageUpdates";
            this.tabPageUpdates.UseVisualStyleBackColor = true;
            // 
            // groupStability
            // 
            resources.ApplyResources(this.groupStability, "groupStability");
            this.groupStability.Controls.Add(this.checkBoxHelpWithTesting);
            this.groupStability.Name = "groupStability";
            this.groupStability.TabStop = false;
            // 
            // checkBoxHelpWithTesting
            // 
            resources.ApplyResources(this.checkBoxHelpWithTesting, "checkBoxHelpWithTesting");
            this.checkBoxHelpWithTesting.Name = "checkBoxHelpWithTesting";
            this.checkBoxHelpWithTesting.UseVisualStyleBackColor = true;
            this.checkBoxHelpWithTesting.CheckedChanged += new System.EventHandler(this.checkBoxHelpWithTesting_CheckedChanged);
            // 
            // groupFreshness
            // 
            resources.ApplyResources(this.groupFreshness, "groupFreshness");
            this.groupFreshness.Controls.Add(this.timespanFreshness);
            this.groupFreshness.Name = "groupFreshness";
            this.groupFreshness.TabStop = false;
            // 
            // timespanFreshness
            // 
            resources.ApplyResources(this.timespanFreshness, "timespanFreshness");
            this.timespanFreshness.Name = "timespanFreshness";
            this.timespanFreshness.Value = System.TimeSpan.Parse("00:00:00");
            this.timespanFreshness.Validated += new System.EventHandler(this.timespanFreshness_Validated);
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
            this.radioNetworkUseOffline.CheckedChanged += new System.EventHandler(this.radioNetworkUse_CheckedChanged);
            // 
            // radioNetworkUseMinimal
            // 
            resources.ApplyResources(this.radioNetworkUseMinimal, "radioNetworkUseMinimal");
            this.radioNetworkUseMinimal.Name = "radioNetworkUseMinimal";
            this.radioNetworkUseMinimal.TabStop = true;
            this.radioNetworkUseMinimal.UseVisualStyleBackColor = true;
            this.radioNetworkUseMinimal.CheckedChanged += new System.EventHandler(this.radioNetworkUse_CheckedChanged);
            // 
            // radioNetworkUseFull
            // 
            resources.ApplyResources(this.radioNetworkUseFull, "radioNetworkUseFull");
            this.radioNetworkUseFull.Name = "radioNetworkUseFull";
            this.radioNetworkUseFull.TabStop = true;
            this.radioNetworkUseFull.UseVisualStyleBackColor = true;
            this.radioNetworkUseFull.CheckedChanged += new System.EventHandler(this.radioNetworkUse_CheckedChanged);
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
            this.checkBoxAutoApproveKeys.CheckedChanged += new System.EventHandler(this.checkBoxAutoApproveKeys_CheckedChanged);
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
            this.tabPageSync.Controls.Add(this.textBoxSyncServer);
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
            // tabPageLanguage
            // 
            this.tabPageLanguage.Controls.Add(this.labelLanguage);
            this.tabPageLanguage.Controls.Add(this.comboBoxLanguage);
            resources.ApplyResources(this.tabPageLanguage, "tabPageLanguage");
            this.tabPageLanguage.Name = "tabPageLanguage";
            this.tabPageLanguage.UseVisualStyleBackColor = true;
            // 
            // labelLanguage
            // 
            resources.ApplyResources(this.labelLanguage, "labelLanguage");
            this.labelLanguage.Name = "labelLanguage";
            // 
            // comboBoxLanguage
            // 
            resources.ApplyResources(this.comboBoxLanguage, "comboBoxLanguage");
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.Controls.Add(this.buttonAdvancedShow);
            this.tabPageAdvanced.Controls.Add(this.labelAdvancedWarning);
            this.tabPageAdvanced.Controls.Add(this.propertyGridAdvanced);
            resources.ApplyResources(this.tabPageAdvanced, "tabPageAdvanced");
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // buttonAdvancedShow
            // 
            resources.ApplyResources(this.buttonAdvancedShow, "buttonAdvancedShow");
            this.buttonAdvancedShow.Name = "buttonAdvancedShow";
            this.buttonAdvancedShow.UseVisualStyleBackColor = true;
            this.buttonAdvancedShow.Click += new System.EventHandler(this.buttonAdvancedShow_Click);
            // 
            // labelAdvancedWarning
            // 
            resources.ApplyResources(this.labelAdvancedWarning, "labelAdvancedWarning");
            this.labelAdvancedWarning.Name = "labelAdvancedWarning";
            // 
            // propertyGridAdvanced
            // 
            resources.ApplyResources(this.propertyGridAdvanced, "propertyGridAdvanced");
            this.propertyGridAdvanced.Name = "propertyGridAdvanced";
            this.propertyGridAdvanced.ToolbarVisible = false;
            this.propertyGridAdvanced.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridAdvanced_PropertyValueChanged);
            // 
            // implDirBrowserDialog
            // 
            resources.ApplyResources(this.implDirBrowserDialog, "implDirBrowserDialog");
            this.implDirBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // ConfigDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabOptions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.Name = "ConfigDialog";
            this.ShowInTaskbar = true;
            this.Controls.SetChildIndex(this.tabOptions, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabOptions.ResumeLayout(false);
            this.tabPageUpdates.ResumeLayout(false);
            this.groupStability.ResumeLayout(false);
            this.groupStability.PerformLayout();
            this.groupFreshness.ResumeLayout(false);
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
            this.tabPageLanguage.ResumeLayout(false);
            this.tabPageAdvanced.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button buttonAddCatalogSource;
        private System.Windows.Forms.Button buttonAddImplDir;
        private System.Windows.Forms.Button buttonAdvancedShow;
        private System.Windows.Forms.Button buttonGoToCatalogSource;
        private System.Windows.Forms.Button buttonGoToImplDir;
        private System.Windows.Forms.Button buttonRemoveCatalogSource;
        private System.Windows.Forms.Button buttonRemoveImplDir;
        private System.Windows.Forms.Button buttonRemoveTrustedKey;
        private System.Windows.Forms.Button buttonResetCatalogSources;
        private System.Windows.Forms.Button buttonSyncCryptoKey;
        private System.Windows.Forms.CheckBox checkBoxAutoApproveKeys;
        private System.Windows.Forms.CheckBox checkBoxHelpWithTesting;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.GroupBox groupCatalogSources;
        private System.Windows.Forms.GroupBox groupFreshness;
        private System.Windows.Forms.GroupBox groupImplDirs;
        private System.Windows.Forms.GroupBox groupNetworkUse;
        private System.Windows.Forms.GroupBox groupStability;
        private System.Windows.Forms.GroupBox groupTrustedKeys;
        private System.Windows.Forms.FolderBrowserDialog implDirBrowserDialog;
        private System.Windows.Forms.Label labelAdvancedWarning;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.Label labelServer;
        private System.Windows.Forms.Label labelSyncCryptoKey;
        private System.Windows.Forms.Label labelSyncPassword;
        private System.Windows.Forms.Label labelSyncUsername;
        private System.Windows.Forms.LinkLabel linkSyncAccount;
        private System.Windows.Forms.ListBox listBoxCatalogSources;
        private System.Windows.Forms.ListBox listBoxImplDirs;
        private System.Windows.Forms.Panel panelTrustedKeys;
        private NanoByte.Common.Controls.ResettablePropertyGrid propertyGridAdvanced;
        private System.Windows.Forms.RadioButton radioNetworkUseFull;
        private System.Windows.Forms.RadioButton radioNetworkUseMinimal;
        private System.Windows.Forms.RadioButton radioNetworkUseOffline;
        private System.Windows.Forms.TabControl tabOptions;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        private System.Windows.Forms.TabPage tabPageCatalog;
        private System.Windows.Forms.TabPage tabPageLanguage;
        private System.Windows.Forms.TabPage tabPageStorage;
        private System.Windows.Forms.TabPage tabPageSync;
        private System.Windows.Forms.TabPage tabPageTrust;
        private System.Windows.Forms.TabPage tabPageUpdates;
        private System.Windows.Forms.TextBox textBoxSyncCryptoKey;
        private System.Windows.Forms.TextBox textBoxSyncPassword;
        private NanoByte.Common.Controls.UriTextBox textBoxSyncServer;
        private System.Windows.Forms.TextBox textBoxSyncUsername;
        private NanoByte.Common.Controls.TimeSpanControl timespanFreshness;
        #endregion
    }
}
