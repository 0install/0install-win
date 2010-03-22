namespace ZeroInstall.FeedEditor
{
    partial class FeedReferenceControl
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
            this.lblExtFeedURL = new System.Windows.Forms.Label();
            this.textBoxExtFeedURL = new Common.Controls.HintTextBox();
            this.targetBaseControl = new ZeroInstall.FeedEditor.TargetBaseControl();
            this.labelURLError = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblExtFeedURL
            // 
            this.lblExtFeedURL.AutoSize = true;
            this.lblExtFeedURL.Location = new System.Drawing.Point(3, 0);
            this.lblExtFeedURL.Name = "lblExtFeedURL";
            this.lblExtFeedURL.Size = new System.Drawing.Size(56, 13);
            this.lblExtFeedURL.TabIndex = 3;
            this.lblExtFeedURL.Text = "Feed URL";
            // 
            // textBoxExtFeedURL
            // 
            this.textBoxExtFeedURL.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxExtFeedURL.ClearButton = true;
            this.textBoxExtFeedURL.HintText = "URL to an external feed";
            this.textBoxExtFeedURL.Location = new System.Drawing.Point(6, 16);
            this.textBoxExtFeedURL.Name = "textBoxExtFeedURL";
            this.textBoxExtFeedURL.Size = new System.Drawing.Size(488, 20);
            this.textBoxExtFeedURL.TabIndex = 2;
            this.textBoxExtFeedURL.TextChanged += new System.EventHandler(this.textBoxExtFeedURL_TextChanged);
            this.textBoxExtFeedURL.Enter += new System.EventHandler(this.textBoxExtFeedURL_Enter);
            // 
            // targetBaseControl
            // 
            this.targetBaseControl.Location = new System.Drawing.Point(0, 40);
            this.targetBaseControl.Name = "targetBaseControl";
            this.targetBaseControl.Size = new System.Drawing.Size(502, 128);
            this.targetBaseControl.TabIndex = 0;
            this.targetBaseControl.TargetBase = null;
            // 
            // labelURLError
            // 
            this.labelURLError.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelURLError.AutoSize = true;
            this.labelURLError.ForeColor = System.Drawing.Color.Red;
            this.labelURLError.Location = new System.Drawing.Point(432, 39);
            this.labelURLError.Name = "labelURLError";
            this.labelURLError.Size = new System.Drawing.Size(62, 13);
            this.labelURLError.TabIndex = 4;
            this.labelURLError.Text = "invalid URL";
            // 
            // FeedReferenceControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelURLError);
            this.Controls.Add(this.lblExtFeedURL);
            this.Controls.Add(this.textBoxExtFeedURL);
            this.Controls.Add(this.targetBaseControl);
            this.Name = "FeedReferenceControl";
            this.Size = new System.Drawing.Size(497, 171);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TargetBaseControl targetBaseControl;
        private System.Windows.Forms.Label lblExtFeedURL;
        private Common.Controls.HintTextBox textBoxExtFeedURL;
        private System.Windows.Forms.Label labelURLError;
    }
}
