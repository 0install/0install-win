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
using NanoByte.Common.Collections;

namespace NanoByte.Common.Undo
{
    /// <summary>
    /// An undo command that sets a <see cref="LocalizableString"/> in a <see cref="LocalizableStringCollection"/>.
    /// </summary>
    public sealed class SetLocalizableString : SimpleCommand
    {
        #region Variables
        /// <summary>
        /// The collection to be modified.
        /// </summary>
        private readonly LocalizableStringCollection _collection;

        /// <summary>
        /// The element to be added or removed from <see cref="_collection"/>.
        /// </summary>
        private readonly LocalizableString _entry;

        private string _previousValue;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new localizable string command.
        /// </summary>
        /// <param name="collection">The collection to be modified.</param>
        /// <param name="element">The entry to be set in the <paramref name="collection"/>.</param>
        public SetLocalizableString(LocalizableStringCollection collection, LocalizableString element)
        {
            _collection = collection;
            _entry = element;
        }
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Sets the entry in the collection.
        /// </summary>
        protected override void OnExecute()
        {
            try
            {
                _previousValue = _collection.GetExactLanguage(_entry.Language);
            }
            catch (KeyNotFoundException)
            {}
            _collection.Set(_entry.Language, _entry.Value);
        }
        #endregion

        #region Undo
        /// <summary>
        /// Restores the original entry in the collection.
        /// </summary>
        protected override void OnUndo()
        {
            _collection.Set(_entry.Language, _previousValue);
        }
        #endregion
    }
}
