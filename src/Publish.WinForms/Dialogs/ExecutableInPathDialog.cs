/*
 * Copyright 2011 Bastian Eicher
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
using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.Dialogs
{
    public partial class ExecutableInPathDialog : OKCancelDialog
    {
        #region Properties
        /// <summary>
        /// The <see cref="ExecutableInPath" /> to be displayed and modified by this form.
        /// </summary>
        private ExecutableInPath _executableBinding = new ExecutableInPath();

        /// <summary>
        /// The <see cref="ExecutableInPath" /> to be displayed and modified by this form. If <see langword="null"/>, the form resets.
        /// </summary>
        public ExecutableInPath ExecutableInPath
        {
            get { return _executableBinding; }
            set
            {
                _executableBinding = value ?? new ExecutableInPath();
                UpdateControl();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Creates a new <see cref="ExecutableInPathDialog"/> object.
        /// </summary>
        public ExecutableInPathDialog()
        {
            InitializeComponent();
        }
        #endregion

        #region Control management methodes
        /// <summary>
        /// Clear all controls on this form.
        /// </summary>
        private void ClearControl()
        {
            hintTextBoxName.Text = "";
            hintTextBoxCommand.Text = "";
        }

        /// <summary>
        /// Clear all controls in this form and set their values from <see cref="_executableBinding"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
            if (!string.IsNullOrEmpty(_executableBinding.Name)) hintTextBoxName.Text = _executableBinding.Name;
            if (!string.IsNullOrEmpty(_executableBinding.Command)) hintTextBoxCommand.Text = _executableBinding.Command;
        }
        #endregion

        #region Dialog buttons
        /// <summary>
        /// Saves the values from the filled controls to <see cref="_executableBinding"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            _executableBinding.Name = (!string.IsNullOrEmpty(hintTextBoxName.Text)) ? hintTextBoxName.Text : null;
            _executableBinding.Command = (!string.IsNullOrEmpty(hintTextBoxCommand.Text)) ? hintTextBoxCommand.Text : null;
        }
        #endregion
    }
}
