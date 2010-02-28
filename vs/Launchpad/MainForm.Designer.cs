namespace ZeroInstall.Launchpad
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
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.splitContainerApps = new System.Windows.Forms.SplitContainer();
            this.groupBoxMyApps = new System.Windows.Forms.GroupBox();
            this.webBrowserMyApps = new System.Windows.Forms.WebBrowser();
            this.groupBoxNewApps = new System.Windows.Forms.GroupBox();
            this.webBrowserNewApps = new System.Windows.Forms.WebBrowser();
            this.groupBoxTools = new System.Windows.Forms.GroupBox();
            this.buttonHelp = new System.Windows.Forms.Button();
            this.buttonManageCache = new System.Windows.Forms.Button();
            this.buttonAddFeed = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.splitContainerApps.Panel1.SuspendLayout();
            this.splitContainerApps.Panel2.SuspendLayout();
            this.splitContainerApps.SuspendLayout();
            this.groupBoxMyApps.SuspendLayout();
            this.groupBoxNewApps.SuspendLayout();
            this.groupBoxTools.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox
            // 
            this.pictureBox.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pictureBox.Image = global::ZeroInstall.Launchpad.Properties.Resources.Logo;
            this.pictureBox.Location = new System.Drawing.Point(190, 25);
            this.pictureBox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(325, 50);
            this.pictureBox.TabIndex = 1;
            this.pictureBox.TabStop = false;
            // 
            // splitContainerApps
            // 
            this.splitContainerApps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerApps.Location = new System.Drawing.Point(22, 89);
            this.splitContainerApps.Name = "splitContainerApps";
            // 
            // splitContainerApps.Panel1
            // 
            this.splitContainerApps.Panel1.Controls.Add(this.groupBoxMyApps);
            this.splitContainerApps.Panel1MinSize = 120;
            // 
            // splitContainerApps.Panel2
            // 
            this.splitContainerApps.Panel2.Controls.Add(this.groupBoxNewApps);
            this.splitContainerApps.Panel2MinSize = 120;
            this.splitContainerApps.Size = new System.Drawing.Size(660, 288);
            this.splitContainerApps.SplitterDistance = 326;
            this.splitContainerApps.SplitterWidth = 10;
            this.splitContainerApps.TabIndex = 7;
            // 
            // groupBoxMyApps
            // 
            this.groupBoxMyApps.Controls.Add(this.webBrowserMyApps);
            this.groupBoxMyApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxMyApps.Location = new System.Drawing.Point(0, 0);
            this.groupBoxMyApps.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxMyApps.Name = "groupBoxMyApps";
            this.groupBoxMyApps.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.groupBoxMyApps.Size = new System.Drawing.Size(326, 288);
            this.groupBoxMyApps.TabIndex = 0;
            this.groupBoxMyApps.TabStop = false;
            this.groupBoxMyApps.Text = "&My applications";
            // 
            // webBrowserMyApps
            // 
            this.webBrowserMyApps.AllowWebBrowserDrop = false;
            this.webBrowserMyApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserMyApps.IsWebBrowserContextMenuEnabled = false;
            this.webBrowserMyApps.Location = new System.Drawing.Point(6, 26);
            this.webBrowserMyApps.Name = "webBrowserMyApps";
            this.webBrowserMyApps.ScriptErrorsSuppressed = true;
            this.webBrowserMyApps.Size = new System.Drawing.Size(314, 255);
            this.webBrowserMyApps.TabIndex = 0;
            this.webBrowserMyApps.WebBrowserShortcutsEnabled = false;
            // 
            // groupBoxNewApps
            // 
            this.groupBoxNewApps.Controls.Add(this.webBrowserNewApps);
            this.groupBoxNewApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxNewApps.Location = new System.Drawing.Point(0, 0);
            this.groupBoxNewApps.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxNewApps.Name = "groupBoxNewApps";
            this.groupBoxNewApps.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.groupBoxNewApps.Size = new System.Drawing.Size(324, 288);
            this.groupBoxNewApps.TabIndex = 0;
            this.groupBoxNewApps.TabStop = false;
            this.groupBoxNewApps.Text = "&New applications";
            // 
            // webBrowserNewApps
            // 
            this.webBrowserNewApps.AllowWebBrowserDrop = false;
            this.webBrowserNewApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserNewApps.IsWebBrowserContextMenuEnabled = false;
            this.webBrowserNewApps.Location = new System.Drawing.Point(6, 26);
            this.webBrowserNewApps.Name = "webBrowserNewApps";
            this.webBrowserNewApps.ScriptErrorsSuppressed = true;
            this.webBrowserNewApps.Size = new System.Drawing.Size(312, 255);
            this.webBrowserNewApps.TabIndex = 0;
            this.webBrowserNewApps.WebBrowserShortcutsEnabled = false;
            // 
            // groupBoxTools
            // 
            this.groupBoxTools.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTools.Controls.Add(this.buttonHelp);
            this.groupBoxTools.Controls.Add(this.buttonManageCache);
            this.groupBoxTools.Controls.Add(this.buttonAddFeed);
            this.groupBoxTools.Location = new System.Drawing.Point(22, 395);
            this.groupBoxTools.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxTools.Name = "groupBoxTools";
            this.groupBoxTools.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.groupBoxTools.Size = new System.Drawing.Size(660, 83);
            this.groupBoxTools.TabIndex = 8;
            this.groupBoxTools.TabStop = false;
            this.groupBoxTools.Text = "&Tools";
            // 
            // buttonHelp
            // 
            this.buttonHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHelp.Location = new System.Drawing.Point(501, 29);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(140, 35);
            this.buttonHelp.TabIndex = 3;
            this.buttonHelp.Text = "Help";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // buttonManageCache
            // 
            this.buttonManageCache.Location = new System.Drawing.Point(164, 29);
            this.buttonManageCache.Name = "buttonManageCache";
            this.buttonManageCache.Size = new System.Drawing.Size(140, 35);
            this.buttonManageCache.TabIndex = 1;
            this.buttonManageCache.Text = "Manage cache";
            this.buttonManageCache.UseVisualStyleBackColor = true;
            this.buttonManageCache.Click += new System.EventHandler(this.buttonManageCache_Click);
            // 
            // buttonAddFeed
            // 
            this.buttonAddFeed.Location = new System.Drawing.Point(18, 29);
            this.buttonAddFeed.Name = "buttonAddFeed";
            this.buttonAddFeed.Size = new System.Drawing.Size(140, 35);
            this.buttonAddFeed.TabIndex = 0;
            this.buttonAddFeed.Text = "Add feed";
            this.buttonAddFeed.UseVisualStyleBackColor = true;
            this.buttonAddFeed.Click += new System.EventHandler(this.buttonAddFeed_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(704, 502);
            this.Controls.Add(this.groupBoxTools);
            this.Controls.Add(this.splitContainerApps);
            this.Controls.Add(this.pictureBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(660, 360);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zero Install";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.splitContainerApps.Panel1.ResumeLayout(false);
            this.splitContainerApps.Panel2.ResumeLayout(false);
            this.splitContainerApps.ResumeLayout(false);
            this.groupBoxMyApps.ResumeLayout(false);
            this.groupBoxNewApps.ResumeLayout(false);
            this.groupBoxTools.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.SplitContainer splitContainerApps;
        private System.Windows.Forms.GroupBox groupBoxMyApps;
        private System.Windows.Forms.WebBrowser webBrowserMyApps;
        private System.Windows.Forms.GroupBox groupBoxNewApps;
        private System.Windows.Forms.WebBrowser webBrowserNewApps;
        private System.Windows.Forms.GroupBox groupBoxTools;
        private System.Windows.Forms.Button buttonAddFeed;
        private System.Windows.Forms.Button buttonManageCache;
        private System.Windows.Forms.Button buttonHelp;

    }
}

