namespace ZeroInstall.Central.WinForms.SyncConfig
{
    partial class ResetCryptoKeyPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResetCryptoKeyPage));
            this.buttonContinue = new System.Windows.Forms.Button();
            this.textBoxCryptoKey = new System.Windows.Forms.TextBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonContinue
            // 
            resources.ApplyResources(this.buttonContinue, "buttonContinue");
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // textBoxCryptoKey
            // 
            resources.ApplyResources(this.textBoxCryptoKey, "textBoxCryptoKey");
            this.textBoxCryptoKey.Name = "textBoxCryptoKey";
            this.textBoxCryptoKey.TextChanged += new System.EventHandler(this.textBoxCryptoKey_TextChanged);
            // 
            // labelUsername
            // 
            resources.ApplyResources(this.labelUsername, "labelUsername");
            this.labelUsername.Name = "labelUsername";
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.Name = "labelTitle";
            // 
            // ResetCryptoKeyPage
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonContinue);
            this.Controls.Add(this.textBoxCryptoKey);
            this.Controls.Add(this.labelUsername);
            this.Controls.Add(this.labelTitle);
            this.Name = "ResetCryptoKeyPage";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.TextBox textBoxCryptoKey;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.Label labelTitle;
    }
}
