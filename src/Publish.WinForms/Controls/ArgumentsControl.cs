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

using System;
using System.Windows.Forms;
using C5;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class ArgumentsControl : UserControl
    {
        #region Properties

        private ArrayList<String> _arguments;

        public ArrayList<String> Arguments
        {
            get
            {
                _arguments.Clear();
                foreach (string argument in listBoxArguments.Items)
                {
                    _arguments.Add(argument);
                }
                return _arguments;
            }
            set
            {
                _arguments = value ?? new ArrayList<string>();
                UpdateControls();
            }
        }

        #endregion

        #region Initialization
        public ArgumentsControl()
        {
            InitializeComponent();
            InitializeListButtons();
            InitializeListBoxArguments();
        }

        private void InitializeListButtons()
        {
            buttonAddArgument.Click += (sender, eventArgs) =>
                                           {
                                               if (string.IsNullOrEmpty(textBoxArgument.Text)) return;
                                               listBoxArguments.Items.Add(textBoxArgument.Text);
                                               textBoxArgument.Text = string.Empty;
                                           };
            buttonRemoveArgument.Click += (sender, eventArgs) =>
                                              {
                                                  if (listBoxArguments.SelectedItem == null) return;
                                                  listBoxArguments.Items.Remove(listBoxArguments.SelectedItem);
                                              };
            buttonClearArguments.Click += (sender, eventArgs) => listBoxArguments.Items.Clear();
        }

        private void InitializeListBoxArguments()
        {
            listBoxArguments.Click += (sender, eventArgs) =>
                                          {
                                              if(listBoxArguments.SelectedItem != null)
                                                textBoxArgument.Text = (String) listBoxArguments.SelectedItem;
                                          };
        }
        #endregion

        #region Control Management
        private void UpdateControls()
        {
            ClearControls();
            listBoxArguments.Items.AddRange(_arguments.ToArray());
        }

        private void ClearControls()
        {
            listBoxArguments.Items.Clear();
        }
        #endregion
    }
}
