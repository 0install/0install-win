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

using System.Windows.Forms;
using Common.Properties;

namespace Common.Controls
{
    /// <summary>
    /// A <see cref="PropertyGrid"/> that provides a "reset value" option in its context menu.
    /// </summary>
    public class ResettablePropertyGrid : PropertyGrid
    {
        public ResettablePropertyGrid()
        {
            var menuItem = new ToolStripMenuItem {Text = Resources.ResetValue};
            menuItem.Click += delegate
            {
                object oldValue = SelectedGridItem.Value;
                ResetSelectedProperty();
                OnPropertyValueChanged(new PropertyValueChangedEventArgs(SelectedGridItem, oldValue));
            };
            ContextMenuStrip = new ContextMenuStrip {Items = {menuItem}};

            SelectedGridItemChanged += delegate { menuItem.Enabled = CanResetSelectedProperty; };
        }

        /// <summary>
        /// Indicates whether <see cref="PropertyGrid.ResetSelectedProperty"/> will work.
        /// </summary>
        public bool CanResetSelectedProperty { get { return SelectedGridItem != null && SelectedGridItem.PropertyDescriptor != null && SelectedGridItem.PropertyDescriptor.CanResetValue(SelectedGridItem.Parent.Value); } }
    }
}
