namespace ZeroInstall.Central.WinForms
{
    partial class IntroDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntroDialog));
            this.labelSubtitles = new System.Windows.Forms.Label();
            this.tabControlApps = new System.Windows.Forms.TabControl();
            this.tabPageAppList = new System.Windows.Forms.TabPage();
            this.arrowIntegrate = new System.Windows.Forms.PictureBox();
            this.appList = new ZeroInstall.Central.WinForms.AppTileList();
            this.tabPageCatalog = new System.Windows.Forms.TabPage();
            this.arrowAdd = new System.Windows.Forms.PictureBox();
            this.arrowRun = new System.Windows.Forms.PictureBox();
            this.catalogList = new ZeroInstall.Central.WinForms.AppTileList();
            this.timerActions = new System.Windows.Forms.Timer(this.components);
            this.labelVideo = new System.Windows.Forms.Label();
            this.buttonReplay = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.tabControlApps.SuspendLayout();
            this.tabPageAppList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.arrowIntegrate)).BeginInit();
            this.tabPageCatalog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.arrowAdd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.arrowRun)).BeginInit();
            this.SuspendLayout();
            // 
            // labelSubtitles
            // 
            resources.ApplyResources(this.labelSubtitles, "labelSubtitles");
            this.labelSubtitles.Name = "labelSubtitles";
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
            this.tabPageAppList.Controls.Add(this.arrowIntegrate);
            this.tabPageAppList.Controls.Add(this.appList);
            resources.ApplyResources(this.tabPageAppList, "tabPageAppList");
            this.tabPageAppList.Name = "tabPageAppList";
            this.tabPageAppList.UseVisualStyleBackColor = true;
            // 
            // arrowIntegrate
            // 
            this.arrowIntegrate.BackColor = System.Drawing.Color.Transparent;
            this.arrowIntegrate.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.ArrowUp;
            resources.ApplyResources(this.arrowIntegrate, "arrowIntegrate");
            this.arrowIntegrate.Name = "arrowIntegrate";
            this.arrowIntegrate.TabStop = false;
            // 
            // appList
            // 
            resources.ApplyResources(this.appList, "appList");
            this.appList.Name = "appList";
            // 
            // tabPageCatalog
            // 
            this.tabPageCatalog.Controls.Add(this.arrowAdd);
            this.tabPageCatalog.Controls.Add(this.arrowRun);
            this.tabPageCatalog.Controls.Add(this.catalogList);
            resources.ApplyResources(this.tabPageCatalog, "tabPageCatalog");
            this.tabPageCatalog.Name = "tabPageCatalog";
            this.tabPageCatalog.UseVisualStyleBackColor = true;
            // 
            // arrowAdd
            // 
            this.arrowAdd.BackColor = System.Drawing.Color.Transparent;
            this.arrowAdd.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.ArrowUp;
            resources.ApplyResources(this.arrowAdd, "arrowAdd");
            this.arrowAdd.Name = "arrowAdd";
            this.arrowAdd.TabStop = false;
            // 
            // arrowRun
            // 
            this.arrowRun.BackColor = System.Drawing.Color.Transparent;
            this.arrowRun.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.ArrowRight;
            resources.ApplyResources(this.arrowRun, "arrowRun");
            this.arrowRun.Name = "arrowRun";
            this.arrowRun.TabStop = false;
            // 
            // catalogList
            // 
            resources.ApplyResources(this.catalogList, "catalogList");
            this.catalogList.Name = "catalogList";
            // 
            // timerActions
            // 
            this.timerActions.Tick += new System.EventHandler(this.timerActions_Tick);
            // 
            // labelVideo
            // 
            resources.ApplyResources(this.labelVideo, "labelVideo");
            this.labelVideo.Name = "labelVideo";
            // 
            // buttonReplay
            // 
            resources.ApplyResources(this.buttonReplay, "buttonReplay");
            this.buttonReplay.Name = "buttonReplay";
            this.buttonReplay.UseVisualStyleBackColor = true;
            this.buttonReplay.Click += new System.EventHandler(this.buttonReplay_Click);
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // IntroDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabControlApps);
            this.Controls.Add(this.labelSubtitles);
            this.Controls.Add(this.labelVideo);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonReplay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IntroDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.tabControlApps.ResumeLayout(false);
            this.tabPageAppList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.arrowIntegrate)).EndInit();
            this.tabPageCatalog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.arrowAdd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.arrowRun)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelSubtitles;
        private System.Windows.Forms.TabControl tabControlApps;
        private System.Windows.Forms.TabPage tabPageAppList;
        private AppTileList appList;
        private System.Windows.Forms.TabPage tabPageCatalog;
        private AppTileList catalogList;
        private System.Windows.Forms.Timer timerActions;
        private System.Windows.Forms.Label labelVideo;
        private System.Windows.Forms.Button buttonReplay;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.PictureBox arrowRun;
        private System.Windows.Forms.PictureBox arrowIntegrate;
        private System.Windows.Forms.PictureBox arrowAdd;
    }
}