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
            this.buttonUpdateAll = new Common.Controls.SplitButton();
            this.menuUpdateAll = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonUpdateAllClean = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSync = new Common.Controls.SplitButton();
            this.menuSync = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.butonSyncSetup = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSyncTroubleshoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tileListMyApps = new ZeroInstall.Central.WinForms.AppTileList();
            this.tabPageCatalog = new System.Windows.Forms.TabPage();
            this.labelLoadingCatalog = new System.Windows.Forms.Label();
            this.labelLastCatalogError = new System.Windows.Forms.Label();
            this.buttonAddOtherApp = new System.Windows.Forms.Button();
            this.buttonRefreshCatalog = new System.Windows.Forms.Button();
            this.tileListCatalog = new ZeroInstall.Central.WinForms.AppTileList();
            this.buttonOptions = new Common.Controls.SplitButton();
            this.menuOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonOptionsAdvanced = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCacheManagement = new System.Windows.Forms.Button();
            this.buttonHelp = new Common.Controls.SplitButton();
            this.menuHelp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonIntro = new System.Windows.Forms.ToolStripMenuItem();
            this.labelVersion = new System.Windows.Forms.Label();
            this.selfUpdateWorker = new System.ComponentModel.BackgroundWorker();
            this.catalogWorker = new System.ComponentModel.BackgroundWorker();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.appListWorker = new System.ComponentModel.BackgroundWorker();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.rootTable = new System.Windows.Forms.TableLayoutPanel();
            this.tabControlApps.SuspendLayout();
            this.tabPageAppList.SuspendLayout();
            this.menuUpdateAll.SuspendLayout();
            this.menuSync.SuspendLayout();
            this.tabPageCatalog.SuspendLayout();
            this.menuOptions.SuspendLayout();
            this.menuHelp.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.rootTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControlApps
            // 
            this.tabControlApps.Controls.Add(this.tabPageAppList);
            this.tabControlApps.Controls.Add(this.tabPageCatalog);
            resources.ApplyResources(this.tabControlApps, "tabControlApps");
            this.tabControlApps.Name = "tabControlApps";
            this.tabControlApps.SelectedIndex = 0;
            this.tabControlApps.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tabControlApps_KeyPress);
            // 
            // tabPageAppList
            // 
            this.tabPageAppList.Controls.Add(this.buttonUpdateAll);
            this.tabPageAppList.Controls.Add(this.buttonSync);
            this.tabPageAppList.Controls.Add(this.tileListMyApps);
            resources.ApplyResources(this.tabPageAppList, "tabPageAppList");
            this.tabPageAppList.Name = "tabPageAppList";
            this.tabPageAppList.UseVisualStyleBackColor = true;
            // 
            // buttonUpdateAll
            // 
            resources.ApplyResources(this.buttonUpdateAll, "buttonUpdateAll");
            this.buttonUpdateAll.ContextMenuStrip = this.menuUpdateAll;
            this.buttonUpdateAll.Name = "buttonUpdateAll";
            this.buttonUpdateAll.ShowSplit = true;
            this.buttonUpdateAll.SplitMenuStrip = this.menuUpdateAll;
            this.buttonUpdateAll.UseVisualStyleBackColor = true;
            this.buttonUpdateAll.Click += new System.EventHandler(this.buttonUpdateAll_Click);
            // 
            // menuUpdateAll
            // 
            this.menuUpdateAll.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonUpdateAllClean});
            this.menuUpdateAll.Name = "contextMenuUpdateAll";
            resources.ApplyResources(this.menuUpdateAll, "menuUpdateAll");
            // 
            // buttonUpdateAllClean
            // 
            this.buttonUpdateAllClean.Name = "buttonUpdateAllClean";
            resources.ApplyResources(this.buttonUpdateAllClean, "buttonUpdateAllClean");
            this.buttonUpdateAllClean.Click += new System.EventHandler(this.buttonUpdateAllClean_Click);
            // 
            // buttonSync
            // 
            resources.ApplyResources(this.buttonSync, "buttonSync");
            this.buttonSync.ContextMenuStrip = this.menuSync;
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.ShowSplit = true;
            this.buttonSync.SplitMenuStrip = this.menuSync;
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.buttonSync_Click);
            // 
            // menuSync
            // 
            this.menuSync.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.butonSyncSetup,
            this.buttonSyncTroubleshoot});
            this.menuSync.Name = "menuSync";
            resources.ApplyResources(this.menuSync, "menuSync");
            // 
            // butonSyncSetup
            // 
            this.butonSyncSetup.Name = "butonSyncSetup";
            resources.ApplyResources(this.butonSyncSetup, "butonSyncSetup");
            this.butonSyncSetup.Click += new System.EventHandler(this.butonSyncSetup_Click);
            // 
            // buttonSyncTroubleshoot
            // 
            this.buttonSyncTroubleshoot.Name = "buttonSyncTroubleshoot";
            resources.ApplyResources(this.buttonSyncTroubleshoot, "buttonSyncTroubleshoot");
            this.buttonSyncTroubleshoot.Click += new System.EventHandler(this.buttonSyncTroubleshoot_Click);
            // 
            // tileListMyApps
            // 
            resources.ApplyResources(this.tileListMyApps, "tileListMyApps");
            this.tileListMyApps.Name = "tileListMyApps";
            // 
            // tabPageCatalog
            // 
            this.tabPageCatalog.Controls.Add(this.labelLoadingCatalog);
            this.tabPageCatalog.Controls.Add(this.labelLastCatalogError);
            this.tabPageCatalog.Controls.Add(this.buttonAddOtherApp);
            this.tabPageCatalog.Controls.Add(this.buttonRefreshCatalog);
            this.tabPageCatalog.Controls.Add(this.tileListCatalog);
            resources.ApplyResources(this.tabPageCatalog, "tabPageCatalog");
            this.tabPageCatalog.Name = "tabPageCatalog";
            this.tabPageCatalog.UseVisualStyleBackColor = true;
            // 
            // labelLoadingCatalog
            // 
            resources.ApplyResources(this.labelLoadingCatalog, "labelLoadingCatalog");
            this.labelLoadingCatalog.Name = "labelLoadingCatalog";
            // 
            // labelLastCatalogError
            // 
            resources.ApplyResources(this.labelLastCatalogError, "labelLastCatalogError");
            this.labelLastCatalogError.AutoEllipsis = true;
            this.labelLastCatalogError.ForeColor = System.Drawing.Color.Red;
            this.labelLastCatalogError.Name = "labelLastCatalogError";
            // 
            // buttonAddOtherApp
            // 
            resources.ApplyResources(this.buttonAddOtherApp, "buttonAddOtherApp");
            this.buttonAddOtherApp.Name = "buttonAddOtherApp";
            this.buttonAddOtherApp.UseVisualStyleBackColor = true;
            this.buttonAddOtherApp.Click += new System.EventHandler(this.buttonAddOtherApp_Click);
            // 
            // buttonRefreshCatalog
            // 
            resources.ApplyResources(this.buttonRefreshCatalog, "buttonRefreshCatalog");
            this.buttonRefreshCatalog.Name = "buttonRefreshCatalog";
            this.buttonRefreshCatalog.UseVisualStyleBackColor = true;
            this.buttonRefreshCatalog.Click += new System.EventHandler(this.buttonRefreshCatalog_Click);
            // 
            // tileListCatalog
            // 
            resources.ApplyResources(this.tileListCatalog, "tileListCatalog");
            this.tileListCatalog.Name = "tileListCatalog";
            // 
            // buttonOptions
            // 
            resources.ApplyResources(this.buttonOptions, "buttonOptions");
            this.buttonOptions.ContextMenuStrip = this.menuOptions;
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.ShowSplit = true;
            this.buttonOptions.SplitMenuStrip = this.menuOptions;
            this.buttonOptions.UseVisualStyleBackColor = true;
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
            // 
            // menuOptions
            // 
            this.menuOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonOptionsAdvanced});
            this.menuOptions.Name = "menuOptions";
            resources.ApplyResources(this.menuOptions, "menuOptions");
            // 
            // buttonOptionsAdvanced
            // 
            this.buttonOptionsAdvanced.Name = "buttonOptionsAdvanced";
            resources.ApplyResources(this.buttonOptionsAdvanced, "buttonOptionsAdvanced");
            this.buttonOptionsAdvanced.Click += new System.EventHandler(this.buttonOptionsAdvanced_Click);
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
            this.buttonHelp.ContextMenuStrip = this.menuHelp;
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.ShowSplit = true;
            this.buttonHelp.SplitMenuStrip = this.menuHelp;
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonIntro});
            this.menuHelp.Name = "menuHelp";
            resources.ApplyResources(this.menuHelp, "menuHelp");
            // 
            // buttonIntro
            // 
            this.buttonIntro.Name = "buttonIntro";
            resources.ApplyResources(this.buttonIntro, "buttonIntro");
            this.buttonIntro.Click += new System.EventHandler(this.buttonIntro_Click);
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
            // appListWorker
            // 
            this.appListWorker.WorkerSupportsCancellation = true;
            this.appListWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.appListWorker_DoWork);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.buttonCacheManagement);
            this.panelBottom.Controls.Add(this.buttonHelp);
            this.panelBottom.Controls.Add(this.buttonOptions);
            this.panelBottom.Controls.Add(this.labelVersion);
            resources.ApplyResources(this.panelBottom, "panelBottom");
            this.panelBottom.Name = "panelBottom";
            // 
            // rootTable
            // 
            resources.ApplyResources(this.rootTable, "rootTable");
            this.rootTable.Controls.Add(this.pictureBoxLogo, 0, 0);
            this.rootTable.Controls.Add(this.panelBottom, 0, 2);
            this.rootTable.Controls.Add(this.tabControlApps, 0, 1);
            this.rootTable.Name = "rootTable";
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rootTable);
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.tabControlApps.ResumeLayout(false);
            this.tabPageAppList.ResumeLayout(false);
            this.tabPageAppList.PerformLayout();
            this.menuUpdateAll.ResumeLayout(false);
            this.menuSync.ResumeLayout(false);
            this.tabPageCatalog.ResumeLayout(false);
            this.tabPageCatalog.PerformLayout();
            this.menuOptions.ResumeLayout(false);
            this.menuHelp.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.rootTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlApps;
        private System.Windows.Forms.TabPage tabPageAppList;
        private System.Windows.Forms.TabPage tabPageCatalog;
        private Common.Controls.SplitButton buttonHelp;
        private System.Windows.Forms.Button buttonCacheManagement;
        private System.Windows.Forms.Label labelVersion;
        private Common.Controls.SplitButton buttonOptions;
        private System.ComponentModel.BackgroundWorker selfUpdateWorker;
        private AppTileList tileListMyApps;
        private AppTileList tileListCatalog;
        private System.ComponentModel.BackgroundWorker catalogWorker;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.Button buttonRefreshCatalog;
        private System.ComponentModel.BackgroundWorker appListWorker;
        private System.Windows.Forms.Button buttonAddOtherApp;
        private Common.Controls.SplitButton buttonSync;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.TableLayoutPanel rootTable;
        private System.Windows.Forms.Label labelLastCatalogError;
        private System.Windows.Forms.Label labelLoadingCatalog;
        private Common.Controls.SplitButton buttonUpdateAll;
        private System.Windows.Forms.ContextMenuStrip menuUpdateAll;
        private System.Windows.Forms.ToolStripMenuItem buttonUpdateAllClean;
        private System.Windows.Forms.ContextMenuStrip menuOptions;
        private System.Windows.Forms.ContextMenuStrip menuHelp;
        private System.Windows.Forms.ToolStripMenuItem buttonOptionsAdvanced;
        private System.Windows.Forms.ToolStripMenuItem buttonIntro;
        private System.Windows.Forms.ContextMenuStrip menuSync;
        private System.Windows.Forms.ToolStripMenuItem butonSyncSetup;
        private System.Windows.Forms.ToolStripMenuItem buttonSyncTroubleshoot;

    }
}

