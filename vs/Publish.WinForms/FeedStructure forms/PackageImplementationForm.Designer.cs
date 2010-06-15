﻿namespace ZeroInstall.Publish.WinForms
{
    partial class PackageImplementationForm
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
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelPackage
            // 
            this.labelPackage.AutoSize = true;
            this.labelPackage.Location = new System.Drawing.Point(12, 48);
            this.labelPackage.Name = "labelPackage";
            this.labelPackage.Size = new System.Drawing.Size(79, 13);
            this.labelPackage.TabIndex = 2;
            this.labelPackage.Text = "Package name";
            // 
            // hintTextBoxPackage
            // 
            this.hintTextBoxPackage.HintText = "Package name in the selected distributions";
            this.hintTextBoxPackage.Location = new System.Drawing.Point(14, 64);
            this.hintTextBoxPackage.Name = "hintTextBoxPackage";
            this.hintTextBoxPackage.Size = new System.Drawing.Size(340, 20);
            this.hintTextBoxPackage.TabIndex = 3;
            // 
            // checkedListBoxDistribution
            // 
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
            this.labelDistributions.AutoSize = true;
            this.labelDistributions.Location = new System.Drawing.Point(288, 87);
            this.labelDistributions.Name = "labelDistributions";
            this.labelDistributions.Size = new System.Drawing.Size(64, 13);
            this.labelDistributions.TabIndex = 8;
            this.labelDistributions.Text = "Distributions";
            // 
            // labelMain
            // 
            this.labelMain.AutoSize = true;
            this.labelMain.Location = new System.Drawing.Point(12, 9);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(30, 13);
            this.labelMain.TabIndex = 0;
            this.labelMain.Text = "Main";
            // 
            // hintTextBoxMain
            // 
            this.hintTextBoxMain.HintText = "Absolute path to the main";
            this.hintTextBoxMain.Location = new System.Drawing.Point(14, 25);
            this.hintTextBoxMain.Name = "hintTextBoxMain";
            this.hintTextBoxMain.Size = new System.Drawing.Size(340, 20);
            this.hintTextBoxMain.TabIndex = 1;
            this.hintTextBoxMain.TextChanged += new System.EventHandler(this.hintTextBoxMain_TextChanged);
            // 
            // hintTextBoxDocDir
            // 
            this.hintTextBoxDocDir.HintText = "Relative path of a directory inside the implementation";
            this.hintTextBoxDocDir.Location = new System.Drawing.Point(14, 143);
            this.hintTextBoxDocDir.Name = "hintTextBoxDocDir";
            this.hintTextBoxDocDir.Size = new System.Drawing.Size(271, 20);
            this.hintTextBoxDocDir.TabIndex = 7;
            this.hintTextBoxDocDir.TextChanged += new System.EventHandler(this.hintTextBoxDocDir_TextChanged);
            // 
            // labelDocDir
            // 
            this.labelDocDir.AutoSize = true;
            this.labelDocDir.Location = new System.Drawing.Point(12, 127);
            this.labelDocDir.Name = "labelDocDir";
            this.labelDocDir.Size = new System.Drawing.Size(122, 13);
            this.labelDocDir.TabIndex = 6;
            this.labelDocDir.Text = "Documentation directory";
            // 
            // comboBoxLicense
            // 
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
            this.comboBoxLicense.Location = new System.Drawing.Point(14, 103);
            this.comboBoxLicense.Name = "comboBoxLicense";
            this.comboBoxLicense.Size = new System.Drawing.Size(271, 21);
            this.comboBoxLicense.Sorted = true;
            this.comboBoxLicense.TabIndex = 5;
            // 
            // labelLicense
            // 
            this.labelLicense.AutoSize = true;
            this.labelLicense.Location = new System.Drawing.Point(12, 87);
            this.labelLicense.Name = "labelLicense";
            this.labelLicense.Size = new System.Drawing.Size(44, 13);
            this.labelLicense.TabIndex = 4;
            this.labelLicense.Text = "License";
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(279, 213);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 11;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(198, 213);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // PackageImplementationForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(370, 248);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
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
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "PackageImplementationForm";
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
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}
