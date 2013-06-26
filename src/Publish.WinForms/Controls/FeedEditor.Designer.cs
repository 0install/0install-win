namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class FeedEditor
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
            this.textBoxDescription = new Common.Controls.LocalizableTextBox();
            this.textBoxSummary = new Common.Controls.LocalizableTextBox();
            this.SuspendLayout();
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDescription.HintText = "Description";
            this.textBoxDescription.Location = new System.Drawing.Point(0, 29);
            this.textBoxDescription.MinimumSize = new System.Drawing.Size(75, 22);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(150, 76);
            this.textBoxDescription.TabIndex = 1;
            // 
            // textBoxSummary
            // 
            this.textBoxSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSummary.HintText = "Summary";
            this.textBoxSummary.Location = new System.Drawing.Point(0, 0);
            this.textBoxSummary.MinimumSize = new System.Drawing.Size(75, 22);
            this.textBoxSummary.Multiline = false;
            this.textBoxSummary.Name = "textBoxSummary";
            this.textBoxSummary.Size = new System.Drawing.Size(150, 23);
            this.textBoxSummary.TabIndex = 0;
            // 
            // FeedEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxDescription);
            this.Controls.Add(this.textBoxSummary);
            this.Name = "FeedEditor";
            this.ResumeLayout(false);

        }

        #endregion

        private Common.Controls.LocalizableTextBox textBoxSummary;
        private Common.Controls.LocalizableTextBox textBoxDescription;

    }
}
