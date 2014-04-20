using NanoByte.Common.Controls;

namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class SingleFileEditor
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
            this.labelDestination = new System.Windows.Forms.Label();
            this.textBoxDestination = new HintTextBox();
            this.SuspendLayout();
            // 
            // labelDestination
            // 
            this.labelDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDestination.AutoSize = true;
            this.labelDestination.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelDestination.Location = new System.Drawing.Point(0, 55);
            this.labelDestination.Name = "labelDestination";
            this.labelDestination.Size = new System.Drawing.Size(63, 13);
            this.labelDestination.TabIndex = 4;
            this.labelDestination.Text = "Destination:";
            // 
            // textBoxDestination
            // 
            this.textBoxDestination.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDestination.HintText = "the local file name to use (required)";
            this.textBoxDestination.Location = new System.Drawing.Point(77, 52);
            this.textBoxDestination.Name = "textBoxDestination";
            this.textBoxDestination.Size = new System.Drawing.Size(73, 20);
            this.textBoxDestination.TabIndex = 5;
            this.textBoxDestination.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // SingleFileEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelDestination);
            this.Controls.Add(this.textBoxDestination);
            this.Name = "SingleFileEditor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelDestination;
        private HintTextBox textBoxDestination;

    }
}
