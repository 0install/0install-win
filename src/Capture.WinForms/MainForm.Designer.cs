namespace ZeroInstall.Capture.WinForms
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
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelPath = new System.Windows.Forms.Label();
            this.textBoxPath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelInfo.Location = new System.Drawing.Point(12, 9);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(318, 20);
            this.labelInfo.TabIndex = 0;
            this.labelInfo.Text = "Please use the 0capture command-line tool.";
            // 
            // labelPath
            // 
            this.labelPath.AutoSize = true;
            this.labelPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelPath.Location = new System.Drawing.Point(12, 45);
            this.labelPath.Name = "labelPath";
            this.labelPath.Size = new System.Drawing.Size(233, 20);
            this.labelPath.TabIndex = 1;
            this.labelPath.Text = "If it is not in your PATH execute:";
            // 
            // textBoxPath
            // 
            this.textBoxPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxPath.Location = new System.Drawing.Point(16, 68);
            this.textBoxPath.Name = "textBoxPath";
            this.textBoxPath.ReadOnly = true;
            this.textBoxPath.Size = new System.Drawing.Size(496, 26);
            this.textBoxPath.TabIndex = 2;
            this.textBoxPath.Text = "0alias 0capture http://0install.de/feeds/ZeroInstall_Tools.xml 0capture";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 106);
            this.Controls.Add(this.textBoxPath);
            this.Controls.Add(this.labelPath);
            this.Controls.Add(this.labelInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zero Install Capture";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelPath;
        private System.Windows.Forms.TextBox textBoxPath;

    }
}