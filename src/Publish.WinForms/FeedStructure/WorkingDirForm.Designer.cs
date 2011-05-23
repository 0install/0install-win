namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class WorkingDirForm
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
            this.labelSource = new System.Windows.Forms.Label();
            this.hintTextBoxSource = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(116, 72);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(197, 72);
            // 
            // labelSource
            // 
            this.labelSource.AutoSize = true;
            this.labelSource.Location = new System.Drawing.Point(12, 9);
            this.labelSource.Name = "labelSource";
            this.labelSource.Size = new System.Drawing.Size(41, 13);
            this.labelSource.TabIndex = 0;
            this.labelSource.Text = "Source";
            // 
            // hintTextBoxSource
            // 
            this.hintTextBoxSource.HintText = "The relative path to the dir in the implementation.";
            this.hintTextBoxSource.Location = new System.Drawing.Point(12, 25);
            this.hintTextBoxSource.Name = "hintTextBoxSource";
            this.hintTextBoxSource.Size = new System.Drawing.Size(260, 20);
            this.hintTextBoxSource.TabIndex = 1;
            // 
            // WorkingDirForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 107);
            this.Controls.Add(this.labelSource);
            this.Controls.Add(this.hintTextBoxSource);
            this.Name = "WorkingDirForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.hintTextBoxSource, 0);
            this.Controls.SetChildIndex(this.labelSource, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelSource;
        private Common.Controls.HintTextBox hintTextBoxSource;
    }
}
