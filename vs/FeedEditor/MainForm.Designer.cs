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
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonNew = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonSave = new System.Windows.Forms.ToolStripButton();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageInterface = new System.Windows.Forms.TabPage();
            this.lblIconUrlError = new System.Windows.Forms.Label();
            this.iconBox = new System.Windows.Forms.PictureBox();
            this.btnIconPreview = new System.Windows.Forms.Button();
            this.textIconUrl = new Common.Controls.HintTextBox();
            this.lblIcon = new System.Windows.Forms.Label();
            this.comboBoxCategory = new System.Windows.Forms.ComboBox();
            this.lblCategory = new System.Windows.Forms.Label();
            this.textSummary = new Common.Controls.HintTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textName = new Common.Controls.HintTextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.tabPageFeed = new System.Windows.Forms.TabPage();
            this.toolStrip.SuspendLayout();
            this.tabControlMain.SuspendLayout();
            this.tabPageInterface.SuspendLayout();
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
            this.toolStrip.Size = new System.Drawing.Size(450, 25);
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
            this.tabControlMain.Controls.Add(this.tabPageInterface);
            this.tabControlMain.Controls.Add(this.tabPageFeed);
            this.tabControlMain.Location = new System.Drawing.Point(7, 34);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(447, 399);
            this.tabControlMain.TabIndex = 1;
            // 
            // tabPageInterface
            // 
            this.tabPageInterface.Controls.Add(this.lblIconUrlError);
            this.tabPageInterface.Controls.Add(this.iconBox);
            this.tabPageInterface.Controls.Add(this.btnIconPreview);
            this.tabPageInterface.Controls.Add(this.textIconUrl);
            this.tabPageInterface.Controls.Add(this.lblIcon);
            this.tabPageInterface.Controls.Add(this.comboBoxCategory);
            this.tabPageInterface.Controls.Add(this.lblCategory);
            this.tabPageInterface.Controls.Add(this.textSummary);
            this.tabPageInterface.Controls.Add(this.label1);
            this.tabPageInterface.Controls.Add(this.textName);
            this.tabPageInterface.Controls.Add(this.lblName);
            this.tabPageInterface.Location = new System.Drawing.Point(4, 22);
            this.tabPageInterface.Name = "tabPageInterface";
            this.tabPageInterface.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageInterface.Size = new System.Drawing.Size(439, 373);
            this.tabPageInterface.TabIndex = 0;
            this.tabPageInterface.Text = "Interface";
            this.tabPageInterface.UseVisualStyleBackColor = true;
            // 
            // lblIconUrlError
            // 
            this.lblIconUrlError.AutoSize = true;
            this.lblIconUrlError.ForeColor = System.Drawing.Color.Red;
            this.lblIconUrlError.Location = new System.Drawing.Point(7, 126);
            this.lblIconUrlError.Name = "lblIconUrlError";
            this.lblIconUrlError.Size = new System.Drawing.Size(0, 13);
            this.lblIconUrlError.TabIndex = 10;
            // 
            // iconBox
            // 
            this.iconBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.iconBox.Location = new System.Drawing.Point(372, 87);
            this.iconBox.Name = "iconBox";
            this.iconBox.Size = new System.Drawing.Size(61, 61);
            this.iconBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.iconBox.TabIndex = 9;
            this.iconBox.TabStop = false;
            // 
            // btnIconPreview
            // 
            this.btnIconPreview.Location = new System.Drawing.Point(288, 126);
            this.btnIconPreview.Name = "btnIconPreview";
            this.btnIconPreview.Size = new System.Drawing.Size(78, 23);
            this.btnIconPreview.TabIndex = 8;
            this.btnIconPreview.Text = "Icon Preview";
            this.btnIconPreview.UseVisualStyleBackColor = true;
            this.btnIconPreview.Click += new System.EventHandler(this.btnIconPreview_Click);
            // 
            // textIconUrl
            // 
            this.textIconUrl.HintText = "an icon to use for the program";
            this.textIconUrl.Location = new System.Drawing.Point(10, 100);
            this.textIconUrl.Name = "textIconUrl";
            this.textIconUrl.Size = new System.Drawing.Size(356, 20);
            this.textIconUrl.TabIndex = 7;
            this.textIconUrl.Text = "http://silvalauinger.de/0install/images/Firefox.png";
            // 
            // lblIcon
            // 
            this.lblIcon.AutoSize = true;
            this.lblIcon.Location = new System.Drawing.Point(7, 84);
            this.lblIcon.Name = "lblIcon";
            this.lblIcon.Size = new System.Drawing.Size(42, 13);
            this.lblIcon.TabIndex = 6;
            this.lblIcon.Text = "Icon url";
            // 
            // comboBoxCategory
            // 
            this.comboBoxCategory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCategory.FormattingEnabled = true;
            this.comboBoxCategory.Items.AddRange(new object[] {
            " ",
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
            this.comboBoxCategory.Location = new System.Drawing.Point(10, 169);
            this.comboBoxCategory.Name = "comboBoxCategory";
            this.comboBoxCategory.Size = new System.Drawing.Size(121, 21);
            this.comboBoxCategory.Sorted = true;
            this.comboBoxCategory.TabIndex = 5;
            // 
            // lblCategory
            // 
            this.lblCategory.AutoSize = true;
            this.lblCategory.Location = new System.Drawing.Point(7, 152);
            this.lblCategory.Name = "lblCategory";
            this.lblCategory.Size = new System.Drawing.Size(49, 13);
            this.lblCategory.TabIndex = 4;
            this.lblCategory.Text = "Category";
            // 
            // textSummary
            // 
            this.textSummary.HintText = "a short one-line description";
            this.textSummary.Location = new System.Drawing.Point(10, 61);
            this.textSummary.MaxLength = 50;
            this.textSummary.Name = "textSummary";
            this.textSummary.Size = new System.Drawing.Size(423, 20);
            this.textSummary.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Summary";
            // 
            // textName
            // 
            this.textName.AllowDrop = true;
            this.textName.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textName.HintText = "a short name to identify the interface (e.g. \"Foo\")";
            this.textName.Location = new System.Drawing.Point(10, 21);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(423, 20);
            this.textName.TabIndex = 1;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(7, 4);
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
            this.tabPageFeed.Size = new System.Drawing.Size(439, 373);
            this.tabPageFeed.TabIndex = 1;
            this.tabPageFeed.Text = "Feed";
            this.tabPageFeed.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(464, 442);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.toolStrip);
            this.MinimumSize = new System.Drawing.Size(325, 247);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.Text = "Zero Install Feed Editor";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.tabControlMain.ResumeLayout(false);
            this.tabPageInterface.ResumeLayout(false);
            this.tabPageInterface.PerformLayout();
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
        private System.Windows.Forms.TabPage tabPageInterface;
        private System.Windows.Forms.TabPage tabPageFeed;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Label label1;
        private Common.Controls.HintTextBox textName;
        private Common.Controls.HintTextBox textSummary;
        private Common.Controls.HintTextBox textIconUrl;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.ComboBox comboBoxCategory;
        private System.Windows.Forms.Label lblCategory;
        private System.Windows.Forms.Button btnIconPreview;
        private System.Windows.Forms.PictureBox iconBox;
        private System.Windows.Forms.Label lblIconUrlError;

    }
}

