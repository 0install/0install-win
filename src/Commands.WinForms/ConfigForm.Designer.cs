namespace ZeroInstall.Commands.WinForms
{
    partial class ConfigForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigForm));
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetValueMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            resources.ApplyResources(this.buttonOK, "buttonOK");
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            // 
            // propertyGrid
            // 
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.ContextMenuStrip = this.contextMenu;
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // contextMenu
            // 
            resources.ApplyResources(this.contextMenu, "contextMenu");
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetValueMenuItem});
            this.contextMenu.Name = "contextMenu";
            // 
            // resetValueMenuItem
            // 
            resources.ApplyResources(this.resetValueMenuItem, "resetValueMenuItem");
            this.resetValueMenuItem.Name = "resetValueMenuItem";
            this.resetValueMenuItem.Click += new System.EventHandler(this.resetValueMenuItem_Click);
            // 
            // ConfigForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.propertyGrid);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "ConfigForm";
            this.Controls.SetChildIndex(this.propertyGrid, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.contextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem resetValueMenuItem;
    }
}