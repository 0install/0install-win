using ZeroInstall.Publish.WinForms.Controls;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    partial class ImplementationForm
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
            this.comboBoxLicense = new System.Windows.Forms.ComboBox();
            this.labelLicense = new System.Windows.Forms.Label();
            this.dateTimePickerRelease = new System.Windows.Forms.DateTimePicker();
            this.labelReleased = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelLocalPath = new System.Windows.Forms.Label();
            this.hintTextBoxLocalPath = new Common.Controls.HintTextBox();
            this.targetBaseControl = new ZeroInstall.Publish.WinForms.Controls.TargetBaseControl();
            this.hintTextBoxID = new Common.Controls.HintTextBox();
            this.labelID = new System.Windows.Forms.Label();
            this.checkBoxSettingDateEnable = new System.Windows.Forms.CheckBox();
            this.buttonShowManifestDigest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(362, 341);
            this.buttonOK.Click += new System.EventHandler(this.ButtonOkClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(443, 341);
            // 
            // labelStability
            // 
            this.labelStability.AutoSize = true;
            this.labelStability.Location = new System.Drawing.Point(406, 9);
            this.labelStability.Name = "labelStability";
            this.labelStability.Size = new System.Drawing.Size(43, 13);
            this.labelStability.TabIndex = 4;
            this.labelStability.Text = "Stability";
            // 
            // comboBoxStability
            // 
            this.comboBoxStability.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxStability.FormattingEnabled = true;
            this.comboBoxStability.Location = new System.Drawing.Point(409, 25);
            this.comboBoxStability.Name = "comboBoxStability";
            this.comboBoxStability.Size = new System.Drawing.Size(109, 21);
            this.comboBoxStability.TabIndex = 5;
            // 
            // hintTextBoxDocDir
            // 
            this.hintTextBoxDocDir.HintText = "";
            this.hintTextBoxDocDir.Location = new System.Drawing.Point(271, 103);
            this.hintTextBoxDocDir.Name = "hintTextBoxDocDir";
            this.hintTextBoxDocDir.Size = new System.Drawing.Size(247, 20);
            this.hintTextBoxDocDir.TabIndex = 11;
            // 
            // labelDocDir
            // 
            this.labelDocDir.AutoSize = true;
            this.labelDocDir.Location = new System.Drawing.Point(268, 87);
            this.labelDocDir.Name = "labelDocDir";
            this.labelDocDir.Size = new System.Drawing.Size(122, 13);
            this.labelDocDir.TabIndex = 10;
            this.labelDocDir.Text = "Documentation directory";
            // 
            // hintTextBoxSelfTest
            // 
            this.hintTextBoxSelfTest.HintText = "";
            this.hintTextBoxSelfTest.Location = new System.Drawing.Point(15, 142);
            this.hintTextBoxSelfTest.Name = "hintTextBoxSelfTest";
            this.hintTextBoxSelfTest.Size = new System.Drawing.Size(249, 20);
            this.hintTextBoxSelfTest.TabIndex = 13;
            // 
            // labelSelfTest
            // 
            this.labelSelfTest.AutoSize = true;
            this.labelSelfTest.Location = new System.Drawing.Point(13, 126);
            this.labelSelfTest.Name = "labelSelfTest";
            this.labelSelfTest.Size = new System.Drawing.Size(45, 13);
            this.labelSelfTest.TabIndex = 12;
            this.labelSelfTest.Text = "Self-test";
            // 
            // hintTextBoxMain
            // 
            this.hintTextBoxMain.HintText = "";
            this.hintTextBoxMain.Location = new System.Drawing.Point(15, 103);
            this.hintTextBoxMain.Name = "hintTextBoxMain";
            this.hintTextBoxMain.Size = new System.Drawing.Size(249, 20);
            this.hintTextBoxMain.TabIndex = 9;
            // 
            // hintTextBoxVersion
            // 
            this.hintTextBoxVersion.HintText = "";
            this.hintTextBoxVersion.Location = new System.Drawing.Point(15, 25);
            this.hintTextBoxVersion.Name = "hintTextBoxVersion";
            this.hintTextBoxVersion.Size = new System.Drawing.Size(122, 20);
            this.hintTextBoxVersion.TabIndex = 1;
            this.hintTextBoxVersion.TextChanged += new System.EventHandler(this.HintTextBoxVersionTextChanged);
            // 
            // labelMain
            // 
            this.labelMain.AutoSize = true;
            this.labelMain.Location = new System.Drawing.Point(13, 87);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(46, 13);
            this.labelMain.TabIndex = 8;
            this.labelMain.Text = "Main file";
            // 
            // comboBoxLicense
            // 
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
            this.comboBoxLicense.Location = new System.Drawing.Point(15, 63);
            this.comboBoxLicense.Name = "comboBoxLicense";
            this.comboBoxLicense.Size = new System.Drawing.Size(249, 21);
            this.comboBoxLicense.Sorted = true;
            this.comboBoxLicense.TabIndex = 7;
            // 
            // labelLicense
            // 
            this.labelLicense.AutoSize = true;
            this.labelLicense.Location = new System.Drawing.Point(12, 48);
            this.labelLicense.Name = "labelLicense";
            this.labelLicense.Size = new System.Drawing.Size(44, 13);
            this.labelLicense.TabIndex = 6;
            this.labelLicense.Text = "License";
            // 
            // dateTimePickerRelease
            // 
            this.dateTimePickerRelease.Enabled = false;
            this.dateTimePickerRelease.Location = new System.Drawing.Point(164, 25);
            this.dateTimePickerRelease.Name = "dateTimePickerRelease";
            this.dateTimePickerRelease.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerRelease.TabIndex = 3;
            // 
            // labelReleased
            // 
            this.labelReleased.AutoSize = true;
            this.labelReleased.Location = new System.Drawing.Point(140, 9);
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
            // labelLocalPath
            // 
            this.labelLocalPath.AutoSize = true;
            this.labelLocalPath.Location = new System.Drawing.Point(268, 126);
            this.labelLocalPath.Name = "labelLocalPath";
            this.labelLocalPath.Size = new System.Drawing.Size(57, 13);
            this.labelLocalPath.TabIndex = 14;
            this.labelLocalPath.Text = "Local path";
            // 
            // hintTextBoxLocalPath
            // 
            this.hintTextBoxLocalPath.HintText = "";
            this.hintTextBoxLocalPath.Location = new System.Drawing.Point(271, 142);
            this.hintTextBoxLocalPath.Name = "hintTextBoxLocalPath";
            this.hintTextBoxLocalPath.Size = new System.Drawing.Size(247, 20);
            this.hintTextBoxLocalPath.TabIndex = 15;
            // 
            // targetBaseControl
            // 
            this.targetBaseControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetBaseControl.Location = new System.Drawing.Point(16, 168);
            this.targetBaseControl.Name = "targetBaseControl";
            this.targetBaseControl.Size = new System.Drawing.Size(502, 128);
            this.targetBaseControl.TabIndex = 16;
            // 
            // hintTextBoxID
            // 
            this.hintTextBoxID.HintText = "";
            this.hintTextBoxID.Location = new System.Drawing.Point(270, 63);
            this.hintTextBoxID.Name = "hintTextBoxID";
            this.hintTextBoxID.Size = new System.Drawing.Size(248, 20);
            this.hintTextBoxID.TabIndex = 1002;
            // 
            // labelID
            // 
            this.labelID.AutoSize = true;
            this.labelID.Location = new System.Drawing.Point(268, 47);
            this.labelID.Name = "labelID";
            this.labelID.Size = new System.Drawing.Size(18, 13);
            this.labelID.TabIndex = 1003;
            this.labelID.Text = "ID";
            // 
            // checkBoxSettingDateEnable
            // 
            this.checkBoxSettingDateEnable.AutoSize = true;
            this.checkBoxSettingDateEnable.Location = new System.Drawing.Point(143, 28);
            this.checkBoxSettingDateEnable.Name = "checkBoxSettingDateEnable";
            this.checkBoxSettingDateEnable.Size = new System.Drawing.Size(15, 14);
            this.checkBoxSettingDateEnable.TabIndex = 1004;
            this.checkBoxSettingDateEnable.UseVisualStyleBackColor = true;
            this.checkBoxSettingDateEnable.CheckedChanged += new System.EventHandler(this.CheckBoxSettingDateEnableCheckedChanged);
            // 
            // buttonShowManifestDigest
            // 
            this.buttonShowManifestDigest.Location = new System.Drawing.Point(16, 302);
            this.buttonShowManifestDigest.Name = "buttonShowManifestDigest";
            this.buttonShowManifestDigest.Size = new System.Drawing.Size(128, 23);
            this.buttonShowManifestDigest.TabIndex = 1005;
            this.buttonShowManifestDigest.Text = "Show Manifest Digests";
            this.buttonShowManifestDigest.UseVisualStyleBackColor = true;
            this.buttonShowManifestDigest.Click += new System.EventHandler(this.ButtonShowManifestDigestClick);
            // 
            // ImplementationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(530, 376);
            this.Controls.Add(this.buttonShowManifestDigest);
            this.Controls.Add(this.checkBoxSettingDateEnable);
            this.Controls.Add(this.labelID);
            this.Controls.Add(this.hintTextBoxID);
            this.Controls.Add(this.hintTextBoxLocalPath);
            this.Controls.Add(this.labelLocalPath);
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
            this.Name = "ImplementationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit implementation";
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
            this.Controls.SetChildIndex(this.comboBoxStability, 0);
            this.Controls.SetChildIndex(this.labelStability, 0);
            this.Controls.SetChildIndex(this.buttonOK, 0);
            this.Controls.SetChildIndex(this.labelLocalPath, 0);
            this.Controls.SetChildIndex(this.buttonCancel, 0);
            this.Controls.SetChildIndex(this.hintTextBoxLocalPath, 0);
            this.Controls.SetChildIndex(this.hintTextBoxID, 0);
            this.Controls.SetChildIndex(this.labelID, 0);
            this.Controls.SetChildIndex(this.checkBoxSettingDateEnable, 0);
            this.Controls.SetChildIndex(this.buttonShowManifestDigest, 0);
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
        private System.Windows.Forms.Label labelLocalPath;
        private Common.Controls.HintTextBox hintTextBoxLocalPath;
        private Common.Controls.HintTextBox hintTextBoxID;
        private System.Windows.Forms.Label labelID;
        private System.Windows.Forms.CheckBox checkBoxSettingDateEnable;
        private System.Windows.Forms.Button buttonShowManifestDigest;
    }
}
