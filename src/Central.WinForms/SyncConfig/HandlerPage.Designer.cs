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
            this.groupProgress = new System.Windows.Forms.GroupBox();
            this.trackingControl = new Common.Controls.TrackingControl();
            this.groupProgress.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupProgress
            // 
            this.groupProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupProgress.Controls.Add(this.trackingControl);
            this.groupProgress.Location = new System.Drawing.Point(13, 198);
            this.groupProgress.Name = "groupProgress";
            this.groupProgress.Size = new System.Drawing.Size(443, 88);
            this.groupProgress.TabIndex = 1000;
            this.groupProgress.TabStop = false;
            this.groupProgress.Visible = false;
            // 
            // trackingControl
            // 
            this.trackingControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackingControl.Location = new System.Drawing.Point(6, 19);
            this.trackingControl.Name = "trackingControl";
            this.trackingControl.Size = new System.Drawing.Size(431, 63);
            this.trackingControl.TabIndex = 0;
            // 
            // HandlerPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupProgress);
            this.Name = "HandlerPage";
            this.Size = new System.Drawing.Size(470, 300);
            this.groupProgress.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupProgress;
        private Common.Controls.TrackingControl trackingControl;
    }
}
