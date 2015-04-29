namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class InstallerCaptureStartPage
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonCapture = new System.Windows.Forms.Button();
            this.labelWarning = new System.Windows.Forms.Label();
            this.buttonSkip = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 18);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(470, 37);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Installer Capture";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelInfo
            // 
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelInfo.Location = new System.Drawing.Point(35, 66);
            this.labelInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(400, 104);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "We will now capture a snapshot of the system\'s current state. After running the i" +
    "nstaller we will create another snapshot and compare to the two in order to dete" +
    "rmine what changes the installer made.";
            // 
            // buttonCapture
            // 
            this.buttonCapture.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonCapture.Location = new System.Drawing.Point(218, 244);
            this.buttonCapture.Name = "buttonCapture";
            this.buttonCapture.Size = new System.Drawing.Size(217, 35);
            this.buttonCapture.TabIndex = 3;
            this.buttonCapture.Text = "&Capture and Run >";
            this.buttonCapture.UseVisualStyleBackColor = true;
            this.buttonCapture.Click += new System.EventHandler(this.buttonCapture_Click);
            // 
            // labelWarning
            // 
            this.labelWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelWarning.Location = new System.Drawing.Point(35, 170);
            this.labelWarning.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(400, 73);
            this.labelWarning.TabIndex = 2;
            this.labelWarning.Text = "Important: For best results you should do this in a pristine VM with nothing inst" +
    "alled except for the operating system and Zero Install.";
            // 
            // buttonSkip
            // 
            this.buttonSkip.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonSkip.Location = new System.Drawing.Point(39, 246);
            this.buttonSkip.Name = "buttonSkip";
            this.buttonSkip.Size = new System.Drawing.Size(120, 35);
            this.buttonSkip.TabIndex = 4;
            this.buttonSkip.Text = "&Skip >";
            this.buttonSkip.UseVisualStyleBackColor = true;
            this.buttonSkip.Click += new System.EventHandler(this.buttonSkip_Click);
            // 
            // InstallerCaptureStartPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonSkip);
            this.Controls.Add(this.buttonCapture);
            this.Controls.Add(this.labelWarning);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelTitle);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "InstallerCaptureStartPage";
            this.Size = new System.Drawing.Size(470, 300);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonCapture;
        private System.Windows.Forms.Label labelWarning;
        private System.Windows.Forms.Button buttonSkip;
    }
}
