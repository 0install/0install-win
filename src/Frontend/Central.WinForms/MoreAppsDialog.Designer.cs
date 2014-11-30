namespace ZeroInstall.Central.WinForms
{
    partial class MoreAppsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoreAppsDialog));
            this.labelFeeds = new System.Windows.Forms.Label();
            this.labelMoreApps = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonFeedEditor = new System.Windows.Forms.Button();
            this.buttonCatalog = new System.Windows.Forms.Button();
            this.buttonFeed = new System.Windows.Forms.Button();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelFeeds
            // 
            resources.ApplyResources(this.labelFeeds, "labelFeeds");
            this.labelFeeds.Name = "labelFeeds";
            // 
            // labelMoreApps
            // 
            resources.ApplyResources(this.labelMoreApps, "labelMoreApps");
            this.labelMoreApps.Name = "labelMoreApps";
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonFeedEditor
            // 
            resources.ApplyResources(this.buttonFeedEditor, "buttonFeedEditor");
            this.buttonFeedEditor.Name = "buttonFeedEditor";
            this.buttonFeedEditor.UseVisualStyleBackColor = true;
            this.buttonFeedEditor.Click += new System.EventHandler(this.buttonFeedEditor_Click);
            // 
            // buttonCatalog
            // 
            resources.ApplyResources(this.buttonCatalog, "buttonCatalog");
            this.buttonCatalog.Name = "buttonCatalog";
            this.buttonCatalog.UseVisualStyleBackColor = true;
            this.buttonCatalog.Click += new System.EventHandler(this.buttonCatalog_Click);
            // 
            // buttonFeed
            // 
            resources.ApplyResources(this.buttonFeed, "buttonFeed");
            this.buttonFeed.Name = "buttonFeed";
            this.buttonFeed.UseVisualStyleBackColor = true;
            this.buttonFeed.Click += new System.EventHandler(this.buttonFeed_Click);
            // 
            // buttonSearch
            // 
            resources.ApplyResources(this.buttonSearch, "buttonSearch");
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // MoreAppsDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonFeedEditor);
            this.Controls.Add(this.buttonCatalog);
            this.Controls.Add(this.buttonFeed);
            this.Controls.Add(this.labelMoreApps);
            this.Controls.Add(this.labelFeeds);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoreAppsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelFeeds;
        private System.Windows.Forms.Label labelMoreApps;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonFeedEditor;
        private System.Windows.Forms.Button buttonCatalog;
        private System.Windows.Forms.Button buttonFeed;
        private System.Windows.Forms.Button buttonSearch;
    }
}