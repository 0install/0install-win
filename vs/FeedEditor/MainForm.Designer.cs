using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace ZeroInstall.FeedEditor
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
            ZeroInstall.Model.Group group1 = new ZeroInstall.Model.Group();
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
            this.textInterfaceURL = new Common.Controls.HintTextBox();
            this.lblInterfaceURL = new System.Windows.Forms.Label();
            this.checkedListCategory = new System.Windows.Forms.CheckedListBox();
            this.textHomepage = new Common.Controls.HintTextBox();
            this.textDescription = new Common.Controls.HintTextBox();
            this.lblHomepage = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.groupBoxIcon = new System.Windows.Forms.GroupBox();
            this.lblIcon = new System.Windows.Forms.Label();
            this.comboIconType = new System.Windows.Forms.ComboBox();
            this.textIconUrl = new Common.Controls.HintTextBox();
            this.lblIconUrlError = new System.Windows.Forms.Label();
            this.lblIconMime = new System.Windows.Forms.Label();
            this.btnIconPreview = new System.Windows.Forms.Button();
            this.btnIconRemove = new System.Windows.Forms.Button();
            this.iconBox = new System.Windows.Forms.PictureBox();
            this.btnIconAdd = new System.Windows.Forms.Button();
            this.listIconsUrls = new System.Windows.Forms.ListBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.textSummary = new Common.Controls.HintTextBox();
            this.lblSummary = new System.Windows.Forms.Label();
            this.textName = new Common.Controls.HintTextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.tabPageFeed = new System.Windows.Forms.TabPage();
            this.groupBoxGroupSettings = new System.Windows.Forms.GroupBox();
            this.groupControl = new ZeroInstall.FeedEditor.GroupControl();
            this.groupBoxFeedStructure = new System.Windows.Forms.GroupBox();
            this.btnAddOverlayBinding = new System.Windows.Forms.Button();
            this.treeViewFeedStructure = new System.Windows.Forms.TreeView();
            this.btnRemoveFeedStructureObject = new System.Windows.Forms.Button();
            this.btnAddEnvironmentBinding = new System.Windows.Forms.Button();
            this.btnAddGroup = new System.Windows.Forms.Button();
            this.btnAddDependency = new System.Windows.Forms.Button();
            this.btnAddPackageImplementation = new System.Windows.Forms.Button();
            this.btnAddImplementation = new System.Windows.Forms.Button();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.groupBoxFeedFor = new System.Windows.Forms.GroupBox();
            this.btnFeedForClear = new System.Windows.Forms.Button();
            this.btnFeedForRemove = new System.Windows.Forms.Button();
            this.btnFeedForAdd = new System.Windows.Forms.Button();
            this.listBoxFeedFor = new System.Windows.Forms.ListBox();
            this.textFeedFor = new Common.Controls.HintTextBox();
            this.comboBoxMinInjectorVersion = new System.Windows.Forms.ComboBox();
            this.lblMinInjectorVersion = new System.Windows.Forms.Label();
            this.groupBoxExternalFeed = new System.Windows.Forms.GroupBox();
            this.btnExtFeedUpdate = new System.Windows.Forms.Button();
            this.groupBoxSelectedFeed = new System.Windows.Forms.GroupBox();
            this.feedReferenceControl = new ZeroInstall.FeedEditor.FeedReferenceControl();
            this.listBoxExtFeeds = new System.Windows.Forms.ListBox();
            this.btnExtFeedsAdd = new System.Windows.Forms.Button();
            this.btnExtFeedsRemove = new System.Windows.Forms.Button();
            this.toolStrip.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.groupBoxIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
            this.tabPageFeed.SuspendLayout();
            this.groupBoxGroupSettings.SuspendLayout();
            this.groupBoxFeedStructure.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.groupBoxFeedFor.SuspendLayout();
            this.groupBoxExternalFeed.SuspendLayout();
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
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip.Size = new System.Drawing.Size(619, 25);
            this.toolStrip.TabIndex = 0;
            // 
            // toolStripButtonNew
            // 
            this.toolStripButtonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonNew.Image = global::ZeroInstall.FeedEditor.Properties.Resources.NewButton;
            this.toolStripButtonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonNew.Name = "toolStripButtonNew";
            this.toolStripButtonNew.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonNew.Text = "New";
            this.toolStripButtonNew.Click += new System.EventHandler(this.ToolStripButtonNew_Click);
            // 
            // toolStripButtonOpen
            // 
            this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOpen.Image = global::ZeroInstall.FeedEditor.Properties.Resources.OpenButton;
            this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonOpen.Text = "Open";
            this.toolStripButtonOpen.Click += new System.EventHandler(this.ToolStripButtonOpen_Click);
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSave.Image = global::ZeroInstall.FeedEditor.Properties.Resources.SaveButton;
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
            this.tabControlMain.Controls.Add(this.tabAdvanced);
            this.tabControlMain.Location = new System.Drawing.Point(7, 34);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(616, 562);
            this.tabControlMain.TabIndex = 1;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.checkBoxNeedsTerminal);
            this.tabPageGeneral.Controls.Add(this.textInterfaceURL);
            this.tabPageGeneral.Controls.Add(this.lblInterfaceURL);
            this.tabPageGeneral.Controls.Add(this.checkedListCategory);
            this.tabPageGeneral.Controls.Add(this.textHomepage);
            this.tabPageGeneral.Controls.Add(this.textDescription);
            this.tabPageGeneral.Controls.Add(this.lblHomepage);
            this.tabPageGeneral.Controls.Add(this.lblDescription);
            this.tabPageGeneral.Controls.Add(this.groupBoxIcon);
            this.tabPageGeneral.Controls.Add(this.lblCategory);
            this.tabPageGeneral.Controls.Add(this.textSummary);
            this.tabPageGeneral.Controls.Add(this.lblSummary);
            this.tabPageGeneral.Controls.Add(this.textName);
            this.tabPageGeneral.Controls.Add(this.lblName);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(608, 536);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // checkBoxNeedsTerminal
            // 
            this.checkBoxNeedsTerminal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxNeedsTerminal.AutoSize = true;
            this.checkBoxNeedsTerminal.Location = new System.Drawing.Point(9, 513);
            this.checkBoxNeedsTerminal.Name = "checkBoxNeedsTerminal";
            this.checkBoxNeedsTerminal.Size = new System.Drawing.Size(98, 17);
            this.checkBoxNeedsTerminal.TabIndex = 13;
            this.checkBoxNeedsTerminal.Text = "needs Terminal";
            this.checkBoxNeedsTerminal.UseVisualStyleBackColor = true;
            // 
            // textInterfaceURL
            // 
            this.textInterfaceURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textInterfaceURL.HintText = "URL to a remote interface";
            this.textInterfaceURL.Location = new System.Drawing.Point(9, 107);
            this.textInterfaceURL.Name = "textInterfaceURL";
            this.textInterfaceURL.Size = new System.Drawing.Size(588, 20);
            this.textInterfaceURL.TabIndex = 7;
            this.textInterfaceURL.TextChanged += new System.EventHandler(this.textInterfaceURL_TextChanged);
            // 
            // lblInterfaceURL
            // 
            this.lblInterfaceURL.AutoSize = true;
            this.lblInterfaceURL.Location = new System.Drawing.Point(6, 91);
            this.lblInterfaceURL.Name = "lblInterfaceURL";
            this.lblInterfaceURL.Size = new System.Drawing.Size(74, 13);
            this.lblInterfaceURL.TabIndex = 6;
            this.lblInterfaceURL.Text = "Interface URL";
            // 
            // checkedListCategory
            // 
            this.checkedListCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListCategory.FormattingEnabled = true;
            this.checkedListCategory.Items.AddRange(new object[] {
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
            this.checkedListCategory.Location = new System.Drawing.Point(481, 20);
            this.checkedListCategory.Name = "checkedListCategory";
            this.checkedListCategory.Size = new System.Drawing.Size(116, 79);
            this.checkedListCategory.Sorted = true;
            this.checkedListCategory.TabIndex = 5;
            // 
            // textHomepage
            // 
            this.textHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textHomepage.HintText = "the URL of a web-page describing this interface in more detail";
            this.textHomepage.Location = new System.Drawing.Point(9, 478);
            this.textHomepage.Name = "textHomepage";
            this.textHomepage.Size = new System.Drawing.Size(588, 20);
            this.textHomepage.TabIndex = 12;
            this.textHomepage.TextChanged += new System.EventHandler(this.textHomepage_TextChanged);
            // 
            // textDescription
            // 
            this.textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textDescription.HintText = "a full description, which can be several paragraphs long";
            this.textDescription.Location = new System.Drawing.Point(9, 155);
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(588, 109);
            this.textDescription.TabIndex = 9;
            // 
            // lblHomepage
            // 
            this.lblHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHomepage.AutoSize = true;
            this.lblHomepage.Location = new System.Drawing.Point(6, 462);
            this.lblHomepage.Name = "lblHomepage";
            this.lblHomepage.Size = new System.Drawing.Size(59, 13);
            this.lblHomepage.TabIndex = 11;
            this.lblHomepage.Text = "Homepage";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(6, 139);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(60, 13);
            this.lblDescription.TabIndex = 8;
            this.lblDescription.Text = "Description";
            // 
            // groupBoxIcon
            // 
            this.groupBoxIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxIcon.Controls.Add(this.lblIcon);
            this.groupBoxIcon.Controls.Add(this.comboIconType);
            this.groupBoxIcon.Controls.Add(this.textIconUrl);
            this.groupBoxIcon.Controls.Add(this.lblIconUrlError);
            this.groupBoxIcon.Controls.Add(this.lblIconMime);
            this.groupBoxIcon.Controls.Add(this.btnIconPreview);
            this.groupBoxIcon.Controls.Add(this.btnIconRemove);
            this.groupBoxIcon.Controls.Add(this.iconBox);
            this.groupBoxIcon.Controls.Add(this.btnIconAdd);
            this.groupBoxIcon.Controls.Add(this.listIconsUrls);
            this.groupBoxIcon.Location = new System.Drawing.Point(9, 279);
            this.groupBoxIcon.Name = "groupBoxIcon";
            this.groupBoxIcon.Size = new System.Drawing.Size(588, 171);
            this.groupBoxIcon.TabIndex = 10;
            this.groupBoxIcon.TabStop = false;
            this.groupBoxIcon.Text = "Icon";
            // 
            // lblIcon
            // 
            this.lblIcon.AutoSize = true;
            this.lblIcon.Location = new System.Drawing.Point(6, 16);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(42, 13);
            this.lblIcon.TabIndex = 0;
            this.lblIcon.Text = "Icon url";
            // 
            // comboIconType
            // 
            this.comboIconType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboIconType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboIconType.FormattingEnabled = true;
            this.comboIconType.Items.AddRange(new object[] {
            "PNG",
            "ICO"});
            this.comboIconType.Location = new System.Drawing.Point(376, 32);
            this.comboIconType.Name = "comboIconType";
            this.comboIconType.Size = new System.Drawing.Size(76, 21);
            this.comboIconType.TabIndex = 3;
            // 
            // textIconUrl
            // 
            this.textIconUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textIconUrl.HintText = "";
            this.textIconUrl.Location = new System.Drawing.Point(9, 32);
            this.textIconUrl.Name = "textIconUrl";
            this.textIconUrl.Size = new System.Drawing.Size(361, 20);
            this.textIconUrl.TabIndex = 1;
            this.textIconUrl.TextChanged += new System.EventHandler(this.textIconUrl_TextChanged);
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
            // lblIconMime
            // 
            this.lblIconMime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIconMime.AutoSize = true;
            this.lblIconMime.Location = new System.Drawing.Point(373, 16);
            this.lblIconMime.Name = "lblIconMime";
            this.lblIconMime.Size = new System.Drawing.Size(55, 13);
            this.lblIconMime.TabIndex = 2;
            this.lblIconMime.Text = "Icon Type";
            // 
            // btnIconPreview
            // 
            this.btnIconPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIconPreview.Location = new System.Drawing.Point(374, 59);
            this.btnIconPreview.Name = "btnIconPreview";
            this.btnIconPreview.Size = new System.Drawing.Size(78, 23);
            this.btnIconPreview.TabIndex = 6;
            this.btnIconPreview.Text = "Icon Preview";
            this.btnIconPreview.UseVisualStyleBackColor = true;
            this.btnIconPreview.Click += new System.EventHandler(this.BtnIconPreviewClick);
            // 
            // btnIconRemove
            // 
            this.btnIconRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIconRemove.Location = new System.Drawing.Point(374, 117);
            this.btnIconRemove.Name = "btnIconRemove";
            this.btnIconRemove.Size = new System.Drawing.Size(78, 23);
            this.btnIconRemove.TabIndex = 8;
            this.btnIconRemove.Text = "Remove";
            this.btnIconRemove.UseVisualStyleBackColor = true;
            this.btnIconRemove.Click += new System.EventHandler(this.btnIconListRemove_Click);
            // 
            // iconBox
            // 
            this.iconBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.iconBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iconBox.Location = new System.Drawing.Point(458, 21);
            this.iconBox.Name = "iconBox";
            this.iconBox.Size = new System.Drawing.Size(120, 120);
            this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iconBox.TabIndex = 9;
            this.iconBox.TabStop = false;
            // 
            // btnIconAdd
            // 
            this.btnIconAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIconAdd.Location = new System.Drawing.Point(374, 88);
            this.btnIconAdd.Name = "btnIconAdd";
            this.btnIconAdd.Size = new System.Drawing.Size(78, 23);
            this.btnIconAdd.TabIndex = 7;
            this.btnIconAdd.Text = "Add";
            this.btnIconAdd.UseVisualStyleBackColor = true;
            this.btnIconAdd.Click += new System.EventHandler(this.btnIconListAdd_Click);
            // 
            // listIconsUrls
            // 
            this.listIconsUrls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listIconsUrls.FormattingEnabled = true;
            this.listIconsUrls.HorizontalScrollbar = true;
            this.listIconsUrls.Location = new System.Drawing.Point(9, 59);
            this.listIconsUrls.Name = "listIconsUrls";
            this.listIconsUrls.Size = new System.Drawing.Size(361, 82);
            this.listIconsUrls.TabIndex = 4;
            this.listIconsUrls.SelectedIndexChanged += new System.EventHandler(this.listIconsUrls_SelectedIndexChanged);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(478, 4);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(49, 13);
            this.lblCategory.TabIndex = 4;
            this.lblCategory.Text = "Category";
            // 
            // textSummary
            // 
            this.textSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textSummary.HintText = "a short one-line description";
            this.textSummary.Location = new System.Drawing.Point(9, 59);
            this.textSummary.Name = "textSummary";
            this.textSummary.Size = new System.Drawing.Size(466, 20);
            this.textSummary.TabIndex = 3;
            // 
            // lblSummary
            // 
            this.lblSummary.AutoSize = true;
            this.lblSummary.Location = new System.Drawing.Point(6, 43);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(50, 13);
            this.lblSummary.TabIndex = 2;
            this.lblSummary.Text = "Summary";
            // 
            // textName
            // 
            this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textName.HintText = "a short name to identify the interface (e.g. \"Foo\")";
            this.textName.Location = new System.Drawing.Point(9, 20);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(466, 20);
            this.textName.TabIndex = 1;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(6, 4);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(35, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Name";
            // 
            // tabPageFeed
            // 
            this.tabPageFeed.Controls.Add(this.groupBoxGroupSettings);
            this.tabPageFeed.Controls.Add(this.groupBoxFeedStructure);
            this.tabPageFeed.Location = new System.Drawing.Point(4, 22);
            this.tabPageFeed.Name = "tabPageFeed";
            this.tabPageFeed.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFeed.Size = new System.Drawing.Size(608, 536);
            this.tabPageFeed.TabIndex = 1;
            this.tabPageFeed.Text = "Feeds";
            this.tabPageFeed.UseVisualStyleBackColor = true;
            // 
            // groupBoxGroupSettings
            // 
            this.groupBoxGroupSettings.Controls.Add(this.groupControl);
            this.groupBoxGroupSettings.Location = new System.Drawing.Point(6, 241);
            this.groupBoxGroupSettings.Name = "groupBoxGroupSettings";
            this.groupBoxGroupSettings.Size = new System.Drawing.Size(596, 279);
            this.groupBoxGroupSettings.TabIndex = 1;
            this.groupBoxGroupSettings.TabStop = false;
            this.groupBoxGroupSettings.Text = "Group settings";
            // 
            // groupControl
            // 
            group1.Architecture = new ZeroInstall.Model.Architecture(ZeroInstall.Model.OS.All, ZeroInstall.Model.Cpu.All);
            group1.DocDir = null;
            group1.License = null;
            group1.Main = null;
            group1.Released = new System.DateTime(2010, 4, 19, 0, 0, 0, 0);
            group1.ReleasedString = "2010-04-19";
            group1.SelfTest = null;
            group1.Version = null;
            group1.VersionModifier = null;
            this.groupControl.Group = group1;
            this.groupControl.Location = new System.Drawing.Point(6, 18);
            this.groupControl.Name = "groupControl";
            this.groupControl.Size = new System.Drawing.Size(588, 261);
            this.groupControl.TabIndex = 0;
            // 
            // groupBoxFeedStructure
            // 
            this.groupBoxFeedStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
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
            this.groupBoxFeedStructure.Size = new System.Drawing.Size(596, 229);
            this.groupBoxFeedStructure.TabIndex = 0;
            this.groupBoxFeedStructure.TabStop = false;
            this.groupBoxFeedStructure.Text = "Feed Structure";
            // 
            // btnAddOverlayBinding
            // 
            this.btnAddOverlayBinding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddOverlayBinding.Enabled = false;
            this.btnAddOverlayBinding.Location = new System.Drawing.Point(459, 167);
            this.btnAddOverlayBinding.Name = "btnAddOverlayBinding";
            this.btnAddOverlayBinding.Size = new System.Drawing.Size(131, 23);
            this.btnAddOverlayBinding.TabIndex = 6;
            this.btnAddOverlayBinding.Text = "Overlay binding";
            this.btnAddOverlayBinding.UseVisualStyleBackColor = true;
            this.btnAddOverlayBinding.Click += new System.EventHandler(this.btnAddOverlayBinding_Click);
            // 
            // treeViewFeedStructure
            // 
            this.treeViewFeedStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewFeedStructure.HideSelection = false;
            this.treeViewFeedStructure.Location = new System.Drawing.Point(6, 19);
            this.treeViewFeedStructure.Name = "treeViewFeedStructure";
            treeNode1.Checked = true;
            treeNode1.Name = "interface";
            treeNode1.Text = "Interface";
            this.treeViewFeedStructure.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewFeedStructure.Size = new System.Drawing.Size(446, 200);
            this.treeViewFeedStructure.TabIndex = 0;
            this.treeViewFeedStructure.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewFeedStructure_AfterSelect);
            // 
            // btnRemoveFeedStructureObject
            // 
            this.btnRemoveFeedStructureObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveFeedStructureObject.Location = new System.Drawing.Point(458, 196);
            this.btnRemoveFeedStructureObject.Name = "btnRemoveFeedStructureObject";
            this.btnRemoveFeedStructureObject.Size = new System.Drawing.Size(132, 23);
            this.btnRemoveFeedStructureObject.TabIndex = 7;
            this.btnRemoveFeedStructureObject.Text = "Remove";
            this.btnRemoveFeedStructureObject.UseVisualStyleBackColor = true;
            this.btnRemoveFeedStructureObject.Click += new System.EventHandler(this.btnRemoveFeedStructureObject_Click);
            // 
            // btnAddEnvironmentBinding
            // 
            this.btnAddEnvironmentBinding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddEnvironmentBinding.Enabled = false;
            this.btnAddEnvironmentBinding.Location = new System.Drawing.Point(458, 137);
            this.btnAddEnvironmentBinding.Name = "btnAddEnvironmentBinding";
            this.btnAddEnvironmentBinding.Size = new System.Drawing.Size(132, 23);
            this.btnAddEnvironmentBinding.TabIndex = 5;
            this.btnAddEnvironmentBinding.Text = "Environment binding";
            this.btnAddEnvironmentBinding.UseVisualStyleBackColor = true;
            this.btnAddEnvironmentBinding.Click += new System.EventHandler(this.btnAddEnvironmentBinding_Click);
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
            this.btnAddGroup.Click += new System.EventHandler(this.btnAddGroup_Click);
            // 
            // btnAddDependency
            // 
            this.btnAddDependency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddDependency.Enabled = false;
            this.btnAddDependency.Location = new System.Drawing.Point(458, 108);
            this.btnAddDependency.Name = "btnAddDependency";
            this.btnAddDependency.Size = new System.Drawing.Size(132, 23);
            this.btnAddDependency.TabIndex = 4;
            this.btnAddDependency.Text = "Dependency";
            this.btnAddDependency.UseVisualStyleBackColor = true;
            this.btnAddDependency.Click += new System.EventHandler(this.btnAddDependency_Click);
            // 
            // btnAddPackageImplementation
            // 
            this.btnAddPackageImplementation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddPackageImplementation.Enabled = false;
            this.btnAddPackageImplementation.Location = new System.Drawing.Point(458, 78);
            this.btnAddPackageImplementation.Name = "btnAddPackageImplementation";
            this.btnAddPackageImplementation.Size = new System.Drawing.Size(132, 23);
            this.btnAddPackageImplementation.TabIndex = 3;
            this.btnAddPackageImplementation.Text = "Package implementation";
            this.btnAddPackageImplementation.UseVisualStyleBackColor = true;
            this.btnAddPackageImplementation.Click += new System.EventHandler(this.btnAddPackageImplementation_Click);
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
            this.btnAddImplementation.Click += new System.EventHandler(this.btnAddImplementation_Click);
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.groupBoxFeedFor);
            this.tabAdvanced.Controls.Add(this.comboBoxMinInjectorVersion);
            this.tabAdvanced.Controls.Add(this.lblMinInjectorVersion);
            this.tabAdvanced.Controls.Add(this.groupBoxExternalFeed);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Size = new System.Drawing.Size(608, 536);
            this.tabAdvanced.TabIndex = 2;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // groupBoxFeedFor
            // 
            this.groupBoxFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFeedFor.Controls.Add(this.btnFeedForClear);
            this.groupBoxFeedFor.Controls.Add(this.btnFeedForRemove);
            this.groupBoxFeedFor.Controls.Add(this.btnFeedForAdd);
            this.groupBoxFeedFor.Controls.Add(this.listBoxFeedFor);
            this.groupBoxFeedFor.Controls.Add(this.textFeedFor);
            this.groupBoxFeedFor.Location = new System.Drawing.Point(6, 350);
            this.groupBoxFeedFor.Name = "groupBoxFeedFor";
            this.groupBoxFeedFor.Size = new System.Drawing.Size(594, 137);
            this.groupBoxFeedFor.TabIndex = 1;
            this.groupBoxFeedFor.TabStop = false;
            this.groupBoxFeedFor.Text = "Feed For Interface";
            // 
            // btnFeedForClear
            // 
            this.btnFeedForClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFeedForClear.Location = new System.Drawing.Point(513, 103);
            this.btnFeedForClear.Name = "btnFeedForClear";
            this.btnFeedForClear.Size = new System.Drawing.Size(75, 23);
            this.btnFeedForClear.TabIndex = 6;
            this.btnFeedForClear.Text = "Clear list";
            this.btnFeedForClear.UseVisualStyleBackColor = true;
            this.btnFeedForClear.Click += new System.EventHandler(this.btnFeedForClear_Click);
            // 
            // btnFeedForRemove
            // 
            this.btnFeedForRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFeedForRemove.Location = new System.Drawing.Point(513, 74);
            this.btnFeedForRemove.Name = "btnFeedForRemove";
            this.btnFeedForRemove.Size = new System.Drawing.Size(75, 23);
            this.btnFeedForRemove.TabIndex = 5;
            this.btnFeedForRemove.Text = "Remove";
            this.btnFeedForRemove.UseVisualStyleBackColor = true;
            this.btnFeedForRemove.Click += new System.EventHandler(this.btnFeedForRemove_Click);
            // 
            // btnFeedForAdd
            // 
            this.btnFeedForAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFeedForAdd.Location = new System.Drawing.Point(513, 45);
            this.btnFeedForAdd.Name = "btnFeedForAdd";
            this.btnFeedForAdd.Size = new System.Drawing.Size(75, 23);
            this.btnFeedForAdd.TabIndex = 4;
            this.btnFeedForAdd.Text = "Add";
            this.btnFeedForAdd.UseVisualStyleBackColor = true;
            this.btnFeedForAdd.Click += new System.EventHandler(this.btnFeedForAdd_Click);
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
            // textFeedFor
            // 
            this.textFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textFeedFor.ClearButton = true;
            this.textFeedFor.HintText = "URL to an Interface";
            this.textFeedFor.Location = new System.Drawing.Point(6, 19);
            this.textFeedFor.Name = "textFeedFor";
            this.textFeedFor.Size = new System.Drawing.Size(582, 20);
            this.textFeedFor.TabIndex = 2;
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
            this.comboBoxMinInjectorVersion.Location = new System.Drawing.Point(6, 506);
            this.comboBoxMinInjectorVersion.Name = "comboBoxMinInjectorVersion";
            this.comboBoxMinInjectorVersion.Size = new System.Drawing.Size(93, 21);
            this.comboBoxMinInjectorVersion.Sorted = true;
            this.comboBoxMinInjectorVersion.TabIndex = 3;
            this.comboBoxMinInjectorVersion.SelectedIndexChanged += new System.EventHandler(this.comboBoxMinInjectorVersion_SelectedIndexChanged);
            // 
            // lblMinInjectorVersion
            // 
            this.lblMinInjectorVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMinInjectorVersion.AutoSize = true;
            this.lblMinInjectorVersion.Location = new System.Drawing.Point(3, 490);
            this.lblMinInjectorVersion.Name = "lblMinInjectorVersion";
            this.lblMinInjectorVersion.Size = new System.Drawing.Size(102, 13);
            this.lblMinInjectorVersion.TabIndex = 2;
            this.lblMinInjectorVersion.Text = "min. Injector Version";
            // 
            // groupBoxExternalFeed
            // 
            this.groupBoxExternalFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxExternalFeed.Controls.Add(this.btnExtFeedUpdate);
            this.groupBoxExternalFeed.Controls.Add(this.groupBoxSelectedFeed);
            this.groupBoxExternalFeed.Controls.Add(this.listBoxExtFeeds);
            this.groupBoxExternalFeed.Controls.Add(this.btnExtFeedsAdd);
            this.groupBoxExternalFeed.Controls.Add(this.btnExtFeedsRemove);
            this.groupBoxExternalFeed.Location = new System.Drawing.Point(6, 6);
            this.groupBoxExternalFeed.Name = "groupBoxExternalFeed";
            this.groupBoxExternalFeed.Size = new System.Drawing.Size(594, 338);
            this.groupBoxExternalFeed.TabIndex = 0;
            this.groupBoxExternalFeed.TabStop = false;
            this.groupBoxExternalFeed.Text = "External Feeds";
            // 
            // btnExtFeedUpdate
            // 
            this.btnExtFeedUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtFeedUpdate.Location = new System.Drawing.Point(513, 48);
            this.btnExtFeedUpdate.Name = "btnExtFeedUpdate";
            this.btnExtFeedUpdate.Size = new System.Drawing.Size(75, 23);
            this.btnExtFeedUpdate.TabIndex = 2;
            this.btnExtFeedUpdate.Text = "Update";
            this.btnExtFeedUpdate.UseVisualStyleBackColor = true;
            this.btnExtFeedUpdate.Click += new System.EventHandler(this.btnExtFeedUpdate_Click);
            // 
            // groupBoxSelectedFeed
            // 
            this.groupBoxSelectedFeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSelectedFeed.Controls.Add(this.feedReferenceControl);
            this.groupBoxSelectedFeed.Location = new System.Drawing.Point(6, 141);
            this.groupBoxSelectedFeed.Name = "groupBoxSelectedFeed";
            this.groupBoxSelectedFeed.Size = new System.Drawing.Size(582, 191);
            this.groupBoxSelectedFeed.TabIndex = 4;
            this.groupBoxSelectedFeed.TabStop = false;
            this.groupBoxSelectedFeed.Text = "Selected Feed";
            // 
            // feedReferenceControl
            // 
            this.feedReferenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            feedReference1.Architecture = new ZeroInstall.Model.Architecture(ZeroInstall.Model.OS.All, ZeroInstall.Model.Cpu.All);
            feedReference1.Target = null;
            feedReference1.TargetString = null;
            this.feedReferenceControl.FeedReference = feedReference1;
            this.feedReferenceControl.Location = new System.Drawing.Point(6, 20);
            this.feedReferenceControl.Name = "feedReferenceControl";
            this.feedReferenceControl.Size = new System.Drawing.Size(570, 171);
            this.feedReferenceControl.TabIndex = 0;
            // 
            // listBoxExtFeeds
            // 
            this.listBoxExtFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxExtFeeds.FormattingEnabled = true;
            this.listBoxExtFeeds.HorizontalScrollbar = true;
            this.listBoxExtFeeds.Location = new System.Drawing.Point(6, 19);
            this.listBoxExtFeeds.Name = "listBoxExtFeeds";
            this.listBoxExtFeeds.Size = new System.Drawing.Size(501, 108);
            this.listBoxExtFeeds.TabIndex = 0;
            this.listBoxExtFeeds.SelectedIndexChanged += new System.EventHandler(this.listBoxExtFeeds_SelectedIndexChanged);
            // 
            // btnExtFeedsAdd
            // 
            this.btnExtFeedsAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtFeedsAdd.Location = new System.Drawing.Point(513, 19);
            this.btnExtFeedsAdd.Name = "btnExtFeedsAdd";
            this.btnExtFeedsAdd.Size = new System.Drawing.Size(75, 23);
            this.btnExtFeedsAdd.TabIndex = 1;
            this.btnExtFeedsAdd.Text = "Add";
            this.btnExtFeedsAdd.UseVisualStyleBackColor = true;
            this.btnExtFeedsAdd.Click += new System.EventHandler(this.btnExtFeedsAdd_Click);
            // 
            // btnExtFeedsRemove
            // 
            this.btnExtFeedsRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtFeedsRemove.Location = new System.Drawing.Point(513, 77);
            this.btnExtFeedsRemove.Name = "btnExtFeedsRemove";
            this.btnExtFeedsRemove.Size = new System.Drawing.Size(75, 23);
            this.btnExtFeedsRemove.TabIndex = 3;
            this.btnExtFeedsRemove.Text = "Remove";
            this.btnExtFeedsRemove.UseVisualStyleBackColor = true;
            this.btnExtFeedsRemove.Click += new System.EventHandler(this.btnExtFeedsRemove_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(633, 605);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.toolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(580, 610);
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
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
            this.tabPageFeed.ResumeLayout(false);
            this.groupBoxGroupSettings.ResumeLayout(false);
            this.groupBoxFeedStructure.ResumeLayout(false);
            this.tabAdvanced.ResumeLayout(false);
            this.tabAdvanced.PerformLayout();
            this.groupBoxFeedFor.ResumeLayout(false);
            this.groupBoxFeedFor.PerformLayout();
            this.groupBoxExternalFeed.ResumeLayout(false);
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
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label lblSummary;
        private Common.Controls.HintTextBox textName;
        private Common.Controls.HintTextBox textSummary;
        private Common.Controls.HintTextBox textIconUrl;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Button btnIconPreview;
        private System.Windows.Forms.PictureBox iconBox;
        private System.Windows.Forms.Label lblIconUrlError;
        private System.Windows.Forms.Button btnIconRemove;
        private System.Windows.Forms.Button btnIconAdd;
        private System.Windows.Forms.ListBox listIconsUrls;
        private System.Windows.Forms.Label lblIconMime;
        private System.Windows.Forms.ComboBox comboIconType;
        private System.Windows.Forms.GroupBox groupBoxIcon;
        private System.Windows.Forms.Label lblDescription;
        private Common.Controls.HintTextBox textDescription;
        private Common.Controls.HintTextBox textHomepage;
        private System.Windows.Forms.Label lblHomepage;
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.CheckedListBox checkedListCategory;

        private static Image GetImageFromUrl(Uri url)
        {
            var fileRequest = (HttpWebRequest) WebRequest.Create(url);
            var fileReponse = (HttpWebResponse) fileRequest.GetResponse();
            Stream stream = fileReponse.GetResponseStream();
            return Image.FromStream(stream);
        }

        private Common.Controls.HintTextBox textInterfaceURL;
        private System.Windows.Forms.Label lblInterfaceURL;
        private System.Windows.Forms.GroupBox groupBoxExternalFeed;
        private System.Windows.Forms.CheckBox checkBoxNeedsTerminal;
        private System.Windows.Forms.Button btnExtFeedsRemove;
        private System.Windows.Forms.Button btnExtFeedsAdd;
        private System.Windows.Forms.ListBox listBoxExtFeeds;
        private System.Windows.Forms.ComboBox comboBoxMinInjectorVersion;
        private System.Windows.Forms.Label lblMinInjectorVersion;
        private Common.Controls.HintTextBox textFeedFor;
        private System.Windows.Forms.GroupBox groupBoxSelectedFeed;
        private System.Windows.Forms.GroupBox groupBoxFeedFor;
        private System.Windows.Forms.Button btnFeedForClear;
        private System.Windows.Forms.Button btnFeedForRemove;
        private System.Windows.Forms.Button btnFeedForAdd;
        private System.Windows.Forms.ListBox listBoxFeedFor;
        private System.Windows.Forms.Button btnRemoveFeedStructureObject;
        private System.Windows.Forms.Button btnAddImplementation;
        private System.Windows.Forms.Button btnAddGroup;
        private System.Windows.Forms.Button btnAddEnvironmentBinding;
        private System.Windows.Forms.Button btnAddDependency;
        private System.Windows.Forms.Button btnAddPackageImplementation;
        private System.Windows.Forms.TreeView treeViewFeedStructure;
        private System.Windows.Forms.GroupBox groupBoxFeedStructure;
        private System.Windows.Forms.Button btnAddOverlayBinding;
        private FeedReferenceControl feedReferenceControl;
        private System.Windows.Forms.Button btnExtFeedUpdate;
        private System.Windows.Forms.GroupBox groupBoxGroupSettings;
        private GroupControl groupControl;
    }
}

