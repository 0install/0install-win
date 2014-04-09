/*
 * Copyright 2006-2014 Bastian Eicher
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

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// Tracks values in <see cref="PropertyGrid"/>s in order to be able to generate <see cref="MultiPropertyChangedCommand"/>s after a property was changed.
    /// </summary>
    public class MultiPropertyTracker
    {
        #region Variables
        /// <summary>The property grid being tracked.</summary>
        private readonly PropertyGrid _propertyGrid;

        /// <summary>Contains backups of property values.</summary>
        private object[] _oldValues;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new multi-property tracker.
        /// </summary>
        /// <param name="propertyGrid">The property grid being tracked.</param>
        public MultiPropertyTracker(PropertyGrid propertyGrid)
        {
            #region Sanity checks
            if (propertyGrid == null) throw new ArgumentNullException("propertyGrid");
            #endregion

            _propertyGrid = propertyGrid;
            propertyGrid.SelectedGridItemChanged += SelectionChanged;
        }
        #endregion

        #region Events
        /// <summary>
        /// Reacts to any focus change in the <see cref="PropertyGrid"/> and creates backups of the current values before the user can change them.
        /// </summary>
        private void SelectionChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            // Only handle property selections
            if (e.NewSelection == null || e.NewSelection.GridItemType != GridItemType.Property)
            {
                _oldValues = null;
                return;
            }

            // Don't handle re-selections triggered by validation failures
            if (e.OldSelection != null && e.NewSelection.Label == e.OldSelection.Label)
                return;

            // Create a new array to hold old values before the user changes them
            _oldValues = new object[_propertyGrid.SelectedObjects.Length];
            string property = MoveOutOfNested(e.NewSelection).PropertyDescriptor.Name;
            for (int i = 0; i < _propertyGrid.SelectedObjects.Length; i++)
            {
                // Use reflection to get the specific property for each object and backup the corresponding old value for each
                var item = _propertyGrid.SelectedObjects[i];
                _oldValues[i] = item.GetType().GetProperty(property).GetValue(item, null);
            }
        }
        #endregion

        #region Command
        /// <summary>
        /// Creates an undo command representing a property change the <see cref="PropertyGrid"/> has just performed.
        /// </summary>
        /// <param name="changedItem">The property grid item that was changed.</param>
        public IUndoCommand GetCommand(GridItem changedItem)
        {
            return new MultiPropertyChangedCommand(_propertyGrid.SelectedObjects, MoveOutOfNested(changedItem), _oldValues);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Moves up a hierachy of nested properties to the top element that is still a property.
        /// </summary>
        private static GridItem MoveOutOfNested(GridItem item)
        {
            while (item.Parent != null && item.Parent.GridItemType == GridItemType.Property)
                item = item.Parent;
            return item;
        }
        #endregion
    }
}
