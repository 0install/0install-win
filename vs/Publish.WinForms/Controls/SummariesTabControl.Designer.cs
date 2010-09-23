namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class SummariesTabControl
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
            this.addRemoveTabControl1 = new Common.Controls.AddRemoveTabControl();
            this.SuspendLayout();
            // 
            // addRemoveTabControl1
            // 
            this.addRemoveTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.addRemoveTabControl1.Location = new System.Drawing.Point(0, 0);
            this.addRemoveTabControl1.Name = "addRemoveTabControl1";
            this.addRemoveTabControl1.Size = new System.Drawing.Size(316, 164);
            this.addRemoveTabControl1.TabIndex = 0;
            this.addRemoveTabControl1.NewTabCreated += new Common.Controls.NewTabEventHandler(this.AddRemoveTabControl1NewTabCreated);
            // 
            // SummariesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.addRemoveTabControl1);
            this.Name = "SummariesControl";
            this.Size = new System.Drawing.Size(316, 164);
            this.ResumeLayout(false);

        }

        #endregion

        private Common.Controls.AddRemoveTabControl addRemoveTabControl1;


    }
}
