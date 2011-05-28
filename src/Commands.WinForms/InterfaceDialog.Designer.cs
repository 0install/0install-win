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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageVersions = new System.Windows.Forms.TabPage();
            this.checkBoxShowAllVersions = new System.Windows.Forms.CheckBox();
            this.labelStability = new System.Windows.Forms.Label();
            this.comboBoxStability = new System.Windows.Forms.ComboBox();
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
            this.tabControl.SuspendLayout();
            this.tabPageVersions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridVersions)).BeginInit();
            this.tabPageFeeds.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(566, 310);
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(647, 310);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageVersions);
            this.tabControl.Controls.Add(this.tabPageFeeds);
            this.tabControl.Location = new System.Drawing.Point(13, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(709, 292);
            this.tabControl.TabIndex = 2;
            // 
            // tabPageVersions
            // 
            this.tabPageVersions.Controls.Add(this.checkBoxShowAllVersions);
            this.tabPageVersions.Controls.Add(this.labelStability);
            this.tabPageVersions.Controls.Add(this.comboBoxStability);
            this.tabPageVersions.Controls.Add(this.dataGridVersions);
            this.tabPageVersions.Location = new System.Drawing.Point(4, 22);
            this.tabPageVersions.Name = "tabPageVersions";
            this.tabPageVersions.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageVersions.Size = new System.Drawing.Size(701, 266);
            this.tabPageVersions.TabIndex = 0;
            this.tabPageVersions.Text = "Versions";
            this.tabPageVersions.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowAllVersions
            // 
            this.checkBoxShowAllVersions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowAllVersions.AutoSize = true;
            this.checkBoxShowAllVersions.Location = new System.Drawing.Point(587, 10);
            this.checkBoxShowAllVersions.Name = "checkBoxShowAllVersions";
            this.checkBoxShowAllVersions.Size = new System.Drawing.Size(108, 17);
            this.checkBoxShowAllVersions.TabIndex = 4;
            this.checkBoxShowAllVersions.Text = "Show all &versions";
            this.checkBoxShowAllVersions.UseVisualStyleBackColor = true;
            this.checkBoxShowAllVersions.CheckedChanged += new System.EventHandler(this.checkBoxShowAllVersions_CheckedChanged);
            // 
            // labelStability
            // 
            this.labelStability.AutoSize = true;
            this.labelStability.Location = new System.Drawing.Point(3, 9);
            this.labelStability.Name = "labelStability";
            this.labelStability.Size = new System.Drawing.Size(90, 13);
            this.labelStability.TabIndex = 2;
            this.labelStability.Text = "Preferred &stability:";
            // 
            // comboBoxStability
            // 
            this.comboBoxStability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStability.FormattingEnabled = true;
            this.comboBoxStability.Location = new System.Drawing.Point(99, 6);
            this.comboBoxStability.Name = "comboBoxStability";
            this.comboBoxStability.Size = new System.Drawing.Size(120, 21);
            this.comboBoxStability.TabIndex = 3;
            // 
            // dataGridVersions
            // 
            this.dataGridVersions.AllowUserToAddRows = false;
            this.dataGridVersions.AllowUserToDeleteRows = false;
            this.dataGridVersions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridVersions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridVersions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dataColumnVersion,
            this.dataColumnReleased,
            this.dataColumnStability,
            this.dataColumnUserStability,
            this.dataColumnArchitecture,
            this.dataColumnNotes,
            this.dataColumnSource});
            this.dataGridVersions.Location = new System.Drawing.Point(6, 33);
            this.dataGridVersions.Name = "dataGridVersions";
            this.dataGridVersions.Size = new System.Drawing.Size(689, 227);
            this.dataGridVersions.TabIndex = 0;
            // 
            // dataColumnVersion
            // 
            this.dataColumnVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataColumnVersion.DataPropertyName = "Version";
            this.dataColumnVersion.HeaderText = "Version";
            this.dataColumnVersion.Name = "dataColumnVersion";
            this.dataColumnVersion.ReadOnly = true;
            this.dataColumnVersion.Width = 67;
            // 
            // dataColumnReleased
            // 
            this.dataColumnReleased.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataColumnReleased.DataPropertyName = "Released";
            this.dataColumnReleased.HeaderText = "Released";
            this.dataColumnReleased.Name = "dataColumnReleased";
            this.dataColumnReleased.ReadOnly = true;
            this.dataColumnReleased.Width = 77;
            // 
            // dataColumnStability
            // 
            this.dataColumnStability.DataPropertyName = "Stability";
            this.dataColumnStability.HeaderText = "Stability";
            this.dataColumnStability.MinimumWidth = 70;
            this.dataColumnStability.Name = "dataColumnStability";
            this.dataColumnStability.ReadOnly = true;
            this.dataColumnStability.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataColumnStability.Width = 70;
            // 
            // dataColumnUserStability
            // 
            this.dataColumnUserStability.DataPropertyName = "UserStability";
            this.dataColumnUserStability.HeaderText = "Override";
            this.dataColumnUserStability.MinimumWidth = 80;
            this.dataColumnUserStability.Name = "dataColumnUserStability";
            this.dataColumnUserStability.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataColumnUserStability.Width = 80;
            // 
            // dataColumnArchitecture
            // 
            this.dataColumnArchitecture.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataColumnArchitecture.DataPropertyName = "Architecture";
            this.dataColumnArchitecture.HeaderText = "Architecture";
            this.dataColumnArchitecture.Name = "dataColumnArchitecture";
            this.dataColumnArchitecture.ReadOnly = true;
            this.dataColumnArchitecture.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.dataColumnArchitecture.Width = 89;
            // 
            // dataColumnNotes
            // 
            this.dataColumnNotes.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.dataColumnNotes.DataPropertyName = "Notes";
            this.dataColumnNotes.HeaderText = "Notes";
            this.dataColumnNotes.Name = "dataColumnNotes";
            this.dataColumnNotes.ReadOnly = true;
            this.dataColumnNotes.Width = 60;
            // 
            // dataColumnSource
            // 
            this.dataColumnSource.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dataColumnSource.DataPropertyName = "FeedID";
            this.dataColumnSource.HeaderText = "Source";
            this.dataColumnSource.MinimumWidth = 50;
            this.dataColumnSource.Name = "dataColumnSource";
            this.dataColumnSource.ReadOnly = true;
            // 
            // tabPageFeeds
            // 
            this.tabPageFeeds.Controls.Add(this.buttonRemoveFeed);
            this.tabPageFeeds.Controls.Add(this.buttonAddFeed);
            this.tabPageFeeds.Controls.Add(this.listBoxFeeds);
            this.tabPageFeeds.Location = new System.Drawing.Point(4, 22);
            this.tabPageFeeds.Name = "tabPageFeeds";
            this.tabPageFeeds.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFeeds.Size = new System.Drawing.Size(701, 266);
            this.tabPageFeeds.TabIndex = 1;
            this.tabPageFeeds.Text = "Feeds";
            this.tabPageFeeds.UseVisualStyleBackColor = true;
            // 
            // buttonRemoveFeed
            // 
            this.buttonRemoveFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveFeed.Enabled = false;
            this.buttonRemoveFeed.Location = new System.Drawing.Point(619, 237);
            this.buttonRemoveFeed.Name = "buttonRemoveFeed";
            this.buttonRemoveFeed.Size = new System.Drawing.Size(75, 23);
            this.buttonRemoveFeed.TabIndex = 2;
            this.buttonRemoveFeed.Text = "&Remove";
            this.buttonRemoveFeed.UseVisualStyleBackColor = true;
            this.buttonRemoveFeed.Click += new System.EventHandler(this.buttonRemoveFeed_Click);
            // 
            // buttonAddFeed
            // 
            this.buttonAddFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddFeed.Location = new System.Drawing.Point(538, 237);
            this.buttonAddFeed.Name = "buttonAddFeed";
            this.buttonAddFeed.Size = new System.Drawing.Size(75, 23);
            this.buttonAddFeed.TabIndex = 1;
            this.buttonAddFeed.Text = "&Add";
            this.buttonAddFeed.UseVisualStyleBackColor = true;
            this.buttonAddFeed.Click += new System.EventHandler(this.buttonAddFeed_Click);
            // 
            // listBoxFeeds
            // 
            this.listBoxFeeds.AllowDrop = true;
            this.listBoxFeeds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxFeeds.DisplayMember = "Source";
            this.listBoxFeeds.FormattingEnabled = true;
            this.listBoxFeeds.Location = new System.Drawing.Point(6, 6);
            this.listBoxFeeds.Name = "listBoxFeeds";
            this.listBoxFeeds.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxFeeds.Size = new System.Drawing.Size(688, 225);
            this.listBoxFeeds.TabIndex = 0;
            this.listBoxFeeds.ValueMember = "Source";
            this.listBoxFeeds.SelectedIndexChanged += new System.EventHandler(this.listBoxFeeds_SelectedIndexChanged);
            this.listBoxFeeds.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBoxFeeds_DragDrop);
            this.listBoxFeeds.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBoxFeeds_DragEnter);
            // 
            // InterfaceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 345);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new System.Drawing.Size(300, 220);
            this.Name = "InterfaceDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Interface properties";
            this.Load += new System.EventHandler(this.InterfaceDialog_Load);
            this.Controls.SetChildIndex(this.tabControl, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabControl.ResumeLayout(false);
            this.tabPageVersions.ResumeLayout(false);
            this.tabPageVersions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridVersions)).EndInit();
            this.tabPageFeeds.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageVersions;
        private System.Windows.Forms.TabPage tabPageFeeds;
        private System.Windows.Forms.DataGridView dataGridVersions;
        private System.Windows.Forms.Button buttonRemoveFeed;
        private System.Windows.Forms.Button buttonAddFeed;
        private System.Windows.Forms.ListBox listBoxFeeds;
        private System.Windows.Forms.Label labelStability;
        private System.Windows.Forms.ComboBox comboBoxStability;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnReleased;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnStability;
        private System.Windows.Forms.DataGridViewComboBoxColumn dataColumnUserStability;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnArchitecture;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnNotes;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataColumnSource;
        private System.Windows.Forms.CheckBox checkBoxShowAllVersions;
    }
}