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
using System.Windows.Forms;
using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class ImplementationForm : OKCancelDialog
    {

        #region Properties

        /// <summary>
        /// The <see cref="Implementation" /> to be displayed and modified by this form.
        /// </summary>
        private Implementation _implementation = new Implementation();

        /// <summary>
        /// The <see cref="Implementation" /> to be displayed and modified by this form. If <see langword="null"/>, the controls will be reset.
        /// </summary>
        public Implementation Implementation
        {
            get { return _implementation; }
            set
            {
                _implementation = value ?? new Implementation();
                targetBaseControl.TargetBase = _implementation.CloneImplementation();
                UpdateFormControls();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new <see cref="ImplementationForm"/> object.
        /// </summary>
        public ImplementationForm()
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
            {
                comboBoxStability.Items.Add(stability);
            }
        }

        #endregion

        #region Control management
        /// <summary>
        /// Clear all controls on this form and set them (if needed) to the default values.
        /// </summary>
        private void ClearFormControls()
        {
            hintTextBoxVersion.Text = string.Empty;
            dateTimePickerRelease.Value = DateTime.Now;
            comboBoxStability.SelectedItem = Stability.Unset;
            comboBoxLicense.Text = String.Empty;
            hintTextBoxID.Text = String.Empty;
            hintTextBoxMain.Text = String.Empty;
            hintTextBoxDocDir.Text = String.Empty;
            hintTextBoxSelfTest.Text = String.Empty;
            hintTextBoxLocalPath.Text = String.Empty;
            targetBaseControl.TargetBase = null;
        }

        /// <summary>
        /// Clear all controls on this form and fill them with the values from <see cref="_implementation"/>.
        /// </summary>
        private void UpdateFormControls()
        {
            ClearFormControls();
            if(!String.IsNullOrEmpty(_implementation.VersionString)) hintTextBoxVersion.Text = _implementation.VersionString;
            if (_implementation.Released != default(DateTime))
            {
                checkBoxSettingDateEnable.Checked = true;
                dateTimePickerRelease.Value = _implementation.Released;
            }
            if (_implementation.Stability != default(Stability)) comboBoxStability.SelectedItem = _implementation.Stability;
            if (!String.IsNullOrEmpty(_implementation.License)) comboBoxLicense.Text = _implementation.License;
            if (!String.IsNullOrEmpty(_implementation.ID)) hintTextBoxID.Text = _implementation.ID;
            if (!String.IsNullOrEmpty(_implementation.Main)) hintTextBoxMain.Text = _implementation.Main;
            if (!String.IsNullOrEmpty(_implementation.DocDir)) hintTextBoxDocDir.Text = _implementation.DocDir;
            if (!String.IsNullOrEmpty(_implementation.SelfTest)) hintTextBoxSelfTest.Text = _implementation.SelfTest;
            if (!String.IsNullOrEmpty(_implementation.LocalPath)) hintTextBoxLocalPath.Text = _implementation.LocalPath;
            targetBaseControl.TargetBase = _implementation.CloneImplementation();
        }
        #endregion

        #region Control validation
        private void HintTextBoxVersionTextChanged(object sender, EventArgs e)
        {
            ImplementationVersion version;
            hintTextBoxVersion.ForeColor = ImplementationVersion.TryCreate(hintTextBoxVersion.Text, out version) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Enables or disables <see cref="dateTimePickerRelease"/> when <see cref="checkBoxSettingDateEnable"/> is checked or unchecked.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void CheckBoxSettingDateEnableCheckedChanged(object sender, EventArgs e)
        {
            dateTimePickerRelease.Enabled = checkBoxSettingDateEnable.Checked;
        }
        #endregion

        #region Buttons
        private void ButtonShowManifestDigestClick(object sender, EventArgs e)
        {
            using(var dialog = new ManifestDigestForm(_implementation.ManifestDigest))
            {
                dialog.MaximizeBox = false;
                dialog.ShowDialog();
            }}
        #endregion

        #region Dialog buttons

        /// <summary>
        /// Saves the values from the filled controls to <see cref="_implementation"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            ImplementationVersion version;

            _implementation.Version = (ImplementationVersion.TryCreate(hintTextBoxVersion.Text, out version)) ? version : null;
            _implementation.Released = checkBoxSettingDateEnable.Checked ? dateTimePickerRelease.Value : default(DateTime);
            _implementation.Stability = (Stability) comboBoxStability.SelectedItem;
            _implementation.License = (!String.IsNullOrEmpty(comboBoxLicense.Text)) ? comboBoxLicense.Text : null;
            _implementation.ID = (!String.IsNullOrEmpty(hintTextBoxID.Text)) ? hintTextBoxID.Text : null;
            _implementation.Main = (!String.IsNullOrEmpty(hintTextBoxMain.Text)) ? hintTextBoxMain.Text : null;
            _implementation.DocDir = (!String.IsNullOrEmpty(hintTextBoxDocDir.Text)) ? hintTextBoxDocDir.Text : null;
            _implementation.SelfTest = (!String.IsNullOrEmpty(hintTextBoxSelfTest.Text)) ? hintTextBoxSelfTest.Text : null;
            _implementation.LocalPath = (!String.IsNullOrEmpty(hintTextBoxLocalPath.Text)) ? hintTextBoxLocalPath.Text : null;
            foreach (var language in targetBaseControl.TargetBase.Languages)
            {
                _implementation.Languages.Add(language);
            }
            _implementation.Architecture = targetBaseControl.TargetBase.Architecture;
        }

        #endregion
    }
}
