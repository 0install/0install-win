/*
 * Copyright 2006-2011 Bastian Eicher
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
    /// Provides helper methods for enumerable collections.
    /// </summary>
    public static class EnumerableUtils
    {
        /// <summary>
        /// Returns the first element of an enumerable collection.
        /// </summary>
        /// <param name="collection">The collection to get the first element from; may be <see langword="null"/>.</param>
        /// <returns>The first element of <paramref name="collection"/> or <see langword="null"/> if <paramref name="collection"/> is empty or <see langword="null"/>.</returns>
        public static T GetFirst<T>(IEnumerable<T> collection) where T : class
        {
            if (collection == null) return null;

            using (var enumerator = collection.GetEnumerator())
                return enumerator.MoveNext() ? enumerator.Current : null;
        }

        /// <summary>
        /// Checkes whether an enumerable collection contains any elements.
        /// </summary>
        /// <param name="collection">The collection to check for elements; may be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="collection"/> is empty or <see langword="null"/>; <see langword="false"/> otherwise.</returns>
        public static bool IsEmpty<T>(IEnumerable<T> collection)
        {
            if (collection == null) return true;

            using (var enumerator = collection.GetEnumerator())
                return !enumerator.MoveNext();
        }

        /// <summary>
        /// Assumes two ordered fast-indexable lists (e.g. sorted arrays).
        /// Determines which elements are present in <paramref name="newList"/> but not in <paramref name="oldList"/>.
        /// </summary>
        /// <param name="oldList">The original list of elements.</param>
        /// <param name="newList">The new list of elements.</param>
        /// <returns>A list of elements that were added.</returns>
        /// <remarks>Elements that are present in <paramref name="oldList"/> but not in <paramref name="newList"/> are ignored.</remarks>
        public static IEnumerable<T> GetAddedEntries<T>(IList<T> oldList, IList<T> newList)
            where T : IComparable<T>
        {
            var added = new C5.LinkedList<T>();

            int oldCounter = 0;
            int newCounter = 0;
            while (newCounter < newList.Count)
            {
                T newElement = newList[newCounter];
                int comparison = (oldCounter < oldList.Count) ? oldList[oldCounter].CompareTo(newElement) : 1;

                if (comparison == 0)
                { // old == new
                    oldCounter++;
                    newCounter++;
                }
                else if (comparison < 0)
                { // old < new
                    oldCounter++;
                }
                else if (comparison > 0)
                { // old > new
                    added.Add(newElement);
                    newCounter++;
                }
            }

            return added;
        }
    }
}
