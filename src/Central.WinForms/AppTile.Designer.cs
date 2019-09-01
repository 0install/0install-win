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
            this.buttonRun2 = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonRunWithOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.runMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.buttonUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.iconDownloadWorker = new System.ComponentModel.BackgroundWorker();
            this.buttonIntegrate = new NanoByte.Common.Controls.DropDownButton();
            this.contextMenuIntegrate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonIntegrate2 = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.contextMenuRun.SuspendLayout();
            this.contextMenuIntegrate.SuspendLayout();
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
            this.buttonRun2,
            this.buttonRunWithOptions,
            this.runMenuSeparator,
            this.buttonUpdate});
            this.contextMenuRun.Name = "contextMenuRun";
            this.contextMenuRun.Size = new System.Drawing.Size(181, 98);
            this.contextMenuRun.Text = "(Run)";
            // 
            // buttonRun2
            // 
            this.buttonRun2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonRun2.Name = "buttonRun2";
            this.buttonRun2.Size = new System.Drawing.Size(180, 22);
            this.buttonRun2.Text = "(Run)";
            this.buttonRun2.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonRunWithOptions
            // 
            this.buttonRunWithOptions.Name = "buttonRunWithOptions";
            this.buttonRunWithOptions.Size = new System.Drawing.Size(180, 22);
            this.buttonRunWithOptions.Text = "(Run with options)";
            this.buttonRunWithOptions.Visible = false;
            this.buttonRunWithOptions.Click += new System.EventHandler(this.buttonRunWithOptions_Click);
            // 
            // runMenuSeparator
            // 
            this.runMenuSeparator.Name = "runMenuSeparator";
            this.runMenuSeparator.Size = new System.Drawing.Size(177, 6);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(180, 22);
            this.buttonUpdate.Text = "(Update)";
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.AutoSize = true;
            this.buttonAdd.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonAdd.Location = new System.Drawing.Point(330, 32);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(63, 23);
            this.buttonAdd.TabIndex = 4;
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // iconDownloadWorker
            // 
            this.iconDownloadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.iconDownloadWorker_DoWork);
            this.iconDownloadWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.iconDownloadWorker_RunWorkerCompleted);
            // 
            // buttonIntegrate
            // 
            this.buttonIntegrate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIntegrate.AutoSize = true;
            this.buttonIntegrate.ContextMenuStrip = this.contextMenuIntegrate;
            this.buttonIntegrate.DropDownMenuStrip = this.contextMenuIntegrate;
            this.buttonIntegrate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonIntegrate.Location = new System.Drawing.Point(330, 32);
            this.buttonIntegrate.Name = "buttonIntegrate";
            this.buttonIntegrate.ShowSplit = true;
            this.buttonIntegrate.Size = new System.Drawing.Size(63, 23);
            this.buttonIntegrate.TabIndex = 5;
            this.buttonIntegrate.UseVisualStyleBackColor = true;
            this.buttonIntegrate.Click += new System.EventHandler(this.buttonIntegrate_Click);
            // 
            // contextMenuIntegrate
            // 
            this.contextMenuIntegrate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonIntegrate2,
            this.buttonRemove});
            this.contextMenuIntegrate.Name = "contextMenuIntegrate";
            this.contextMenuIntegrate.Size = new System.Drawing.Size(136, 48);
            // 
            // buttonIntegrate2
            // 
            this.buttonIntegrate2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonIntegrate2.Name = "buttonIntegrate2";
            this.buttonIntegrate2.Size = new System.Drawing.Size(135, 22);
            this.buttonIntegrate2.Text = "(Integrate)";
            this.buttonIntegrate2.Click += new System.EventHandler(this.buttonIntegrate_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(135, 22);
            this.buttonRemove.Text = "(Remove)";
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBoxIcon.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBoxIcon.Location = new System.Drawing.Point(6, 6);
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
            this.contextMenuIntegrate.ResumeLayout(false);
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
        private System.Windows.Forms.ToolStripMenuItem buttonRunWithOptions;
        private System.Windows.Forms.ToolStripSeparator runMenuSeparator;
        private System.Windows.Forms.ToolStripMenuItem buttonUpdate;
        internal NanoByte.Common.Controls.DropDownButton buttonIntegrate;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ContextMenuStrip contextMenuIntegrate;
        private System.Windows.Forms.ToolStripMenuItem buttonRemove;
        private System.Windows.Forms.ToolStripMenuItem buttonIntegrate2;
        private System.Windows.Forms.ToolStripMenuItem buttonRun2;
    }
}
