namespace ZeroInstall.Commands.WinForms
{
    partial class SelectionsControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectionsControl));
            this.tableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.labelName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelTask = new System.Windows.Forms.Label();
            this.buttonDone = new System.Windows.Forms.Button();
            this.scrollPanel = new System.Windows.Forms.Panel();
            this.tableLayout.SuspendLayout();
            this.scrollPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayout
            // 
            resources.ApplyResources(this.tableLayout, "tableLayout");
            this.tableLayout.Controls.Add(this.labelName, 0, 0);
            this.tableLayout.Controls.Add(this.labelVersion, 1, 0);
            this.tableLayout.Controls.Add(this.labelTask, 2, 0);
            this.tableLayout.Name = "tableLayout";
            // 
            // labelName
            // 
            resources.ApplyResources(this.labelName, "labelName");
            this.labelName.Name = "labelName";
            // 
            // labelVersion
            // 
            resources.ApplyResources(this.labelVersion, "labelVersion");
            this.labelVersion.Name = "labelVersion";
            // 
            // labelTask
            // 
            resources.ApplyResources(this.labelTask, "labelTask");
            this.labelTask.Name = "labelTask";
            // 
            // buttonDone
            // 
            resources.ApplyResources(this.buttonDone, "buttonDone");
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // scrollPanel
            // 
            resources.ApplyResources(this.scrollPanel, "scrollPanel");
            this.scrollPanel.Controls.Add(this.tableLayout);
            this.scrollPanel.Name = "scrollPanel";
            // 
            // SelectionsControl
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.scrollPanel);
            this.Name = "SelectionsControl";
            this.tableLayout.ResumeLayout(false);
            this.tableLayout.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.scrollPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayout;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelTask;
        private System.Windows.Forms.Panel scrollPanel;

    }
}
