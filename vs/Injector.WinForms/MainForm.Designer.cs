namespace ZeroInstall.Injector.WinForms
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.progressBar = new Common.Controls.TrackingProgressBar();
            this.labelOperation = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 38);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(285, 23);
            this.progressBar.TabIndex = 1;
            this.progressBar.UseTaskbar = true;
            // 
            // labelOperation
            // 
            this.labelOperation.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelOperation.AutoSize = true;
            this.labelOperation.Location = new System.Drawing.Point(12, 14);
            this.labelOperation.Name = "labelOperation";
            this.labelOperation.Size = new System.Drawing.Size(51, 13);
            this.labelOperation.TabIndex = 0;
            this.labelOperation.Text = "Solving...";
            // 
            // labelName
            // 
            this.labelName.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(9, 64);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(41, 13);
            this.labelName.TabIndex = 2;
            this.labelName.Text = "(Name)";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(222, 87);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(309, 122);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.labelOperation);
            this.Controls.Add(this.progressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(200, 160);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Launching application";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Common.Controls.TrackingProgressBar progressBar;
        #endregion
        private System.Windows.Forms.Label labelOperation;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Button buttonCancel;

    }
}