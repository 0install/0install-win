namespace ZeroInstall.Central.WinForms.SyncConfig
{
    partial class SetupWelcomePage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupWelcomePage));
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelQuestion = new System.Windows.Forms.Label();
            this.buttonUsedBeforeYes = new System.Windows.Forms.Button();
            this.buttonUsedBeforeNo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            resources.ApplyResources(this.labelTitle, "labelTitle");
            this.labelTitle.Name = "labelTitle";
            // 
            // labelInfo
            // 
            resources.ApplyResources(this.labelInfo, "labelInfo");
            this.labelInfo.Name = "labelInfo";
            // 
            // labelQuestion
            // 
            resources.ApplyResources(this.labelQuestion, "labelQuestion");
            this.labelQuestion.Name = "labelQuestion";
            // 
            // buttonUsedBeforeYes
            // 
            resources.ApplyResources(this.buttonUsedBeforeYes, "buttonUsedBeforeYes");
            this.buttonUsedBeforeYes.Name = "buttonUsedBeforeYes";
            this.buttonUsedBeforeYes.UseVisualStyleBackColor = true;
            this.buttonUsedBeforeYes.Click += new System.EventHandler(this.buttonUsedBeforeYes_Click);
            // 
            // buttonUsedBeforeNo
            // 
            resources.ApplyResources(this.buttonUsedBeforeNo, "buttonUsedBeforeNo");
            this.buttonUsedBeforeNo.Name = "buttonUsedBeforeNo";
            this.buttonUsedBeforeNo.UseVisualStyleBackColor = true;
            this.buttonUsedBeforeNo.Click += new System.EventHandler(this.buttonUsedBeforeNo_Click);
            // 
            // SetupWelcomePage
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonUsedBeforeNo);
            this.Controls.Add(this.buttonUsedBeforeYes);
            this.Controls.Add(this.labelQuestion);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.labelTitle);
            this.Name = "SetupWelcomePage";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelQuestion;
        private System.Windows.Forms.Button buttonUsedBeforeYes;
        private System.Windows.Forms.Button buttonUsedBeforeNo;
    }
}
