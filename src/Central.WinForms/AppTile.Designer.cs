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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AppTile));
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.labelName = new System.Windows.Forms.Label();
            this.labelSummary = new System.Windows.Forms.Label();
            this.buttonRun = new Common.Controls.SplitButton();
            this.contextMenuRun = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonSelectCommand = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSelectVersion = new System.Windows.Forms.ToolStripMenuItem();
            this.runMenuSeperator = new System.Windows.Forms.ToolStripSeparator();
            this.buttonUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.linkLabelDetails = new System.Windows.Forms.LinkLabel();
            this.trackingProgressBar = new Common.Controls.TrackingProgressBar();
            this.iconDownloadWorker = new System.ComponentModel.BackgroundWorker();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonIntegrate = new System.Windows.Forms.Button();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.contextMenuRun.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxIcon
            // 
            resources.ApplyResources(this.pictureBoxIcon, "pictureBoxIcon");
            this.pictureBoxIcon.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.App;
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.TabStop = false;
            // 
            // labelName
            // 
            resources.ApplyResources(this.labelName, "labelName");
            this.labelName.Name = "labelName";
            // 
            // labelSummary
            // 
            resources.ApplyResources(this.labelSummary, "labelSummary");
            this.labelSummary.Name = "labelSummary";
            // 
            // buttonRun
            // 
            resources.ApplyResources(this.buttonRun, "buttonRun");
            this.buttonRun.ContextMenuStrip = this.contextMenuRun;
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.ShowSplit = true;
            this.buttonRun.SplitMenuStrip = this.contextMenuRun;
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // contextMenuRun
            // 
            this.contextMenuRun.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonSelectCommand,
            this.buttonSelectVersion,
            this.runMenuSeperator,
            this.buttonUpdate});
            this.contextMenuRun.Name = "contextMenuRun";
            resources.ApplyResources(this.contextMenuRun, "contextMenuRun");
            // 
            // buttonSelectCommand
            // 
            this.buttonSelectCommand.Name = "buttonSelectCommand";
            resources.ApplyResources(this.buttonSelectCommand, "buttonSelectCommand");
            this.buttonSelectCommand.Click += new System.EventHandler(this.buttonSelectCommmand_Click);
            // 
            // buttonSelectVersion
            // 
            this.buttonSelectVersion.Name = "buttonSelectVersion";
            resources.ApplyResources(this.buttonSelectVersion, "buttonSelectVersion");
            this.buttonSelectVersion.Click += new System.EventHandler(this.buttonSelectVersion_Click);
            // 
            // runMenuSeperator
            // 
            this.runMenuSeperator.Name = "runMenuSeperator";
            resources.ApplyResources(this.runMenuSeperator, "runMenuSeperator");
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Name = "buttonUpdate";
            resources.ApplyResources(this.buttonUpdate, "buttonUpdate");
            this.buttonUpdate.Click += new System.EventHandler(this.buttonUpdate_Click);
            // 
            // buttonAdd
            // 
            resources.ApplyResources(this.buttonAdd, "buttonAdd");
            this.buttonAdd.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.AddButton;
            this.buttonAdd.Name = "buttonAdd";
            this.toolTip.SetToolTip(this.buttonAdd, resources.GetString("buttonAdd.ToolTip"));
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // linkLabelDetails
            // 
            resources.ApplyResources(this.linkLabelDetails, "linkLabelDetails");
            this.linkLabelDetails.Name = "linkLabelDetails";
            this.linkLabelDetails.TabStop = true;
            this.linkLabelDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDetails_LinkClicked);
            // 
            // trackingProgressBar
            // 
            resources.ApplyResources(this.trackingProgressBar, "trackingProgressBar");
            this.trackingProgressBar.Name = "trackingProgressBar";
            // 
            // iconDownloadWorker
            // 
            this.iconDownloadWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.iconDownloadWorker_DoWork);
            this.iconDownloadWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.iconDownloadWorker_RunWorkerCompleted);
            // 
            // buttonRemove
            // 
            resources.ApplyResources(this.buttonRemove, "buttonRemove");
            this.buttonRemove.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.RemoveButton;
            this.buttonRemove.Name = "buttonRemove";
            this.toolTip.SetToolTip(this.buttonRemove, resources.GetString("buttonRemove.ToolTip"));
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonIntegrate
            // 
            resources.ApplyResources(this.buttonIntegrate, "buttonIntegrate");
            this.buttonIntegrate.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.SetupButton;
            this.buttonIntegrate.Name = "buttonIntegrate";
            this.toolTip.SetToolTip(this.buttonIntegrate, resources.GetString("buttonIntegrate.ToolTip"));
            this.buttonIntegrate.UseVisualStyleBackColor = true;
            this.buttonIntegrate.Click += new System.EventHandler(this.buttonIntegrate_Click);
            // 
            // AppTile
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trackingProgressBar);
            this.Controls.Add(this.linkLabelDetails);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelSummary);
            this.Controls.Add(this.pictureBoxIcon);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonIntegrate);
            this.MaximumSize = new System.Drawing.Size(4096, 60);
            this.MinimumSize = new System.Drawing.Size(220, 60);
            this.Name = "AppTile";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.contextMenuRun.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelSummary;
        private Common.Controls.SplitButton buttonRun;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.LinkLabel linkLabelDetails;
        private Common.Controls.TrackingProgressBar trackingProgressBar;
        private System.ComponentModel.BackgroundWorker iconDownloadWorker;
        private System.Windows.Forms.ContextMenuStrip contextMenuRun;
        private System.Windows.Forms.ToolStripMenuItem buttonSelectVersion;
        private System.Windows.Forms.ToolStripMenuItem buttonSelectCommand;
        private System.Windows.Forms.ToolStripSeparator runMenuSeperator;
        private System.Windows.Forms.ToolStripMenuItem buttonUpdate;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonIntegrate;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
