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
using System.Diagnostics.CodeAnalysis;

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// An undo command that replaces an element in a list with a new one.
    /// </summary>
    /// <typeparam name="T">The type of elements the list contains.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "The complete name is not ambiguous.")]
    public class ReplaceInList<T> : SimpleCommand, IValueCommand
    {
        #region Variables
        private readonly IList<T> _list;
        private readonly T _oldElement;
        private readonly T _newElement;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public object Value { get { return _newElement; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new replace in list command.
        /// </summary>
        /// <param name="list">The collection to be modified.</param>
        /// <param name="oldElement">The element to be removed from <paramref name="list"/>.</param>
        /// <param name="newElement">The element to be added to <paramref name="list"/>.</param>
        public ReplaceInList(IList<T> list, T oldElement, T newElement)
        {
            _list = list;
            _oldElement = oldElement;
            _newElement = newElement;
        }
        #endregion

        //--------------------//

        #region Undo / Redo
        protected override void OnExecute()
        {
            int index = _list.IndexOf(_oldElement);
            _list[index] = _newElement;
        }

        protected override void OnUndo()
        {
            int index = _list.IndexOf(_newElement);
            _list[index] = _oldElement;
        }
        #endregion
    }
}
