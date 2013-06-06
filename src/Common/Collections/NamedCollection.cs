/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Linq;
using Common.Properties;
using Common.Utils;

namespace Common.Collections
{
    /// <summary>
    /// A keyed collection (pseudo-dictionary) of <see cref="INamed{T}"/> objects. Elements are automatically maintained in an alphabetically sorted order. Suitable for XML serialization.
    /// </summary>
    /// <remarks>Correct behavior with duplicate names or renaming without using the <see cref="Rename"/> method is not guaranteed!</remarks>
    public class NamedCollection<T> : C5.TreeSet<T> where T : INamed<T>
    {
        private static readonly NamedComparer<T> _comparer = new NamedComparer<T>();

        /// <summary>
        /// Creates a new case-insensitive named collection.
        /// </summary>
        public NamedCollection() : base(_comparer)
        {}

        /// <summary>
        /// Gets the element with the specified key.
        /// </summary>
        /// <param name="name">The key of the element to get.</param>
        /// <returns>The element with the specified key. If an element with the specified key is not found, an exception is thrown.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">An element with the specified key does not exist in the dictionary.</exception>
        public T this[string name] { get { return this.First(x => StringUtils.EqualsIgnoreCase(x.Name, name)); } }

        /// <summary>
        /// Determines whether the collection contains an element with the specified key.
        /// </summary>
        /// <param name="name">The key to locate in the dictionary.</param>
        /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public bool Contains(string name)
        {
            return this.Any(x => StringUtils.EqualsIgnoreCase(x.Name, name));
        }

        /// <summary>
        /// Changes the <see cref="INamed{T}.Name"/> of an entry.
        /// </summary>
        /// <param name="entry">The entry in the collection to be renamed.</param>
        /// <param name="newName">The new name for the collection</param>
        /// <exception cref="InvalidOperationException">Thrown if an entry with the name <paramref name="newName"/> is already contained within the collection.</exception>
        /// <remarks>This method must be used instead of directly modifying the <see cref="INamed{T}.Name"/> property to ensure the directory lookup structures stay in sync.</remarks>
        public void Rename(T entry, string newName)
        {
            if (entry.Name == newName) return;
            if (Contains(newName)) throw new InvalidOperationException(Resources.KeyAlreadyPresent);

            Remove(entry);
            entry.Name = newName;
            Add(entry);
        }
    }
}
