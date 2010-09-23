namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class SummariesControl
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
            this.languageComboBox = new ZeroInstall.Publish.WinForms.Controls.LanguageComboBox();
            this.hintTextBoxSummary = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // languageComboBox
            // 
            this.languageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.languageComboBox.Location = new System.Drawing.Point(193, 0);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(125, 21);
            this.languageComboBox.TabIndex = 0;
            // 
            // hintTextBoxSummary
            // 
            this.hintTextBoxSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSummary.HintText = "a short one-line description";
            this.hintTextBoxSummary.Location = new System.Drawing.Point(0, 1);
            this.hintTextBoxSummary.Name = "hintTextBoxSummary";
            this.hintTextBoxSummary.Size = new System.Drawing.Size(187, 20);
            this.hintTextBoxSummary.TabIndex = 1;
            // 
            // SummariesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hintTextBoxSummary);
            this.Controls.Add(this.languageComboBox);
            this.Name = "SummariesControl";
            this.Size = new System.Drawing.Size(318, 22);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public Common.Controls.HintTextBox hintTextBoxSummary;
        public LanguageComboBox languageComboBox;
    }
}
