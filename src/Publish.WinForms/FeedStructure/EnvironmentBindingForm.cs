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

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class EnvironmentBindingForm : OKCancelDialog
    {
        #region Properties
        private EnvironmentBinding _environmentBinding = new EnvironmentBinding();

        public EnvironmentBinding EnvironmentBinding
        {
            get { return _environmentBinding; }
            set
            {
                _environmentBinding = (value ?? new EnvironmentBinding());
                UpdateControl();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Creates a new <see cref="EnvironmentBindingForm"/> object.
        /// </summary>
        public EnvironmentBindingForm()
        {
            InitializeComponent();
            InitializeComboBoxMode();
        }

        /// <summary>
        /// Fills <see cref="comboBoxMode"/> with the items of <see cref="EnvironmentMode"/>.
        /// </summary>
        private void InitializeComboBoxMode()
        {
            //TODO Change methode that localisation is possible (Stability only contains the english names). 
            foreach (EnvironmentMode mode in Enum.GetValues(typeof(EnvironmentMode)))
                comboBoxMode.Items.Add(mode);
        }
        #endregion

        #region Control management methodes
        /// <summary>
        /// Clear all controls on this form and set them (if needed) to the default values.
        /// </summary>
        private void ClearControl()
        {
            comboBoxName.SelectedIndex = 0;
            hintTextBoxInsert.Text = "";
            comboBoxMode.SelectedIndex = 0;
            hintTextBoxSeparator.Text = "";
            hintTextBoxDefault.Text = "";
        }

        /// <summary>
        /// Clear all controls on this form and fill them with the values from <see cref="_environmentBinding"/>.
        /// </summary>
        private void UpdateControl()
        {
            ClearControl();
            if (!String.IsNullOrEmpty(_environmentBinding.Name))
            {
                comboBoxName.Items.Add(_environmentBinding.Name);
                comboBoxName.SelectedItem = _environmentBinding.Name;
            }
            hintTextBoxInsert.Text = _environmentBinding.Insert;
            comboBoxMode.SelectedItem = _environmentBinding.Mode;
            hintTextBoxSeparator.Text = _environmentBinding.Separator;
            hintTextBoxDefault.Text = _environmentBinding.Default;
        }
        #endregion

        #region Dialog buttons
        /// <summary>
        /// Saves the values from the filled controls to <see cref="_environmentBinding"/> and closes the window.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonOkClick(object sender, EventArgs e)
        {
            _environmentBinding.Name = (!string.IsNullOrEmpty(comboBoxName.Text) ? comboBoxName.Text : null);
            _environmentBinding.Insert = (!string.IsNullOrEmpty(hintTextBoxInsert.Text) ? hintTextBoxInsert.Text : null);
            _environmentBinding.Mode = (EnvironmentMode)comboBoxMode.SelectedItem;
            _environmentBinding.Separator = (!string.IsNullOrEmpty(hintTextBoxSeparator.Text) ? hintTextBoxSeparator.Text : null);
            _environmentBinding.Default = (!string.IsNullOrEmpty(hintTextBoxDefault.Text) ? hintTextBoxDefault.Text : null);
        }
        #endregion
    }
}
