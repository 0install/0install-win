namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class DonePage
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
            this.buttonFinish = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelInfo2 = new System.Windows.Forms.Label();
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
            this.labelTitle.Text = "Done";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonFinish
            // 
            this.buttonFinish.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonFinish.Location = new System.Drawing.Point(315, 238);
            this.buttonFinish.Name = "buttonFinish";
            this.buttonFinish.Size = new System.Drawing.Size(120, 35);
            this.buttonFinish.TabIndex = 2;
            this.buttonFinish.Text = "&Finish";
            this.buttonFinish.UseVisualStyleBackColor = true;
            this.buttonFinish.Click += new System.EventHandler(this.buttonFinish_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelInfo.Location = new System.Drawing.Point(35, 82);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(400, 92);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "The wizard is done! We will now open the feed in the editor so you can look every" +
    "thing over, make any changes you like and save the feed as a file afterwards.";
            // 
            // labelInfo2
            // 
            this.labelInfo2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelInfo2.Location = new System.Drawing.Point(35, 174);
            this.labelInfo2.Name = "labelInfo2";
            this.labelInfo2.Size = new System.Drawing.Size(400, 61);
            this.labelInfo2.TabIndex = 1;
            this.labelInfo2.Text = "When uploading the feed make sure you also include these files: feed.xsl, feed.cs" +
    "s, *.gpg";
            // 
            // DonePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelInfo2);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonFinish);
            this.Controls.Add(this.labelTitle);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "DonePage";
            this.Size = new System.Drawing.Size(470, 300);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonFinish;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelInfo2;
    }
}
