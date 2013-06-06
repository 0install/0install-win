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

namespace Common.Controls
{
    /// <summary>
    /// Edits arbritary types of elements using a <see cref="PropertyGrid"/>.
    /// </summary>
    /// <typeparam name="T">The type of element to edit.</typeparam>
    public partial class EditorDialog<T> : OKCancelDialog, IEditorDialog<T> where T : class
    {
        public EditorDialog()
        {
            InitializeComponent();
            buttonResetValue.Text = Resources.ResetValue;

            Load += delegate { Text = typeof(T).Name; };
        }

        /// <summary>
        /// Displays a modal dialog for editing an element.
        /// </summary>
        /// <param name="owner">The parent window used to make the dialog modal.</param>
        /// <param name="element">The element to be edited.</param>
        /// <returns>Save changes if <see cref="DialogResult.OK"/>; discard changes if  <see cref="DialogResult.Cancel"/>.</returns>
        public DialogResult ShowDialog(IWin32Window owner, T element)
        {
            #region Sanity checks
            if (owner == null) throw new ArgumentNullException("owner");
            if (element == null) throw new ArgumentNullException("element");
            #endregion

            propertyGrid.SelectedObject = element;

            return ShowDialog(owner);
        }

        /// <summary>
        /// Displays a modal dialog without a parent (listed in the taskbar) for editing an element.
        /// </summary>
        /// <param name="element">The element to be edited.</param>
        /// <returns>Save changes if <see cref="DialogResult.OK"/>; discard changes if <see cref="DialogResult.Cancel"/>.</returns>
        public DialogResult ShowDialog(T element)
        {
            #region Sanity checks
            if (element == null) throw new ArgumentNullException("element");
            #endregion

            propertyGrid.SelectedObject = element;

            ShowInTaskbar = true;
            return ShowDialog();
        }

        private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            buttonResetValue.Enabled = propertyGrid.CanResetSelectedProperty;
        }

        private void buttonResetValue_Click(object sender, EventArgs e)
        {
            propertyGrid.ResetSelectedProperty();
        }
    }
}
