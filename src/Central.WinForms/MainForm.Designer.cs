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
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.tabControlApps = new System.Windows.Forms.TabControl();
            this.tabPageMyApps = new System.Windows.Forms.TabPage();
            this.labelNotAvailableYet = new System.Windows.Forms.Label();
            this.tabPageNewApps = new System.Windows.Forms.TabPage();
            this.toolStripNewApps = new System.Windows.Forms.ToolStrip();
            this.toolStripButtonBack = new System.Windows.Forms.ToolStripButton();
            this.browserCatalog = new System.Windows.Forms.WebBrowser();
            this.buttonLaunchInterface = new System.Windows.Forms.Button();
            this.groupBoxTools = new System.Windows.Forms.GroupBox();
            this.buttonConfiguration = new System.Windows.Forms.Button();
            this.buttonCacheManagement = new System.Windows.Forms.Button();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.selfUpdateWorker = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.tabControlApps.SuspendLayout();
            this.tabPageMyApps.SuspendLayout();
            this.tabPageNewApps.SuspendLayout();
            this.toolStripNewApps.SuspendLayout();
            this.groupBoxTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBoxLogo
            // 
            resources.ApplyResources(this.pictureBoxLogo, "pictureBoxLogo");
            this.pictureBoxLogo.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.Logo;
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.TabStop = false;
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
            this.tabPageMyApps.Controls.Add(this.labelNotAvailableYet);
            resources.ApplyResources(this.tabPageMyApps, "tabPageMyApps");
            this.tabPageMyApps.Name = "tabPageMyApps";
            this.tabPageMyApps.UseVisualStyleBackColor = true;
            // 
            // labelNotAvailableYet
            // 
            resources.ApplyResources(this.labelNotAvailableYet, "labelNotAvailableYet");
            this.labelNotAvailableYet.Name = "labelNotAvailableYet";
            // 
            // tabPageNewApps
            // 
            this.tabPageNewApps.Controls.Add(this.toolStripNewApps);
            this.tabPageNewApps.Controls.Add(this.browserCatalog);
            resources.ApplyResources(this.tabPageNewApps, "tabPageNewApps");
            this.tabPageNewApps.Name = "tabPageNewApps";
            this.tabPageNewApps.UseVisualStyleBackColor = true;
            // 
            // toolStripNewApps
            // 
            this.toolStripNewApps.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonBack});
            resources.ApplyResources(this.toolStripNewApps, "toolStripNewApps");
            this.toolStripNewApps.Name = "toolStripNewApps";
            // 
            // toolStripButtonBack
            // 
            resources.ApplyResources(this.toolStripButtonBack, "toolStripButtonBack");
            this.toolStripButtonBack.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.Back;
            this.toolStripButtonBack.Name = "toolStripButtonBack";
            this.toolStripButtonBack.Click += new System.EventHandler(this.toolStripButtonBack_Click);
            // 
            // browserCatalog
            // 
            this.browserCatalog.AllowWebBrowserDrop = false;
            resources.ApplyResources(this.browserCatalog, "browserCatalog");
            this.browserCatalog.IsWebBrowserContextMenuEnabled = false;
            this.browserCatalog.Name = "browserCatalog";
            this.browserCatalog.ScriptErrorsSuppressed = true;
            this.browserCatalog.Url = new System.Uri("", System.UriKind.Relative);
            this.browserCatalog.WebBrowserShortcutsEnabled = false;
            this.browserCatalog.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.browserNewApps_Navigating);
            this.browserCatalog.NewWindow += new System.ComponentModel.CancelEventHandler(this.browserNewApps_NewWindow);
            // 
            // buttonLaunchInterface
            // 
            resources.ApplyResources(this.buttonLaunchInterface, "buttonLaunchInterface");
            this.buttonLaunchInterface.Name = "buttonLaunchInterface";
            this.buttonLaunchInterface.UseVisualStyleBackColor = true;
            this.buttonLaunchInterface.Click += new System.EventHandler(this.buttonLaunchInterface_Click);
            // 
            // groupBoxTools
            // 
            resources.ApplyResources(this.groupBoxTools, "groupBoxTools");
            this.groupBoxTools.Controls.Add(this.buttonConfiguration);
            this.groupBoxTools.Controls.Add(this.buttonCacheManagement);
            this.groupBoxTools.Controls.Add(this.buttonLaunchInterface);
            this.groupBoxTools.Controls.Add(this.buttonHelp);
            this.groupBoxTools.Name = "groupBoxTools";
            this.groupBoxTools.TabStop = false;
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
            // MainForm
            // 
            this.AllowDrop = true;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.groupBoxTools);
            this.Controls.Add(this.tabControlApps);
            this.Controls.Add(this.pictureBoxLogo);
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.tabControlApps.ResumeLayout(false);
            this.tabPageMyApps.ResumeLayout(false);
            this.tabPageMyApps.PerformLayout();
            this.tabPageNewApps.ResumeLayout(false);
            this.tabPageNewApps.PerformLayout();
            this.toolStripNewApps.ResumeLayout(false);
            this.toolStripNewApps.PerformLayout();
            this.groupBoxTools.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxLogo;
        private System.Windows.Forms.TabControl tabControlApps;
        private System.Windows.Forms.TabPage tabPageMyApps;
        private System.Windows.Forms.TabPage tabPageNewApps;
        private System.Windows.Forms.WebBrowser browserCatalog;
        private System.Windows.Forms.GroupBox groupBoxTools;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.Button buttonLaunchInterface;
        private System.Windows.Forms.ToolStrip toolStripNewApps;
        private System.Windows.Forms.ToolStripButton toolStripButtonBack;
        private System.Windows.Forms.Label labelNotAvailableYet;
        private System.Windows.Forms.Button buttonCacheManagement;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Button buttonConfiguration;
        private System.ComponentModel.BackgroundWorker selfUpdateWorker;

    }
}

