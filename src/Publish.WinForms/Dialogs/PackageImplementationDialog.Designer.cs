namespace ZeroInstall.Publish.WinForms.Dialogs
{
    partial class PackageImplementationDialog
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
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(198, 222);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(279, 222);
            // 
            // labelPackage
            // 
            this.labelPackage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPackage.AutoSize = true;
            this.labelPackage.Location = new System.Drawing.Point(12, 48);
            this.labelPackage.Name = "labelPackage";
            this.labelPackage.Size = new System.Drawing.Size(79, 13);
            this.labelPackage.TabIndex = 2;
            this.labelPackage.Text = "Package name";
            // 
            // hintTextBoxPackage
            // 
            this.hintTextBoxPackage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxPackage.HintText = "Package name in the selected distributions";
            this.hintTextBoxPackage.Location = new System.Drawing.Point(14, 64);
            this.hintTextBoxPackage.Name = "hintTextBoxPackage";
            this.hintTextBoxPackage.Size = new System.Drawing.Size(340, 20);
            this.hintTextBoxPackage.TabIndex = 3;
            // 
            // checkedListBoxDistribution
            // 
            this.checkedListBoxDistribution.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedListBoxDistribution.CheckOnClick = true;
            this.checkedListBoxDistribution.FormattingEnabled = true;
            this.checkedListBoxDistribution.Items.AddRange(new object[] {
            "RPM",
            "Debian",
            "Gentoo",
            "Slack",
            "Ports"});
            this.checkedListBoxDistribution.Location = new System.Drawing.Point(291, 103);
            this.checkedListBoxDistribution.Name = "checkedListBoxDistribution";
            this.checkedListBoxDistribution.Size = new System.Drawing.Size(63, 79);
            this.checkedListBoxDistribution.TabIndex = 9;
            // 
            // labelDistributions
            // 
            this.labelDistributions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDistributions.AutoSize = true;
            this.labelDistributions.Location = new System.Drawing.Point(288, 87);
            this.labelDistributions.Name = "labelDistributions";
            this.labelDistributions.Size = new System.Drawing.Size(64, 13);
            this.labelDistributions.TabIndex = 8;
            this.labelDistributions.Text = "Distributions";
            // 
            // labelMain
            // 
            this.labelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMain.AutoSize = true;
            this.labelMain.Location = new System.Drawing.Point(12, 9);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(30, 13);
            this.labelMain.TabIndex = 0;
            this.labelMain.Text = "Main";
            // 
            // hintTextBoxMain
            // 
            this.hintTextBoxMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxMain.HintText = "Absolute path to the main execution file";
            this.hintTextBoxMain.Location = new System.Drawing.Point(14, 25);
            this.hintTextBoxMain.Name = "hintTextBoxMain";
            this.hintTextBoxMain.Size = new System.Drawing.Size(340, 20);
            this.hintTextBoxMain.TabIndex = 1;
            this.hintTextBoxMain.TextChanged += new System.EventHandler(this.HintTextBoxMainTextChanged);
            // 
            // hintTextBoxDocDir
            // 
            this.hintTextBoxDocDir.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hintTextBoxDocDir.HintText = "Relative path to the docs inside the archive";
            this.hintTextBoxDocDir.Location = new System.Drawing.Point(14, 143);
            this.hintTextBoxDocDir.Name = "hintTextBoxDocDir";
            this.hintTextBoxDocDir.Size = new System.Drawing.Size(271, 20);
            this.hintTextBoxDocDir.TabIndex = 7;
            this.hintTextBoxDocDir.TextChanged += new System.EventHandler(this.HintTextBoxDocDirTextChanged);
            // 
            // labelDocDir
            // 
            this.labelDocDir.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDocDir.AutoSize = true;
            this.labelDocDir.Location = new System.Drawing.Point(12, 127);
            this.labelDocDir.Name = "labelDocDir";
            this.labelDocDir.Size = new System.Drawing.Size(122, 13);
            this.labelDocDir.TabIndex = 6;
            this.labelDocDir.Text = "Documentation directory";
            // 
            // comboBoxLicense
            // 
            this.comboBoxLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLicense.FormattingEnabled = true;
            this.comboBoxLicense.Items.AddRange(new object[] {
            "",
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
            this.comboBoxLicense.Location = new System.Drawing.Point(14, 103);
            this.comboBoxLicense.Name = "comboBoxLicense";
            this.comboBoxLicense.Size = new System.Drawing.Size(271, 21);
            this.comboBoxLicense.Sorted = true;
            this.comboBoxLicense.TabIndex = 5;
            // 
            // labelLicense
            // 
            this.labelLicense.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLicense.AutoSize = true;
            this.labelLicense.Location = new System.Drawing.Point(12, 87);
            this.labelLicense.Name = "labelLicense";
            this.labelLicense.Size = new System.Drawing.Size(44, 13);
            this.labelLicense.TabIndex = 4;
            this.labelLicense.Text = "License";
            // 
            // PackageImplementationDialog
            // 
            this.ClientSize = new System.Drawing.Size(366, 257);
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
            this.Name = "PackageImplementationDialog";
            this.Text = "Edit package implementation";
            this.Controls.SetChildIndex(this.labelPackage, 0);
            this.Controls.SetChildIndex(this.hintTextBoxPackage, 0);
            this.Controls.SetChildIndex(this.checkedListBoxDistribution, 0);
            this.Controls.SetChildIndex(this.labelDistributions, 0);
            this.Controls.SetChildIndex(this.labelMain, 0);
            this.Controls.SetChildIndex(this.hintTextBoxMain, 0);
            this.Controls.SetChildIndex(this.labelLicense, 0);
            this.Controls.SetChildIndex(this.comboBoxLicense, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.labelDocDir, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.hintTextBoxDocDir, 0);
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
