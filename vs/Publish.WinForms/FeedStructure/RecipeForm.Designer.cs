namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class RecipeForm
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
            this.tabControlRecipe = new System.Windows.Forms.TabControl();
            this.tabPageArchive0 = new System.Windows.Forms.TabPage();
            this.archiveControl0 = new ZeroInstall.Publish.WinForms.Controls.ArchiveControl();
            this.tabPageAddNew = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.tabControlRecipe.SuspendLayout();
            this.tabPageArchive0.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(135, 503);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(216, 503);
            // 
            // tabControlRecipe
            // 
            this.tabControlRecipe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlRecipe.Controls.Add(this.tabPageArchive0);
            this.tabControlRecipe.Controls.Add(this.tabPageAddNew);
            this.tabControlRecipe.Location = new System.Drawing.Point(12, 12);
            this.tabControlRecipe.Name = "tabControlRecipe";
            this.tabControlRecipe.SelectedIndex = 0;
            this.tabControlRecipe.Size = new System.Drawing.Size(279, 429);
            this.tabControlRecipe.TabIndex = 1002;
            this.tabControlRecipe.DoubleClick += new System.EventHandler(this.TabControlRecipeDoubleClick);
            this.tabControlRecipe.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TabControlRecipeMouseClick);
            // 
            // tabPageArchive0
            // 
            this.tabPageArchive0.Controls.Add(this.archiveControl0);
            this.tabPageArchive0.Location = new System.Drawing.Point(4, 22);
            this.tabPageArchive0.Name = "tabPageArchive0";
            this.tabPageArchive0.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageArchive0.Size = new System.Drawing.Size(271, 403);
            this.tabPageArchive0.TabIndex = 0;
            this.tabPageArchive0.Text = "Archive";
            this.tabPageArchive0.UseVisualStyleBackColor = true;
            // 
            // archiveControl0
            // 
            this.archiveControl0.Location = new System.Drawing.Point(6, 6);
            this.archiveControl0.Name = "archiveControl0";
            this.archiveControl0.Size = new System.Drawing.Size(259, 396);
            this.archiveControl0.TabIndex = 0;
            // 
            // tabPageAddNew
            // 
            this.tabPageAddNew.Location = new System.Drawing.Point(4, 22);
            this.tabPageAddNew.Name = "tabPageAddNew";
            this.tabPageAddNew.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAddNew.Size = new System.Drawing.Size(271, 403);
            this.tabPageAddNew.TabIndex = 1;
            this.tabPageAddNew.Text = "    +";
            this.tabPageAddNew.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(12, 447);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(78, 23);
            this.button1.TabIndex = 1003;
            this.button1.Text = "Create recipe";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // RecipeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(303, 538);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tabControlRecipe);
            this.Name = "RecipeForm";
            this.Text = "Edit recipe archives";
            this.Controls.SetChildIndex(this.tabControlRecipe, 0);
            this.Controls.SetChildIndex(this.button1, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabControlRecipe.ResumeLayout(false);
            this.tabPageArchive0.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlRecipe;
        private System.Windows.Forms.TabPage tabPageArchive0;
        private System.Windows.Forms.TabPage tabPageAddNew;
        private System.Windows.Forms.Button button1;
        private Controls.ArchiveControl archiveControl0;
    }
}