namespace ZeroInstall.Central.WinForms
{
    partial class SyncWizard
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncWizard));
            this.wizardControl = new AeroWizard.WizardControl();
            this.pageSetupWelcome = new AeroWizard.WizardPage();
            this.buttonSetupSubsequent = new System.Windows.Forms.Button();
            this.buttonSetupFirst = new System.Windows.Forms.Button();
            this.labelSetup = new System.Windows.Forms.Label();
            this.labelSetupWelcome = new System.Windows.Forms.Label();
            this.pageServer = new AeroWizard.WizardPage();
            this.optionFileShare = new System.Windows.Forms.RadioButton();
            this.optionCustomServer = new System.Windows.Forms.RadioButton();
            this.optionOfficialServer = new System.Windows.Forms.RadioButton();
            this.labelServerType = new System.Windows.Forms.Label();
            this.labelServer = new System.Windows.Forms.Label();
            this.groupCustomServer = new System.Windows.Forms.GroupBox();
            this.textBoxCustomServer = new NanoByte.Common.Controls.UriTextBox();
            this.linkCustomServer = new System.Windows.Forms.LinkLabel();
            this.groupFileShare = new System.Windows.Forms.GroupBox();
            this.buttonFileShareBrowse = new System.Windows.Forms.Button();
            this.textBoxFileShare = new NanoByte.Common.Controls.HintTextBox();
            this.pageRegister = new AeroWizard.WizardPage();
            this.labelRegister2 = new System.Windows.Forms.Label();
            this.linkRegister = new System.Windows.Forms.LinkLabel();
            this.labelRegister = new System.Windows.Forms.Label();
            this.pageCredentials = new AeroWizard.WizardPage();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.labelCredentials = new System.Windows.Forms.Label();
            this.pageExistingCryptoKey = new AeroWizard.WizardPage();
            this.buttonForgotKey = new System.Windows.Forms.Button();
            this.textBoxCryptoKey = new System.Windows.Forms.TextBox();
            this.labelCryptoKey = new System.Windows.Forms.Label();
            this.labelExistingCryptoKey = new System.Windows.Forms.Label();
            this.pageResetCryptoKey = new AeroWizard.WizardPage();
            this.labelResetCryptoKey = new System.Windows.Forms.Label();
            this.textBoxCryptoKeyReset = new System.Windows.Forms.TextBox();
            this.labelCryptoKeyReset = new System.Windows.Forms.Label();
            this.pageCryptoKeyChanged = new AeroWizard.WizardPage();
            this.labelCryptoKeyChangedHint = new System.Windows.Forms.Label();
            this.labelCryptoKeyChanged = new System.Windows.Forms.Label();
            this.pageNewCryptoKey = new AeroWizard.WizardPage();
            this.labelNewCryptoKeyHint = new System.Windows.Forms.Label();
            this.textBoxCryptoKeyNew = new System.Windows.Forms.TextBox();
            this.labelCryptoKeyNew = new System.Windows.Forms.Label();
            this.labelNewCryptoKey = new System.Windows.Forms.Label();
            this.pageSetupFinished = new AeroWizard.WizardPage();
            this.labelSetupFinished = new System.Windows.Forms.Label();
            this.pageResetWelcome = new AeroWizard.WizardPage();
            this.buttonResetClient = new System.Windows.Forms.Button();
            this.buttonResetServer = new System.Windows.Forms.Button();
            this.buttonChangeCryptoKey = new System.Windows.Forms.Button();
            this.labelResetWelcome = new System.Windows.Forms.Label();
            this.pageChangeCryptoKey = new AeroWizard.WizardPage();
            this.labelChangeCryptoKey = new System.Windows.Forms.Label();
            this.textBoxCryptoKeyChange = new System.Windows.Forms.TextBox();
            this.labelCryptoKeyChange = new System.Windows.Forms.Label();
            this.pageResetServer = new AeroWizard.WizardPage();
            this.labelResetServer = new System.Windows.Forms.Label();
            this.pageResetServerFinished = new AeroWizard.WizardPage();
            this.labelResetServerFinishedHint = new System.Windows.Forms.Label();
            this.labelResetServerFinished = new System.Windows.Forms.Label();
            this.pageResetClient = new AeroWizard.WizardPage();
            this.labelResetClient = new System.Windows.Forms.Label();
            this.pageResetClientFinished = new AeroWizard.WizardPage();
            this.labelResetClientFinished = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl)).BeginInit();
            this.pageSetupWelcome.SuspendLayout();
            this.pageServer.SuspendLayout();
            this.groupCustomServer.SuspendLayout();
            this.groupFileShare.SuspendLayout();
            this.pageRegister.SuspendLayout();
            this.pageCredentials.SuspendLayout();
            this.pageExistingCryptoKey.SuspendLayout();
            this.pageResetCryptoKey.SuspendLayout();
            this.pageCryptoKeyChanged.SuspendLayout();
            this.pageNewCryptoKey.SuspendLayout();
            this.pageSetupFinished.SuspendLayout();
            this.pageResetWelcome.SuspendLayout();
            this.pageChangeCryptoKey.SuspendLayout();
            this.pageResetServer.SuspendLayout();
            this.pageResetServerFinished.SuspendLayout();
            this.pageResetClient.SuspendLayout();
            this.pageResetClientFinished.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardControl
            // 
            resources.ApplyResources(this.wizardControl, "wizardControl");
            this.wizardControl.Name = "wizardControl";
            this.wizardControl.Pages.Add(this.pageSetupWelcome);
            this.wizardControl.Pages.Add(this.pageServer);
            this.wizardControl.Pages.Add(this.pageRegister);
            this.wizardControl.Pages.Add(this.pageCredentials);
            this.wizardControl.Pages.Add(this.pageExistingCryptoKey);
            this.wizardControl.Pages.Add(this.pageResetCryptoKey);
            this.wizardControl.Pages.Add(this.pageCryptoKeyChanged);
            this.wizardControl.Pages.Add(this.pageNewCryptoKey);
            this.wizardControl.Pages.Add(this.pageSetupFinished);
            this.wizardControl.Pages.Add(this.pageResetWelcome);
            this.wizardControl.Pages.Add(this.pageChangeCryptoKey);
            this.wizardControl.Pages.Add(this.pageResetServer);
            this.wizardControl.Pages.Add(this.pageResetServerFinished);
            this.wizardControl.Pages.Add(this.pageResetClient);
            this.wizardControl.Pages.Add(this.pageResetClientFinished);
            // 
            // pageSetupWelcome
            // 
            this.pageSetupWelcome.AllowBack = false;
            this.pageSetupWelcome.Controls.Add(this.buttonSetupSubsequent);
            this.pageSetupWelcome.Controls.Add(this.buttonSetupFirst);
            this.pageSetupWelcome.Controls.Add(this.labelSetup);
            this.pageSetupWelcome.Controls.Add(this.labelSetupWelcome);
            this.pageSetupWelcome.Name = "pageSetupWelcome";
            this.pageSetupWelcome.NextPage = this.pageServer;
            this.pageSetupWelcome.ShowNext = false;
            resources.ApplyResources(this.pageSetupWelcome, "pageSetupWelcome");
            // 
            // buttonSetupSubsequent
            // 
            resources.ApplyResources(this.buttonSetupSubsequent, "buttonSetupSubsequent");
            this.buttonSetupSubsequent.Name = "buttonSetupSubsequent";
            this.buttonSetupSubsequent.UseVisualStyleBackColor = true;
            this.buttonSetupSubsequent.Click += new System.EventHandler(this.buttonSetupSubsequent_Click);
            // 
            // buttonSetupFirst
            // 
            resources.ApplyResources(this.buttonSetupFirst, "buttonSetupFirst");
            this.buttonSetupFirst.Name = "buttonSetupFirst";
            this.buttonSetupFirst.UseVisualStyleBackColor = true;
            this.buttonSetupFirst.Click += new System.EventHandler(this.buttonSetupFirst_Click);
            // 
            // labelSetup
            // 
            resources.ApplyResources(this.labelSetup, "labelSetup");
            this.labelSetup.Name = "labelSetup";
            // 
            // labelSetupWelcome
            // 
            resources.ApplyResources(this.labelSetupWelcome, "labelSetupWelcome");
            this.labelSetupWelcome.Name = "labelSetupWelcome";
            // 
            // pageServer
            // 
            this.pageServer.Controls.Add(this.optionFileShare);
            this.pageServer.Controls.Add(this.optionCustomServer);
            this.pageServer.Controls.Add(this.optionOfficialServer);
            this.pageServer.Controls.Add(this.labelServerType);
            this.pageServer.Controls.Add(this.labelServer);
            this.pageServer.Controls.Add(this.groupCustomServer);
            this.pageServer.Controls.Add(this.groupFileShare);
            this.pageServer.Name = "pageServer";
            resources.ApplyResources(this.pageServer, "pageServer");
            this.pageServer.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageServer_Commit);
            // 
            // optionFileShare
            // 
            resources.ApplyResources(this.optionFileShare, "optionFileShare");
            this.optionFileShare.Name = "optionFileShare";
            this.optionFileShare.UseVisualStyleBackColor = true;
            this.optionFileShare.CheckedChanged += new System.EventHandler(this.pageServer_InputChanged);
            // 
            // optionCustomServer
            // 
            resources.ApplyResources(this.optionCustomServer, "optionCustomServer");
            this.optionCustomServer.Name = "optionCustomServer";
            this.optionCustomServer.UseVisualStyleBackColor = true;
            this.optionCustomServer.CheckedChanged += new System.EventHandler(this.pageServer_InputChanged);
            // 
            // optionOfficialServer
            // 
            resources.ApplyResources(this.optionOfficialServer, "optionOfficialServer");
            this.optionOfficialServer.Checked = true;
            this.optionOfficialServer.Name = "optionOfficialServer";
            this.optionOfficialServer.TabStop = true;
            this.optionOfficialServer.UseVisualStyleBackColor = true;
            this.optionOfficialServer.CheckedChanged += new System.EventHandler(this.pageServer_InputChanged);
            // 
            // labelServerType
            // 
            resources.ApplyResources(this.labelServerType, "labelServerType");
            this.labelServerType.Name = "labelServerType";
            // 
            // labelServer
            // 
            resources.ApplyResources(this.labelServer, "labelServer");
            this.labelServer.Name = "labelServer";
            // 
            // groupCustomServer
            // 
            resources.ApplyResources(this.groupCustomServer, "groupCustomServer");
            this.groupCustomServer.Controls.Add(this.textBoxCustomServer);
            this.groupCustomServer.Controls.Add(this.linkCustomServer);
            this.groupCustomServer.Name = "groupCustomServer";
            this.groupCustomServer.TabStop = false;
            // 
            // textBoxCustomServer
            // 
            this.textBoxCustomServer.AllowDrop = true;
            resources.ApplyResources(this.textBoxCustomServer, "textBoxCustomServer");
            this.textBoxCustomServer.HttpOnly = true;
            this.textBoxCustomServer.Name = "textBoxCustomServer";
            this.textBoxCustomServer.TextChanged += new System.EventHandler(this.pageServer_InputChanged);
            // 
            // linkCustomServer
            // 
            resources.ApplyResources(this.linkCustomServer, "linkCustomServer");
            this.linkCustomServer.Name = "linkCustomServer";
            this.linkCustomServer.TabStop = true;
            this.linkCustomServer.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCustomServer_LinkClicked);
            // 
            // groupFileShare
            // 
            resources.ApplyResources(this.groupFileShare, "groupFileShare");
            this.groupFileShare.Controls.Add(this.buttonFileShareBrowse);
            this.groupFileShare.Controls.Add(this.textBoxFileShare);
            this.groupFileShare.Name = "groupFileShare";
            this.groupFileShare.TabStop = false;
            // 
            // buttonFileShareBrowse
            // 
            resources.ApplyResources(this.buttonFileShareBrowse, "buttonFileShareBrowse");
            this.buttonFileShareBrowse.Name = "buttonFileShareBrowse";
            this.buttonFileShareBrowse.UseVisualStyleBackColor = true;
            this.buttonFileShareBrowse.Click += new System.EventHandler(this.buttonFileShareBrowse_Click);
            // 
            // textBoxFileShare
            // 
            resources.ApplyResources(this.textBoxFileShare, "textBoxFileShare");
            this.textBoxFileShare.Name = "textBoxFileShare";
            this.textBoxFileShare.TextChanged += new System.EventHandler(this.pageServer_InputChanged);
            // 
            // pageRegister
            // 
            this.pageRegister.Controls.Add(this.labelRegister2);
            this.pageRegister.Controls.Add(this.linkRegister);
            this.pageRegister.Controls.Add(this.labelRegister);
            this.pageRegister.Name = "pageRegister";
            this.pageRegister.NextPage = this.pageCredentials;
            resources.ApplyResources(this.pageRegister, "pageRegister");
            // 
            // labelRegister2
            // 
            resources.ApplyResources(this.labelRegister2, "labelRegister2");
            this.labelRegister2.Name = "labelRegister2";
            // 
            // linkRegister
            // 
            resources.ApplyResources(this.linkRegister, "linkRegister");
            this.linkRegister.Name = "linkRegister";
            this.linkRegister.TabStop = true;
            this.linkRegister.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkRegister_LinkClicked);
            // 
            // labelRegister
            // 
            resources.ApplyResources(this.labelRegister, "labelRegister");
            this.labelRegister.Name = "labelRegister";
            // 
            // pageCredentials
            // 
            this.pageCredentials.AllowNext = false;
            this.pageCredentials.Controls.Add(this.textBoxPassword);
            this.pageCredentials.Controls.Add(this.labelPassword);
            this.pageCredentials.Controls.Add(this.textBoxUsername);
            this.pageCredentials.Controls.Add(this.labelUsername);
            this.pageCredentials.Controls.Add(this.labelCredentials);
            this.pageCredentials.Name = "pageCredentials";
            resources.ApplyResources(this.pageCredentials, "pageCredentials");
            this.pageCredentials.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageCredentials_Commit);
            // 
            // textBoxPassword
            // 
            resources.ApplyResources(this.textBoxPassword, "textBoxPassword");
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.UseSystemPasswordChar = true;
            this.textBoxPassword.TextChanged += new System.EventHandler(this.textBoxCredentials_TextChanged);
            // 
            // labelPassword
            // 
            resources.ApplyResources(this.labelPassword, "labelPassword");
            this.labelPassword.Name = "labelPassword";
            // 
            // textBoxUsername
            // 
            resources.ApplyResources(this.textBoxUsername, "textBoxUsername");
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.TextChanged += new System.EventHandler(this.textBoxCredentials_TextChanged);
            // 
            // labelUsername
            // 
            resources.ApplyResources(this.labelUsername, "labelUsername");
            this.labelUsername.Name = "labelUsername";
            // 
            // labelCredentials
            // 
            resources.ApplyResources(this.labelCredentials, "labelCredentials");
            this.labelCredentials.Name = "labelCredentials";
            // 
            // pageExistingCryptoKey
            // 
            this.pageExistingCryptoKey.AllowNext = false;
            this.pageExistingCryptoKey.Controls.Add(this.buttonForgotKey);
            this.pageExistingCryptoKey.Controls.Add(this.textBoxCryptoKey);
            this.pageExistingCryptoKey.Controls.Add(this.labelCryptoKey);
            this.pageExistingCryptoKey.Controls.Add(this.labelExistingCryptoKey);
            this.pageExistingCryptoKey.Name = "pageExistingCryptoKey";
            resources.ApplyResources(this.pageExistingCryptoKey, "pageExistingCryptoKey");
            this.pageExistingCryptoKey.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageExistingCryptoKey_Commit);
            this.pageExistingCryptoKey.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageExistingCryptoKey_Initialize);
            // 
            // buttonForgotKey
            // 
            resources.ApplyResources(this.buttonForgotKey, "buttonForgotKey");
            this.buttonForgotKey.Name = "buttonForgotKey";
            this.buttonForgotKey.UseVisualStyleBackColor = true;
            this.buttonForgotKey.Click += new System.EventHandler(this.buttonForgotKey_Click);
            // 
            // textBoxCryptoKey
            // 
            resources.ApplyResources(this.textBoxCryptoKey, "textBoxCryptoKey");
            this.textBoxCryptoKey.Name = "textBoxCryptoKey";
            this.textBoxCryptoKey.TextChanged += new System.EventHandler(this.textBoxCryptoKey_TextChanged);
            // 
            // labelCryptoKey
            // 
            resources.ApplyResources(this.labelCryptoKey, "labelCryptoKey");
            this.labelCryptoKey.Name = "labelCryptoKey";
            // 
            // labelExistingCryptoKey
            // 
            resources.ApplyResources(this.labelExistingCryptoKey, "labelExistingCryptoKey");
            this.labelExistingCryptoKey.Name = "labelExistingCryptoKey";
            // 
            // pageResetCryptoKey
            // 
            this.pageResetCryptoKey.AllowNext = false;
            this.pageResetCryptoKey.Controls.Add(this.labelResetCryptoKey);
            this.pageResetCryptoKey.Controls.Add(this.textBoxCryptoKeyReset);
            this.pageResetCryptoKey.Controls.Add(this.labelCryptoKeyReset);
            this.pageResetCryptoKey.Name = "pageResetCryptoKey";
            this.pageResetCryptoKey.NextPage = this.pageCryptoKeyChanged;
            resources.ApplyResources(this.pageResetCryptoKey, "pageResetCryptoKey");
            this.pageResetCryptoKey.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageResetCryptoKey_Commit);
            this.pageResetCryptoKey.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageResetCryptoKey_Initialize);
            // 
            // labelResetCryptoKey
            // 
            resources.ApplyResources(this.labelResetCryptoKey, "labelResetCryptoKey");
            this.labelResetCryptoKey.Name = "labelResetCryptoKey";
            // 
            // textBoxCryptoKeyReset
            // 
            resources.ApplyResources(this.textBoxCryptoKeyReset, "textBoxCryptoKeyReset");
            this.textBoxCryptoKeyReset.Name = "textBoxCryptoKeyReset";
            this.textBoxCryptoKeyReset.TextChanged += new System.EventHandler(this.textBoxCryptoKeyReset_TextChanged);
            // 
            // labelCryptoKeyReset
            // 
            resources.ApplyResources(this.labelCryptoKeyReset, "labelCryptoKeyReset");
            this.labelCryptoKeyReset.Name = "labelCryptoKeyReset";
            // 
            // pageCryptoKeyChanged
            // 
            this.pageCryptoKeyChanged.AllowBack = false;
            this.pageCryptoKeyChanged.Controls.Add(this.labelCryptoKeyChangedHint);
            this.pageCryptoKeyChanged.Controls.Add(this.labelCryptoKeyChanged);
            this.pageCryptoKeyChanged.Name = "pageCryptoKeyChanged";
            resources.ApplyResources(this.pageCryptoKeyChanged, "pageCryptoKeyChanged");
            this.pageCryptoKeyChanged.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageCryptoKeyChanged_Initialize);
            // 
            // labelCryptoKeyChangedHint
            // 
            resources.ApplyResources(this.labelCryptoKeyChangedHint, "labelCryptoKeyChangedHint");
            this.labelCryptoKeyChangedHint.Name = "labelCryptoKeyChangedHint";
            // 
            // labelCryptoKeyChanged
            // 
            resources.ApplyResources(this.labelCryptoKeyChanged, "labelCryptoKeyChanged");
            this.labelCryptoKeyChanged.Name = "labelCryptoKeyChanged";
            // 
            // pageNewCryptoKey
            // 
            this.pageNewCryptoKey.AllowNext = false;
            this.pageNewCryptoKey.Controls.Add(this.labelNewCryptoKeyHint);
            this.pageNewCryptoKey.Controls.Add(this.textBoxCryptoKeyNew);
            this.pageNewCryptoKey.Controls.Add(this.labelCryptoKeyNew);
            this.pageNewCryptoKey.Controls.Add(this.labelNewCryptoKey);
            this.pageNewCryptoKey.Name = "pageNewCryptoKey";
            this.pageNewCryptoKey.NextPage = this.pageSetupFinished;
            resources.ApplyResources(this.pageNewCryptoKey, "pageNewCryptoKey");
            this.pageNewCryptoKey.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageNewCryptoKey_Commit);
            this.pageNewCryptoKey.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageNewCryptoKey_Initialize);
            // 
            // labelNewCryptoKeyHint
            // 
            resources.ApplyResources(this.labelNewCryptoKeyHint, "labelNewCryptoKeyHint");
            this.labelNewCryptoKeyHint.Name = "labelNewCryptoKeyHint";
            // 
            // textBoxCryptoKeyNew
            // 
            resources.ApplyResources(this.textBoxCryptoKeyNew, "textBoxCryptoKeyNew");
            this.textBoxCryptoKeyNew.Name = "textBoxCryptoKeyNew";
            this.textBoxCryptoKeyNew.TextChanged += new System.EventHandler(this.textBoxCryptoKeyNew_TextChanged);
            // 
            // labelCryptoKeyNew
            // 
            resources.ApplyResources(this.labelCryptoKeyNew, "labelCryptoKeyNew");
            this.labelCryptoKeyNew.Name = "labelCryptoKeyNew";
            // 
            // labelNewCryptoKey
            // 
            resources.ApplyResources(this.labelNewCryptoKey, "labelNewCryptoKey");
            this.labelNewCryptoKey.Name = "labelNewCryptoKey";
            // 
            // pageSetupFinished
            // 
            this.pageSetupFinished.Controls.Add(this.labelSetupFinished);
            this.pageSetupFinished.IsFinishPage = true;
            this.pageSetupFinished.Name = "pageSetupFinished";
            resources.ApplyResources(this.pageSetupFinished, "pageSetupFinished");
            this.pageSetupFinished.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageSetupFinished_Commit);
            // 
            // labelSetupFinished
            // 
            resources.ApplyResources(this.labelSetupFinished, "labelSetupFinished");
            this.labelSetupFinished.Name = "labelSetupFinished";
            // 
            // pageResetWelcome
            // 
            this.pageResetWelcome.AllowBack = false;
            this.pageResetWelcome.Controls.Add(this.buttonResetClient);
            this.pageResetWelcome.Controls.Add(this.buttonResetServer);
            this.pageResetWelcome.Controls.Add(this.buttonChangeCryptoKey);
            this.pageResetWelcome.Controls.Add(this.labelResetWelcome);
            this.pageResetWelcome.Name = "pageResetWelcome";
            this.pageResetWelcome.ShowNext = false;
            resources.ApplyResources(this.pageResetWelcome, "pageResetWelcome");
            // 
            // buttonResetClient
            // 
            resources.ApplyResources(this.buttonResetClient, "buttonResetClient");
            this.buttonResetClient.Name = "buttonResetClient";
            this.buttonResetClient.UseVisualStyleBackColor = true;
            this.buttonResetClient.Click += new System.EventHandler(this.buttonResetClient_Click);
            // 
            // buttonResetServer
            // 
            resources.ApplyResources(this.buttonResetServer, "buttonResetServer");
            this.buttonResetServer.Name = "buttonResetServer";
            this.buttonResetServer.UseVisualStyleBackColor = true;
            this.buttonResetServer.Click += new System.EventHandler(this.buttonResetServer_Click);
            // 
            // buttonChangeCryptoKey
            // 
            resources.ApplyResources(this.buttonChangeCryptoKey, "buttonChangeCryptoKey");
            this.buttonChangeCryptoKey.Name = "buttonChangeCryptoKey";
            this.buttonChangeCryptoKey.UseVisualStyleBackColor = true;
            this.buttonChangeCryptoKey.Click += new System.EventHandler(this.buttonChangeCryptoKey_Click);
            // 
            // labelResetWelcome
            // 
            resources.ApplyResources(this.labelResetWelcome, "labelResetWelcome");
            this.labelResetWelcome.Name = "labelResetWelcome";
            // 
            // pageChangeCryptoKey
            // 
            this.pageChangeCryptoKey.AllowNext = false;
            this.pageChangeCryptoKey.Controls.Add(this.labelChangeCryptoKey);
            this.pageChangeCryptoKey.Controls.Add(this.textBoxCryptoKeyChange);
            this.pageChangeCryptoKey.Controls.Add(this.labelCryptoKeyChange);
            this.pageChangeCryptoKey.Name = "pageChangeCryptoKey";
            this.pageChangeCryptoKey.NextPage = this.pageCryptoKeyChanged;
            resources.ApplyResources(this.pageChangeCryptoKey, "pageChangeCryptoKey");
            this.pageChangeCryptoKey.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageChangeCryptoKey_Commit);
            this.pageChangeCryptoKey.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageChangeCryptoKey_Initialize);
            // 
            // labelChangeCryptoKey
            // 
            resources.ApplyResources(this.labelChangeCryptoKey, "labelChangeCryptoKey");
            this.labelChangeCryptoKey.Name = "labelChangeCryptoKey";
            // 
            // textBoxCryptoKeyChange
            // 
            resources.ApplyResources(this.textBoxCryptoKeyChange, "textBoxCryptoKeyChange");
            this.textBoxCryptoKeyChange.Name = "textBoxCryptoKeyChange";
            this.textBoxCryptoKeyChange.TextChanged += new System.EventHandler(this.textBoxCryptoKeyChange_TextChanged);
            // 
            // labelCryptoKeyChange
            // 
            resources.ApplyResources(this.labelCryptoKeyChange, "labelCryptoKeyChange");
            this.labelCryptoKeyChange.Name = "labelCryptoKeyChange";
            // 
            // pageResetServer
            // 
            this.pageResetServer.Controls.Add(this.labelResetServer);
            this.pageResetServer.Name = "pageResetServer";
            this.pageResetServer.NextPage = this.pageResetServerFinished;
            resources.ApplyResources(this.pageResetServer, "pageResetServer");
            this.pageResetServer.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageResetServer_Commit);
            // 
            // labelResetServer
            // 
            resources.ApplyResources(this.labelResetServer, "labelResetServer");
            this.labelResetServer.Name = "labelResetServer";
            // 
            // pageResetServerFinished
            // 
            this.pageResetServerFinished.AllowBack = false;
            this.pageResetServerFinished.Controls.Add(this.labelResetServerFinishedHint);
            this.pageResetServerFinished.Controls.Add(this.labelResetServerFinished);
            this.pageResetServerFinished.IsFinishPage = true;
            this.pageResetServerFinished.Name = "pageResetServerFinished";
            this.pageResetServerFinished.ShowCancel = false;
            resources.ApplyResources(this.pageResetServerFinished, "pageResetServerFinished");
            // 
            // labelResetServerFinishedHint
            // 
            resources.ApplyResources(this.labelResetServerFinishedHint, "labelResetServerFinishedHint");
            this.labelResetServerFinishedHint.Name = "labelResetServerFinishedHint";
            // 
            // labelResetServerFinished
            // 
            resources.ApplyResources(this.labelResetServerFinished, "labelResetServerFinished");
            this.labelResetServerFinished.Name = "labelResetServerFinished";
            // 
            // pageResetClient
            // 
            this.pageResetClient.Controls.Add(this.labelResetClient);
            this.pageResetClient.Name = "pageResetClient";
            this.pageResetClient.NextPage = this.pageResetClientFinished;
            resources.ApplyResources(this.pageResetClient, "pageResetClient");
            this.pageResetClient.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageResetClient_Commit);
            // 
            // labelResetClient
            // 
            resources.ApplyResources(this.labelResetClient, "labelResetClient");
            this.labelResetClient.Name = "labelResetClient";
            // 
            // pageResetClientFinished
            // 
            this.pageResetClientFinished.AllowBack = false;
            this.pageResetClientFinished.Controls.Add(this.labelResetClientFinished);
            this.pageResetClientFinished.IsFinishPage = true;
            this.pageResetClientFinished.Name = "pageResetClientFinished";
            this.pageResetClientFinished.ShowCancel = false;
            resources.ApplyResources(this.pageResetClientFinished, "pageResetClientFinished");
            // 
            // labelResetClientFinished
            // 
            resources.ApplyResources(this.labelResetClientFinished, "labelResetClientFinished");
            this.labelResetClientFinished.Name = "labelResetClientFinished";
            // 
            // SyncWizard
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.wizardControl);
            this.Name = "SyncWizard";
            this.ShowIcon = false;
            this.Load += new System.EventHandler(this.SyncWizard_Load);
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl)).EndInit();
            this.pageSetupWelcome.ResumeLayout(false);
            this.pageServer.ResumeLayout(false);
            this.pageServer.PerformLayout();
            this.groupCustomServer.ResumeLayout(false);
            this.groupCustomServer.PerformLayout();
            this.groupFileShare.ResumeLayout(false);
            this.groupFileShare.PerformLayout();
            this.pageRegister.ResumeLayout(false);
            this.pageRegister.PerformLayout();
            this.pageCredentials.ResumeLayout(false);
            this.pageCredentials.PerformLayout();
            this.pageExistingCryptoKey.ResumeLayout(false);
            this.pageExistingCryptoKey.PerformLayout();
            this.pageResetCryptoKey.ResumeLayout(false);
            this.pageResetCryptoKey.PerformLayout();
            this.pageCryptoKeyChanged.ResumeLayout(false);
            this.pageNewCryptoKey.ResumeLayout(false);
            this.pageNewCryptoKey.PerformLayout();
            this.pageSetupFinished.ResumeLayout(false);
            this.pageResetWelcome.ResumeLayout(false);
            this.pageChangeCryptoKey.ResumeLayout(false);
            this.pageChangeCryptoKey.PerformLayout();
            this.pageResetServer.ResumeLayout(false);
            this.pageResetServerFinished.ResumeLayout(false);
            this.pageResetClient.ResumeLayout(false);
            this.pageResetClientFinished.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private AeroWizard.WizardControl wizardControl;
        private AeroWizard.WizardPage pageSetupWelcome;
        private AeroWizard.WizardPage pageServer;
        private AeroWizard.WizardPage pageRegister;
        private AeroWizard.WizardPage pageCredentials;
        private AeroWizard.WizardPage pageExistingCryptoKey;
        private AeroWizard.WizardPage pageResetCryptoKey;
        private AeroWizard.WizardPage pageCryptoKeyChanged;
        private AeroWizard.WizardPage pageNewCryptoKey;
        private AeroWizard.WizardPage pageSetupFinished;
        private AeroWizard.WizardPage pageResetWelcome;
        private AeroWizard.WizardPage pageChangeCryptoKey;
        private AeroWizard.WizardPage pageResetServer;
        private AeroWizard.WizardPage pageResetServerFinished;
        private AeroWizard.WizardPage pageResetClient;
        private AeroWizard.WizardPage pageResetClientFinished;
        private System.Windows.Forms.Button buttonSetupSubsequent;
        private System.Windows.Forms.Button buttonSetupFirst;
        private System.Windows.Forms.Label labelSetup;
        private System.Windows.Forms.Label labelSetupWelcome;
        private NanoByte.Common.Controls.UriTextBox textBoxCustomServer;
        private System.Windows.Forms.RadioButton optionCustomServer;
        private System.Windows.Forms.RadioButton optionOfficialServer;
        private System.Windows.Forms.Label labelServerType;
        private System.Windows.Forms.Label labelServer;
        private System.Windows.Forms.Label labelRegister2;
        private System.Windows.Forms.LinkLabel linkRegister;
        private System.Windows.Forms.Label labelRegister;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.Label labelCredentials;
        private System.Windows.Forms.TextBox textBoxCryptoKey;
        private System.Windows.Forms.Label labelCryptoKey;
        private System.Windows.Forms.Label labelExistingCryptoKey;
        private System.Windows.Forms.Label labelResetCryptoKey;
        private System.Windows.Forms.TextBox textBoxCryptoKeyReset;
        private System.Windows.Forms.Label labelCryptoKeyReset;
        private System.Windows.Forms.Label labelCryptoKeyChangedHint;
        private System.Windows.Forms.Label labelCryptoKeyChanged;
        private System.Windows.Forms.Label labelNewCryptoKeyHint;
        private System.Windows.Forms.TextBox textBoxCryptoKeyNew;
        private System.Windows.Forms.Label labelCryptoKeyNew;
        private System.Windows.Forms.Label labelNewCryptoKey;
        private System.Windows.Forms.Label labelSetupFinished;
        private System.Windows.Forms.Button buttonResetClient;
        private System.Windows.Forms.Button buttonResetServer;
        private System.Windows.Forms.Button buttonChangeCryptoKey;
        private System.Windows.Forms.Label labelResetWelcome;
        private System.Windows.Forms.Label labelChangeCryptoKey;
        private System.Windows.Forms.TextBox textBoxCryptoKeyChange;
        private System.Windows.Forms.Label labelCryptoKeyChange;
        private System.Windows.Forms.Label labelResetServer;
        private System.Windows.Forms.Label labelResetServerFinishedHint;
        private System.Windows.Forms.Label labelResetServerFinished;
        private System.Windows.Forms.Label labelResetClient;
        private System.Windows.Forms.Label labelResetClientFinished;
        private System.Windows.Forms.Button buttonForgotKey;
        private System.Windows.Forms.LinkLabel linkCustomServer;
        private System.Windows.Forms.Button buttonFileShareBrowse;
        private NanoByte.Common.Controls.HintTextBox textBoxFileShare;
        private System.Windows.Forms.RadioButton optionFileShare;
        private System.Windows.Forms.GroupBox groupCustomServer;
        private System.Windows.Forms.GroupBox groupFileShare;
    }
}
