namespace ZeroInstall.Publish.WinForms.Dialogs
{
    partial class ExecutableInPathDialog
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
            this.hintTextBoxName = new Common.Controls.HintTextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.labelCommand = new System.Windows.Forms.Label();
            this.hintTextBoxCommand = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(156, 119);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(237, 119);
            // 
            // hintTextBoxName
            // 
            this.hintTextBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxName.HintText = "The name of the executable (without file extensions).";
            this.hintTextBoxName.Location = new System.Drawing.Point(15, 25);
            this.hintTextBoxName.Name = "hintTextBoxName";
            this.hintTextBoxName.Size = new System.Drawing.Size(297, 20);
            this.hintTextBoxName.TabIndex = 1;
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
            // labelCommand
            // 
            this.labelCommand.AutoSize = true;
            this.labelCommand.Location = new System.Drawing.Point(12, 48);
            this.labelCommand.Name = "labelCommand";
            this.labelCommand.Size = new System.Drawing.Size(54, 13);
            this.labelCommand.TabIndex = 2;
            this.labelCommand.Text = "Command";
            // 
            // hintTextBoxCommand
            // 
            this.hintTextBoxCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxCommand.HintText = "The name of command in the implementation to launch.";
            this.hintTextBoxCommand.Location = new System.Drawing.Point(15, 64);
            this.hintTextBoxCommand.Name = "hintTextBoxCommand";
            this.hintTextBoxCommand.Size = new System.Drawing.Size(297, 20);
            this.hintTextBoxCommand.TabIndex = 3;
            // 
            // ExecutableInPathDialog
            // 
            this.ClientSize = new System.Drawing.Size(324, 154);
            this.Controls.Add(this.hintTextBoxCommand);
            this.Controls.Add(this.labelCommand);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.hintTextBoxName);
            this.Name = "ExecutableInPathDialog";
            this.Text = "Edit executable in PATH binding";
            this.Controls.SetChildIndex(this.hintTextBoxName, 0);
            this.Controls.SetChildIndex(this.labelName, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.labelCommand, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.hintTextBoxCommand, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.Controls.HintTextBox hintTextBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelCommand;
        private Common.Controls.HintTextBox hintTextBoxCommand;
    }
}
