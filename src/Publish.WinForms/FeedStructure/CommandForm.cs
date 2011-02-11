/*
 * Copyright 2011 Simon E. Silva Lauinger
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

using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class CommandForm : OKCancelDialog
    {
        #region Properties
        /// <summary>
        /// The <see cref="Command"/> to edit by this form.
        /// </summary>
        private Command _command = new Command();

        /// <summary>
        /// The <see cref="Command"/> to edit by this form.
        /// </summary>
        public Command Command
        {
            get { return _command; }
            set
            {
                _command = value ?? new Command();
                UpdateControlForms();
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the form.
        /// </summary>
        public CommandForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Control management
        /// <summary>
        /// Clears all controls on the form.
        /// </summary>
        private void ClearFormControls()
        {
            textBoxName.Text = string.Empty;
            textBoxPath.Text = string.Empty;
            argumentsControl1.Arguments.Clear();
            checkBoxWorkingDir.Checked = false;
            hintTextBoxSource.Enabled = false;
            hintTextBoxSource.Text = string.Empty;
        }

        /// <summary>
        /// Enters the values of <see cref="_command"/> into the form controls. Controls for not setted values will be cleared.
        /// </summary>
        private void UpdateControlForms()
        {
            ClearFormControls();

            textBoxName.Text = _command.Name;
            textBoxPath.Text = _command.Path;
            argumentsControl1.Arguments.AddAll(_command.Arguments);
            if (_command.WorkingDir != null)
            {
                checkBoxWorkingDir.Checked = true;
                hintTextBoxSource.Text = _command.WorkingDir.Source;
            }
            else
            {
                checkBoxWorkingDir.Checked = false;
            }
        }
        #endregion

        #region WorkingDir Group
        private void CheckBoxWorkingDirCheckedChanged(object sender, System.EventArgs e)
        {
            hintTextBoxSource.Enabled = checkBoxWorkingDir.Checked;
        }

        #endregion

        #region Dialog Buttons
        /// <summary>
        /// Saves the values from the controls to <see cref="_command"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOkClick(object sender, System.EventArgs e)
        {
            _command.Name = textBoxName.Text;
            _command.Path = string.IsNullOrEmpty(textBoxPath.Text) ? null : textBoxPath.Text;
            _command.Arguments.Clear();
            _command.Arguments.AddAll(argumentsControl1.Arguments);
            _command.WorkingDir = checkBoxWorkingDir.Checked ? new WorkingDir { Source = hintTextBoxSource.Text } : null;
        }
        #endregion
    }
}
