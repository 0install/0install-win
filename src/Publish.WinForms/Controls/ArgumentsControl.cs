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
using System.ComponentModel;
using System.Windows.Forms;

namespace ZeroInstall.Publish.WinForms.Controls
{
    public partial class ArgumentsControl : UserControl
    {
        #region Properties
        /// <summary>
        /// The list for the arguments in <see cref="listBoxArguments"/>.
        /// </summary>
        private readonly C5.ArrayList<String> _arguments = new C5.ArrayList<string>();

        /// <summary>
        /// Returns the entered Arguments. Changing this List means changing the list box with the arguments.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public C5.ArrayList<String> Arguments
        {
            get
            {
                _arguments.CollectionChanged -= CollectionChanged;
                _arguments.Clear();
                foreach (string argument in listBoxArguments.Items)
                    _arguments.Add(argument);
                _arguments.CollectionChanged += CollectionChanged;
                return _arguments;
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the control.
        /// </summary>
        public ArgumentsControl()
        {
            InitializeComponent();
            InitializePropertyArguments();
            InitializeListButtons();
            InitializeListBoxArguments();
        }

        /// <summary>
        /// Sets the <see cref="CollectionChanged"/> event.
        /// </summary>
        private void InitializePropertyArguments()
        {
            _arguments.CollectionChanged += CollectionChanged;
        }

        /// <summary>
        /// Sets the clicking delegates of the buttons <see cref="buttonAddArgument"/>, <see cref="buttonRemoveArgument"/> and <see cref="buttonClearArguments"/>.
        /// </summary>
        private void InitializeListButtons()
        {
            // adds item of the textBox to the listBox
            buttonAddArgument.Click += (sender, eventArgs) =>
            {
                if (string.IsNullOrEmpty(textBoxArgument.Text)) return;
                listBoxArguments.Items.Add(textBoxArgument.Text);
                textBoxArgument.Text = "";
            };
            // removes selected item from listBox
            buttonRemoveArgument.Click += (sender, eventArgs) =>
            {
                if (listBoxArguments.SelectedItem == null) return;
                listBoxArguments.Items.Remove(listBoxArguments.SelectedItem);
            };
            // clears textBox and listBox
            buttonClearArguments.Click += (sender, eventArgs) => ClearControl();
        }

        /// <summary>
        /// Sets the clicking delegate for <see cref="listBoxArguments"/>.
        /// </summary>
        private void InitializeListBoxArguments()
        {
            // Copies the clicked text to the textBox
            listBoxArguments.Click += (sender, eventArgs) =>
            {
                if (listBoxArguments.SelectedItem != null)
                    textBoxArgument.Text = (String)listBoxArguments.SelectedItem;
            };
        }
        #endregion

        #region Control Management
        /// <summary>
        /// Clears <see cref="textBoxArgument"/> and <see cref="listBoxArguments"/>.
        /// </summary>
        private void ClearControl()
        {
            textBoxArgument.Text = "";
            listBoxArguments.Items.Clear();
        }

        private void UpdateControl()
        {
            listBoxArguments.Items.Clear();
            listBoxArguments.Items.AddRange(_arguments.ToArray());
        }
        #endregion

        /// <summary>
        /// Updates the control when invoked.
        /// </summary>
        /// <param name="value">Not used.</param>
        private void CollectionChanged(object value)
        {
            UpdateControl();
        }
    }
}
