namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class IconManagementControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IconManagementControl));
            this.labelIconUrl = new System.Windows.Forms.Label();
            this.comboBoxIconType = new System.Windows.Forms.ComboBox();
            this.lableIconUrlError = new System.Windows.Forms.Label();
            this.labelIconMimeType = new System.Windows.Forms.Label();
            this.buttonPreview = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.listBoxIconsUrls = new System.Windows.Forms.ListBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.uriTextBoxIconUrl = new Common.Controls.UriTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // labelIconUrl
            // 
            resources.ApplyResources(this.labelIconUrl, "labelIconUrl");
            this.labelIconUrl.Name = "labelIconUrl";
            // 
            // comboBoxIconType
            // 
            resources.ApplyResources(this.comboBoxIconType, "comboBoxIconType");
            this.comboBoxIconType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxIconType.FormattingEnabled = true;
            this.comboBoxIconType.Name = "comboBoxIconType";
            // 
            // lableIconUrlError
            // 
            resources.ApplyResources(this.lableIconUrlError, "lableIconUrlError");
            this.lableIconUrlError.ForeColor = System.Drawing.Color.Red;
            this.lableIconUrlError.Name = "lableIconUrlError";
            // 
            // labelIconMimeType
            // 
            resources.ApplyResources(this.labelIconMimeType, "labelIconMimeType");
            this.labelIconMimeType.Name = "labelIconMimeType";
            // 
            // buttonPreview
            // 
            resources.ApplyResources(this.buttonPreview, "buttonPreview");
            this.buttonPreview.Name = "buttonPreview";
            this.buttonPreview.UseVisualStyleBackColor = true;
            this.buttonPreview.Click += new System.EventHandler(this.ButtonPreviewClick);
            // 
            // buttonRemove
            // 
            resources.ApplyResources(this.buttonRemove, "buttonRemove");
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.ButtonRemoveClick);
            // 
            // buttonAdd
            // 
            resources.ApplyResources(this.buttonAdd, "buttonAdd");
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.ButtonAddClick);
            // 
            // listBoxIconsUrls
            // 
            resources.ApplyResources(this.listBoxIconsUrls, "listBoxIconsUrls");
            this.listBoxIconsUrls.FormattingEnabled = true;
            this.listBoxIconsUrls.Name = "listBoxIconsUrls";
            this.listBoxIconsUrls.SelectedIndexChanged += new System.EventHandler(this.ListIconsUrlsSelectedIndexChanged);
            // 
            // pictureBoxPreview
            // 
            resources.ApplyResources(this.pictureBoxPreview, "pictureBoxPreview");
            this.pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.TabStop = false;
            // 
            // uriTextBoxIconUrl
            // 
            this.uriTextBoxIconUrl.AllowDrop = true;
            resources.ApplyResources(this.uriTextBoxIconUrl, "uriTextBoxIconUrl");
            this.uriTextBoxIconUrl.ForeColor = System.Drawing.Color.Red;
            this.uriTextBoxIconUrl.Name = "uriTextBoxIconUrl";
            // 
            // IconManagementControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.uriTextBoxIconUrl);
            this.Controls.Add(this.labelIconUrl);
            this.Controls.Add(this.comboBoxIconType);
            this.Controls.Add(this.lableIconUrlError);
            this.Controls.Add(this.labelIconMimeType);
            this.Controls.Add(this.buttonPreview);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.pictureBoxPreview);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listBoxIconsUrls);
            this.Name = "IconManagementControl";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelIconUrl;
        private System.Windows.Forms.ComboBox comboBoxIconType;
        private System.Windows.Forms.Label lableIconUrlError;
        private System.Windows.Forms.Label labelIconMimeType;
        private System.Windows.Forms.Button buttonPreview;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ListBox listBoxIconsUrls;
        private Common.Controls.UriTextBox uriTextBoxIconUrl;
    }
}
