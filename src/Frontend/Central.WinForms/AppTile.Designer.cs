namespace ZeroInstall.Central.WinForms
{
    partial class AppTile
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelName = new System.Windows.Forms.Label();
            this.labelSummary = new System.Windows.Forms.Label();
            this.buttonRun = new NanoByte.Common.Controls.DropDownButton();
            this.contextMenuRun = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonSelectCommand = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSelectVersion = new System.Windows.Forms.ToolStripMenuItem();
            this.runMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.buttonUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.iconDownloadWorker = new System.ComponentModel.BackgroundWorker();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonIntegrate = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.contextMenuRun.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelName.AutoEllipsis = true;
            this.labelName.AutoSize = true;
            this.labelName.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
            this.labelName.Location = new System.Drawing.Point(60, 6);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(0, 18);
            this.labelName.TabIndex = 0;
            this.labelName.Click += new System.EventHandler(this.LinkClicked);
            // 
            // labelSummary
            // 
            this.labelSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSummary.AutoEllipsis = true;
            this.labelSummary.Location = new System.Drawing.Point(60, 24);
            this.labelSummary.Name = "labelSummary";
            this.labelSummary.Size = new System.Drawing.Size(264, 31);
            this.labelSummary.TabIndex = 1;
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.AutoSize = true;
            this.buttonRun.ContextMenuStrip = this.contextMenuRun;
            this.buttonRun.DropDownMenuStrip = this.contextMenuRun;
            this.buttonRun.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRun.Location = new System.Drawing.Point(330, 6);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.ShowSplit = true;
            this.buttonRun.Size = new System.Drawing.Size(63, 23);
            this.buttonRun.TabIndex = 3;
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // contextMenuRun
            // 
            this.contextMenuRun.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSelectCommand,
            this.buttonSelectVersion,
            this.runMenuSeparator,
            this.buttonUpdate});
            this.contextMenuRun.Name = "contextMenuRun";
            this.contextMenuRun.Size = new System.Drawing.Size(68, 76);
            // 
            // buttonSelectCommand
            // 
            this.buttonSelectCommand.Name = "buttonSelectCommand";
            this.buttonSelectCommand.Size = new System.Drawing.Size(67, 22);
            this.buttonSelectCommand.Visible = false;
            this.buttonSelectCommand.Click += new System.EventHandler(this.buttonSelectCommand_Click);
            // 
            // buttonSelectVersion
            // 
            this.buttonSelectVersion.Name = "buttonSelectVersion";
            this.buttonSelectVersion.Size = new System.Drawing.Size(67, 22);
            this.buttonSelectVersion.Click += new System.EventHandler(this.buttonSelectVersion_Click);
            // 
            // runMenuSeparator
            // 
            this.runMenuSeparator.Name = "runMenuSeparator";
            this.runMenuSeparator.Size = new System.Drawing.Size(64, 6);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(67, 22);
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.AutoSize = true;
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(330, 32);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(30, 23);
            this.buttonAdd.TabIndex = 4;
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // iconDownloadWorker
            // 
            this.iconDownloadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.iconDownloadWorker_DoWork);
            this.iconDownloadWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.iconDownloadWorker_RunWorkerCompleted);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.AutoSize = true;
            this.buttonRemove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonRemove.Location = new System.Drawing.Point(330, 32);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(30, 23);
            this.buttonRemove.TabIndex = 5;
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonIntegrate
            // 
            this.buttonIntegrate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIntegrate.AutoSize = true;
            this.buttonIntegrate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonIntegrate.Location = new System.Drawing.Point(363, 32);
            this.buttonIntegrate.Name = "buttonIntegrate";
            this.buttonIntegrate.Size = new System.Drawing.Size(30, 23);
            this.buttonIntegrate.TabIndex = 6;
            this.buttonIntegrate.UseVisualStyleBackColor = true;
            this.buttonIntegrate.Click += new System.EventHandler(this.buttonIntegrate_Click);
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBoxIcon.Location = new System.Drawing.Point(6, 6);
            this.pictureBoxIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxIcon.TabIndex = 0;
            this.pictureBoxIcon.TabStop = false;
            this.pictureBoxIcon.Click += new System.EventHandler(this.LinkClicked);
            // 
            // AppTile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonIntegrate);
            this.Controls.Add(this.pictureBoxIcon);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelSummary);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximumSize = new System.Drawing.Size(4096, 60);
            this.MinimumSize = new System.Drawing.Size(220, 60);
            this.Name = "AppTile";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(400, 60);
            this.contextMenuRun.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelSummary;
        internal NanoByte.Common.Controls.DropDownButton buttonRun;
        internal System.Windows.Forms.Button buttonAdd;
        private System.ComponentModel.BackgroundWorker iconDownloadWorker;
        private System.Windows.Forms.ContextMenuStrip contextMenuRun;
        private System.Windows.Forms.ToolStripMenuItem buttonSelectVersion;
        private System.Windows.Forms.ToolStripMenuItem buttonSelectCommand;
        private System.Windows.Forms.ToolStripSeparator runMenuSeparator;
        private System.Windows.Forms.ToolStripMenuItem buttonUpdate;
        internal System.Windows.Forms.Button buttonRemove;
        internal System.Windows.Forms.Button buttonIntegrate;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
