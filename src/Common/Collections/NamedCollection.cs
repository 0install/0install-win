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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using NanoByte.Common.Properties;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// A keyed collection (pseudo-dictionary) of <see cref="INamed{T}"/> objects. Elements are automatically maintained in an alphabetically sorted order. Suitable for XML serialization.
    /// </summary>
    /// <remarks>Correct behavior with duplicate names or renaming without using the <see cref="Rename"/> method is not guaranteed!</remarks>
    public class NamedCollection<T> : KeyedCollection<string, T>, ICloneable where T : INamed<T>
    {
        #region Events
        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        public event Action<object> CollectionChanged;

        private void OnAfterChanged()
        {
            if (CollectionChanged != null) CollectionChanged(this);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new case-insensitive named collection.
        /// </summary>
        public NamedCollection() : base(StringComparer.OrdinalIgnoreCase)
        {}
        #endregion

        //--------------------//

        #region Key access
        /// <inheritdoc/>
        protected override string GetKeyForItem(T item)
        {
            return item.Name;
        }
        #endregion

        #region Rename
        /// <inheritdoc/>
        public void Rename(T entry, string newName)
        {
            if (entry.Name == newName) return;
            if (Contains(newName)) throw new InvalidOperationException(Resources.KeyAlreadyPresent);

            ChangeItemKey(entry, newName);
            entry.Name = newName;

            Sort();

            OnAfterChanged();
        }
        #endregion

        #region Sort
        /// <summary>
        /// Sorts all elements alphabetically by their <see cref="INamed{T}.Name"/>.
        /// </summary>
        private void Sort()
        {
            var items = Items as List<T>;
            if (items != null)
                items.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
        }
        #endregion

        #region Hooks
        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            Sort();
            OnAfterChanged();
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            Sort();
            OnAfterChanged();
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            OnAfterChanged();
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            base.ClearItems();
            OnAfterChanged();
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this collection (elements are not cloned).
        /// </summary>
        /// <returns>The cloned collection.</returns>
        public virtual NamedCollection<T> Clone()
        {
            var newCollection = new NamedCollection<T>();
            foreach (T entry in this)
                newCollection.Add(entry);
            return newCollection;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
