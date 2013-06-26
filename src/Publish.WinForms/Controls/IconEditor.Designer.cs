namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class IconEditor
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
            this.labelUrl = new System.Windows.Forms.Label();
            this.comboBoxMimeType = new System.Windows.Forms.ComboBox();
            this.lableStatus = new System.Windows.Forms.Label();
            this.labelMimeType = new System.Windows.Forms.Label();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.uriTextBoxUrl = new Common.Controls.UriTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // labelUrl
            // 
            this.labelUrl.AutoSize = true;
            this.labelUrl.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelUrl.Location = new System.Drawing.Point(-3, 32);
            this.labelUrl.Name = "labelUrl";
            this.labelUrl.Size = new System.Drawing.Size(56, 13);
            this.labelUrl.TabIndex = 2;
            this.labelUrl.Text = "Icon URL:";
            // 
            // comboBoxMimeType
            // 
            this.comboBoxMimeType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMimeType.FormattingEnabled = true;
            this.comboBoxMimeType.Location = new System.Drawing.Point(68, 0);
            this.comboBoxMimeType.Name = "comboBoxMimeType";
            this.comboBoxMimeType.Size = new System.Drawing.Size(82, 21);
            this.comboBoxMimeType.TabIndex = 1;
            this.comboBoxMimeType.Validating += new System.ComponentModel.CancelEventHandler(this.comboBoxMimeType_Validating);
            // 
            // lableStatus
            // 
            this.lableStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lableStatus.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lableStatus.Location = new System.Drawing.Point(126, 90);
            this.lableStatus.Name = "lableStatus";
            this.lableStatus.Size = new System.Drawing.Size(24, 60);
            this.lableStatus.TabIndex = 5;
            this.lableStatus.Tag = "";
            this.lableStatus.Text = "(Status)";
            this.lableStatus.Visible = false;
            // 
            // labelMimeType
            // 
            this.labelMimeType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMimeType.AutoSize = true;
            this.labelMimeType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelMimeType.Location = new System.Drawing.Point(-3, 3);
            this.labelMimeType.Name = "labelMimeType";
            this.labelMimeType.Size = new System.Drawing.Size(65, 13);
            this.labelMimeType.TabIndex = 0;
            this.labelMimeType.Text = "MIME Type:";
            // 
            // buttonPreview
            // 
            this.buttonPreview.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonPreview.Location = new System.Drawing.Point(0, 61);
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.Size = new System.Drawing.Size(80, 23);
            this.buttonPreview.TabIndex = 4;
            this.buttonPreview.Text = "Preview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.buttonPreview_Click);
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPreview.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBoxPreview.Location = new System.Drawing.Point(0, 90);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(120, 120);
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxPreview.TabIndex = 19;
            this.pictureBoxPreview.TabStop = false;
            // 
            // uriTextBoxUrl
            // 
            this.uriTextBoxUrl.AccessibleName = "HTTP Address";
            this.uriTextBoxUrl.AllowDrop = true;
            this.uriTextBoxUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.uriTextBoxUrl.HintText = "HTTP Address";
            this.uriTextBoxUrl.Location = new System.Drawing.Point(68, 29);
            this.uriTextBoxUrl.Name = "uriTextBoxUrl";
            this.uriTextBoxUrl.Size = new System.Drawing.Size(82, 20);
            this.uriTextBoxUrl.TabIndex = 3;
            this.uriTextBoxUrl.Validated += new System.EventHandler(this.uriTextBoxUrl_Validated);
            // 
            // IconEditor
            // 
            this.Controls.Add(this.uriTextBoxUrl);
            this.Controls.Add(this.labelUrl);
            this.Controls.Add(this.comboBoxMimeType);
            this.Controls.Add(this.lableStatus);
            this.Controls.Add(this.labelMimeType);
            this.Controls.Add(this.buttonPreview);
            this.Controls.Add(this.pictureBoxPreview);
            this.Name = "IconEditor";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelUrl;
        private System.Windows.Forms.ComboBox comboBoxMimeType;
        private System.Windows.Forms.Label lableStatus;
        private System.Windows.Forms.Label labelMimeType;
        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private Common.Controls.UriTextBox uriTextBoxUrl;
    }
}
