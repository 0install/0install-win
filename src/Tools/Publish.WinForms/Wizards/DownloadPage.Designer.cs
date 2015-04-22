namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class DownloadPage
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
            this.labelUrl = new System.Windows.Forms.Label();
            this.groupLocalCopy = new System.Windows.Forms.GroupBox();
            this.buttonSelectPath = new System.Windows.Forms.Button();
            this.textBoxLocalPath = new NanoByte.Common.Controls.HintTextBox();
            this.checkLocalCopy = new System.Windows.Forms.CheckBox();
            this.buttonNext = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupLocalCopy.SuspendLayout();
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
            this.labelTitle.Text = "Download";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.AllowDrop = true;
            this.textBoxUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxUrl.ForeColor = System.Drawing.Color.Red;
            this.textBoxUrl.HintText = "HTTP/FTP URL";
            this.textBoxUrl.Location = new System.Drawing.Point(39, 115);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(396, 26);
            this.textBoxUrl.TabIndex = 2;
            this.textBoxUrl.TextChanged += new System.EventHandler(this.ToggleControls);
            // 
            // labelUrl
            // 
            this.labelUrl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelUrl.Location = new System.Drawing.Point(35, 68);
            this.labelUrl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(400, 44);
            this.labelUrl.TabIndex = 1;
            this.labelUrl.Text = "Where can the current version of the application be downloaded? (.zip, .tar.gz, ." +
    "msi, .exe, .jar, ...)";
            // 
            // groupLocalCopy
            // 
            this.groupLocalCopy.Controls.Add(this.buttonSelectPath);
            this.groupLocalCopy.Controls.Add(this.textBoxLocalPath);
            this.groupLocalCopy.Enabled = false;
            this.groupLocalCopy.Location = new System.Drawing.Point(39, 160);
            this.groupLocalCopy.Name = "groupLocalCopy";
            this.groupLocalCopy.Size = new System.Drawing.Size(396, 65);
            this.groupLocalCopy.TabIndex = 4;
            this.groupLocalCopy.TabStop = false;
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
            // textBoxLocalPath
            // 
            this.textBoxLocalPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxLocalPath.HintText = "File path";
            this.textBoxLocalPath.Location = new System.Drawing.Point(11, 26);
            this.textBoxLocalPath.Name = "textBoxLocalPath";
            this.textBoxLocalPath.Size = new System.Drawing.Size(343, 26);
            this.textBoxLocalPath.TabIndex = 0;
            this.textBoxLocalPath.TextChanged += new System.EventHandler(this.ToggleControls);
            // 
            // checkLocalCopy
            // 
            this.checkLocalCopy.AutoSize = true;
            this.checkLocalCopy.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.checkLocalCopy.Location = new System.Drawing.Point(50, 154);
            this.checkLocalCopy.Name = "checkLocalCopy";
            this.checkLocalCopy.Size = new System.Drawing.Size(228, 24);
            this.checkLocalCopy.TabIndex = 3;
            this.checkLocalCopy.Text = "I have a &local copy of this file";
            this.checkLocalCopy.UseVisualStyleBackColor = true;
            this.checkLocalCopy.CheckedChanged += new System.EventHandler(this.ToggleControls);
            // 
            // buttonNext
            // 
            this.buttonNext.Enabled = false;
            this.buttonNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonNext.Location = new System.Drawing.Point(315, 238);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(120, 35);
            this.buttonNext.TabIndex = 5;
            this.buttonNext.Text = "&Next >";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openFileDialog_FileOk);
            // 
            // DownloadPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkLocalCopy);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.groupLocalCopy);
            this.Controls.Add(this.textBoxUrl);
            this.Controls.Add(this.labelUrl);
            this.Controls.Add(this.labelTitle);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DownloadPage";
            this.Size = new System.Drawing.Size(470, 300);
            this.groupLocalCopy.ResumeLayout(false);
            this.groupLocalCopy.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private NanoByte.Common.Controls.UriTextBox textBoxUrl;
        private System.Windows.Forms.Label labelUrl;
        private System.Windows.Forms.GroupBox groupLocalCopy;
        private System.Windows.Forms.Button buttonSelectPath;
        private NanoByte.Common.Controls.HintTextBox textBoxLocalPath;
        private System.Windows.Forms.CheckBox checkLocalCopy;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}
