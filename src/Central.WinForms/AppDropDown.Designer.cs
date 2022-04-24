using System.ComponentModel;

namespace ZeroInstall.Central.WinForms
{
    partial class AppDropDown
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonIntegrate = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.AutoEllipsis = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 9);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(216, 33);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "(Status)";
            // 
            // buttonIntegrate
            // 
            this.buttonIntegrate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIntegrate.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonIntegrate.Location = new System.Drawing.Point(12, 50);
            this.buttonIntegrate.Name = "buttonIntegrate";
            this.buttonIntegrate.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.buttonIntegrate.Size = new System.Drawing.Size(216, 26);
            this.buttonIntegrate.TabIndex = 1;
            this.buttonIntegrate.Text = "(Integrate)";
            this.buttonIntegrate.UseVisualStyleBackColor = true;
            this.buttonIntegrate.Visible = false;
            this.buttonIntegrate.Click += new System.EventHandler(this.buttonIntegrate_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.buttonRemove.Location = new System.Drawing.Point(12, 82);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Padding = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.buttonRemove.Size = new System.Drawing.Size(216, 26);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "(Remove)";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Visible = false;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // AppDropDown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonIntegrate);
            this.Controls.Add(this.labelStatus);
            this.Name = "AppDropDown";
            this.Size = new System.Drawing.Size(240, 120);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonIntegrate;
        private System.Windows.Forms.Button buttonRemove;
        #endregion
    }
}

