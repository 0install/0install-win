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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Properties;

namespace Common.Collections
{
    /// <summary>
    /// Provides helper methods for enumerable collections.
    /// </summary>
    public static class EnumerableUtils
    {
        #region LINQ
        /// <summary>
        /// Returns the first element in a list or throws a custom exception if no element exists.
        /// </summary>
        /// <param name="source">The list to get the first element from.</param>
        /// <param name="noneException">Callback to retreive hte exception to be thrown if no element exists.</param>
        public static T First<T>(this IEnumerable<T> source, Func<Exception> noneException) where T : class
        {
            #region Sanity checks
            if (source == null) throw new ArgumentNullException("source");
            if (noneException == null) throw new ArgumentNullException("noneException");
            #endregion

            var result = source.FirstOrDefault();
            if (result == null) throw noneException();
            return result;
        }

        /// <summary>
        /// Returns the first element in a list or that matches a predicate or throws a custom exception if no such element exists.
        /// </summary>
        /// <param name="source">The list to get the first element from.</param>
        /// <param name="predicate">The predicate the element must match.</param>
        /// <param name="noneException">Callback to retreive hte exception to be thrown if no element exists.</param>
        public static T First<T>(this IEnumerable<T> source, Func<T, bool> predicate, Func<Exception> noneException) where T : class
        {
            #region Sanity checks
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");
            if (noneException == null) throw new ArgumentNullException("noneException");
            #endregion

            var result = source.FirstOrDefault(predicate);
            if (result == null) throw noneException();
            return result;
        }
        #endregion

        #region Added elements
        /// <summary>
        /// Assumes two sorted arrays. Determines which elements are present in <paramref name="newArray"/> but not in <paramref name="oldArray"/>.
        /// </summary>
        /// <param name="newArray">The new list of elements; may be <see langword="null"/> (will be treated as an empty array).</param>
        /// <param name="oldArray">The original list of elements; may be <see langword="null"/> (will be treated as an empty array).</param>
        /// <returns>An array of elements that were added.</returns>
        /// <remarks>Elements that are present in <paramref name="oldArray"/> but not in <paramref name="newArray"/> are ignored. Elements that are equal for <see cref="IComparable{T}.CompareTo"/> but have been otherwise modified will be added.</remarks>
        public static T[] GetAddedElements<T>(this T[] newArray, T[] oldArray)
            where T : IComparable<T>, IEquatable<T>
        {
            return GetAddedElements(newArray, oldArray, new DefaultComparer<T>());
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
        /// <param name="newArray">The new list of elements; may be <see langword="null"/> (will be treated as an empty array).</param>
        /// <param name="oldArray">The original list of elements; may be <see langword="null"/> (will be treated as an empty array).</param>
        /// <param name="comparer">An object that compares to elements to determine which one is bigger.</param>
        /// <returns>An array of elements that were added.</returns>
        /// <remarks>Elements that are present in <paramref name="oldArray"/> but not in <paramref name="newArray"/> are ignored. Elements that are equal for <see cref="IComparable{T}.CompareTo"/> but have been otherwise modified will be added.</remarks>
        public static T[] GetAddedElements<T>(this T[] newArray, T[] oldArray, IComparer<T> comparer)
        {
            #region Sanity checks
            if (comparer == null) throw new ArgumentNullException("comparer");
            #endregion

            if (newArray == null) return new T[0];
            if (oldArray == null) return newArray;

            var added = new LinkedList<T>();

            int oldCounter = 0;
            int newCounter = 0;
            while (newCounter < newArray.Length)
            {
                T newElement = newArray[newCounter];
                int comparison = (oldCounter < oldArray.Length)
                    // In-range, compare elements
                    ? comparer.Compare(oldArray[oldCounter], newElement)
                    // Out-of-range, add all remaining new elements
                    : 1;

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
                    added.AddLast(newElement);
                    newCounter++;
                }
            }

            var result = new T[added.Count];
            added.CopyTo(result, 0);
            return result;
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
        public static void ApplyWithRollback<T>(this IEnumerable<T> elements, Action<T> apply, Action<T> rollback)
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
                        Log.Error(string.Format(Resources.FailedToRollback, element));
                        Log.Error(ex);
                    }
                }

                throw;
            }
        }
        #endregion

        #region Merge
        /// <summary>
        /// Performs a 2-way merge on two collections. <paramref name="theirsList"/> is updated to reflect <paramref name="mineList"/> using callback delegates.
        /// </summary>
        /// <param name="theirsList">The foreign list with changes that shell be merged in.</param>
        /// <param name="mineList">The local list that shall be updated with foreign changes.</param>
        /// <param name="added">Called for every element that should be added to <paramref name="mineList"/>.</param>
        /// <param name="removed">Called for every element that should be removed from <paramref name="mineList"/>.</param>
        /// <remarks>
        /// <paramref name="theirsList"/> and <paramref name="mineList"/> should use an internal hashmap for <see cref="ICollection{T}.Contains"/> for better performance.
        /// <see langword="null"/> elements are completely ignored.
        /// </remarks>
        public static void Merge<T>(ICollection<T> theirsList, ICollection<T> mineList, Action<T> added, Action<T> removed)
        {
            #region Sanity checks
            if (theirsList == null) throw new ArgumentNullException("theirsList");
            if (mineList == null) throw new ArgumentNullException("mineList");
            if (added == null) throw new ArgumentNullException("added");
            if (removed == null) throw new ArgumentNullException("removed");
            #endregion

            // ReSharper disable CompareNonConstrainedGenericWithNull
            foreach (var mine in mineList.Where(mine => mine != null).
                // Entry in mineList, but not in theirsList
                Where(mine => !theirsList.Contains(mine)))
                removed(mine);

            foreach (var theirs in theirsList.Where(theirs => theirs != null).
                // Entry in theirsList, but not in mineList
                Where(theirs => !mineList.Contains(theirs)))
                added(theirs);
            // ReSharper restore CompareNonConstrainedGenericWithNull
        }

        /// <summary>
        /// Performs a 3-way merge on a set of collections. Changes between <paramref name="baseList"/> and <paramref name="theirsList"/> are applied to <paramref name="mineList"/> using callback delegates.
        /// </summary>
        /// <param name="baseList">A common baseline from which both <paramref name="theirsList"/> and <paramref name="mineList"/> were modified.</param>
        /// <param name="theirsList">The foreign list with changes that shell be merged in.</param>
        /// <param name="mineList">The local list that shall be updated with foreign changes.</param>
        /// <param name="added">Called for every element that should be added to <paramref name="mineList"/>.</param>
        /// <param name="removed">Called for every element that should be removed from <paramref name="mineList"/>.</param>
        /// <remarks>
        /// Modified elements are handled by calling <paramref name="removed"/> for the old state and <paramref name="added"/> for the new state.
        /// <see langword="null"/> elements are completely ignored.
        /// </remarks>
        public static void Merge<T>(IEnumerable<T> baseList, IEnumerable<T> theirsList, IEnumerable<T> mineList, Action<T> added, Action<T> removed)
            where T : class, IMergeable<T>
        {
            #region Sanity checks
            if (baseList == null) throw new ArgumentNullException("baseList");
            if (theirsList == null) throw new ArgumentNullException("theirsList");
            if (mineList == null) throw new ArgumentNullException("mineList");
            if (added == null) throw new ArgumentNullException("added");
            if (removed == null) throw new ArgumentNullException("removed");
            #endregion

            foreach (var theirs in (
                from theirs in theirsList.Where(theirs => theirs != null)
                // Entry in theirsList, but not in mineList
                where FindMergeID(mineList, theirs.MergeID) == null
                select theirs).Where(theirs => !baseList.Contains(theirs)))
                added(theirs); // Added in theirsList

            foreach (var mine in mineList)
            {
                if (mine == null) continue;

                var matchingTheirs = FindMergeID(theirsList, mine.MergeID);
                if (matchingTheirs == null)
                { // Entry in mineList, but not in theirsList
                    if (baseList.Contains(mine))
                        removed(mine); // Removed from theirsList
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
            return elements.FirstOrDefault(element => element != null && element.MergeID == id);
        }
        #endregion
    }
}
