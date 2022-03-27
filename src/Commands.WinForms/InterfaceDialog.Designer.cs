namespace ZeroInstall.Commands.WinForms
{
    partial class InterfaceDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InterfaceDialog));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageVersions = new System.Windows.Forms.TabPage();
            this.panelStability = new System.Windows.Forms.FlowLayoutPanel();
            this.labelStability = new System.Windows.Forms.Label();
            this.comboBoxStability = new System.Windows.Forms.ComboBox();
            this.checkBoxShowAllVersions = new System.Windows.Forms.CheckBox();
            this.dataGridVersions = new System.Windows.Forms.DataGridView();
            this.dataColumnVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataColumnReleased = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataColumnStability = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataColumnUserStability = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.dataColumnArchitecture = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataColumnNotes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataColumnSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageFeeds = new System.Windows.Forms.TabPage();
            this.buttonRemoveFeed = new System.Windows.Forms.Button();
            this.buttonAddFeed = new System.Windows.Forms.Button();
            this.listBoxFeeds = new System.Windows.Forms.ListBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.buttonApply = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabPageVersions.SuspendLayout();
            this.panelStability.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridVersions)).BeginInit();
            this.tabPageFeeds.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.Click += new System.EventHandler(this.buttonOKApply_Click);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            // 
            // tabControl
            // 
            resources.ApplyResources(this.tabControl, "tabControl");
            this.tabControl.Controls.Add(this.tabPageVersions);
            this.tabControl.Controls.Add(this.tabPageFeeds);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            // 
            // tabPageVersions
            // 
            this.tabPageVersions.Controls.Add(this.panelStability);
            this.tabPageVersions.Controls.Add(this.checkBoxShowAllVersions);
            this.tabPageVersions.Controls.Add(this.dataGridVersions);
            resources.ApplyResources(this.tabPageVersions, "tabPageVersions");
            this.tabPageVersions.Name = "tabPageVersions";
            this.tabPageVersions.UseVisualStyleBackColor = true;
            // 
            // panelStability
            // 
            resources.ApplyResources(this.panelStability, "panelStability");
            this.panelStability.Controls.Add(this.labelStability);
            this.panelStability.Controls.Add(this.comboBoxStability);
            this.panelStability.Name = "panelStability";
            // 
            // labelStability
            // 
            resources.ApplyResources(this.labelStability, "labelStability");
            this.labelStability.Name = "labelStability";
            // 
            // comboBoxStability
            // 
            this.comboBoxStability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStability.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxStability, "comboBoxStability");
            this.comboBoxStability.Name = "comboBoxStability";
            this.toolTip.SetToolTip(this.comboBoxStability, resources.GetString("comboBoxStability.ToolTip"));
            // 
            // checkBoxShowAllVersions
            // 
            resources.ApplyResources(this.checkBoxShowAllVersions, "checkBoxShowAllVersions");
            this.checkBoxShowAllVersions.Name = "checkBoxShowAllVersions";
            this.toolTip.SetToolTip(this.checkBoxShowAllVersions, resources.GetString("checkBoxShowAllVersions.ToolTip"));
            this.checkBoxShowAllVersions.UseVisualStyleBackColor = true;
            this.checkBoxShowAllVersions.CheckedChanged += new System.EventHandler(this.checkBoxShowAllVersions_CheckedChanged);
            // 
            // dataGridVersions
            // 
            this.dataGridVersions.AllowUserToAddRows = false;
            this.dataGridVersions.AllowUserToDeleteRows = false;
            resources.ApplyResources(this.dataGridVersions, "dataGridVersions");
            this.dataGridVersions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridVersions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.dataColumnVersion, this.dataColumnReleased, this.dataColumnStability, this.dataColumnUserStability, this.dataColumnArchitecture, this.dataColumnNotes, this.dataColumnSource });
            this.dataGridVersions.Name = "dataGridVersions";
            // 
            // dataColumnVersion
            // 
            this.dataColumnVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataColumnVersion.DataPropertyName = "Version";
            resources.ApplyResources(this.dataColumnVersion, "dataColumnVersion");
            this.dataColumnVersion.Name = "dataColumnVersion";
            this.dataColumnVersion.ReadOnly = true;
            // 
            // dataColumnReleased
            // 
            this.dataColumnReleased.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataColumnReleased.DataPropertyName = "Released";
            resources.ApplyResources(this.dataColumnReleased, "dataColumnReleased");
            this.dataColumnReleased.Name = "dataColumnReleased";
            this.dataColumnReleased.ReadOnly = true;
            // 
            // dataColumnStability
            // 
            this.dataColumnStability.DataPropertyName = "Stability";
            resources.ApplyResources(this.dataColumnStability, "dataColumnStability");
            this.dataColumnStability.Name = "dataColumnStability";
            this.dataColumnStability.ReadOnly = true;
            this.dataColumnStability.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataColumnUserStability
            // 
            this.dataColumnUserStability.DataPropertyName = "UserStability";
            resources.ApplyResources(this.dataColumnUserStability, "dataColumnUserStability");
            this.dataColumnUserStability.Name = "dataColumnUserStability";
            this.dataColumnUserStability.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataColumnArchitecture
            // 
            this.dataColumnArchitecture.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataColumnArchitecture.DataPropertyName = "Architecture";
            resources.ApplyResources(this.dataColumnArchitecture, "dataColumnArchitecture");
            this.dataColumnArchitecture.Name = "dataColumnArchitecture";
            this.dataColumnArchitecture.ReadOnly = true;
            this.dataColumnArchitecture.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            // 
            // dataColumnNotes
            // 
            this.dataColumnNotes.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataColumnNotes.DataPropertyName = "Notes";
            resources.ApplyResources(this.dataColumnNotes, "dataColumnNotes");
            this.dataColumnNotes.Name = "dataColumnNotes";
            this.dataColumnNotes.ReadOnly = true;
            // 
            // dataColumnSource
            // 
            this.dataColumnSource.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataColumnSource.DataPropertyName = "FeedUri";
            resources.ApplyResources(this.dataColumnSource, "dataColumnSource");
            this.dataColumnSource.Name = "dataColumnSource";
            this.dataColumnSource.ReadOnly = true;
            // 
            // tabPageFeeds
            // 
            this.tabPageFeeds.Controls.Add(this.buttonRemoveFeed);
            this.tabPageFeeds.Controls.Add(this.buttonAddFeed);
            this.tabPageFeeds.Controls.Add(this.listBoxFeeds);
            resources.ApplyResources(this.tabPageFeeds, "tabPageFeeds");
            this.tabPageFeeds.Name = "tabPageFeeds";
            this.tabPageFeeds.UseVisualStyleBackColor = true;
            // 
            // buttonRemoveFeed
            // 
            resources.ApplyResources(this.buttonRemoveFeed, "buttonRemoveFeed");
            this.buttonRemoveFeed.Name = "buttonRemoveFeed";
            this.toolTip.SetToolTip(this.buttonRemoveFeed, resources.GetString("buttonRemoveFeed.ToolTip"));
            this.buttonRemoveFeed.UseVisualStyleBackColor = true;
            this.buttonRemoveFeed.Click += new System.EventHandler(this.buttonRemoveFeed_Click);
            // 
            // buttonAddFeed
            // 
            resources.ApplyResources(this.buttonAddFeed, "buttonAddFeed");
            this.buttonAddFeed.Name = "buttonAddFeed";
            this.toolTip.SetToolTip(this.buttonAddFeed, resources.GetString("buttonAddFeed.ToolTip"));
            this.buttonAddFeed.UseVisualStyleBackColor = true;
            this.buttonAddFeed.Click += new System.EventHandler(this.buttonAddFeed_Click);
            // 
            // listBoxFeeds
            // 
            this.listBoxFeeds.AllowDrop = true;
            resources.ApplyResources(this.listBoxFeeds, "listBoxFeeds");
            this.listBoxFeeds.DisplayMember = "Source";
            this.listBoxFeeds.FormattingEnabled = true;
            this.listBoxFeeds.Name = "listBoxFeeds";
            this.listBoxFeeds.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxFeeds.ValueMember = "Source";
            this.listBoxFeeds.SelectedIndexChanged += new System.EventHandler(this.listBoxFeeds_SelectedIndexChanged);
            this.listBoxFeeds.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBoxFeeds_DragDrop);
            this.listBoxFeeds.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBoxFeeds_DragEnter);
            // 
            // buttonApply
            // 
            resources.ApplyResources(this.buttonApply, "buttonApply");
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonOKApply_Click);
            // 
            // InterfaceDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.Name = "InterfaceDialog";
            this.Load += new System.EventHandler(this.InterfaceDialog_Load);
            this.Controls.SetChildIndex(this.tabControl, 0);
            this.Controls.SetChildIndex(this.buttonApply, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabControl.ResumeLayout(false);
            this.tabPageVersions.ResumeLayout(false);
            this.tabPageVersions.PerformLayout();
            this.panelStability.ResumeLayout(false);
            this.panelStability.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridVersions)).EndInit();
            this.tabPageFeeds.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.FlowLayoutPanel panelStability;
        private System.Windows.Forms.Button buttonAddFeed;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonRemoveFeed;
        private System.Windows.Forms.CheckBox checkBoxShowAllVersions;
        private System.Windows.Forms.ComboBox comboBoxStability;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnArchitecture;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnNotes;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnReleased;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnStability;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataColumnUserStability;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnVersion;
        private System.Windows.Forms.DataGridView dataGridVersions;
        private System.Windows.Forms.Label labelStability;
        private System.Windows.Forms.ListBox listBoxFeeds;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageFeeds;
        private System.Windows.Forms.TabPage tabPageVersions;
        private System.Windows.Forms.ToolTip toolTip;
        #endregion
    }
}
