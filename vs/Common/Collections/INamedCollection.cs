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

namespace Common.Collections
{
    /// <summary>
    /// A read-only access to a keyed collection (pseudo-dictionary) of <see cref="INamed"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="INamed"/> objects to hold</typeparam>
    public interface INamedCollection<T> : IEnumerable<T> where T : INamed
    {
        /// <summary>
        /// Gets the element with the specified key.
        /// </summary>
        /// <param name="name">The key of the element to get.</param>
        /// <returns>The element with the specified key. If an element with the specified key is not found, an exception is thrown.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">An element with the specified key does not exist in the dictionary.</exception>
        T this[string name] { get; }

        /// <summary>
        /// Determines whether the collection contains an element with the specified key.
        /// </summary>
        /// <param name="name">The key to locate in the dictionary.</param>
        /// <returns><see langword="true"/> if the dictionary contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        bool Contains(string name);
    }
}
