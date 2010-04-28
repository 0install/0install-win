namespace ZeroInstall.FeedEditor
{
    partial class PackageImplementationControl
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
            this.labelPackage = new System.Windows.Forms.Label();
            this.hintTextBoxPackage = new Common.Controls.HintTextBox();
            this.checkedListBoxDistribution = new System.Windows.Forms.CheckedListBox();
            this.labelDistributions = new System.Windows.Forms.Label();
            this.labelMain = new System.Windows.Forms.Label();
            this.hintTextBoxMain = new Common.Controls.HintTextBox();
            this.hintTextBoxDocDir = new Common.Controls.HintTextBox();
            this.labelDocDir = new System.Windows.Forms.Label();
            this.comboBoxLicense = new System.Windows.Forms.ComboBox();
            this.labelLicense = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelPackage
            // 
            this.labelPackage.AutoSize = true;
            this.labelPackage.Location = new System.Drawing.Point(1, 39);
            this.labelPackage.Name = "labelPackage";
            this.labelPackage.Size = new System.Drawing.Size(79, 13);
            this.labelPackage.TabIndex = 0;
            this.labelPackage.Text = "Package name";
            // 
            // hintTextBoxPackage
            // 
            this.hintTextBoxPackage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxPackage.HintText = "Package name in the selected distributions";
            this.hintTextBoxPackage.Location = new System.Drawing.Point(3, 55);
            this.hintTextBoxPackage.Name = "hintTextBoxPackage";
            this.hintTextBoxPackage.Size = new System.Drawing.Size(340, 20);
            this.hintTextBoxPackage.TabIndex = 1;
            this.hintTextBoxPackage.TextChanged += new System.EventHandler(this.hintTextBoxPackage_TextChanged);
            // 
            // checkedListBoxDistribution
            // 
            this.checkedListBoxDistribution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxDistribution.FormattingEnabled = true;
            this.checkedListBoxDistribution.Items.AddRange(new object[] {
            "RPM",
            "Debian",
            "Gentoo"});
            this.checkedListBoxDistribution.Location = new System.Drawing.Point(280, 94);
            this.checkedListBoxDistribution.Name = "checkedListBoxDistribution";
            this.checkedListBoxDistribution.Size = new System.Drawing.Size(63, 49);
            this.checkedListBoxDistribution.TabIndex = 2;
            this.checkedListBoxDistribution.SelectedIndexChanged += new System.EventHandler(this.checkedListBoxDistribution_SelectedIndexChanged);
            // 
            // labelDistributions
            // 
            this.labelDistributions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDistributions.AutoSize = true;
            this.labelDistributions.Location = new System.Drawing.Point(277, 78);
            this.labelDistributions.Name = "labelDistributions";
            this.labelDistributions.Size = new System.Drawing.Size(64, 13);
            this.labelDistributions.TabIndex = 3;
            this.labelDistributions.Text = "Distributions";
            // 
            // labelMain
            // 
            this.labelMain.AutoSize = true;
            this.labelMain.Location = new System.Drawing.Point(1, 0);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(30, 13);
            this.labelMain.TabIndex = 4;
            this.labelMain.Text = "Main";
            // 
            // hintTextBoxMain
            // 
            this.hintTextBoxMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxMain.HintText = "Absolute path to the main";
            this.hintTextBoxMain.Location = new System.Drawing.Point(3, 16);
            this.hintTextBoxMain.Name = "hintTextBoxMain";
            this.hintTextBoxMain.Size = new System.Drawing.Size(340, 20);
            this.hintTextBoxMain.TabIndex = 5;
            this.hintTextBoxMain.TextChanged += new System.EventHandler(this.hintTextBoxMain_TextChanged);
            // 
            // hintTextBoxDocDir
            // 
            this.hintTextBoxDocDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxDocDir.HintText = "Relative path of a directory inside the implementation";
            this.hintTextBoxDocDir.Location = new System.Drawing.Point(3, 134);
            this.hintTextBoxDocDir.Name = "hintTextBoxDocDir";
            this.hintTextBoxDocDir.Size = new System.Drawing.Size(271, 20);
            this.hintTextBoxDocDir.TabIndex = 43;
            this.hintTextBoxDocDir.TextChanged += new System.EventHandler(this.hintTextBoxDocDir_TextChanged);
            // 
            // labelDocDir
            // 
            this.labelDocDir.AutoSize = true;
            this.labelDocDir.Location = new System.Drawing.Point(1, 118);
            this.labelDocDir.Name = "labelDocDir";
            this.labelDocDir.Size = new System.Drawing.Size(122, 13);
            this.labelDocDir.TabIndex = 42;
            this.labelDocDir.Text = "Documentation directory";
            // 
            // comboBoxLicense
            // 
            this.comboBoxLicense.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLicense.FormattingEnabled = true;
            this.comboBoxLicense.Items.AddRange(new object[] {
            "AFL (Academic Free License)",
            "AFPL (Aladdin Free Public License)",
            "AGPL v1 (Affero General Public License)",
            "AGPL v2 (Affero General Public License)",
            "AGPL v3 (Affero General Public License)",
            "APL (Adaptive Public License)",
            "APSL v1 (Apple Public Source License)",
            "APSL v2 (Apple Public Source License)",
            "Artistic License",
            "BSD License (original)",
            "BSD License (revised)",
            "CDDL (Common Development and Distribution License)",
            "Common Public License",
            "Copyback License",
            "CVW (MITRE Collaborative Virtual Workspace License)",
            "DFSG approved (Debian Free Software Guidelines)",
            "EFL (Eiffel Forum License)",
            "EPL (Eclipse Public License)",
            "FDL (GNU Free Documentation License)",
            "Free for educational use",
            "Free for home use",
            "Free for non-commercial use",
            "Free to use but restricted",
            "Freely distributable",
            "Freeware",
            "GMGPL (GNAT Modified GPL)",
            "GPL v1 (GNU General Public License)",
            "GPL v2 (GNU General Public License)",
            "GPL v3 (GNU General Public License)",
            "Guile License",
            "IBM Public License",
            "LGPL (GNU Lesser General Public License)",
            "LPPL (The Latex Project Public License)",
            "MIT/X Consortium License",
            "MPL (Mozilla Public License)",
            "NOKOS (Nokia Open Source License)",
            "NPL (Netscape Public License)",
            "Open Software License",
            "OSI approved",
            "Other / Proprietary License",
            "Other / Proprietary License with Free Trial",
            "Other / Proprietary License with Source",
            "Perl License",
            "Public Domain",
            "Python License",
            "QPL (Q Public License)",
            "Ricoh Source Code Public License",
            "Shareware",
            "SUN Binary Code License",
            "SUN Community Source License",
            "SUN Public License",
            "The Apache License v1",
            "The Apache License v2",
            "The CeCILL License",
            "The Clarified Artistic License",
            "The Open Content License",
            "The PHP License",
            "VPL (Voxel Public License)",
            "W3C License",
            "WTFPL v1 (Do What The Fuck You Want To Public License)",
            "WTFPL v2 (Do What The Fuck You Want To Public License)",
            "zlib/libpng License",
            "ZPL (Zope Public License)"});
            this.comboBoxLicense.Location = new System.Drawing.Point(3, 94);
            this.comboBoxLicense.Name = "comboBoxLicense";
            this.comboBoxLicense.Size = new System.Drawing.Size(271, 21);
            this.comboBoxLicense.Sorted = true;
            this.comboBoxLicense.TabIndex = 35;
            this.comboBoxLicense.SelectedIndexChanged += new System.EventHandler(this.comboBoxLicense_SelectedIndexChanged);
            // 
            // labelLicense
            // 
            this.labelLicense.AutoSize = true;
            this.labelLicense.Location = new System.Drawing.Point(1, 78);
            this.labelLicense.Name = "labelLicense";
            this.labelLicense.Size = new System.Drawing.Size(44, 13);
            this.labelLicense.TabIndex = 34;
            this.labelLicense.Text = "License";
            // 
            // PackageImplementationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hintTextBoxDocDir);
            this.Controls.Add(this.labelDocDir);
            this.Controls.Add(this.comboBoxLicense);
            this.Controls.Add(this.labelLicense);
            this.Controls.Add(this.hintTextBoxMain);
            this.Controls.Add(this.labelMain);
            this.Controls.Add(this.labelDistributions);
            this.Controls.Add(this.checkedListBoxDistribution);
            this.Controls.Add(this.hintTextBoxPackage);
            this.Controls.Add(this.labelPackage);
            this.Name = "PackageImplementationControl";
            this.Size = new System.Drawing.Size(347, 157);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelPackage;
        private Common.Controls.HintTextBox hintTextBoxPackage;
        private System.Windows.Forms.CheckedListBox checkedListBoxDistribution;
        private System.Windows.Forms.Label labelDistributions;
        private System.Windows.Forms.Label labelMain;
        private Common.Controls.HintTextBox hintTextBoxMain;
        private Common.Controls.HintTextBox hintTextBoxDocDir;
        private System.Windows.Forms.Label labelDocDir;
        private System.Windows.Forms.ComboBox comboBoxLicense;
        private System.Windows.Forms.Label labelLicense;
    }
}
