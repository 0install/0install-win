namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class ManifestDigestForm
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
            this.hintTextBoxSha1Old = new Common.Controls.HintTextBox();
            this.labelSha1Old = new System.Windows.Forms.Label();
            this.labelSha1New = new System.Windows.Forms.Label();
            this.hintTextBoxSha1New = new Common.Controls.HintTextBox();
            this.labelSha256 = new System.Windows.Forms.Label();
            this.hintTextBoxSha256 = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonOK.Location = new System.Drawing.Point(343, 148);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // hintTextBoxSha1Old
            // 
            this.hintTextBoxSha1Old.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSha1Old.Enabled = false;
            this.hintTextBoxSha1Old.HintText = "";
            this.hintTextBoxSha1Old.Location = new System.Drawing.Point(15, 25);
            this.hintTextBoxSha1Old.Name = "hintTextBoxSha1Old";
            this.hintTextBoxSha1Old.Size = new System.Drawing.Size(403, 20);
            this.hintTextBoxSha1Old.TabIndex = 1;
            // 
            // labelSha1Old
            // 
            this.labelSha1Old.AutoSize = true;
            this.labelSha1Old.Location = new System.Drawing.Point(12, 9);
            this.labelSha1Old.Name = "labelSha1Old";
            this.labelSha1Old.Size = new System.Drawing.Size(52, 13);
            this.labelSha1Old.TabIndex = 2;
            this.labelSha1Old.Text = "SHA1 old";
            // 
            // labelSha1New
            // 
            this.labelSha1New.AutoSize = true;
            this.labelSha1New.Location = new System.Drawing.Point(12, 48);
            this.labelSha1New.Name = "labelSha1New";
            this.labelSha1New.Size = new System.Drawing.Size(58, 13);
            this.labelSha1New.TabIndex = 3;
            this.labelSha1New.Text = "SHA1 new";
            // 
            // hintTextBoxSha1New
            // 
            this.hintTextBoxSha1New.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSha1New.Enabled = false;
            this.hintTextBoxSha1New.HintText = "";
            this.hintTextBoxSha1New.Location = new System.Drawing.Point(15, 64);
            this.hintTextBoxSha1New.Name = "hintTextBoxSha1New";
            this.hintTextBoxSha1New.Size = new System.Drawing.Size(403, 20);
            this.hintTextBoxSha1New.TabIndex = 4;
            // 
            // labelSha256
            // 
            this.labelSha256.AutoSize = true;
            this.labelSha256.Location = new System.Drawing.Point(12, 87);
            this.labelSha256.Name = "labelSha256";
            this.labelSha256.Size = new System.Drawing.Size(50, 13);
            this.labelSha256.TabIndex = 5;
            this.labelSha256.Text = "SHA 256";
            // 
            // hintTextBoxSha256
            // 
            this.hintTextBoxSha256.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSha256.Enabled = false;
            this.hintTextBoxSha256.HintText = "";
            this.hintTextBoxSha256.Location = new System.Drawing.Point(15, 103);
            this.hintTextBoxSha256.Name = "hintTextBoxSha256";
            this.hintTextBoxSha256.Size = new System.Drawing.Size(403, 20);
            this.hintTextBoxSha256.TabIndex = 6;
            // 
            // ManifestDigestForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonOK;
            this.ClientSize = new System.Drawing.Size(430, 183);
            this.Controls.Add(this.hintTextBoxSha256);
            this.Controls.Add(this.labelSha256);
            this.Controls.Add(this.hintTextBoxSha1New);
            this.Controls.Add(this.labelSha1New);
            this.Controls.Add(this.labelSha1Old);
            this.Controls.Add(this.hintTextBoxSha1Old);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ManifestDigestForm";
            this.Text = "Manifest digest";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private Common.Controls.HintTextBox hintTextBoxSha1Old;
        private System.Windows.Forms.Label labelSha1Old;
        private System.Windows.Forms.Label labelSha1New;
        private Common.Controls.HintTextBox hintTextBoxSha1New;
        private System.Windows.Forms.Label labelSha256;
        private Common.Controls.HintTextBox hintTextBoxSha256;
    }
}