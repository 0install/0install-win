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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hintTextBoxSrc
            // 
            this.hintTextBoxSrc.HintText = "Relative path of the directory in the implementation";
            this.hintTextBoxSrc.Location = new System.Drawing.Point(15, 25);
            this.hintTextBoxSrc.Name = "hintTextBoxSrc";
            this.hintTextBoxSrc.Size = new System.Drawing.Size(294, 20);
            this.hintTextBoxSrc.TabIndex = 1;
            this.hintTextBoxSrc.TextChanged += new System.EventHandler(this.hintTextBoxSrc_TextChanged);
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
            this.hintTextBoxMountPoint.HintText = "Mount point on which source is to appear in the filesystem";
            this.hintTextBoxMountPoint.Location = new System.Drawing.Point(15, 64);
            this.hintTextBoxMountPoint.Name = "hintTextBoxMountPoint";
            this.hintTextBoxMountPoint.Size = new System.Drawing.Size(294, 20);
            this.hintTextBoxMountPoint.TabIndex = 3;
            this.hintTextBoxMountPoint.TextChanged += new System.EventHandler(this.hintTextBoxMountPoint_TextChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(234, 114);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 5;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(153, 114);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // OverlayBindingForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(324, 146);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.hintTextBoxMountPoint);
            this.Controls.Add(this.labelMountPoint);
            this.Controls.Add(this.labelSrc);
            this.Controls.Add(this.hintTextBoxSrc);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "OverlayBindingForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.Controls.HintTextBox hintTextBoxSrc;
        private System.Windows.Forms.Label labelSrc;
        private System.Windows.Forms.Label labelMountPoint;
        private Common.Controls.HintTextBox hintTextBoxMountPoint;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
