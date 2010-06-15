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
using ZeroInstall.Model;
using Constraint = ZeroInstall.Model.Constraint;

namespace ZeroInstall.Publish.WinForms
{
    public partial class DependencyForm : Form
    {

        #region Properties
        private Dependency _dependency = new Dependency();

        public Dependency Dependency
        {
            get { return _dependency; }
            set
            {
                _dependency = value ?? new Dependency();
                UpdateFormControls();
            }
        }

        #endregion

        # region Initialzation
        
        public DependencyForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Control management methodes

        /// <summary>
        /// Clears all controls on this form.
        /// </summary>
        private void ClearFormControls()
        {
            hintTextBoxInterface.Text = String.Empty;
            hintTextBoxUse.Text = String.Empty;
            hintTextBoxNotBefore.Text = String.Empty;
            hintTextBoxBefore.Text = String.Empty;
            listBoxConstraints.Items.Clear();
        }

        /// <summary>
        /// Clears all controls on this form and set their values from <see cref="_dependency"/>.
        /// </summary>
        private void UpdateFormControls()
        {
            ClearFormControls();
            if (_dependency.Interface != null) hintTextBoxInterface.Text = _dependency.InterfaceString;
            if (!String.IsNullOrEmpty(_dependency.Use)) hintTextBoxUse.Text = _dependency.Use;
            foreach (var constraint in _dependency.Constraints)
            {
                listBoxConstraints.Items.Add(constraint);
            }
        }

        #endregion

        #region Constraint methodes

        /// <summary>
        /// Adds a <see cref="Constraint"/> from "hintTextBoxNotBefore.Text" and/or "hintTextBoxBefore.Text" to "listBoxConstraints.Items".
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonConstraintAdd_Click(object sender, EventArgs e)
        {
            Constraint constraint;
            ImplementationVersion notBefore = null, before = null;

            // add NotBefore and Before)
            if (ImplementationVersion.TryCreate(hintTextBoxNotBefore.Text, out notBefore) & ImplementationVersion.TryCreate(hintTextBoxBefore.Text, out before))
            {
                constraint = new Constraint(notBefore, before);
            }
                //add only NotBefore
            else if (notBefore != null)
            {
                constraint = new Constraint(notBefore, null);
            }
                //add only Before
            else
            {
                constraint = new Constraint(null, before);
            }
            // add to the list if it is not allready in the list
            if (!listBoxConstraints.Items.Contains(constraint))
                listBoxConstraints.Items.Add(constraint);
        }

        /// <summary>
        /// Removes the selected item from <see cref="listBoxConstraints"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonConstraintRemove_Click(object sender, EventArgs e)
        {
            var selectedVersion = (Constraint)listBoxConstraints.SelectedItem;
            listBoxConstraints.Items.Remove(selectedVersion);
        }

        /// <summary>
        /// Checks if "hintTextBoxNotBefore.Text" for a valid <see cref="ImplementationVersion"/> and sets hintTextBoxNotBefore.ForeColor to <see cref="Color.Green"/> if its valid or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxNotBefore_TextChanged(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;
            hintTextBoxNotBefore.ForeColor = ImplementationVersion.TryCreate(hintTextBoxNotBefore.Text, out implementationVersion) ? Color.Green : Color.Red;
            checkConstraints();
        }

        /// <summary>
        /// Checks if "hintTextBoxBefore.Text" for a valid <see cref="ImplementationVersion"/> and sets "hintTextBoxBefore.ForeColor" to <see cref="Color.Green"/> if its valid or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxBefore_TextChanged(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;
            hintTextBoxBefore.ForeColor = ImplementationVersion.TryCreate(hintTextBoxBefore.Text, out implementationVersion) ? Color.Green : Color.Red;
            checkConstraints();
        }

        /// <summary>
        /// Checks if "hintTextBoxNotBefore.Text" or "hintTextBoxBefore.Text" have a valid <see cref="ImplementationVersion"/> and enables <see cref="buttonConstraintAdd"/> if its valid or disables if not.
        /// </summary>
        private void checkConstraints()
        {
            ImplementationVersion implementationVersion;
            buttonConstraintAdd.Enabled = ImplementationVersion.TryCreate(hintTextBoxNotBefore.Text, out implementationVersion) || ImplementationVersion.TryCreate(hintTextBoxBefore.Text, out implementationVersion);
        }

        /// <summary>
        /// Sets "hintTextBoxBefore.Text" and "hintTextBoxBefore.Text" with the values from a <see cref="ImplementationVersion"/> from listBoxConstraints.SelectedItem
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void listBoxConstraints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxConstraints.SelectedItem == null) return;
            var selectedItem = (Constraint)listBoxConstraints.SelectedItem;
            hintTextBoxBefore.Text = selectedItem.BeforeVersionString ?? String.Empty;
            hintTextBoxNotBefore.Text = selectedItem.NotBeforeVersionString ?? String.Empty;
        }

        #endregion

        #region Interface and Use methodes

        /// <summary>
        /// Checks if "hintTextBoxInterface.Text" is a valid interface url and sets "hintTextBoxInterface.ForeColor" to <see cref="Color.Green"/> or to <see cref="Color.Red"/> if its not valid.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxInterface_TextChanged(object sender, EventArgs e)
        {
            Uri uri;
            hintTextBoxInterface.ForeColor = (ControlHelpers.IsValidFeedUrl(hintTextBoxInterface.Text, out uri)) ? Color.Green : Color.Red;
        }
        #endregion

        #region Dialog buttons

        /// <summary>
        /// Saves the values from the filled controls to <see cref="_dependency"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonOk_Click(object sender, EventArgs e)
        {
            Uri interfaceUrl;
            _dependency.Interface = (ControlHelpers.IsValidFeedUrl(hintTextBoxInterface.Text, out interfaceUrl)) ? interfaceUrl : null;
            _dependency.Use = (!String.IsNullOrEmpty(hintTextBoxUse.Text)) ? hintTextBoxUse.Text : String.Empty;
            foreach (Constraint constraint in listBoxConstraints.Items)
            {
                _dependency.Constraints.Add(constraint);
            }

            buttonCancel_Click(null, null);
        }

        /// <summary>
        /// Closes the window WITHOUT saving the values from the controls.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
            Dispose();
        }

        #endregion
    }
}
