namespace ZeroInstall
{
    partial class MainForm
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
            this.pictureBoxSplashScreen = new System.Windows.Forms.PictureBox();
            this.taskControl = new NanoByte.Common.Controls.TaskControl();
            this.labelAppName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSplashScreen)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxSplashScreen
            // 
            this.pictureBoxSplashScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSplashScreen.BackgroundImage = global::ZeroInstall.Properties.Resources.SplashScreen;
            this.pictureBoxSplashScreen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBoxSplashScreen.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxSplashScreen.Name = "pictureBoxSplashScreen";
            this.pictureBoxSplashScreen.Size = new System.Drawing.Size(560, 200);
            this.pictureBoxSplashScreen.TabIndex = 0;
            this.pictureBoxSplashScreen.TabStop = false;
            // 
            // taskControl
            // 
            this.taskControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.taskControl.Location = new System.Drawing.Point(12, 245);
            this.taskControl.Name = "taskControl";
            this.taskControl.Size = new System.Drawing.Size(536, 54);
            this.taskControl.TabIndex = 1;
            this.taskControl.Visible = false;
            // 
            // labelAppName
            // 
            this.labelAppName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAppName.AutoEllipsis = true;
            this.labelAppName.Font = new System.Drawing.Font("Segoe UI", 15.75F);
            this.labelAppName.Location = new System.Drawing.Point(12, 202);
            this.labelAppName.Name = "labelAppName";
            this.labelAppName.Size = new System.Drawing.Size(536, 40);
            this.labelAppName.TabIndex = 0;
            this.labelAppName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(560, 311);
            this.Controls.Add(this.labelAppName);
            this.Controls.Add(this.taskControl);
            this.Controls.Add(this.pictureBoxSplashScreen);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(370, 270);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zero Install";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSplashScreen)).EndInit();
            this.ResumeLayout(false);
        }


        #endregion

        private System.Windows.Forms.PictureBox pictureBoxSplashScreen;
        private NanoByte.Common.Controls.TaskControl taskControl;
        private System.Windows.Forms.Label labelAppName;
    }
}
