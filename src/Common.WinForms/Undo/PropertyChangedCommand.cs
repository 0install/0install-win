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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// An undo command that handles a changed property - usually used with a <see cref="PropertyGrid"/>.
    /// </summary>
    public class PropertyChangedCommand : PreExecutedCommand
    {
        #region Variables
        private readonly object _target;
        private readonly PropertyDescriptor _property;
        private readonly object _oldValue, _newValue;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the command after the property was first changed.
        /// </summary>
        /// <param name="target">The object the property belongs to.</param>
        /// <param name="property">The property that was changed.</param>
        /// <param name="oldValue">The property's old value.</param>
        /// <param name="newValue">The property's current value.</param>
        public PropertyChangedCommand(object target, PropertyDescriptor property, object oldValue, object newValue)
        {
            _target = target;
            _property = property;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        /// <summary>
        /// Initializes the command after the property was first changed.
        /// </summary>
        /// <param name="target">The object the <see cref="PropertyGrid.SelectedObject"/> is target at.</param>
        /// <param name="e">The event data from the <see cref="PropertyGrid.PropertyValueChanged"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "The arguments are passed on to a different overload of the constructor")]
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", Justification = "This is simply a comfort wrapper for extracting values from the event arguments")]
        public PropertyChangedCommand(object target, PropertyValueChangedEventArgs e)
            : this(target, e.ChangedItem.PropertyDescriptor, e.OldValue, e.ChangedItem.Value)
        {}
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <summary>
        /// Set the changed property value again.
        /// </summary>
        protected override void OnRedo()
        {
            _property.SetValue(_target, _newValue);
        }

        /// <summary>
        /// Restore the original property value.
        /// </summary>
        protected override void OnUndo()
        {
            _property.SetValue(_target, _oldValue);
        }
        #endregion
    }
}
