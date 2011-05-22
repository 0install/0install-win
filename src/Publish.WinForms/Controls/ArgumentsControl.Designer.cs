namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class ArgumentsControl
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
            this.labelArgument = new System.Windows.Forms.Label();
            this.listBoxArguments = new System.Windows.Forms.ListBox();
            this.buttonAddArgument = new System.Windows.Forms.Button();
            this.textBoxArgument = new System.Windows.Forms.TextBox();
            this.buttonRemoveArgument = new System.Windows.Forms.Button();
            this.buttonClearArguments = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelArgument
            // 
            this.labelArgument.AutoSize = true;
            this.labelArgument.Location = new System.Drawing.Point(-3, 0);
            this.labelArgument.Name = "labelArgument";
            this.labelArgument.Size = new System.Drawing.Size(52, 13);
            this.labelArgument.TabIndex = 0;
            this.labelArgument.Text = "Argument";
            // 
            // listBoxArguments
            // 
            this.listBoxArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxArguments.FormattingEnabled = true;
            this.listBoxArguments.Location = new System.Drawing.Point(0, 42);
            this.listBoxArguments.Name = "listBoxArguments";
            this.listBoxArguments.Size = new System.Drawing.Size(260, 82);
            this.listBoxArguments.TabIndex = 2;
            // 
            // buttonAddArgument
            // 
            this.buttonAddArgument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddArgument.Location = new System.Drawing.Point(269, 42);
            this.buttonAddArgument.Name = "buttonAddArgument";
            this.buttonAddArgument.Size = new System.Drawing.Size(78, 23);
            this.buttonAddArgument.TabIndex = 3;
            this.buttonAddArgument.Text = "Add";
            this.buttonAddArgument.UseVisualStyleBackColor = true;
            // 
            // textBoxArgument
            // 
            this.textBoxArgument.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxArgument.Location = new System.Drawing.Point(0, 16);
            this.textBoxArgument.Name = "textBoxArgument";
            this.textBoxArgument.Size = new System.Drawing.Size(260, 20);
            this.textBoxArgument.TabIndex = 1;
            // 
            // buttonRemoveArgument
            // 
            this.buttonRemoveArgument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemoveArgument.Location = new System.Drawing.Point(269, 71);
            this.buttonRemoveArgument.Name = "buttonRemoveArgument";
            this.buttonRemoveArgument.Size = new System.Drawing.Size(78, 23);
            this.buttonRemoveArgument.TabIndex = 4;
            this.buttonRemoveArgument.Text = "Remove";
            this.buttonRemoveArgument.UseVisualStyleBackColor = true;
            // 
            // buttonClearArguments
            // 
            this.buttonClearArguments.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearArguments.Location = new System.Drawing.Point(269, 100);
            this.buttonClearArguments.Name = "buttonClearArguments";
            this.buttonClearArguments.Size = new System.Drawing.Size(78, 23);
            this.buttonClearArguments.TabIndex = 5;
            this.buttonClearArguments.Text = "Clear";
            this.buttonClearArguments.UseVisualStyleBackColor = true;
            // 
            // ArgumentsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonClearArguments);
            this.Controls.Add(this.buttonRemoveArgument);
            this.Controls.Add(this.textBoxArgument);
            this.Controls.Add(this.buttonAddArgument);
            this.Controls.Add(this.labelArgument);
            this.Controls.Add(this.listBoxArguments);
            this.Name = "ArgumentsControl";
            this.Size = new System.Drawing.Size(346, 127);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelArgument;
        private System.Windows.Forms.ListBox listBoxArguments;
        private System.Windows.Forms.Button buttonAddArgument;
        private System.Windows.Forms.TextBox textBoxArgument;
        private System.Windows.Forms.Button buttonRemoveArgument;
        private System.Windows.Forms.Button buttonClearArguments;
    }
}
