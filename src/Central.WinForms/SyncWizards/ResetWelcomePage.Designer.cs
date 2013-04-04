namespace ZeroInstall.Central.WinForms.SyncWizards
{
    partial class ResetWelcomePage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResetWelcomePage));
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.buttonChangeCryptoKey = new System.Windows.Forms.Button();
            this.buttonResetServer = new System.Windows.Forms.Button();
            this.buttonResetClient = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.Name = "labelTitle";
            // 
            // labelInfo
            // 
            resources.ApplyResources(this.labelInfo, "labelInfo");
            this.labelInfo.Name = "labelInfo";
            // 
            // buttonChangeCryptoKey
            // 
            resources.ApplyResources(this.buttonChangeCryptoKey, "buttonChangeCryptoKey");
            this.buttonChangeCryptoKey.Name = "buttonChangeCryptoKey";
            this.buttonChangeCryptoKey.UseVisualStyleBackColor = true;
            this.buttonChangeCryptoKey.Click += new System.EventHandler(this.buttonChangeCryptoKey_Click);
            // 
            // buttonResetServer
            // 
            resources.ApplyResources(this.buttonResetServer, "buttonResetServer");
            this.buttonResetServer.Name = "buttonResetServer";
            this.buttonResetServer.UseVisualStyleBackColor = true;
            this.buttonResetServer.Click += new System.EventHandler(this.buttonResetServer_Click);
            // 
            // buttonResetClient
            // 
            resources.ApplyResources(this.buttonResetClient, "buttonResetClient");
            this.buttonResetClient.Name = "buttonResetClient";
            this.buttonResetClient.UseVisualStyleBackColor = true;
            this.buttonResetClient.Click += new System.EventHandler(this.buttonResetClient_Click);
            // 
            // ResetWelcomePage
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonResetClient);
            this.Controls.Add(this.buttonResetServer);
            this.Controls.Add(this.buttonChangeCryptoKey);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelTitle);
            this.Name = "ResetWelcomePage";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Button buttonChangeCryptoKey;
        private System.Windows.Forms.Button buttonResetServer;
        private System.Windows.Forms.Button buttonResetClient;
    }
}
