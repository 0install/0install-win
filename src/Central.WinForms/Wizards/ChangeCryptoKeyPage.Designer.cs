namespace ZeroInstall.Central.WinForms.Wizards
{
    partial class ChangeCryptoKeyPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangeCryptoKeyPage));
            this.buttonChange = new System.Windows.Forms.Button();
            this.textBoxCryptoKey = new System.Windows.Forms.TextBox();
            this.labelCryptoKey = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.resetWorker = new System.ComponentModel.BackgroundWorker();
            this.labelInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonChange
            // 
            resources.ApplyResources(this.buttonChange, "buttonChange");
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.UseVisualStyleBackColor = true;
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);
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
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.Name = "labelTitle";
            // 
            // resetWorker
            // 
            this.resetWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.resetWorker_DoWork);
            this.resetWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.resetWorker_RunWorkerCompleted);
            // 
            // labelInfo
            // 
            resources.ApplyResources(this.labelInfo, "labelInfo");
            this.labelInfo.Name = "labelInfo";
            // 
            // ChangeCryptoKeyPage
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonChange);
            this.Controls.Add(this.textBoxCryptoKey);
            this.Controls.Add(this.labelCryptoKey);
            this.Controls.Add(this.labelTitle);
            this.Name = "ChangeCryptoKeyPage";
            this.Controls.SetChildIndex(this.labelTitle, 0);
            this.Controls.SetChildIndex(this.labelCryptoKey, 0);
            this.Controls.SetChildIndex(this.textBoxCryptoKey, 0);
            this.Controls.SetChildIndex(this.buttonChange, 0);
            this.Controls.SetChildIndex(this.labelInfo, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonChange;
        private System.Windows.Forms.TextBox textBoxCryptoKey;
        private System.Windows.Forms.Label labelCryptoKey;
        private System.Windows.Forms.Label labelTitle;
        private System.ComponentModel.BackgroundWorker resetWorker;
        private System.Windows.Forms.Label labelInfo;
    }
}
