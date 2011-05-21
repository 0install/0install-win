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
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dataGridVersions = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.buttonRemoveFeed = new System.Windows.Forms.Button();
            this.buttonAddFeed = new System.Windows.Forms.Button();
            this.listBoxFeeds = new System.Windows.Forms.ListBox();
            this.labelStability = new System.Windows.Forms.Label();
            this.comboBoxStability = new System.Windows.Forms.ComboBox();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridVersions)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(116, 247);
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(197, 247);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.Location = new System.Drawing.Point(13, 39);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(259, 202);
            this.tabControl.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridVersions);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(251, 176);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Versions";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dataGridVersions
            // 
            this.dataGridVersions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridVersions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridVersions.Location = new System.Drawing.Point(3, 3);
            this.dataGridVersions.Name = "dataGridVersions";
            this.dataGridVersions.Size = new System.Drawing.Size(245, 170);
            this.dataGridVersions.TabIndex = 1002;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.buttonRemoveFeed);
            this.tabPage2.Controls.Add(this.buttonAddFeed);
            this.tabPage2.Controls.Add(this.listBoxFeeds);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(251, 176);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Feeds";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // buttonRemoveFeed
            // 
            this.buttonRemoveFeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveFeed.Enabled = false;
            this.buttonRemoveFeed.Location = new System.Drawing.Point(170, 147);
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
            this.buttonAddFeed.Location = new System.Drawing.Point(89, 147);
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
            this.listBoxFeeds.Location = new System.Drawing.Point(3, 3);
            this.listBoxFeeds.Name = "listBoxFeeds";
            this.listBoxFeeds.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxFeeds.Size = new System.Drawing.Size(245, 134);
            this.listBoxFeeds.TabIndex = 0;
            this.listBoxFeeds.ValueMember = "Source";
            this.listBoxFeeds.SelectedIndexChanged += new System.EventHandler(this.listBoxFeeds_SelectedIndexChanged);
            this.listBoxFeeds.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBoxFeeds_DragDrop);
            this.listBoxFeeds.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBoxFeeds_DragEnter);
            // 
            // labelStability
            // 
            this.labelStability.AutoSize = true;
            this.labelStability.Location = new System.Drawing.Point(12, 15);
            this.labelStability.Name = "labelStability";
            this.labelStability.Size = new System.Drawing.Size(90, 13);
            this.labelStability.TabIndex = 0;
            this.labelStability.Text = "Preferred &stability:";
            // 
            // comboBoxStability
            // 
            this.comboBoxStability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStability.FormattingEnabled = true;
            this.comboBoxStability.Location = new System.Drawing.Point(108, 12);
            this.comboBoxStability.Name = "comboBoxStability";
            this.comboBoxStability.Size = new System.Drawing.Size(90, 21);
            this.comboBoxStability.TabIndex = 1;
            // 
            // InterfaceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 282);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.labelStability);
            this.Controls.Add(this.comboBoxStability);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "InterfaceDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Interface preferences";
            this.Controls.SetChildIndex(this.comboBoxStability, 0);
            this.Controls.SetChildIndex(this.labelStability, 0);
            this.Controls.SetChildIndex(this.tabControl, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridVersions)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridVersions;
        private System.Windows.Forms.Button buttonRemoveFeed;
        private System.Windows.Forms.Button buttonAddFeed;
        private System.Windows.Forms.ListBox listBoxFeeds;
        private System.Windows.Forms.Label labelStability;
        private System.Windows.Forms.ComboBox comboBoxStability;
    }
}