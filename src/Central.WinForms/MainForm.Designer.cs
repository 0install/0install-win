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
            this.browserNewApps = new System.Windows.Forms.WebBrowser();
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
            this.pictureBoxLogo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.pictureBoxLogo.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.Logo;
            this.pictureBoxLogo.Location = new System.Drawing.Point(227, 15);
            this.pictureBoxLogo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(327, 57);
            this.pictureBoxLogo.TabIndex = 1;
            this.pictureBoxLogo.TabStop = false;
            // 
            // tabControlApps
            // 
            this.tabControlApps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlApps.Controls.Add(this.tabPageMyApps);
            this.tabControlApps.Controls.Add(this.tabPageNewApps);
            this.tabControlApps.Location = new System.Drawing.Point(14, 90);
            this.tabControlApps.Name = "tabControlApps";
            this.tabControlApps.SelectedIndex = 0;
            this.tabControlApps.Size = new System.Drawing.Size(756, 347);
            this.tabControlApps.TabIndex = 0;
            // 
            // tabPageMyApps
            // 
            this.tabPageMyApps.Controls.Add(this.labelNotAvailableYet);
            this.tabPageMyApps.Location = new System.Drawing.Point(4, 29);
            this.tabPageMyApps.Name = "tabPageMyApps";
            this.tabPageMyApps.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMyApps.Size = new System.Drawing.Size(748, 314);
            this.tabPageMyApps.TabIndex = 0;
            this.tabPageMyApps.Text = "My applications";
            this.tabPageMyApps.UseVisualStyleBackColor = true;
            // 
            // labelNotAvailableYet
            // 
            this.labelNotAvailableYet.AutoSize = true;
            this.labelNotAvailableYet.Location = new System.Drawing.Point(6, 12);
            this.labelNotAvailableYet.Name = "labelNotAvailableYet";
            this.labelNotAvailableYet.Size = new System.Drawing.Size(229, 20);
            this.labelNotAvailableYet.TabIndex = 0;
            this.labelNotAvailableYet.Text = "This feature is not available yet!";
            // 
            // tabPageNewApps
            // 
            this.tabPageNewApps.Controls.Add(this.toolStripNewApps);
            this.tabPageNewApps.Controls.Add(this.browserNewApps);
            this.tabPageNewApps.Location = new System.Drawing.Point(4, 29);
            this.tabPageNewApps.Name = "tabPageNewApps";
            this.tabPageNewApps.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageNewApps.Size = new System.Drawing.Size(748, 314);
            this.tabPageNewApps.TabIndex = 1;
            this.tabPageNewApps.Text = "New applications";
            this.tabPageNewApps.UseVisualStyleBackColor = true;
            // 
            // toolStripNewApps
            // 
            this.toolStripNewApps.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonBack});
            this.toolStripNewApps.Location = new System.Drawing.Point(3, 3);
            this.toolStripNewApps.Name = "toolStripNewApps";
            this.toolStripNewApps.Size = new System.Drawing.Size(742, 25);
            this.toolStripNewApps.TabIndex = 0;
            this.toolStripNewApps.Text = "toolStrip1";
            // 
            // toolStripButtonBack
            // 
            this.toolStripButtonBack.Enabled = false;
            this.toolStripButtonBack.Image = global::ZeroInstall.Central.WinForms.Properties.Resources.Back;
            this.toolStripButtonBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonBack.Name = "toolStripButtonBack";
            this.toolStripButtonBack.Size = new System.Drawing.Size(52, 22);
            this.toolStripButtonBack.Text = "Back";
            this.toolStripButtonBack.Click += new System.EventHandler(this.toolStripButtonBack_Click);
            // 
            // browserNewApps
            // 
            this.browserNewApps.AllowWebBrowserDrop = false;
            this.browserNewApps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.browserNewApps.IsWebBrowserContextMenuEnabled = false;
            this.browserNewApps.Location = new System.Drawing.Point(0, 31);
            this.browserNewApps.Name = "browserNewApps";
            this.browserNewApps.ScriptErrorsSuppressed = true;
            this.browserNewApps.Size = new System.Drawing.Size(745, 280);
            this.browserNewApps.TabIndex = 1;
            this.browserNewApps.Url = new System.Uri("", System.UriKind.Relative);
            this.browserNewApps.WebBrowserShortcutsEnabled = false;
            this.browserNewApps.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.browserNewApps_Navigating);
            this.browserNewApps.NewWindow += new System.ComponentModel.CancelEventHandler(this.browserNewApps_NewWindow);
            // 
            // buttonLaunchInterface
            // 
            this.buttonLaunchInterface.Location = new System.Drawing.Point(19, 29);
            this.buttonLaunchInterface.Name = "buttonLaunchInterface";
            this.buttonLaunchInterface.Size = new System.Drawing.Size(140, 35);
            this.buttonLaunchInterface.TabIndex = 0;
            this.buttonLaunchInterface.Text = "&Launch interface";
            this.buttonLaunchInterface.UseVisualStyleBackColor = true;
            this.buttonLaunchInterface.Click += new System.EventHandler(this.buttonLaunchInterface_Click);
            // 
            // groupBoxTools
            // 
            this.groupBoxTools.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxTools.Controls.Add(this.buttonConfiguration);
            this.groupBoxTools.Controls.Add(this.buttonCacheManagement);
            this.groupBoxTools.Controls.Add(this.buttonLaunchInterface);
            this.groupBoxTools.Controls.Add(this.buttonHelp);
            this.groupBoxTools.Location = new System.Drawing.Point(14, 445);
            this.groupBoxTools.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBoxTools.Name = "groupBoxTools";
            this.groupBoxTools.Padding = new System.Windows.Forms.Padding(6, 7, 6, 7);
            this.groupBoxTools.Size = new System.Drawing.Size(756, 83);
            this.groupBoxTools.TabIndex = 1;
            this.groupBoxTools.TabStop = false;
            this.groupBoxTools.Text = "Tools";
            // 
            // buttonConfiguration
            // 
            this.buttonConfiguration.Location = new System.Drawing.Point(341, 29);
            this.buttonConfiguration.Name = "buttonConfiguration";
            this.buttonConfiguration.Size = new System.Drawing.Size(140, 35);
            this.buttonConfiguration.TabIndex = 2;
            this.buttonConfiguration.Text = "C&onfiguration";
            this.buttonConfiguration.UseVisualStyleBackColor = true;
            this.buttonConfiguration.Click += new System.EventHandler(this.buttonConfiguration_Click);
            // 
            // buttonCacheManagement
            // 
            this.buttonCacheManagement.Location = new System.Drawing.Point(165, 29);
            this.buttonCacheManagement.Name = "buttonCacheManagement";
            this.buttonCacheManagement.Size = new System.Drawing.Size(170, 35);
            this.buttonCacheManagement.TabIndex = 1;
            this.buttonCacheManagement.Text = "&Cache management";
            this.buttonCacheManagement.UseVisualStyleBackColor = true;
            this.buttonCacheManagement.Click += new System.EventHandler(this.buttonCacheManagement_Click);
            // 
            // buttonHelp
            // 
            this.buttonHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHelp.Location = new System.Drawing.Point(607, 29);
            this.buttonHelp.Name = "buttonHelp";
            this.buttonHelp.Size = new System.Drawing.Size(140, 35);
            this.buttonHelp.TabIndex = 3;
            this.buttonHelp.Text = "&Help";
            this.buttonHelp.UseVisualStyleBackColor = true;
            this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersion.Location = new System.Drawing.Point(14, 533);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(756, 20);
            this.labelVersion.TabIndex = 2;
            this.labelVersion.Text = "(Version)";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // selfUpdateWorker
            // 
            this.selfUpdateWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.selfUpdateWorker_DoWork);
            this.selfUpdateWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.selfUpdateWorker_RunWorkerCompleted);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.groupBoxTools);
            this.Controls.Add(this.tabControlApps);
            this.Controls.Add(this.pictureBoxLogo);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MinimumSize = new System.Drawing.Size(683, 480);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Zero Install";
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
        private System.Windows.Forms.WebBrowser browserNewApps;
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

