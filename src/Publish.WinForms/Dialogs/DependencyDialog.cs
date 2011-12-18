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
using Constraint = ZeroInstall.Model.Constraint;

namespace ZeroInstall.Publish.WinForms.Dialogs
{
    public partial class DependencyDialog : OKCancelDialog
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
        public DependencyDialog()
        {
            InitializeComponent();

            comboBoxImportance.Items.AddRange(new object[] {Importance.Essential, Importance.Recommended});
        }
        #endregion

        #region Control management methodes
        /// <summary>
        /// Clears all controls on this form.
        /// </summary>
        private void ClearFormControls()
        {
            hintTextBoxInterface.Text = "";
            hintTextBoxUse.Text = "";
            hintTextBoxNotBefore.Text = "";
            hintTextBoxBefore.Text = "";
            comboBoxImportance.SelectedItem = Importance.Essential;
            listBoxConstraints.Items.Clear();
        }

        /// <summary>
        /// Clears all controls on this form and set their values from <see cref="_dependency"/>.
        /// </summary>
        private void UpdateFormControls()
        {
            ClearFormControls();
            hintTextBoxInterface.Text = _dependency.Interface;
            hintTextBoxUse.Text = _dependency.Use;
            comboBoxImportance.SelectedItem = _dependency.Importance;
            foreach (var constraint in _dependency.Constraints)
                listBoxConstraints.Items.Add(constraint);
        }
        #endregion

        #region Constraint methodes
        /// <summary>
        /// Adds a <see cref="Constraint"/> from "hintTextBoxNotBefore.Text" and/or "hintTextBoxBefore.Text" to "listBoxConstraints.Items".
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonConstraintAddClick(object sender, EventArgs e)
        {
            Constraint constraint;
            ImplementationVersion notBefore, before;

            // add NotBefore and Before
            if (ImplementationVersion.TryCreate(hintTextBoxNotBefore.Text, out notBefore) & ImplementationVersion.TryCreate(hintTextBoxBefore.Text, out before))
                constraint = new Constraint(notBefore, before);
                //add only NotBefore
            else if (notBefore != null)
                constraint = new Constraint(notBefore, null);
                //add only Before
            else
                constraint = new Constraint(null, before);
            // add to the list if it is not allready in the list
            if (!listBoxConstraints.Items.Contains(constraint))
                listBoxConstraints.Items.Add(constraint);
        }

        /// <summary>
        /// Removes the selected item from <see cref="listBoxConstraints"/>.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonConstraintRemoveClick(object sender, EventArgs e)
        {
            var selectedVersion = (Constraint)listBoxConstraints.SelectedItem;
            listBoxConstraints.Items.Remove(selectedVersion);
        }

        /// <summary>
        /// Checks if "hintTextBoxNotBefore.Text" for a valid <see cref="ImplementationVersion"/> and sets hintTextBoxNotBefore.ForeColor to <see cref="Color.Green"/> if its valid or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxNotBeforeTextChanged(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;
            hintTextBoxNotBefore.ForeColor = ImplementationVersion.TryCreate(hintTextBoxNotBefore.Text, out implementationVersion) ? Color.Green : Color.Red;
            CheckConstraints();
        }

        /// <summary>
        /// Checks if "hintTextBoxBefore.Text" for a valid <see cref="ImplementationVersion"/> and sets "hintTextBoxBefore.ForeColor" to <see cref="Color.Green"/> if its valid or to <see cref="Color.Red"/> if not.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxBeforeTextChanged(object sender, EventArgs e)
        {
            ImplementationVersion implementationVersion;
            hintTextBoxBefore.ForeColor = ImplementationVersion.TryCreate(hintTextBoxBefore.Text, out implementationVersion) ? Color.Green : Color.Red;
            CheckConstraints();
        }

        /// <summary>
        /// Checks if "hintTextBoxNotBefore.Text" or "hintTextBoxBefore.Text" have a valid <see cref="ImplementationVersion"/> and enables <see cref="buttonConstraintAdd"/> if its valid or disables if not.
        /// </summary>
        private void CheckConstraints()
        {
            ImplementationVersion implementationVersion;
            buttonConstraintAdd.Enabled = ImplementationVersion.TryCreate(hintTextBoxNotBefore.Text, out implementationVersion) || ImplementationVersion.TryCreate(hintTextBoxBefore.Text, out implementationVersion);
        }

        /// <summary>
        /// Sets "hintTextBoxBefore.Text" and "hintTextBoxBefore.Text" with the values from a <see cref="ImplementationVersion"/> from listBoxConstraints.SelectedItem
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ListBoxConstraintsSelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxConstraints.SelectedItem == null) return;
            var selectedItem = (Constraint)listBoxConstraints.SelectedItem;
            hintTextBoxBefore.Text = selectedItem.BeforeVersionString ?? "";
            hintTextBoxNotBefore.Text = selectedItem.NotBeforeVersionString ?? "";
        }
        #endregion

        #region Interface and Use methodes
        /// <summary>
        /// Checks if "hintTextBoxInterface.Text" is a valid interface url and sets "hintTextBoxInterface.ForeColor" to <see cref="Color.Green"/> or to <see cref="Color.Red"/> if its not valid.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void HintTextBoxInterfaceTextChanged(object sender, EventArgs e)
        {
            Uri uri;
            hintTextBoxInterface.ForeColor = (ModelUtils.TryParseUri(hintTextBoxInterface.Text, out uri)) ? Color.Green : Color.Red;
        }
        #endregion

        #region Dialog buttons
        /// <summary>
        /// Saves the values from the filled controls to <see cref="_dependency"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            _dependency.Interface = hintTextBoxInterface.Text;
            _dependency.Use = (!string.IsNullOrEmpty(hintTextBoxUse.Text) ? hintTextBoxUse.Text : null);
            foreach (Constraint constraint in listBoxConstraints.Items)
                _dependency.Constraints.Add(constraint);

            if (comboBoxImportance.SelectedItem is Importance) _dependency.Importance = (Importance)comboBoxImportance.SelectedItem;
        }
        #endregion
    }
}
