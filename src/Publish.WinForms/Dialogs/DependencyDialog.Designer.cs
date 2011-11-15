namespace ZeroInstall.Publish.WinForms.Dialogs
{
    partial class DependencyDialog
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
            this.labelImportance = new System.Windows.Forms.Label();
            this.comboBoxImportance = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(156, 239);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(237, 239);
            // 
            // hintTextBoxInterface
            // 
            this.hintTextBoxInterface.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxInterface.HintText = "";
            this.hintTextBoxInterface.Location = new System.Drawing.Point(15, 25);
            this.hintTextBoxInterface.Name = "hintTextBoxInterface";
            this.hintTextBoxInterface.Size = new System.Drawing.Size(297, 20);
            this.hintTextBoxInterface.TabIndex = 1;
            this.hintTextBoxInterface.TextChanged += new System.EventHandler(this.HintTextBoxInterfaceTextChanged);
            // 
            // labelInterface
            // 
            this.labelInterface.AutoSize = true;
            this.labelInterface.Location = new System.Drawing.Point(12, 9);
            this.labelInterface.Name = "labelInterface";
            this.labelInterface.Size = new System.Drawing.Size(49, 13);
            this.labelInterface.TabIndex = 0;
            this.labelInterface.Text = "Interface";
            // 
            // labelUse
            // 
            this.labelUse.AutoSize = true;
            this.labelUse.Location = new System.Drawing.Point(12, 48);
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
            this.hintTextBoxUse.Location = new System.Drawing.Point(15, 64);
            this.hintTextBoxUse.Name = "hintTextBoxUse";
            this.hintTextBoxUse.Size = new System.Drawing.Size(297, 20);
            this.hintTextBoxUse.TabIndex = 3;
            // 
            // listBoxConstraints
            // 
            this.listBoxConstraints.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxConstraints.FormattingEnabled = true;
            this.listBoxConstraints.Location = new System.Drawing.Point(15, 129);
            this.listBoxConstraints.Name = "listBoxConstraints";
            this.listBoxConstraints.Size = new System.Drawing.Size(216, 56);
            this.listBoxConstraints.TabIndex = 9;
            this.listBoxConstraints.SelectedIndexChanged += new System.EventHandler(this.ListBoxConstraintsSelectedIndexChanged);
            // 
            // hintTextBoxNotBefore
            // 
            this.hintTextBoxNotBefore.HintText = "";
            this.hintTextBoxNotBefore.Location = new System.Drawing.Point(15, 103);
            this.hintTextBoxNotBefore.Name = "hintTextBoxNotBefore";
            this.hintTextBoxNotBefore.Size = new System.Drawing.Size(110, 20);
            this.hintTextBoxNotBefore.TabIndex = 5;
            this.hintTextBoxNotBefore.TextChanged += new System.EventHandler(this.HintTextBoxNotBeforeTextChanged);
            // 
            // labelNotBefore
            // 
            this.labelNotBefore.AutoSize = true;
            this.labelNotBefore.Location = new System.Drawing.Point(12, 87);
            this.labelNotBefore.Name = "labelNotBefore";
            this.labelNotBefore.Size = new System.Drawing.Size(58, 13);
            this.labelNotBefore.TabIndex = 4;
            this.labelNotBefore.Text = "Not Before";
            // 
            // hintTextBoxBefore
            // 
            this.hintTextBoxBefore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxBefore.HintText = "";
            this.hintTextBoxBefore.Location = new System.Drawing.Point(203, 103);
            this.hintTextBoxBefore.Name = "hintTextBoxBefore";
            this.hintTextBoxBefore.Size = new System.Drawing.Size(109, 20);
            this.hintTextBoxBefore.TabIndex = 8;
            this.hintTextBoxBefore.TextChanged += new System.EventHandler(this.HintTextBoxBeforeTextChanged);
            // 
            // labelBefore
            // 
            this.labelBefore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelBefore.AutoSize = true;
            this.labelBefore.Location = new System.Drawing.Point(200, 87);
            this.labelBefore.Name = "labelBefore";
            this.labelBefore.Size = new System.Drawing.Size(38, 13);
            this.labelBefore.TabIndex = 7;
            this.labelBefore.Text = "Before";
            // 
            // buttonConstraintAdd
            // 
            this.buttonConstraintAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConstraintAdd.Location = new System.Drawing.Point(237, 133);
            this.buttonConstraintAdd.Name = "buttonConstraintAdd";
            this.buttonConstraintAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonConstraintAdd.TabIndex = 10;
            this.buttonConstraintAdd.Text = "Add";
            this.buttonConstraintAdd.UseVisualStyleBackColor = true;
            this.buttonConstraintAdd.Click += new System.EventHandler(this.ButtonConstraintAddClick);
            // 
            // buttonConstraintRemove
            // 
            this.buttonConstraintRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConstraintRemove.Location = new System.Drawing.Point(237, 162);
            this.buttonConstraintRemove.Name = "buttonConstraintRemove";
            this.buttonConstraintRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonConstraintRemove.TabIndex = 11;
            this.buttonConstraintRemove.Text = "Remove";
            this.buttonConstraintRemove.UseVisualStyleBackColor = true;
            this.buttonConstraintRemove.Click += new System.EventHandler(this.ButtonConstraintRemoveClick);
            // 
            // labelVersion
            // 
            this.labelVersion.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(132, 106);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(65, 13);
            this.labelVersion.TabIndex = 6;
            this.labelVersion.Text = "<= version <";
            // 
            // labelImportance
            // 
            this.labelImportance.AutoSize = true;
            this.labelImportance.Location = new System.Drawing.Point(12, 188);
            this.labelImportance.Name = "labelImportance";
            this.labelImportance.Size = new System.Drawing.Size(60, 13);
            this.labelImportance.TabIndex = 12;
            this.labelImportance.Text = "Importance";
            // 
            // comboBoxImportance
            // 
            this.comboBoxImportance.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxImportance.FormattingEnabled = true;
            this.comboBoxImportance.Location = new System.Drawing.Point(15, 204);
            this.comboBoxImportance.Name = "comboBoxImportance";
            this.comboBoxImportance.Size = new System.Drawing.Size(112, 21);
            this.comboBoxImportance.TabIndex = 13;
            // 
            // DependencyDialog
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 274);
            this.Controls.Add(this.labelImportance);
            this.Controls.Add(this.comboBoxImportance);
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
            this.Name = "DependencyDialog";
            this.Text = "Edit dependency";
            this.Controls.SetChildIndex(this.hintTextBoxInterface, 0);
            this.Controls.SetChildIndex(this.labelInterface, 0);
            this.Controls.SetChildIndex(this.labelUse, 0);
            this.Controls.SetChildIndex(this.hintTextBoxUse, 0);
            this.Controls.SetChildIndex(this.listBoxConstraints, 0);
            this.Controls.SetChildIndex(this.hintTextBoxNotBefore, 0);
            this.Controls.SetChildIndex(this.labelNotBefore, 0);
            this.Controls.SetChildIndex(this.hintTextBoxBefore, 0);
            this.Controls.SetChildIndex(this.labelBefore, 0);
            this.Controls.SetChildIndex(this.buttonConstraintAdd, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.buttonConstraintRemove, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.labelVersion, 0);
            this.Controls.SetChildIndex(this.comboBoxImportance, 0);
            this.Controls.SetChildIndex(this.labelImportance, 0);
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
        protected System.Windows.Forms.Label labelImportance;
        protected System.Windows.Forms.ComboBox comboBoxImportance;
    }
}
