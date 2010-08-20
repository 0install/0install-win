using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace ZeroInstall.Publish.WinForms
{
    partial class MainForm
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Foo")]
        private void InitializeComponent()
        {
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Interface");
            ZeroInstall.Model.FeedReference feedReference1 = new ZeroInstall.Model.FeedReference();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.checkBoxNeedsTerminal = new System.Windows.Forms.CheckBox();
            this.hintTextBoxInterfaceUrl = new Common.Controls.HintTextBox();
            this.labelInterfaceUrl = new System.Windows.Forms.Label();
            this.checkedListBoxCategories = new System.Windows.Forms.CheckedListBox();
            this.hintTextBoxHomepage = new Common.Controls.HintTextBox();
            this.hintTextBoxDescription = new Common.Controls.HintTextBox();
            this.labelHomepage = new System.Windows.Forms.Label();
            this.labelDescription = new System.Windows.Forms.Label();
            this.groupBoxIcon = new System.Windows.Forms.GroupBox();
            this.labelIconUrl = new System.Windows.Forms.Label();
            this.comboBoxIconType = new System.Windows.Forms.ComboBox();
            this.hintTextBoxIconUrl = new Common.Controls.HintTextBox();
            this.lblIconUrlError = new System.Windows.Forms.Label();
            this.labelIconMimeType = new System.Windows.Forms.Label();
            this.buttonIconPreview = new System.Windows.Forms.Button();
            this.buttonIconRemove = new System.Windows.Forms.Button();
            this.pictureBoxIconPreview = new System.Windows.Forms.PictureBox();
            this.buttonIconAdd = new System.Windows.Forms.Button();
            this.listBoxIconsUrls = new System.Windows.Forms.ListBox();
            this.labelCategories = new System.Windows.Forms.Label();
            this.hintTextBoxSummary = new Common.Controls.HintTextBox();
            this.labelSummary = new System.Windows.Forms.Label();
            this.hintTextBoxProgramName = new Common.Controls.HintTextBox();
            this.labelProgramName = new System.Windows.Forms.Label();
            this.tabPageFeed = new System.Windows.Forms.TabPage();
            this.groupBoxFeedStructure = new System.Windows.Forms.GroupBox();
            this.buttonAddRecipe = new System.Windows.Forms.Button();
            this.buttonClearList = new System.Windows.Forms.Button();
            this.buttonAddArchive = new System.Windows.Forms.Button();
            this.btnAddOverlayBinding = new System.Windows.Forms.Button();
            this.treeViewFeedStructure = new System.Windows.Forms.TreeView();
            this.btnRemoveFeedStructureObject = new System.Windows.Forms.Button();
            this.btnAddEnvironmentBinding = new System.Windows.Forms.Button();
            this.btnAddGroup = new System.Windows.Forms.Button();
            this.btnAddDependency = new System.Windows.Forms.Button();
            this.btnAddPackageImplementation = new System.Windows.Forms.Button();
            this.btnAddImplementation = new System.Windows.Forms.Button();
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.groupBoxFeedFor = new System.Windows.Forms.GroupBox();
            this.buttonClearFeedFor = new System.Windows.Forms.Button();
            this.buttonRemoveFeedFor = new System.Windows.Forms.Button();
            this.buttonAddFeedFor = new System.Windows.Forms.Button();
            this.listBoxFeedFor = new System.Windows.Forms.ListBox();
            this.hintTextBoxFeedFor = new Common.Controls.HintTextBox();
            this.comboBoxMinInjectorVersion = new System.Windows.Forms.ComboBox();
            this.labelMinInjectorVersion = new System.Windows.Forms.Label();
            this.groupBoxExternalFeeds = new System.Windows.Forms.GroupBox();
            this.buttonUpdateExternalFeed = new System.Windows.Forms.Button();
            this.groupBoxSelectedFeed = new System.Windows.Forms.GroupBox();
            this.listBoxExternalFeeds = new System.Windows.Forms.ListBox();
            this.buttonAddExternalFeeds = new System.Windows.Forms.Button();
            this.buttonRemoveExternalFeed = new System.Windows.Forms.Button();
            this.feedReferenceControl = new ZeroInstall.Publish.WinForms.FeedReferenceControl();
            this.toolStrip.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.groupBoxIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIconPreview)).BeginInit();
            this.tabPageFeed.SuspendLayout();
            this.groupBoxFeedStructure.SuspendLayout();
            this.tabPageAdvanced.SuspendLayout();
            this.groupBoxFeedFor.SuspendLayout();
            this.groupBoxExternalFeeds.SuspendLayout();
            this.groupBoxSelectedFeed.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonNew,
            this.toolStripButtonOpen,
            this.toolStripButtonSave});
            this.toolStrip.Location = new System.Drawing.Point(7, 6);
            this.toolStrip.Margin = new System.Windows.Forms.Padding(2);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip.Size = new System.Drawing.Size(619, 25);
            this.toolStrip.TabIndex = 0;
            // 
            // toolStripButtonNew
            // 
            this.toolStripButtonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonNew.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.NewButton;
            this.toolStripButtonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNew.Name = "toolStripButtonNew";
            this.toolStripButtonNew.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonNew.Text = "New";
            this.toolStripButtonNew.Click += new System.EventHandler(this.ToolStripButtonNew_Click);
            // 
            // toolStripButtonOpen
            // 
            this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOpen.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.OpenButton;
            this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonOpen.Text = "Open";
            this.toolStripButtonOpen.Click += new System.EventHandler(this.ToolStripButtonOpen_Click);
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSave.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.SaveButton;
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonSave.Text = "Save";
            this.toolStripButtonSave.Click += new System.EventHandler(this.ToolStripButtonSave_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.OpenFileDialog_FileOk);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.SaveFileDialog_FileOk);
            // 
            // tabControlMain
            // 
            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabPageGeneral);
            this.tabControlMain.Controls.Add(this.tabPageFeed);
            this.tabControlMain.Controls.Add(this.tabPageAdvanced);
            this.tabControlMain.Location = new System.Drawing.Point(7, 34);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(616, 535);
            this.tabControlMain.TabIndex = 1;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.checkBoxNeedsTerminal);
            this.tabPageGeneral.Controls.Add(this.hintTextBoxInterfaceUrl);
            this.tabPageGeneral.Controls.Add(this.labelInterfaceUrl);
            this.tabPageGeneral.Controls.Add(this.checkedListBoxCategories);
            this.tabPageGeneral.Controls.Add(this.hintTextBoxHomepage);
            this.tabPageGeneral.Controls.Add(this.hintTextBoxDescription);
            this.tabPageGeneral.Controls.Add(this.labelHomepage);
            this.tabPageGeneral.Controls.Add(this.labelDescription);
            this.tabPageGeneral.Controls.Add(this.groupBoxIcon);
            this.tabPageGeneral.Controls.Add(this.labelCategories);
            this.tabPageGeneral.Controls.Add(this.hintTextBoxSummary);
            this.tabPageGeneral.Controls.Add(this.labelSummary);
            this.tabPageGeneral.Controls.Add(this.hintTextBoxProgramName);
            this.tabPageGeneral.Controls.Add(this.labelProgramName);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(608, 509);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // checkBoxNeedsTerminal
            // 
            this.checkBoxNeedsTerminal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxNeedsTerminal.AutoSize = true;
            this.checkBoxNeedsTerminal.Location = new System.Drawing.Point(9, 441);
            this.checkBoxNeedsTerminal.Name = "checkBoxNeedsTerminal";
            this.checkBoxNeedsTerminal.Size = new System.Drawing.Size(98, 17);
            this.checkBoxNeedsTerminal.TabIndex = 13;
            this.checkBoxNeedsTerminal.Text = "needs Terminal";
            this.checkBoxNeedsTerminal.UseVisualStyleBackColor = true;
            // 
            // hintTextBoxInterfaceUrl
            // 
            this.hintTextBoxInterfaceUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxInterfaceUrl.HintText = "URL to a remote interface";
            this.hintTextBoxInterfaceUrl.Location = new System.Drawing.Point(9, 98);
            this.hintTextBoxInterfaceUrl.Name = "hintTextBoxInterfaceUrl";
            this.hintTextBoxInterfaceUrl.Size = new System.Drawing.Size(593, 20);
            this.hintTextBoxInterfaceUrl.TabIndex = 7;
            this.hintTextBoxInterfaceUrl.TextChanged += new System.EventHandler(this.textInterfaceURL_TextChanged);
            // 
            // labelInterfaceUrl
            // 
            this.labelInterfaceUrl.AutoSize = true;
            this.labelInterfaceUrl.Location = new System.Drawing.Point(6, 82);
            this.labelInterfaceUrl.Name = "labelInterfaceUrl";
            this.labelInterfaceUrl.Size = new System.Drawing.Size(74, 13);
            this.labelInterfaceUrl.TabIndex = 6;
            this.labelInterfaceUrl.Text = "Interface URL";
            // 
            // checkedListBoxCategories
            // 
            this.checkedListBoxCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxCategories.CheckOnClick = true;
            this.checkedListBoxCategories.FormattingEnabled = true;
            this.checkedListBoxCategories.Items.AddRange(new object[] {
            "Audio",
            "AudioVideo",
            "Development",
            "Education",
            "Game",
            "Graphics",
            "Network",
            "Office",
            "Settings",
            "System",
            "Utility",
            "Video"});
            this.checkedListBoxCategories.Location = new System.Drawing.Point(486, 20);
            this.checkedListBoxCategories.Name = "checkedListBoxCategories";
            this.checkedListBoxCategories.Size = new System.Drawing.Size(116, 64);
            this.checkedListBoxCategories.Sorted = true;
            this.checkedListBoxCategories.TabIndex = 5;
            // 
            // hintTextBoxHomepage
            // 
            this.hintTextBoxHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxHomepage.HintText = "the URL of a web-page describing this interface in more detail";
            this.hintTextBoxHomepage.Location = new System.Drawing.Point(9, 415);
            this.hintTextBoxHomepage.Name = "hintTextBoxHomepage";
            this.hintTextBoxHomepage.Size = new System.Drawing.Size(593, 20);
            this.hintTextBoxHomepage.TabIndex = 12;
            this.hintTextBoxHomepage.TextChanged += new System.EventHandler(this.textHomepage_TextChanged);
            // 
            // hintTextBoxDescription
            // 
            this.hintTextBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxDescription.HintText = "a full description, which can be several paragraphs long";
            this.hintTextBoxDescription.Location = new System.Drawing.Point(9, 137);
            this.hintTextBoxDescription.Multiline = true;
            this.hintTextBoxDescription.Name = "hintTextBoxDescription";
            this.hintTextBoxDescription.Size = new System.Drawing.Size(593, 82);
            this.hintTextBoxDescription.TabIndex = 9;
            // 
            // labelHomepage
            // 
            this.labelHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelHomepage.AutoSize = true;
            this.labelHomepage.Location = new System.Drawing.Point(6, 399);
            this.labelHomepage.Name = "labelHomepage";
            this.labelHomepage.Size = new System.Drawing.Size(59, 13);
            this.labelHomepage.TabIndex = 11;
            this.labelHomepage.Text = "Homepage";
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(6, 121);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(60, 13);
            this.labelDescription.TabIndex = 8;
            this.labelDescription.Text = "Description";
            // 
            // groupBoxIcon
            // 
            this.groupBoxIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxIcon.Controls.Add(this.labelIconUrl);
            this.groupBoxIcon.Controls.Add(this.comboBoxIconType);
            this.groupBoxIcon.Controls.Add(this.hintTextBoxIconUrl);
            this.groupBoxIcon.Controls.Add(this.lblIconUrlError);
            this.groupBoxIcon.Controls.Add(this.labelIconMimeType);
            this.groupBoxIcon.Controls.Add(this.buttonIconPreview);
            this.groupBoxIcon.Controls.Add(this.buttonIconRemove);
            this.groupBoxIcon.Controls.Add(this.pictureBoxIconPreview);
            this.groupBoxIcon.Controls.Add(this.buttonIconAdd);
            this.groupBoxIcon.Controls.Add(this.listBoxIconsUrls);
            this.groupBoxIcon.Location = new System.Drawing.Point(6, 225);
            this.groupBoxIcon.Name = "groupBoxIcon";
            this.groupBoxIcon.Size = new System.Drawing.Size(596, 171);
            this.groupBoxIcon.TabIndex = 10;
            this.groupBoxIcon.TabStop = false;
            this.groupBoxIcon.Text = "Icon";
            // 
            // labelIconUrl
            // 
            this.labelIconUrl.AutoSize = true;
            this.labelIconUrl.Location = new System.Drawing.Point(6, 16);
            this.labelIconUrl.Name = "labelIconUrl";
            this.labelIconUrl.Size = new System.Drawing.Size(42, 13);
            this.labelIconUrl.TabIndex = 0;
            this.labelIconUrl.Text = "Icon url";
            // 
            // comboBoxIconType
            // 
            this.comboBoxIconType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxIconType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIconType.FormattingEnabled = true;
            this.comboBoxIconType.Items.AddRange(new object[] {
            "PNG",
            "ICO"});
            this.comboBoxIconType.Location = new System.Drawing.Point(384, 31);
            this.comboBoxIconType.Name = "comboBoxIconType";
            this.comboBoxIconType.Size = new System.Drawing.Size(76, 21);
            this.comboBoxIconType.TabIndex = 3;
            // 
            // hintTextBoxIconUrl
            // 
            this.hintTextBoxIconUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxIconUrl.HintText = "";
            this.hintTextBoxIconUrl.Location = new System.Drawing.Point(9, 32);
            this.hintTextBoxIconUrl.Name = "hintTextBoxIconUrl";
            this.hintTextBoxIconUrl.Size = new System.Drawing.Size(369, 20);
            this.hintTextBoxIconUrl.TabIndex = 1;
            this.hintTextBoxIconUrl.TextChanged += new System.EventHandler(this.textIconUrl_TextChanged);
            // 
            // lblIconUrlError
            // 
            this.lblIconUrlError.AutoSize = true;
            this.lblIconUrlError.ForeColor = System.Drawing.Color.Red;
            this.lblIconUrlError.Location = new System.Drawing.Point(6, 144);
            this.lblIconUrlError.Name = "lblIconUrlError";
            this.lblIconUrlError.Size = new System.Drawing.Size(0, 13);
            this.lblIconUrlError.TabIndex = 5;
            // 
            // labelIconMimeType
            // 
            this.labelIconMimeType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIconMimeType.AutoSize = true;
            this.labelIconMimeType.Location = new System.Drawing.Point(381, 16);
            this.labelIconMimeType.Name = "labelIconMimeType";
            this.labelIconMimeType.Size = new System.Drawing.Size(55, 13);
            this.labelIconMimeType.TabIndex = 2;
            this.labelIconMimeType.Text = "Icon Type";
            // 
            // buttonIconPreview
            // 
            this.buttonIconPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIconPreview.Location = new System.Drawing.Point(384, 58);
            this.buttonIconPreview.Name = "buttonIconPreview";
            this.buttonIconPreview.Size = new System.Drawing.Size(80, 23);
            this.buttonIconPreview.TabIndex = 6;
            this.buttonIconPreview.Text = "Preview";
            this.buttonIconPreview.UseVisualStyleBackColor = true;
            this.buttonIconPreview.Click += new System.EventHandler(this.BtnIconPreviewClick);
            // 
            // buttonIconRemove
            // 
            this.buttonIconRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIconRemove.Location = new System.Drawing.Point(384, 116);
            this.buttonIconRemove.Name = "buttonIconRemove";
            this.buttonIconRemove.Size = new System.Drawing.Size(80, 23);
            this.buttonIconRemove.TabIndex = 8;
            this.buttonIconRemove.Text = "Remove";
            this.buttonIconRemove.UseVisualStyleBackColor = true;
            this.buttonIconRemove.Click += new System.EventHandler(this.btnIconListRemove_Click);
            // 
            // pictureBoxIconPreview
            // 
            this.pictureBoxIconPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxIconPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxIconPreview.Location = new System.Drawing.Point(470, 19);
            this.pictureBoxIconPreview.Name = "pictureBoxIconPreview";
            this.pictureBoxIconPreview.Size = new System.Drawing.Size(120, 120);
            this.pictureBoxIconPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxIconPreview.TabIndex = 9;
            this.pictureBoxIconPreview.TabStop = false;
            // 
            // buttonIconAdd
            // 
            this.buttonIconAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIconAdd.Location = new System.Drawing.Point(384, 87);
            this.buttonIconAdd.Name = "buttonIconAdd";
            this.buttonIconAdd.Size = new System.Drawing.Size(80, 23);
            this.buttonIconAdd.TabIndex = 7;
            this.buttonIconAdd.Text = "Add";
            this.buttonIconAdd.UseVisualStyleBackColor = true;
            this.buttonIconAdd.Click += new System.EventHandler(this.btnIconListAdd_Click);
            // 
            // listBoxIconsUrls
            // 
            this.listBoxIconsUrls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxIconsUrls.FormattingEnabled = true;
            this.listBoxIconsUrls.HorizontalScrollbar = true;
            this.listBoxIconsUrls.Location = new System.Drawing.Point(9, 59);
            this.listBoxIconsUrls.Name = "listBoxIconsUrls";
            this.listBoxIconsUrls.Size = new System.Drawing.Size(369, 82);
            this.listBoxIconsUrls.TabIndex = 4;
            this.listBoxIconsUrls.SelectedIndexChanged += new System.EventHandler(this.listIconsUrls_SelectedIndexChanged);
            // 
            // labelCategories
            // 
            this.labelCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCategories.AutoSize = true;
            this.labelCategories.Location = new System.Drawing.Point(483, 3);
            this.labelCategories.Name = "labelCategories";
            this.labelCategories.Size = new System.Drawing.Size(57, 13);
            this.labelCategories.TabIndex = 4;
            this.labelCategories.Text = "Categories";
            // 
            // hintTextBoxSummary
            // 
            this.hintTextBoxSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSummary.HintText = "a short one-line description";
            this.hintTextBoxSummary.Location = new System.Drawing.Point(9, 59);
            this.hintTextBoxSummary.Name = "hintTextBoxSummary";
            this.hintTextBoxSummary.Size = new System.Drawing.Size(471, 20);
            this.hintTextBoxSummary.TabIndex = 3;
            // 
            // labelSummary
            // 
            this.labelSummary.AutoSize = true;
            this.labelSummary.Location = new System.Drawing.Point(6, 43);
            this.labelSummary.Name = "labelSummary";
            this.labelSummary.Size = new System.Drawing.Size(50, 13);
            this.labelSummary.TabIndex = 2;
            this.labelSummary.Text = "Summary";
            // 
            // hintTextBoxProgramName
            // 
            this.hintTextBoxProgramName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxProgramName.HintText = "a short name to identify the interface (e.g. \"Foo\")";
            this.hintTextBoxProgramName.Location = new System.Drawing.Point(9, 20);
            this.hintTextBoxProgramName.Name = "hintTextBoxProgramName";
            this.hintTextBoxProgramName.Size = new System.Drawing.Size(471, 20);
            this.hintTextBoxProgramName.TabIndex = 1;
            // 
            // labelProgramName
            // 
            this.labelProgramName.AutoSize = true;
            this.labelProgramName.Location = new System.Drawing.Point(6, 4);
            this.labelProgramName.Name = "labelProgramName";
            this.labelProgramName.Size = new System.Drawing.Size(35, 13);
            this.labelProgramName.TabIndex = 0;
            this.labelProgramName.Text = "Name";
            // 
            // tabPageFeed
            // 
            this.tabPageFeed.Controls.Add(this.groupBoxFeedStructure);
            this.tabPageFeed.Location = new System.Drawing.Point(4, 22);
            this.tabPageFeed.Name = "tabPageFeed";
            this.tabPageFeed.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFeed.Size = new System.Drawing.Size(608, 509);
            this.tabPageFeed.TabIndex = 1;
            this.tabPageFeed.Text = "Feeds";
            this.tabPageFeed.UseVisualStyleBackColor = true;
            // 
            // groupBoxFeedStructure
            // 
            this.groupBoxFeedStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFeedStructure.Controls.Add(this.buttonAddRecipe);
            this.groupBoxFeedStructure.Controls.Add(this.buttonClearList);
            this.groupBoxFeedStructure.Controls.Add(this.buttonAddArchive);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddOverlayBinding);
            this.groupBoxFeedStructure.Controls.Add(this.treeViewFeedStructure);
            this.groupBoxFeedStructure.Controls.Add(this.btnRemoveFeedStructureObject);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddEnvironmentBinding);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddGroup);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddDependency);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddPackageImplementation);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddImplementation);
            this.groupBoxFeedStructure.Location = new System.Drawing.Point(6, 6);
            this.groupBoxFeedStructure.Name = "groupBoxFeedStructure";
            this.groupBoxFeedStructure.Size = new System.Drawing.Size(596, 497);
            this.groupBoxFeedStructure.TabIndex = 0;
            this.groupBoxFeedStructure.TabStop = false;
            this.groupBoxFeedStructure.Text = "Feed Structure";
            // 
            // buttonAddRecipe
            // 
            this.buttonAddRecipe.Enabled = false;
            this.buttonAddRecipe.Location = new System.Drawing.Point(458, 106);
            this.buttonAddRecipe.Name = "buttonAddRecipe";
            this.buttonAddRecipe.Size = new System.Drawing.Size(132, 23);
            this.buttonAddRecipe.TabIndex = 4;
            this.buttonAddRecipe.Text = "Recipe";
            this.buttonAddRecipe.UseVisualStyleBackColor = true;
            this.buttonAddRecipe.Click += new System.EventHandler(this.ButtonAddRecipeClick);
            // 
            // buttonClearList
            // 
            this.buttonClearList.Location = new System.Drawing.Point(458, 439);
            this.buttonClearList.Name = "buttonClearList";
            this.buttonClearList.Size = new System.Drawing.Size(131, 23);
            this.buttonClearList.TabIndex = 9;
            this.buttonClearList.Text = "Clear feed structure";
            this.buttonClearList.UseVisualStyleBackColor = true;
            this.buttonClearList.Click += new System.EventHandler(this.ButtonClearListClick);
            // 
            // buttonAddArchive
            // 
            this.buttonAddArchive.Enabled = false;
            this.buttonAddArchive.Location = new System.Drawing.Point(458, 77);
            this.buttonAddArchive.Name = "buttonAddArchive";
            this.buttonAddArchive.Size = new System.Drawing.Size(132, 23);
            this.buttonAddArchive.TabIndex = 3;
            this.buttonAddArchive.Text = "Archive";
            this.buttonAddArchive.UseVisualStyleBackColor = true;
            this.buttonAddArchive.Click += new System.EventHandler(this.ButtonAddArchiveClick);
            // 
            // btnAddOverlayBinding
            // 
            this.btnAddOverlayBinding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddOverlayBinding.Enabled = false;
            this.btnAddOverlayBinding.Location = new System.Drawing.Point(458, 222);
            this.btnAddOverlayBinding.Name = "btnAddOverlayBinding";
            this.btnAddOverlayBinding.Size = new System.Drawing.Size(132, 23);
            this.btnAddOverlayBinding.TabIndex = 8;
            this.btnAddOverlayBinding.Text = "Overlay binding";
            this.btnAddOverlayBinding.UseVisualStyleBackColor = true;
            this.btnAddOverlayBinding.Click += new System.EventHandler(this.BtnAddOverlayBindingClick);
            // 
            // treeViewFeedStructure
            // 
            this.treeViewFeedStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewFeedStructure.HideSelection = false;
            this.treeViewFeedStructure.Location = new System.Drawing.Point(6, 19);
            this.treeViewFeedStructure.Name = "treeViewFeedStructure";
            treeNode1.Name = "interface";
            treeNode1.Tag = "";
            treeNode1.Text = "Interface";
            this.treeViewFeedStructure.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewFeedStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.treeViewFeedStructure.ShowRootLines = false;
            this.treeViewFeedStructure.Size = new System.Drawing.Size(446, 472);
            this.treeViewFeedStructure.TabIndex = 0;
            this.treeViewFeedStructure.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewFeedStructureAfterSelect);
            this.treeViewFeedStructure.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewFeedStructureNodeMouseDoubleClick);
            // 
            // btnRemoveFeedStructureObject
            // 
            this.btnRemoveFeedStructureObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveFeedStructureObject.Location = new System.Drawing.Point(458, 468);
            this.btnRemoveFeedStructureObject.Name = "btnRemoveFeedStructureObject";
            this.btnRemoveFeedStructureObject.Size = new System.Drawing.Size(132, 23);
            this.btnRemoveFeedStructureObject.TabIndex = 10;
            this.btnRemoveFeedStructureObject.Text = "Remove item";
            this.btnRemoveFeedStructureObject.UseVisualStyleBackColor = true;
            this.btnRemoveFeedStructureObject.Click += new System.EventHandler(this.BtnRemoveFeedStructureObjectClick);
            // 
            // btnAddEnvironmentBinding
            // 
            this.btnAddEnvironmentBinding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddEnvironmentBinding.Enabled = false;
            this.btnAddEnvironmentBinding.Location = new System.Drawing.Point(458, 193);
            this.btnAddEnvironmentBinding.Name = "btnAddEnvironmentBinding";
            this.btnAddEnvironmentBinding.Size = new System.Drawing.Size(132, 23);
            this.btnAddEnvironmentBinding.TabIndex = 7;
            this.btnAddEnvironmentBinding.Text = "Environment binding";
            this.btnAddEnvironmentBinding.UseVisualStyleBackColor = true;
            this.btnAddEnvironmentBinding.Click += new System.EventHandler(this.BtnAddEnvironmentBindingClick);
            // 
            // btnAddGroup
            // 
            this.btnAddGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddGroup.Location = new System.Drawing.Point(458, 19);
            this.btnAddGroup.Name = "btnAddGroup";
            this.btnAddGroup.Size = new System.Drawing.Size(132, 23);
            this.btnAddGroup.TabIndex = 1;
            this.btnAddGroup.Text = "Group";
            this.btnAddGroup.UseVisualStyleBackColor = true;
            this.btnAddGroup.Click += new System.EventHandler(this.BtnAddGroupClick);
            // 
            // btnAddDependency
            // 
            this.btnAddDependency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddDependency.Enabled = false;
            this.btnAddDependency.Location = new System.Drawing.Point(458, 164);
            this.btnAddDependency.Name = "btnAddDependency";
            this.btnAddDependency.Size = new System.Drawing.Size(132, 23);
            this.btnAddDependency.TabIndex = 6;
            this.btnAddDependency.Text = "Dependency";
            this.btnAddDependency.UseVisualStyleBackColor = true;
            this.btnAddDependency.Click += new System.EventHandler(this.BtnAddDependencyClick);
            // 
            // btnAddPackageImplementation
            // 
            this.btnAddPackageImplementation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddPackageImplementation.Enabled = false;
            this.btnAddPackageImplementation.Location = new System.Drawing.Point(458, 135);
            this.btnAddPackageImplementation.Name = "btnAddPackageImplementation";
            this.btnAddPackageImplementation.Size = new System.Drawing.Size(132, 23);
            this.btnAddPackageImplementation.TabIndex = 5;
            this.btnAddPackageImplementation.Text = "Package implementation";
            this.btnAddPackageImplementation.UseVisualStyleBackColor = true;
            this.btnAddPackageImplementation.Click += new System.EventHandler(this.BtnAddPackageImplementationClick);
            // 
            // btnAddImplementation
            // 
            this.btnAddImplementation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddImplementation.Location = new System.Drawing.Point(458, 48);
            this.btnAddImplementation.Name = "btnAddImplementation";
            this.btnAddImplementation.Size = new System.Drawing.Size(132, 23);
            this.btnAddImplementation.TabIndex = 2;
            this.btnAddImplementation.Text = "Implementation";
            this.btnAddImplementation.UseVisualStyleBackColor = true;
            this.btnAddImplementation.Click += new System.EventHandler(this.BtnAddImplementationClick);
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.Controls.Add(this.groupBoxFeedFor);
            this.tabPageAdvanced.Controls.Add(this.comboBoxMinInjectorVersion);
            this.tabPageAdvanced.Controls.Add(this.labelMinInjectorVersion);
            this.tabPageAdvanced.Controls.Add(this.groupBoxExternalFeeds);
            this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.Size = new System.Drawing.Size(608, 509);
            this.tabPageAdvanced.TabIndex = 2;
            this.tabPageAdvanced.Text = "Advanced";
            this.tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // groupBoxFeedFor
            // 
            this.groupBoxFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFeedFor.Controls.Add(this.buttonClearFeedFor);
            this.groupBoxFeedFor.Controls.Add(this.buttonRemoveFeedFor);
            this.groupBoxFeedFor.Controls.Add(this.buttonAddFeedFor);
            this.groupBoxFeedFor.Controls.Add(this.listBoxFeedFor);
            this.groupBoxFeedFor.Controls.Add(this.hintTextBoxFeedFor);
            this.groupBoxFeedFor.Location = new System.Drawing.Point(6, 323);
            this.groupBoxFeedFor.Name = "groupBoxFeedFor";
            this.groupBoxFeedFor.Size = new System.Drawing.Size(594, 137);
            this.groupBoxFeedFor.TabIndex = 1;
            this.groupBoxFeedFor.TabStop = false;
            this.groupBoxFeedFor.Text = "Feed For Interface";
            // 
            // buttonClearFeedFor
            // 
            this.buttonClearFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearFeedFor.Location = new System.Drawing.Point(513, 103);
            this.buttonClearFeedFor.Name = "buttonClearFeedFor";
            this.buttonClearFeedFor.Size = new System.Drawing.Size(75, 23);
            this.buttonClearFeedFor.TabIndex = 6;
            this.buttonClearFeedFor.Text = "Clear list";
            this.buttonClearFeedFor.UseVisualStyleBackColor = true;
            this.buttonClearFeedFor.Click += new System.EventHandler(this.btnFeedForClear_Click);
            // 
            // buttonRemoveFeedFor
            // 
            this.buttonRemoveFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveFeedFor.Location = new System.Drawing.Point(513, 74);
            this.buttonRemoveFeedFor.Name = "buttonRemoveFeedFor";
            this.buttonRemoveFeedFor.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveFeedFor.TabIndex = 5;
            this.buttonRemoveFeedFor.Text = "Remove";
            this.buttonRemoveFeedFor.UseVisualStyleBackColor = true;
            this.buttonRemoveFeedFor.Click += new System.EventHandler(this.btnFeedForRemove_Click);
            // 
            // buttonAddFeedFor
            // 
            this.buttonAddFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddFeedFor.Location = new System.Drawing.Point(513, 45);
            this.buttonAddFeedFor.Name = "buttonAddFeedFor";
            this.buttonAddFeedFor.Size = new System.Drawing.Size(75, 23);
            this.buttonAddFeedFor.TabIndex = 4;
            this.buttonAddFeedFor.Text = "Add";
            this.buttonAddFeedFor.UseVisualStyleBackColor = true;
            this.buttonAddFeedFor.Click += new System.EventHandler(this.btnFeedForAdd_Click);
            // 
            // listBoxFeedFor
            // 
            this.listBoxFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxFeedFor.FormattingEnabled = true;
            this.listBoxFeedFor.Location = new System.Drawing.Point(7, 46);
            this.listBoxFeedFor.Name = "listBoxFeedFor";
            this.listBoxFeedFor.Size = new System.Drawing.Size(500, 82);
            this.listBoxFeedFor.TabIndex = 3;
            // 
            // hintTextBoxFeedFor
            // 
            this.hintTextBoxFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxFeedFor.ClearButton = true;
            this.hintTextBoxFeedFor.HintText = "URL to an Interface";
            this.hintTextBoxFeedFor.Location = new System.Drawing.Point(6, 19);
            this.hintTextBoxFeedFor.Name = "hintTextBoxFeedFor";
            this.hintTextBoxFeedFor.Size = new System.Drawing.Size(582, 20);
            this.hintTextBoxFeedFor.TabIndex = 2;
            // 
            // comboBoxMinInjectorVersion
            // 
            this.comboBoxMinInjectorVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMinInjectorVersion.FormattingEnabled = true;
            this.comboBoxMinInjectorVersion.Items.AddRange(new object[] {
            "",
            "0.31",
            "0.32",
            "0.33",
            "0.34",
            "0.35",
            "0.36",
            "0.37",
            "0.38",
            "0.39",
            "0.40",
            "0.41",
            "0.41.1",
            "0.42",
            "0.42.1",
            "0.43",
            "0.44",
            "0.45"});
            this.comboBoxMinInjectorVersion.Location = new System.Drawing.Point(6, 479);
            this.comboBoxMinInjectorVersion.Name = "comboBoxMinInjectorVersion";
            this.comboBoxMinInjectorVersion.Size = new System.Drawing.Size(93, 21);
            this.comboBoxMinInjectorVersion.Sorted = true;
            this.comboBoxMinInjectorVersion.TabIndex = 3;
            // 
            // labelMinInjectorVersion
            // 
            this.labelMinInjectorVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelMinInjectorVersion.AutoSize = true;
            this.labelMinInjectorVersion.Location = new System.Drawing.Point(3, 463);
            this.labelMinInjectorVersion.Name = "labelMinInjectorVersion";
            this.labelMinInjectorVersion.Size = new System.Drawing.Size(102, 13);
            this.labelMinInjectorVersion.TabIndex = 2;
            this.labelMinInjectorVersion.Text = "min. Injector Version";
            // 
            // groupBoxExternalFeeds
            // 
            this.groupBoxExternalFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxExternalFeeds.Controls.Add(this.buttonUpdateExternalFeed);
            this.groupBoxExternalFeeds.Controls.Add(this.groupBoxSelectedFeed);
            this.groupBoxExternalFeeds.Controls.Add(this.listBoxExternalFeeds);
            this.groupBoxExternalFeeds.Controls.Add(this.buttonAddExternalFeeds);
            this.groupBoxExternalFeeds.Controls.Add(this.buttonRemoveExternalFeed);
            this.groupBoxExternalFeeds.Location = new System.Drawing.Point(6, 6);
            this.groupBoxExternalFeeds.Name = "groupBoxExternalFeeds";
            this.groupBoxExternalFeeds.Size = new System.Drawing.Size(594, 311);
            this.groupBoxExternalFeeds.TabIndex = 0;
            this.groupBoxExternalFeeds.TabStop = false;
            this.groupBoxExternalFeeds.Text = "External Feeds";
            // 
            // buttonUpdateExternalFeed
            // 
            this.buttonUpdateExternalFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdateExternalFeed.Location = new System.Drawing.Point(513, 48);
            this.buttonUpdateExternalFeed.Name = "buttonUpdateExternalFeed";
            this.buttonUpdateExternalFeed.Size = new System.Drawing.Size(75, 23);
            this.buttonUpdateExternalFeed.TabIndex = 2;
            this.buttonUpdateExternalFeed.Text = "Update";
            this.buttonUpdateExternalFeed.UseVisualStyleBackColor = true;
            this.buttonUpdateExternalFeed.Click += new System.EventHandler(this.btnExtFeedUpdate_Click);
            // 
            // groupBoxSelectedFeed
            // 
            this.groupBoxSelectedFeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSelectedFeed.Controls.Add(this.feedReferenceControl);
            this.groupBoxSelectedFeed.Location = new System.Drawing.Point(6, 114);
            this.groupBoxSelectedFeed.Name = "groupBoxSelectedFeed";
            this.groupBoxSelectedFeed.Size = new System.Drawing.Size(582, 191);
            this.groupBoxSelectedFeed.TabIndex = 4;
            this.groupBoxSelectedFeed.TabStop = false;
            this.groupBoxSelectedFeed.Text = "Selected Feed";
            // 
            // listBoxExternalFeeds
            // 
            this.listBoxExternalFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxExternalFeeds.FormattingEnabled = true;
            this.listBoxExternalFeeds.HorizontalScrollbar = true;
            this.listBoxExternalFeeds.Location = new System.Drawing.Point(6, 19);
            this.listBoxExternalFeeds.Name = "listBoxExternalFeeds";
            this.listBoxExternalFeeds.Size = new System.Drawing.Size(501, 82);
            this.listBoxExternalFeeds.TabIndex = 0;
            this.listBoxExternalFeeds.SelectedIndexChanged += new System.EventHandler(this.listBoxExtFeeds_SelectedIndexChanged);
            // 
            // buttonAddExternalFeeds
            // 
            this.buttonAddExternalFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddExternalFeeds.Location = new System.Drawing.Point(513, 19);
            this.buttonAddExternalFeeds.Name = "buttonAddExternalFeeds";
            this.buttonAddExternalFeeds.Size = new System.Drawing.Size(75, 23);
            this.buttonAddExternalFeeds.TabIndex = 1;
            this.buttonAddExternalFeeds.Text = "Add";
            this.buttonAddExternalFeeds.UseVisualStyleBackColor = true;
            this.buttonAddExternalFeeds.Click += new System.EventHandler(this.btnExtFeedsAdd_Click);
            // 
            // buttonRemoveExternalFeed
            // 
            this.buttonRemoveExternalFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveExternalFeed.Location = new System.Drawing.Point(513, 77);
            this.buttonRemoveExternalFeed.Name = "buttonRemoveExternalFeed";
            this.buttonRemoveExternalFeed.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveExternalFeed.TabIndex = 3;
            this.buttonRemoveExternalFeed.Text = "Remove";
            this.buttonRemoveExternalFeed.UseVisualStyleBackColor = true;
            this.buttonRemoveExternalFeed.Click += new System.EventHandler(this.btnExtFeedsRemove_Click);
            // 
            // feedReferenceControl
            // 
            this.feedReferenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            feedReference1.Architecture = new ZeroInstall.Model.Architecture(ZeroInstall.Model.OS.All, ZeroInstall.Model.Cpu.All);
            feedReference1.Source = null;
            this.feedReferenceControl.FeedReference = feedReference1;
            this.feedReferenceControl.Location = new System.Drawing.Point(6, 20);
            this.feedReferenceControl.Name = "feedReferenceControl";
            this.feedReferenceControl.Size = new System.Drawing.Size(570, 171);
            this.feedReferenceControl.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(633, 578);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.toolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(580, 568);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zero Install Feed Editor";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.tabControlMain.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.tabPageGeneral.PerformLayout();
            this.groupBoxIcon.ResumeLayout(false);
            this.groupBoxIcon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIconPreview)).EndInit();
            this.tabPageFeed.ResumeLayout(false);
            this.groupBoxFeedStructure.ResumeLayout(false);
            this.tabPageAdvanced.ResumeLayout(false);
            this.tabPageAdvanced.PerformLayout();
            this.groupBoxFeedFor.ResumeLayout(false);
            this.groupBoxFeedFor.PerformLayout();
            this.groupBoxExternalFeeds.ResumeLayout(false);
            this.groupBoxSelectedFeed.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ToolStripButton toolStripButtonOpen;
        private System.Windows.Forms.ToolStripButton toolStripButtonSave;
        private System.Windows.Forms.ToolStripButton toolStripButtonNew;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageFeed;
        private System.Windows.Forms.Label labelProgramName;
        private System.Windows.Forms.Label labelSummary;
        private Common.Controls.HintTextBox hintTextBoxProgramName;
        private Common.Controls.HintTextBox hintTextBoxSummary;
        private Common.Controls.HintTextBox hintTextBoxIconUrl;
        private System.Windows.Forms.Label labelIconUrl;
        private System.Windows.Forms.Label labelCategories;
        private System.Windows.Forms.Button buttonIconPreview;
        private System.Windows.Forms.PictureBox pictureBoxIconPreview;
        private System.Windows.Forms.Label lblIconUrlError;
        private System.Windows.Forms.Button buttonIconRemove;
        private System.Windows.Forms.Button buttonIconAdd;
        private System.Windows.Forms.ListBox listBoxIconsUrls;
        private System.Windows.Forms.Label labelIconMimeType;
        private System.Windows.Forms.ComboBox comboBoxIconType;
        private System.Windows.Forms.GroupBox groupBoxIcon;
        private System.Windows.Forms.Label labelDescription;
        private Common.Controls.HintTextBox hintTextBoxDescription;
        private Common.Controls.HintTextBox hintTextBoxHomepage;
        private System.Windows.Forms.Label labelHomepage;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        private System.Windows.Forms.CheckedListBox checkedListBoxCategories;

        private static Image GetImageFromUrl(Uri url)
        {
            var fileRequest = (HttpWebRequest) WebRequest.Create(url);
            var fileReponse = (HttpWebResponse) fileRequest.GetResponse();
            Stream stream = fileReponse.GetResponseStream();
            return Image.FromStream(stream);
        }

        private Common.Controls.HintTextBox hintTextBoxInterfaceUrl;
        private System.Windows.Forms.Label labelInterfaceUrl;
        private System.Windows.Forms.GroupBox groupBoxExternalFeeds;
        private System.Windows.Forms.CheckBox checkBoxNeedsTerminal;
        private System.Windows.Forms.Button buttonRemoveExternalFeed;
        private System.Windows.Forms.Button buttonAddExternalFeeds;
        private System.Windows.Forms.ListBox listBoxExternalFeeds;
        private System.Windows.Forms.ComboBox comboBoxMinInjectorVersion;
        private System.Windows.Forms.Label labelMinInjectorVersion;
        private Common.Controls.HintTextBox hintTextBoxFeedFor;
        private System.Windows.Forms.GroupBox groupBoxSelectedFeed;
        private System.Windows.Forms.GroupBox groupBoxFeedFor;
        private System.Windows.Forms.Button buttonClearFeedFor;
        private System.Windows.Forms.Button buttonRemoveFeedFor;
        private System.Windows.Forms.Button buttonAddFeedFor;
        private System.Windows.Forms.ListBox listBoxFeedFor;
        private System.Windows.Forms.Button btnRemoveFeedStructureObject;
        private System.Windows.Forms.Button btnAddImplementation;
        private System.Windows.Forms.Button btnAddGroup;
        private System.Windows.Forms.Button btnAddEnvironmentBinding;
        private System.Windows.Forms.Button btnAddDependency;
        private System.Windows.Forms.Button btnAddPackageImplementation;
        private System.Windows.Forms.GroupBox groupBoxFeedStructure;
        private System.Windows.Forms.Button btnAddOverlayBinding;
        private FeedReferenceControl feedReferenceControl;
        private System.Windows.Forms.Button buttonUpdateExternalFeed;
        private System.Windows.Forms.Button buttonClearList;
        private System.Windows.Forms.Button buttonAddArchive;
        private System.Windows.Forms.Button buttonAddRecipe;
        private System.Windows.Forms.TreeView treeViewFeedStructure;
    }
}

