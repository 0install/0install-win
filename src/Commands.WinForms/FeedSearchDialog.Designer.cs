namespace ZeroInstall.Commands.WinForms
{
    partial class FeedSearchDialog
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
            System.Windows.Forms.ToolStripSeparator separator1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FeedSearchDialog));
            System.Windows.Forms.ToolStripSeparator separator2;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.columnUri = new System.Windows.Forms.DataGridViewLinkColumn();
            this.columnName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnScore = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnSummary = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnCategories = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.buttonRun = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonIntegrate = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonDetails = new System.Windows.Forms.ToolStripMenuItem();
            this.textKeywords = new NanoByte.Common.Controls.HintTextBox();
            this.labelInfo = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            separator1 = new System.Windows.Forms.ToolStripSeparator();
            separator2 = new System.Windows.Forms.ToolStripSeparator();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.contextMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // separator1
            // 
            separator1.Name = "separator1";
            resources.ApplyResources(separator1, "separator1");
            // 
            // separator2
            // 
            separator2.Name = "separator2";
            resources.ApplyResources(separator2, "separator2");
            // 
            // dataGrid
            // 
            this.dataGrid.AllowUserToAddRows = false;
            this.dataGrid.AllowUserToDeleteRows = false;
            this.dataGrid.AllowUserToOrderColumns = true;
            resources.ApplyResources(this.dataGrid, "dataGrid");
            this.dataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { this.columnUri, this.columnName, this.columnScore, this.columnSummary, this.columnCategories });
            this.dataGrid.MultiSelect = false;
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.ReadOnly = true;
            this.dataGrid.RowHeadersVisible = false;
            this.dataGrid.ShowEditingIcon = false;
            this.dataGrid.StandardTab = true;
            this.dataGrid.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGrid_CellContentClick);
            // 
            // columnUri
            // 
            this.columnUri.DataPropertyName = "Uri";
            resources.ApplyResources(this.columnUri, "columnUri");
            this.columnUri.Name = "columnUri";
            this.columnUri.ReadOnly = true;
            this.columnUri.TrackVisitedState = false;
            // 
            // columnName
            // 
            this.columnName.DataPropertyName = "Name";
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.columnName.DefaultCellStyle = dataGridViewCellStyle1;
            resources.ApplyResources(this.columnName, "columnName");
            this.columnName.Name = "columnName";
            this.columnName.ReadOnly = true;
            // 
            // columnScore
            // 
            this.columnScore.DataPropertyName = "Score";
            dataGridViewCellStyle2.NullValue = null;
            this.columnScore.DefaultCellStyle = dataGridViewCellStyle2;
            resources.ApplyResources(this.columnScore, "columnScore");
            this.columnScore.Name = "columnScore";
            this.columnScore.ReadOnly = true;
            // 
            // columnSummary
            // 
            this.columnSummary.DataPropertyName = "Summary";
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.columnSummary.DefaultCellStyle = dataGridViewCellStyle3;
            resources.ApplyResources(this.columnSummary, "columnSummary");
            this.columnSummary.Name = "columnSummary";
            this.columnSummary.ReadOnly = true;
            // 
            // columnCategories
            // 
            this.columnCategories.DataPropertyName = "CategoriesString";
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.columnCategories.DefaultCellStyle = dataGridViewCellStyle4;
            resources.ApplyResources(this.columnCategories, "columnCategories");
            this.columnCategories.Name = "columnCategories";
            this.columnCategories.ReadOnly = true;
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { this.buttonRun, separator1, this.buttonAdd, this.buttonIntegrate, separator2, this.buttonDetails });
            this.contextMenu.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenu, "contextMenu");
            // 
            // buttonRun
            // 
            this.buttonRun.Name = "buttonRun";
            resources.ApplyResources(this.buttonRun, "buttonRun");
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonAdd
            // 
            resources.ApplyResources(this.buttonAdd, "buttonAdd");
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonIntegrate
            // 
            resources.ApplyResources(this.buttonIntegrate, "buttonIntegrate");
            this.buttonIntegrate.Name = "buttonIntegrate";
            this.buttonIntegrate.Click += new System.EventHandler(this.buttonIntegrate_Click);
            // 
            // buttonDetails
            // 
            resources.ApplyResources(this.buttonDetails, "buttonDetails");
            this.buttonDetails.Name = "buttonDetails";
            this.buttonDetails.Click += new System.EventHandler(this.buttonDetails_Click);
            // 
            // textKeywords
            // 
            resources.ApplyResources(this.textKeywords, "textKeywords");
            this.textKeywords.Name = "textKeywords";
            this.textKeywords.ShowClearButton = true;
            // 
            // labelInfo
            // 
            resources.ApplyResources(this.labelInfo, "labelInfo");
            this.labelInfo.AutoEllipsis = true;
            this.labelInfo.Name = "labelInfo";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // FeedSearchDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.textKeywords);
            this.Controls.Add(this.dataGrid);
            this.Name = "FeedSearchDialog";
            this.ShowIcon = false;
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.contextMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ToolStripMenuItem buttonAdd;
        private System.Windows.Forms.ToolStripMenuItem buttonDetails;
        private System.Windows.Forms.ToolStripMenuItem buttonIntegrate;
        private System.Windows.Forms.ToolStripMenuItem buttonRun;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnCategories;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnName;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnScore;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnSummary;
        private System.Windows.Forms.DataGridViewLinkColumn columnUri;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.DataGridView dataGrid;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Label labelInfo;
        private NanoByte.Common.Controls.HintTextBox textKeywords;
        #endregion
    }
}
