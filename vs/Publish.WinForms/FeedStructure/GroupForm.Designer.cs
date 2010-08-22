using ZeroInstall.Publish.WinForms.Controls;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class GroupForm
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
            this.labelDocDir = new System.Windows.Forms.Label();
            this.labelSelfTest = new System.Windows.Forms.Label();
            this.labelMain = new System.Windows.Forms.Label();
            this.comboBoxLicense = new System.Windows.Forms.ComboBox();
            this.labelLicense = new System.Windows.Forms.Label();
            this.dateTimePickerRelease = new System.Windows.Forms.DateTimePicker();
            this.labelReleased = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.hintTextBoxDocDir = new Common.Controls.HintTextBox();
            this.hintTextBoxSelfTest = new Common.Controls.HintTextBox();
            this.hintTextBoxMain = new Common.Controls.HintTextBox();
            this.hintTextBoxVersion = new Common.Controls.HintTextBox();
            this.targetBaseControl = new TargetBaseControl();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(448, 290);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(529, 290);
            // 
            // labelStability
            // 
            this.labelStability.AutoSize = true;
            this.labelStability.Location = new System.Drawing.Point(489, 47);
            this.labelStability.Name = "labelStability";
            this.labelStability.Size = new System.Drawing.Size(43, 13);
            this.labelStability.TabIndex = 8;
            this.labelStability.Text = "Stability";
            // 
            // comboBoxStability
            // 
            this.comboBoxStability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStability.FormattingEnabled = true;
            this.comboBoxStability.Location = new System.Drawing.Point(492, 63);
            this.comboBoxStability.Name = "comboBoxStability";
            this.comboBoxStability.Size = new System.Drawing.Size(112, 21);
            this.comboBoxStability.TabIndex = 9;
            // 
            // labelDocDir
            // 
            this.labelDocDir.AutoSize = true;
            this.labelDocDir.Location = new System.Drawing.Point(12, 87);
            this.labelDocDir.Name = "labelDocDir";
            this.labelDocDir.Size = new System.Drawing.Size(122, 13);
            this.labelDocDir.TabIndex = 10;
            this.labelDocDir.Text = "Documentation directory";
            // 
            // labelSelfTest
            // 
            this.labelSelfTest.AutoSize = true;
            this.labelSelfTest.Location = new System.Drawing.Point(307, 87);
            this.labelSelfTest.Name = "labelSelfTest";
            this.labelSelfTest.Size = new System.Drawing.Size(45, 13);
            this.labelSelfTest.TabIndex = 12;
            this.labelSelfTest.Text = "Self-test";
            // 
            // labelMain
            // 
            this.labelMain.AutoSize = true;
            this.labelMain.Location = new System.Drawing.Point(12, 48);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(46, 13);
            this.labelMain.TabIndex = 6;
            this.labelMain.Text = "Main file";
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
            this.comboBoxLicense.Location = new System.Drawing.Point(336, 25);
            this.comboBoxLicense.Name = "comboBoxLicense";
            this.comboBoxLicense.Size = new System.Drawing.Size(268, 21);
            this.comboBoxLicense.Sorted = true;
            this.comboBoxLicense.TabIndex = 5;
            // 
            // labelLicense
            // 
            this.labelLicense.AutoSize = true;
            this.labelLicense.Location = new System.Drawing.Point(333, 9);
            this.labelLicense.Name = "labelLicense";
            this.labelLicense.Size = new System.Drawing.Size(44, 13);
            this.labelLicense.TabIndex = 4;
            this.labelLicense.Text = "License";
            // 
            // dateTimePickerRelease
            // 
            this.dateTimePickerRelease.Location = new System.Drawing.Point(130, 25);
            this.dateTimePickerRelease.Name = "dateTimePickerRelease";
            this.dateTimePickerRelease.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerRelease.TabIndex = 3;
            // 
            // labelReleased
            // 
            this.labelReleased.AutoSize = true;
            this.labelReleased.Location = new System.Drawing.Point(127, 9);
            this.labelReleased.Name = "labelReleased";
            this.labelReleased.Size = new System.Drawing.Size(70, 13);
            this.labelReleased.TabIndex = 2;
            this.labelReleased.Text = "Release date";
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(12, 9);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(42, 13);
            this.labelVersion.TabIndex = 0;
            this.labelVersion.Text = "Version";
            // 
            // hintTextBoxDocDir
            // 
            this.hintTextBoxDocDir.HintText = "Relative path of a directory inside the implementation";
            this.hintTextBoxDocDir.Location = new System.Drawing.Point(15, 103);
            this.hintTextBoxDocDir.Name = "hintTextBoxDocDir";
            this.hintTextBoxDocDir.Size = new System.Drawing.Size(289, 20);
            this.hintTextBoxDocDir.TabIndex = 11;
            this.hintTextBoxDocDir.TextChanged += new System.EventHandler(this.HintTextBoxDocDirTextChanged);
            // 
            // hintTextBoxSelfTest
            // 
            this.hintTextBoxSelfTest.HintText = "Relative path of an executable inside the implementation";
            this.hintTextBoxSelfTest.Location = new System.Drawing.Point(310, 103);
            this.hintTextBoxSelfTest.Name = "hintTextBoxSelfTest";
            this.hintTextBoxSelfTest.Size = new System.Drawing.Size(294, 20);
            this.hintTextBoxSelfTest.TabIndex = 13;
            this.hintTextBoxSelfTest.TextChanged += new System.EventHandler(this.HintTextBoxSelfTestTextChanged);
            // 
            // hintTextBoxMain
            // 
            this.hintTextBoxMain.HintText = "Relative path of an executable inside the implementation";
            this.hintTextBoxMain.Location = new System.Drawing.Point(15, 64);
            this.hintTextBoxMain.Name = "hintTextBoxMain";
            this.hintTextBoxMain.Size = new System.Drawing.Size(471, 20);
            this.hintTextBoxMain.TabIndex = 7;
            this.hintTextBoxMain.TextChanged += new System.EventHandler(this.HintTextBoxMainTextChanged);
            // 
            // hintTextBoxVersion
            // 
            this.hintTextBoxVersion.HintText = "";
            this.hintTextBoxVersion.Location = new System.Drawing.Point(15, 25);
            this.hintTextBoxVersion.Name = "hintTextBoxVersion";
            this.hintTextBoxVersion.Size = new System.Drawing.Size(109, 20);
            this.hintTextBoxVersion.TabIndex = 1;
            this.hintTextBoxVersion.TextChanged += new System.EventHandler(this.HintTextBoxVersionTextChanged);
            // 
            // targetBaseControl
            // 
            this.targetBaseControl.Location = new System.Drawing.Point(15, 129);
            this.targetBaseControl.Name = "targetBaseControl";
            this.targetBaseControl.Size = new System.Drawing.Size(502, 128);
            this.targetBaseControl.TabIndex = 14;
            this.targetBaseControl.TargetBase = null;
            // 
            // GroupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 325);
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
            this.Name = "GroupForm";
            this.Text = "Edit group";
            this.Controls.SetChildIndex(this.labelVersion, 0);
            this.Controls.SetChildIndex(this.labelReleased, 0);
            this.Controls.SetChildIndex(this.dateTimePickerRelease, 0);
            this.Controls.SetChildIndex(this.labelLicense, 0);
            this.Controls.SetChildIndex(this.comboBoxLicense, 0);
            this.Controls.SetChildIndex(this.targetBaseControl, 0);
            this.Controls.SetChildIndex(this.labelMain, 0);
            this.Controls.SetChildIndex(this.hintTextBoxVersion, 0);
            this.Controls.SetChildIndex(this.hintTextBoxMain, 0);
            this.Controls.SetChildIndex(this.labelSelfTest, 0);
            this.Controls.SetChildIndex(this.hintTextBoxSelfTest, 0);
            this.Controls.SetChildIndex(this.labelDocDir, 0);
            this.Controls.SetChildIndex(this.hintTextBoxDocDir, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.comboBoxStability, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.labelStability, 0);
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
    }
}
