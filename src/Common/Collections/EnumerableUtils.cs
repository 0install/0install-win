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
        /// Assumes two sorted arrays. Determines which elements are present in <paramref name="newArray"/> but not in <paramref name="oldArray"/>.
        /// </summary>
        /// <param name="oldArray">The original list of elements; may be <see langword="null"/> (will be treated as an empty array).</param>
        /// <param name="newArray">The new list of elements; may be <see langword="null"/> (will be treated as an empty array).</param>
        /// <returns>An array of elements that were added.</returns>
        /// <remarks>Elements that are present in <paramref name="oldArray"/> but not in <paramref name="newArray"/> are ignored. Elements that are equal for <see cref="IComparable{T}.CompareTo"/> but have been otherwise modified will be added.</remarks>
        public static T[] GetAddedElements<T>(T[] oldArray, T[] newArray)
            where T : IComparable<T>, IEquatable<T>
        {
            return GetAddedElements(oldArray, newArray, new DefaultComparer<T>());
        }

        private class DefaultComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y) { return x.CompareTo(y); }
        }

        /// <summary>
        /// Assumes two sorted arrays. Determines which elements are present in <paramref name="newArray"/> but not in <paramref name="oldArray"/>.
        /// </summary>
        /// <param name="oldArray">The original list of elements; may be <see langword="null"/> (will be treated as an empty array).</param>
        /// <param name="newArray">The new list of elements; may be <see langword="null"/> (will be treated as an empty array).</param>
        /// <param name="comparer">An object that compares to elements to determine which one is bigger.</param>
        /// <returns>An array of elements that were added.</returns>
        /// <remarks>Elements that are present in <paramref name="oldArray"/> but not in <paramref name="newArray"/> are ignored. Elements that are equal for <see cref="IComparable{T}.CompareTo"/> but have been otherwise modified will be added.</remarks>
        public static T[] GetAddedElements<T>(T[] oldArray, T[] newArray, IComparer<T> comparer)
        {
            #region Sanity checks
            if (comparer == null) throw new ArgumentNullException("comparer");
            #endregion

            if (newArray == null) return new T[0];
            if (oldArray == null) return newArray;

            var added = new C5.LinkedList<T>();

            int oldCounter = 0;
            int newCounter = 0;
            while (newCounter < newArray.Length)
            {
                T oldElement;
                T newElement = newArray[newCounter];
                int comparison;
                if (oldCounter < oldArray.Length)
                { // In-range, compare elements
                    oldElement = oldArray[oldCounter];
                    comparison = comparer.Compare(oldElement, newElement);
                }
                else
                { // Out-of-range, add all remaining new elements
                    comparison = 1;
                }

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

            return added.ToArray();
        }
    }
}
