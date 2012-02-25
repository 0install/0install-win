using System;
using System.IO;

namespace ZeroInstall.Publish.WinForms.Controls
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
            this.labelArchiveUrl = new System.Windows.Forms.Label();
            this.buttonDownload = new System.Windows.Forms.Button();
            this.labelLocalArchive = new System.Windows.Forms.Label();
            this.buttonLocalArchive = new System.Windows.Forms.Button();
            this.labelSubDirectory = new System.Windows.Forms.Label();
            this.treeViewSubDirectory = new System.Windows.Forms.TreeView();
            this.buttonExtractArchive = new System.Windows.Forms.Button();
            this.openFileDialogLocalArchive = new System.Windows.Forms.OpenFileDialog();
            this.hintTextBoxLocalArchive = new Common.Controls.HintTextBox();
            this.hintTextBoxStartOffset = new Common.Controls.HintTextBox();
            this.uriTextBoxArchiveUrl = new Common.Controls.UriTextBox();
            this.saveFileDialogDownloadFile = new System.Windows.Forms.SaveFileDialog();
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
            this.comboBoxArchiveFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxArchiveFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxArchiveFormat.FormattingEnabled = true;
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
            this.labelStartOffsetBytes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStartOffsetBytes.AutoSize = true;
            this.labelStartOffsetBytes.Location = new System.Drawing.Point(224, 59);
            this.labelStartOffsetBytes.Name = "labelStartOffsetBytes";
            this.labelStartOffsetBytes.Size = new System.Drawing.Size(33, 13);
            this.labelStartOffsetBytes.TabIndex = 4;
            this.labelStartOffsetBytes.Text = "Bytes";
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
            // buttonDownload
            // 
            this.buttonDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDownload.Location = new System.Drawing.Point(189, 121);
            this.buttonDownload.Name = "buttonDownload";
            this.buttonDownload.Size = new System.Drawing.Size(68, 23);
            this.buttonDownload.TabIndex = 7;
            this.buttonDownload.Text = "Download";
            this.buttonDownload.UseVisualStyleBackColor = true;
            this.buttonDownload.Click += new System.EventHandler(this.ButtonDownloadClick);
            // 
            // labelLocalArchive
            // 
            this.labelLocalArchive.AutoSize = true;
            this.labelLocalArchive.Location = new System.Drawing.Point(0, 147);
            this.labelLocalArchive.Name = "labelLocalArchive";
            this.labelLocalArchive.Size = new System.Drawing.Size(71, 13);
            this.labelLocalArchive.TabIndex = 8;
            this.labelLocalArchive.Text = "Local archive";
            // 
            // buttonLocalArchive
            // 
            this.buttonLocalArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLocalArchive.Location = new System.Drawing.Point(162, 189);
            this.buttonLocalArchive.Name = "buttonLocalArchive";
            this.buttonLocalArchive.Size = new System.Drawing.Size(95, 23);
            this.buttonLocalArchive.TabIndex = 10;
            this.buttonLocalArchive.Text = "Choose archive";
            this.buttonLocalArchive.UseVisualStyleBackColor = true;
            this.buttonLocalArchive.Click += new System.EventHandler(this.ButtonLocalArchiveClick);
            // 
            // labelSubDirectory
            // 
            this.labelSubDirectory.AutoSize = true;
            this.labelSubDirectory.Location = new System.Drawing.Point(0, 246);
            this.labelSubDirectory.Name = "labelSubDirectory";
            this.labelSubDirectory.Size = new System.Drawing.Size(66, 13);
            this.labelSubDirectory.TabIndex = 12;
            this.labelSubDirectory.Text = "Subdirectory";
            // 
            // treeViewSubDirectory
            // 
            this.treeViewSubDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewSubDirectory.Location = new System.Drawing.Point(3, 262);
            this.treeViewSubDirectory.Name = "treeViewSubDirectory";
            treeNode1.Name = "rootNode";
            treeNode1.Text = "Top folder";
            this.treeViewSubDirectory.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeViewSubDirectory.PathSeparator = "/";
            this.treeViewSubDirectory.ShowLines = false;
            this.treeViewSubDirectory.ShowRootLines = false;
            this.treeViewSubDirectory.Size = new System.Drawing.Size(254, 133);
            this.treeViewSubDirectory.TabIndex = 13;
            this.treeViewSubDirectory.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewSubDirectoryAfterSelect);
            // 
            // buttonExtractArchive
            // 
            this.buttonExtractArchive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExtractArchive.Enabled = false;
            this.buttonExtractArchive.Location = new System.Drawing.Point(162, 233);
            this.buttonExtractArchive.Name = "buttonExtractArchive";
            this.buttonExtractArchive.Size = new System.Drawing.Size(95, 23);
            this.buttonExtractArchive.TabIndex = 11;
            this.buttonExtractArchive.Text = "Extract archive";
            this.buttonExtractArchive.UseVisualStyleBackColor = true;
            this.buttonExtractArchive.Click += new System.EventHandler(this.ButtonExtractArchiveClick);
            // 
            // openFileDialogLocalArchive
            // 
            this.openFileDialogLocalArchive.Title = "Choose local archive";
            // 
            // hintTextBoxLocalArchive
            // 
            this.hintTextBoxLocalArchive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxLocalArchive.HintText = "";
            this.hintTextBoxLocalArchive.Location = new System.Drawing.Point(3, 163);
            this.hintTextBoxLocalArchive.Name = "hintTextBoxLocalArchive";
            this.hintTextBoxLocalArchive.ReadOnly = true;
            this.hintTextBoxLocalArchive.Size = new System.Drawing.Size(254, 20);
            this.hintTextBoxLocalArchive.TabIndex = 9;
            // 
            // hintTextBoxStartOffset
            // 
            this.hintTextBoxStartOffset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxStartOffset.HintText = "";
            this.hintTextBoxStartOffset.Location = new System.Drawing.Point(3, 56);
            this.hintTextBoxStartOffset.Name = "hintTextBoxStartOffset";
            this.hintTextBoxStartOffset.Size = new System.Drawing.Size(215, 20);
            this.hintTextBoxStartOffset.TabIndex = 3;
            this.hintTextBoxStartOffset.TextChanged += new System.EventHandler(this.HintTextBoxStartOffsetTextChanged);
            // 
            // uriTextBoxArchiveUrl
            // 
            this.uriTextBoxArchiveUrl.AllowDrop = true;
            this.uriTextBoxArchiveUrl.ForeColor = System.Drawing.Color.Red;
            this.uriTextBoxArchiveUrl.HintText = "";
            this.uriTextBoxArchiveUrl.HttpOnly = true;
            this.uriTextBoxArchiveUrl.Location = new System.Drawing.Point(3, 96);
            this.uriTextBoxArchiveUrl.Name = "uriTextBoxArchiveUrl";
            this.uriTextBoxArchiveUrl.Size = new System.Drawing.Size(251, 20);
            this.uriTextBoxArchiveUrl.TabIndex = 6;
            this.uriTextBoxArchiveUrl.Validated += new System.EventHandler(this.UriTextBoxArchiveUrlValidated);
            // 
            // ArchiveControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uriTextBoxArchiveUrl);
            this.Controls.Add(this.buttonExtractArchive);
            this.Controls.Add(this.treeViewSubDirectory);
            this.Controls.Add(this.labelSubDirectory);
            this.Controls.Add(this.buttonLocalArchive);
            this.Controls.Add(this.hintTextBoxLocalArchive);
            this.Controls.Add(this.labelLocalArchive);
            this.Controls.Add(this.buttonDownload);
            this.Controls.Add(this.labelArchiveUrl);
            this.Controls.Add(this.hintTextBoxStartOffset);
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
        private Common.Controls.HintTextBox hintTextBoxStartOffset;
        private System.Windows.Forms.Label labelArchiveUrl;
        private System.Windows.Forms.Button buttonDownload;
        private System.Windows.Forms.Label labelLocalArchive;
        private Common.Controls.HintTextBox hintTextBoxLocalArchive;
        private System.Windows.Forms.Button buttonLocalArchive;
        private System.Windows.Forms.Label labelSubDirectory;
        private System.Windows.Forms.TreeView treeViewSubDirectory;
        private System.Windows.Forms.Button buttonExtractArchive;
        private System.Windows.Forms.OpenFileDialog openFileDialogLocalArchive;
        private Common.Controls.UriTextBox uriTextBoxArchiveUrl;
        private System.Windows.Forms.SaveFileDialog saveFileDialogDownloadFile;
    }
}
