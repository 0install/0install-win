using NanoByte.Common.Controls;

namespace ZeroInstall.Publish.WinForms.Controls
{
    partial class FeedEditor
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
            this.labelName = new System.Windows.Forms.Label();
            this.textBoxName = new HintTextBox();
            this.labelUri = new System.Windows.Forms.Label();
            this.textBoxUri = new UriTextBox();
            this.textBoxSummary = new LocalizableTextBox();
            this.textBoxDescription = new LocalizableTextBox();
            this.textBoxHomepage = new UriTextBox();
            this.labelHomepage = new System.Windows.Forms.Label();
            this.checkBoxNeedTerminal = new System.Windows.Forms.CheckBox();
            this.labelMinZeroInstallVersion = new System.Windows.Forms.Label();
            this.comboBoxMinimumZeroInstallVersion = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(0, 3);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(38, 13);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name:";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.HintText = "the application\'s name (required)";
            this.textBoxName.Location = new System.Drawing.Point(66, 0);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(84, 20);
            this.textBoxName.TabIndex = 1;
            // 
            // labelUri
            // 
            this.labelUri.AutoSize = true;
            this.labelUri.Location = new System.Drawing.Point(0, 29);
            this.labelUri.Name = "labelUri";
            this.labelUri.Size = new System.Drawing.Size(23, 13);
            this.labelUri.TabIndex = 2;
            this.labelUri.Text = "Uri:";
            // 
            // textBoxUri
            // 
            this.textBoxUri.AllowDrop = true;
            this.textBoxUri.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUri.HintText = "HTTP URI where the feed will be uploaded (required, unless local-only)";
            this.textBoxUri.HttpOnly = true;
            this.textBoxUri.Location = new System.Drawing.Point(66, 26);
            this.textBoxUri.Name = "textBoxUri";
            this.textBoxUri.Size = new System.Drawing.Size(84, 20);
            this.textBoxUri.TabIndex = 3;
            // 
            // textBoxSummary
            // 
            this.textBoxSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSummary.HintText = "Summary (required)";
            this.textBoxSummary.Location = new System.Drawing.Point(0, 52);
            this.textBoxSummary.MinimumSize = new System.Drawing.Size(65, 22);
            this.textBoxSummary.Multiline = false;
            this.textBoxSummary.Name = "textBoxSummary";
            this.textBoxSummary.Size = new System.Drawing.Size(150, 23);
            this.textBoxSummary.TabIndex = 4;
            // 
            // textBoxDescription
            // 
            this.textBoxDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDescription.HintText = "Description";
            this.textBoxDescription.Location = new System.Drawing.Point(0, 81);
            this.textBoxDescription.MinimumSize = new System.Drawing.Size(65, 22);
            this.textBoxDescription.Name = "textBoxDescription";
            this.textBoxDescription.Size = new System.Drawing.Size(150, 76);
            this.textBoxDescription.TabIndex = 5;
            // 
            // textBoxHomepage
            // 
            this.textBoxHomepage.AllowDrop = true;
            this.textBoxHomepage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHomepage.ForeColor = System.Drawing.Color.Empty;
            this.textBoxHomepage.HintText = "website with information about the application";
            this.textBoxHomepage.Location = new System.Drawing.Point(66, 163);
            this.textBoxHomepage.Name = "textBoxHomepage";
            this.textBoxHomepage.Size = new System.Drawing.Size(84, 20);
            this.textBoxHomepage.TabIndex = 7;
            // 
            // labelHomepage
            // 
            this.labelHomepage.AutoSize = true;
            this.labelHomepage.Location = new System.Drawing.Point(0, 166);
            this.labelHomepage.Name = "labelHomepage";
            this.labelHomepage.Size = new System.Drawing.Size(62, 13);
            this.labelHomepage.TabIndex = 6;
            this.labelHomepage.Text = "Homepage:";
            // 
            // checkBoxNeedTerminal
            // 
            this.checkBoxNeedTerminal.AutoSize = true;
            this.checkBoxNeedTerminal.Location = new System.Drawing.Point(3, 195);
            this.checkBoxNeedTerminal.Name = "checkBoxNeedTerminal";
            this.checkBoxNeedTerminal.Size = new System.Drawing.Size(233, 17);
            this.checkBoxNeedTerminal.TabIndex = 8;
            this.checkBoxNeedTerminal.Text = "Needs a terminal (command-line application)";
            this.checkBoxNeedTerminal.UseVisualStyleBackColor = true;
            // 
            // labelMinZeroInstallVersion
            // 
            this.labelMinZeroInstallVersion.Location = new System.Drawing.Point(0, 218);
            this.labelMinZeroInstallVersion.Name = "labelMinZeroInstallVersion";
            this.labelMinZeroInstallVersion.Size = new System.Drawing.Size(60, 43);
            this.labelMinZeroInstallVersion.TabIndex = 9;
            this.labelMinZeroInstallVersion.Text = "Minimum Zero Install version:";
            // 
            // comboBoxMinimumZeroInstallVersion
            // 
            this.comboBoxMinimumZeroInstallVersion.FormattingEnabled = true;
            this.comboBoxMinimumZeroInstallVersion.Items.AddRange(new object[] {
            "",
            "0.18",
            "0.20",
            "0.21",
            "0.24",
            "0.28",
            "0.48",
            "0.54",
            "1.0",
            "1.1",
            "1.2",
            "1.10",
            "1.12",
            "1.13",
            "1.15",
            "2.0",
            "2.1"});
            this.comboBoxMinimumZeroInstallVersion.Location = new System.Drawing.Point(66, 227);
            this.comboBoxMinimumZeroInstallVersion.Name = "comboBoxMinimumZeroInstallVersion";
            this.comboBoxMinimumZeroInstallVersion.Size = new System.Drawing.Size(84, 21);
            this.comboBoxMinimumZeroInstallVersion.TabIndex = 10;
            // 
            // FeedEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBoxMinimumZeroInstallVersion);
            this.Controls.Add(this.labelMinZeroInstallVersion);
            this.Controls.Add(this.checkBoxNeedTerminal);
            this.Controls.Add(this.textBoxHomepage);
            this.Controls.Add(this.labelHomepage);
            this.Controls.Add(this.textBoxSummary);
            this.Controls.Add(this.textBoxDescription);
            this.Controls.Add(this.textBoxUri);
            this.Controls.Add(this.labelUri);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.labelName);
            this.Name = "FeedEditor";
            this.Size = new System.Drawing.Size(150, 274);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelName;
        private HintTextBox textBoxName;
        private System.Windows.Forms.Label labelUri;
        private UriTextBox textBoxUri;
        private LocalizableTextBox textBoxSummary;
        private LocalizableTextBox textBoxDescription;
        private UriTextBox textBoxHomepage;
        private System.Windows.Forms.Label labelHomepage;
        private System.Windows.Forms.CheckBox checkBoxNeedTerminal;
        private System.Windows.Forms.Label labelMinZeroInstallVersion;
        private System.Windows.Forms.ComboBox comboBoxMinimumZeroInstallVersion;

    }
}
