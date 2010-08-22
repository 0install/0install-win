﻿namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class ArchiveControl
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Top folder");
            this.labelArchiveFormat = new System.Windows.Forms.Label();
            this.comboBoxArchiveFormat = new System.Windows.Forms.ComboBox();
            this.labelStartOffset = new System.Windows.Forms.Label();
            this.labelStartOffsetBytes = new System.Windows.Forms.Label();
            this.hintTextBox1 = new Common.Controls.HintTextBox();
            this.labelArchiveUrl = new System.Windows.Forms.Label();
            this.hintTextBoxArchiveUrl = new Common.Controls.HintTextBox();
            this.trackingProgressBarDownload = new Common.Controls.TrackingProgressBar();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.labelDownloadMessages = new System.Windows.Forms.Label();
            this.labelLocalArchive = new System.Windows.Forms.Label();
            this.hintTextBoxLocalArchive = new Common.Controls.HintTextBox();
            this.buttonLocalArchive = new System.Windows.Forms.Button();
            this.labelSubDirectory = new System.Windows.Forms.Label();
            this.treeViewSubDirectory = new System.Windows.Forms.TreeView();
            this.buttonExtractArchive = new System.Windows.Forms.Button();
            this.labelExtractArchiveMessages = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelArchiveFormat
            // 
            this.labelArchiveFormat.AutoSize = true;
            this.labelArchiveFormat.Location = new System.Drawing.Point(0, 0);
            this.labelArchiveFormat.Name = "labelArchiveFormat";
            this.labelArchiveFormat.Size = new System.Drawing.Size(75, 13);
            this.labelArchiveFormat.TabIndex = 0;
            this.labelArchiveFormat.Text = "Archive format";
            // 
            // comboBoxArchiveFormat
            // 
            this.comboBoxArchiveFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxArchiveFormat.FormattingEnabled = true;
            this.comboBoxArchiveFormat.Items.AddRange(new object[] {
            "(auto detect)",
            "application/x-rpm",
            "application/x-deb",
            "application/x-tar",
            "application/x-bzip-compressed-tar",
            "application/x-lzma-compressed-tar",
            "application/x-compressed-tar",
            "application/zip",
            "application/vnd.ms-cab-compressed"});
            this.comboBoxArchiveFormat.Location = new System.Drawing.Point(3, 16);
            this.comboBoxArchiveFormat.Name = "comboBoxArchiveFormat";
            this.comboBoxArchiveFormat.Size = new System.Drawing.Size(254, 21);
            this.comboBoxArchiveFormat.TabIndex = 1;
            // 
            // labelStartOffset
            // 
            this.labelStartOffset.AutoSize = true;
            this.labelStartOffset.Location = new System.Drawing.Point(0, 40);
            this.labelStartOffset.Name = "labelStartOffset";
            this.labelStartOffset.Size = new System.Drawing.Size(58, 13);
            this.labelStartOffset.TabIndex = 2;
            this.labelStartOffset.Text = "Start offset";
            // 
            // labelStartOffsetBytes
            // 
            this.labelStartOffsetBytes.AutoSize = true;
            this.labelStartOffsetBytes.Location = new System.Drawing.Point(224, 59);
            this.labelStartOffsetBytes.Name = "labelStartOffsetBytes";
            this.labelStartOffsetBytes.Size = new System.Drawing.Size(33, 13);
            this.labelStartOffsetBytes.TabIndex = 3;
            this.labelStartOffsetBytes.Text = "Bytes";
            // 
            // hintTextBox1
            // 
            this.hintTextBox1.HintText = "";
            this.hintTextBox1.Location = new System.Drawing.Point(3, 56);
            this.hintTextBox1.Name = "hintTextBox1";
            this.hintTextBox1.Size = new System.Drawing.Size(215, 20);
            this.hintTextBox1.TabIndex = 4;
            // 
            // labelArchiveUrl
            // 
            this.labelArchiveUrl.AutoSize = true;
            this.labelArchiveUrl.Location = new System.Drawing.Point(0, 79);
            this.labelArchiveUrl.Name = "labelArchiveUrl";
            this.labelArchiveUrl.Size = new System.Drawing.Size(59, 13);
            this.labelArchiveUrl.TabIndex = 5;
            this.labelArchiveUrl.Text = "Archive Url";
            // 
            // hintTextBoxArchiveUrl
            // 
            this.hintTextBoxArchiveUrl.HintText = "";
            this.hintTextBoxArchiveUrl.Location = new System.Drawing.Point(3, 95);
            this.hintTextBoxArchiveUrl.Name = "hintTextBoxArchiveUrl";
            this.hintTextBoxArchiveUrl.Size = new System.Drawing.Size(254, 20);
            this.hintTextBoxArchiveUrl.TabIndex = 6;
            // 
            // trackingProgressBarDownload
            // 
            this.trackingProgressBarDownload.Location = new System.Drawing.Point(3, 121);
            this.trackingProgressBarDownload.Name = "trackingProgressBarDownload";
            this.trackingProgressBarDownload.Size = new System.Drawing.Size(180, 10);
            this.trackingProgressBarDownload.TabIndex = 7;
            this.trackingProgressBarDownload.Task = null;
            // 
            // buttonDownload
            // 
            this.buttonDownload.Location = new System.Drawing.Point(189, 121);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(68, 23);
            this.buttonDownload.TabIndex = 8;
            this.buttonDownload.Text = "Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            // 
            // labelDownloadMessages
            // 
            this.labelDownloadMessages.AutoSize = true;
            this.labelDownloadMessages.Location = new System.Drawing.Point(0, 131);
            this.labelDownloadMessages.Name = "labelDownloadMessages";
            this.labelDownloadMessages.Size = new System.Drawing.Size(0, 13);
            this.labelDownloadMessages.TabIndex = 9;
            // 
            // labelLocalArchive
            // 
            this.labelLocalArchive.AutoSize = true;
            this.labelLocalArchive.Location = new System.Drawing.Point(0, 147);
            this.labelLocalArchive.Name = "labelLocalArchive";
            this.labelLocalArchive.Size = new System.Drawing.Size(71, 13);
            this.labelLocalArchive.TabIndex = 10;
            this.labelLocalArchive.Text = "Local archive";
            // 
            // hintTextBoxLocalArchive
            // 
            this.hintTextBoxLocalArchive.HintText = "";
            this.hintTextBoxLocalArchive.Location = new System.Drawing.Point(3, 163);
            this.hintTextBoxLocalArchive.Name = "hintTextBoxLocalArchive";
            this.hintTextBoxLocalArchive.ReadOnly = true;
            this.hintTextBoxLocalArchive.Size = new System.Drawing.Size(254, 20);
            this.hintTextBoxLocalArchive.TabIndex = 11;
            // 
            // buttonLocalArchive
            // 
            this.buttonLocalArchive.Location = new System.Drawing.Point(162, 189);
            this.buttonLocalArchive.Name = "buttonLocalArchive";
            this.buttonLocalArchive.Size = new System.Drawing.Size(95, 23);
            this.buttonLocalArchive.TabIndex = 12;
            this.buttonLocalArchive.Text = "Choose archive";
            this.buttonLocalArchive.UseVisualStyleBackColor = true;
            // 
            // labelSubDirectory
            // 
            this.labelSubDirectory.AutoSize = true;
            this.labelSubDirectory.Location = new System.Drawing.Point(0, 215);
            this.labelSubDirectory.Name = "labelSubDirectory";
            this.labelSubDirectory.Size = new System.Drawing.Size(66, 13);
            this.labelSubDirectory.TabIndex = 13;
            this.labelSubDirectory.Text = "Subdirectory";
            // 
            // treeViewSubDirectory
            // 
            this.treeViewSubDirectory.Location = new System.Drawing.Point(3, 231);
            this.treeViewSubDirectory.Name = "treeViewSubDirectory";
            treeNode1.Name = "rootNode";
            treeNode1.Text = "Top folder";
            this.treeViewSubDirectory.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewSubDirectory.PathSeparator = "/";
            this.treeViewSubDirectory.ShowLines = false;
            this.treeViewSubDirectory.ShowRootLines = false;
            this.treeViewSubDirectory.Size = new System.Drawing.Size(254, 133);
            this.treeViewSubDirectory.TabIndex = 14;
            // 
            // buttonExtractArchive
            // 
            this.buttonExtractArchive.Location = new System.Drawing.Point(162, 370);
            this.buttonExtractArchive.Name = "buttonExtractArchive";
            this.buttonExtractArchive.Size = new System.Drawing.Size(95, 23);
            this.buttonExtractArchive.TabIndex = 15;
            this.buttonExtractArchive.Text = "Extract archive";
            this.buttonExtractArchive.UseVisualStyleBackColor = true;
            // 
            // labelExtractArchiveMessages
            // 
            this.labelExtractArchiveMessages.AutoSize = true;
            this.labelExtractArchiveMessages.Location = new System.Drawing.Point(0, 367);
            this.labelExtractArchiveMessages.Name = "labelExtractArchiveMessages";
            this.labelExtractArchiveMessages.Size = new System.Drawing.Size(0, 13);
            this.labelExtractArchiveMessages.TabIndex = 16;
            // 
            // ArchiveControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelExtractArchiveMessages);
            this.Controls.Add(this.buttonExtractArchive);
            this.Controls.Add(this.treeViewSubDirectory);
            this.Controls.Add(this.labelSubDirectory);
            this.Controls.Add(this.buttonLocalArchive);
            this.Controls.Add(this.hintTextBoxLocalArchive);
            this.Controls.Add(this.labelLocalArchive);
            this.Controls.Add(this.labelDownloadMessages);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.trackingProgressBarDownload);
            this.Controls.Add(this.hintTextBoxArchiveUrl);
            this.Controls.Add(this.labelArchiveUrl);
            this.Controls.Add(this.hintTextBox1);
            this.Controls.Add(this.labelStartOffsetBytes);
            this.Controls.Add(this.labelStartOffset);
            this.Controls.Add(this.comboBoxArchiveFormat);
            this.Controls.Add(this.labelArchiveFormat);
            this.Name = "ArchiveControl";
            this.Size = new System.Drawing.Size(257, 395);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelArchiveFormat;
        private System.Windows.Forms.ComboBox comboBoxArchiveFormat;
        private System.Windows.Forms.Label labelStartOffset;
        private System.Windows.Forms.Label labelStartOffsetBytes;
        private Common.Controls.HintTextBox hintTextBox1;
        private System.Windows.Forms.Label labelArchiveUrl;
        private Common.Controls.HintTextBox hintTextBoxArchiveUrl;
        private Common.Controls.TrackingProgressBar trackingProgressBarDownload;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Label labelDownloadMessages;
        private System.Windows.Forms.Label labelLocalArchive;
        private Common.Controls.HintTextBox hintTextBoxLocalArchive;
        private System.Windows.Forms.Button buttonLocalArchive;
        private System.Windows.Forms.Label labelSubDirectory;
        private System.Windows.Forms.TreeView treeViewSubDirectory;
        private System.Windows.Forms.Button buttonExtractArchive;
        private System.Windows.Forms.Label labelExtractArchiveMessages;


    }
}
