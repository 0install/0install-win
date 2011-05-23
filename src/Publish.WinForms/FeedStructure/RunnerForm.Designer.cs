namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class RunnerForm
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
            this.labelCommand = new System.Windows.Forms.Label();
            this.textCommand = new Common.Controls.HintTextBox();
            this.groupBoxArguments = new System.Windows.Forms.GroupBox();
            this.argumentsControl = new ZeroInstall.Publish.WinForms.Controls.ArgumentsControl();
            this.groupBoxArguments.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(156, 391);
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(237, 391);
            // 
            // labelCommand
            // 
            this.labelCommand.AutoSize = true;
            this.labelCommand.Location = new System.Drawing.Point(12, 190);
            this.labelCommand.Name = "labelCommand";
            this.labelCommand.Size = new System.Drawing.Size(54, 13);
            this.labelCommand.TabIndex = 12;
            this.labelCommand.Text = "Command";
            // 
            // textCommand
            // 
            this.textCommand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textCommand.HintText = "";
            this.textCommand.Location = new System.Drawing.Point(15, 206);
            this.textCommand.Name = "textCommand";
            this.textCommand.Size = new System.Drawing.Size(297, 20);
            this.textCommand.TabIndex = 13;
            // 
            // groupBoxArguments
            // 
            this.groupBoxArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxArguments.Controls.Add(this.argumentsControl);
            this.groupBoxArguments.Location = new System.Drawing.Point(15, 232);
            this.groupBoxArguments.Name = "groupBoxArguments";
            this.groupBoxArguments.Size = new System.Drawing.Size(297, 154);
            this.groupBoxArguments.TabIndex = 14;
            this.groupBoxArguments.TabStop = false;
            this.groupBoxArguments.Text = "Arguments";
            // 
            // argumentsControl
            // 
            this.argumentsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.argumentsControl.Location = new System.Drawing.Point(6, 19);
            this.argumentsControl.Name = "argumentsControl";
            this.argumentsControl.Size = new System.Drawing.Size(285, 133);
            this.argumentsControl.TabIndex = 0;
            // 
            // RunnerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 426);
            this.Controls.Add(this.labelCommand);
            this.Controls.Add(this.textCommand);
            this.Controls.Add(this.groupBoxArguments);
            this.Name = "RunnerForm";
            this.ShowInTaskbar = false;
            this.Text = "Edit runner";
            this.Controls.SetChildIndex(this.groupBoxArguments, 0);
            this.Controls.SetChildIndex(this.textCommand, 0);
            this.Controls.SetChildIndex(this.labelCommand, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.groupBoxArguments.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCommand;
        private Common.Controls.HintTextBox textCommand;
        private System.Windows.Forms.GroupBox groupBoxArguments;
        private Controls.ArgumentsControl argumentsControl;
    }
}