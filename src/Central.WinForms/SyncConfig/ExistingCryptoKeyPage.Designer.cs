namespace ZeroInstall.Central.WinForms.SyncConfig
{
    partial class ExistingCryptoKeyPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExistingCryptoKeyPage));
            this.buttonContinue = new System.Windows.Forms.Button();
            this.textBoxCryptoKey = new System.Windows.Forms.TextBox();
            this.labelCryptoKey = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.keyCheckWorker = new System.ComponentModel.BackgroundWorker();
            this.buttonReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonContinue
            // 
            resources.ApplyResources(this.buttonContinue, "buttonContinue");
            this.buttonContinue.Name = "buttonContinue";
            this.buttonContinue.UseVisualStyleBackColor = true;
            this.buttonContinue.Click += new System.EventHandler(this.buttonContinue_Click);
            // 
            // textBoxCryptoKey
            // 
            resources.ApplyResources(this.textBoxCryptoKey, "textBoxCryptoKey");
            this.textBoxCryptoKey.Name = "textBoxCryptoKey";
            this.textBoxCryptoKey.TextChanged += new System.EventHandler(this.textBoxCryptoKey_TextChanged);
            // 
            // labelCryptoKey
            // 
            resources.ApplyResources(this.labelCryptoKey, "labelCryptoKey");
            this.labelCryptoKey.Name = "labelCryptoKey";
            // 
            // labelInfo
            // 
            resources.ApplyResources(this.labelInfo, "labelInfo");
            this.labelInfo.Name = "labelInfo";
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.Name = "labelTitle";
            // 
            // keyCheckWorker
            // 
            this.keyCheckWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.keyCheckWorker_DoWork);
            this.keyCheckWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.keyCheckWorker_RunWorkerCompleted);
            // 
            // buttonReset
            // 
            resources.ApplyResources(this.buttonReset, "buttonReset");
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // ExistingCryptoKeyPage
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonContinue);
            this.Controls.Add(this.textBoxCryptoKey);
            this.Controls.Add(this.labelCryptoKey);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelTitle);
            this.Name = "ExistingCryptoKeyPage";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonContinue;
        private System.Windows.Forms.TextBox textBoxCryptoKey;
        private System.Windows.Forms.Label labelCryptoKey;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelTitle;
        private System.ComponentModel.BackgroundWorker keyCheckWorker;
        private System.Windows.Forms.Button buttonReset;
    }
}
