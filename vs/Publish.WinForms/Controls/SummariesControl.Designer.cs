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
            this.hintTextBoxSummary = new Common.Controls.HintTextBox();
            this.comboBoxLanguages = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // hintTextBoxSummary
            // 
            this.hintTextBoxSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSummary.ClearButton = true;
            this.hintTextBoxSummary.HintText = "a short one-line description";
            this.hintTextBoxSummary.Location = new System.Drawing.Point(164, 2);
            this.hintTextBoxSummary.Name = "hintTextBoxSummary";
            this.hintTextBoxSummary.Size = new System.Drawing.Size(299, 20);
            this.hintTextBoxSummary.TabIndex = 1;
            this.hintTextBoxSummary.TextChanged += new System.EventHandler(this.hintTextBoxSummary_TextChanged);
            // 
            // comboBoxLanguages
            // 
            this.comboBoxLanguages.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguages.FormattingEnabled = true;
            this.comboBoxLanguages.Location = new System.Drawing.Point(0, 1);
            this.comboBoxLanguages.Name = "comboBoxLanguages";
            this.comboBoxLanguages.Size = new System.Drawing.Size(158, 21);
            this.comboBoxLanguages.TabIndex = 2;
            // 
            // SummariesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBoxLanguages);
            this.Controls.Add(this.hintTextBoxSummary);
            this.Name = "SummariesControl";
            this.Size = new System.Drawing.Size(463, 23);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public Common.Controls.HintTextBox hintTextBoxSummary;
        private System.Windows.Forms.ComboBox comboBoxLanguages;
    }
}
