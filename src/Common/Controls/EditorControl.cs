/*
 * Copyright 2006-2013 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Windows.Forms;
using Common.Properties;
using Common.Undo;

namespace Common.Controls
{
    /// <summary>
    /// Edits arbitrary types of elements using a <see cref="ResettablePropertyGrid"/>. Provides optional <see cref="Common.Undo"/> support.
    /// </summary>
    /// <typeparam name="T">The type of element to edit.</typeparam>
    public partial class EditorControl<T> : UserControl, IEditorControl<T> where T : class
    {
        private readonly MultiPropertyTracker _tracker;

        /// <inheritdoc/>
        public T Target { get { return propertyGrid.SelectedObject as T; } set { propertyGrid.SelectedObject = value; } }

        /// <inheritdoc/>
        public Undo.ICommandExecutor CommandExecutor { get; set; }

        public EditorControl()
        {
            InitializeComponent();
            buttonResetValue.Text = Resources.ResetValue;

            _tracker = new MultiPropertyTracker(propertyGrid);
        }

        private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            buttonResetValue.Enabled = propertyGrid.CanResetSelectedProperty;
        }

        private void buttonResetValue_Click(object sender, EventArgs e)
        {
            propertyGrid.ResetSelectedProperty();
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (CommandExecutor != null)
                CommandExecutor.ExecuteCommand(_tracker.GetCommand(e.ChangedItem));
        }
    }
}
