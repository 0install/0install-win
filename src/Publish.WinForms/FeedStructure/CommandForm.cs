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

using C5;
using Common.Controls;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.FeedStructure
{
    public partial class CommandForm : OKCancelDialog
    {
        #region Properties
        private Command _command = new Command();

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
        public CommandForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Control management
        private void ClearFormControls()
        {
            textBoxName.Text = string.Empty;
            textBoxPath.Text = string.Empty;
            argumentsControl1.Arguments = null;
            checkBoxWorkingDir.Checked = false;
            hintTextBoxSource.Enabled = false;
            hintTextBoxSource.Text = string.Empty;
        }

        private void UpdateControlForms()
        {
            ClearFormControls();

            textBoxName.Text = _command.Name;
            textBoxPath.Text = _command.Path;
            argumentsControl1.Arguments = (ArrayList<string>) _command.Arguments.Clone();
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

        #region
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
