namespace NanoByte.Common.Controls
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
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.textBox = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.DropDownWidth = 86;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(0, 0);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(60, 21);
            this.comboBoxLanguage.TabIndex = 0;
            this.comboBoxLanguage.SelectionChangeCommitted += new System.EventHandler(this.comboBoxLanguage_SelectionChangeCommitted);
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.HintText = null;
            this.textBox.Location = new System.Drawing.Point(66, 0);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ShowClearButton = true;
            this.textBox.Size = new System.Drawing.Size(84, 150);
            this.textBox.TabIndex = 1;
            this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            this.textBox.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_Validating);
            // 
            // LocalizableTextBox
            // 
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.textBox);
            this.MinimumSize = new System.Drawing.Size(65, 22);
            this.Name = "LocalizableTextBox";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxLanguage;
        public Common.Controls.HintTextBox textBox;
    }
}
