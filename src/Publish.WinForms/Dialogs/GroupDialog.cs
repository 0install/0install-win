/*
 * Copyright 2010 Simon E. Silva Lauinger
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Drawing;
using Common.Controls;
using ZeroInstall.Model;
using System.IO;

namespace ZeroInstall.Publish.WinForms.Dialogs
{
    public partial class GroupDialog : OKCancelDialog
    {
        #region Properties
        /// <summary>
        /// The <see cref="Group" /> to be displayed and modified by this form.
        /// </summary>
        private Group _group = new Group();

        /// <summary>
        /// The <see cref="Group" /> to be displayed and modified by this form. If <see langword="null"/>, the controls will be reset.
        /// </summary>
        public Group Group
        {
            get { return _group; }
            set
            {
                _group = value ?? new Group();
                targetBaseControl.TargetBase = _group.CloneGroup();
                UpdateFormControls();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Creates a new <see cref="GroupDialog"/> object.
        /// </summary>
        public GroupDialog()
        {
            InitializeComponent();
            InitializeComboBoxStability();
        }

        /// <summary>
        /// Fills <see cref="comboBoxStability"/> with the items of <see cref="Stability"/>.
        /// </summary>
        //TODO Change methode that localisation is possible (Stability only contains the english names).
        private void InitializeComboBoxStability()
        {
            foreach (var stability in Enum.GetValues(typeof(Stability)))
                comboBoxStability.Items.Add(stability);
        }
        #endregion

        #region Control management
        /// <summary>
        /// Clear all controls on this form and set them (if needed) to the default values.
        /// </summary>
        private void ClearFormControls()
        {
            hintTextBoxVersion.Text = "";
            dateTimePickerRelease.Value = DateTime.Now;
            comboBoxLicense.Text = "";
            hintTextBoxMain.Text = "";
            hintTextBoxSelfTest.Text = "";
            hintTextBoxDocDir.Text = "";
            comboBoxStability.SelectedItem = Stability.Unset;
            targetBaseControl.TargetBase = null;
        }

        /// <summary>
        /// Clear all controls on this form and fill them with the values from <see cref="_group"/>.
        /// </summary>
        private void UpdateFormControls()
        {
            ClearFormControls();
            if (_group.Version != null) hintTextBoxVersion.Text = _group.VersionString;
            if (_group.Released != default(DateTime))
            {
                checkBoxEnableSettingDate.Checked = true;
                dateTimePickerRelease.Value = _group.Released;
            }
            if (!string.IsNullOrEmpty(_group.License)) comboBoxLicense.Text = _group.License;
            if (!string.IsNullOrEmpty(_group.Main)) hintTextBoxMain.Text = _group.Main;
            if (!string.IsNullOrEmpty(_group.SelfTest)) hintTextBoxSelfTest.Text = _group.SelfTest;
            if (!string.IsNullOrEmpty(_group.DocDir)) hintTextBoxDocDir.Text = _group.DocDir;
            if (_group.Stability != default(Stability)) comboBoxStability.SelectedItem = _group.Stability;
            targetBaseControl.TargetBase = _group.CloneGroup();
        }
        #endregion

        #region Control validation
        /// <summary>
        /// Sets "hintTextBoxMain.ForeColor" to <see cref="Color.Green"/> if the path is a relative path or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxMainTextChanged(object sender, EventArgs e)
        {
            hintTextBoxMain.ForeColor = !Path.IsPathRooted(hintTextBoxMain.Text) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Set "hintTextBoxVersion.ForeColor" to <see cref="Color.Green"/> if "hintTextBoxVersion.Text" is a valid <see cref="Version"/> or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxVersionTextChanged(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;
            hintTextBoxVersion.ForeColor = (ImplementationVersion.TryCreate(hintTextBoxVersion.Text, out implementationVersion)) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Sets "hintTextBoxDocDir.ForeColor" to <see cref="Color.Green"/> if "hintTextBoxDocDir.Text" is a relative path or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxDocDirTextChanged(object sender, EventArgs e)
        {
            hintTextBoxDocDir.ForeColor = (!Path.IsPathRooted(hintTextBoxDocDir.Text)) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Sets "hintTextBoxSelfTest.ForeColor" to <see cref="Color.Green"/> if "hintTextBoxSelfTest.Text" is a relative path or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxSelfTestTextChanged(object sender, EventArgs e)
        {
            hintTextBoxSelfTest.ForeColor = (!Path.IsPathRooted(hintTextBoxSelfTest.Text)) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Enables or disables <see cref="dateTimePickerRelease"/> when <see cref="checkBoxEnableSettingDate"/> is checked or unchecked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void CheckBoxEnableSettingDateCheckedChanged(object sender, EventArgs e)
        {
            dateTimePickerRelease.Enabled = checkBoxEnableSettingDate.Checked;
        }
        #endregion

        #region Dialog buttons
        /// <summary>
        /// Saves the values from the filled controls to <see cref="_group"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;

            if (ImplementationVersion.TryCreate(hintTextBoxVersion.Text, out implementationVersion)) _group.Version = implementationVersion;
            _group.Released = checkBoxEnableSettingDate.Checked ? dateTimePickerRelease.Value : default(DateTime);
            if (!string.IsNullOrEmpty(comboBoxLicense.Text)) _group.License = comboBoxLicense.Text;
            if (!Path.IsPathRooted(hintTextBoxMain.Text)) _group.Main = hintTextBoxMain.Text;
            if (!Path.IsPathRooted(hintTextBoxSelfTest.Text)) _group.SelfTest = hintTextBoxSelfTest.Text;
            if (!Path.IsPathRooted(hintTextBoxDocDir.Text)) _group.DocDir = hintTextBoxDocDir.Text;
            _group.Stability = (Stability)comboBoxStability.SelectedItem;
            _group.Architecture = targetBaseControl.TargetBase.Architecture;
            foreach (var language in targetBaseControl.TargetBase.Languages)
                _group.Languages.Add(language);
        }
        #endregion
    }
}
