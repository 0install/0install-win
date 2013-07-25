namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class SingleFileEditor
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
            this.labelsize = new System.Windows.Forms.Label();
            this.textBoxSize = new Common.Controls.HintTextBox();
            this.labelUrl = new System.Windows.Forms.Label();
            this.textBoxUrl = new Common.Controls.UriTextBox();
            this.labelDestination = new System.Windows.Forms.Label();
            this.textBoxDestination = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // labelsize
            // 
            this.labelsize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelsize.AutoSize = true;
            this.labelsize.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelsize.Location = new System.Drawing.Point(0, 29);
            this.labelsize.Name = "labelsize";
            this.labelsize.Size = new System.Drawing.Size(47, 13);
            this.labelsize.TabIndex = 2;
            this.labelsize.Text = "File size:";
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
            // labelUrl
            // 
            this.labelUrl.AutoSize = true;
            this.labelUrl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelUrl.Location = new System.Drawing.Point(0, 3);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(71, 13);
            this.labelUrl.TabIndex = 0;
            this.labelUrl.Text = "Archive URL:";
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
            // 
            // labelDestination
            // 
            this.labelDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDestination.AutoSize = true;
            this.labelDestination.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelDestination.Location = new System.Drawing.Point(0, 55);
            this.labelDestination.Name = "labelDestination";
            this.labelDestination.Size = new System.Drawing.Size(63, 13);
            this.labelDestination.TabIndex = 4;
            this.labelDestination.Text = "Destination:";
            // 
            // textBoxDestination
            // 
            this.textBoxDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDestination.HintText = "the local file name to use (required)";
            this.textBoxDestination.Location = new System.Drawing.Point(77, 52);
            this.textBoxDestination.Name = "textBoxDestination";
            this.textBoxDestination.Size = new System.Drawing.Size(73, 20);
            this.textBoxDestination.TabIndex = 5;
            // 
            // SingleFileEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelDestination);
            this.Controls.Add(this.textBoxDestination);
            this.Controls.Add(this.labelsize);
            this.Controls.Add(this.textBoxSize);
            this.Controls.Add(this.labelUrl);
            this.Controls.Add(this.textBoxUrl);
            this.Name = "SingleFileEditor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelsize;
        private Common.Controls.HintTextBox textBoxSize;
        private System.Windows.Forms.Label labelUrl;
        private Common.Controls.UriTextBox textBoxUrl;
        private System.Windows.Forms.Label labelDestination;
        private Common.Controls.HintTextBox textBoxDestination;

    }
}
