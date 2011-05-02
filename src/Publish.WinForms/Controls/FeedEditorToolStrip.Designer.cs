namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class FeedEditorToolStrip
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FeedEditorToolStrip));
            this.buttonNew = new System.Windows.Forms.ToolStripButton();
            this.buttonOpen = new System.Windows.Forms.ToolStripButton();
            this.buttonSave = new System.Windows.Forms.ToolStripButton();
            this.buttonSaveAs = new System.Windows.Forms.ToolStripButton();
            this.buttonUndo = new System.Windows.Forms.ToolStripButton();
            this.buttonRedo = new System.Windows.Forms.ToolStripButton();
            this.labelGnuPG = new System.Windows.Forms.ToolStripLabel();
            this.comboBoxGnuPG = new System.Windows.Forms.ToolStripComboBox();
            this.SuspendLayout();
            // 
            // buttonNew
            // 
            this.buttonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonNew.Image = ((System.Drawing.Image)(resources.GetObject("buttonNew.Image")));
            this.buttonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(23, 22);
            this.buttonNew.Text = "New";
            // 
            // buttonOpen
            // 
            this.buttonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonOpen.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.OpenButton;
            this.buttonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(23, 22);
            this.buttonOpen.Text = "Open";
            // 
            // buttonSave
            // 
            this.buttonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonSave.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.SaveButton;
            this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(23, 22);
            this.buttonSave.Text = "Save";
            // 
            // buttonSaveAs
            // 
            this.buttonSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("buttonSaveAs.Image")));
            this.buttonSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonSaveAs.Name = "buttonSaveAs";
            this.buttonSaveAs.Size = new System.Drawing.Size(23, 20);
            this.buttonSaveAs.Text = "SaveAs";
            // 
            // buttonUndo
            // 
            this.buttonUndo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonUndo.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.UndoButton;
            this.buttonUndo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Size = new System.Drawing.Size(23, 20);
            this.buttonUndo.Text = "Undo";
            // 
            // buttonRedo
            // 
            this.buttonRedo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRedo.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.RedoButton;
            this.buttonRedo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRedo.Name = "buttonRedo";
            this.buttonRedo.Size = new System.Drawing.Size(23, 20);
            this.buttonRedo.Text = "Redo";
            // 
            // labelGnuPG
            // 
            this.labelGnuPG.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.labelGnuPG.Name = "labelGnuPG";
            this.labelGnuPG.Size = new System.Drawing.Size(68, 15);
            this.labelGnuPG.Text = "GnuPG key:";
            // 
            // comboBoxGnuPG
            // 
            this.comboBoxGnuPG.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.comboBoxGnuPG.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGnuPG.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.comboBoxGnuPG.Name = "comboBoxGnuPG";
            this.comboBoxGnuPG.Size = new System.Drawing.Size(121, 23);
            // 
            // FeedEditorToolStrip
            // 
            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonNew,
            this.buttonOpen,
            this.buttonSave,
            this.buttonSaveAs,
            this.buttonUndo,
            this.buttonRedo,
            this.comboBoxGnuPG,
            this.labelGnuPG});
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripButton buttonNew;
        private System.Windows.Forms.ToolStripButton buttonOpen;
        private System.Windows.Forms.ToolStripButton buttonSave;
        private System.Windows.Forms.ToolStripButton buttonSaveAs;
        private System.Windows.Forms.ToolStripButton buttonUndo;
        private System.Windows.Forms.ToolStripButton buttonRedo;
        private System.Windows.Forms.ToolStripLabel labelGnuPG;
        private System.Windows.Forms.ToolStripComboBox comboBoxGnuPG;
    }
}
