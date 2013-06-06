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

namespace ZeroInstall.Publish.WinForms.Dialogs
{
    public partial class ImplementationDialog : OKCancelDialog, IEditorDialog<Implementation>
    {
        public DialogResult ShowDialog(IWin32Window owner, Implementation element)
        {
            Implementation = element;
            return ShowDialog(owner);
        }

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
        /// Creates a new <see cref="ImplementationDialog"/> object.
        /// </summary>
        public ImplementationDialog()
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
            comboBoxStability.SelectedItem = Stability.Unset;
            comboBoxLicense.Text = "";
            hintTextBoxID.Text = "";
            hintTextBoxMain.Text = "";
            hintTextBoxDocDir.Text = "";
            hintTextBoxSelfTest.Text = "";
            hintTextBoxLocalPath.Text = "";
            targetBaseControl.TargetBase = null;
        }

        /// <summary>
        /// Clear all controls on this form and fill them with the values from <see cref="_implementation"/>.
        /// </summary>
        private void UpdateFormControls()
        {
            ClearFormControls();
            if (!string.IsNullOrEmpty(_implementation.VersionString)) hintTextBoxVersion.Text = _implementation.VersionString;
            if (_implementation.Released != default(DateTime))
            {
                checkBoxSettingDateEnable.Checked = true;
                dateTimePickerRelease.Value = _implementation.Released;
            }
            if (_implementation.Stability != default(Stability)) comboBoxStability.SelectedItem = _implementation.Stability;
            if (!string.IsNullOrEmpty(_implementation.License)) comboBoxLicense.Text = _implementation.License;
            if (!string.IsNullOrEmpty(_implementation.ID)) hintTextBoxID.Text = _implementation.ID;
            if (!string.IsNullOrEmpty(_implementation.Main)) hintTextBoxMain.Text = _implementation.Main;
            if (!string.IsNullOrEmpty(_implementation.DocDir)) hintTextBoxDocDir.Text = _implementation.DocDir;
            if (!string.IsNullOrEmpty(_implementation.SelfTest)) hintTextBoxSelfTest.Text = _implementation.SelfTest;
            if (!string.IsNullOrEmpty(_implementation.LocalPath)) hintTextBoxLocalPath.Text = _implementation.LocalPath;
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
            using (var dialog = new ManifestDigestDialog(_implementation.ManifestDigest))
                dialog.ShowDialog();
        }
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
            _implementation.Stability = (Stability)comboBoxStability.SelectedItem;
            _implementation.License = (!string.IsNullOrEmpty(comboBoxLicense.Text)) ? comboBoxLicense.Text : null;
            _implementation.ID = (!string.IsNullOrEmpty(hintTextBoxID.Text)) ? hintTextBoxID.Text : null;
            _implementation.Main = (!string.IsNullOrEmpty(hintTextBoxMain.Text)) ? hintTextBoxMain.Text : null;
            _implementation.DocDir = (!string.IsNullOrEmpty(hintTextBoxDocDir.Text)) ? hintTextBoxDocDir.Text : null;
            _implementation.SelfTest = (!string.IsNullOrEmpty(hintTextBoxSelfTest.Text)) ? hintTextBoxSelfTest.Text : null;
            _implementation.LocalPath = (!string.IsNullOrEmpty(hintTextBoxLocalPath.Text)) ? hintTextBoxLocalPath.Text : null;
            foreach (var language in targetBaseControl.TargetBase.Languages)
                _implementation.Languages.Add(language);
            _implementation.Architecture = targetBaseControl.TargetBase.Architecture;
        }
        #endregion
    }
}
