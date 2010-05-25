namespace ZeroInstall.Publish.WinForms
{
    partial class DependencyControl
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
            this.hintTextBoxInterface = new Common.Controls.HintTextBox();
            this.labelInterface = new System.Windows.Forms.Label();
            this.labelUse = new System.Windows.Forms.Label();
            this.hintTextBoxUse = new Common.Controls.HintTextBox();
            this.listBoxConstraints = new System.Windows.Forms.ListBox();
            this.hintTextBoxNotBefore = new Common.Controls.HintTextBox();
            this.labelNotBefore = new System.Windows.Forms.Label();
            this.hintTextBoxBefore = new Common.Controls.HintTextBox();
            this.labelBefore = new System.Windows.Forms.Label();
            this.buttonConstraintAdd = new System.Windows.Forms.Button();
            this.buttonConstraintRemove = new System.Windows.Forms.Button();
            this.labelVersion = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // hintTextBoxInterface
            // 
            this.hintTextBoxInterface.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxInterface.HintText = "";
            this.hintTextBoxInterface.Location = new System.Drawing.Point(7, 20);
            this.hintTextBoxInterface.Name = "hintTextBoxInterface";
            this.hintTextBoxInterface.Size = new System.Drawing.Size(296, 20);
            this.hintTextBoxInterface.TabIndex = 0;
            this.hintTextBoxInterface.TextChanged += new System.EventHandler(this.hintTextBoxInterface_TextChanged);
            // 
            // labelInterface
            // 
            this.labelInterface.AutoSize = true;
            this.labelInterface.Location = new System.Drawing.Point(4, 4);
            this.labelInterface.Name = "labelInterface";
            this.labelInterface.Size = new System.Drawing.Size(49, 13);
            this.labelInterface.TabIndex = 1;
            this.labelInterface.Text = "Interface";
            // 
            // labelUse
            // 
            this.labelUse.AutoSize = true;
            this.labelUse.Location = new System.Drawing.Point(3, 43);
            this.labelUse.Name = "labelUse";
            this.labelUse.Size = new System.Drawing.Size(26, 13);
            this.labelUse.TabIndex = 2;
            this.labelUse.Text = "Use";
            // 
            // hintTextBoxUse
            // 
            this.hintTextBoxUse.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxUse.HintText = "";
            this.hintTextBoxUse.Location = new System.Drawing.Point(7, 59);
            this.hintTextBoxUse.Name = "hintTextBoxUse";
            this.hintTextBoxUse.Size = new System.Drawing.Size(296, 20);
            this.hintTextBoxUse.TabIndex = 3;
            this.hintTextBoxUse.TextChanged += new System.EventHandler(this.hintTextBoxUse_TextChanged);
            // 
            // listBoxConstraints
            // 
            this.listBoxConstraints.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxConstraints.FormattingEnabled = true;
            this.listBoxConstraints.Location = new System.Drawing.Point(7, 124);
            this.listBoxConstraints.Name = "listBoxConstraints";
            this.listBoxConstraints.Size = new System.Drawing.Size(215, 56);
            this.listBoxConstraints.TabIndex = 4;
            this.listBoxConstraints.SelectedIndexChanged += new System.EventHandler(this.listBoxConstraints_SelectedIndexChanged);
            // 
            // hintTextBoxNotBefore
            // 
            this.hintTextBoxNotBefore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxNotBefore.HintText = "";
            this.hintTextBoxNotBefore.Location = new System.Drawing.Point(7, 98);
            this.hintTextBoxNotBefore.Name = "hintTextBoxNotBefore";
            this.hintTextBoxNotBefore.Size = new System.Drawing.Size(110, 20);
            this.hintTextBoxNotBefore.TabIndex = 5;
            this.hintTextBoxNotBefore.TextChanged += new System.EventHandler(this.hintTextBoxNotBefore_TextChanged);
            // 
            // labelNotBefore
            // 
            this.labelNotBefore.AutoSize = true;
            this.labelNotBefore.Location = new System.Drawing.Point(3, 82);
            this.labelNotBefore.Name = "labelNotBefore";
            this.labelNotBefore.Size = new System.Drawing.Size(58, 13);
            this.labelNotBefore.TabIndex = 6;
            this.labelNotBefore.Text = "Not Before";
            // 
            // hintTextBoxBefore
            // 
            this.hintTextBoxBefore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxBefore.HintText = "";
            this.hintTextBoxBefore.Location = new System.Drawing.Point(194, 98);
            this.hintTextBoxBefore.Name = "hintTextBoxBefore";
            this.hintTextBoxBefore.Size = new System.Drawing.Size(109, 20);
            this.hintTextBoxBefore.TabIndex = 7;
            this.hintTextBoxBefore.TextChanged += new System.EventHandler(this.hintTextBoxBefore_TextChanged);
            // 
            // labelBefore
            // 
            this.labelBefore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBefore.AutoSize = true;
            this.labelBefore.Location = new System.Drawing.Point(191, 82);
            this.labelBefore.Name = "labelBefore";
            this.labelBefore.Size = new System.Drawing.Size(38, 13);
            this.labelBefore.TabIndex = 8;
            this.labelBefore.Text = "Before";
            // 
            // buttonConstraintAdd
            // 
            this.buttonConstraintAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConstraintAdd.Location = new System.Drawing.Point(228, 128);
            this.buttonConstraintAdd.Name = "buttonConstraintAdd";
            this.buttonConstraintAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonConstraintAdd.TabIndex = 9;
            this.buttonConstraintAdd.Text = "Add";
            this.buttonConstraintAdd.UseVisualStyleBackColor = true;
            this.buttonConstraintAdd.Click += new System.EventHandler(this.buttonConstraintAdd_Click);
            // 
            // buttonConstraintRemove
            // 
            this.buttonConstraintRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConstraintRemove.Location = new System.Drawing.Point(228, 157);
            this.buttonConstraintRemove.Name = "buttonConstraintRemove";
            this.buttonConstraintRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonConstraintRemove.TabIndex = 10;
            this.buttonConstraintRemove.Text = "Remove";
            this.buttonConstraintRemove.UseVisualStyleBackColor = true;
            this.buttonConstraintRemove.Click += new System.EventHandler(this.buttonConstraintRemove_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(123, 101);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(65, 13);
            this.labelVersion.TabIndex = 12;
            this.labelVersion.Text = "<= version <";
            // 
            // DependencyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.buttonConstraintRemove);
            this.Controls.Add(this.buttonConstraintAdd);
            this.Controls.Add(this.labelBefore);
            this.Controls.Add(this.hintTextBoxBefore);
            this.Controls.Add(this.labelNotBefore);
            this.Controls.Add(this.hintTextBoxNotBefore);
            this.Controls.Add(this.listBoxConstraints);
            this.Controls.Add(this.hintTextBoxUse);
            this.Controls.Add(this.labelUse);
            this.Controls.Add(this.labelInterface);
            this.Controls.Add(this.hintTextBoxInterface);
            this.Name = "DependencyControl";
            this.Size = new System.Drawing.Size(311, 185);
            this.Enter += new System.EventHandler(this.DependencyControl_Enter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Common.Controls.HintTextBox hintTextBoxInterface;
        private System.Windows.Forms.Label labelInterface;
        private System.Windows.Forms.Label labelUse;
        private Common.Controls.HintTextBox hintTextBoxUse;
        private System.Windows.Forms.ListBox listBoxConstraints;
        private Common.Controls.HintTextBox hintTextBoxNotBefore;
        private System.Windows.Forms.Label labelNotBefore;
        private Common.Controls.HintTextBox hintTextBoxBefore;
        private System.Windows.Forms.Label labelBefore;
        private System.Windows.Forms.Button buttonConstraintAdd;
        private System.Windows.Forms.Button buttonConstraintRemove;
        private System.Windows.Forms.Label labelVersion;
    }
}
