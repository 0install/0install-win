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
            this.tileListMyApps = new ZeroInstall.Central.WinForms.AppTileList();
            this.tabPageCatalog = new System.Windows.Forms.TabPage();
            this.tileListCatalog = new ZeroInstall.Central.WinForms.AppTileList();
            this.labelVideo = new System.Windows.Forms.Label();
            this.buttonReplay = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.arrowMyApps = new System.Windows.Forms.PictureBox();
            this.arrowSearch = new System.Windows.Forms.PictureBox();
            this.tabControlApps.SuspendLayout();
            this.tabPageAppList.SuspendLayout();
            this.tabPageCatalog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.arrowMyApps)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.arrowSearch)).BeginInit();
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
            this.tabPageAppList.Controls.Add(this.tileListMyApps);
            resources.ApplyResources(this.tabPageAppList, "tabPageAppList");
            this.tabPageAppList.Name = "tabPageAppList";
            this.tabPageAppList.UseVisualStyleBackColor = true;
            // 
            // tileListMyApps
            // 
            resources.ApplyResources(this.tileListMyApps, "tileListMyApps");
            this.tileListMyApps.Name = "tileListMyApps";
            // 
            // tabPageCatalog
            // 
            this.tabPageCatalog.Controls.Add(this.tileListCatalog);
            resources.ApplyResources(this.tabPageCatalog, "tabPageCatalog");
            this.tabPageCatalog.Name = "tabPageCatalog";
            this.tabPageCatalog.UseVisualStyleBackColor = true;
            // 
            // tileListCatalog
            // 
            resources.ApplyResources(this.tileListCatalog, "tileListCatalog");
            this.tileListCatalog.Name = "tileListCatalog";
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
            // arrowMyApps
            // 
            this.arrowMyApps.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.ArrowDown;
            resources.ApplyResources(this.arrowMyApps, "arrowMyApps");
            this.arrowMyApps.Name = "arrowMyApps";
            this.arrowMyApps.TabStop = false;
            // 
            // arrowSearch
            // 
            this.arrowSearch.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.ArrowRight;
            resources.ApplyResources(this.arrowSearch, "arrowSearch");
            this.arrowSearch.Name = "arrowSearch";
            this.arrowSearch.TabStop = false;
            // 
            // IntroDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.arrowSearch);
            this.Controls.Add(this.arrowMyApps);
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
            this.Load += new System.EventHandler(this.IntroDialog_Load);
            this.tabControlApps.ResumeLayout(false);
            this.tabPageAppList.ResumeLayout(false);
            this.tabPageCatalog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.arrowMyApps)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.arrowSearch)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelSubtitles;
        private System.Windows.Forms.TabControl tabControlApps;
        private System.Windows.Forms.TabPage tabPageAppList;
        private AppTileList tileListMyApps;
        private System.Windows.Forms.TabPage tabPageCatalog;
        private AppTileList tileListCatalog;
        private System.Windows.Forms.Label labelVideo;
        private System.Windows.Forms.Button buttonReplay;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.PictureBox arrowMyApps;
        private System.Windows.Forms.PictureBox arrowSearch;
    }
}
