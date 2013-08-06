namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class ArchiveEditor
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
            this.labelUrl = new System.Windows.Forms.Label();
            this.textBoxUrl = new Common.Controls.UriTextBox();
            this.labelMimeType = new System.Windows.Forms.Label();
            this.comboBoxMimeType = new System.Windows.Forms.ComboBox();
            this.labelSize = new System.Windows.Forms.Label();
            this.textBoxSize = new Common.Controls.HintTextBox();
            this.labelExtract = new System.Windows.Forms.Label();
            this.textBoxExtract = new Common.Controls.HintTextBox();
            this.labelDestination = new System.Windows.Forms.Label();
            this.textBoxDestination = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // labelUrl
            // 
            this.labelUrl.AutoSize = true;
            this.labelUrl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelUrl.Location = new System.Drawing.Point(0, 3);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(69, 13);
            this.labelUrl.TabIndex = 0;
            this.labelUrl.Text = "Source URL:";
            // 
            // textBoxUrl
            // 
            this.textBoxUrl.AllowDrop = true;
            this.textBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUrl.HintText = "HTTP/FTP URL (required)";
            this.textBoxUrl.Location = new System.Drawing.Point(77, 0);
            this.textBoxUrl.Name = "textBoxUrl";
            this.textBoxUrl.Size = new System.Drawing.Size(73, 20);
            this.textBoxUrl.TabIndex = 1;
            this.textBoxUrl.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // labelMimeType
            // 
            this.labelMimeType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMimeType.AutoSize = true;
            this.labelMimeType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelMimeType.Location = new System.Drawing.Point(0, 55);
            this.labelMimeType.Name = "labelMimeType";
            this.labelMimeType.Size = new System.Drawing.Size(65, 13);
            this.labelMimeType.TabIndex = 4;
            this.labelMimeType.Text = "MIME Type:";
            // 
            // comboBoxMimeType
            // 
            this.comboBoxMimeType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMimeType.FormattingEnabled = true;
            this.comboBoxMimeType.Location = new System.Drawing.Point(77, 52);
            this.comboBoxMimeType.Name = "comboBoxMimeType";
            this.comboBoxMimeType.Size = new System.Drawing.Size(73, 21);
            this.comboBoxMimeType.TabIndex = 5;
            // 
            // labelSize
            // 
            this.labelSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSize.AutoSize = true;
            this.labelSize.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelSize.Location = new System.Drawing.Point(0, 29);
            this.labelSize.Name = "labelSize";
            this.labelSize.Size = new System.Drawing.Size(47, 13);
            this.labelSize.TabIndex = 2;
            this.labelSize.Text = "File size:";
            // 
            // textBoxSize
            // 
            this.textBoxSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSize.HintText = "in bytes (required)";
            this.textBoxSize.Location = new System.Drawing.Point(77, 26);
            this.textBoxSize.Name = "textBoxSize";
            this.textBoxSize.Size = new System.Drawing.Size(73, 20);
            this.textBoxSize.TabIndex = 3;
            // 
            // labelExtract
            // 
            this.labelExtract.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelExtract.AutoSize = true;
            this.labelExtract.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelExtract.Location = new System.Drawing.Point(0, 82);
            this.labelExtract.Name = "labelExtract";
            this.labelExtract.Size = new System.Drawing.Size(43, 13);
            this.labelExtract.TabIndex = 6;
            this.labelExtract.Text = "Extract:";
            // 
            // textBoxExtract
            // 
            this.textBoxExtract.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExtract.HintText = "the directory to extract from";
            this.textBoxExtract.Location = new System.Drawing.Point(77, 79);
            this.textBoxExtract.Name = "textBoxExtract";
            this.textBoxExtract.Size = new System.Drawing.Size(73, 20);
            this.textBoxExtract.TabIndex = 7;
            this.textBoxExtract.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // labelDestination
            // 
            this.labelDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDestination.AutoSize = true;
            this.labelDestination.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelDestination.Location = new System.Drawing.Point(0, 108);
            this.labelDestination.Name = "labelDestination";
            this.labelDestination.Size = new System.Drawing.Size(63, 13);
            this.labelDestination.TabIndex = 8;
            this.labelDestination.Text = "Destination:";
            // 
            // textBoxDestination
            // 
            this.textBoxDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDestination.HintText = "the directory to extract to";
            this.textBoxDestination.Location = new System.Drawing.Point(77, 105);
            this.textBoxDestination.Name = "textBoxDestination";
            this.textBoxDestination.Size = new System.Drawing.Size(73, 20);
            this.textBoxDestination.TabIndex = 9;
            this.textBoxDestination.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // ArchiveEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelDestination);
            this.Controls.Add(this.textBoxDestination);
            this.Controls.Add(this.labelExtract);
            this.Controls.Add(this.textBoxExtract);
            this.Controls.Add(this.labelSize);
            this.Controls.Add(this.textBoxSize);
            this.Controls.Add(this.labelMimeType);
            this.Controls.Add(this.comboBoxMimeType);
            this.Controls.Add(this.labelUrl);
            this.Controls.Add(this.textBoxUrl);
            this.Name = "ArchiveEditor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUrl;
        private Common.Controls.UriTextBox textBoxUrl;
        private System.Windows.Forms.Label labelMimeType;
        private System.Windows.Forms.ComboBox comboBoxMimeType;
        private System.Windows.Forms.Label labelSize;
        private Common.Controls.HintTextBox textBoxSize;
        private System.Windows.Forms.Label labelExtract;
        private Common.Controls.HintTextBox textBoxExtract;
        private System.Windows.Forms.Label labelDestination;
        private Common.Controls.HintTextBox textBoxDestination;

    }
}
