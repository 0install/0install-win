namespace ZeroInstall.FeedEditor
{
    partial class EnvironmentBindingControl
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
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(3, 0);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(35, 13);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name";
            // 
            // hintTextBoxInsert
            // 
            this.hintTextBoxInsert.HintText = "";
            this.hintTextBoxInsert.Location = new System.Drawing.Point(6, 55);
            this.hintTextBoxInsert.Name = "hintTextBoxInsert";
            this.hintTextBoxInsert.Size = new System.Drawing.Size(305, 20);
            this.hintTextBoxInsert.TabIndex = 1;
            this.hintTextBoxInsert.TextChanged += new System.EventHandler(this.hintTextBoxInsert_TextChanged);
            // 
            // labelInsert
            // 
            this.labelInsert.AutoSize = true;
            this.labelInsert.Location = new System.Drawing.Point(3, 39);
            this.labelInsert.Name = "labelInsert";
            this.labelInsert.Size = new System.Drawing.Size(33, 13);
            this.labelInsert.TabIndex = 2;
            this.labelInsert.Text = "Insert";
            // 
            // comboBoxName
            // 
            this.comboBoxName.FormattingEnabled = true;
            this.comboBoxName.Items.AddRange(new object[] {
            "LD_LIBRARY_PATH",
            "PATH",
            "PKG_CONFIG_PATH",
            "PYTHONPATH",
            "XDG_CONFIG_DIRS",
            "XDG_DATA_DIRS"});
            this.comboBoxName.Location = new System.Drawing.Point(6, 16);
            this.comboBoxName.Name = "comboBoxName";
            this.comboBoxName.Size = new System.Drawing.Size(305, 21);
            this.comboBoxName.Sorted = true;
            this.comboBoxName.TabIndex = 3;
            this.comboBoxName.TextChanged += new System.EventHandler(this.comboBoxName_TextChanged);
            // 
            // labelMode
            // 
            this.labelMode.AutoSize = true;
            this.labelMode.Location = new System.Drawing.Point(3, 79);
            this.labelMode.Name = "labelMode";
            this.labelMode.Size = new System.Drawing.Size(34, 13);
            this.labelMode.TabIndex = 4;
            this.labelMode.Text = "Mode";
            // 
            // comboBoxMode
            // 
            this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMode.FormattingEnabled = true;
            this.comboBoxMode.Location = new System.Drawing.Point(6, 95);
            this.comboBoxMode.Name = "comboBoxMode";
            this.comboBoxMode.Size = new System.Drawing.Size(305, 21);
            this.comboBoxMode.TabIndex = 5;
            this.comboBoxMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxMode_SelectedIndexChanged);
            // 
            // labelDefault
            // 
            this.labelDefault.AutoSize = true;
            this.labelDefault.Location = new System.Drawing.Point(3, 119);
            this.labelDefault.Name = "labelDefault";
            this.labelDefault.Size = new System.Drawing.Size(41, 13);
            this.labelDefault.TabIndex = 6;
            this.labelDefault.Text = "Default";
            // 
            // hintTextBoxDefault
            // 
            this.hintTextBoxDefault.HintText = "";
            this.hintTextBoxDefault.Location = new System.Drawing.Point(6, 135);
            this.hintTextBoxDefault.Name = "hintTextBoxDefault";
            this.hintTextBoxDefault.Size = new System.Drawing.Size(305, 20);
            this.hintTextBoxDefault.TabIndex = 7;
            this.hintTextBoxDefault.TextChanged += new System.EventHandler(this.hintTextBoxDefault_TextChanged);
            // 
            // EnvironmentBindingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hintTextBoxDefault);
            this.Controls.Add(this.labelDefault);
            this.Controls.Add(this.comboBoxMode);
            this.Controls.Add(this.labelMode);
            this.Controls.Add(this.comboBoxName);
            this.Controls.Add(this.labelInsert);
            this.Controls.Add(this.hintTextBoxInsert);
            this.Controls.Add(this.labelName);
            this.Name = "EnvironmentBindingControl";
            this.Size = new System.Drawing.Size(314, 160);
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
    }
}
