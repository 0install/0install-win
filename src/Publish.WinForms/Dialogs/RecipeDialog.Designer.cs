namespace ZeroInstall.Publish.WinForms.Dialogs
{
    partial class RecipeDialog
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
            this.tabPageAddNew = new System.Windows.Forms.TabPage();
            this.buttonCreateRecipe = new System.Windows.Forms.Button();
            this.tabControlRecipe.SuspendLayout();
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
            this.tabControlRecipe.Controls.Add(this.tabPageAddNew);
            this.tabControlRecipe.Location = new System.Drawing.Point(12, 12);
            this.tabControlRecipe.Name = "tabControlRecipe";
            this.tabControlRecipe.SelectedIndex = 0;
            this.tabControlRecipe.Size = new System.Drawing.Size(279, 429);
            this.tabControlRecipe.TabIndex = 1002;
            this.tabControlRecipe.DoubleClick += new System.EventHandler(this.TabControlRecipeDoubleClick);
            this.tabControlRecipe.MouseClick += new System.Windows.Forms.MouseEventHandler(this.TabControlRecipeMouseClick);
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
            // buttonCreateRecipe
            // 
            this.buttonCreateRecipe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCreateRecipe.Location = new System.Drawing.Point(12, 447);
            this.buttonCreateRecipe.Name = "buttonCreateRecipe";
            this.buttonCreateRecipe.Size = new System.Drawing.Size(78, 23);
            this.buttonCreateRecipe.TabIndex = 1003;
            this.buttonCreateRecipe.Text = "Create recipe";
            this.buttonCreateRecipe.UseVisualStyleBackColor = true;
            this.buttonCreateRecipe.Click += new System.EventHandler(this.ButtonCreateRecipeClick);
            // 
            // RecipeDialog
            // 
            this.ClientSize = new System.Drawing.Size(303, 538);
            this.Controls.Add(this.buttonCreateRecipe);
            this.Controls.Add(this.tabControlRecipe);
            this.Name = "RecipeDialog";
            this.Text = "Edit recipe archives";
            this.Controls.SetChildIndex(this.tabControlRecipe, 0);
            this.Controls.SetChildIndex(this.buttonCreateRecipe, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.tabControlRecipe.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControlRecipe;
        private System.Windows.Forms.TabPage tabPageAddNew;
        private System.Windows.Forms.Button buttonCreateRecipe;
    }
}