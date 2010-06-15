namespace ZeroInstall.Publish.WinForms
{
    partial class DependencyForm
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
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hintTextBoxInterface
            // 
            this.hintTextBoxInterface.HintText = "";
            this.hintTextBoxInterface.Location = new System.Drawing.Point(15, 25);
            this.hintTextBoxInterface.Name = "hintTextBoxInterface";
            this.hintTextBoxInterface.Size = new System.Drawing.Size(297, 20);
            this.hintTextBoxInterface.TabIndex = 1;
            this.hintTextBoxInterface.TextChanged += new System.EventHandler(this.hintTextBoxInterface_TextChanged);
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
            this.hintTextBoxUse.HintText = "";
            this.hintTextBoxUse.Location = new System.Drawing.Point(15, 64);
            this.hintTextBoxUse.Name = "hintTextBoxUse";
            this.hintTextBoxUse.Size = new System.Drawing.Size(297, 20);
            this.hintTextBoxUse.TabIndex = 3;
            // 
            // listBoxConstraints
            // 
            this.listBoxConstraints.FormattingEnabled = true;
            this.listBoxConstraints.Location = new System.Drawing.Point(16, 129);
            this.listBoxConstraints.Name = "listBoxConstraints";
            this.listBoxConstraints.Size = new System.Drawing.Size(215, 56);
            this.listBoxConstraints.TabIndex = 9;
            this.listBoxConstraints.SelectedIndexChanged += new System.EventHandler(this.listBoxConstraints_SelectedIndexChanged);
            // 
            // hintTextBoxNotBefore
            // 
            this.hintTextBoxNotBefore.HintText = "";
            this.hintTextBoxNotBefore.Location = new System.Drawing.Point(16, 103);
            this.hintTextBoxNotBefore.Name = "hintTextBoxNotBefore";
            this.hintTextBoxNotBefore.Size = new System.Drawing.Size(110, 20);
            this.hintTextBoxNotBefore.TabIndex = 5;
            this.hintTextBoxNotBefore.TextChanged += new System.EventHandler(this.hintTextBoxNotBefore_TextChanged);
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
            this.hintTextBoxBefore.HintText = "";
            this.hintTextBoxBefore.Location = new System.Drawing.Point(203, 103);
            this.hintTextBoxBefore.Name = "hintTextBoxBefore";
            this.hintTextBoxBefore.Size = new System.Drawing.Size(109, 20);
            this.hintTextBoxBefore.TabIndex = 8;
            this.hintTextBoxBefore.TextChanged += new System.EventHandler(this.hintTextBoxBefore_TextChanged);
            // 
            // labelBefore
            // 
            this.labelBefore.AutoSize = true;
            this.labelBefore.Location = new System.Drawing.Point(200, 87);
            this.labelBefore.Name = "labelBefore";
            this.labelBefore.Size = new System.Drawing.Size(38, 13);
            this.labelBefore.TabIndex = 7;
            this.labelBefore.Text = "Before";
            // 
            // buttonConstraintAdd
            // 
            this.buttonConstraintAdd.Location = new System.Drawing.Point(237, 133);
            this.buttonConstraintAdd.Name = "buttonConstraintAdd";
            this.buttonConstraintAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonConstraintAdd.TabIndex = 10;
            this.buttonConstraintAdd.Text = "Add";
            this.buttonConstraintAdd.UseVisualStyleBackColor = true;
            this.buttonConstraintAdd.Click += new System.EventHandler(this.buttonConstraintAdd_Click);
            // 
            // buttonConstraintRemove
            // 
            this.buttonConstraintRemove.Location = new System.Drawing.Point(237, 162);
            this.buttonConstraintRemove.Name = "buttonConstraintRemove";
            this.buttonConstraintRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonConstraintRemove.TabIndex = 11;
            this.buttonConstraintRemove.Text = "Remove";
            this.buttonConstraintRemove.UseVisualStyleBackColor = true;
            this.buttonConstraintRemove.Click += new System.EventHandler(this.buttonConstraintRemove_Click);
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(132, 106);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(65, 13);
            this.labelVersion.TabIndex = 6;
            this.labelVersion.Text = "<= version <";
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(236, 220);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 13;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(155, 220);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // DependencyForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(328, 255);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
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
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DependencyForm";
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
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}
