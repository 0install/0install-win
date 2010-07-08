namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class ArchiveForm
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
            this.labelExtractArchiveMessage = new System.Windows.Forms.Label();
            this.buttonExtractArchive = new System.Windows.Forms.Button();
            this.buttonChooseArchive = new System.Windows.Forms.Button();
            this.downloadProgressBarArchive = new Common.Controls.DownloadProgressBar();
            this.labelArchiveDownloadMessages = new System.Windows.Forms.Label();
            this.labelExtract = new System.Windows.Forms.Label();
            this.treeViewExtract = new System.Windows.Forms.TreeView();
            this.labelStartOffsetBytes = new System.Windows.Forms.Label();
            this.labelStartOffset = new System.Windows.Forms.Label();
            this.hintTextBoxStartOffset = new Common.Controls.HintTextBox();
            this.labelArchiveUrl = new System.Windows.Forms.Label();
            this.hintTextBoxLocalArchive = new Common.Controls.HintTextBox();
            this.labelLocalArchive = new System.Windows.Forms.Label();
            this.buttonArchiveDownload = new System.Windows.Forms.Button();
            this.hintTextBoxArchiveUrl = new Common.Controls.HintTextBox();
            this.comboBoxArchiveFormat = new System.Windows.Forms.ComboBox();
            this.labelArchiveFormat = new System.Windows.Forms.Label();
            this.openFileDialogLocalArchive = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialogDownloadPath = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(116, 403);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(197, 403);
            this.buttonCancel.Click += new System.EventHandler(this.ButtonCancelClick);
            // 
            // labelExtractArchiveMessage
            // 
            this.labelExtractArchiveMessage.AutoSize = true;
            this.labelExtractArchiveMessage.Location = new System.Drawing.Point(12, 346);
            this.labelExtractArchiveMessage.Name = "labelExtractArchiveMessage";
            this.labelExtractArchiveMessage.Size = new System.Drawing.Size(0, 13);
            this.labelExtractArchiveMessage.TabIndex = 37;
            // 
            // buttonExtractArchive
            // 
            this.buttonExtractArchive.Enabled = false;
            this.buttonExtractArchive.Location = new System.Drawing.Point(181, 341);
            this.buttonExtractArchive.Name = "buttonExtractArchive";
            this.buttonExtractArchive.Size = new System.Drawing.Size(91, 23);
            this.buttonExtractArchive.TabIndex = 36;
            this.buttonExtractArchive.Text = "Extract archive";
            this.buttonExtractArchive.UseVisualStyleBackColor = true;
            this.buttonExtractArchive.Click += new System.EventHandler(this.ButtonExtractArchiveClick);
            // 
            // buttonChooseArchive
            // 
            this.buttonChooseArchive.Enabled = false;
            this.buttonChooseArchive.Location = new System.Drawing.Point(181, 195);
            this.buttonChooseArchive.Name = "buttonChooseArchive";
            this.buttonChooseArchive.Size = new System.Drawing.Size(91, 23);
            this.buttonChooseArchive.TabIndex = 35;
            this.buttonChooseArchive.Text = "Choose Archive";
            this.buttonChooseArchive.UseVisualStyleBackColor = true;
            this.buttonChooseArchive.Click += new System.EventHandler(this.ButtonChooseArchiveClick);
            // 
            // downloadProgressBarArchive
            // 
            this.downloadProgressBarArchive.Download = null;
            this.downloadProgressBarArchive.Location = new System.Drawing.Point(15, 130);
            this.downloadProgressBarArchive.Name = "downloadProgressBarArchive";
            this.downloadProgressBarArchive.Size = new System.Drawing.Size(176, 10);
            this.downloadProgressBarArchive.TabIndex = 34;
            this.downloadProgressBarArchive.UseTaskbar = false;
            // 
            // labelArchiveDownloadMessages
            // 
            this.labelArchiveDownloadMessages.AutoSize = true;
            this.labelArchiveDownloadMessages.Location = new System.Drawing.Point(12, 140);
            this.labelArchiveDownloadMessages.Name = "labelArchiveDownloadMessages";
            this.labelArchiveDownloadMessages.Size = new System.Drawing.Size(0, 13);
            this.labelArchiveDownloadMessages.TabIndex = 33;
            // 
            // labelExtract
            // 
            this.labelExtract.AutoSize = true;
            this.labelExtract.Location = new System.Drawing.Point(12, 221);
            this.labelExtract.Name = "labelExtract";
            this.labelExtract.Size = new System.Drawing.Size(66, 13);
            this.labelExtract.TabIndex = 32;
            this.labelExtract.Text = "Subdirectory";
            // 
            // treeViewExtract
            // 
            this.treeViewExtract.Location = new System.Drawing.Point(15, 237);
            this.treeViewExtract.Name = "treeViewExtract";
            this.treeViewExtract.PathSeparator = "/";
            this.treeViewExtract.ShowRootLines = false;
            this.treeViewExtract.Size = new System.Drawing.Size(257, 98);
            this.treeViewExtract.TabIndex = 31;
            // 
            // labelStartOffsetBytes
            // 
            this.labelStartOffsetBytes.AutoSize = true;
            this.labelStartOffsetBytes.Location = new System.Drawing.Point(239, 68);
            this.labelStartOffsetBytes.Name = "labelStartOffsetBytes";
            this.labelStartOffsetBytes.Size = new System.Drawing.Size(33, 13);
            this.labelStartOffsetBytes.TabIndex = 30;
            this.labelStartOffsetBytes.Text = "Bytes";
            // 
            // labelStartOffset
            // 
            this.labelStartOffset.AutoSize = true;
            this.labelStartOffset.Location = new System.Drawing.Point(12, 49);
            this.labelStartOffset.Name = "labelStartOffset";
            this.labelStartOffset.Size = new System.Drawing.Size(58, 13);
            this.labelStartOffset.TabIndex = 29;
            this.labelStartOffset.Text = "Start-offset";
            // 
            // hintTextBoxStartOffset
            // 
            this.hintTextBoxStartOffset.HintText = "";
            this.hintTextBoxStartOffset.Location = new System.Drawing.Point(15, 65);
            this.hintTextBoxStartOffset.Name = "hintTextBoxStartOffset";
            this.hintTextBoxStartOffset.Size = new System.Drawing.Size(218, 20);
            this.hintTextBoxStartOffset.TabIndex = 28;
            this.hintTextBoxStartOffset.TextChanged += new System.EventHandler(this.HintTextBoxStartOffsetTextChanged);
            // 
            // labelArchiveUrl
            // 
            this.labelArchiveUrl.AutoSize = true;
            this.labelArchiveUrl.Location = new System.Drawing.Point(12, 88);
            this.labelArchiveUrl.Name = "labelArchiveUrl";
            this.labelArchiveUrl.Size = new System.Drawing.Size(57, 13);
            this.labelArchiveUrl.TabIndex = 27;
            this.labelArchiveUrl.Text = "Archive url";
            // 
            // hintTextBoxLocalArchive
            // 
            this.hintTextBoxLocalArchive.HintText = "";
            this.hintTextBoxLocalArchive.Location = new System.Drawing.Point(15, 169);
            this.hintTextBoxLocalArchive.Name = "hintTextBoxLocalArchive";
            this.hintTextBoxLocalArchive.ReadOnly = true;
            this.hintTextBoxLocalArchive.Size = new System.Drawing.Size(257, 20);
            this.hintTextBoxLocalArchive.TabIndex = 26;
            // 
            // labelLocalArchive
            // 
            this.labelLocalArchive.AutoSize = true;
            this.labelLocalArchive.Location = new System.Drawing.Point(12, 153);
            this.labelLocalArchive.Name = "labelLocalArchive";
            this.labelLocalArchive.Size = new System.Drawing.Size(71, 13);
            this.labelLocalArchive.TabIndex = 25;
            this.labelLocalArchive.Text = "Local archive";
            // 
            // buttonArchiveDownload
            // 
            this.buttonArchiveDownload.Enabled = false;
            this.buttonArchiveDownload.Location = new System.Drawing.Point(197, 130);
            this.buttonArchiveDownload.Name = "buttonArchiveDownload";
            this.buttonArchiveDownload.Size = new System.Drawing.Size(75, 23);
            this.buttonArchiveDownload.TabIndex = 24;
            this.buttonArchiveDownload.Text = "Download";
            this.buttonArchiveDownload.UseVisualStyleBackColor = true;
            this.buttonArchiveDownload.Click += new System.EventHandler(this.ButtonArchiveDownloadClick);
            // 
            // hintTextBoxArchiveUrl
            // 
            this.hintTextBoxArchiveUrl.HintText = "";
            this.hintTextBoxArchiveUrl.Location = new System.Drawing.Point(15, 104);
            this.hintTextBoxArchiveUrl.Name = "hintTextBoxArchiveUrl";
            this.hintTextBoxArchiveUrl.Size = new System.Drawing.Size(257, 20);
            this.hintTextBoxArchiveUrl.TabIndex = 23;
            this.hintTextBoxArchiveUrl.TextChanged += new System.EventHandler(this.HintTextBoxArchiveUrlTextChanged);
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
            this.comboBoxArchiveFormat.Location = new System.Drawing.Point(15, 25);
            this.comboBoxArchiveFormat.Name = "comboBoxArchiveFormat";
            this.comboBoxArchiveFormat.Size = new System.Drawing.Size(257, 21);
            this.comboBoxArchiveFormat.TabIndex = 22;
            // 
            // labelArchiveFormat
            // 
            this.labelArchiveFormat.AutoSize = true;
            this.labelArchiveFormat.Location = new System.Drawing.Point(12, 9);
            this.labelArchiveFormat.Name = "labelArchiveFormat";
            this.labelArchiveFormat.Size = new System.Drawing.Size(75, 13);
            this.labelArchiveFormat.TabIndex = 21;
            this.labelArchiveFormat.Text = "Archive format";
            // 
            // openFileDialogLocalArchive
            // 
            this.openFileDialogLocalArchive.Title = "Choose local archive";
            // 
            // folderBrowserDialogDownloadPath
            // 
            this.folderBrowserDialogDownloadPath.Description = "Select directory to download the archive into";
            this.folderBrowserDialogDownloadPath.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // ArchiveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 438);
            this.Controls.Add(this.labelExtractArchiveMessage);
            this.Controls.Add(this.buttonExtractArchive);
            this.Controls.Add(this.buttonChooseArchive);
            this.Controls.Add(this.downloadProgressBarArchive);
            this.Controls.Add(this.labelArchiveDownloadMessages);
            this.Controls.Add(this.labelExtract);
            this.Controls.Add(this.treeViewExtract);
            this.Controls.Add(this.labelStartOffsetBytes);
            this.Controls.Add(this.labelStartOffset);
            this.Controls.Add(this.hintTextBoxStartOffset);
            this.Controls.Add(this.labelArchiveUrl);
            this.Controls.Add(this.hintTextBoxLocalArchive);
            this.Controls.Add(this.labelLocalArchive);
            this.Controls.Add(this.buttonArchiveDownload);
            this.Controls.Add(this.hintTextBoxArchiveUrl);
            this.Controls.Add(this.comboBoxArchiveFormat);
            this.Controls.Add(this.labelArchiveFormat);
            this.Name = "ArchiveForm";
            this.Text = "ArchiveForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ArchiveForm_FormClosed);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.labelArchiveFormat, 0);
            this.Controls.SetChildIndex(this.comboBoxArchiveFormat, 0);
            this.Controls.SetChildIndex(this.hintTextBoxArchiveUrl, 0);
            this.Controls.SetChildIndex(this.buttonArchiveDownload, 0);
            this.Controls.SetChildIndex(this.labelLocalArchive, 0);
            this.Controls.SetChildIndex(this.hintTextBoxLocalArchive, 0);
            this.Controls.SetChildIndex(this.labelArchiveUrl, 0);
            this.Controls.SetChildIndex(this.hintTextBoxStartOffset, 0);
            this.Controls.SetChildIndex(this.labelStartOffset, 0);
            this.Controls.SetChildIndex(this.labelStartOffsetBytes, 0);
            this.Controls.SetChildIndex(this.treeViewExtract, 0);
            this.Controls.SetChildIndex(this.labelExtract, 0);
            this.Controls.SetChildIndex(this.labelArchiveDownloadMessages, 0);
            this.Controls.SetChildIndex(this.downloadProgressBarArchive, 0);
            this.Controls.SetChildIndex(this.buttonChooseArchive, 0);
            this.Controls.SetChildIndex(this.buttonExtractArchive, 0);
            this.Controls.SetChildIndex(this.labelExtractArchiveMessage, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelExtractArchiveMessage;
        private System.Windows.Forms.Button buttonExtractArchive;
        private System.Windows.Forms.Button buttonChooseArchive;
        private Common.Controls.DownloadProgressBar downloadProgressBarArchive;
        private System.Windows.Forms.Label labelArchiveDownloadMessages;
        private System.Windows.Forms.Label labelExtract;
        private System.Windows.Forms.TreeView treeViewExtract;
        private System.Windows.Forms.Label labelStartOffsetBytes;
        private System.Windows.Forms.Label labelStartOffset;
        private Common.Controls.HintTextBox hintTextBoxStartOffset;
        private System.Windows.Forms.Label labelArchiveUrl;
        private Common.Controls.HintTextBox hintTextBoxLocalArchive;
        private System.Windows.Forms.Label labelLocalArchive;
        private System.Windows.Forms.Button buttonArchiveDownload;
        private Common.Controls.HintTextBox hintTextBoxArchiveUrl;
        private System.Windows.Forms.ComboBox comboBoxArchiveFormat;
        private System.Windows.Forms.Label labelArchiveFormat;
        private System.Windows.Forms.OpenFileDialog openFileDialogLocalArchive;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialogDownloadPath;

    }
}