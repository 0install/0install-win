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
using System.IO;
using System.Windows.Forms;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class OverlayBindingForm : Form
    {

        #region Properties

        /// <summary>
        /// The <see cref="OverlayBinding" /> to be displayed and modified by this form.
        /// </summary>
        private OverlayBinding _overlayBinding = new OverlayBinding();

        /// <summary>
        /// The <see cref="OverlayBinding" /> to be displayed and modified by this form. If <see langword="null"/>, the form resets.
        /// </summary>
        public OverlayBinding OverlayBinding
        {
            get { return _overlayBinding; }
            set
            {
                _overlayBinding = value ?? new OverlayBinding();
                UpdateControl();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new <see cref="OverlayBindingForm"/> object.
        /// </summary>
        public OverlayBindingForm()
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
            hintTextBoxSrc.Text = String.Empty;
            hintTextBoxMountPoint.Text = String.Empty;
        }

        /// <summary>
        /// Clear all controls in this form and set their values from <see cref="_overlayBinding"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
            if (!String.IsNullOrEmpty(_overlayBinding.Source)) hintTextBoxSrc.Text = _overlayBinding.Source;
            if (!String.IsNullOrEmpty(_overlayBinding.MountPoint)) hintTextBoxMountPoint.Text = _overlayBinding.MountPoint;
        }

        #endregion

        #region Control validation

        /// <summary>
        /// Sets the color of "hintTextBoxMountPoint.Text" to <see cref="Color.Green"/> if "hintTextBoxMountPoint.Text" is NOT rooted or to <see cref="Color.Red"/> if it is.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxSrc_TextChanged(object sender, EventArgs e)
        {
            hintTextBoxSrc.ForeColor = (!Path.IsPathRooted(hintTextBoxSrc.Text)) ? Color.Green : Color.Red;
        }

        /// <summary>
        /// Sets the color of "hintTextBoxMountPoint.Text" to <see cref="Color.Green"/> if "hintTextBoxMountPoint.Text" is NOT rooted or to <see cref="Color.Red"/> if it is.
        /// Sets _overlayBinding.MountPoint if hintTextBoxMountPoint.Text is a valid mount point.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void hintTextBoxMountPoint_TextChanged(object sender, EventArgs e)
        {
            hintTextBoxMountPoint.ForeColor = (Path.IsPathRooted(hintTextBoxMountPoint.Text)) ? Color.Green : Color.Red;
        }

        #endregion

        #region Dialog buttons

        /// <summary>
        /// Saves the values from the filled controls to <see cref="_overlayBinding"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            _overlayBinding.Source = (!String.IsNullOrEmpty(hintTextBoxSrc.Text)) ? hintTextBoxSrc.Text : null;
            _overlayBinding.MountPoint = (!String.IsNullOrEmpty(hintTextBoxMountPoint.Text)) ? hintTextBoxMountPoint.Text : null;
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
