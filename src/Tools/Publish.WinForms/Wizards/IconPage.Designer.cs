using NanoByte.Common.Controls;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class IconPage
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.pictureBoxIcon = new System.Windows.Forms.PictureBox();
            this.buttonNext = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelStep1 = new System.Windows.Forms.Label();
            this.buttonSaveIco = new System.Windows.Forms.Button();
            this.labelStep2 = new System.Windows.Forms.Label();
            this.textBoxHrefIco = new NanoByte.Common.Controls.UriTextBox();
            this.textBoxHrefPng = new NanoByte.Common.Controls.UriTextBox();
            this.labelStep4 = new System.Windows.Forms.Label();
            this.buttonSavePng = new System.Windows.Forms.Button();
            this.labelStep3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).BeginInit();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 18);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(470, 37);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Icon";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBoxIcon
            // 
            this.pictureBoxIcon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxIcon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBoxIcon.Location = new System.Drawing.Point(40, 18);
            this.pictureBoxIcon.Name = "pictureBoxIcon";
            this.pictureBoxIcon.Size = new System.Drawing.Size(48, 48);
            this.pictureBoxIcon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxIcon.TabIndex = 20;
            this.pictureBoxIcon.TabStop = false;
            // 
            // buttonNext
            // 
            this.buttonNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonNext.Location = new System.Drawing.Point(315, 238);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(120, 35);
            this.buttonNext.TabIndex = 10;
            this.buttonNext.Text = "&Next >";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelInfo.Location = new System.Drawing.Point(35, 71);
            this.labelInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(400, 44);
            this.labelInfo.TabIndex = 1;
            this.labelInfo.Text = "Icons need to be extracted from the application and uploaded separately as both a" +
    "n ICO and a PNG.";
            // 
            // labelStep1
            // 
            this.labelStep1.AutoSize = true;
            this.labelStep1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelStep1.Location = new System.Drawing.Point(35, 120);
            this.labelStep1.Name = "labelStep1";
            this.labelStep1.Size = new System.Drawing.Size(22, 20);
            this.labelStep1.TabIndex = 2;
            this.labelStep1.Text = "1.";
            // 
            // buttonSaveIco
            // 
            this.buttonSaveIco.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonSaveIco.Location = new System.Drawing.Point(65, 116);
            this.buttonSaveIco.Name = "buttonSaveIco";
            this.buttonSaveIco.Size = new System.Drawing.Size(137, 28);
            this.buttonSaveIco.TabIndex = 3;
            this.buttonSaveIco.Text = "Extract as &ICO file";
            this.buttonSaveIco.UseVisualStyleBackColor = true;
            this.buttonSaveIco.Click += new System.EventHandler(this.buttonSaveIco_Click);
            // 
            // labelStep2
            // 
            this.labelStep2.AutoSize = true;
            this.labelStep2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelStep2.Location = new System.Drawing.Point(35, 147);
            this.labelStep2.Name = "labelStep2";
            this.labelStep2.Size = new System.Drawing.Size(207, 20);
            this.labelStep2.TabIndex = 4;
            this.labelStep2.Text = "2.   Where will you upload it?";
            // 
            // textBoxHrefIco
            // 
            this.textBoxHrefIco.AllowDrop = true;
            this.textBoxHrefIco.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHrefIco.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxHrefIco.HintText = "HTTP Address";
            this.textBoxHrefIco.HttpOnly = true;
            this.textBoxHrefIco.Location = new System.Drawing.Point(244, 144);
            this.textBoxHrefIco.Name = "textBoxHrefIco";
            this.textBoxHrefIco.Size = new System.Drawing.Size(191, 26);
            this.textBoxHrefIco.TabIndex = 5;
            // 
            // textBoxHrefPng
            // 
            this.textBoxHrefPng.AllowDrop = true;
            this.textBoxHrefPng.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHrefPng.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxHrefPng.HintText = "HTTP Address";
            this.textBoxHrefPng.HttpOnly = true;
            this.textBoxHrefPng.Location = new System.Drawing.Point(244, 198);
            this.textBoxHrefPng.Name = "textBoxHrefPng";
            this.textBoxHrefPng.Size = new System.Drawing.Size(191, 26);
            this.textBoxHrefPng.TabIndex = 9;
            // 
            // labelStep4
            // 
            this.labelStep4.AutoSize = true;
            this.labelStep4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelStep4.Location = new System.Drawing.Point(35, 201);
            this.labelStep4.Name = "labelStep4";
            this.labelStep4.Size = new System.Drawing.Size(207, 20);
            this.labelStep4.TabIndex = 8;
            this.labelStep4.Text = "4.   Where will you upload it?";
            // 
            // buttonSavePng
            // 
            this.buttonSavePng.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonSavePng.Location = new System.Drawing.Point(65, 170);
            this.buttonSavePng.Name = "buttonSavePng";
            this.buttonSavePng.Size = new System.Drawing.Size(137, 28);
            this.buttonSavePng.TabIndex = 7;
            this.buttonSavePng.Text = "Extract as &PNG file";
            this.buttonSavePng.UseVisualStyleBackColor = true;
            this.buttonSavePng.Click += new System.EventHandler(this.buttonSavePng_Click);
            // 
            // labelStep3
            // 
            this.labelStep3.AutoSize = true;
            this.labelStep3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelStep3.Location = new System.Drawing.Point(35, 174);
            this.labelStep3.Name = "labelStep3";
            this.labelStep3.Size = new System.Drawing.Size(22, 20);
            this.labelStep3.TabIndex = 6;
            this.labelStep3.Text = "3.";
            // 
            // IconPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxHrefPng);
            this.Controls.Add(this.labelStep4);
            this.Controls.Add(this.buttonSavePng);
            this.Controls.Add(this.labelStep3);
            this.Controls.Add(this.textBoxHrefIco);
            this.Controls.Add(this.labelStep2);
            this.Controls.Add(this.buttonSaveIco);
            this.Controls.Add(this.labelStep1);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.pictureBoxIcon);
            this.Controls.Add(this.labelTitle);
            this.Controls.Add(this.labelInfo);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "IconPage";
            this.Size = new System.Drawing.Size(470, 300);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIcon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.PictureBox pictureBoxIcon;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelStep1;
        private System.Windows.Forms.Button buttonSaveIco;
        private System.Windows.Forms.Label labelStep2;
        private UriTextBox textBoxHrefIco;
        private UriTextBox textBoxHrefPng;
        private System.Windows.Forms.Label labelStep4;
        private System.Windows.Forms.Button buttonSavePng;
        private System.Windows.Forms.Label labelStep3;

    }
}
