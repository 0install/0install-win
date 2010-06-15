namespace ZeroInstall.Publish.WinForms
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelArchiveFormat = new System.Windows.Forms.Label();
            this.comboBoxArchiveFormat = new System.Windows.Forms.ComboBox();
            this.buttonArchiveDownload = new System.Windows.Forms.Button();
            this.openFileDialogLocalArchive = new System.Windows.Forms.OpenFileDialog();
            this.labelLocalArchive = new System.Windows.Forms.Label();
            this.labelArchiveUrl = new System.Windows.Forms.Label();
            this.labelStartOffset = new System.Windows.Forms.Label();
            this.labelStartOffsetBytes = new System.Windows.Forms.Label();
            this.treeViewExtract = new System.Windows.Forms.TreeView();
            this.labelExtract = new System.Windows.Forms.Label();
            this.hintTextBoxStartOffset = new Common.Controls.HintTextBox();
            this.hintTextBoxLocalArchive = new Common.Controls.HintTextBox();
            this.hintTextBoxArchiveUrl = new Common.Controls.HintTextBox();
            this.downloadProgressBarArchive = new ZeroInstall.Publish.WinForms.DownloadProgressBar();
            this.labelArchiveDownloadError = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(197, 392);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(116, 392);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelArchiveFormat
            // 
            this.labelArchiveFormat.AutoSize = true;
            this.labelArchiveFormat.Location = new System.Drawing.Point(12, 9);
            this.labelArchiveFormat.Name = "labelArchiveFormat";
            this.labelArchiveFormat.Size = new System.Drawing.Size(75, 13);
            this.labelArchiveFormat.TabIndex = 2;
            this.labelArchiveFormat.Text = "Archive format";
            // 
            // comboBoxArchiveFormat
            // 
            this.comboBoxArchiveFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxArchiveFormat.FormattingEnabled = true;
            this.comboBoxArchiveFormat.Items.AddRange(new object[] {
            "(auto detect)",
            "application/zip"});
            this.comboBoxArchiveFormat.Location = new System.Drawing.Point(15, 25);
            this.comboBoxArchiveFormat.Name = "comboBoxArchiveFormat";
            this.comboBoxArchiveFormat.Size = new System.Drawing.Size(257, 21);
            this.comboBoxArchiveFormat.TabIndex = 3;
            // 
            // buttonArchiveDownload
            // 
            this.buttonArchiveDownload.Location = new System.Drawing.Point(197, 95);
            this.buttonArchiveDownload.Name = "buttonArchiveDownload";
            this.buttonArchiveDownload.Size = new System.Drawing.Size(75, 23);
            this.buttonArchiveDownload.TabIndex = 5;
            this.buttonArchiveDownload.Text = "Download";
            this.buttonArchiveDownload.UseVisualStyleBackColor = true;
            this.buttonArchiveDownload.Click += new System.EventHandler(this.buttonArchiveDownload_Click);
            // 
            // openFileDialogLocalArchive
            // 
            this.openFileDialogLocalArchive.FileName = "openFileDialog1";
            // 
            // labelLocalArchive
            // 
            this.labelLocalArchive.AutoSize = true;
            this.labelLocalArchive.Location = new System.Drawing.Point(12, 162);
            this.labelLocalArchive.Name = "labelLocalArchive";
            this.labelLocalArchive.Size = new System.Drawing.Size(71, 13);
            this.labelLocalArchive.TabIndex = 6;
            this.labelLocalArchive.Text = "Local archive";
            // 
            // labelArchiveUrl
            // 
            this.labelArchiveUrl.AutoSize = true;
            this.labelArchiveUrl.Location = new System.Drawing.Point(12, 53);
            this.labelArchiveUrl.Name = "labelArchiveUrl";
            this.labelArchiveUrl.Size = new System.Drawing.Size(57, 13);
            this.labelArchiveUrl.TabIndex = 8;
            this.labelArchiveUrl.Text = "Archive url";
            // 
            // labelStartOffset
            // 
            this.labelStartOffset.AutoSize = true;
            this.labelStartOffset.Location = new System.Drawing.Point(12, 201);
            this.labelStartOffset.Name = "labelStartOffset";
            this.labelStartOffset.Size = new System.Drawing.Size(58, 13);
            this.labelStartOffset.TabIndex = 10;
            this.labelStartOffset.Text = "Start-offset";
            // 
            // labelStartOffsetBytes
            // 
            this.labelStartOffsetBytes.AutoSize = true;
            this.labelStartOffsetBytes.Location = new System.Drawing.Point(239, 220);
            this.labelStartOffsetBytes.Name = "labelStartOffsetBytes";
            this.labelStartOffsetBytes.Size = new System.Drawing.Size(33, 13);
            this.labelStartOffsetBytes.TabIndex = 11;
            this.labelStartOffsetBytes.Text = "Bytes";
            // 
            // treeViewExtract
            // 
            this.treeViewExtract.Location = new System.Drawing.Point(15, 256);
            this.treeViewExtract.Name = "treeViewExtract";
            this.treeViewExtract.Size = new System.Drawing.Size(257, 98);
            this.treeViewExtract.TabIndex = 12;
            // 
            // labelExtract
            // 
            this.labelExtract.AutoSize = true;
            this.labelExtract.Location = new System.Drawing.Point(12, 240);
            this.labelExtract.Name = "labelExtract";
            this.labelExtract.Size = new System.Drawing.Size(66, 13);
            this.labelExtract.TabIndex = 13;
            this.labelExtract.Text = "Subdirectory";
            // 
            // hintTextBoxStartOffset
            // 
            this.hintTextBoxStartOffset.HintText = "";
            this.hintTextBoxStartOffset.Location = new System.Drawing.Point(15, 217);
            this.hintTextBoxStartOffset.Name = "hintTextBoxStartOffset";
            this.hintTextBoxStartOffset.Size = new System.Drawing.Size(218, 20);
            this.hintTextBoxStartOffset.TabIndex = 9;
            // 
            // hintTextBoxLocalArchive
            // 
            this.hintTextBoxLocalArchive.HintText = "";
            this.hintTextBoxLocalArchive.Location = new System.Drawing.Point(15, 178);
            this.hintTextBoxLocalArchive.Name = "hintTextBoxLocalArchive";
            this.hintTextBoxLocalArchive.Size = new System.Drawing.Size(257, 20);
            this.hintTextBoxLocalArchive.TabIndex = 7;
            // 
            // hintTextBoxArchiveUrl
            // 
            this.hintTextBoxArchiveUrl.HintText = "";
            this.hintTextBoxArchiveUrl.Location = new System.Drawing.Point(15, 69);
            this.hintTextBoxArchiveUrl.Name = "hintTextBoxArchiveUrl";
            this.hintTextBoxArchiveUrl.Size = new System.Drawing.Size(257, 20);
            this.hintTextBoxArchiveUrl.TabIndex = 4;
            this.hintTextBoxArchiveUrl.TextChanged += new System.EventHandler(this.hintTextBoxArchiveUrl_TextChanged);
            // 
            // downloadProgressBarArchive
            // 
            this.downloadProgressBarArchive.Download = null;
            this.downloadProgressBarArchive.Location = new System.Drawing.Point(15, 92);
            this.downloadProgressBarArchive.Name = "downloadProgressBarArchive";
            this.downloadProgressBarArchive.Size = new System.Drawing.Size(176, 10);
            this.downloadProgressBarArchive.TabIndex = 14;
            this.downloadProgressBarArchive.UseTaskbar = false;
            // 
            // labelArchiveDownloadError
            // 
            this.labelArchiveDownloadError.AutoSize = true;
            this.labelArchiveDownloadError.Location = new System.Drawing.Point(12, 105);
            this.labelArchiveDownloadError.Name = "labelArchiveDownloadError";
            this.labelArchiveDownloadError.Size = new System.Drawing.Size(0, 13);
            this.labelArchiveDownloadError.TabIndex = 15;
            // 
            // ArchiveForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(284, 460);
            this.Controls.Add(this.labelArchiveDownloadError);
            this.Controls.Add(this.downloadProgressBarArchive);
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
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ArchiveForm";
            this.Text = "ArchiveForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelArchiveFormat;
        private System.Windows.Forms.ComboBox comboBoxArchiveFormat;
        private Common.Controls.HintTextBox hintTextBoxArchiveUrl;
        private System.Windows.Forms.Button buttonArchiveDownload;
        private System.Windows.Forms.OpenFileDialog openFileDialogLocalArchive;
        private System.Windows.Forms.Label labelLocalArchive;
        private Common.Controls.HintTextBox hintTextBoxLocalArchive;
        private System.Windows.Forms.Label labelArchiveUrl;
        private Common.Controls.HintTextBox hintTextBoxStartOffset;
        private System.Windows.Forms.Label labelStartOffset;
        private System.Windows.Forms.Label labelStartOffsetBytes;
        private System.Windows.Forms.TreeView treeViewExtract;
        private System.Windows.Forms.Label labelExtract;
        private DownloadProgressBar downloadProgressBarArchive;
        private System.Windows.Forms.Label labelArchiveDownloadError;
    }
}