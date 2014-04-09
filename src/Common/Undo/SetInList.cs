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

using System.Collections.Generic;

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// Replaces an entry in a <see cref="IList{T}"/> with a new one.
    /// </summary>
    /// <typeparam name="T">The type of elements the list contains.</typeparam>
    public sealed class SetInList<T> : SimpleCommand, IValueCommand
    {
        #region Variables
        private readonly IList<T> _list;
        private readonly T _oldElement, _newElement;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public object Value { get { return _newElement; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new set in list command.
        /// </summary>
        /// <param name="list">The list to be modified.</param>
        /// <param name="oldElement">The old element currently in the <paramref name="list"/> to be replaced.</param>
        /// <param name="newElement">The new element to take the place of <paramref name="oldElement"/> in the <paramref name="list"/>.</param>
        public SetInList(IList<T> list, T oldElement, T newElement)
        {
            _list = list;
            _oldElement = oldElement;
            _newElement = newElement;
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        /// <summary>
        /// Sets the new entry in the list.
        /// </summary>
        protected override void OnExecute()
        {
            _list[_list.IndexOf(_oldElement)] = _newElement;
        }

        /// <summary>
        /// Restores the old entry in the list.
        /// </summary>
        protected override void OnUndo()
        {
            _list[_list.IndexOf(_newElement)] = _oldElement;
        }
        #endregion
    }
}
