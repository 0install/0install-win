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
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// A dictionary that allows a key to reference multiple values.
    /// </summary>
    /// <typeparam name="TKey">The type to use as a key to identify entries in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type to use as elements to store in the dictionary.</typeparam>
    /// <remarks>This structure internally uses hash maps, so most operations run in O(1).</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This class behaves mostly like a normal dictionary but cannot implement the usual interfaces since they are incompatible with the \"multiple values per key\" paradigm")]
    public class MultiDictionary<TKey, TValue>
    {
        #region Variables
        /// <summary>A list of all values in the dictionary.</summary>
        private HashSet<TValue> _values = new HashSet<TValue>();

        /// <summary>The internal dictionary data-structure used for hash-based storage.</summary>
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets a collection containing the keys of the dictionary.
        /// </summary>
        public IEnumerable<TKey> Keys { get { return _dictionary.Keys; } }

        /// <summary>
        /// Gets a collection containing the values in the dictionary.
        /// </summary>
        public IEnumerable<TValue> Values { get { return _values; } }
        #endregion

        //--------------------//

        #region Add
        /// <summary>
        /// Adds an element with the provided key and value to the dictionary.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
        public void Add(TKey key, TValue value)
        {
            #region Sanity checks
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (key == null) throw new ArgumentNullException("key");
            if (value == null) throw new ArgumentNullException("value");
            // ReSharper restore CompareNonConstrainedGenericWithNull
            #endregion

            // Add the value to the values list
            _values.Add(value);

            // Create value set for the key if none exists yet in the internal dictionary data-structure
            if (!_dictionary.ContainsKey(key)) _dictionary.Add(key, new HashSet<TValue>());

            // Add the value to the set for its key
            _dictionary[key].Add(value);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes all elements with the specified key from the dictionary.
        /// </summary>
        /// <returns><see langword="true"/> if any elements were successfully removed; otherwise, <see langword="false"/>.
        /// This method also returns <see langword="false"/> if <paramref name="key"/> was not found in the dictionary.</returns>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <remarks>Since the <see cref="Values"/> list needs to be rebuild by traversing all <see cref="Keys"/>, this operation is not O(1) efficient.</remarks>
        public bool RemoveKey(TKey key)
        {
            #region Sanity checks
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (key == null) throw new ArgumentNullException("key");
            // ReSharper restore CompareNonConstrainedGenericWithNull
            #endregion

            // Remove the key from the internal dictionary data-structure
            bool result = _dictionary.Remove(key);

            // Rebuild the values list
            _values = new HashSet<TValue>(_dictionary.Values.SelectMany(set => set));

            return result;
        }

        /// <summary>
        /// Removes an element with a specific value from the dictionary.
        /// </summary>
        /// <returns><see langword="true"/> if the element was successfully removed; otherwise, <see langword="false"/>.
        /// This method also returns <see langword="false"/> if <paramref name="value"/> was not found in the dictionary.</returns>
        /// <param name="value">The value of the element to remove.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public bool RemoveValue(TValue value)
        {
            #region Sanity checks
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (value == null) throw new ArgumentNullException("value");
            // ReSharper restore CompareNonConstrainedGenericWithNull
            #endregion

            // Remove the value from the value sets for all keys
            var emptyKeys = new LinkedList<TKey>();
            foreach (var pair in _dictionary)
            {
                pair.Value.Remove(value);

                // Mark keys that have become empty for removal; can not be done directly within foreach
                if (pair.Value.Count == 0) emptyKeys.AddLast(pair.Key);
            }

            // Remove the keys that have become empty
            foreach (TKey key in emptyKeys) _dictionary.Remove(key);

            // Remove the value from the values list
            return _values.Remove(value);
        }
        #endregion

        #region Contains
        /// <summary>
        /// Determines whether the dictionary contains an element with the specified key.
        /// </summary>
        /// <returns><see langword="true"/> if the dictionary contains an element with the key; otherwise, <see langword="false"/>.</returns>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the dictionary contains an element with the specified value.
        /// </summary>
        /// <returns><see langword="true"/> if the dictionary contains an element with the value; otherwise, <see langword="false"/>.</returns>
        /// <param name="value">The value to locate in the dictionary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public bool ContainsValue(TValue value)
        {
            return _values.Contains(value);
        }
        #endregion

        #region Index accessor
        /// <summary>
        /// Gets a collection containing the values with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get.</param>
        /// <returns>A list of elements with the specified key. Empty list if the key was not found.</returns>
        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                HashSet<TValue> result;
                return _dictionary.TryGetValue(key, out result) ? result : Enumerable.Empty<TValue>();
            }
        }
        #endregion
    }
}
