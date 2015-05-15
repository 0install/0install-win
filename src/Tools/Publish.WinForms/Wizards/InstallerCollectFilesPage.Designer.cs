namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class InstallerCollectFilesPage
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
            this.textBoxUrl = new NanoByte.Common.Controls.UriTextBox();
            this.labelLocalPath = new System.Windows.Forms.Label();
            this.buttonSelectPath = new System.Windows.Forms.Button();
            this.textBoxLocalPath = new NanoByte.Common.Controls.HintTextBox();
            this.buttonCreateArchive = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.labelUrl = new System.Windows.Forms.Label();
            this.buttonExistingArchive = new System.Windows.Forms.Button();
            this.groupCreateArchive = new System.Windows.Forms.GroupBox();
            this.groupCreateArchive.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 5);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(470, 37);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Collect files";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.AllowDrop = true;
            this.textBoxUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxUrl.ForeColor = System.Drawing.Color.Red;
            this.textBoxUrl.HintText = "HTTP/FTP URL";
            this.textBoxUrl.Location = new System.Drawing.Point(10, 148);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(396, 26);
            this.textBoxUrl.TabIndex = 4;
            this.textBoxUrl.TextChanged += new System.EventHandler(this.ToggleControls);
            // 
            // labelLocalPath
            // 
            this.labelLocalPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelLocalPath.Location = new System.Drawing.Point(6, 16);
            this.labelLocalPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLocalPath.Name = "labelLocalPath";
            this.labelLocalPath.Size = new System.Drawing.Size(400, 62);
            this.labelLocalPath.TabIndex = 0;
            this.labelLocalPath.Text = "The wizard can create a ZIP archive containing the installation directory for you" +
    ". Where do you want to place it?";
            // 
            // buttonSelectPath
            // 
            this.buttonSelectPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonSelectPath.Location = new System.Drawing.Point(377, 81);
            this.buttonSelectPath.Name = "buttonSelectPath";
            this.buttonSelectPath.Size = new System.Drawing.Size(29, 26);
            this.buttonSelectPath.TabIndex = 2;
            this.buttonSelectPath.Text = "...";
            this.buttonSelectPath.UseVisualStyleBackColor = true;
            this.buttonSelectPath.Click += new System.EventHandler(this.buttonSelectPath_Click);
            // 
            // textBoxLocalPath
            // 
            this.textBoxLocalPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxLocalPath.HintText = "File path";
            this.textBoxLocalPath.Location = new System.Drawing.Point(10, 81);
            this.textBoxLocalPath.Name = "textBoxLocalPath";
            this.textBoxLocalPath.Size = new System.Drawing.Size(361, 26);
            this.textBoxLocalPath.TabIndex = 1;
            this.textBoxLocalPath.TextChanged += new System.EventHandler(this.ToggleControls);
            // 
            // buttonCreateArchive
            // 
            this.buttonCreateArchive.Enabled = false;
            this.buttonCreateArchive.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonCreateArchive.Location = new System.Drawing.Point(296, 223);
            this.buttonCreateArchive.Name = "buttonCreateArchive";
            this.buttonCreateArchive.Size = new System.Drawing.Size(139, 35);
            this.buttonCreateArchive.TabIndex = 2;
            this.buttonCreateArchive.Text = "&Create archive >";
            this.buttonCreateArchive.UseVisualStyleBackColor = true;
            this.buttonCreateArchive.Click += new System.EventHandler(this.buttonCreateArchive_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "ZIP archive (*.zip)|*.zip";
            this.saveFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveFileDialog_FileOk);
            // 
            // labelUrl
            // 
            this.labelUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelUrl.Location = new System.Drawing.Point(6, 120);
            this.labelUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(400, 25);
            this.labelUrl.TabIndex = 3;
            this.labelUrl.Text = "Where will you upload this ZIP archive?";
            // 
            // buttonExistingArchive
            // 
            this.buttonExistingArchive.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonExistingArchive.Location = new System.Drawing.Point(29, 253);
            this.buttonExistingArchive.Name = "buttonExistingArchive";
            this.buttonExistingArchive.Size = new System.Drawing.Size(178, 35);
            this.buttonExistingArchive.TabIndex = 3;
            this.buttonExistingArchive.Text = "Use &existing archive >";
            this.buttonExistingArchive.UseVisualStyleBackColor = true;
            this.buttonExistingArchive.Click += new System.EventHandler(this.buttonExistingArchive_Click);
            // 
            // groupCreateArchive
            // 
            this.groupCreateArchive.Controls.Add(this.labelLocalPath);
            this.groupCreateArchive.Controls.Add(this.labelUrl);
            this.groupCreateArchive.Controls.Add(this.textBoxUrl);
            this.groupCreateArchive.Controls.Add(this.buttonSelectPath);
            this.groupCreateArchive.Controls.Add(this.textBoxLocalPath);
            this.groupCreateArchive.Location = new System.Drawing.Point(29, 44);
            this.groupCreateArchive.Name = "groupCreateArchive";
            this.groupCreateArchive.Size = new System.Drawing.Size(417, 197);
            this.groupCreateArchive.TabIndex = 1;
            this.groupCreateArchive.TabStop = false;
            // 
            // InstallerCollectFilesPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonExistingArchive);
            this.Controls.Add(this.buttonCreateArchive);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.groupCreateArchive);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "InstallerCollectFilesPage";
            this.Size = new System.Drawing.Size(470, 300);
            this.groupCreateArchive.ResumeLayout(false);
            this.groupCreateArchive.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private NanoByte.Common.Controls.UriTextBox textBoxUrl;
        private System.Windows.Forms.Label labelLocalPath;
        private System.Windows.Forms.Button buttonSelectPath;
        private NanoByte.Common.Controls.HintTextBox textBoxLocalPath;
        private System.Windows.Forms.Button buttonCreateArchive;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Label labelUrl;
        private System.Windows.Forms.Button buttonExistingArchive;
        private System.Windows.Forms.GroupBox groupCreateArchive;
    }
}
