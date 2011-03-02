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

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class PackageImplementationForm : OKCancelDialog
    {

        #region Properties

        /// <summary>
        /// The <see cref="PackageImplementation" /> to be displayed and modified by this form.
        /// </summary>
        private PackageImplementation _packageImplementation = new PackageImplementation();

        /// <summary>
        /// The <see cref="PackageImplementation" /> to be displayed and modified by this form. If <see langword="null"/>, the form resets.
        /// </summary>
        public PackageImplementation PackageImplementation
        {
            get { return _packageImplementation; }
            set
            {
                _packageImplementation = value ?? new PackageImplementation();
                UpdateControl();
            }
        }

        #endregion

        #region Initialization

        public PackageImplementationForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Control management

        /// <summary>
        /// Clear all controls on this form.
        /// </summary>
        private void ClearControl()
        {
            comboBoxLicense.SelectedText = "GPL v3 (GNU General Public License)";
            hintTextBoxMain.Text = String.Empty;
            hintTextBoxDocDir.Text = String.Empty;
            hintTextBoxPackage.Text = String.Empty;
            hintTextBoxMain.Text = String.Empty;
            checkedListBoxDistribution.ClearSelected();
        }

        /// <summary>
        /// Clear all controls on this form and set their values from <see cref="_packageImplementation"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
            if (!String.IsNullOrEmpty(_packageImplementation.License)) comboBoxLicense.Text = _packageImplementation.License;
            if (!String.IsNullOrEmpty(_packageImplementation.Main)) hintTextBoxMain.Text = _packageImplementation.Main;
            if (!String.IsNullOrEmpty(_packageImplementation.DocDir)) hintTextBoxDocDir.Text = _packageImplementation.DocDir;
            if (!String.IsNullOrEmpty(_packageImplementation.Package)) hintTextBoxPackage.Text = _packageImplementation.Package;
            if (!String.IsNullOrEmpty(hintTextBoxMain.Text)) hintTextBoxMain.Text = _packageImplementation.Main;
            foreach (var distribution in _packageImplementation.Distributions)
            {
                if (!checkedListBoxDistribution.Items.Contains(distribution)) continue;
                var itemIndex = checkedListBoxDistribution.Items.IndexOf(distribution);
                checkedListBoxDistribution.SetItemChecked(itemIndex, true);
            }
        }

        #endregion

        #region Control validation

        /// <summary>
        /// Sets "hintTextBoxMain.ForeColor" to <see cref="Color.Green"/> if "hintTextBoxMain.Text" is an absolute path or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxMainTextChanged(object sender, EventArgs e)
        {
            hintTextBoxMain.ForeColor = Path.IsPathRooted(hintTextBoxMain.Text) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Sets "hintTextBoxDocDir.ForeColor" to <see cref="Color.Green"/> if "hintTextBoxDocDir.Text" is a relative path or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxDocDirTextChanged(object sender, EventArgs e)
        {
            hintTextBoxDocDir.ForeColor = !Path.IsPathRooted(hintTextBoxDocDir.Text) ? Color.Green : Color.Red;
        }

        #endregion

        #region Dialog buttons

        /// <summary>
        /// Saves the values from the filled controls to <see cref="_packageImplementation"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            _packageImplementation.Main = (Path.IsPathRooted(hintTextBoxMain.Text)) ? hintTextBoxMain.Text : null;
            _packageImplementation.Package = (!String.IsNullOrEmpty(hintTextBoxPackage.Text)) ? hintTextBoxPackage.Text : null;
            _packageImplementation.License = (!String.IsNullOrEmpty(comboBoxLicense.Text)) ? comboBoxLicense.Text : null;
            _packageImplementation.DocDir = (!Path.IsPathRooted(hintTextBoxDocDir.Text)) ? hintTextBoxDocDir.Text : null;
            _packageImplementation.Distributions.Clear();
            foreach (String checkedItem in checkedListBoxDistribution.CheckedItems)
            {
                _packageImplementation.Distributions.Add(checkedItem);
            }
        }
        #endregion
    }
}
