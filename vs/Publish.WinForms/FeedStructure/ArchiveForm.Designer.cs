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
            ZeroInstall.Model.Archive archive2 = new ZeroInstall.Model.Archive();
            this.archiveControl = new ZeroInstall.Publish.WinForms.Controls.ArchiveControl();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Enabled = false;
            this.buttonOK.Location = new System.Drawing.Point(114, 446);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(195, 446);
            // 
            // archiveControl
            // 
            this.archiveControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            archive2.Extract = null;
            archive2.Location = null;
            archive2.LocationString = null;
            archive2.MimeType = null;
            archive2.Size = ((long)(0));
            this.archiveControl.Archive = archive2;
            this.archiveControl.Location = new System.Drawing.Point(13, 13);
            this.archiveControl.Name = "archiveControl";
            this.archiveControl.Size = new System.Drawing.Size(257, 395);
            this.archiveControl.TabIndex = 1002;
            // 
            // ArchiveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 481);
            this.Controls.Add(this.archiveControl);
            this.Name = "ArchiveForm";
            this.Text = "Edit archive";
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.archiveControl, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ArchiveControl archiveControl;

    }
}