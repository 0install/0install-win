namespace ZeroInstall.Central.WinForms
{
    partial class SelectCommandDialog
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectCommandDialog));
            this.labelCommand = new System.Windows.Forms.Label();
            this.comboBoxCommand = new System.Windows.Forms.ComboBox();
            this.labelSummary = new System.Windows.Forms.Label();
            this.labelOptions = new System.Windows.Forms.Label();
            this.checkBoxCustomizeVersion = new System.Windows.Forms.CheckBox();
            this.checkBoxRefresh = new System.Windows.Forms.CheckBox();
            this.labelArgs = new System.Windows.Forms.Label();
            this.textBoxArgs = new System.Windows.Forms.TextBox();
            this.groupBoxCommandLine = new System.Windows.Forms.GroupBox();
            this.textBoxCommandLine = new System.Windows.Forms.TextBox();
            this.groupBoxCommandLine.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelCommand
            // 
            resources.ApplyResources(this.labelCommand, "labelCommand");
            this.labelCommand.Name = "labelCommand";
            // 
            // comboBoxCommand
            // 
            resources.ApplyResources(this.comboBoxCommand, "comboBoxCommand");
            this.comboBoxCommand.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxCommand.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxCommand.FormattingEnabled = true;
            this.comboBoxCommand.Name = "comboBoxCommand";
            this.comboBoxCommand.SelectedIndexChanged += new System.EventHandler(this.UpdateDescription);
            this.comboBoxCommand.TextChanged += new System.EventHandler(this.UpdateDescription);
            // 
            // labelSummary
            // 
            resources.ApplyResources(this.labelSummary, "labelSummary");
            this.labelSummary.AutoEllipsis = true;
            this.labelSummary.Name = "labelSummary";
            // 
            // labelOptions
            // 
            resources.ApplyResources(this.labelOptions, "labelOptions");
            this.labelOptions.Name = "labelOptions";
            // 
            // checkBoxCustomizeVersion
            // 
            resources.ApplyResources(this.checkBoxCustomizeVersion, "checkBoxCustomizeVersion");
            this.checkBoxCustomizeVersion.Name = "checkBoxCustomizeVersion";
            this.checkBoxCustomizeVersion.UseVisualStyleBackColor = true;
            this.checkBoxCustomizeVersion.CheckedChanged += new System.EventHandler(this.UpdateDescription);
            // 
            // checkBoxRefresh
            // 
            resources.ApplyResources(this.checkBoxRefresh, "checkBoxRefresh");
            this.checkBoxRefresh.Name = "checkBoxRefresh";
            this.checkBoxRefresh.UseVisualStyleBackColor = true;
            this.checkBoxRefresh.CheckedChanged += new System.EventHandler(this.UpdateDescription);
            // 
            // labelArgs
            // 
            resources.ApplyResources(this.labelArgs, "labelArgs");
            this.labelArgs.Name = "labelArgs";
            // 
            // textBoxArgs
            // 
            resources.ApplyResources(this.textBoxArgs, "textBoxArgs");
            this.textBoxArgs.Name = "textBoxArgs";
            this.textBoxArgs.TextChanged += new System.EventHandler(this.UpdateDescription);
            // 
            // groupBoxCommandLine
            // 
            resources.ApplyResources(this.groupBoxCommandLine, "groupBoxCommandLine");
            this.groupBoxCommandLine.Controls.Add(this.textBoxCommandLine);
            this.groupBoxCommandLine.Name = "groupBoxCommandLine";
            this.groupBoxCommandLine.TabStop = false;
            // 
            // textBoxCommandLine
            // 
            resources.ApplyResources(this.textBoxCommandLine, "textBoxCommandLine");
            this.textBoxCommandLine.Name = "textBoxCommandLine";
            this.textBoxCommandLine.ReadOnly = true;
            // 
            // SelectCommandDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxCommandLine);
            this.Controls.Add(this.textBoxArgs);
            this.Controls.Add(this.labelArgs);
            this.Controls.Add(this.checkBoxRefresh);
            this.Controls.Add(this.checkBoxCustomizeVersion);
            this.Controls.Add(this.labelOptions);
            this.Controls.Add(this.labelSummary);
            this.Controls.Add(this.comboBoxCommand);
            this.Controls.Add(this.labelCommand);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "SelectCommandDialog";
            this.Load += new System.EventHandler(this.SelectCommandDialog_Load);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.labelCommand, 0);
            this.Controls.SetChildIndex(this.comboBoxCommand, 0);
            this.Controls.SetChildIndex(this.labelSummary, 0);
            this.Controls.SetChildIndex(this.labelOptions, 0);
            this.Controls.SetChildIndex(this.checkBoxCustomizeVersion, 0);
            this.Controls.SetChildIndex(this.checkBoxRefresh, 0);
            this.Controls.SetChildIndex(this.labelArgs, 0);
            this.Controls.SetChildIndex(this.textBoxArgs, 0);
            this.Controls.SetChildIndex(this.groupBoxCommandLine, 0);
            this.groupBoxCommandLine.ResumeLayout(false);
            this.groupBoxCommandLine.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelCommand;
        private System.Windows.Forms.ComboBox comboBoxCommand;
        private System.Windows.Forms.Label labelSummary;
        private System.Windows.Forms.Label labelOptions;
        private System.Windows.Forms.CheckBox checkBoxCustomizeVersion;
        private System.Windows.Forms.CheckBox checkBoxRefresh;
        private System.Windows.Forms.Label labelArgs;
        private System.Windows.Forms.TextBox textBoxArgs;
        private System.Windows.Forms.GroupBox groupBoxCommandLine;
        private System.Windows.Forms.TextBox textBoxCommandLine;
    }
}
