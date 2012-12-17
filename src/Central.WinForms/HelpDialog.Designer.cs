namespace ZeroInstall.Central.WinForms
{
    partial class HelpDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpDialog));
            this.buttonIntro = new System.Windows.Forms.Button();
            this.buttonDocumentation = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonIntro
            // 
            resources.ApplyResources(this.buttonIntro, "buttonIntro");
            this.buttonIntro.Name = "buttonIntro";
            this.buttonIntro.UseVisualStyleBackColor = true;
            this.buttonIntro.Click += new System.EventHandler(this.buttonIntro_Click);
            // 
            // buttonDocumentation
            // 
            resources.ApplyResources(this.buttonDocumentation, "buttonDocumentation");
            this.buttonDocumentation.Name = "buttonDocumentation";
            this.buttonDocumentation.UseVisualStyleBackColor = true;
            this.buttonDocumentation.Click += new System.EventHandler(this.buttonDocumentation_Click);
            // 
            // buttonClose
            // 
            resources.ApplyResources(this.buttonClose, "buttonClose");
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // HelpDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonDocumentation);
            this.Controls.Add(this.buttonIntro);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HelpDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonIntro;
        private System.Windows.Forms.Button buttonDocumentation;
        private System.Windows.Forms.Button buttonClose;
    }
}