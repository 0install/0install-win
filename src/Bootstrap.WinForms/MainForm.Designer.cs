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
            this.textPath = new System.Windows.Forms.TextBox();
            this.buttonChangePath = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonContinue = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupPath = new System.Windows.Forms.GroupBox();
            this.buttonMachineWide = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSplashScreen)).BeginInit();
            this.groupPath.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxSplashScreen
            // 
            this.pictureBoxSplashScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
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
            // textPath
            // 
            this.textPath.AccessibleName = "Path";
            this.textPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.textPath.Location = new System.Drawing.Point(12, 19);
            this.textPath.Name = "textPath";
            this.textPath.ReadOnly = true;
            this.textPath.Size = new System.Drawing.Size(404, 20);
            this.textPath.TabIndex = 0;
            // 
            // buttonChangePath
            // 
            this.buttonChangePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChangePath.Location = new System.Drawing.Point(422, 18);
            this.buttonChangePath.Name = "buttonChangePath";
            this.buttonChangePath.Size = new System.Drawing.Size(79, 24);
            this.buttonChangePath.TabIndex = 1;
            this.buttonChangePath.Text = "(Change)";
            this.buttonChangePath.UseVisualStyleBackColor = true;
            this.buttonChangePath.Click += new System.EventHandler(this.buttonChangePath_Click);
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;
            // 
            // buttonContinue
            // 
            this.buttonContinue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonContinue.Location = new System.Drawing.Point(342, 262);
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.Size = new System.Drawing.Size(100, 37);
            this.buttonContinue.TabIndex = 4;
            this.buttonContinue.Text = "(Continue)";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Visible = false;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(448, 262);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 37);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "(Cancel)";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Visible = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupPath
            // 
            this.groupPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupPath.Controls.Add(this.textPath);
            this.groupPath.Controls.Add(this.buttonChangePath);
            this.groupPath.Location = new System.Drawing.Point(24, 190);
            this.groupPath.Name = "groupPath";
            this.groupPath.Size = new System.Drawing.Size(512, 54);
            this.groupPath.TabIndex = 2;
            this.groupPath.TabStop = false;
            this.groupPath.Text = "(DestinationFolder)";
            this.groupPath.Visible = false;
            // 
            // buttonMachineWide
            // 
            this.buttonMachineWide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMachineWide.Location = new System.Drawing.Point(12, 262);
            this.buttonMachineWide.Name = "buttonMachineWide";
            this.buttonMachineWide.Size = new System.Drawing.Size(100, 37);
            this.buttonMachineWide.TabIndex = 3;
            this.buttonMachineWide.Text = "(Machine Wide)";
            this.buttonMachineWide.UseVisualStyleBackColor = true;
            this.buttonMachineWide.Visible = false;
            this.buttonMachineWide.Click += new System.EventHandler(this.buttonMachineWide_Click);
            // 
            // MainForm
            // 
            this.AcceptButton = this.buttonContinue;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(560, 311);
            this.Controls.Add(this.buttonMachineWide);
            this.Controls.Add(this.groupPath);
            this.Controls.Add(this.buttonContinue);
            this.Controls.Add(this.buttonCancel);
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
            this.groupPath.ResumeLayout(false);
            this.groupPath.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxSplashScreen;
        private NanoByte.Common.Controls.TaskControl taskControl;
        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.Button buttonMachineWide;
        private System.Windows.Forms.GroupBox groupPath;
        private System.Windows.Forms.TextBox textPath;
        private System.Windows.Forms.Button buttonChangePath;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.Button buttonCancel;
    }
}
