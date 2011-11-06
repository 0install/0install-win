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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Common.Collections
{
    /// <summary>
    /// Provides helper methods for enumerable collections.
    /// </summary>
    public static class EnumerableUtils
    {
        #region First
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
        #endregion

        #region Empty
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
        #endregion

        #region Of type
        /// <summary>
        /// Filters a list of elements based on their type.
        /// </summary>
        /// <typeparam name="TResult">The type of elements to find.</typeparam>
        /// <param name="source">The list of elements to filter.</param>
        /// <returns>All elements in <paramref name="source"/> that are of tpye <typeparamref name="TResult"/>.</returns>
        public static IEnumerable<TResult> OfType<TResult>(IEnumerable source)
        {
            foreach (var element in source)
                if (element is TResult) yield return (TResult)element;
        }
        #endregion

        #region Added elements
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
            public int Compare(T x, T y)
            {
                return x.CompareTo(y);
            }
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
        #endregion

        #region Transactions
        /// <summary>
        /// Applies an operation for all elements of a collection. Automatically applies rollback operations in case of an exception.
        /// </summary>
        /// <typeparam name="T">The type of elements to operate on.</typeparam>
        /// <param name="elements">The elements to apply the action for.</param>
        /// <param name="apply">The action to apply to each element.</param>
        /// <param name="rollback">The action to apply to each element that <paramref name="apply"/> was called on in case of an exception.</param>
        /// <remarks>
        /// <paramref name="rollback"/> is applied to the element that raised an exception in <paramref name="apply"/> and then interating backwards through all previous elements.
        /// After rollback is complete the exception is passed on.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Suppress exceptions during rollback since they would hide the actual exception that caused the rollback in the first place")]
        public static void ApplyWithRollback<T>(IEnumerable<T> elements, Action<T> apply, Action<T> rollback)
        {
            #region Sanity checks
            if (elements == null) throw new ArgumentNullException("elements");
            if (apply == null) throw new ArgumentNullException("apply");
            if (rollback == null) throw new ArgumentNullException("rollback");
            #endregion

            var rollbackJournal = new LinkedList<T>();
            try
            {
                foreach (var element in elements)
                {
                    // Remember the element for potential rollback
                    rollbackJournal.AddFirst(element);

                    apply(element);
                }
            }
            catch
            {
                foreach (var element in rollbackJournal)
                {
                    try
                    {
                        rollback(element);
                    }
                    catch (Exception ex)
                    {
                        // Suppress exceptions during rollback since they would hide the actual exception that caused the rollback in the first place
                        Log.Error("Failed to rollback " + element + ":\n" + ex.Message);
                    }
                }

                throw;
            }
        }
        #endregion

        #region Merge
        /// <summary>
        /// Performs a 2-way merge on two collections. Changes from a foreign list are applied to a local list using callback delegates.
        /// </summary>
        /// <param name="theirsList">The foreign list with changes that shell be merged in.</param>
        /// <param name="mineList">The local list that shall be updated with foreign changes.</param>
        /// <param name="added">Called for every element that should be added to <paramref name="mineList"/>.</param>
        /// <param name="removed">Called for every element that should be removed from <paramref name="mineList"/>.</param>
        /// <remarks><paramref name="theirsList"/> and <paramref name="mineList"/> Should use an internal hashmap for <see cref="ICollection{T}.Contains"/> for better performance.</remarks>
        public static void Merge<T>(ICollection<T> theirsList, ICollection<T> mineList, Action<T> added, Action<T> removed)
        {
            #region Sanity checks
            if (theirsList == null) throw new ArgumentNullException("theirsList");
            if (mineList == null) throw new ArgumentNullException("mineList");
            if (added == null) throw new ArgumentNullException("added");
            if (removed == null) throw new ArgumentNullException("removed");
            #endregion

            foreach (var mine in mineList)
            {
                if (!theirsList.Contains(mine))
                { // Entry in mineList, but not in theirsList
                    removed(mine);
                }
            }

            foreach (var theirs in theirsList)
            {
                if (!mineList.Contains(theirs))
                { // Entry in theirsList, but not in mineList
                    added(theirs);
                }
            }
        }

        /// <summary>
        /// Performs a 3-way merge on a set of collections. Changes from a foreign list are applied to a local list using callback delegates.
        /// </summary>
        /// <param name="baseList">A common baseline from which both <paramref name="theirsList"/> and <paramref name="mineList"/> were modified.</param>
        /// <param name="theirsList">The foreign list with changes that shell be merged in.</param>
        /// <param name="mineList">The local list that shall be updated with foreign changes.</param>
        /// <param name="added">Called for every element that should be added to <paramref name="mineList"/>.</param>
        /// <param name="removed">Called for every element that should be removed from <paramref name="mineList"/>.</param>
        /// <remarks>Modified elements are handled by calling <paramref name="removed"/> for the old state and <paramref name="added"/> for the new state.</remarks>
        public static void Merge<T>(ICollection<T> baseList, IEnumerable<T> theirsList, IEnumerable<T> mineList, Action<T> added, Action<T> removed)
            where T : class, IMergeable<T>
        {
            #region Sanity checks
            if (baseList == null) throw new ArgumentNullException("baseList");
            if (theirsList == null) throw new ArgumentNullException("theirsList");
            if (mineList == null) throw new ArgumentNullException("mineList");
            if (added == null) throw new ArgumentNullException("added");
            if (removed == null) throw new ArgumentNullException("removed");
            #endregion

            foreach (var theirs in theirsList)
            {
                if (theirs == null) continue;

                var matchingMine = FindMergeID(mineList, theirs.MergeID);
                if (matchingMine == null)
                { // Entry in theirsList, but not in mineList
                    if (!baseList.Contains(theirs)) added(theirs); // Added in theirsList
                }
            }

            foreach (var mine in mineList)
            {
                if (mine == null) continue;

                var matchingTheirs = FindMergeID(theirsList, mine.MergeID);
                if (matchingTheirs == null)
                { // Entry in mineList, but not in theirsList
                    if (baseList.Contains(mine)) removed(mine); // Removed from theirsList
                }
                else
                { // Entry both in mineList and in theirsList
                    if (!mine.Equals(matchingTheirs) && matchingTheirs.Timestamp > mine.Timestamp)
                    { // Theirs newer than mine
                        removed(mine);
                        added(matchingTheirs);
                    }
                }
            }
        }

        /// <summary>
        /// Finds the first element in a list matching the specified <see cref="IMergeable{T}.MergeID"/>.
        /// </summary>
        private static T FindMergeID<T>(IEnumerable<T> elements, string id)
            where T : class, IMergeable<T>
        {
            foreach (var element in elements)
                if (element != null && element.MergeID == id) return element;
            return null;
        }
        #endregion
    }
}
