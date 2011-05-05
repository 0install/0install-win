namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class CommandForm
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
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.labelPath = new System.Windows.Forms.Label();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.argumentsControl = new ZeroInstall.Publish.WinForms.Controls.ArgumentsControl();
            this.groupBoxArguments = new System.Windows.Forms.GroupBox();
            this.groupBoxWorkingDir = new System.Windows.Forms.GroupBox();
            this.checkBoxWorkingDir = new System.Windows.Forms.CheckBox();
            this.labelSource = new System.Windows.Forms.Label();
            this.hintTextBoxSource = new Common.Controls.HintTextBox();
            this.groupBoxArguments.SuspendLayout();
            this.groupBoxWorkingDir.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(241, 328);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(322, 328);
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(15, 25);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(382, 20);
            this.textBoxName.TabIndex = 1002;
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(12, 9);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(35, 13);
            this.labelName.TabIndex = 1003;
            this.labelName.Text = "Name";
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Location = new System.Drawing.Point(12, 48);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(29, 13);
            this.labelPath.TabIndex = 1004;
            this.labelPath.Text = "Path";
            // 
            // textBoxPath
            // 
            this.textBoxPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPath.Location = new System.Drawing.Point(15, 64);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.Size = new System.Drawing.Size(382, 20);
            this.textBoxPath.TabIndex = 1005;
            // 
            // argumentsControl
            // 
            this.argumentsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.argumentsControl.Location = new System.Drawing.Point(6, 19);
            this.argumentsControl.Name = "argumentsControl";
            this.argumentsControl.Size = new System.Drawing.Size(370, 133);
            this.argumentsControl.TabIndex = 1006;
            // 
            // groupBoxArguments
            // 
            this.groupBoxArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxArguments.Controls.Add(this.argumentsControl);
            this.groupBoxArguments.Location = new System.Drawing.Point(15, 90);
            this.groupBoxArguments.Name = "groupBoxArguments";
            this.groupBoxArguments.Size = new System.Drawing.Size(382, 154);
            this.groupBoxArguments.TabIndex = 1009;
            this.groupBoxArguments.TabStop = false;
            this.groupBoxArguments.Text = "Arguments";
            // 
            // groupBoxWorkingDir
            // 
            this.groupBoxWorkingDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxWorkingDir.Controls.Add(this.checkBoxWorkingDir);
            this.groupBoxWorkingDir.Controls.Add(this.labelSource);
            this.groupBoxWorkingDir.Controls.Add(this.hintTextBoxSource);
            this.groupBoxWorkingDir.Location = new System.Drawing.Point(15, 248);
            this.groupBoxWorkingDir.Name = "groupBoxWorkingDir";
            this.groupBoxWorkingDir.Size = new System.Drawing.Size(382, 59);
            this.groupBoxWorkingDir.TabIndex = 1010;
            this.groupBoxWorkingDir.TabStop = false;
            this.groupBoxWorkingDir.Text = "Working Directory";
            // 
            // checkBoxWorkingDir
            // 
            this.checkBoxWorkingDir.AutoSize = true;
            this.checkBoxWorkingDir.Location = new System.Drawing.Point(9, 35);
            this.checkBoxWorkingDir.Name = "checkBoxWorkingDir";
            this.checkBoxWorkingDir.Size = new System.Drawing.Size(15, 14);
            this.checkBoxWorkingDir.TabIndex = 1011;
            this.checkBoxWorkingDir.UseVisualStyleBackColor = true;
            this.checkBoxWorkingDir.CheckedChanged += new System.EventHandler(this.CheckBoxWorkingDirCheckedChanged);
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(6, 16);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(41, 13);
            this.labelSource.TabIndex = 1009;
            this.labelSource.Text = "Source";
            // 
            // hintTextBoxSource
            // 
            this.hintTextBoxSource.HintText = "The relative path to the dir in the implementation.";
            this.hintTextBoxSource.Location = new System.Drawing.Point(30, 32);
            this.hintTextBoxSource.Name = "hintTextBoxSource";
            this.hintTextBoxSource.Size = new System.Drawing.Size(346, 20);
            this.hintTextBoxSource.TabIndex = 1010;
            // 
            // CommandForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 363);
            this.Controls.Add(this.groupBoxWorkingDir);
            this.Controls.Add(this.groupBoxArguments);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.textBoxPath);
            this.Controls.Add(this.labelPath);
            this.Name = "CommandForm";
            this.Text = "Command";
            this.Controls.SetChildIndex(this.labelPath, 0);
            this.Controls.SetChildIndex(this.textBoxPath, 0);
            this.Controls.SetChildIndex(this.textBoxName, 0);
            this.Controls.SetChildIndex(this.labelName, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.groupBoxArguments, 0);
            this.Controls.SetChildIndex(this.groupBoxWorkingDir, 0);
            this.groupBoxArguments.ResumeLayout(false);
            this.groupBoxWorkingDir.ResumeLayout(false);
            this.groupBoxWorkingDir.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.TextBox textBoxPath;
        private Controls.ArgumentsControl argumentsControl;
        private System.Windows.Forms.GroupBox groupBoxArguments;
        private System.Windows.Forms.GroupBox groupBoxWorkingDir;
        private System.Windows.Forms.Label labelSource;
        private Common.Controls.HintTextBox hintTextBoxSource;
        private System.Windows.Forms.CheckBox checkBoxWorkingDir;
    }
}