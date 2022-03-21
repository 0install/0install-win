namespace ZeroInstall.Central.WinForms
{
    partial class PortableCreatorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PortableCreatorDialog));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.textBoxTarget = new NanoByte.Common.Controls.HintTextBox();
            this.buttonDeploy = new System.Windows.Forms.Button();
            this.buttonSelectTarget = new System.Windows.Forms.Button();
            this.labelInfo2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            resources.ApplyResources(this.buttonCancel, "buttonCancel");
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelInfo
            // 
            resources.ApplyResources(this.labelInfo, "labelInfo");
            this.labelInfo.Name = "labelInfo";
            // 
            // textBoxTarget
            // 
            resources.ApplyResources(this.textBoxTarget, "textBoxTarget");
            this.textBoxTarget.Name = "textBoxTarget";
            this.textBoxTarget.TextChanged += new System.EventHandler(this.textBoxTarget_TextChanged);
            // 
            // buttonDeploy
            // 
            resources.ApplyResources(this.buttonDeploy, "buttonDeploy");
            this.buttonDeploy.Name = "buttonDeploy";
            this.buttonDeploy.UseVisualStyleBackColor = true;
            this.buttonDeploy.Click += new System.EventHandler(this.buttonDeploy_Click);
            // 
            // buttonSelectTarget
            // 
            resources.ApplyResources(this.buttonSelectTarget, "buttonSelectTarget");
            this.buttonSelectTarget.Name = "buttonSelectTarget";
            this.buttonSelectTarget.UseVisualStyleBackColor = true;
            this.buttonSelectTarget.Click += new System.EventHandler(this.buttonSelectTarget_Click);
            // 
            // labelInfo2
            // 
            resources.ApplyResources(this.labelInfo2, "labelInfo2");
            this.labelInfo2.Name = "labelInfo2";
            // 
            // PortableCreatorDialog
            // 
            this.AcceptButton = this.buttonDeploy;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.Controls.Add(this.labelInfo2);
            this.Controls.Add(this.buttonSelectTarget);
            this.Controls.Add(this.buttonDeploy);
            this.Controls.Add(this.textBoxTarget);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PortableCreatorDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.PortableCreatorDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelInfo;
        private NanoByte.Common.Controls.HintTextBox textBoxTarget;
        private System.Windows.Forms.Button buttonDeploy;
        private System.Windows.Forms.Button buttonSelectTarget;
        private System.Windows.Forms.Label labelInfo2;
    }
}
