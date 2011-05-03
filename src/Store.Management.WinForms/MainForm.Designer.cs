using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Management.WinForms
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
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanelSize = new System.Windows.Forms.TableLayoutPanel();
            this.labelCurrentSize = new System.Windows.Forms.Label();
            this.textTotalSize = new System.Windows.Forms.TextBox();
            this.textCurrentSize = new System.Windows.Forms.TextBox();
            this.textCheckedSize = new System.Windows.Forms.TextBox();
            this.labelCheckedSize = new System.Windows.Forms.Label();
            this.labelTotalSize = new System.Windows.Forms.Label();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonVerify = new System.Windows.Forms.Button();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tableLayoutPanelSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(409, 439);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRefresh.Location = new System.Drawing.Point(328, 439);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 2;
            this.buttonRefresh.Text = "&Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 12);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tableLayoutPanelSize);
            this.splitContainer.Panel2.Controls.Add(this.propertyGrid);
            this.splitContainer.Size = new System.Drawing.Size(472, 421);
            this.splitContainer.SplitterDistance = 229;
            this.splitContainer.TabIndex = 0;
            // 
            // tableLayoutPanelSize
            // 
            this.tableLayoutPanelSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelSize.ColumnCount = 2;
            this.tableLayoutPanelSize.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSize.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelSize.Controls.Add(this.labelCurrentSize, 0, 0);
            this.tableLayoutPanelSize.Controls.Add(this.textTotalSize, 1, 2);
            this.tableLayoutPanelSize.Controls.Add(this.textCurrentSize, 1, 0);
            this.tableLayoutPanelSize.Controls.Add(this.textCheckedSize, 1, 1);
            this.tableLayoutPanelSize.Controls.Add(this.labelCheckedSize, 0, 1);
            this.tableLayoutPanelSize.Controls.Add(this.labelTotalSize, 0, 2);
            this.tableLayoutPanelSize.Location = new System.Drawing.Point(0, 360);
            this.tableLayoutPanelSize.Name = "tableLayoutPanelSize";
            this.tableLayoutPanelSize.RowCount = 3;
            this.tableLayoutPanelSize.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanelSize.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.tableLayoutPanelSize.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.tableLayoutPanelSize.Size = new System.Drawing.Size(239, 58);
            this.tableLayoutPanelSize.TabIndex = 1;
            // 
            // labelCurrentSize
            // 
            this.labelCurrentSize.AutoSize = true;
            this.labelCurrentSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCurrentSize.Location = new System.Drawing.Point(3, 0);
            this.labelCurrentSize.Name = "labelCurrentSize";
            this.labelCurrentSize.Size = new System.Drawing.Size(108, 19);
            this.labelCurrentSize.TabIndex = 0;
            this.labelCurrentSize.Text = "Current entry size:";
            this.labelCurrentSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textTotalSize
            // 
            this.textTotalSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textTotalSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textTotalSize.Location = new System.Drawing.Point(117, 41);
            this.textTotalSize.Name = "textTotalSize";
            this.textTotalSize.ReadOnly = true;
            this.textTotalSize.Size = new System.Drawing.Size(119, 13);
            this.textTotalSize.TabIndex = 5;
            this.textTotalSize.Text = "-";
            // 
            // textCurrentSize
            // 
            this.textCurrentSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textCurrentSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textCurrentSize.Location = new System.Drawing.Point(117, 3);
            this.textCurrentSize.Name = "textCurrentSize";
            this.textCurrentSize.ReadOnly = true;
            this.textCurrentSize.Size = new System.Drawing.Size(119, 13);
            this.textCurrentSize.TabIndex = 1;
            this.textCurrentSize.Text = "-";
            // 
            // textCheckedSize
            // 
            this.textCheckedSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textCheckedSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textCheckedSize.Location = new System.Drawing.Point(117, 22);
            this.textCheckedSize.Name = "textCheckedSize";
            this.textCheckedSize.ReadOnly = true;
            this.textCheckedSize.Size = new System.Drawing.Size(119, 13);
            this.textCheckedSize.TabIndex = 3;
            this.textCheckedSize.Text = "-";
            // 
            // labelCheckedSize
            // 
            this.labelCheckedSize.AutoSize = true;
            this.labelCheckedSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCheckedSize.Location = new System.Drawing.Point(3, 19);
            this.labelCheckedSize.Name = "labelCheckedSize";
            this.labelCheckedSize.Size = new System.Drawing.Size(108, 19);
            this.labelCheckedSize.TabIndex = 2;
            this.labelCheckedSize.Text = "Checked entries size:";
            this.labelCheckedSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelTotalSize
            // 
            this.labelTotalSize.AutoSize = true;
            this.labelTotalSize.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelTotalSize.Location = new System.Drawing.Point(3, 38);
            this.labelTotalSize.Name = "labelTotalSize";
            this.labelTotalSize.Size = new System.Drawing.Size(108, 20);
            this.labelTotalSize.TabIndex = 4;
            this.labelTotalSize.Text = "Total size:";
            this.labelTotalSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // propertyGrid
            // 
            this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid.Location = new System.Drawing.Point(0, 0);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGrid.Size = new System.Drawing.Size(239, 354);
            this.propertyGrid.TabIndex = 0;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemove.Enabled = false;
            this.buttonRemove.Location = new System.Drawing.Point(12, 439);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 1;
            this.buttonRemove.Text = "&Remove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonVerify
            // 
            this.buttonVerify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonVerify.Enabled = false;
            this.buttonVerify.Location = new System.Drawing.Point(93, 439);
            this.buttonVerify.Name = "buttonVerify";
            this.buttonVerify.Size = new System.Drawing.Size(75, 23);
            this.buttonVerify.TabIndex = 1;
            this.buttonVerify.Text = "&Verify";
            this.buttonVerify.UseVisualStyleBackColor = true;
            this.buttonVerify.Click += new System.EventHandler(this.buttonVerify_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(496, 474);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.buttonVerify);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonClose);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(350, 250);
            this.Name = "MainForm";
            this.Text = "Zero Install cache management";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.tableLayoutPanelSize.ResumeLayout(false);
            this.tableLayoutPanelSize.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSize;
        private System.Windows.Forms.Label labelCurrentSize;
        private System.Windows.Forms.TextBox textTotalSize;
        private System.Windows.Forms.TextBox textCurrentSize;
        private System.Windows.Forms.TextBox textCheckedSize;
        private System.Windows.Forms.Label labelCheckedSize;
        private System.Windows.Forms.Label labelTotalSize;
        private System.Windows.Forms.Button buttonVerify;

    }
}