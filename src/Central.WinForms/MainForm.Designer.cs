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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControlApps = new System.Windows.Forms.TabControl();
            this.tabPageAppList = new System.Windows.Forms.TabPage();
            this.buttonSync = new System.Windows.Forms.Button();
            this.appList = new ZeroInstall.Central.WinForms.AppTileList();
            this.tabPageCatalog = new System.Windows.Forms.TabPage();
            this.buttonRefreshCatalog = new System.Windows.Forms.Button();
            this.buttonAddOtherApp = new System.Windows.Forms.Button();
            this.catalogList = new ZeroInstall.Central.WinForms.AppTileList();
            this.buttonOptions = new System.Windows.Forms.Button();
            this.buttonCacheManagement = new System.Windows.Forms.Button();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.selfUpdateWorker = new System.ComponentModel.BackgroundWorker();
            this.catalogWorker = new System.ComponentModel.BackgroundWorker();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.appListWatcher = new System.IO.FileSystemWatcher();
            this.appListWorker = new System.ComponentModel.BackgroundWorker();
            this.appListTimer = new System.Windows.Forms.Timer(this.components);
            this.tabControlApps.SuspendLayout();
            this.tabPageAppList.SuspendLayout();
            this.tabPageCatalog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.appListWatcher)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControlApps
            // 
            resources.ApplyResources(this.tabControlApps, "tabControlApps");
            this.tabControlApps.Controls.Add(this.tabPageAppList);
            this.tabControlApps.Controls.Add(this.tabPageCatalog);
            this.tabControlApps.Name = "tabControlApps";
            this.tabControlApps.SelectedIndex = 0;
            // 
            // tabPageAppList
            // 
            this.tabPageAppList.Controls.Add(this.buttonSync);
            this.tabPageAppList.Controls.Add(this.appList);
            resources.ApplyResources(this.tabPageAppList, "tabPageAppList");
            this.tabPageAppList.Name = "tabPageAppList";
            this.tabPageAppList.UseVisualStyleBackColor = true;
            // 
            // buttonSync
            // 
            resources.ApplyResources(this.buttonSync, "buttonSync");
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.buttonSync_Click);
            // 
            // appList
            // 
            resources.ApplyResources(this.appList, "appList");
            this.appList.Name = "appList";
            // 
            // tabPageCatalog
            // 
            this.tabPageCatalog.Controls.Add(this.buttonRefreshCatalog);
            this.tabPageCatalog.Controls.Add(this.buttonAddOtherApp);
            this.tabPageCatalog.Controls.Add(this.catalogList);
            resources.ApplyResources(this.tabPageCatalog, "tabPageCatalog");
            this.tabPageCatalog.Name = "tabPageCatalog";
            this.tabPageCatalog.UseVisualStyleBackColor = true;
            // 
            // buttonRefreshCatalog
            // 
            resources.ApplyResources(this.buttonRefreshCatalog, "buttonRefreshCatalog");
            this.buttonRefreshCatalog.Name = "buttonRefreshCatalog";
            this.buttonRefreshCatalog.UseVisualStyleBackColor = true;
            this.buttonRefreshCatalog.Click += new System.EventHandler(this.buttonRefreshCatalog_Click);
            // 
            // buttonAddOtherApp
            // 
            resources.ApplyResources(this.buttonAddOtherApp, "buttonAddOtherApp");
            this.buttonAddOtherApp.Name = "buttonAddOtherApp";
            this.buttonAddOtherApp.UseVisualStyleBackColor = true;
            this.buttonAddOtherApp.Click += new System.EventHandler(this.buttonAddOtherApp_Click);
            // 
            // catalogList
            // 
            resources.ApplyResources(this.catalogList, "catalogList");
            this.catalogList.Name = "catalogList";
            // 
            // buttonOptions
            // 
            resources.ApplyResources(this.buttonOptions, "buttonOptions");
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.UseVisualStyleBackColor = true;
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
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
            // appListWatcher
            // 
            this.appListWatcher.EnableRaisingEvents = true;
            this.appListWatcher.NotifyFilter = ((System.IO.NotifyFilters)((((System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.Size) 
            | System.IO.NotifyFilters.LastWrite) 
            | System.IO.NotifyFilters.CreationTime)));
            this.appListWatcher.SynchronizingObject = this;
            this.appListWatcher.Changed += new System.IO.FileSystemEventHandler(this.appListWatcher_Changed);
            // 
            // appListWorker
            // 
            this.appListWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.feedLoadWorker_DoWork);
            // 
            // appListTimer
            // 
            this.appListTimer.Enabled = true;
            this.appListTimer.Interval = 2500;
            this.appListTimer.Tick += new System.EventHandler(this.appListTimer_Tick);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.pictureBoxLogo);
            this.Controls.Add(this.buttonOptions);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonCacheManagement);
            this.Controls.Add(this.buttonHelp);
            this.Controls.Add(this.tabControlApps);
            this.Name = "MainForm";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.tabControlApps.ResumeLayout(false);
            this.tabPageAppList.ResumeLayout(false);
            this.tabPageCatalog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.appListWatcher)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlApps;
        private System.Windows.Forms.TabPage tabPageAppList;
        private System.Windows.Forms.TabPage tabPageCatalog;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.Button buttonAddOtherApp;
        private System.Windows.Forms.Button buttonCacheManagement;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button buttonOptions;
        private System.ComponentModel.BackgroundWorker selfUpdateWorker;
        private AppTileList appList;
        private AppTileList catalogList;
        private System.ComponentModel.BackgroundWorker catalogWorker;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.Button buttonRefreshCatalog;
        private System.IO.FileSystemWatcher appListWatcher;
        private System.Windows.Forms.Button buttonSync;
        private System.ComponentModel.BackgroundWorker appListWorker;
        private System.Windows.Forms.Timer appListTimer;

    }
}

