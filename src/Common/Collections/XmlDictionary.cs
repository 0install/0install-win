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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NanoByte.Common.Properties;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// A string dictionary that supports data-binding and can be XML serialized.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This class behaves like a dictionary but doesn't implement the corresponding interfaces because that would prevent XML serialization"),
     SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This class behaves like a dictionary but doesn't implement the corresponding interfaces because that would prevent XML serialization")]
    public class XmlDictionary : BindingList<XmlDictionaryEntry>, ICloneable
    {
        #region Add
        /// <summary>
        /// Adds a new value and links it to a key
        /// </summary>
        /// <param name="key">The key object</param>
        /// <param name="value">The value</param>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="key"/> already exists in the dictionary.</exception>
        public void Add(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && ContainsKey(key))
                throw new ArgumentException(Resources.KeyAlreadyPresent, "key");
            Add(new XmlDictionaryEntry(key, value));
        }

        /// <inheritdoc/>
        protected override void InsertItem(int index, XmlDictionaryEntry item)
        {
            #region Sanity checks
            if (item == null) throw new ArgumentNullException("item");
            #endregion

            if (!string.IsNullOrEmpty(item.Key) && ContainsKey(item.Key))
                throw new ArgumentException(Resources.KeyAlreadyPresent, "item");
            item.Parent = this;

            base.InsertItem(index, item);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes all values assigned to this key.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns><see langword="true"/> if one or more elements were removed; otherwise, <see langword="false"/>.</returns>
        public bool Remove(string key)
        {
            // Build a list of elements to remove
            var pendingRemove = new LinkedList<XmlDictionaryEntry>();
            foreach (XmlDictionaryEntry pair in this)
                if (pair.Key.Equals(key)) pendingRemove.AddLast(pair);

            // Remove the elements one-by-one
            foreach (XmlDictionaryEntry pair in pendingRemove) Remove(pair);

            // Were any elements removed?
            return (pendingRemove.Count > 0);
        }
        #endregion

        #region Contains
        /// <summary>
        /// Checks whether this collection contains a certain key.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns><see langword="true"/> if the key was found.</returns>
        public bool ContainsKey(string key)
        {
            return this.Any(entry => entry.Key == key);
        }

        /// <summary>
        /// Checks whether this collection contains a certain value.
        /// </summary>
        /// <param name="value">The value to look for.</param>
        /// <returns><see langword="true"/> if the value was found.</returns>
        public bool ContainsValue(string value)
        {
            return this.Any(entry => entry.Value == value);
        }
        #endregion

        #region Sort
        /// <summary>
        /// Sorts all entries alphabetically by their key.
        /// </summary>
        public void Sort()
        {
            // Get list to sort
            var items = Items as List<XmlDictionaryEntry>;

            // Apply the sorting algorithms, if items are sortable
            if (items != null)
                items.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase));

            // Let bound controls know they should refresh their views
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        #endregion

        #region Indexer
        /// <summary>
        /// Returns the value associated to a specific key.
        /// </summary>
        /// <param name="key">The key to look for.</param>
        /// <returns>The value associated to <paramref name="key"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if <paramref name="key"/> was not found in the collection.</exception>
        public string GetValue(string key)
        {
            foreach (XmlDictionaryEntry pair in this)
                if (pair.Key.Equals(key)) return pair.Value;
            throw new KeyNotFoundException();
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Convert this <see cref="XmlDictionary"/> to a <see cref="Dictionary{TKey,TValue}"/> for better lookup-performance.
        /// </summary>
        /// <returns>A dictionary containing the same data as this collection.</returns>
        public IDictionary<string, string> ToDictionary()
        {
            return this.ToDictionary(entry => entry.Key, entry => entry.Value);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="XmlDictionary"/> (elements are cloned).
        /// </summary>
        /// <returns>The cloned <see cref="XmlDictionary"/>.</returns>
        public virtual object Clone()
        {
            var newDict = new XmlDictionary();
            foreach (XmlDictionaryEntry entry in this)
                newDict.Add(entry.Clone());

            return newDict;
        }
        #endregion
    }
}
