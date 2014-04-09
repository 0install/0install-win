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
    /// An undo command that adds or removes an element from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements the collection contains.</typeparam>
    public abstract class CollectionCommand<T> : SimpleCommand, IValueCommand
    {
        #region Variables
        /// <summary>
        /// The collection to be modified.
        /// </summary>
        protected readonly ICollection<T> Collection;

        /// <summary>
        /// The element to be added or removed from <see cref="Collection"/>.
        /// </summary>
        protected readonly T Element;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public object Value { get { return Element; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new collection command.
        /// </summary>
        /// <param name="collection">The collection to be modified.</param>
        /// <param name="element">The element to be added or removed from <paramref name="collection"/>.</param>
        protected CollectionCommand(ICollection<T> collection, T element)
        {
            Collection = collection;
            Element = element;
        }
        #endregion
    }
}
