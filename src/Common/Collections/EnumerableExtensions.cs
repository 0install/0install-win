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
using NanoByte.Common.Properties;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// Provides helper methods for enumerable collections.
    /// </summary>
    public static class EnumerableExtensions
    {
        #region LINQ
        /// <summary>
        /// Filters a sequence of elements to remove any that match the <paramref name="predicate"/>.
        /// The opposite of <see cref="Enumerable.Where{TSource}(System.Collections.Generic.IEnumerable{TSource},System.Func{TSource,bool})"/>.
        /// </summary>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumeration, Func<T, bool> predicate)
        {
            return enumeration.Where(x => !predicate(x));
        }

        /// <summary>
        /// Filters a sequence of elements to remove any that are equal to <paramref name="element"/>.
        /// </summary>
        public static IEnumerable<T> Except<T>(this IEnumerable<T> enumeration, T element)
        {
            return enumeration.Except(new[] {element});
        }

        /// <summary>
        /// Flattens a list of lists.
        /// </summary>
        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumeration)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            return enumeration.SelectMany(x => x);
        }

        /// <summary>
        /// Appends an element to a list.
        /// </summary>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumeration, T element)
        {
            return enumeration.Concat(new[] {element});
        }

        /// <summary>
        /// Filters a sequence of elements to remove any <see langword="null"/> values.
        /// </summary>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> collection)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            return collection.Where(element => element != null);
        }
        #endregion

        #region Equality
        /// <summary>
        /// Determines whether two collections contain the same elements in the same order.
        /// </summary>
        /// <param name="first">The first of the two collections to compare.</param>
        /// <param name="second">The first of the two collections to compare.</param>
        /// <param name="comparer">Controls how to compare elements; leave <see langword="null"/> for default comparer.</param>
        public static bool SequencedEquals<T>(this ICollection<T> first, ICollection<T> second, IEqualityComparer<T> comparer = null)
        {
            #region Sanity checks
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            #endregion

            if (first.Count != second.Count) return false;
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            return first.SequenceEqual(second, comparer);
        }

        /// <summary>
        /// Determines whether two arrays contain the same elements in the same order.
        /// </summary>
        /// <param name="first">The first of the two collections to compare.</param>
        /// <param name="second">The first of the two collections to compare.</param>
        /// <param name="comparer">Controls how to compare elements; leave <see langword="null"/> for default comparer.</param>
        public static bool SequencedEquals<T>(this T[] first, T[] second, IEqualityComparer<T> comparer = null)
        {
            #region Sanity checks
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            #endregion

            if (first.Length != second.Length) return false;
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < first.Length; i++)
                if (!comparer.Equals(first[i], second[i])) return false;
            return true;
        }

        /// <summary>
        /// Determines whether two collections contain the same elements disregarding the order they are in.
        /// </summary>
        /// <param name="first">The first of the two collections to compare.</param>
        /// <param name="second">The first of the two collections to compare.</param>
        /// <param name="comparer">Controls how to compare elements; leave <see langword="null"/> for default comparer.</param>
        public static bool UnsequencedEquals<T>(this ICollection<T> first, ICollection<T> second, IEqualityComparer<T> comparer = null)
        {
            #region Sanity checks
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            #endregion

            if (first.Count != second.Count) return false;
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            if (first.GetUnsequencedHashCode(comparer) != second.GetUnsequencedHashCode(comparer)) return false;
            return first.All(x => second.Contains(x, comparer));
        }

        /// <summary>
        /// Generates a hash code for the contents of a collection. Changing the elements' order will change the hash.
        /// </summary>
        /// <param name="collection">The collection to generate the hash for.</param>
        /// <param name="comparer">Controls how to compare elements; leave <see langword="null"/> for default comparer.</param>
        /// <seealso cref="SequencedEquals{T}(System.Collections.Generic.ICollection{T},System.Collections.Generic.ICollection{T},System.Collections.Generic.IEqualityComparer{T})"/>
        /// <seealso cref="SequencedEquals{T}(T[],T[],System.Collections.Generic.IEqualityComparer{T})"/>
        public static int GetSequencedHashCode<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer = null)
        {
            #region Sanity checks
            if (collection == null) throw new ArgumentNullException("collection");
            #endregion

            if (comparer == null) comparer = EqualityComparer<T>.Default;

            unchecked
            {
                int result = 397;
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (T unknown in collection.WhereNotNull())
                    result = (result * 397) ^ comparer.GetHashCode(unknown);
                return result;
            }
        }

        /// <summary>
        /// Generates a hash code for the contents of a collection. Changing the elements' order will not change the hash.
        /// </summary>
        /// <param name="collection">The collection to generate the hash for.</param>
        /// <param name="comparer">Controls how to compare elements; leave <see langword="null"/> for default comparer.</param>
        /// <seealso cref="UnsequencedEquals{T}"/>
        public static int GetUnsequencedHashCode<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer = null)
        {
            #region Sanity checks
            if (collection == null) throw new ArgumentNullException("collection");
            #endregion

            if (comparer == null) comparer = EqualityComparer<T>.Default;

            unchecked
            {
                int result = 397;
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (T unknown in collection.WhereNotNull())
                    result = result ^ comparer.GetHashCode(unknown);
                return result;
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Calls <see cref="ICloneable.Clone"/> for every element in a collection and returns the results as a new collection.
        /// </summary>
        public static IEnumerable<T> CloneElements<T>(this IEnumerable<T> enumerable) where T : ICloneable
        {
            return enumerable.Select(entry => (T)entry.Clone());
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

        /// <summary>
        /// Applies an operation for the first possible element of a collection.
        /// If the operation succeeds the remaining elements are ignored. If the operation fails it is repeated for the next element.
        /// </summary>
        /// <typeparam name="T">The type of elements to operate on.</typeparam>
        /// <param name="elements">The elements to apply the action for.</param>
        /// <param name="action">The action to apply to an element.</param>
        /// <exception cref="Exception">The exception thrown by <paramref name="action"/> for the last element of <paramref name="elements"/>.</exception>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Last excption is rethrown, other exceptions are logged")]
        public static void Try<T>(this IEnumerable<T> elements, Action<T> action)
        {
            #region Sanity checks
            if (elements == null) throw new ArgumentNullException("elements");
            if (action == null) throw new ArgumentNullException("action");
            #endregion

            var enumerator = elements.GetEnumerator();
            if (!enumerator.MoveNext()) return;

            while (true)
            {
                try
                {
                    action(enumerator.Current);
                    return;
                }
                catch (Exception ex)
                {
                    if (enumerator.MoveNext()) Log.Error(ex); // Log exception and try next element
                    else throw; // Rethrow exception if there are no more elements
                }
            }
        }
        #endregion

        #region List
        /// <summary>
        /// Adds multiple elements to the list.
        /// </summary>
        /// <remarks>This is a covariant wrapper for <see cref="List{T}.AddRange"/>.</remarks>
        public static void AddRange<TList, TElements>(this List<TList> list, IEnumerable<TElements> elements)
            where TElements : TList
        {
            #region Sanity checks
            if (list == null) throw new ArgumentNullException("list");
            #endregion

            list.AddRange(elements.Cast<TList>());
        }

        /// <summary>
        /// Removes multiple elements from the list.
        /// </summary>
        public static void RemoveRange<TList, TElements>(this List<TList> list, IEnumerable<TElements> elements)
            where TElements : TList
        {
            #region Sanity checks
            if (list == null) throw new ArgumentNullException("list");
            if (elements == null) throw new ArgumentNullException("elements");
            #endregion

            foreach (var element in elements) list.Remove(element);
        }

        /// <summary>
        /// Removes the last n elements from the list.
        /// </summary>
        /// <param name="list">The list to remove the elements from.</param>
        /// <param name="number">The number of elements to remove.</param>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Specifically extends List<T>")]
        public static void RemoveLast<T>(this List<T> list, int number = 1)
        {
            #region Sanity checks
            if (list == null) throw new ArgumentNullException("list");
            if (number < 0) throw new ArgumentOutOfRangeException("number");
            #endregion

            list.RemoveRange(list.Count - number, number);
        }
        #endregion

        #region Dictionary
        /// <summary>
        /// Adds multiple pairs to the dictionary in one go.
        /// </summary>
        public static void AddRange<TSourceKey, TSourceValue, TTargetKey, TTargetValue>(this IDictionary<TTargetKey, TTargetValue> target, IEnumerable<KeyValuePair<TSourceKey, TSourceValue>> source)
            where TSourceKey : TTargetKey
            where TSourceValue : TTargetValue
        {
            #region Sanity checks
            if (target == null) throw new ArgumentNullException("target");
            if (source == null) throw new ArgumentNullException("source");
            #endregion

            foreach (var pair in source)
                target.Add(pair.Key, pair.Value);
        }
        #endregion
    }
}
