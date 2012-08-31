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
            this.comboBoxCommand = new System.Windows.Forms.ComboBox();
            this.labelSummary = new System.Windows.Forms.Label();
            this.textBoxArgs = new System.Windows.Forms.TextBox();
            this.labelArgs = new System.Windows.Forms.Label();
            this.labelCommand = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            // 
            // comboBoxCommand
            // 
            resources.ApplyResources(this.comboBoxCommand, "comboBoxCommand");
            this.comboBoxCommand.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBoxCommand.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxCommand.FormattingEnabled = true;
            this.comboBoxCommand.Name = "comboBoxCommand";
            this.comboBoxCommand.SelectedIndexChanged += new System.EventHandler(this.comboBoxCommand_SelectedIndexChanged);
            // 
            // labelSummary
            // 
            resources.ApplyResources(this.labelSummary, "labelSummary");
            this.labelSummary.Name = "labelSummary";
            // 
            // textBoxArgs
            // 
            resources.ApplyResources(this.textBoxArgs, "textBoxArgs");
            this.textBoxArgs.Name = "textBoxArgs";
            // 
            // labelArgs
            // 
            resources.ApplyResources(this.labelArgs, "labelArgs");
            this.labelArgs.Name = "labelArgs";
            // 
            // labelCommand
            // 
            resources.ApplyResources(this.labelCommand, "labelCommand");
            this.labelCommand.Name = "labelCommand";
            // 
            // SelectCommandDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelCommand);
            this.Controls.Add(this.comboBoxCommand);
            this.Controls.Add(this.labelSummary);
            this.Controls.Add(this.textBoxArgs);
            this.Controls.Add(this.labelArgs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "SelectCommandDialog";
            this.Load += new System.EventHandler(this.SelectCommandDialog_Load);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.labelArgs, 0);
            this.Controls.SetChildIndex(this.textBoxArgs, 0);
            this.Controls.SetChildIndex(this.labelSummary, 0);
            this.Controls.SetChildIndex(this.comboBoxCommand, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.labelCommand, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxCommand;
        private System.Windows.Forms.Label labelSummary;
        private System.Windows.Forms.TextBox textBoxArgs;
        private System.Windows.Forms.Label labelArgs;
        private System.Windows.Forms.Label labelCommand;
    }
}