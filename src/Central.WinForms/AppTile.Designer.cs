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
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.labelName = new System.Windows.Forms.Label();
            this.labelSummary = new System.Windows.Forms.Label();
            this.buttonRun = new Common.Controls.SplitButton();
            this.contextMenuRun = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonSelectVersion = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSelectComponent = new System.Windows.Forms.ToolStripMenuItem();
            this.runMenuSeperator = new System.Windows.Forms.ToolStripSeparator();
            this.buttonUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonConf = new Common.Controls.SplitButton();
            this.contextMenuManage = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAdd = new Common.Controls.SplitButton();
            this.contextMenuAdd = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonIntegrate = new System.Windows.Forms.ToolStripMenuItem();
            this.linkLabelDetails = new System.Windows.Forms.LinkLabel();
            this.trackingProgressBar = new Common.Controls.TrackingProgressBar();
            this.iconDownloadWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.contextMenuRun.SuspendLayout();
            this.contextMenuManage.SuspendLayout();
            this.contextMenuAdd.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.pictureBoxIcon.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.App;
            this.pictureBoxIcon.Location = new System.Drawing.Point(6, 6);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxIcon.TabIndex = 0;
            this.pictureBoxIcon.TabStop = false;
            // 
            // labelName
            // 
            this.labelName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelName.Location = new System.Drawing.Point(60, 6);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(258, 18);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "(Name)";
            // 
            // labelSummary
            // 
            this.labelSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSummary.Location = new System.Drawing.Point(60, 24);
            this.labelSummary.Name = "labelSummary";
            this.labelSummary.Size = new System.Drawing.Size(258, 26);
            this.labelSummary.TabIndex = 1;
            this.labelSummary.Text = "(Summary)";
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.AutoSize = true;
            this.buttonRun.ContextMenuStrip = this.contextMenuRun;
            this.buttonRun.Location = new System.Drawing.Point(324, 6);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.ShowSplit = true;
            this.buttonRun.Size = new System.Drawing.Size(70, 23);
            this.buttonRun.SplitMenuStrip = this.contextMenuRun;
            this.buttonRun.TabIndex = 4;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // contextMenuRun
            // 
            this.contextMenuRun.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSelectVersion,
            this.buttonSelectComponent,
            this.runMenuSeperator,
            this.buttonUpdate});
            this.contextMenuRun.Name = "contextMenuRun";
            this.contextMenuRun.Size = new System.Drawing.Size(171, 76);
            // 
            // buttonSelectVersion
            // 
            this.buttonSelectVersion.Name = "buttonSelectVersion";
            this.buttonSelectVersion.Size = new System.Drawing.Size(170, 22);
            this.buttonSelectVersion.Text = "Select &version";
            this.buttonSelectVersion.Click += new System.EventHandler(this.buttonSelectVersion_Click);
            // 
            // buttonSelectComponent
            // 
            this.buttonSelectComponent.Name = "buttonSelectComponent";
            this.buttonSelectComponent.Size = new System.Drawing.Size(170, 22);
            this.buttonSelectComponent.Text = "Select &component";
            this.buttonSelectComponent.Click += new System.EventHandler(this.buttonSelectComponent_Click);
            // 
            // runMenuSeperator
            // 
            this.runMenuSeperator.Name = "runMenuSeperator";
            this.runMenuSeperator.Size = new System.Drawing.Size(167, 6);
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(170, 22);
            this.buttonUpdate.Text = "&Update";
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonConf
            // 
            this.buttonConf.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConf.AutoSize = true;
            this.buttonConf.ContextMenuStrip = this.contextMenuManage;
            this.buttonConf.Location = new System.Drawing.Point(324, 31);
            this.buttonConf.Name = "buttonConf";
            this.buttonConf.ShowSplit = true;
            this.buttonConf.Size = new System.Drawing.Size(70, 23);
            this.buttonConf.SplitMenuStrip = this.contextMenuManage;
            this.buttonConf.TabIndex = 6;
            this.buttonConf.Text = "Conf";
            this.buttonConf.UseVisualStyleBackColor = true;
            this.buttonConf.Visible = false;
            this.buttonConf.Click += new System.EventHandler(this.buttonConf_Click);
            // 
            // contextMenuManage
            // 
            this.contextMenuManage.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonRemove});
            this.contextMenuManage.Name = "contextMenuManage";
            this.contextMenuManage.Size = new System.Drawing.Size(118, 26);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(117, 22);
            this.buttonRemove.Text = "&Remove";
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.AutoSize = true;
            this.buttonAdd.ContextMenuStrip = this.contextMenuAdd;
            this.buttonAdd.Location = new System.Drawing.Point(324, 31);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.ShowSplit = true;
            this.buttonAdd.Size = new System.Drawing.Size(70, 23);
            this.buttonAdd.SplitMenuStrip = this.contextMenuAdd;
            this.buttonAdd.TabIndex = 5;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // contextMenuAdd
            // 
            this.contextMenuAdd.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonIntegrate});
            this.contextMenuAdd.Name = "contextMenuAdd";
            this.contextMenuAdd.Size = new System.Drawing.Size(122, 26);
            // 
            // buttonIntegrate
            // 
            this.buttonIntegrate.Name = "buttonIntegrate";
            this.buttonIntegrate.Size = new System.Drawing.Size(121, 22);
            this.buttonIntegrate.Text = "&Integrate";
            this.buttonIntegrate.Click += new System.EventHandler(this.buttonIntegrate_Click);
            // 
            // linkLabelDetails
            // 
            this.linkLabelDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelDetails.AutoSize = true;
            this.linkLabelDetails.Location = new System.Drawing.Point(278, 37);
            this.linkLabelDetails.Name = "linkLabelDetails";
            this.linkLabelDetails.Size = new System.Drawing.Size(40, 13);
            this.linkLabelDetails.TabIndex = 2;
            this.linkLabelDetails.TabStop = true;
            this.linkLabelDetails.Text = "More...";
            this.linkLabelDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDetails_LinkClicked);
            // 
            // trackingProgressBar
            // 
            this.trackingProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackingProgressBar.Location = new System.Drawing.Point(63, 32);
            this.trackingProgressBar.Name = "trackingProgressBar";
            this.trackingProgressBar.Size = new System.Drawing.Size(255, 18);
            this.trackingProgressBar.TabIndex = 3;
            this.trackingProgressBar.Visible = false;
            // 
            // iconDownloadWorker
            // 
            this.iconDownloadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.iconDownloadWorker_DoWork);
            this.iconDownloadWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.iconDownloadWorker_RunWorkerCompleted);
            // 
            // AppTile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonConf);
            this.Controls.Add(this.linkLabelDetails);
            this.Controls.Add(this.trackingProgressBar);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelSummary);
            this.Controls.Add(this.pictureBoxIcon);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonRun);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximumSize = new System.Drawing.Size(4096, 60);
            this.MinimumSize = new System.Drawing.Size(220, 60);
            this.Name = "AppTile";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(400, 60);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.contextMenuRun.ResumeLayout(false);
            this.contextMenuManage.ResumeLayout(false);
            this.contextMenuAdd.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelSummary;
        private Common.Controls.SplitButton buttonRun;
        private Common.Controls.SplitButton buttonConf;
        private Common.Controls.SplitButton buttonAdd;
        private System.Windows.Forms.ContextMenuStrip contextMenuManage;
        private System.Windows.Forms.ToolStripMenuItem buttonRemove;
        private System.Windows.Forms.LinkLabel linkLabelDetails;
        private Common.Controls.TrackingProgressBar trackingProgressBar;
        private System.ComponentModel.BackgroundWorker iconDownloadWorker;
        private System.Windows.Forms.ContextMenuStrip contextMenuRun;
        private System.Windows.Forms.ToolStripMenuItem buttonSelectVersion;
        private System.Windows.Forms.ToolStripMenuItem buttonSelectComponent;
        private System.Windows.Forms.ToolStripSeparator runMenuSeperator;
        private System.Windows.Forms.ToolStripMenuItem buttonUpdate;
        private System.Windows.Forms.ContextMenuStrip contextMenuAdd;
        private System.Windows.Forms.ToolStripMenuItem buttonIntegrate;
    }
}
