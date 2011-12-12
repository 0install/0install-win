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
using ZeroInstall.Model;

namespace ZeroInstall.Publish.WinForms.Dialogs
{
    public partial class RunnerDialog : DependencyDialog
    {
        public Runner Runner
        {
            get { return Dependency as Runner; }
            set
            {
                #region Sanity checks
                if (value == null) throw new ArgumentNullException("value");
                #endregion

                Dependency = value;

                textCommand.Text = value.Command;

                argumentsControl.Arguments.Clear();
                argumentsControl.Arguments.AddAll(value.Arguments);
            }
        }

        public RunnerDialog()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            Runner.Command = string.IsNullOrEmpty(textCommand.Text) ? null : textCommand.Text;

            Runner.Arguments.Clear();
            Runner.Arguments.AddAll(argumentsControl.Arguments);
        }
    }
}
