namespace ZeroInstall.Publish.WinForms
{
    partial class WelcomeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeForm));
            this.labelIntro = new System.Windows.Forms.Label();
            this.labelNewEmpty = new System.Windows.Forms.Label();
            this.buttonNewEmpty = new System.Windows.Forms.Button();
            this.labelOpen = new System.Windows.Forms.Label();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.labelNewWizard = new System.Windows.Forms.Label();
            this.buttonNewWizard = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelIntro
            // 
            this.labelIntro.Location = new System.Drawing.Point(15, 16);
            this.labelIntro.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelIntro.Name = "labelIntro";
            this.labelIntro.Size = new System.Drawing.Size(447, 60);
            this.labelIntro.TabIndex = 0;
            this.labelIntro.Text = "Zero Install uses \"feeds\", XML files on the internet, to describe applications an" +
    "d how to download them.";
            // 
            // labelNewEmpty
            // 
            this.labelNewEmpty.Location = new System.Drawing.Point(16, 138);
            this.labelNewEmpty.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelNewEmpty.Name = "labelNewEmpty";
            this.labelNewEmpty.Size = new System.Drawing.Size(142, 111);
            this.labelNewEmpty.TabIndex = 2;
            this.labelNewEmpty.Text = "Create a blank feed to be filled manually. For advanced users.";
            // 
            // buttonNewEmpty
            // 
            this.buttonNewEmpty.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.NewButton;
            this.buttonNewEmpty.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonNewEmpty.Location = new System.Drawing.Point(15, 81);
            this.buttonNewEmpty.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonNewEmpty.Name = "buttonNewEmpty";
            this.buttonNewEmpty.Size = new System.Drawing.Size(142, 52);
            this.buttonNewEmpty.TabIndex = 1;
            this.buttonNewEmpty.Text = "&New Empty Feed";
            this.buttonNewEmpty.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonNewEmpty.UseVisualStyleBackColor = true;
            this.buttonNewEmpty.Click += new System.EventHandler(this.buttonNewEmpty_Click);
            // 
            // labelOpen
            // 
            this.labelOpen.Location = new System.Drawing.Point(321, 138);
            this.labelOpen.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelOpen.Name = "labelOpen";
            this.labelOpen.Size = new System.Drawing.Size(142, 111);
            this.labelOpen.TabIndex = 6;
            this.labelOpen.Text = "Open an existing feed for modification.";
            // 
            // buttonOpen
            // 
            this.buttonOpen.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.OpenButton;
            this.buttonOpen.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonOpen.Location = new System.Drawing.Point(320, 81);
            this.buttonOpen.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(142, 52);
            this.buttonOpen.TabIndex = 5;
            this.buttonOpen.Text = "&Open Feed";
            this.buttonOpen.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // labelNewWizard
            // 
            this.labelNewWizard.Location = new System.Drawing.Point(170, 138);
            this.labelNewWizard.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelNewWizard.Name = "labelNewWizard";
            this.labelNewWizard.Size = new System.Drawing.Size(142, 112);
            this.labelNewWizard.TabIndex = 4;
            this.labelNewWizard.Text = "Create a feed for an application step by step. Recommended for most users.";
            // 
            // buttonNewWizard
            // 
            this.buttonNewWizard.Image = global::ZeroInstall.Publish.WinForms.Properties.Resources.NewWizardButton;
            this.buttonNewWizard.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.buttonNewWizard.Location = new System.Drawing.Point(169, 81);
            this.buttonNewWizard.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonNewWizard.Name = "buttonNewWizard";
            this.buttonNewWizard.Size = new System.Drawing.Size(142, 52);
            this.buttonNewWizard.TabIndex = 3;
            this.buttonNewWizard.Text = "New &Feed Wizard";
            this.buttonNewWizard.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.buttonNewWizard.UseVisualStyleBackColor = true;
            this.buttonNewWizard.Click += new System.EventHandler(this.buttonNewWizard_Click);
            // 
            // WelcomeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(476, 259);
            this.Controls.Add(this.labelOpen);
            this.Controls.Add(this.buttonOpen);
            this.Controls.Add(this.labelNewWizard);
            this.Controls.Add(this.buttonNewWizard);
            this.Controls.Add(this.labelNewEmpty);
            this.Controls.Add(this.buttonNewEmpty);
            this.Controls.Add(this.labelIntro);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "WelcomeForm";
            this.Text = "Zero Install Publishing Tools";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelIntro;
        private System.Windows.Forms.Label labelNewEmpty;
        private System.Windows.Forms.Button buttonNewEmpty;
        private System.Windows.Forms.Label labelOpen;
        private System.Windows.Forms.Button buttonOpen;
        private System.Windows.Forms.Label labelNewWizard;
        private System.Windows.Forms.Button buttonNewWizard;
    }
}