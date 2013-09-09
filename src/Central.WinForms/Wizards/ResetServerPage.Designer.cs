namespace ZeroInstall.Central.WinForms.Wizards
{
    partial class ResetServerPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResetServerPage));
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelTitle = new System.Windows.Forms.Label();
            this.buttonReset = new System.Windows.Forms.Button();
            this.resetWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // labelInfo
            // 
            resources.ApplyResources(this.labelInfo, "labelInfo");
            this.labelInfo.Name = "labelInfo";
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.Name = "labelTitle";
            // 
            // buttonReset
            // 
            resources.ApplyResources(this.buttonReset, "buttonReset");
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // resetWorker
            // 
            this.resetWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.resetWorker_DoWork);
            this.resetWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.resetWorker_RunWorkerCompleted);
            // 
            // ResetServerPage
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelTitle);
            this.Name = "ResetServerPage";
            this.Controls.SetChildIndex(this.labelTitle, 0);
            this.Controls.SetChildIndex(this.labelInfo, 0);
            this.Controls.SetChildIndex(this.buttonReset, 0);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Button buttonReset;
        private System.ComponentModel.BackgroundWorker resetWorker;
    }
}
