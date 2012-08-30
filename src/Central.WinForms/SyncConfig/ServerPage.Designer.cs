namespace ZeroInstall.Central.WinForms.SyncConfig
{
    partial class ServerPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerPage));
            this.labelQuestion = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.buttonCustomServer = new System.Windows.Forms.Button();
            this.buttonOfficalServer = new System.Windows.Forms.Button();
            this.textBoxCustomServer = new Common.Controls.UriTextBox();
            this.serverCheckWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // labelQuestion
            // 
            resources.ApplyResources(this.labelQuestion, "labelQuestion");
            this.labelQuestion.Name = "labelQuestion";
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
            // buttonCustomServer
            // 
            resources.ApplyResources(this.buttonCustomServer, "buttonCustomServer");
            this.buttonCustomServer.Name = "buttonCustomServer";
            this.buttonCustomServer.UseVisualStyleBackColor = true;
            this.buttonCustomServer.Click += new System.EventHandler(this.buttonCustomServer_Click);
            // 
            // buttonOfficalServer
            // 
            resources.ApplyResources(this.buttonOfficalServer, "buttonOfficalServer");
            this.buttonOfficalServer.Name = "buttonOfficalServer";
            this.buttonOfficalServer.UseVisualStyleBackColor = true;
            this.buttonOfficalServer.Click += new System.EventHandler(this.buttonOfficalServer_Click);
            // 
            // textBoxCustomServer
            // 
            resources.ApplyResources(this.textBoxCustomServer, "textBoxCustomServer");
            this.textBoxCustomServer.AllowDrop = true;
            this.textBoxCustomServer.ForeColor = System.Drawing.Color.Red;
            this.textBoxCustomServer.HttpOnly = true;
            this.textBoxCustomServer.Name = "textBoxCustomServer";
            this.textBoxCustomServer.TextChanged += new System.EventHandler(this.textBoxCustomServer_TextChanged);
            // 
            // serverCheckWorker
            // 
            this.serverCheckWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.serverCheckWorker_DoWork);
            this.serverCheckWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.serverCheckWorker_RunWorkerCompleted);
            // 
            // ServerPage
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.textBoxCustomServer);
            this.Controls.Add(this.buttonCustomServer);
            this.Controls.Add(this.buttonOfficalServer);
            this.Controls.Add(this.labelQuestion);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelTitle);
            this.Name = "ServerPage";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelQuestion;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonCustomServer;
        private System.Windows.Forms.Button buttonOfficalServer;
        private Common.Controls.UriTextBox textBoxCustomServer;
        private System.ComponentModel.BackgroundWorker serverCheckWorker;
    }
}
