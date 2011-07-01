namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class EnvironmentBindingForm
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
            this.labelName = new System.Windows.Forms.Label();
            this.hintTextBoxInsert = new Common.Controls.HintTextBox();
            this.labelInsert = new System.Windows.Forms.Label();
            this.comboBoxName = new System.Windows.Forms.ComboBox();
            this.labelMode = new System.Windows.Forms.Label();
            this.comboBoxMode = new System.Windows.Forms.ComboBox();
            this.labelDefault = new System.Windows.Forms.Label();
            this.hintTextBoxDefault = new Common.Controls.HintTextBox();
            this.hintTextBoxSeparator = new Common.Controls.HintTextBox();
            this.labelSeparator = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(165, 220);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(246, 220);
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(12, 9);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(35, 13);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name";
            // 
            // hintTextBoxInsert
            // 
            this.hintTextBoxInsert.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxInsert.HintText = "";
            this.hintTextBoxInsert.Location = new System.Drawing.Point(15, 64);
            this.hintTextBoxInsert.Name = "hintTextBoxInsert";
            this.hintTextBoxInsert.Size = new System.Drawing.Size(306, 20);
            this.hintTextBoxInsert.TabIndex = 3;
            // 
            // labelInsert
            // 
            this.labelInsert.AutoSize = true;
            this.labelInsert.Location = new System.Drawing.Point(12, 48);
            this.labelInsert.Name = "labelInsert";
            this.labelInsert.Size = new System.Drawing.Size(33, 13);
            this.labelInsert.TabIndex = 2;
            this.labelInsert.Text = "Insert";
            // 
            // comboBoxName
            // 
            this.comboBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxName.FormattingEnabled = true;
            this.comboBoxName.Items.AddRange(new object[] {
            "LD_LIBRARY_PATH",
            "PATH",
            "PKG_CONFIG_PATH",
            "PYTHONPATH",
            "XDG_CONFIG_DIRS",
            "XDG_DATA_DIRS"});
            this.comboBoxName.Location = new System.Drawing.Point(15, 25);
            this.comboBoxName.Name = "comboBoxName";
            this.comboBoxName.Size = new System.Drawing.Size(306, 21);
            this.comboBoxName.Sorted = true;
            this.comboBoxName.TabIndex = 1;
            // 
            // labelMode
            // 
            this.labelMode.AutoSize = true;
            this.labelMode.Location = new System.Drawing.Point(12, 88);
            this.labelMode.Name = "labelMode";
            this.labelMode.Size = new System.Drawing.Size(34, 13);
            this.labelMode.TabIndex = 4;
            this.labelMode.Text = "Mode";
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(15, 104);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(306, 21);
            this.comboBoxMode.TabIndex = 5;
            // 
            // labelDefault
            // 
            this.labelDefault.AutoSize = true;
            this.labelDefault.Location = new System.Drawing.Point(12, 167);
            this.labelDefault.Name = "labelDefault";
            this.labelDefault.Size = new System.Drawing.Size(41, 13);
            this.labelDefault.TabIndex = 8;
            this.labelDefault.Text = "Default";
            // 
            // hintTextBoxDefault
            // 
            this.hintTextBoxDefault.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxDefault.HintText = "";
            this.hintTextBoxDefault.Location = new System.Drawing.Point(15, 183);
            this.hintTextBoxDefault.Name = "hintTextBoxDefault";
            this.hintTextBoxDefault.Size = new System.Drawing.Size(306, 20);
            this.hintTextBoxDefault.TabIndex = 9;
            // 
            // hintTextBoxSeparator
            // 
            this.hintTextBoxSeparator.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxSeparator.HintText = "";
            this.hintTextBoxSeparator.Location = new System.Drawing.Point(15, 144);
            this.hintTextBoxSeparator.Name = "hintTextBoxSeparator";
            this.hintTextBoxSeparator.Size = new System.Drawing.Size(306, 20);
            this.hintTextBoxSeparator.TabIndex = 7;
            // 
            // labelSeparator
            // 
            this.labelSeparator.AutoSize = true;
            this.labelSeparator.Location = new System.Drawing.Point(12, 128);
            this.labelSeparator.Name = "labelSeparator";
            this.labelSeparator.Size = new System.Drawing.Size(53, 13);
            this.labelSeparator.TabIndex = 6;
            this.labelSeparator.Text = "Separator";
            // 
            // EnvironmentBindingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(333, 255);
            this.Controls.Add(this.hintTextBoxSeparator);
            this.Controls.Add(this.labelSeparator);
            this.Controls.Add(this.hintTextBoxDefault);
            this.Controls.Add(this.labelDefault);
            this.Controls.Add(this.comboBoxMode);
            this.Controls.Add(this.labelMode);
            this.Controls.Add(this.comboBoxName);
            this.Controls.Add(this.labelInsert);
            this.Controls.Add(this.hintTextBoxInsert);
            this.Controls.Add(this.labelName);
            this.Name = "EnvironmentBindingForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit environment binding";
            this.Controls.SetChildIndex(this.labelName, 0);
            this.Controls.SetChildIndex(this.hintTextBoxInsert, 0);
            this.Controls.SetChildIndex(this.labelInsert, 0);
            this.Controls.SetChildIndex(this.comboBoxName, 0);
            this.Controls.SetChildIndex(this.labelMode, 0);
            this.Controls.SetChildIndex(this.comboBoxMode, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.labelDefault, 0);
            this.Controls.SetChildIndex(this.hintTextBoxDefault, 0);
            this.Controls.SetChildIndex(this.labelSeparator, 0);
            this.Controls.SetChildIndex(this.hintTextBoxSeparator, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private Common.Controls.HintTextBox hintTextBoxInsert;
        private System.Windows.Forms.Label labelInsert;
        private System.Windows.Forms.ComboBox comboBoxName;
        private System.Windows.Forms.Label labelMode;
        private System.Windows.Forms.ComboBox comboBoxMode;
        private System.Windows.Forms.Label labelDefault;
        private Common.Controls.HintTextBox hintTextBoxDefault;
        private Common.Controls.HintTextBox hintTextBoxSeparator;
        private System.Windows.Forms.Label labelSeparator;
    }
}
