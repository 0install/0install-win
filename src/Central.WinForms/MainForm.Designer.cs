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
            this.buttonUpdateAll = new NanoByte.Common.Controls.DropDownButton();
            this.menuUpdateAll = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonUpdateAll2 = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonUpdateAllClean = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSync = new NanoByte.Common.Controls.DropDownButton();
            this.menuSync = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonSync2 = new System.Windows.Forms.ToolStripMenuItem();
            this.separator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonSyncSetup = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonSyncTroubleshoot = new System.Windows.Forms.ToolStripMenuItem();
            this.tileListMyApps = new ZeroInstall.Central.WinForms.AppTileList();
            this.tabPageCatalog = new System.Windows.Forms.TabPage();
            this.labelLoadingCatalog = new System.Windows.Forms.Label();
            this.labelLastCatalogError = new System.Windows.Forms.Label();
            this.buttonMoreApps = new NanoByte.Common.Controls.DropDownButton();
            this.menuMoreApps = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAddFeed = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAddCatalog = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonFeedEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonRefreshCatalog = new System.Windows.Forms.Button();
            this.tileListCatalog = new ZeroInstall.Central.WinForms.AppTileList();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.buttonOptions = new System.Windows.Forms.ToolStripButton();
            this.buttonTools = new System.Windows.Forms.ToolStripDropDownButton();
            this.buttonStoreManage = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonPortableCreator = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonCommandLine = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonHelp = new System.Windows.Forms.ToolStripDropDownButton();
            this.buttonDocumentation = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonIntro = new System.Windows.Forms.ToolStripMenuItem();
            this.labelVersion = new System.Windows.Forms.ToolStripLabel();
            this.rootTable = new System.Windows.Forms.TableLayoutPanel();
            this.labelNotificationBar = new System.Windows.Forms.Label();
            this.tabControlApps.SuspendLayout();
            this.tabPageAppList.SuspendLayout();
            this.menuUpdateAll.SuspendLayout();
            this.menuSync.SuspendLayout();
            this.tabPageCatalog.SuspendLayout();
            this.menuMoreApps.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.toolStrip.SuspendLayout();
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
            this.buttonUpdateAll.DropDownMenuStrip = this.menuUpdateAll;
            this.buttonUpdateAll.Name = "buttonUpdateAll";
            this.buttonUpdateAll.ShowSplit = true;
            this.buttonUpdateAll.UseVisualStyleBackColor = true;
            this.buttonUpdateAll.Click += new System.EventHandler(this.buttonUpdateAll_Click);
            // 
            // menuUpdateAll
            // 
            this.menuUpdateAll.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuUpdateAll.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.buttonUpdateAll2, this.buttonUpdateAllClean });
            this.menuUpdateAll.Name = "contextMenuUpdateAll";
            resources.ApplyResources(this.menuUpdateAll, "menuUpdateAll");
            // 
            // buttonUpdateAll2
            // 
            resources.ApplyResources(this.buttonUpdateAll2, "buttonUpdateAll2");
            this.buttonUpdateAll2.Name = "buttonUpdateAll2";
            this.buttonUpdateAll2.Click += new System.EventHandler(this.buttonUpdateAll_Click);
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
            this.buttonSync.DropDownMenuStrip = this.menuSync;
            this.buttonSync.Name = "buttonSync";
            this.buttonSync.ShowSplit = true;
            this.buttonSync.UseVisualStyleBackColor = true;
            this.buttonSync.Click += new System.EventHandler(this.buttonSync_Click);
            // 
            // menuSync
            // 
            this.menuSync.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.menuSync.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.buttonSync2, this.separator1, this.buttonSyncSetup, this.buttonSyncTroubleshoot });
            this.menuSync.Name = "menuSync";
            resources.ApplyResources(this.menuSync, "menuSync");
            // 
            // buttonSync2
            // 
            resources.ApplyResources(this.buttonSync2, "buttonSync2");
            this.buttonSync2.Name = "buttonSync2";
            this.buttonSync2.Click += new System.EventHandler(this.buttonSync_Click);
            // 
            // separator1
            // 
            this.separator1.Name = "separator1";
            resources.ApplyResources(this.separator1, "separator1");
            // 
            // buttonSyncSetup
            // 
            this.buttonSyncSetup.Name = "buttonSyncSetup";
            resources.ApplyResources(this.buttonSyncSetup, "buttonSyncSetup");
            this.buttonSyncSetup.Click += new System.EventHandler(this.buttonSyncSetup_Click);
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
            this.tabPageCatalog.Controls.Add(this.buttonMoreApps);
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
            // buttonMoreApps
            // 
            resources.ApplyResources(this.buttonMoreApps, "buttonMoreApps");
            this.buttonMoreApps.ContextMenuStrip = this.menuMoreApps;
            this.buttonMoreApps.DropDownMenuStrip = this.menuMoreApps;
            this.buttonMoreApps.Name = "buttonMoreApps";
            this.buttonMoreApps.UseVisualStyleBackColor = true;
            // 
            // menuMoreApps
            // 
            this.menuMoreApps.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.buttonSearch, this.buttonAddFeed, this.buttonAddCatalog, this.buttonFeedEditor });
            this.menuMoreApps.Name = "contextMenuStrip1";
            resources.ApplyResources(this.menuMoreApps, "menuMoreApps");
            // 
            // buttonSearch
            // 
            resources.ApplyResources(this.buttonSearch, "buttonSearch");
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // buttonAddFeed
            // 
            resources.ApplyResources(this.buttonAddFeed, "buttonAddFeed");
            this.buttonAddFeed.Name = "buttonAddFeed";
            this.buttonAddFeed.Click += new System.EventHandler(this.buttonAddFeed_Click);
            // 
            // buttonAddCatalog
            // 
            resources.ApplyResources(this.buttonAddCatalog, "buttonAddCatalog");
            this.buttonAddCatalog.Name = "buttonAddCatalog";
            this.buttonAddCatalog.Click += new System.EventHandler(this.buttonAddCatalog_Click);
            // 
            // buttonFeedEditor
            // 
            resources.ApplyResources(this.buttonFeedEditor, "buttonFeedEditor");
            this.buttonFeedEditor.Name = "buttonFeedEditor";
            this.buttonFeedEditor.Click += new System.EventHandler(this.buttonFeedEditor_Click);
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
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.toolStrip);
            resources.ApplyResources(this.panelBottom, "panelBottom");
            this.panelBottom.Name = "panelBottom";
            // 
            // toolStrip
            // 
            resources.ApplyResources(this.toolStrip, "toolStrip");
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.buttonOptions, this.buttonTools, this.buttonHelp, this.labelVersion });
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            // 
            // buttonOptions
            // 
            this.buttonOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            resources.ApplyResources(this.buttonOptions, "buttonOptions");
            this.buttonOptions.Name = "buttonOptions";
            this.buttonOptions.Click += new System.EventHandler(this.buttonOptions_Click);
            // 
            // buttonTools
            // 
            this.buttonTools.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.buttonTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this.buttonStoreManage, this.buttonPortableCreator, this.buttonCommandLine });
            resources.ApplyResources(this.buttonTools, "buttonTools");
            this.buttonTools.Name = "buttonTools";
            // 
            // buttonStoreManage
            // 
            this.buttonStoreManage.Name = "buttonStoreManage";
            resources.ApplyResources(this.buttonStoreManage, "buttonStoreManage");
            this.buttonStoreManage.Click += new System.EventHandler(this.buttonStoreManage_Click);
            // 
            // buttonPortableCreator
            // 
            this.buttonPortableCreator.Name = "buttonPortableCreator";
            resources.ApplyResources(this.buttonPortableCreator, "buttonPortableCreator");
            this.buttonPortableCreator.Click += new System.EventHandler(this.buttonPortableCreator_Click);
            // 
            // buttonCommandLine
            // 
            this.buttonCommandLine.Name = "buttonCommandLine";
            resources.ApplyResources(this.buttonCommandLine, "buttonCommandLine");
            this.buttonCommandLine.Click += new System.EventHandler(this.buttonCommandLine_Click);
            // 
            // buttonHelp
            // 
            this.buttonHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.buttonHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { this.buttonDocumentation, this.buttonIntro });
            resources.ApplyResources(this.buttonHelp, "buttonHelp");
            this.buttonHelp.Name = "buttonHelp";
            // 
            // buttonDocumentation
            // 
            this.buttonDocumentation.Name = "buttonDocumentation";
            resources.ApplyResources(this.buttonDocumentation, "buttonDocumentation");
            this.buttonDocumentation.Click += new System.EventHandler(this.buttonDocumentation_Click);
            // 
            // buttonIntro
            // 
            this.buttonIntro.Name = "buttonIntro";
            resources.ApplyResources(this.buttonIntro, "buttonIntro");
            this.buttonIntro.Click += new System.EventHandler(this.buttonIntro_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.labelVersion.Name = "labelVersion";
            resources.ApplyResources(this.labelVersion, "labelVersion");
            // 
            // rootTable
            // 
            resources.ApplyResources(this.rootTable, "rootTable");
            this.rootTable.Controls.Add(this.panelBottom, 0, 1);
            this.rootTable.Controls.Add(this.tabControlApps, 0, 0);
            this.rootTable.Name = "rootTable";
            // 
            // labelNotificationBar
            // 
            resources.ApplyResources(this.labelNotificationBar, "labelNotificationBar");
            this.labelNotificationBar.BackColor = System.Drawing.SystemColors.Info;
            this.labelNotificationBar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelNotificationBar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.labelNotificationBar.Name = "labelNotificationBar";
            this.labelNotificationBar.Click += new System.EventHandler(this.labelNotificationBar_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelNotificationBar);
            this.Controls.Add(this.rootTable);
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
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
            this.menuMoreApps.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.rootTable.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TabControl tabControlApps;
        private System.Windows.Forms.TabPage tabPageAppList;
        private System.Windows.Forms.TabPage tabPageCatalog;
        private ZeroInstall.Central.WinForms.AppTileList tileListMyApps;
        private AppTileList tileListCatalog;
        private System.Windows.Forms.Button buttonRefreshCatalog;
        private NanoByte.Common.Controls.DropDownButton buttonMoreApps;
        private NanoByte.Common.Controls.DropDownButton buttonSync;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.TableLayoutPanel rootTable;
        private System.Windows.Forms.Label labelLastCatalogError;
        private System.Windows.Forms.Label labelLoadingCatalog;
        private NanoByte.Common.Controls.DropDownButton buttonUpdateAll;
        private System.Windows.Forms.ContextMenuStrip menuUpdateAll;
        private System.Windows.Forms.ToolStripMenuItem buttonUpdateAllClean;
        private System.Windows.Forms.ContextMenuStrip menuSync;
        private System.Windows.Forms.ToolStripMenuItem buttonSyncSetup;
        private System.Windows.Forms.ToolStripMenuItem buttonSyncTroubleshoot;
        private System.Windows.Forms.Label labelNotificationBar;
        private System.Windows.Forms.ToolStripDropDownButton buttonTools;
        private System.Windows.Forms.ToolStripLabel labelVersion;
        private System.Windows.Forms.ToolStripDropDownButton buttonHelp;
        private System.Windows.Forms.ToolStripButton buttonOptions;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripMenuItem buttonIntro;
        private System.Windows.Forms.ToolStripMenuItem buttonStoreManage;
        private System.Windows.Forms.ToolStripMenuItem buttonCommandLine;
        private System.Windows.Forms.ToolStripMenuItem buttonPortableCreator;
        private System.Windows.Forms.ContextMenuStrip menuMoreApps;
        private System.Windows.Forms.ToolStripMenuItem buttonSearch;
        private System.Windows.Forms.ToolStripMenuItem buttonAddFeed;
        private System.Windows.Forms.ToolStripMenuItem buttonAddCatalog;
        private System.Windows.Forms.ToolStripMenuItem buttonFeedEditor;
        private System.Windows.Forms.ToolStripMenuItem buttonUpdateAll2;
        private System.Windows.Forms.ToolStripMenuItem buttonSync2;
        private System.Windows.Forms.ToolStripSeparator separator1;
        private System.Windows.Forms.ToolStripMenuItem buttonDocumentation;
    }
}
