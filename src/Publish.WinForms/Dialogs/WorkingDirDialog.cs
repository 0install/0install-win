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
using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.Dialogs
{
    public partial class WorkingDirDialog : OKCancelDialog
    {
        #region Properties
        /// <summary>
        /// The <see cref="WorkingDir"/> to shown and edited by this form.
        /// </summary>
        private WorkingDir _workingDir = new WorkingDir();

        /// <summary>
        /// The <see cref="WorkingDir"/> to show and edit by this form.
        /// </summary>
        public WorkingDir WorkingDir
        {
            get { return _workingDir; }
            set
            {
                _workingDir = value ?? new WorkingDir();
                UpdateControl();
            }
        }
        #endregion

        #region Initialization
        public WorkingDirDialog()
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
            hintTextBoxSource.Text = "";
        }

        /// <summary>
        /// Clear all controls in this form and set their values from <see cref="WorkingDir"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();

            hintTextBoxSource.Text = WorkingDir.Source;
        }
        #endregion

        #region Dialog buttons
        /// <summary>
        /// Saves the values from the filled controls to <see cref="WorkingDir"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            string workingDir = hintTextBoxSource.Text;
            WorkingDir.Source = string.IsNullOrEmpty(workingDir) ? "." : workingDir;
        }
        #endregion
    }
}
