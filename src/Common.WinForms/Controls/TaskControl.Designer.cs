namespace NanoByte.Common.Controls
{
    partial class TaskControl
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
            this.components = new System.ComponentModel.Container();
            this.labelOperation = new System.Windows.Forms.Label();
            this.progressBar = new Common.Controls.TaskProgressBar();
            this.progressLabel = new Common.Controls.TaskLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // labelOperation
            // 
            this.labelOperation.Dock = System.Windows.Forms.DockStyle.Top;
            this.labelOperation.Location = new System.Drawing.Point(0, 32);
            this.labelOperation.Name = "labelOperation";
            this.labelOperation.Size = new System.Drawing.Size(200, 16);
            this.labelOperation.TabIndex = 0;
            this.labelOperation.UseMnemonic = false;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressBar.Location = new System.Drawing.Point(0, 19);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(200, 13);
            this.progressBar.TabIndex = 1;
            this.progressBar.UseTaskbar = true;
            // 
            // progressLabel
            // 
            this.progressLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressLabel.Location = new System.Drawing.Point(0, 0);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(200, 19);
            this.progressLabel.TabIndex = 2;
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // TrackingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelOperation);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.progressLabel);
            this.Name = "TrackingControl";
            this.Size = new System.Drawing.Size(200, 50);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelOperation;
        private Common.Controls.TaskProgressBar progressBar;
        private Common.Controls.TaskLabel progressLabel;
        private System.Windows.Forms.ToolTip toolTip;

    }
}
