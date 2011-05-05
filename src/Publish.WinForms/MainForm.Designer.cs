using System;
using System.Drawing;
using System.IO;
using System.Net;
using Common.Controls;
using ZeroInstall.Model;
using ZeroInstall.Publish.WinForms.Controls;

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
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Windows.Forms.TreeNode treeNode12 = new System.Windows.Forms.TreeNode("Interface");
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.descriptionControl = new ZeroInstall.Publish.WinForms.Controls.LocalizableTextControl();
            this.summariesControl = new ZeroInstall.Publish.WinForms.Controls.LocalizableTextControl();
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelSummary = new System.Windows.Forms.Label();
            this.checkBoxNeedsTerminal = new System.Windows.Forms.CheckBox();
            this.textInterfaceUri = new Common.Controls.UriTextBox();
            this.labelInterfaceUri = new System.Windows.Forms.Label();
            this.checkedListBoxCategories = new System.Windows.Forms.CheckedListBox();
            this.textHomepage = new Common.Controls.UriTextBox();
            this.labelHomepage = new System.Windows.Forms.Label();
            this.groupBoxIcon = new System.Windows.Forms.GroupBox();
            this.iconManagementControl = new ZeroInstall.Publish.WinForms.Controls.IconManagementControl();
            this.labelCategories = new System.Windows.Forms.Label();
            this.textName = new Common.Controls.HintTextBox();
            this.labelProgramName = new System.Windows.Forms.Label();
            this.tabPageFeed = new System.Windows.Forms.TabPage();
            this.groupBoxFeedStructure = new System.Windows.Forms.GroupBox();
            this.buttonAddRecipe = new System.Windows.Forms.Button();
            this.buttonAddArchive = new System.Windows.Forms.Button();
            this.btnAddOverlayBinding = new System.Windows.Forms.Button();
            this.treeViewFeedStructure = new System.Windows.Forms.TreeView();
            this.btnRemoveFeedStructureObject = new System.Windows.Forms.Button();
            this.btnAddEnvironmentBinding = new System.Windows.Forms.Button();
            this.btnAddGroup = new System.Windows.Forms.Button();
            this.btnAddCommand = new System.Windows.Forms.Button();
            this.btnAddDependency = new System.Windows.Forms.Button();
            this.btnAddPackageImplementation = new System.Windows.Forms.Button();
            this.btnAddImplementation = new System.Windows.Forms.Button();
            this.tabPageAdvanced = new System.Windows.Forms.TabPage();
            this.groupBoxFeedFor = new System.Windows.Forms.GroupBox();
            this.buttonClearFeedFor = new System.Windows.Forms.Button();
            this.buttonAddFeedFor = new System.Windows.Forms.Button();
            this.buttonRemoveExternalFeed = new System.Windows.Forms.Button();
            this.listBoxFeedFor = new System.Windows.Forms.ListBox();
            this.hintTextBoxFeedFor = new Common.Controls.HintTextBox();
            this.comboBoxMinInjectorVersion = new System.Windows.Forms.ComboBox();
            this.labelMinInjectorVersion = new System.Windows.Forms.Label();
            this.groupBoxExternalFeeds = new System.Windows.Forms.GroupBox();
            this.buttonUpdateExternalFeed = new System.Windows.Forms.Button();
            this.groupBoxSelectedFeed = new System.Windows.Forms.GroupBox();
            this.feedReferenceControl = new ZeroInstall.Publish.WinForms.Controls.FeedReferenceControl();
            this.listBoxExternalFeeds = new System.Windows.Forms.ListBox();
            this.buttonRemoveFeedFor = new System.Windows.Forms.Button();
            this.buttonAddExternalFeeds = new System.Windows.Forms.Button();
            this.feedEditorToolStrip = new ZeroInstall.Publish.WinForms.Controls.FeedEditorToolStrip();
            this.feedManager1 = new ZeroInstall.Publish.WinForms.FeedManager(this.components);
            this.tabControlMain.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.groupBoxIcon.SuspendLayout();
            this.tabPageFeed.SuspendLayout();
            this.groupBoxFeedStructure.SuspendLayout();
            this.tabPageAdvanced.SuspendLayout();
            this.groupBoxFeedFor.SuspendLayout();
            this.groupBoxExternalFeeds.SuspendLayout();
            this.groupBoxSelectedFeed.SuspendLayout();
            this.SuspendLayout();
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
            this.tabControlMain.Size = new System.Drawing.Size(559, 509);
            this.tabControlMain.TabIndex = 1;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.descriptionControl);
            this.tabPageGeneral.Controls.Add(this.summariesControl);
            this.tabPageGeneral.Controls.Add(this.labelDescription);
            this.tabPageGeneral.Controls.Add(this.labelSummary);
            this.tabPageGeneral.Controls.Add(this.checkBoxNeedsTerminal);
            this.tabPageGeneral.Controls.Add(this.textInterfaceUri);
            this.tabPageGeneral.Controls.Add(this.labelInterfaceUri);
            this.tabPageGeneral.Controls.Add(this.checkedListBoxCategories);
            this.tabPageGeneral.Controls.Add(this.textHomepage);
            this.tabPageGeneral.Controls.Add(this.labelHomepage);
            this.tabPageGeneral.Controls.Add(this.groupBoxIcon);
            this.tabPageGeneral.Controls.Add(this.labelCategories);
            this.tabPageGeneral.Controls.Add(this.textName);
            this.tabPageGeneral.Controls.Add(this.labelProgramName);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(551, 483);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
            // 
            // descriptionControl
            // 
            this.descriptionControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionControl.Location = new System.Drawing.Point(9, 140);
            this.descriptionControl.Multiline = true;
            this.descriptionControl.Name = "descriptionControl";
            this.descriptionControl.Size = new System.Drawing.Size(536, 79);
            this.descriptionControl.TabIndex = 9;
            // 
            // summariesControl
            // 
            this.summariesControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.summariesControl.Location = new System.Drawing.Point(9, 98);
            this.summariesControl.Multiline = false;
            this.summariesControl.Name = "summariesControl";
            this.summariesControl.Size = new System.Drawing.Size(536, 23);
            this.summariesControl.TabIndex = 7;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(6, 124);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(60, 13);
            this.labelDescription.TabIndex = 8;
            this.labelDescription.Text = "Description";
            // 
            // labelSummary
            // 
            this.labelSummary.AutoSize = true;
            this.labelSummary.Location = new System.Drawing.Point(6, 82);
            this.labelSummary.Name = "labelSummary";
            this.labelSummary.Size = new System.Drawing.Size(50, 13);
            this.labelSummary.TabIndex = 6;
            this.labelSummary.Text = "Summary";
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
            // textInterfaceUri
            // 
            this.textInterfaceUri.AllowDrop = true;
            this.textInterfaceUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textInterfaceUri.BackColor = System.Drawing.SystemColors.Window;
            this.textInterfaceUri.HintText = "URL to a remote interface";
            this.textInterfaceUri.HttpOnly = true;
            this.textInterfaceUri.Location = new System.Drawing.Point(9, 59);
            this.textInterfaceUri.Name = "textInterfaceUri";
            this.textInterfaceUri.ShowClearButton = true;
            this.textInterfaceUri.Size = new System.Drawing.Size(414, 20);
            this.textInterfaceUri.TabIndex = 3;
            // 
            // labelInterfaceUri
            // 
            this.labelInterfaceUri.AutoSize = true;
            this.labelInterfaceUri.Location = new System.Drawing.Point(6, 43);
            this.labelInterfaceUri.Name = "labelInterfaceUri";
            this.labelInterfaceUri.Size = new System.Drawing.Size(71, 13);
            this.labelInterfaceUri.TabIndex = 2;
            this.labelInterfaceUri.Text = "Interface URI";
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
            this.checkedListBoxCategories.Location = new System.Drawing.Point(429, 20);
            this.checkedListBoxCategories.Name = "checkedListBoxCategories";
            this.checkedListBoxCategories.Size = new System.Drawing.Size(116, 64);
            this.checkedListBoxCategories.Sorted = true;
            this.checkedListBoxCategories.TabIndex = 5;
            // 
            // textHomepage
            // 
            this.textHomepage.AllowDrop = true;
            this.textHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textHomepage.HintText = "the URL of a web-page describing this interface in more detail";
            this.textHomepage.Location = new System.Drawing.Point(9, 415);
            this.textHomepage.Name = "textHomepage";
            this.textHomepage.ShowClearButton = true;
            this.textHomepage.Size = new System.Drawing.Size(536, 20);
            this.textHomepage.TabIndex = 12;
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
            // groupBoxIcon
            // 
            this.groupBoxIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxIcon.Controls.Add(this.iconManagementControl);
            this.groupBoxIcon.Location = new System.Drawing.Point(9, 225);
            this.groupBoxIcon.Name = "groupBoxIcon";
            this.groupBoxIcon.Size = new System.Drawing.Size(536, 171);
            this.groupBoxIcon.TabIndex = 10;
            this.groupBoxIcon.TabStop = false;
            this.groupBoxIcon.Text = "Icon";
            // 
            // iconManagementControl
            // 
            this.iconManagementControl.Location = new System.Drawing.Point(7, 20);
            this.iconManagementControl.Name = "iconManagementControl";
            this.iconManagementControl.Size = new System.Drawing.Size(521, 143);
            this.iconManagementControl.TabIndex = 0;
            // 
            // labelCategories
            // 
            this.labelCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCategories.AutoSize = true;
            this.labelCategories.Location = new System.Drawing.Point(426, 3);
            this.labelCategories.Name = "labelCategories";
            this.labelCategories.Size = new System.Drawing.Size(57, 13);
            this.labelCategories.TabIndex = 4;
            this.labelCategories.Text = "Categories";
            // 
            // textName
            // 
            this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textName.HintText = "a short name to identify the interface (e.g. \"Foo\")";
            this.textName.Location = new System.Drawing.Point(9, 20);
            this.textName.Name = "textName";
            this.textName.ShowClearButton = true;
            this.textName.Size = new System.Drawing.Size(414, 20);
            this.textName.TabIndex = 1;
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
            this.tabPageFeed.Size = new System.Drawing.Size(551, 483);
            this.tabPageFeed.TabIndex = 1;
            this.tabPageFeed.Text = "Feed structure";
            this.tabPageFeed.UseVisualStyleBackColor = true;
            // 
            // groupBoxFeedStructure
            // 
            this.groupBoxFeedStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFeedStructure.Controls.Add(this.buttonAddRecipe);
            this.groupBoxFeedStructure.Controls.Add(this.buttonAddArchive);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddOverlayBinding);
            this.groupBoxFeedStructure.Controls.Add(this.treeViewFeedStructure);
            this.groupBoxFeedStructure.Controls.Add(this.btnRemoveFeedStructureObject);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddEnvironmentBinding);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddGroup);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddCommand);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddDependency);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddPackageImplementation);
            this.groupBoxFeedStructure.Controls.Add(this.btnAddImplementation);
            this.groupBoxFeedStructure.Location = new System.Drawing.Point(6, 6);
            this.groupBoxFeedStructure.Name = "groupBoxFeedStructure";
            this.groupBoxFeedStructure.Size = new System.Drawing.Size(539, 471);
            this.groupBoxFeedStructure.TabIndex = 0;
            this.groupBoxFeedStructure.TabStop = false;
            this.groupBoxFeedStructure.Text = "Feed Structure";
            // 
            // buttonAddRecipe
            // 
            this.buttonAddRecipe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddRecipe.Enabled = false;
            this.buttonAddRecipe.Location = new System.Drawing.Point(401, 251);
            this.buttonAddRecipe.Name = "buttonAddRecipe";
            this.buttonAddRecipe.Size = new System.Drawing.Size(132, 23);
            this.buttonAddRecipe.TabIndex = 4;
            this.buttonAddRecipe.Text = "Recipe";
            this.buttonAddRecipe.UseVisualStyleBackColor = true;
            // 
            // buttonAddArchive
            // 
            this.buttonAddArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddArchive.Enabled = false;
            this.buttonAddArchive.Location = new System.Drawing.Point(401, 222);
            this.buttonAddArchive.Name = "buttonAddArchive";
            this.buttonAddArchive.Size = new System.Drawing.Size(132, 23);
            this.buttonAddArchive.TabIndex = 3;
            this.buttonAddArchive.Text = "Archive";
            this.buttonAddArchive.UseVisualStyleBackColor = true;
            // 
            // btnAddOverlayBinding
            // 
            this.btnAddOverlayBinding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddOverlayBinding.Enabled = false;
            this.btnAddOverlayBinding.Location = new System.Drawing.Point(401, 135);
            this.btnAddOverlayBinding.Name = "btnAddOverlayBinding";
            this.btnAddOverlayBinding.Size = new System.Drawing.Size(132, 23);
            this.btnAddOverlayBinding.TabIndex = 8;
            this.btnAddOverlayBinding.Text = "Overlay binding";
            this.btnAddOverlayBinding.UseVisualStyleBackColor = true;
            // 
            // treeViewFeedStructure
            // 
            this.treeViewFeedStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewFeedStructure.HideSelection = false;
            this.treeViewFeedStructure.Location = new System.Drawing.Point(6, 19);
            this.treeViewFeedStructure.Name = "treeViewFeedStructure";
            treeNode12.Name = "interface";
            treeNode12.Tag = "";
            treeNode12.Text = "Interface";
            this.treeViewFeedStructure.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode12});
            this.treeViewFeedStructure.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.treeViewFeedStructure.ShowRootLines = false;
            this.treeViewFeedStructure.Size = new System.Drawing.Size(389, 446);
            this.treeViewFeedStructure.TabIndex = 0;
            this.treeViewFeedStructure.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewFeedStructureAfterSelect);
            this.treeViewFeedStructure.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.TreeViewFeedStructureNodeMouseDoubleClick);
            // 
            // btnRemoveFeedStructureObject
            // 
            this.btnRemoveFeedStructureObject.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemoveFeedStructureObject.Location = new System.Drawing.Point(401, 442);
            this.btnRemoveFeedStructureObject.Name = "btnRemoveFeedStructureObject";
            this.btnRemoveFeedStructureObject.Size = new System.Drawing.Size(132, 23);
            this.btnRemoveFeedStructureObject.TabIndex = 10;
            this.btnRemoveFeedStructureObject.Text = "Remove item";
            this.btnRemoveFeedStructureObject.UseVisualStyleBackColor = true;
            // 
            // btnAddEnvironmentBinding
            // 
            this.btnAddEnvironmentBinding.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddEnvironmentBinding.Enabled = false;
            this.btnAddEnvironmentBinding.Location = new System.Drawing.Point(401, 106);
            this.btnAddEnvironmentBinding.Name = "btnAddEnvironmentBinding";
            this.btnAddEnvironmentBinding.Size = new System.Drawing.Size(132, 23);
            this.btnAddEnvironmentBinding.TabIndex = 7;
            this.btnAddEnvironmentBinding.Text = "Environment binding";
            this.btnAddEnvironmentBinding.UseVisualStyleBackColor = true;
            // 
            // btnAddGroup
            // 
            this.btnAddGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddGroup.Enabled = false;
            this.btnAddGroup.Location = new System.Drawing.Point(401, 19);
            this.btnAddGroup.Name = "btnAddGroup";
            this.btnAddGroup.Size = new System.Drawing.Size(132, 23);
            this.btnAddGroup.TabIndex = 1;
            this.btnAddGroup.Text = "Group";
            this.btnAddGroup.UseVisualStyleBackColor = true;
            // 
            // btnAddCommand
            // 
            this.btnAddCommand.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddCommand.Enabled = false;
            this.btnAddCommand.Location = new System.Drawing.Point(401, 77);
            this.btnAddCommand.Name = "btnAddCommand";
            this.btnAddCommand.Size = new System.Drawing.Size(132, 23);
            this.btnAddCommand.TabIndex = 6;
            this.btnAddCommand.Text = "Command";
            this.btnAddCommand.UseVisualStyleBackColor = true;
            // 
            // btnAddDependency
            // 
            this.btnAddDependency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddDependency.Enabled = false;
            this.btnAddDependency.Location = new System.Drawing.Point(401, 48);
            this.btnAddDependency.Name = "btnAddDependency";
            this.btnAddDependency.Size = new System.Drawing.Size(132, 23);
            this.btnAddDependency.TabIndex = 6;
            this.btnAddDependency.Text = "Dependency";
            this.btnAddDependency.UseVisualStyleBackColor = true;
            // 
            // btnAddPackageImplementation
            // 
            this.btnAddPackageImplementation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddPackageImplementation.Enabled = false;
            this.btnAddPackageImplementation.Location = new System.Drawing.Point(401, 164);
            this.btnAddPackageImplementation.Name = "btnAddPackageImplementation";
            this.btnAddPackageImplementation.Size = new System.Drawing.Size(132, 23);
            this.btnAddPackageImplementation.TabIndex = 5;
            this.btnAddPackageImplementation.Text = "Package implementation";
            this.btnAddPackageImplementation.UseVisualStyleBackColor = true;
            // 
            // btnAddImplementation
            // 
            this.btnAddImplementation.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddImplementation.Enabled = false;
            this.btnAddImplementation.Location = new System.Drawing.Point(401, 193);
            this.btnAddImplementation.Name = "btnAddImplementation";
            this.btnAddImplementation.Size = new System.Drawing.Size(132, 23);
            this.btnAddImplementation.TabIndex = 2;
            this.btnAddImplementation.Text = "Implementation";
            this.btnAddImplementation.UseVisualStyleBackColor = true;
            // 
            // tabPageAdvanced
            // 
            this.tabPageAdvanced.Controls.Add(this.groupBoxFeedFor);
            this.tabPageAdvanced.Controls.Add(this.comboBoxMinInjectorVersion);
            this.tabPageAdvanced.Controls.Add(this.labelMinInjectorVersion);
            this.tabPageAdvanced.Controls.Add(this.groupBoxExternalFeeds);
            this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabPageAdvanced.Name = "tabPageAdvanced";
            this.tabPageAdvanced.Size = new System.Drawing.Size(551, 483);
            this.tabPageAdvanced.TabIndex = 2;
            this.tabPageAdvanced.Text = "Advanced";
            this.tabPageAdvanced.UseVisualStyleBackColor = true;
            // 
            // groupBoxFeedFor
            // 
            this.groupBoxFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxFeedFor.Controls.Add(this.buttonClearFeedFor);
            this.groupBoxFeedFor.Controls.Add(this.buttonAddFeedFor);
            this.groupBoxFeedFor.Controls.Add(this.buttonRemoveExternalFeed);
            this.groupBoxFeedFor.Controls.Add(this.listBoxFeedFor);
            this.groupBoxFeedFor.Controls.Add(this.hintTextBoxFeedFor);
            this.groupBoxFeedFor.Location = new System.Drawing.Point(6, 316);
            this.groupBoxFeedFor.Name = "groupBoxFeedFor";
            this.groupBoxFeedFor.Size = new System.Drawing.Size(539, 111);
            this.groupBoxFeedFor.TabIndex = 1;
            this.groupBoxFeedFor.TabStop = false;
            this.groupBoxFeedFor.Text = "Feed For Interface";
            // 
            // buttonClearFeedFor
            // 
            this.buttonClearFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearFeedFor.Location = new System.Drawing.Point(458, 79);
            this.buttonClearFeedFor.Name = "buttonClearFeedFor";
            this.buttonClearFeedFor.Size = new System.Drawing.Size(75, 23);
            this.buttonClearFeedFor.TabIndex = 6;
            this.buttonClearFeedFor.Text = "Clear list";
            this.buttonClearFeedFor.UseVisualStyleBackColor = true;
            this.buttonClearFeedFor.Click += new System.EventHandler(this.BtnFeedForClearClick);
            // 
            // buttonAddFeedFor
            // 
            this.buttonAddFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddFeedFor.Location = new System.Drawing.Point(458, 21);
            this.buttonAddFeedFor.Name = "buttonAddFeedFor";
            this.buttonAddFeedFor.Size = new System.Drawing.Size(75, 23);
            this.buttonAddFeedFor.TabIndex = 4;
            this.buttonAddFeedFor.Text = "Add";
            this.buttonAddFeedFor.UseVisualStyleBackColor = true;
            this.buttonAddFeedFor.Click += new System.EventHandler(this.BtnFeedForAddClick);
            // 
            // buttonRemoveExternalFeed
            // 
            this.buttonRemoveExternalFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveExternalFeed.Location = new System.Drawing.Point(458, 50);
            this.buttonRemoveExternalFeed.Name = "buttonRemoveExternalFeed";
            this.buttonRemoveExternalFeed.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveExternalFeed.TabIndex = 3;
            this.buttonRemoveExternalFeed.Text = "Remove";
            this.buttonRemoveExternalFeed.UseVisualStyleBackColor = true;
            this.buttonRemoveExternalFeed.Click += new System.EventHandler(this.BtnFeedForRemoveClick);
            // 
            // listBoxFeedFor
            // 
            this.listBoxFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxFeedFor.FormattingEnabled = true;
            this.listBoxFeedFor.Location = new System.Drawing.Point(7, 46);
            this.listBoxFeedFor.Name = "listBoxFeedFor";
            this.listBoxFeedFor.Size = new System.Drawing.Size(445, 56);
            this.listBoxFeedFor.TabIndex = 3;
            // 
            // hintTextBoxFeedFor
            // 
            this.hintTextBoxFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxFeedFor.HintText = "URL to an Interface";
            this.hintTextBoxFeedFor.Location = new System.Drawing.Point(6, 19);
            this.hintTextBoxFeedFor.Name = "hintTextBoxFeedFor";
            this.hintTextBoxFeedFor.ShowClearButton = true;
            this.hintTextBoxFeedFor.Size = new System.Drawing.Size(446, 20);
            this.hintTextBoxFeedFor.TabIndex = 2;
            // 
            // comboBoxMinInjectorVersion
            // 
            this.comboBoxMinInjectorVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxMinInjectorVersion.FormattingEnabled = true;
            this.comboBoxMinInjectorVersion.Location = new System.Drawing.Point(6, 446);
            this.comboBoxMinInjectorVersion.Name = "comboBoxMinInjectorVersion";
            this.comboBoxMinInjectorVersion.Size = new System.Drawing.Size(93, 21);
            this.comboBoxMinInjectorVersion.Sorted = true;
            this.comboBoxMinInjectorVersion.TabIndex = 3;
            // 
            // labelMinInjectorVersion
            // 
            this.labelMinInjectorVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelMinInjectorVersion.AutoSize = true;
            this.labelMinInjectorVersion.Location = new System.Drawing.Point(3, 430);
            this.labelMinInjectorVersion.Name = "labelMinInjectorVersion";
            this.labelMinInjectorVersion.Size = new System.Drawing.Size(102, 13);
            this.labelMinInjectorVersion.TabIndex = 2;
            this.labelMinInjectorVersion.Text = "min. Injector Version";
            // 
            // groupBoxExternalFeeds
            // 
            this.groupBoxExternalFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxExternalFeeds.Controls.Add(this.buttonUpdateExternalFeed);
            this.groupBoxExternalFeeds.Controls.Add(this.groupBoxSelectedFeed);
            this.groupBoxExternalFeeds.Controls.Add(this.listBoxExternalFeeds);
            this.groupBoxExternalFeeds.Controls.Add(this.buttonRemoveFeedFor);
            this.groupBoxExternalFeeds.Controls.Add(this.buttonAddExternalFeeds);
            this.groupBoxExternalFeeds.Location = new System.Drawing.Point(3, 6);
            this.groupBoxExternalFeeds.Name = "groupBoxExternalFeeds";
            this.groupBoxExternalFeeds.Size = new System.Drawing.Size(542, 304);
            this.groupBoxExternalFeeds.TabIndex = 0;
            this.groupBoxExternalFeeds.TabStop = false;
            this.groupBoxExternalFeeds.Text = "External Feeds";
            // 
            // buttonUpdateExternalFeed
            // 
            this.buttonUpdateExternalFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdateExternalFeed.Location = new System.Drawing.Point(461, 246);
            this.buttonUpdateExternalFeed.Name = "buttonUpdateExternalFeed";
            this.buttonUpdateExternalFeed.Size = new System.Drawing.Size(75, 23);
            this.buttonUpdateExternalFeed.TabIndex = 2;
            this.buttonUpdateExternalFeed.Text = "Update";
            this.buttonUpdateExternalFeed.UseVisualStyleBackColor = true;
            this.buttonUpdateExternalFeed.Click += new System.EventHandler(this.BtnExtFeedUpdateClick);
            // 
            // groupBoxSelectedFeed
            // 
            this.groupBoxSelectedFeed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSelectedFeed.Controls.Add(this.feedReferenceControl);
            this.groupBoxSelectedFeed.Location = new System.Drawing.Point(6, 19);
            this.groupBoxSelectedFeed.Name = "groupBoxSelectedFeed";
            this.groupBoxSelectedFeed.Size = new System.Drawing.Size(530, 191);
            this.groupBoxSelectedFeed.TabIndex = 4;
            this.groupBoxSelectedFeed.TabStop = false;
            this.groupBoxSelectedFeed.Text = "Edit external feed";
            // 
            // feedReferenceControl
            // 
            this.feedReferenceControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.feedReferenceControl.Location = new System.Drawing.Point(6, 19);
            this.feedReferenceControl.Name = "feedReferenceControl";
            this.feedReferenceControl.Size = new System.Drawing.Size(518, 168);
            this.feedReferenceControl.TabIndex = 0;
            // 
            // listBoxExternalFeeds
            // 
            this.listBoxExternalFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxExternalFeeds.FormattingEnabled = true;
            this.listBoxExternalFeeds.HorizontalScrollbar = true;
            this.listBoxExternalFeeds.Location = new System.Drawing.Point(6, 216);
            this.listBoxExternalFeeds.Name = "listBoxExternalFeeds";
            this.listBoxExternalFeeds.Size = new System.Drawing.Size(449, 82);
            this.listBoxExternalFeeds.TabIndex = 0;
            this.listBoxExternalFeeds.SelectedIndexChanged += new System.EventHandler(this.ListBoxExtFeedsSelectedIndexChanged);
            // 
            // buttonRemoveFeedFor
            // 
            this.buttonRemoveFeedFor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveFeedFor.Location = new System.Drawing.Point(461, 275);
            this.buttonRemoveFeedFor.Name = "buttonRemoveFeedFor";
            this.buttonRemoveFeedFor.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveFeedFor.TabIndex = 5;
            this.buttonRemoveFeedFor.Text = "Remove";
            this.buttonRemoveFeedFor.UseVisualStyleBackColor = true;
            this.buttonRemoveFeedFor.Click += new System.EventHandler(this.BtnExtFeedsRemoveClick);
            // 
            // buttonAddExternalFeeds
            // 
            this.buttonAddExternalFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddExternalFeeds.Location = new System.Drawing.Point(461, 217);
            this.buttonAddExternalFeeds.Name = "buttonAddExternalFeeds";
            this.buttonAddExternalFeeds.Size = new System.Drawing.Size(75, 23);
            this.buttonAddExternalFeeds.TabIndex = 1;
            this.buttonAddExternalFeeds.Text = "Add";
            this.buttonAddExternalFeeds.UseVisualStyleBackColor = true;
            this.buttonAddExternalFeeds.Click += new System.EventHandler(this.BtnExtFeedsAddClick);
            // 
            // feedEditorToolStrip
            // 
            this.feedEditorToolStrip.Location = new System.Drawing.Point(7, 6);
            this.feedEditorToolStrip.Name = "feedEditorToolStrip";
            this.feedEditorToolStrip.Size = new System.Drawing.Size(570, 25);
            this.feedEditorToolStrip.TabIndex = 2;
            this.feedEditorToolStrip.Text = "feedEditorToolStrip1";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(584, 552);
            this.Controls.Add(this.feedEditorToolStrip);
            this.Controls.Add(this.tabControlMain);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(592, 568);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zero Install Feed Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.tabControlMain.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.tabPageGeneral.PerformLayout();
            this.groupBoxIcon.ResumeLayout(false);
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

        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageGeneral;
        private System.Windows.Forms.TabPage tabPageFeed;
        private System.Windows.Forms.Label labelProgramName;
        private Common.Controls.HintTextBox textName;
        private System.Windows.Forms.Label labelCategories;
        private System.Windows.Forms.GroupBox groupBoxIcon;
        private UriTextBox textHomepage;
        private System.Windows.Forms.Label labelHomepage;
        private System.Windows.Forms.TabPage tabPageAdvanced;
        private System.Windows.Forms.CheckedListBox checkedListBoxCategories;
        private UriTextBox textInterfaceUri;
        private System.Windows.Forms.Label labelInterfaceUri;
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
        private System.Windows.Forms.Button buttonUpdateExternalFeed;
        private System.Windows.Forms.Button buttonAddArchive;
        private System.Windows.Forms.Button buttonAddRecipe;
        private System.Windows.Forms.TreeView treeViewFeedStructure;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelSummary;
        private FeedReferenceControl feedReferenceControl;
        private LocalizableTextControl summariesControl;
        private LocalizableTextControl descriptionControl;
        private System.Windows.Forms.Button btnAddCommand;
        private IconManagementControl iconManagementControl;
        private FeedEditorToolStrip feedEditorToolStrip;
        private FeedManager feedManager1;
    }
}

