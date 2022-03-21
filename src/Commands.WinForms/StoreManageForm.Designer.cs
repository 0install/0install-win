namespace ZeroInstall.Commands.WinForms
{
    partial class StoreManageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StoreManageForm));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanelSize = new System.Windows.Forms.TableLayoutPanel();
            this.labelCurrentSize = new System.Windows.Forms.Label();
            this.textTotalSize = new System.Windows.Forms.TextBox();
            this.textCurrentSize = new System.Windows.Forms.TextBox();
            this.textCheckedSize = new System.Windows.Forms.TextBox();
            this.labelCheckedSize = new System.Windows.Forms.Label();
            this.labelTotalSize = new System.Windows.Forms.Label();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonVerify = new System.Windows.Forms.Button();
            this.buttonRunAsAdmin = new System.Windows.Forms.Button();
            this.labelLoading = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer)).BeginInit();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tableLayoutPanelSize.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            resources.ApplyResources(this.splitContainer, "splitContainer");
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.tableLayoutPanelSize);
            this.splitContainer.Panel2.Controls.Add(this.propertyGrid);
            // 
            // tableLayoutPanelSize
            // 
            resources.ApplyResources(this.tableLayoutPanelSize, "tableLayoutPanelSize");
            this.tableLayoutPanelSize.Controls.Add(this.labelCurrentSize, 0, 0);
            this.tableLayoutPanelSize.Controls.Add(this.textTotalSize, 1, 2);
            this.tableLayoutPanelSize.Controls.Add(this.textCurrentSize, 1, 0);
            this.tableLayoutPanelSize.Controls.Add(this.textCheckedSize, 1, 1);
            this.tableLayoutPanelSize.Controls.Add(this.labelCheckedSize, 0, 1);
            this.tableLayoutPanelSize.Controls.Add(this.labelTotalSize, 0, 2);
            this.tableLayoutPanelSize.Name = "tableLayoutPanelSize";
            // 
            // labelCurrentSize
            // 
            resources.ApplyResources(this.labelCurrentSize, "labelCurrentSize");
            this.labelCurrentSize.Name = "labelCurrentSize";
            // 
            // textTotalSize
            // 
            this.textTotalSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textTotalSize, "textTotalSize");
            this.textTotalSize.Name = "textTotalSize";
            this.textTotalSize.ReadOnly = true;
            // 
            // textCurrentSize
            // 
            this.textCurrentSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textCurrentSize, "textCurrentSize");
            this.textCurrentSize.Name = "textCurrentSize";
            this.textCurrentSize.ReadOnly = true;
            // 
            // textCheckedSize
            // 
            this.textCheckedSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            resources.ApplyResources(this.textCheckedSize, "textCheckedSize");
            this.textCheckedSize.Name = "textCheckedSize";
            this.textCheckedSize.ReadOnly = true;
            // 
            // labelCheckedSize
            // 
            resources.ApplyResources(this.labelCheckedSize, "labelCheckedSize");
            this.labelCheckedSize.Name = "labelCheckedSize";
            // 
            // labelTotalSize
            // 
            resources.ApplyResources(this.labelTotalSize, "labelTotalSize");
            this.labelTotalSize.Name = "labelTotalSize";
            // 
            // propertyGrid
            // 
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonRefresh
            // 
            resources.ApplyResources(this.buttonRefresh, "buttonRefresh");
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // buttonRemove
            // 
            resources.ApplyResources(this.buttonRemove, "buttonRemove");
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonVerify
            // 
            resources.ApplyResources(this.buttonVerify, "buttonVerify");
            this.buttonVerify.Name = "buttonVerify";
            this.buttonVerify.UseVisualStyleBackColor = true;
            this.buttonVerify.Click += new System.EventHandler(this.buttonVerify_Click);
            // 
            // buttonRunAsAdmin
            // 
            resources.ApplyResources(this.buttonRunAsAdmin, "buttonRunAsAdmin");
            this.buttonRunAsAdmin.Name = "buttonRunAsAdmin";
            this.buttonRunAsAdmin.UseVisualStyleBackColor = true;
            this.buttonRunAsAdmin.Click += new System.EventHandler(this.buttonRunAsAdmin_Click);
            // 
            // labelLoading
            // 
            resources.ApplyResources(this.labelLoading, "labelLoading");
            this.labelLoading.Name = "labelLoading";
            // 
            // StoreManageForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.Controls.Add(this.buttonRunAsAdmin);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.buttonVerify);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.labelLoading);
            this.Name = "StoreManageForm";
            this.ShowIcon = false;
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize) (this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.tableLayoutPanelSize.ResumeLayout(false);
            this.tableLayoutPanelSize.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonRunAsAdmin;
        private System.Windows.Forms.Button buttonVerify;
        private System.Windows.Forms.Label labelCheckedSize;
        private System.Windows.Forms.Label labelCurrentSize;
        private System.Windows.Forms.Label labelLoading;
        private System.Windows.Forms.Label labelTotalSize;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanelSize;
        private System.Windows.Forms.TextBox textCheckedSize;
        private System.Windows.Forms.TextBox textCurrentSize;
        private System.Windows.Forms.TextBox textTotalSize;
        #endregion
    }
}
