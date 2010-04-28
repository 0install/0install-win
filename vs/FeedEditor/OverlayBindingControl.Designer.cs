namespace ZeroInstall.FeedEditor
{
    partial class OverlayBindingControl
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
            // hintTextBoxSrc
            // 
            this.hintTextBoxSrc.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSrc.HintText = "Relative path of the directory in the implementation";
            this.hintTextBoxSrc.Location = new System.Drawing.Point(6, 16);
            this.hintTextBoxSrc.Name = "hintTextBoxSrc";
            this.hintTextBoxSrc.Size = new System.Drawing.Size(294, 20);
            this.hintTextBoxSrc.TabIndex = 0;
            this.hintTextBoxSrc.TextChanged += new System.EventHandler(this.hintTextBoxSrc_TextChanged);
            // 
            // labelSrc
            // 
            this.labelSrc.AutoSize = true;
            this.labelSrc.Location = new System.Drawing.Point(3, 0);
            this.labelSrc.Name = "labelSrc";
            this.labelSrc.Size = new System.Drawing.Size(41, 13);
            this.labelSrc.TabIndex = 1;
            this.labelSrc.Text = "Source";
            // 
            // labelMountPoint
            // 
            this.labelMountPoint.AutoSize = true;
            this.labelMountPoint.Location = new System.Drawing.Point(3, 39);
            this.labelMountPoint.Name = "labelMountPoint";
            this.labelMountPoint.Size = new System.Drawing.Size(63, 13);
            this.labelMountPoint.TabIndex = 2;
            this.labelMountPoint.Text = "Mount point";
            // 
            // hintTextBoxMountPoint
            // 
            this.hintTextBoxMountPoint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxMountPoint.HintText = "Mount point on which source is to appear in the filesystem";
            this.hintTextBoxMountPoint.Location = new System.Drawing.Point(6, 55);
            this.hintTextBoxMountPoint.Name = "hintTextBoxMountPoint";
            this.hintTextBoxMountPoint.Size = new System.Drawing.Size(294, 20);
            this.hintTextBoxMountPoint.TabIndex = 3;
            this.hintTextBoxMountPoint.TextChanged += new System.EventHandler(this.hintTextBoxMountPoint_TextChanged);
            // 
            // OverlayBindingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hintTextBoxMountPoint);
            this.Controls.Add(this.labelMountPoint);
            this.Controls.Add(this.labelSrc);
            this.Controls.Add(this.hintTextBoxSrc);
            this.Name = "OverlayBindingControl";
            this.Size = new System.Drawing.Size(305, 79);
            this.Enter += new System.EventHandler(this.OverlayBindingControl_Enter);
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
