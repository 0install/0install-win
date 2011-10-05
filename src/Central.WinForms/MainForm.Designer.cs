namespace ZeroInstall.Central.WinForms
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControlApps = new System.Windows.Forms.TabControl();
            this.tabPageMyApps = new System.Windows.Forms.TabPage();
            this.myAppsList = new ZeroInstall.Central.WinForms.AppTileList();
            this.tabPageNewApps = new System.Windows.Forms.TabPage();
            this.catalogList = new ZeroInstall.Central.WinForms.AppTileList();
            this.buttonLaunchInterface = new System.Windows.Forms.Button();
            this.buttonConfiguration = new System.Windows.Forms.Button();
            this.buttonCacheManagement = new System.Windows.Forms.Button();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.selfUpdateWorker = new System.ComponentModel.BackgroundWorker();
            this.catalogWorker = new System.ComponentModel.BackgroundWorker();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.tabControlApps.SuspendLayout();
            this.tabPageMyApps.SuspendLayout();
            this.tabPageNewApps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlApps
            // 
            resources.ApplyResources(this.tabControlApps, "tabControlApps");
            this.tabControlApps.Controls.Add(this.tabPageMyApps);
            this.tabControlApps.Controls.Add(this.tabPageNewApps);
            this.tabControlApps.Name = "tabControlApps";
            this.tabControlApps.SelectedIndex = 0;
            // 
            // tabPageMyApps
            // 
            this.tabPageMyApps.Controls.Add(this.myAppsList);
            resources.ApplyResources(this.tabPageMyApps, "tabPageMyApps");
            this.tabPageMyApps.Name = "tabPageMyApps";
            this.tabPageMyApps.UseVisualStyleBackColor = true;
            // 
            // myAppsList
            // 
            resources.ApplyResources(this.myAppsList, "myAppsList");
            this.myAppsList.Name = "myAppsList";
            // 
            // tabPageNewApps
            // 
            this.tabPageNewApps.Controls.Add(this.catalogList);
            this.tabPageNewApps.Controls.Add(this.buttonLaunchInterface);
            resources.ApplyResources(this.tabPageNewApps, "tabPageNewApps");
            this.tabPageNewApps.Name = "tabPageNewApps";
            this.tabPageNewApps.UseVisualStyleBackColor = true;
            // 
            // catalogList
            // 
            resources.ApplyResources(this.catalogList, "catalogList");
            this.catalogList.Name = "catalogList";
            // 
            // buttonLaunchInterface
            // 
            resources.ApplyResources(this.buttonLaunchInterface, "buttonLaunchInterface");
            this.buttonLaunchInterface.Name = "buttonLaunchInterface";
            this.buttonLaunchInterface.UseVisualStyleBackColor = true;
            this.buttonLaunchInterface.Click += new System.EventHandler(this.buttonLaunchInterface_Click);
            // 
            // buttonConfiguration
            // 
            resources.ApplyResources(this.buttonConfiguration, "buttonConfiguration");
            this.buttonConfiguration.Name = "buttonConfiguration";
            this.buttonConfiguration.UseVisualStyleBackColor = true;
            this.buttonConfiguration.Click += new System.EventHandler(this.buttonConfiguration_Click);
            // 
            // buttonCacheManagement
            // 
            resources.ApplyResources(this.buttonCacheManagement, "buttonCacheManagement");
            this.buttonCacheManagement.Name = "buttonCacheManagement";
            this.buttonCacheManagement.UseVisualStyleBackColor = true;
            this.buttonCacheManagement.Click += new System.EventHandler(this.buttonCacheManagement_Click);
            // 
            // buttonHelp
            // 
            resources.ApplyResources(this.buttonHelp, "buttonHelp");
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // labelVersion
            // 
            resources.ApplyResources(this.labelVersion, "labelVersion");
            this.labelVersion.Name = "labelVersion";
            // 
            // selfUpdateWorker
            // 
            this.selfUpdateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.selfUpdateWorker_DoWork);
            this.selfUpdateWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.selfUpdateWorker_RunWorkerCompleted);
            // 
            // catalogWorker
            // 
            this.catalogWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.catalogWorker_DoWork);
            this.catalogWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.catalogWorker_RunWorkerCompleted);
            // 
            // pictureBoxLogo
            // 
            resources.ApplyResources(this.pictureBoxLogo, "pictureBoxLogo");
            this.pictureBoxLogo.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.Logo;
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.TabStop = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.BackColor = System.Drawing.Color.White;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.pictureBoxLogo);
            this.Controls.Add(this.buttonConfiguration);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonCacheManagement);
            this.Controls.Add(this.buttonHelp);
            this.Controls.Add(this.tabControlApps);
            this.Name = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.tabControlApps.ResumeLayout(false);
            this.tabPageMyApps.ResumeLayout(false);
            this.tabPageNewApps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlApps;
        private System.Windows.Forms.TabPage tabPageMyApps;
        private System.Windows.Forms.TabPage tabPageNewApps;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.Button buttonLaunchInterface;
        private System.Windows.Forms.Button buttonCacheManagement;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button buttonConfiguration;
        private System.ComponentModel.BackgroundWorker selfUpdateWorker;
        private AppTileList myAppsList;
        private AppTileList catalogList;
        private System.ComponentModel.BackgroundWorker catalogWorker;
        private System.Windows.Forms.PictureBox pictureBoxLogo;

    }
}

