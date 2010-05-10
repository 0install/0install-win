namespace ZeroInstall.FeedEditor
{
    partial class ImplementationControl
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
            this.labelStability = new System.Windows.Forms.Label();
            this.comboBoxStability = new System.Windows.Forms.ComboBox();
            this.hintTextBoxDocDir = new Common.Controls.HintTextBox();
            this.labelDocDir = new System.Windows.Forms.Label();
            this.hintTextBoxSelfTest = new Common.Controls.HintTextBox();
            this.labelSelfTest = new System.Windows.Forms.Label();
            this.hintTextBoxMain = new Common.Controls.HintTextBox();
            this.hintTextBoxVersion = new Common.Controls.HintTextBox();
            this.labelMain = new System.Windows.Forms.Label();
            this.targetBaseControl = new ZeroInstall.FeedEditor.TargetBaseControl();
            this.comboBoxLicense = new System.Windows.Forms.ComboBox();
            this.labelLicense = new System.Windows.Forms.Label();
            this.dateTimePickerRelease = new System.Windows.Forms.DateTimePicker();
            this.labelReleased = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelID = new System.Windows.Forms.Label();
            this.hintTextBox1 = new Common.Controls.HintTextBox();
            this.labelLocalPath = new System.Windows.Forms.Label();
            this.hintTextBoxLocalPath = new Common.Controls.HintTextBox();
            this.SuspendLayout();
            // 
            // labelStability
            // 
            this.labelStability.AutoSize = true;
            this.labelStability.Location = new System.Drawing.Point(465, 39);
            this.labelStability.Name = "labelStability";
            this.labelStability.Size = new System.Drawing.Size(43, 13);
            this.labelStability.TabIndex = 45;
            this.labelStability.Text = "Stability";
            // 
            // comboBoxStability
            // 
            this.comboBoxStability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStability.FormattingEnabled = true;
            this.comboBoxStability.Items.AddRange(new object[] {
            "",
            "stable",
            "testing",
            "developer",
            "buggy",
            "insecure"});
            this.comboBoxStability.Location = new System.Drawing.Point(468, 54);
            this.comboBoxStability.Name = "comboBoxStability";
            this.comboBoxStability.Size = new System.Drawing.Size(109, 21);
            this.comboBoxStability.TabIndex = 44;
            // 
            // hintTextBoxDocDir
            // 
            this.hintTextBoxDocDir.HintText = "";
            this.hintTextBoxDocDir.Location = new System.Drawing.Point(0, 94);
            this.hintTextBoxDocDir.Name = "hintTextBoxDocDir";
            this.hintTextBoxDocDir.Size = new System.Drawing.Size(289, 20);
            this.hintTextBoxDocDir.TabIndex = 43;
            // 
            // labelDocDir
            // 
            this.labelDocDir.AutoSize = true;
            this.labelDocDir.Location = new System.Drawing.Point(-3, 78);
            this.labelDocDir.Name = "labelDocDir";
            this.labelDocDir.Size = new System.Drawing.Size(122, 13);
            this.labelDocDir.TabIndex = 42;
            this.labelDocDir.Text = "Documentation directory";
            // 
            // hintTextBoxSelfTest
            // 
            this.hintTextBoxSelfTest.HintText = "";
            this.hintTextBoxSelfTest.Location = new System.Drawing.Point(295, 94);
            this.hintTextBoxSelfTest.Name = "hintTextBoxSelfTest";
            this.hintTextBoxSelfTest.Size = new System.Drawing.Size(282, 20);
            this.hintTextBoxSelfTest.TabIndex = 41;
            // 
            // labelSelfTest
            // 
            this.labelSelfTest.AutoSize = true;
            this.labelSelfTest.Location = new System.Drawing.Point(292, 78);
            this.labelSelfTest.Name = "labelSelfTest";
            this.labelSelfTest.Size = new System.Drawing.Size(45, 13);
            this.labelSelfTest.TabIndex = 40;
            this.labelSelfTest.Text = "Self-test";
            // 
            // hintTextBoxMain
            // 
            this.hintTextBoxMain.HintText = "";
            this.hintTextBoxMain.Location = new System.Drawing.Point(0, 55);
            this.hintTextBoxMain.Name = "hintTextBoxMain";
            this.hintTextBoxMain.Size = new System.Drawing.Size(462, 20);
            this.hintTextBoxMain.TabIndex = 39;
            // 
            // hintTextBoxVersion
            // 
            this.hintTextBoxVersion.HintText = "";
            this.hintTextBoxVersion.Location = new System.Drawing.Point(0, 16);
            this.hintTextBoxVersion.Name = "hintTextBoxVersion";
            this.hintTextBoxVersion.Size = new System.Drawing.Size(100, 20);
            this.hintTextBoxVersion.TabIndex = 38;
            // 
            // labelMain
            // 
            this.labelMain.AutoSize = true;
            this.labelMain.Location = new System.Drawing.Point(-3, 39);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(46, 13);
            this.labelMain.TabIndex = 37;
            this.labelMain.Text = "Main file";
            // 
            // targetBaseControl
            // 
            this.targetBaseControl.Location = new System.Drawing.Point(0, 120);
            this.targetBaseControl.Name = "targetBaseControl";
            this.targetBaseControl.Size = new System.Drawing.Size(502, 128);
            this.targetBaseControl.TabIndex = 36;
            this.targetBaseControl.TargetBase = null;
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
            this.comboBoxLicense.Location = new System.Drawing.Point(312, 15);
            this.comboBoxLicense.Name = "comboBoxLicense";
            this.comboBoxLicense.Size = new System.Drawing.Size(265, 21);
            this.comboBoxLicense.Sorted = true;
            this.comboBoxLicense.TabIndex = 35;
            // 
            // labelLicense
            // 
            this.labelLicense.AutoSize = true;
            this.labelLicense.Location = new System.Drawing.Point(309, 0);
            this.labelLicense.Name = "labelLicense";
            this.labelLicense.Size = new System.Drawing.Size(44, 13);
            this.labelLicense.TabIndex = 34;
            this.labelLicense.Text = "License";
            // 
            // dateTimePickerRelease
            // 
            this.dateTimePickerRelease.Location = new System.Drawing.Point(106, 16);
            this.dateTimePickerRelease.Name = "dateTimePickerRelease";
            this.dateTimePickerRelease.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerRelease.TabIndex = 33;
            // 
            // labelReleased
            // 
            this.labelReleased.AutoSize = true;
            this.labelReleased.Location = new System.Drawing.Point(103, 0);
            this.labelReleased.Name = "labelReleased";
            this.labelReleased.Size = new System.Drawing.Size(70, 13);
            this.labelReleased.TabIndex = 32;
            this.labelReleased.Text = "Release date";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(-3, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(42, 13);
            this.labelVersion.TabIndex = 31;
            this.labelVersion.Text = "Version";
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Location = new System.Drawing.Point(3, 251);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(18, 13);
            this.labelID.TabIndex = 46;
            this.labelID.Text = "ID";
            // 
            // hintTextBox1
            // 
            this.hintTextBox1.HintText = "";
            this.hintTextBox1.Location = new System.Drawing.Point(6, 268);
            this.hintTextBox1.Name = "hintTextBox1";
            this.hintTextBox1.Size = new System.Drawing.Size(352, 20);
            this.hintTextBox1.TabIndex = 47;
            // 
            // labelLocalPath
            // 
            this.labelLocalPath.AutoSize = true;
            this.labelLocalPath.Location = new System.Drawing.Point(403, 251);
            this.labelLocalPath.Name = "labelLocalPath";
            this.labelLocalPath.Size = new System.Drawing.Size(57, 13);
            this.labelLocalPath.TabIndex = 48;
            this.labelLocalPath.Text = "Local path";
            // 
            // hintTextBoxLocalPath
            // 
            this.hintTextBoxLocalPath.HintText = "";
            this.hintTextBoxLocalPath.Location = new System.Drawing.Point(406, 268);
            this.hintTextBoxLocalPath.Name = "hintTextBoxLocalPath";
            this.hintTextBoxLocalPath.Size = new System.Drawing.Size(177, 20);
            this.hintTextBoxLocalPath.TabIndex = 49;
            // 
            // ImplementationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.hintTextBoxLocalPath);
            this.Controls.Add(this.labelLocalPath);
            this.Controls.Add(this.hintTextBox1);
            this.Controls.Add(this.labelID);
            this.Controls.Add(this.labelStability);
            this.Controls.Add(this.comboBoxStability);
            this.Controls.Add(this.hintTextBoxDocDir);
            this.Controls.Add(this.labelDocDir);
            this.Controls.Add(this.hintTextBoxSelfTest);
            this.Controls.Add(this.labelSelfTest);
            this.Controls.Add(this.hintTextBoxMain);
            this.Controls.Add(this.hintTextBoxVersion);
            this.Controls.Add(this.labelMain);
            this.Controls.Add(this.targetBaseControl);
            this.Controls.Add(this.comboBoxLicense);
            this.Controls.Add(this.labelLicense);
            this.Controls.Add(this.dateTimePickerRelease);
            this.Controls.Add(this.labelReleased);
            this.Controls.Add(this.labelVersion);
            this.Name = "ImplementationControl";
            this.Size = new System.Drawing.Size(602, 320);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelStability;
        private System.Windows.Forms.ComboBox comboBoxStability;
        private Common.Controls.HintTextBox hintTextBoxDocDir;
        private System.Windows.Forms.Label labelDocDir;
        private Common.Controls.HintTextBox hintTextBoxSelfTest;
        private System.Windows.Forms.Label labelSelfTest;
        private Common.Controls.HintTextBox hintTextBoxMain;
        private Common.Controls.HintTextBox hintTextBoxVersion;
        private System.Windows.Forms.Label labelMain;
        private TargetBaseControl targetBaseControl;
        private System.Windows.Forms.ComboBox comboBoxLicense;
        private System.Windows.Forms.Label labelLicense;
        private System.Windows.Forms.DateTimePicker dateTimePickerRelease;
        private System.Windows.Forms.Label labelReleased;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelID;
        private Common.Controls.HintTextBox hintTextBox1;
        private System.Windows.Forms.Label labelLocalPath;
        private Common.Controls.HintTextBox hintTextBoxLocalPath;
    }
}
