namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class InstallerCaptureDiffPage
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonCapture = new System.Windows.Forms.Button();
            this.groupInstallationDir = new System.Windows.Forms.GroupBox();
            this.buttonSelectPath = new System.Windows.Forms.Button();
            this.textBoxInstallationDir = new NanoByte.Common.Controls.HintTextBox();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupInstallationDir.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 18);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(470, 37);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Installer Capture";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelInfo
            // 
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelInfo.Location = new System.Drawing.Point(35, 66);
            this.labelInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(400, 80);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Make sure the installer has finished installing the application. When you are rea" +
    "dy, continue to capture a second snapshot.";
            // 
            // buttonCapture
            // 
            this.buttonCapture.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonCapture.Location = new System.Drawing.Point(197, 244);
            this.buttonCapture.Name = "buttonCapture";
            this.buttonCapture.Size = new System.Drawing.Size(238, 35);
            this.buttonCapture.TabIndex = 2;
            this.buttonCapture.Text = "&Capture and compare >";
            this.buttonCapture.UseVisualStyleBackColor = true;
            this.buttonCapture.Click += new System.EventHandler(this.buttonCapture_Click);
            // 
            // groupInstallationDir
            // 
            this.groupInstallationDir.Controls.Add(this.buttonSelectPath);
            this.groupInstallationDir.Controls.Add(this.textBoxInstallationDir);
            this.groupInstallationDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.groupInstallationDir.Location = new System.Drawing.Point(39, 149);
            this.groupInstallationDir.Name = "groupInstallationDir";
            this.groupInstallationDir.Size = new System.Drawing.Size(396, 66);
            this.groupInstallationDir.TabIndex = 3;
            this.groupInstallationDir.TabStop = false;
            this.groupInstallationDir.Text = "Where did you install the application?";
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonSelectPath.Location = new System.Drawing.Point(360, 26);
            this.buttonSelectPath.Name = "buttonSelectPath";
            this.buttonSelectPath.Size = new System.Drawing.Size(29, 26);
            this.buttonSelectPath.TabIndex = 1;
            this.buttonSelectPath.Text = "...";
            this.buttonSelectPath.UseVisualStyleBackColor = true;
            this.buttonSelectPath.Click += new System.EventHandler(this.buttonSelectPath_Click);
            // 
            // textBoxInstallationDir
            // 
            this.textBoxInstallationDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxInstallationDir.HintText = "Directory path; leave empty to auto-detect";
            this.textBoxInstallationDir.Location = new System.Drawing.Point(6, 26);
            this.textBoxInstallationDir.Name = "textBoxInstallationDir";
            this.textBoxInstallationDir.Size = new System.Drawing.Size(348, 26);
            this.textBoxInstallationDir.TabIndex = 0;
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // InstallerCaptureDiffPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupInstallationDir);
            this.Controls.Add(this.buttonCapture);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelTitle);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "InstallerCaptureDiffPage";
            this.Size = new System.Drawing.Size(470, 300);
            this.groupInstallationDir.ResumeLayout(false);
            this.groupInstallationDir.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonCapture;
        private System.Windows.Forms.GroupBox groupInstallationDir;
        private System.Windows.Forms.Button buttonSelectPath;
        private NanoByte.Common.Controls.HintTextBox textBoxInstallationDir;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}
