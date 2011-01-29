namespace Common.Controls
{
    partial class ErrorReportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ErrorReportForm));
            this.infoLabel = new System.Windows.Forms.Label();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.detailsLabel = new System.Windows.Forms.Label();
            this.detailsBox = new System.Windows.Forms.TextBox();
            this.commentLabel = new System.Windows.Forms.Label();
            this.buttonReport = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.commentBox = new Common.Controls.HintTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // infoLabel
            // 
            this.infoLabel.AccessibleDescription = null;
            this.infoLabel.AccessibleName = null;
            resources.ApplyResources(this.infoLabel, "infoLabel");
            this.infoLabel.BackColor = System.Drawing.Color.Transparent;
            this.infoLabel.Name = "infoLabel";
            // 
            // pictureBox
            // 
            this.pictureBox.AccessibleDescription = null;
            this.pictureBox.AccessibleName = null;
            resources.ApplyResources(this.pictureBox, "pictureBox");
            this.pictureBox.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox.BackgroundImage = null;
            this.pictureBox.Font = null;
            this.pictureBox.Image = global::Common.Properties.Resources.Warning;
            this.pictureBox.ImageLocation = null;
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.TabStop = false;
            // 
            // detailsLabel
            // 
            this.detailsLabel.AccessibleDescription = null;
            this.detailsLabel.AccessibleName = null;
            resources.ApplyResources(this.detailsLabel, "detailsLabel");
            this.detailsLabel.Name = "detailsLabel";
            // 
            // detailsBox
            // 
            this.detailsBox.AccessibleDescription = null;
            this.detailsBox.AccessibleName = null;
            resources.ApplyResources(this.detailsBox, "detailsBox");
            this.detailsBox.BackColor = System.Drawing.SystemColors.Control;
            this.detailsBox.BackgroundImage = null;
            this.detailsBox.Font = null;
            this.detailsBox.Name = "detailsBox";
            this.detailsBox.ReadOnly = true;
            this.detailsBox.TabStop = false;
            // 
            // commentLabel
            // 
            this.commentLabel.AccessibleDescription = null;
            this.commentLabel.AccessibleName = null;
            resources.ApplyResources(this.commentLabel, "commentLabel");
            this.commentLabel.Name = "commentLabel";
            // 
            // buttonReport
            // 
            this.buttonReport.AccessibleDescription = null;
            this.buttonReport.AccessibleName = null;
            resources.ApplyResources(this.buttonReport, "buttonReport");
            this.buttonReport.BackgroundImage = null;
            this.buttonReport.Font = null;
            this.buttonReport.Name = "buttonReport";
            this.buttonReport.UseVisualStyleBackColor = true;
            this.buttonReport.Click += new System.EventHandler(this.buttonReport_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.AccessibleDescription = null;
            this.buttonCancel.AccessibleName = null;
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.BackgroundImage = null;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = null;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // commentBox
            // 
            this.commentBox.AccessibleDescription = null;
            this.commentBox.AccessibleName = null;
            resources.ApplyResources(this.commentBox, "commentBox");
            this.commentBox.BackColor = System.Drawing.SystemColors.Window;
            this.commentBox.BackgroundImage = null;
            this.commentBox.Font = null;
            this.commentBox.Name = "commentBox";
            this.commentBox.TabStop = false;
            // 
            // ErrorReportForm
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.commentBox);
            this.Controls.Add(this.commentLabel);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonReport);
            this.Controls.Add(this.detailsBox);
            this.Controls.Add(this.detailsLabel);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.pictureBox);
            this.DoubleBuffered = true;
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = null;
            this.MaximizeBox = false;
            this.Name = "ErrorReportForm";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label infoLabel;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.Label detailsLabel;
        private System.Windows.Forms.TextBox detailsBox;
        private System.Windows.Forms.Label commentLabel;
        private HintTextBox commentBox;
        private System.Windows.Forms.Button buttonReport;
        private System.Windows.Forms.Button buttonCancel;


    }
}