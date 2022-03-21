using System.ComponentModel;

namespace ZeroInstall.Central.WinForms
{
    partial class AppPopup
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
            this.labelBorder = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonIntegrate = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.iconStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelBorder
            // 
            this.labelBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelBorder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelBorder.Location = new System.Drawing.Point(0, 0);
            this.labelBorder.Name = "labelBorder";
            this.labelBorder.Size = new System.Drawing.Size(240, 120);
            this.labelBorder.TabIndex = 0;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.Location = new System.Drawing.Point(12, 9);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(173, 33);
            this.labelStatus.TabIndex = 2;
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
            this.buttonIntegrate.TabIndex = 4;
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
            this.buttonRemove.TabIndex = 5;
            this.buttonRemove.Text = "(Remove)";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Visible = false;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.FlatAppearance.BorderColor = System.Drawing.SystemColors.WindowFrame;
            this.buttonClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonClose.Location = new System.Drawing.Point(217, 0);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(23, 23);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.TabStop = false;
            this.buttonClose.Text = "X";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // iconStatus
            // 
            this.iconStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.iconStatus.Location = new System.Drawing.Point(191, 4);
            this.iconStatus.Name = "iconStatus";
            this.iconStatus.Size = new System.Drawing.Size(16, 16);
            this.iconStatus.TabIndex = 1;
            // 
            // AppPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(240, 120);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonIntegrate);
            this.Controls.Add(this.iconStatus);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelBorder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "AppPopup";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Label labelBorder;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label iconStatus;
        private System.Windows.Forms.Button buttonIntegrate;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonClose;

        #endregion
    }
}

