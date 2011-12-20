namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class TargetBaseControl
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
            this.btnLanguageClear = new System.Windows.Forms.Button();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.listBoxLanguages = new System.Windows.Forms.ListBox();
            this.btnLanguageRemove = new System.Windows.Forms.Button();
            this.lblLanguage = new System.Windows.Forms.Label();
            this.btnLanguageAdd = new System.Windows.Forms.Button();
            this.lblCPU = new System.Windows.Forms.Label();
            this.comboBoxCpu = new System.Windows.Forms.ComboBox();
            this.lblOS = new System.Windows.Forms.Label();
            this.comboBoxOS = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnLanguageClear
            // 
            this.btnLanguageClear.Location = new System.Drawing.Point(0, 101);
            this.btnLanguageClear.Name = "btnLanguageClear";
            this.btnLanguageClear.Size = new System.Drawing.Size(75, 23);
            this.btnLanguageClear.TabIndex = 4;
            this.btnLanguageClear.Text = "Clear list";
            this.btnLanguageClear.UseVisualStyleBackColor = true;
            this.btnLanguageClear.Click += new System.EventHandler(this.BtnLanguageClearClick);
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(0, 16);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(411, 21);
            this.comboBoxLanguage.Sorted = true;
            this.comboBoxLanguage.TabIndex = 1;
            // 
            // listBoxLanguages
            // 
            this.listBoxLanguages.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxLanguages.FormattingEnabled = true;
            this.listBoxLanguages.Location = new System.Drawing.Point(81, 43);
            this.listBoxLanguages.Name = "listBoxLanguages";
            this.listBoxLanguages.Size = new System.Drawing.Size(330, 82);
            this.listBoxLanguages.TabIndex = 5;
            this.listBoxLanguages.Enter += new System.EventHandler(this.ListBoxLanguagesEnter);
            // 
            // btnLanguageRemove
            // 
            this.btnLanguageRemove.Location = new System.Drawing.Point(0, 72);
            this.btnLanguageRemove.Name = "btnLanguageRemove";
            this.btnLanguageRemove.Size = new System.Drawing.Size(75, 23);
            this.btnLanguageRemove.TabIndex = 3;
            this.btnLanguageRemove.Text = "Remove";
            this.btnLanguageRemove.UseVisualStyleBackColor = true;
            this.btnLanguageRemove.Click += new System.EventHandler(this.BtnLanguageRemoveClick);
            // 
            // lblLanguage
            // 
            this.lblLanguage.AutoSize = true;
            this.lblLanguage.Location = new System.Drawing.Point(-3, 0);
            this.lblLanguage.Name = "lblLanguage";
            this.lblLanguage.Size = new System.Drawing.Size(55, 13);
            this.lblLanguage.TabIndex = 0;
            this.lblLanguage.Text = "Language";
            // 
            // btnLanguageAdd
            // 
            this.btnLanguageAdd.Location = new System.Drawing.Point(0, 43);
            this.btnLanguageAdd.Name = "btnLanguageAdd";
            this.btnLanguageAdd.Size = new System.Drawing.Size(75, 23);
            this.btnLanguageAdd.TabIndex = 2;
            this.btnLanguageAdd.Text = "Add";
            this.btnLanguageAdd.UseVisualStyleBackColor = true;
            this.btnLanguageAdd.Click += new System.EventHandler(this.BtnLanguageAddClick);
            // 
            // lblCPU
            // 
            this.lblCPU.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCPU.AutoSize = true;
            this.lblCPU.Location = new System.Drawing.Point(414, 87);
            this.lblCPU.Name = "lblCPU";
            this.lblCPU.Size = new System.Drawing.Size(29, 13);
            this.lblCPU.TabIndex = 8;
            this.lblCPU.Text = "CPU";
            // 
            // comboBoxCpu
            // 
            this.comboBoxCpu.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxCpu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCpu.FormattingEnabled = true;
            this.comboBoxCpu.Location = new System.Drawing.Point(417, 103);
            this.comboBoxCpu.Name = "comboBoxCpu";
            this.comboBoxCpu.Size = new System.Drawing.Size(70, 21);
            this.comboBoxCpu.TabIndex = 9;
            this.comboBoxCpu.SelectedIndexChanged += new System.EventHandler(this.ComboBoxCpuSelectedIndexChanged);
            // 
            // lblOS
            // 
            this.lblOS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblOS.AutoSize = true;
            this.lblOS.Location = new System.Drawing.Point(414, 47);
            this.lblOS.Name = "lblOS";
            this.lblOS.Size = new System.Drawing.Size(22, 13);
            this.lblOS.TabIndex = 6;
            this.lblOS.Text = "OS";
            // 
            // comboBoxOS
            // 
            this.comboBoxOS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxOS.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxOS.FormattingEnabled = true;
            this.comboBoxOS.Location = new System.Drawing.Point(417, 63);
            this.comboBoxOS.Name = "comboBoxOS";
            this.comboBoxOS.Size = new System.Drawing.Size(70, 21);
            this.comboBoxOS.TabIndex = 7;
            this.comboBoxOS.SelectedIndexChanged += new System.EventHandler(this.ComboBoxOsSelectedIndexChanged);
            // 
            // TargetBaseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnLanguageClear);
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.listBoxLanguages);
            this.Controls.Add(this.btnLanguageRemove);
            this.Controls.Add(this.lblLanguage);
            this.Controls.Add(this.btnLanguageAdd);
            this.Controls.Add(this.lblCPU);
            this.Controls.Add(this.comboBoxCpu);
            this.Controls.Add(this.lblOS);
            this.Controls.Add(this.comboBoxOS);
            this.Name = "TargetBaseControl";
            this.Size = new System.Drawing.Size(489, 128);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLanguageClear;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.ListBox listBoxLanguages;
        private System.Windows.Forms.Button btnLanguageRemove;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.Button btnLanguageAdd;
        private System.Windows.Forms.Label lblCPU;
        private System.Windows.Forms.ComboBox comboBoxCpu;
        private System.Windows.Forms.Label lblOS;
        private System.Windows.Forms.ComboBox comboBoxOS;

    }
}
