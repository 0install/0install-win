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
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageGeneral = new System.Windows.Forms.TabPage();
            this.checkedListCategory = new System.Windows.Forms.CheckedListBox();
            this.lblHomepage = new System.Windows.Forms.Label();
            this.lblDescription = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblIcon = new System.Windows.Forms.Label();
            this.comboIconType = new System.Windows.Forms.ComboBox();
            this.lblIconUrlError = new System.Windows.Forms.Label();
            this.lblIconMime = new System.Windows.Forms.Label();
            this.btnIconPreview = new System.Windows.Forms.Button();
            this.btnIconRemove = new System.Windows.Forms.Button();
            this.iconBox = new System.Windows.Forms.PictureBox();
            this.btnIconAdd = new System.Windows.Forms.Button();
            this.listIconsUrls = new System.Windows.Forms.ListBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.tabPageFeed = new System.Windows.Forms.TabPage();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.textHomepage = new Common.Controls.HintTextBox();
            this.textDescription = new Common.Controls.HintTextBox();
            this.textIconUrl = new Common.Controls.HintTextBox();
            this.textSummary = new Common.Controls.HintTextBox();
            this.textName = new Common.Controls.HintTextBox();
            this.lblInterfaceURL = new System.Windows.Forms.Label();
            this.textInterfaceURL = new Common.Controls.HintTextBox();
            this.toolStrip.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.tabPageGeneral.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).BeginInit();
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
            this.toolStripButtonNew.Click += new System.EventHandler(this.toolStripButtonNew_Click);
            // 
            // toolStripButtonOpen
            // 
            this.toolStripButtonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonOpen.Image = global::ZeroInstall.FeedEditor.Properties.Resources.OpenButton;
            this.toolStripButtonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonOpen.Name = "toolStripButtonOpen";
            this.toolStripButtonOpen.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonOpen.Text = "Open";
            this.toolStripButtonOpen.Click += new System.EventHandler(this.toolStripButtonOpen_Click);
            // 
            // toolStripButtonSave
            // 
            this.toolStripButtonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonSave.Image = global::ZeroInstall.FeedEditor.Properties.Resources.SaveButton;
            this.toolStripButtonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonSave.Name = "toolStripButtonSave";
            this.toolStripButtonSave.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonSave.Text = "Save";
            this.toolStripButtonSave.Click += new System.EventHandler(this.toolStripButtonSave_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
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
            this.tabControlMain.Size = new System.Drawing.Size(616, 464);
            this.tabControlMain.TabIndex = 1;
            // 
            // tabPageGeneral
            // 
            this.tabPageGeneral.Controls.Add(this.textInterfaceURL);
            this.tabPageGeneral.Controls.Add(this.lblInterfaceURL);
            this.tabPageGeneral.Controls.Add(this.checkedListCategory);
            this.tabPageGeneral.Controls.Add(this.textHomepage);
            this.tabPageGeneral.Controls.Add(this.lblHomepage);
            this.tabPageGeneral.Controls.Add(this.textDescription);
            this.tabPageGeneral.Controls.Add(this.lblDescription);
            this.tabPageGeneral.Controls.Add(this.groupBox1);
            this.tabPageGeneral.Controls.Add(this.lblCategory);
            this.tabPageGeneral.Controls.Add(this.textSummary);
            this.tabPageGeneral.Controls.Add(this.label1);
            this.tabPageGeneral.Controls.Add(this.textName);
            this.tabPageGeneral.Controls.Add(this.lblName);
            this.tabPageGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabPageGeneral.Name = "tabPageGeneral";
            this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageGeneral.Size = new System.Drawing.Size(608, 438);
            this.tabPageGeneral.TabIndex = 0;
            this.tabPageGeneral.Text = "General";
            this.tabPageGeneral.UseVisualStyleBackColor = true;
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
            this.checkedListCategory.Size = new System.Drawing.Size(119, 64);
            this.checkedListCategory.Sorted = true;
            this.checkedListCategory.TabIndex = 25;
            // 
            // lblHomepage
            // 
            this.lblHomepage.AutoSize = true;
            this.lblHomepage.Location = new System.Drawing.Point(6, 391);
            this.lblHomepage.Name = "lblHomepage";
            this.lblHomepage.Size = new System.Drawing.Size(59, 13);
            this.lblHomepage.TabIndex = 23;
            this.lblHomepage.Text = "Homepage";
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(6, 259);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(60, 13);
            this.lblDescription.TabIndex = 21;
            this.lblDescription.Text = "Description";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.lblIcon);
            this.groupBox1.Controls.Add(this.comboIconType);
            this.groupBox1.Controls.Add(this.textIconUrl);
            this.groupBox1.Controls.Add(this.lblIconUrlError);
            this.groupBox1.Controls.Add(this.lblIconMime);
            this.groupBox1.Controls.Add(this.btnIconPreview);
            this.groupBox1.Controls.Add(this.btnIconRemove);
            this.groupBox1.Controls.Add(this.iconBox);
            this.groupBox1.Controls.Add(this.btnIconAdd);
            this.groupBox1.Controls.Add(this.listIconsUrls);
            this.groupBox1.Location = new System.Drawing.Point(9, 85);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(591, 171);
            this.groupBox1.TabIndex = 19;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Icon";
            // 
            // lblIcon
            // 
            this.lblIcon.AutoSize = true;
            this.lblIcon.Location = new System.Drawing.Point(6, 16);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(42, 13);
            this.lblIcon.TabIndex = 6;
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
            this.comboIconType.Location = new System.Drawing.Point(379, 32);
            this.comboIconType.Name = "comboIconType";
            this.comboIconType.Size = new System.Drawing.Size(76, 21);
            this.comboIconType.TabIndex = 18;
            // 
            // lblIconUrlError
            // 
            this.lblIconUrlError.AutoSize = true;
            this.lblIconUrlError.ForeColor = System.Drawing.Color.Red;
            this.lblIconUrlError.Location = new System.Drawing.Point(6, 144);
            this.lblIconUrlError.Name = "lblIconUrlError";
            this.lblIconUrlError.Size = new System.Drawing.Size(0, 13);
            this.lblIconUrlError.TabIndex = 10;
            // 
            // lblIconMime
            // 
            this.lblIconMime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIconMime.AutoSize = true;
            this.lblIconMime.Location = new System.Drawing.Point(376, 16);
            this.lblIconMime.Name = "lblIconMime";
            this.lblIconMime.Size = new System.Drawing.Size(55, 13);
            this.lblIconMime.TabIndex = 17;
            this.lblIconMime.Text = "Icon Type";
            // 
            // btnIconPreview
            // 
            this.btnIconPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIconPreview.Location = new System.Drawing.Point(377, 59);
            this.btnIconPreview.Name = "btnIconPreview";
            this.btnIconPreview.Size = new System.Drawing.Size(78, 23);
            this.btnIconPreview.TabIndex = 8;
            this.btnIconPreview.Text = "Icon Preview";
            this.btnIconPreview.UseVisualStyleBackColor = true;
            this.btnIconPreview.Click += new System.EventHandler(this.BtnIconPreviewClick);
            // 
            // btnIconRemove
            // 
            this.btnIconRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIconRemove.Location = new System.Drawing.Point(377, 117);
            this.btnIconRemove.Name = "btnIconRemove";
            this.btnIconRemove.Size = new System.Drawing.Size(78, 23);
            this.btnIconRemove.TabIndex = 16;
            this.btnIconRemove.Text = "Remove";
            this.btnIconRemove.UseVisualStyleBackColor = true;
            this.btnIconRemove.Click += new System.EventHandler(this.btnIconListRemove_Click);
            // 
            // iconBox
            // 
            this.iconBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.iconBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iconBox.Location = new System.Drawing.Point(461, 21);
            this.iconBox.Name = "iconBox";
            this.iconBox.Size = new System.Drawing.Size(120, 120);
            this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iconBox.TabIndex = 9;
            this.iconBox.TabStop = false;
            // 
            // btnIconAdd
            // 
            this.btnIconAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIconAdd.Location = new System.Drawing.Point(377, 88);
            this.btnIconAdd.Name = "btnIconAdd";
            this.btnIconAdd.Size = new System.Drawing.Size(78, 23);
            this.btnIconAdd.TabIndex = 15;
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
            this.listIconsUrls.Size = new System.Drawing.Size(364, 82);
            this.listIconsUrls.TabIndex = 14;
            this.listIconsUrls.SelectedIndexChanged += new System.EventHandler(this.listIconsUrls_SelectedIndexChanged);
            // 
            // lblCategory
            // 
            this.lblCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(477, 4);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(49, 13);
            this.lblCategory.TabIndex = 4;
            this.lblCategory.Text = "Category";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Summary";
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
            this.tabPageFeed.Location = new System.Drawing.Point(4, 22);
            this.tabPageFeed.Name = "tabPageFeed";
            this.tabPageFeed.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFeed.Size = new System.Drawing.Size(608, 399);
            this.tabPageFeed.TabIndex = 1;
            this.tabPageFeed.Text = "Feeds";
            this.tabPageFeed.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Size = new System.Drawing.Size(608, 399);
            this.tabAdvanced.TabIndex = 2;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // textHomepage
            // 
            this.textHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textHomepage.HintText = "the URL of a web-page describing this interface in more detail";
            this.textHomepage.Location = new System.Drawing.Point(9, 407);
            this.textHomepage.Name = "textHomepage";
            this.textHomepage.Size = new System.Drawing.Size(592, 20);
            this.textHomepage.TabIndex = 24;
            // 
            // textDescription
            // 
            this.textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textDescription.HintText = "a full description, which can be several paragraphs long";
            this.textDescription.Location = new System.Drawing.Point(9, 275);
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(591, 74);
            this.textDescription.TabIndex = 22;
            // 
            // textIconUrl
            // 
            this.textIconUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textIconUrl.HintText = "";
            this.textIconUrl.Location = new System.Drawing.Point(9, 32);
            this.textIconUrl.Name = "textIconUrl";
            this.textIconUrl.Size = new System.Drawing.Size(364, 20);
            this.textIconUrl.TabIndex = 11;
            // 
            // textSummary
            // 
            this.textSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textSummary.HintText = "a short one-line description";
            this.textSummary.Location = new System.Drawing.Point(9, 59);
            this.textSummary.Name = "textSummary";
            this.textSummary.Size = new System.Drawing.Size(465, 20);
            this.textSummary.TabIndex = 12;
            // 
            // textName
            // 
            this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textName.HintText = "a short name to identify the interface (e.g. \"Foo\")";
            this.textName.Location = new System.Drawing.Point(9, 20);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(465, 20);
            this.textName.TabIndex = 13;
            // 
            // lblInterfaceURL
            // 
            this.lblInterfaceURL.AutoSize = true;
            this.lblInterfaceURL.Location = new System.Drawing.Point(6, 352);
            this.lblInterfaceURL.Name = "lblInterfaceURL";
            this.lblInterfaceURL.Size = new System.Drawing.Size(74, 13);
            this.lblInterfaceURL.TabIndex = 26;
            this.lblInterfaceURL.Text = "Interface URL";
            // 
            // textInterfaceURL
            // 
            this.textInterfaceURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textInterfaceURL.HintText = "URL to a remote interface";
            this.textInterfaceURL.Location = new System.Drawing.Point(9, 368);
            this.textInterfaceURL.Name = "textInterfaceURL";
            this.textInterfaceURL.Size = new System.Drawing.Size(591, 20);
            this.textInterfaceURL.TabIndex = 27;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(633, 507);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.toolStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(325, 247);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zero Install Feed Editor";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.tabControlMain.ResumeLayout(false);
            this.tabPageGeneral.ResumeLayout(false);
            this.tabPageGeneral.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconBox)).EndInit();
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
        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.GroupBox groupBox1;
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
    }
}

