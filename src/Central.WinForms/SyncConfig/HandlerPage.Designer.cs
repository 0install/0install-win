namespace ZeroInstall.Central.WinForms.SyncConfig
{
    partial class HandlerPage
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
            this.labelWorking = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelWorking
            // 
            this.labelWorking.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWorking.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelWorking.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelWorking.Location = new System.Drawing.Point(0, 244);
            this.labelWorking.Name = "labelWorking";
            this.labelWorking.Size = new System.Drawing.Size(470, 35);
            this.labelWorking.TabIndex = 1000;
            this.labelWorking.Text = "Working...";
            this.labelWorking.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelWorking.Visible = false;
            // 
            // HandlerPage
            // 
            this.Controls.Add(this.labelWorking);
            this.Name = "HandlerPage";
            this.Size = new System.Drawing.Size(470, 300);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelWorking;

    }
}
