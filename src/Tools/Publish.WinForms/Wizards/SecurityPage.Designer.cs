using NanoByte.Common.Controls;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class SecurityPage
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
            this.buttonNext = new System.Windows.Forms.Button();
            this.textBoxUri = new NanoByte.Common.Controls.UriTextBox();
            this.labelUri = new System.Windows.Forms.Label();
            this.labelSignature = new System.Windows.Forms.Label();
            this.comboBoxKeys = new System.Windows.Forms.ComboBox();
            this.buttonNewKey = new System.Windows.Forms.Button();
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
            this.labelTitle.Text = "Security";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonNext
            // 
            this.buttonNext.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonNext.Location = new System.Drawing.Point(315, 238);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(120, 35);
            this.buttonNext.TabIndex = 6;
            this.buttonNext.Text = "&Next >";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // textBoxUri
            // 
            this.textBoxUri.AllowDrop = true;
            this.textBoxUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUri.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.textBoxUri.HintText = "HTTP URI";
            this.textBoxUri.HttpOnly = true;
            this.textBoxUri.Location = new System.Drawing.Point(39, 201);
            this.textBoxUri.Name = "textBoxUri";
            this.textBoxUri.Size = new System.Drawing.Size(396, 26);
            this.textBoxUri.TabIndex = 5;
            // 
            // labelUri
            // 
            this.labelUri.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelUri.Location = new System.Drawing.Point(35, 153);
            this.labelUri.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelUri.Name = "labelUri";
            this.labelUri.Size = new System.Drawing.Size(400, 45);
            this.labelUri.TabIndex = 4;
            this.labelUri.Text = "Where will you upload the feed? This address will be stored within the feed itsel" +
    "f!";
            // 
            // labelSignature
            // 
            this.labelSignature.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelSignature.Location = new System.Drawing.Point(35, 65);
            this.labelSignature.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSignature.Name = "labelSignature";
            this.labelSignature.Size = new System.Drawing.Size(400, 45);
            this.labelSignature.TabIndex = 1;
            this.labelSignature.Text = "Zero Install protects feeds with GnuPG signatures. Please select a private key to" +
    " sign your feed:";
            // 
            // comboBoxKeys
            // 
            this.comboBoxKeys.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxKeys.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.comboBoxKeys.FormattingEnabled = true;
            this.comboBoxKeys.Location = new System.Drawing.Point(39, 113);
            this.comboBoxKeys.Name = "comboBoxKeys";
            this.comboBoxKeys.Size = new System.Drawing.Size(270, 28);
            this.comboBoxKeys.TabIndex = 2;
            this.comboBoxKeys.SelectedIndexChanged += new System.EventHandler(this.comboBoxKeys_SelectedIndexChanged);
            // 
            // buttonNewKey
            // 
            this.buttonNewKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonNewKey.Location = new System.Drawing.Point(315, 113);
            this.buttonNewKey.Name = "buttonNewKey";
            this.buttonNewKey.Size = new System.Drawing.Size(120, 28);
            this.buttonNewKey.TabIndex = 3;
            this.buttonNewKey.Text = "&New key";
            this.buttonNewKey.UseVisualStyleBackColor = true;
            this.buttonNewKey.Click += new System.EventHandler(this.buttonNewKey_Click);
            // 
            // SecurityPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonNewKey);
            this.Controls.Add(this.comboBoxKeys);
            this.Controls.Add(this.labelSignature);
            this.Controls.Add(this.labelUri);
            this.Controls.Add(this.textBoxUri);
            this.Controls.Add(this.buttonNext);
            this.Controls.Add(this.labelTitle);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "SecurityPage";
            this.Size = new System.Drawing.Size(470, 300);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonNext;
        private UriTextBox textBoxUri;
        private System.Windows.Forms.Label labelUri;
        private System.Windows.Forms.Label labelSignature;
        private System.Windows.Forms.ComboBox comboBoxKeys;
        private System.Windows.Forms.Button buttonNewKey;
    }
}
