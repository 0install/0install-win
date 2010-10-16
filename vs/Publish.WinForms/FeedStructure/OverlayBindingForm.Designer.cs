namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class OverlayBindingForm
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
            this.hintTextBoxSrc = new Common.Controls.HintTextBox();
            this.labelSrc = new System.Windows.Forms.Label();
            this.labelMountPoint = new System.Windows.Forms.Label();
            this.hintTextBoxMountPoint = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(156, 119);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(237, 119);
            // 
            // hintTextBoxSrc
            // 
            this.hintTextBoxSrc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSrc.HintText = "Relative path of the directory in the implementation";
            this.hintTextBoxSrc.Location = new System.Drawing.Point(15, 25);
            this.hintTextBoxSrc.Name = "hintTextBoxSrc";
            this.hintTextBoxSrc.Size = new System.Drawing.Size(297, 20);
            this.hintTextBoxSrc.TabIndex = 1;
            this.hintTextBoxSrc.TextChanged += new System.EventHandler(this.HintTextBoxSrcTextChanged);
            // 
            // labelSrc
            // 
            this.labelSrc.AutoSize = true;
            this.labelSrc.Location = new System.Drawing.Point(12, 9);
            this.labelSrc.Name = "labelSrc";
            this.labelSrc.Size = new System.Drawing.Size(41, 13);
            this.labelSrc.TabIndex = 0;
            this.labelSrc.Text = "Source";
            // 
            // labelMountPoint
            // 
            this.labelMountPoint.AutoSize = true;
            this.labelMountPoint.Location = new System.Drawing.Point(12, 48);
            this.labelMountPoint.Name = "labelMountPoint";
            this.labelMountPoint.Size = new System.Drawing.Size(63, 13);
            this.labelMountPoint.TabIndex = 2;
            this.labelMountPoint.Text = "Mount point";
            // 
            // hintTextBoxMountPoint
            // 
            this.hintTextBoxMountPoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxMountPoint.HintText = "Mount point on which source is to appear in the file system";
            this.hintTextBoxMountPoint.Location = new System.Drawing.Point(15, 64);
            this.hintTextBoxMountPoint.Name = "hintTextBoxMountPoint";
            this.hintTextBoxMountPoint.Size = new System.Drawing.Size(297, 20);
            this.hintTextBoxMountPoint.TabIndex = 3;
            this.hintTextBoxMountPoint.TextChanged += new System.EventHandler(this.HintTextBoxMountPointTextChanged);
            // 
            // OverlayBindingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 154);
            this.Controls.Add(this.hintTextBoxMountPoint);
            this.Controls.Add(this.labelMountPoint);
            this.Controls.Add(this.labelSrc);
            this.Controls.Add(this.hintTextBoxSrc);
            this.Name = "OverlayBindingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit overlay binding";
            this.Controls.SetChildIndex(this.hintTextBoxSrc, 0);
            this.Controls.SetChildIndex(this.labelSrc, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.labelMountPoint, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.hintTextBoxMountPoint, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.Controls.HintTextBox hintTextBoxSrc;
        private System.Windows.Forms.Label labelSrc;
        private System.Windows.Forms.Label labelMountPoint;
        private Common.Controls.HintTextBox hintTextBoxMountPoint;
    }
}
