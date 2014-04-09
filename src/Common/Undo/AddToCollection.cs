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
    /// An undo command that adds an element to a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements the collection contains.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "The complete name is not ambiguous.")]
    public sealed class AddToCollection<T> : CollectionCommand<T>
    {
        #region Constructor
        /// <summary>
        /// Creates a new add to collection command.
        /// </summary>
        /// <param name="collection">The collection to be modified.</param>
        /// <param name="element">The element to be added to <paramref name="collection"/>.</param>
        public AddToCollection(ICollection<T> collection, T element) : base(collection, element)
        {}
        #endregion

        //--------------------//

        #region Execute
        /// <summary>
        /// Adds the element to the collection.
        /// </summary>
        protected override void OnExecute()
        {
            Collection.Add(Element);
        }
        #endregion

        #region Undo
        /// <summary>
        /// Removes the element from the collection.
        /// </summary>
        protected override void OnUndo()
        {
            Collection.Remove(Element);
        }
        #endregion
    }
}
