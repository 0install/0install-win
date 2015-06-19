namespace ZeroInstall.Publish.WinForms
{
    partial class NewFeedWizard
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
            if (disposing)
            {
                if (components != null) components.Dispose();
                if (_feedBuilder != null) _feedBuilder.Dispose();
                if (_installerCapture != null) _installerCapture.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewFeedWizard));
            this.wizardControl = new AeroWizard.WizardControl();
            this.downloadPage = new AeroWizard.WizardPage();
            this.checkLocalCopy = new System.Windows.Forms.CheckBox();
            this.groupLocalCopy = new System.Windows.Forms.GroupBox();
            this.buttonSelectLocalPath = new System.Windows.Forms.Button();
            this.textBoxLocalPath = new NanoByte.Common.Controls.HintTextBox();
            this.textBoxDownloadUrl = new NanoByte.Common.Controls.UriTextBox();
            this.labelDownloadUrl = new System.Windows.Forms.Label();
            this.archiveExtractPage = new AeroWizard.WizardPage();
            this.listBoxExtract = new System.Windows.Forms.ListBox();
            this.labelExtract = new System.Windows.Forms.Label();
            this.entryPointPage = new AeroWizard.WizardPage();
            this.listBoxEntryPoint = new System.Windows.Forms.ListBox();
            this.labelEntyPoint = new System.Windows.Forms.Label();
            this.detailsPage = new AeroWizard.WizardPage();
            this.labelDetails = new System.Windows.Forms.Label();
            this.propertyGridCandidate = new NanoByte.Common.Controls.ResettablePropertyGrid();
            this.iconPage = new AeroWizard.WizardPage();
            this.textBoxHrefPng = new NanoByte.Common.Controls.UriTextBox();
            this.labelIconStep4 = new System.Windows.Forms.Label();
            this.buttonSavePng = new System.Windows.Forms.Button();
            this.labelIconStep3 = new System.Windows.Forms.Label();
            this.textBoxHrefIco = new NanoByte.Common.Controls.UriTextBox();
            this.labelIconStep2 = new System.Windows.Forms.Label();
            this.buttonSaveIco = new System.Windows.Forms.Button();
            this.labelIconStep1 = new System.Windows.Forms.Label();
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.labelIcon = new System.Windows.Forms.Label();
            this.securityPage = new AeroWizard.WizardPage();
            this.buttonNewKey = new System.Windows.Forms.Button();
            this.comboBoxKeys = new System.Windows.Forms.ComboBox();
            this.labelInfoSignature = new System.Windows.Forms.Label();
            this.labelInterfaceUri = new System.Windows.Forms.Label();
            this.textBoxInterfaceUri = new NanoByte.Common.Controls.UriTextBox();
            this.donePage = new AeroWizard.WizardPage();
            this.labelDone2 = new System.Windows.Forms.Label();
            this.labelDone = new System.Windows.Forms.Label();
            this.installerCaptureStartPage = new AeroWizard.WizardPage();
            this.buttonSkipCapture = new System.Windows.Forms.Button();
            this.labelCaptureStart2 = new System.Windows.Forms.Label();
            this.labelCaptureStart = new System.Windows.Forms.Label();
            this.installerCaptureDiffPage = new AeroWizard.WizardPage();
            this.groupInstallationDir = new System.Windows.Forms.GroupBox();
            this.buttonSelectInstallationDir = new System.Windows.Forms.Button();
            this.textBoxInstallationDir = new NanoByte.Common.Controls.HintTextBox();
            this.labelCaptureDiff = new System.Windows.Forms.Label();
            this.installerCollectFilesPage = new AeroWizard.WizardPage();
            this.buttonExistingArchive = new System.Windows.Forms.Button();
            this.buttonCreateArchive = new System.Windows.Forms.Button();
            this.groupCreateArchive = new System.Windows.Forms.GroupBox();
            this.labelLocalPathArchive = new System.Windows.Forms.Label();
            this.labelUploadUrl = new System.Windows.Forms.Label();
            this.textBoxUploadUrl = new NanoByte.Common.Controls.UriTextBox();
            this.buttonSelectArchivePath = new System.Windows.Forms.Button();
            this.textBoxArchivePath = new NanoByte.Common.Controls.HintTextBox();
            this.installerAltDownloadPage = new AeroWizard.WizardPage();
            this.checkAltLocalCopy = new System.Windows.Forms.CheckBox();
            this.groupAltLocalCopy = new System.Windows.Forms.GroupBox();
            this.buttonSelectAltLocalPath = new System.Windows.Forms.Button();
            this.textBoxAltLocalPath = new NanoByte.Common.Controls.HintTextBox();
            this.textBoxAltDownloadUrl = new NanoByte.Common.Controls.UriTextBox();
            this.labelAltDownloadUrl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl)).BeginInit();
            this.downloadPage.SuspendLayout();
            this.groupLocalCopy.SuspendLayout();
            this.archiveExtractPage.SuspendLayout();
            this.entryPointPage.SuspendLayout();
            this.detailsPage.SuspendLayout();
            this.iconPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.securityPage.SuspendLayout();
            this.donePage.SuspendLayout();
            this.installerCaptureStartPage.SuspendLayout();
            this.installerCaptureDiffPage.SuspendLayout();
            this.groupInstallationDir.SuspendLayout();
            this.installerCollectFilesPage.SuspendLayout();
            this.groupCreateArchive.SuspendLayout();
            this.installerAltDownloadPage.SuspendLayout();
            this.groupAltLocalCopy.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardControl
            // 
            this.wizardControl.Location = new System.Drawing.Point(0, 0);
            this.wizardControl.Name = "wizardControl";
            this.wizardControl.Pages.Add(this.downloadPage);
            this.wizardControl.Pages.Add(this.archiveExtractPage);
            this.wizardControl.Pages.Add(this.installerCaptureStartPage);
            this.wizardControl.Pages.Add(this.installerCaptureDiffPage);
            this.wizardControl.Pages.Add(this.installerCollectFilesPage);
            this.wizardControl.Pages.Add(this.installerAltDownloadPage);
            this.wizardControl.Pages.Add(this.entryPointPage);
            this.wizardControl.Pages.Add(this.detailsPage);
            this.wizardControl.Pages.Add(this.iconPage);
            this.wizardControl.Pages.Add(this.securityPage);
            this.wizardControl.Pages.Add(this.donePage);
            this.wizardControl.Size = new System.Drawing.Size(574, 415);
            this.wizardControl.TabIndex = 0;
            this.wizardControl.Title = "New Feed";
            // 
            // downloadPage
            // 
            this.downloadPage.AllowNext = false;
            this.downloadPage.Controls.Add(this.checkLocalCopy);
            this.downloadPage.Controls.Add(this.groupLocalCopy);
            this.downloadPage.Controls.Add(this.textBoxDownloadUrl);
            this.downloadPage.Controls.Add(this.labelDownloadUrl);
            this.downloadPage.Name = "downloadPage";
            this.downloadPage.Size = new System.Drawing.Size(527, 262);
            this.downloadPage.TabIndex = 0;
            this.downloadPage.Text = "Download";
            this.downloadPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.downloadPage_Commit);
            // 
            // checkLocalCopy
            // 
            this.checkLocalCopy.AutoSize = true;
            this.checkLocalCopy.Location = new System.Drawing.Point(18, 124);
            this.checkLocalCopy.Name = "checkLocalCopy";
            this.checkLocalCopy.Size = new System.Drawing.Size(178, 19);
            this.checkLocalCopy.TabIndex = 2;
            this.checkLocalCopy.Text = "I have a &local copy of this file";
            this.checkLocalCopy.UseVisualStyleBackColor = true;
            this.checkLocalCopy.CheckedChanged += new System.EventHandler(this.downloadPage_ToggleControls);
            // 
            // groupLocalCopy
            // 
            this.groupLocalCopy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupLocalCopy.Controls.Add(this.buttonSelectLocalPath);
            this.groupLocalCopy.Controls.Add(this.textBoxLocalPath);
            this.groupLocalCopy.Enabled = false;
            this.groupLocalCopy.Location = new System.Drawing.Point(7, 123);
            this.groupLocalCopy.Name = "groupLocalCopy";
            this.groupLocalCopy.Size = new System.Drawing.Size(474, 65);
            this.groupLocalCopy.TabIndex = 3;
            this.groupLocalCopy.TabStop = false;
            // 
            // buttonSelectLocalPath
            // 
            this.buttonSelectLocalPath.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonSelectLocalPath.Location = new System.Drawing.Point(438, 26);
            this.buttonSelectLocalPath.Name = "buttonSelectLocalPath";
            this.buttonSelectLocalPath.Size = new System.Drawing.Size(29, 23);
            this.buttonSelectLocalPath.TabIndex = 1;
            this.buttonSelectLocalPath.Text = "...";
            this.buttonSelectLocalPath.UseVisualStyleBackColor = true;
            this.buttonSelectLocalPath.Click += new System.EventHandler(this.buttonSelectLocalPath_Click);
            // 
            // textBoxLocalPath
            // 
            this.textBoxLocalPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLocalPath.HintText = "File path";
            this.textBoxLocalPath.Location = new System.Drawing.Point(11, 26);
            this.textBoxLocalPath.Name = "textBoxLocalPath";
            this.textBoxLocalPath.Size = new System.Drawing.Size(421, 23);
            this.textBoxLocalPath.TabIndex = 0;
            this.textBoxLocalPath.TextChanged += new System.EventHandler(this.downloadPage_ToggleControls);
            // 
            // textBoxDownloadUrl
            // 
            this.textBoxDownloadUrl.AllowDrop = true;
            this.textBoxDownloadUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDownloadUrl.ForeColor = System.Drawing.Color.Red;
            this.textBoxDownloadUrl.HintText = "HTTP/FTP URL";
            this.textBoxDownloadUrl.Location = new System.Drawing.Point(7, 67);
            this.textBoxDownloadUrl.Name = "textBoxDownloadUrl";
            this.textBoxDownloadUrl.Size = new System.Drawing.Size(474, 23);
            this.textBoxDownloadUrl.TabIndex = 1;
            this.textBoxDownloadUrl.TextChanged += new System.EventHandler(this.downloadPage_ToggleControls);
            // 
            // labelDownloadUrl
            // 
            this.labelDownloadUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDownloadUrl.Location = new System.Drawing.Point(4, 9);
            this.labelDownloadUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDownloadUrl.Name = "labelDownloadUrl";
            this.labelDownloadUrl.Size = new System.Drawing.Size(477, 55);
            this.labelDownloadUrl.TabIndex = 0;
            this.labelDownloadUrl.Text = "Where can the current version of the application be downloaded? (.zip, .tar.gz, ." +
    "msi, .exe, .jar, ...)";
            // 
            // archiveExtractPage
            // 
            this.archiveExtractPage.Controls.Add(this.listBoxExtract);
            this.archiveExtractPage.Controls.Add(this.labelExtract);
            this.archiveExtractPage.Name = "archiveExtractPage";
            this.archiveExtractPage.NextPage = this.entryPointPage;
            this.archiveExtractPage.Size = new System.Drawing.Size(527, 262);
            this.archiveExtractPage.TabIndex = 1;
            this.archiveExtractPage.Text = "Archive";
            this.archiveExtractPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.archiveExtractPage_Commit);
            this.archiveExtractPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.archiveExtractPage_Initialize);
            // 
            // listBoxExtract
            // 
            this.listBoxExtract.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxExtract.FormattingEnabled = true;
            this.listBoxExtract.ItemHeight = 15;
            this.listBoxExtract.Location = new System.Drawing.Point(7, 67);
            this.listBoxExtract.Name = "listBoxExtract";
            this.listBoxExtract.Size = new System.Drawing.Size(474, 169);
            this.listBoxExtract.TabIndex = 1;
            // 
            // labelExtract
            // 
            this.labelExtract.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelExtract.Location = new System.Drawing.Point(4, 9);
            this.labelExtract.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelExtract.Name = "labelExtract";
            this.labelExtract.Size = new System.Drawing.Size(477, 55);
            this.labelExtract.TabIndex = 0;
            this.labelExtract.Text = "What is the top-level directory of the application in the archive? (Just leave th" +
    "e default if you are not sure.)";
            // 
            // entryPointPage
            // 
            this.entryPointPage.Controls.Add(this.listBoxEntryPoint);
            this.entryPointPage.Controls.Add(this.labelEntyPoint);
            this.entryPointPage.Name = "entryPointPage";
            this.entryPointPage.NextPage = this.detailsPage;
            this.entryPointPage.Size = new System.Drawing.Size(527, 262);
            this.entryPointPage.TabIndex = 2;
            this.entryPointPage.Text = "Entry point";
            this.entryPointPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.entryPointPage_Commit);
            this.entryPointPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.entryPointPage_Initialize);
            // 
            // listBoxEntryPoint
            // 
            this.listBoxEntryPoint.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxEntryPoint.FormattingEnabled = true;
            this.listBoxEntryPoint.ItemHeight = 15;
            this.listBoxEntryPoint.Location = new System.Drawing.Point(7, 47);
            this.listBoxEntryPoint.Name = "listBoxEntryPoint";
            this.listBoxEntryPoint.Size = new System.Drawing.Size(474, 184);
            this.listBoxEntryPoint.TabIndex = 1;
            // 
            // labelEntyPoint
            // 
            this.labelEntyPoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEntyPoint.Location = new System.Drawing.Point(4, 9);
            this.labelEntyPoint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelEntyPoint.Name = "labelEntyPoint";
            this.labelEntyPoint.Size = new System.Drawing.Size(477, 35);
            this.labelEntyPoint.TabIndex = 0;
            this.labelEntyPoint.Text = "Which file starts the main application?";
            // 
            // detailsPage
            // 
            this.detailsPage.Controls.Add(this.labelDetails);
            this.detailsPage.Controls.Add(this.propertyGridCandidate);
            this.detailsPage.Name = "detailsPage";
            this.detailsPage.NextPage = this.iconPage;
            this.detailsPage.Size = new System.Drawing.Size(527, 262);
            this.detailsPage.TabIndex = 6;
            this.detailsPage.Text = "Details";
            this.detailsPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.detailsPage_Commit);
            this.detailsPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.detailsPage_Initialize);
            // 
            // labelDetails
            // 
            this.labelDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDetails.Location = new System.Drawing.Point(4, 9);
            this.labelDetails.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelDetails.Name = "labelDetails";
            this.labelDetails.Size = new System.Drawing.Size(477, 19);
            this.labelDetails.TabIndex = 0;
            this.labelDetails.Text = "Please fill in the missing details.";
            // 
            // propertyGridCandidate
            // 
            this.propertyGridCandidate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGridCandidate.Location = new System.Drawing.Point(7, 31);
            this.propertyGridCandidate.Name = "propertyGridCandidate";
            this.propertyGridCandidate.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGridCandidate.Size = new System.Drawing.Size(474, 214);
            this.propertyGridCandidate.TabIndex = 1;
            this.propertyGridCandidate.ToolbarVisible = false;
            // 
            // iconPage
            // 
            this.iconPage.Controls.Add(this.textBoxHrefPng);
            this.iconPage.Controls.Add(this.labelIconStep4);
            this.iconPage.Controls.Add(this.buttonSavePng);
            this.iconPage.Controls.Add(this.labelIconStep3);
            this.iconPage.Controls.Add(this.textBoxHrefIco);
            this.iconPage.Controls.Add(this.labelIconStep2);
            this.iconPage.Controls.Add(this.buttonSaveIco);
            this.iconPage.Controls.Add(this.labelIconStep1);
            this.iconPage.Controls.Add(this.pictureBoxIcon);
            this.iconPage.Controls.Add(this.labelIcon);
            this.iconPage.Name = "iconPage";
            this.iconPage.NextPage = this.securityPage;
            this.iconPage.Size = new System.Drawing.Size(527, 262);
            this.iconPage.TabIndex = 7;
            this.iconPage.Text = "Icon";
            this.iconPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.iconPage_Commit);
            this.iconPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.iconPage_Initialize);
            // 
            // textBoxHrefPng
            // 
            this.textBoxHrefPng.AllowDrop = true;
            this.textBoxHrefPng.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHrefPng.HintText = "HTTP Address";
            this.textBoxHrefPng.HttpOnly = true;
            this.textBoxHrefPng.Location = new System.Drawing.Point(171, 148);
            this.textBoxHrefPng.Name = "textBoxHrefPng";
            this.textBoxHrefPng.Size = new System.Drawing.Size(310, 23);
            this.textBoxHrefPng.TabIndex = 29;
            // 
            // labelIconStep4
            // 
            this.labelIconStep4.AutoSize = true;
            this.labelIconStep4.Location = new System.Drawing.Point(7, 151);
            this.labelIconStep4.Name = "labelIconStep4";
            this.labelIconStep4.Size = new System.Drawing.Size(158, 15);
            this.labelIconStep4.TabIndex = 8;
            this.labelIconStep4.Text = "4.   Where will you upload it?";
            // 
            // buttonSavePng
            // 
            this.buttonSavePng.Location = new System.Drawing.Point(28, 120);
            this.buttonSavePng.Name = "buttonSavePng";
            this.buttonSavePng.Size = new System.Drawing.Size(137, 23);
            this.buttonSavePng.TabIndex = 27;
            this.buttonSavePng.Text = "Extract as &PNG file";
            this.buttonSavePng.UseVisualStyleBackColor = true;
            this.buttonSavePng.Click += new System.EventHandler(this.buttonSavePng_Click);
            // 
            // labelIconStep3
            // 
            this.labelIconStep3.AutoSize = true;
            this.labelIconStep3.Location = new System.Drawing.Point(7, 124);
            this.labelIconStep3.Name = "labelIconStep3";
            this.labelIconStep3.Size = new System.Drawing.Size(16, 15);
            this.labelIconStep3.TabIndex = 26;
            this.labelIconStep3.Text = "3.";
            // 
            // textBoxHrefIco
            // 
            this.textBoxHrefIco.AllowDrop = true;
            this.textBoxHrefIco.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHrefIco.HintText = "HTTP Address";
            this.textBoxHrefIco.HttpOnly = true;
            this.textBoxHrefIco.Location = new System.Drawing.Point(171, 94);
            this.textBoxHrefIco.Name = "textBoxHrefIco";
            this.textBoxHrefIco.Size = new System.Drawing.Size(310, 23);
            this.textBoxHrefIco.TabIndex = 6;
            // 
            // labelIconStep2
            // 
            this.labelIconStep2.AutoSize = true;
            this.labelIconStep2.Location = new System.Drawing.Point(7, 97);
            this.labelIconStep2.Name = "labelIconStep2";
            this.labelIconStep2.Size = new System.Drawing.Size(158, 15);
            this.labelIconStep2.TabIndex = 4;
            this.labelIconStep2.Text = "2.   Where will you upload it?";
            // 
            // buttonSaveIco
            // 
            this.buttonSaveIco.Location = new System.Drawing.Point(28, 66);
            this.buttonSaveIco.Name = "buttonSaveIco";
            this.buttonSaveIco.Size = new System.Drawing.Size(137, 23);
            this.buttonSaveIco.TabIndex = 23;
            this.buttonSaveIco.Text = "Extract as &ICO file";
            this.buttonSaveIco.UseVisualStyleBackColor = true;
            this.buttonSaveIco.Click += new System.EventHandler(this.buttonSaveIco_Click);
            // 
            // labelIconStep1
            // 
            this.labelIconStep1.AutoSize = true;
            this.labelIconStep1.Location = new System.Drawing.Point(7, 70);
            this.labelIconStep1.Name = "labelIconStep1";
            this.labelIconStep1.Size = new System.Drawing.Size(16, 15);
            this.labelIconStep1.TabIndex = 2;
            this.labelIconStep1.Text = "1.";
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBoxIcon.Location = new System.Drawing.Point(7, 9);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxIcon.TabIndex = 30;
            this.pictureBoxIcon.TabStop = false;
            // 
            // labelIcon
            // 
            this.labelIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIcon.Location = new System.Drawing.Point(62, 9);
            this.labelIcon.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIcon.Name = "labelIcon";
            this.labelIcon.Size = new System.Drawing.Size(419, 48);
            this.labelIcon.TabIndex = 0;
            this.labelIcon.Text = "Icons need to be extracted from the application and uploaded separately as both a" +
    "n ICO and a PNG.";
            // 
            // securityPage
            // 
            this.securityPage.Controls.Add(this.buttonNewKey);
            this.securityPage.Controls.Add(this.comboBoxKeys);
            this.securityPage.Controls.Add(this.labelInfoSignature);
            this.securityPage.Controls.Add(this.labelInterfaceUri);
            this.securityPage.Controls.Add(this.textBoxInterfaceUri);
            this.securityPage.Name = "securityPage";
            this.securityPage.NextPage = this.donePage;
            this.securityPage.Size = new System.Drawing.Size(527, 262);
            this.securityPage.TabIndex = 8;
            this.securityPage.Text = "Security";
            this.securityPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.securityPage_Commit);
            this.securityPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.securityPage_Initialize);
            // 
            // buttonNewKey
            // 
            this.buttonNewKey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNewKey.Location = new System.Drawing.Point(361, 66);
            this.buttonNewKey.Name = "buttonNewKey";
            this.buttonNewKey.Size = new System.Drawing.Size(120, 24);
            this.buttonNewKey.TabIndex = 8;
            this.buttonNewKey.Text = "New &key";
            this.buttonNewKey.UseVisualStyleBackColor = true;
            this.buttonNewKey.Click += new System.EventHandler(this.buttonNewKey_Click);
            // 
            // comboBoxKeys
            // 
            this.comboBoxKeys.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxKeys.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKeys.FormattingEnabled = true;
            this.comboBoxKeys.Location = new System.Drawing.Point(7, 67);
            this.comboBoxKeys.Name = "comboBoxKeys";
            this.comboBoxKeys.Size = new System.Drawing.Size(348, 23);
            this.comboBoxKeys.TabIndex = 7;
            this.comboBoxKeys.SelectedIndexChanged += new System.EventHandler(this.comboBoxKeys_SelectedIndexChanged);
            // 
            // labelInfoSignature
            // 
            this.labelInfoSignature.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInfoSignature.Location = new System.Drawing.Point(4, 9);
            this.labelInfoSignature.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelInfoSignature.Name = "labelInfoSignature";
            this.labelInfoSignature.Size = new System.Drawing.Size(477, 55);
            this.labelInfoSignature.TabIndex = 2;
            this.labelInfoSignature.Text = "Zero Install protects feeds with GnuPG signatures. Please select a private key to" +
    " sign your feed:";
            // 
            // labelInterfaceUri
            // 
            this.labelInterfaceUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInterfaceUri.Location = new System.Drawing.Point(4, 141);
            this.labelInterfaceUri.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelInterfaceUri.Name = "labelInterfaceUri";
            this.labelInterfaceUri.Size = new System.Drawing.Size(477, 38);
            this.labelInterfaceUri.TabIndex = 4;
            this.labelInterfaceUri.Text = "Where will you upload the feed? This address will be stored within the feed itsel" +
    "f!";
            // 
            // textBoxInterfaceUri
            // 
            this.textBoxInterfaceUri.AllowDrop = true;
            this.textBoxInterfaceUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxInterfaceUri.HintText = "HTTP URI";
            this.textBoxInterfaceUri.HttpOnly = true;
            this.textBoxInterfaceUri.Location = new System.Drawing.Point(7, 182);
            this.textBoxInterfaceUri.Name = "textBoxInterfaceUri";
            this.textBoxInterfaceUri.Size = new System.Drawing.Size(474, 23);
            this.textBoxInterfaceUri.TabIndex = 10;
            // 
            // donePage
            // 
            this.donePage.Controls.Add(this.labelDone2);
            this.donePage.Controls.Add(this.labelDone);
            this.donePage.IsFinishPage = true;
            this.donePage.Name = "donePage";
            this.donePage.Size = new System.Drawing.Size(527, 262);
            this.donePage.TabIndex = 9;
            this.donePage.Text = "Done";
            this.donePage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.donePage_Commit);
            // 
            // labelDone2
            // 
            this.labelDone2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDone2.Location = new System.Drawing.Point(4, 101);
            this.labelDone2.Name = "labelDone2";
            this.labelDone2.Size = new System.Drawing.Size(477, 55);
            this.labelDone2.TabIndex = 1;
            this.labelDone2.Text = "When uploading the feed make sure you also include these files: feed.xsl, feed.cs" +
    "s, *.gpg";
            // 
            // labelDone
            // 
            this.labelDone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDone.Location = new System.Drawing.Point(4, 9);
            this.labelDone.Name = "labelDone";
            this.labelDone.Size = new System.Drawing.Size(477, 55);
            this.labelDone.TabIndex = 0;
            this.labelDone.Text = "The wizard is done! We will now open the feed in the editor so you can look every" +
    "thing over, make any changes you like and save the feed as a file afterwards.";
            // 
            // installerCaptureStartPage
            // 
            this.installerCaptureStartPage.Controls.Add(this.buttonSkipCapture);
            this.installerCaptureStartPage.Controls.Add(this.labelCaptureStart2);
            this.installerCaptureStartPage.Controls.Add(this.labelCaptureStart);
            this.installerCaptureStartPage.Name = "installerCaptureStartPage";
            this.installerCaptureStartPage.NextPage = this.installerCaptureDiffPage;
            this.installerCaptureStartPage.Size = new System.Drawing.Size(527, 262);
            this.installerCaptureStartPage.TabIndex = 3;
            this.installerCaptureStartPage.Text = "Installer Capture";
            this.installerCaptureStartPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.installerCaptureStartPage_Commit);
            // 
            // buttonSkipCapture
            // 
            this.buttonSkipCapture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSkipCapture.Location = new System.Drawing.Point(452, 239);
            this.buttonSkipCapture.Name = "buttonSkipCapture";
            this.buttonSkipCapture.Size = new System.Drawing.Size(75, 23);
            this.buttonSkipCapture.TabIndex = 2;
            this.buttonSkipCapture.Text = "&Skip >";
            this.buttonSkipCapture.UseVisualStyleBackColor = true;
            this.buttonSkipCapture.Click += new System.EventHandler(this.buttonSkipCapture_Click);
            // 
            // labelCaptureStart2
            // 
            this.labelCaptureStart2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCaptureStart2.Location = new System.Drawing.Point(4, 82);
            this.labelCaptureStart2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCaptureStart2.Name = "labelCaptureStart2";
            this.labelCaptureStart2.Size = new System.Drawing.Size(477, 64);
            this.labelCaptureStart2.TabIndex = 1;
            this.labelCaptureStart2.Text = "Important: For best results you should do this in a pristine VM with nothing inst" +
    "alled except for the operating system and Zero Install.";
            // 
            // labelCaptureStart
            // 
            this.labelCaptureStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCaptureStart.Location = new System.Drawing.Point(4, 9);
            this.labelCaptureStart.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCaptureStart.Name = "labelCaptureStart";
            this.labelCaptureStart.Size = new System.Drawing.Size(477, 73);
            this.labelCaptureStart.TabIndex = 0;
            this.labelCaptureStart.Text = "We will now capture a snapshot of the system\'s current state. After running the i" +
    "nstaller we will create another snapshot and compare to the two in order to dete" +
    "rmine what changes the installer made.";
            // 
            // installerCaptureDiffPage
            // 
            this.installerCaptureDiffPage.Controls.Add(this.groupInstallationDir);
            this.installerCaptureDiffPage.Controls.Add(this.labelCaptureDiff);
            this.installerCaptureDiffPage.Name = "installerCaptureDiffPage";
            this.installerCaptureDiffPage.Size = new System.Drawing.Size(527, 262);
            this.installerCaptureDiffPage.TabIndex = 4;
            this.installerCaptureDiffPage.Text = "Installer Capture";
            this.installerCaptureDiffPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.installerCaptureDiffPage_Commit);
            this.installerCaptureDiffPage.Rollback += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.installerCaptureDiffPage_Rollback);
            // 
            // groupInstallationDir
            // 
            this.groupInstallationDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupInstallationDir.Controls.Add(this.buttonSelectInstallationDir);
            this.groupInstallationDir.Controls.Add(this.textBoxInstallationDir);
            this.groupInstallationDir.Location = new System.Drawing.Point(7, 92);
            this.groupInstallationDir.Name = "groupInstallationDir";
            this.groupInstallationDir.Size = new System.Drawing.Size(474, 66);
            this.groupInstallationDir.TabIndex = 1;
            this.groupInstallationDir.TabStop = false;
            this.groupInstallationDir.Text = "Where did you install the application?";
            // 
            // buttonSelectInstallationDir
            // 
            this.buttonSelectInstallationDir.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonSelectInstallationDir.Location = new System.Drawing.Point(438, 26);
            this.buttonSelectInstallationDir.Name = "buttonSelectInstallationDir";
            this.buttonSelectInstallationDir.Size = new System.Drawing.Size(29, 23);
            this.buttonSelectInstallationDir.TabIndex = 1;
            this.buttonSelectInstallationDir.Text = "...";
            this.buttonSelectInstallationDir.UseVisualStyleBackColor = true;
            this.buttonSelectInstallationDir.Click += new System.EventHandler(this.buttonSelectInstallationDir_Click);
            // 
            // textBoxInstallationDir
            // 
            this.textBoxInstallationDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxInstallationDir.HintText = "Directory path; leave empty to auto-detect";
            this.textBoxInstallationDir.Location = new System.Drawing.Point(6, 26);
            this.textBoxInstallationDir.Name = "textBoxInstallationDir";
            this.textBoxInstallationDir.Size = new System.Drawing.Size(426, 23);
            this.textBoxInstallationDir.TabIndex = 0;
            // 
            // labelCaptureDiff
            // 
            this.labelCaptureDiff.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCaptureDiff.Location = new System.Drawing.Point(4, 9);
            this.labelCaptureDiff.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelCaptureDiff.Name = "labelCaptureDiff";
            this.labelCaptureDiff.Size = new System.Drawing.Size(477, 80);
            this.labelCaptureDiff.TabIndex = 0;
            this.labelCaptureDiff.Text = "Make sure the installer has finished installing the application. When you are rea" +
    "dy, continue to capture a second snapshot.";
            // 
            // installerCollectFilesPage
            // 
            this.installerCollectFilesPage.Controls.Add(this.buttonExistingArchive);
            this.installerCollectFilesPage.Controls.Add(this.buttonCreateArchive);
            this.installerCollectFilesPage.Controls.Add(this.groupCreateArchive);
            this.installerCollectFilesPage.Name = "installerCollectFilesPage";
            this.installerCollectFilesPage.ShowNext = false;
            this.installerCollectFilesPage.Size = new System.Drawing.Size(527, 262);
            this.installerCollectFilesPage.TabIndex = 5;
            this.installerCollectFilesPage.Text = "Collect files";
            // 
            // buttonExistingArchive
            // 
            this.buttonExistingArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExistingArchive.Location = new System.Drawing.Point(314, 191);
            this.buttonExistingArchive.Name = "buttonExistingArchive";
            this.buttonExistingArchive.Size = new System.Drawing.Size(160, 23);
            this.buttonExistingArchive.TabIndex = 2;
            this.buttonExistingArchive.Text = "Use &existing archive >";
            this.buttonExistingArchive.UseVisualStyleBackColor = true;
            this.buttonExistingArchive.Click += new System.EventHandler(this.buttonExistingArchive_Click);
            // 
            // buttonCreateArchive
            // 
            this.buttonCreateArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCreateArchive.Enabled = false;
            this.buttonCreateArchive.Location = new System.Drawing.Point(314, 162);
            this.buttonCreateArchive.Name = "buttonCreateArchive";
            this.buttonCreateArchive.Size = new System.Drawing.Size(160, 23);
            this.buttonCreateArchive.TabIndex = 1;
            this.buttonCreateArchive.Text = "&Create archive >";
            this.buttonCreateArchive.UseVisualStyleBackColor = true;
            this.buttonCreateArchive.Click += new System.EventHandler(this.buttonCreateArchive_Click);
            // 
            // groupCreateArchive
            // 
            this.groupCreateArchive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupCreateArchive.Controls.Add(this.labelLocalPathArchive);
            this.groupCreateArchive.Controls.Add(this.labelUploadUrl);
            this.groupCreateArchive.Controls.Add(this.textBoxUploadUrl);
            this.groupCreateArchive.Controls.Add(this.buttonSelectArchivePath);
            this.groupCreateArchive.Controls.Add(this.textBoxArchivePath);
            this.groupCreateArchive.Location = new System.Drawing.Point(7, 3);
            this.groupCreateArchive.Name = "groupCreateArchive";
            this.groupCreateArchive.Size = new System.Drawing.Size(473, 171);
            this.groupCreateArchive.TabIndex = 0;
            this.groupCreateArchive.TabStop = false;
            // 
            // labelLocalPathArchive
            // 
            this.labelLocalPathArchive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLocalPathArchive.Location = new System.Drawing.Point(7, 16);
            this.labelLocalPathArchive.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLocalPathArchive.Name = "labelLocalPathArchive";
            this.labelLocalPathArchive.Size = new System.Drawing.Size(455, 49);
            this.labelLocalPathArchive.TabIndex = 0;
            this.labelLocalPathArchive.Text = "The wizard can create a ZIP archive containing the installation directory for you" +
    ". Where do you want to place it?";
            // 
            // labelUploadUrl
            // 
            this.labelUploadUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelUploadUrl.Location = new System.Drawing.Point(7, 108);
            this.labelUploadUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUploadUrl.Name = "labelUploadUrl";
            this.labelUploadUrl.Size = new System.Drawing.Size(455, 19);
            this.labelUploadUrl.TabIndex = 3;
            this.labelUploadUrl.Text = "Where will you upload this ZIP archive?";
            // 
            // textBoxUploadUrl
            // 
            this.textBoxUploadUrl.AllowDrop = true;
            this.textBoxUploadUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUploadUrl.ForeColor = System.Drawing.Color.Red;
            this.textBoxUploadUrl.HintText = "HTTP/FTP URL";
            this.textBoxUploadUrl.Location = new System.Drawing.Point(10, 130);
            this.textBoxUploadUrl.Name = "textBoxUploadUrl";
            this.textBoxUploadUrl.Size = new System.Drawing.Size(457, 23);
            this.textBoxUploadUrl.TabIndex = 4;
            this.textBoxUploadUrl.TextChanged += new System.EventHandler(this.installerCollectFilesPage_ToggleControls);
            // 
            // buttonSelectArchivePath
            // 
            this.buttonSelectArchivePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectArchivePath.Location = new System.Drawing.Point(438, 68);
            this.buttonSelectArchivePath.Name = "buttonSelectArchivePath";
            this.buttonSelectArchivePath.Size = new System.Drawing.Size(29, 23);
            this.buttonSelectArchivePath.TabIndex = 2;
            this.buttonSelectArchivePath.Text = "...";
            this.buttonSelectArchivePath.UseVisualStyleBackColor = true;
            this.buttonSelectArchivePath.Click += new System.EventHandler(this.buttonSelectArchivePath_Click);
            // 
            // textBoxArchivePath
            // 
            this.textBoxArchivePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxArchivePath.HintText = "File path";
            this.textBoxArchivePath.Location = new System.Drawing.Point(10, 68);
            this.textBoxArchivePath.Name = "textBoxArchivePath";
            this.textBoxArchivePath.Size = new System.Drawing.Size(422, 23);
            this.textBoxArchivePath.TabIndex = 1;
            this.textBoxArchivePath.TextChanged += new System.EventHandler(this.installerCollectFilesPage_ToggleControls);
            // 
            // installerAltDownloadPage
            // 
            this.installerAltDownloadPage.Controls.Add(this.checkAltLocalCopy);
            this.installerAltDownloadPage.Controls.Add(this.groupAltLocalCopy);
            this.installerAltDownloadPage.Controls.Add(this.textBoxAltDownloadUrl);
            this.installerAltDownloadPage.Controls.Add(this.labelAltDownloadUrl);
            this.installerAltDownloadPage.Name = "installerAltDownloadPage";
            this.installerAltDownloadPage.Size = new System.Drawing.Size(527, 262);
            this.installerAltDownloadPage.TabIndex = 10;
            this.installerAltDownloadPage.Text = "Alternative download";
            this.installerAltDownloadPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.installerAltDownloadPage_Commit);
            // 
            // checkAltLocalCopy
            // 
            this.checkAltLocalCopy.AutoSize = true;
            this.checkAltLocalCopy.Location = new System.Drawing.Point(18, 144);
            this.checkAltLocalCopy.Name = "checkAltLocalCopy";
            this.checkAltLocalCopy.Size = new System.Drawing.Size(178, 19);
            this.checkAltLocalCopy.TabIndex = 2;
            this.checkAltLocalCopy.Text = "I have a &local copy of this file";
            this.checkAltLocalCopy.UseVisualStyleBackColor = true;
            this.checkAltLocalCopy.CheckedChanged += new System.EventHandler(this.installerAltDownloadPage_ToggleControls);
            // 
            // groupAltLocalCopy
            // 
            this.groupAltLocalCopy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupAltLocalCopy.Controls.Add(this.buttonSelectAltLocalPath);
            this.groupAltLocalCopy.Controls.Add(this.textBoxAltLocalPath);
            this.groupAltLocalCopy.Enabled = false;
            this.groupAltLocalCopy.Location = new System.Drawing.Point(7, 143);
            this.groupAltLocalCopy.Name = "groupAltLocalCopy";
            this.groupAltLocalCopy.Size = new System.Drawing.Size(474, 65);
            this.groupAltLocalCopy.TabIndex = 3;
            this.groupAltLocalCopy.TabStop = false;
            // 
            // buttonSelectAltLocalPath
            // 
            this.buttonSelectAltLocalPath.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonSelectAltLocalPath.Location = new System.Drawing.Point(438, 26);
            this.buttonSelectAltLocalPath.Name = "buttonSelectAltLocalPath";
            this.buttonSelectAltLocalPath.Size = new System.Drawing.Size(29, 23);
            this.buttonSelectAltLocalPath.TabIndex = 1;
            this.buttonSelectAltLocalPath.Text = "...";
            this.buttonSelectAltLocalPath.UseVisualStyleBackColor = true;
            this.buttonSelectAltLocalPath.Click += new System.EventHandler(this.buttonSelectAltLocalPath_Click);
            // 
            // textBoxAltLocalPath
            // 
            this.textBoxAltLocalPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAltLocalPath.HintText = "File path";
            this.textBoxAltLocalPath.Location = new System.Drawing.Point(11, 26);
            this.textBoxAltLocalPath.Name = "textBoxAltLocalPath";
            this.textBoxAltLocalPath.Size = new System.Drawing.Size(421, 23);
            this.textBoxAltLocalPath.TabIndex = 0;
            this.textBoxAltLocalPath.TextChanged += new System.EventHandler(this.installerAltDownloadPage_ToggleControls);
            // 
            // textBoxAltDownloadUrl
            // 
            this.textBoxAltDownloadUrl.AllowDrop = true;
            this.textBoxAltDownloadUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxAltDownloadUrl.ForeColor = System.Drawing.Color.Red;
            this.textBoxAltDownloadUrl.HintText = "HTTP/FTP URL";
            this.textBoxAltDownloadUrl.Location = new System.Drawing.Point(7, 92);
            this.textBoxAltDownloadUrl.Name = "textBoxAltDownloadUrl";
            this.textBoxAltDownloadUrl.Size = new System.Drawing.Size(474, 23);
            this.textBoxAltDownloadUrl.TabIndex = 1;
            this.textBoxAltDownloadUrl.TextChanged += new System.EventHandler(this.installerAltDownloadPage_ToggleControls);
            // 
            // labelAltDownloadUrl
            // 
            this.labelAltDownloadUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAltDownloadUrl.Location = new System.Drawing.Point(4, 9);
            this.labelAltDownloadUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelAltDownloadUrl.Name = "labelAltDownloadUrl";
            this.labelAltDownloadUrl.Size = new System.Drawing.Size(477, 80);
            this.labelAltDownloadUrl.TabIndex = 0;
            this.labelAltDownloadUrl.Text = resources.GetString("labelAltDownloadUrl.Text");
            // 
            // NewFeedWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 415);
            this.Controls.Add(this.wizardControl);
            this.MinimumSize = new System.Drawing.Size(400, 400);
            this.Name = "NewFeedWizard";
            this.ShowIcon = false;
            this.Text = "New Feed";
            ((System.ComponentModel.ISupportInitialize)(this.wizardControl)).EndInit();
            this.downloadPage.ResumeLayout(false);
            this.downloadPage.PerformLayout();
            this.groupLocalCopy.ResumeLayout(false);
            this.groupLocalCopy.PerformLayout();
            this.archiveExtractPage.ResumeLayout(false);
            this.entryPointPage.ResumeLayout(false);
            this.detailsPage.ResumeLayout(false);
            this.iconPage.ResumeLayout(false);
            this.iconPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.securityPage.ResumeLayout(false);
            this.securityPage.PerformLayout();
            this.donePage.ResumeLayout(false);
            this.installerCaptureStartPage.ResumeLayout(false);
            this.installerCaptureDiffPage.ResumeLayout(false);
            this.groupInstallationDir.ResumeLayout(false);
            this.groupInstallationDir.PerformLayout();
            this.installerCollectFilesPage.ResumeLayout(false);
            this.groupCreateArchive.ResumeLayout(false);
            this.groupCreateArchive.PerformLayout();
            this.installerAltDownloadPage.ResumeLayout(false);
            this.installerAltDownloadPage.PerformLayout();
            this.groupAltLocalCopy.ResumeLayout(false);
            this.groupAltLocalCopy.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private AeroWizard.WizardControl wizardControl;
        private AeroWizard.WizardPage downloadPage;
        private AeroWizard.WizardPage archiveExtractPage;
        private AeroWizard.WizardPage entryPointPage;
        private AeroWizard.WizardPage installerCaptureStartPage;
        private AeroWizard.WizardPage installerCaptureDiffPage;
        private AeroWizard.WizardPage installerCollectFilesPage;
        private AeroWizard.WizardPage detailsPage;
        private AeroWizard.WizardPage iconPage;
        private AeroWizard.WizardPage securityPage;
        private AeroWizard.WizardPage donePage;
        private System.Windows.Forms.GroupBox groupLocalCopy;
        private System.Windows.Forms.CheckBox checkLocalCopy;
        private System.Windows.Forms.Button buttonSelectLocalPath;
        private NanoByte.Common.Controls.HintTextBox textBoxLocalPath;
        private NanoByte.Common.Controls.UriTextBox textBoxDownloadUrl;
        private System.Windows.Forms.Label labelDownloadUrl;
        private System.Windows.Forms.ListBox listBoxExtract;
        private System.Windows.Forms.Label labelExtract;
        private System.Windows.Forms.Label labelEntyPoint;
        private System.Windows.Forms.Button buttonSkipCapture;
        private System.Windows.Forms.Label labelCaptureStart2;
        private System.Windows.Forms.Label labelCaptureStart;
        private System.Windows.Forms.GroupBox groupInstallationDir;
        private System.Windows.Forms.Button buttonSelectInstallationDir;
        private NanoByte.Common.Controls.HintTextBox textBoxInstallationDir;
        private System.Windows.Forms.Label labelCaptureDiff;
        private System.Windows.Forms.Button buttonExistingArchive;
        private System.Windows.Forms.Button buttonCreateArchive;
        private System.Windows.Forms.GroupBox groupCreateArchive;
        private System.Windows.Forms.Label labelLocalPathArchive;
        private System.Windows.Forms.Label labelUploadUrl;
        private NanoByte.Common.Controls.UriTextBox textBoxUploadUrl;
        private System.Windows.Forms.Button buttonSelectArchivePath;
        private NanoByte.Common.Controls.HintTextBox textBoxArchivePath;
        private System.Windows.Forms.Label labelDetails;
        private NanoByte.Common.Controls.ResettablePropertyGrid propertyGridCandidate;
        private NanoByte.Common.Controls.UriTextBox textBoxHrefPng;
        private System.Windows.Forms.Label labelIconStep4;
        private System.Windows.Forms.Button buttonSavePng;
        private System.Windows.Forms.Label labelIconStep3;
        private NanoByte.Common.Controls.UriTextBox textBoxHrefIco;
        private System.Windows.Forms.Label labelIconStep2;
        private System.Windows.Forms.Button buttonSaveIco;
        private System.Windows.Forms.Label labelIconStep1;
        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Label labelIcon;
        private System.Windows.Forms.Button buttonNewKey;
        private System.Windows.Forms.ComboBox comboBoxKeys;
        private System.Windows.Forms.Label labelInfoSignature;
        private System.Windows.Forms.Label labelInterfaceUri;
        private NanoByte.Common.Controls.UriTextBox textBoxInterfaceUri;
        private System.Windows.Forms.Label labelDone2;
        private System.Windows.Forms.Label labelDone;
        private AeroWizard.WizardPage installerAltDownloadPage;
        private System.Windows.Forms.CheckBox checkAltLocalCopy;
        private System.Windows.Forms.GroupBox groupAltLocalCopy;
        private System.Windows.Forms.Button buttonSelectAltLocalPath;
        private NanoByte.Common.Controls.HintTextBox textBoxAltLocalPath;
        private NanoByte.Common.Controls.UriTextBox textBoxAltDownloadUrl;
        private System.Windows.Forms.Label labelAltDownloadUrl;
        private System.Windows.Forms.ListBox listBoxEntryPoint;
    }
}