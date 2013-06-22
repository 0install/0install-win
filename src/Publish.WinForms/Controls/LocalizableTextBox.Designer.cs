namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class LocalizableTextBox
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being Used.
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
            this.textBox = new Common.Controls.HintTextBox();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.AccessibleName = "";
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.HintText = "";
            this.textBox.Location = new System.Drawing.Point(85, 2);
            this.textBox.Name = "textBox";
            this.textBox.ShowClearButton = true;
            this.textBox.Size = new System.Drawing.Size(342, 20);
            this.textBox.TabIndex = 10;
            this.textBox.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_Validating);
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(0, 1);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(79, 21);
            this.comboBoxLanguage.TabIndex = 0;
            // 
            // LocalizableTextBox
            // 
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.comboBoxLanguage);
            this.Name = "LocalizableTextBox";
            this.Size = new System.Drawing.Size(427, 23);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxLanguage;
        public Common.Controls.HintTextBox textBox;
    }
}
