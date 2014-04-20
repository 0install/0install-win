namespace ZeroInstall.Publish.WinForms.Wizards
{
    partial class SourcePage
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
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelQuestion = new System.Windows.Forms.Label();
            this.buttonArchive = new System.Windows.Forms.Button();
            this.buttonSingleFile = new System.Windows.Forms.Button();
            this.buttonSetup = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTitle
            // 
            this.labelTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold);
            this.labelTitle.Location = new System.Drawing.Point(0, 18);
            this.labelTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(470, 37);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "Application source";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelQuestion
            // 
            this.labelQuestion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.labelQuestion.Location = new System.Drawing.Point(35, 82);
            this.labelQuestion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelQuestion.Name = "labelQuestion";
            this.labelQuestion.Size = new System.Drawing.Size(400, 22);
            this.labelQuestion.TabIndex = 1;
            this.labelQuestion.Text = "How is the application distributed?";
            // 
            // buttonArchive
            // 
            this.buttonArchive.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonArchive.Location = new System.Drawing.Point(39, 118);
            this.buttonArchive.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonArchive.Name = "buttonArchive";
            this.buttonArchive.Size = new System.Drawing.Size(396, 35);
            this.buttonArchive.TabIndex = 2;
            this.buttonArchive.Text = "Archive (.zip, .tar, .msi, ...)";
            this.buttonArchive.UseVisualStyleBackColor = true;
            this.buttonArchive.Click += new System.EventHandler(this.buttonArchive_Click);
            // 
            // buttonSingleFile
            // 
            this.buttonSingleFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonSingleFile.Location = new System.Drawing.Point(39, 163);
            this.buttonSingleFile.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonSingleFile.Name = "buttonSingleFile";
            this.buttonSingleFile.Size = new System.Drawing.Size(396, 35);
            this.buttonSingleFile.TabIndex = 3;
            this.buttonSingleFile.Text = "Single Executable (.exe, .jar, ...)";
            this.buttonSingleFile.UseVisualStyleBackColor = true;
            this.buttonSingleFile.Click += new System.EventHandler(this.buttonSingleFile_Click);
            // 
            // buttonSetup
            // 
            this.buttonSetup.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.buttonSetup.Location = new System.Drawing.Point(39, 208);
            this.buttonSetup.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.buttonSetup.Name = "buttonSetup";
            this.buttonSetup.Size = new System.Drawing.Size(396, 35);
            this.buttonSetup.TabIndex = 4;
            this.buttonSetup.Text = "&Setup (.exe)";
            this.buttonSetup.UseVisualStyleBackColor = true;
            this.buttonSetup.Click += new System.EventHandler(this.buttonSetup_Click);
            // 
            // SourcePage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonSetup);
            this.Controls.Add(this.buttonSingleFile);
            this.Controls.Add(this.buttonArchive);
            this.Controls.Add(this.labelQuestion);
            this.Controls.Add(this.labelTitle);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "SourcePage";
            this.Size = new System.Drawing.Size(470, 300);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelQuestion;
        private System.Windows.Forms.Button buttonArchive;
        private System.Windows.Forms.Button buttonSingleFile;
        private System.Windows.Forms.Button buttonSetup;
    }
}
