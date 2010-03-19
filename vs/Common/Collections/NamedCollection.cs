/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Common.Collections
{
    /// <summary>
    /// A keyed collection (pseudo-dictionary) of <see cref="INamed"/> objects.
    /// </summary>
    public class NamedCollection<T> : KeyedCollection<string, T>, INamedCollection<T>, ICloneable where T : INamed
    {
        #region Constructor
        /// <summary>
        /// Creates a new case-insensitive named collection.
        /// </summary>
        public NamedCollection() : base(StringComparer.OrdinalIgnoreCase)
        {}
        #endregion

        //--------------------//

        #region Access
        protected override string GetKeyForItem(T item)
        {
            return item.Name;
        }
        #endregion

        #region Sort
        /// <summary>
        /// Sorts all entries alphabetically by their name.
        /// </summary>
        public void Sort()
        {
            // Get list to sort
            var items = Items as List<T>;

            // Apply and set the sort, if items to sort
            if (items != null)
            {
                items.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this <see cref="NamedCollection{T}"/> (elements are not cloned).
        /// </summary>
        /// <returns>The cloned <see cref="NamedCollection{T}"/>.</returns>
        public virtual object Clone()
        {
            var newCollection = new NamedCollection<T>();
            foreach (T entry in this)
                newCollection.Add(entry);

            return newCollection;
        }
        #endregion
    }
}