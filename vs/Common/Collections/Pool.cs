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
using Common.Properties;

namespace Common.Collections
{
    #region Helper interface
    /// <summary>
    /// An interface items must implement to be addable to <see cref="Pool{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of items to store in <see cref="Pool{T}"/>.</typeparam>
    /// <example>Implement like this: <code>RenderTarget IPoolable&lt;RenderTarget&gt;.NextElement { get; set; }</code></example>
    public interface IPoolable<T> where T : class, IPoolable<T>
    {
        /// <summary>
        /// A reference to the next element in the <see cref="Pool{T}"/> chain.
        /// </summary>
        T NextElement { get; set; }
    }
    #endregion

    /// <summary>
    /// A highly efficient pseudo-collection (doesn't implement <see cref="IEnumerable{T}"/>) that supports fast adding at the beginning.
    /// </summary>
    /// <remarks>An item can always only be in one <see cref="Pool{T}"/> at any given time.</remarks>
    /// <typeparam name="T">The type of items to store in the <see cref="Pool{T}"/>.</typeparam>
    public class Pool<T> : IPoolable<T> where T : class, IPoolable<T>
    {
        #region Variables
        private int _count;
        private T _firstElement;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of elements contained in the pool
        /// </summary>
        /// <returns>The number of elements contained in the pool</returns>
        public int Count { get { return _count; } }

        T IPoolable<T>.NextElement { get { return _firstElement; } set { _firstElement = value; } }
        #endregion

        //--------------------//

        #region ForEach
        /// <summary>
        /// Performs the specified action on each element of the pool
        /// </summary>
        /// <param name="action">A delegate to perform on each element of the pool</param>
        public void ForEach(Action<T> action)
        {
            #region Sanity checks
            if (action == null) throw new ArgumentNullException("action");
            #endregion

            // Get the first element
            T currentElement = _firstElement;

            while (currentElement != null)
            {
                // Execute the delegate for each element
                action(currentElement);

                // Move on to the next element
                currentElement = currentElement.NextElement;
            }
        }
        #endregion

        #region Add
        /// <summary>
        /// Adds an item to the beginning pool
        /// </summary>
        /// <param name="item">The object to add to the pool</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="item"/> is already in a <see cref="Pool{T}"/></exception>
        public void Add(T item)
        {
            #region Sanity checks
            if (item == null) throw new ArgumentNullException("item");
            if (item.NextElement != null) throw new ArgumentException(Resources.ItemAlreadyInPool, "item");
            #endregion

            // Add the new item as the first element and hook in the previous items after that
            item.NextElement = _firstElement;
            _firstElement = item;

            _count++;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes the first occurrence of a specific object from the pool
        /// </summary>
        /// <param name="item">The object to remove from the pool</param>
        /// <returns><see langword="true"/> if <paramref name="item" /> was successfully removed from the buffer list; otherwise, false. This method also returns <see langword="false"/> if <paramref name="item" /> is not found in the original pool</returns>
        /// <remarks>Not all too fast, try to avoid using this</remarks>
        public bool Remove(T item)
        {
            #region Sanity checks
            if (item == null) throw new ArgumentNullException("item");
            #endregion

            // Get the first element and the object pointing to it
            IPoolable<T> previousElement = this;
            T currentElement = _firstElement;

            while (currentElement != null)
            {
                // Check if this is the item we want to remove
                if (currentElement == item)
                {
                    // Pull up the next entry
                    previousElement.NextElement = currentElement.NextElement;
                    _count--;

                    // Remove linkage from item and exit
                    item.NextElement = null;
                    return true;
                }

                // Move on to the next element
                previousElement = currentElement;
                currentElement = currentElement.NextElement;
            }

            // Element not found
            return false;
        }

        /// <summary>
        /// Removes all items from the pool
        /// </summary>
        public void Clear()
        {
            _firstElement = null;
            _count = 0;
        }
        #endregion

        #region Contains
        /// <summary>
        /// Determines whether the pool contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the pool</param>
        /// <returns><see langword="true"/> if <paramref name="item" /> is found in the pool; otherwise, false.</returns>
        public bool Contains(T item)
        {
            #region Sanity checks
            if (item == null) throw new ArgumentNullException("item");
            #endregion

            // Get the first element
            var currentElement = _firstElement;

            while (currentElement != null)
            {
                // Compare with each element
                if (currentElement.Equals(item)) return true;

                // Move on to the next element
                currentElement = currentElement.NextElement;
            }

            // Element not found
            return false;
        }
        #endregion

        //--------------------//

        #region Remove all
        /// <summary>
        /// Removes all the items in the buffer one-by-one, executing <paramref name="action"/> after each removal.
        /// </summary>
        /// <param name="action">A delegate that is executed right after an item is removed.</param>
        /// <remarks>Ideal for moving all elements to a new data structure.</remarks>
        public void RemoveAll(Action<T> action)
        {
            #region Sanity checks
            if (action == null) throw new ArgumentNullException("action");
            #endregion

            // Get the first element and remove the pointer to it
            T currentElement = _firstElement;
            _firstElement = null;

            while (currentElement != null)
            {
                // Remove the linkage beforehand to allow the element to be easily added to a new Pool
                var nextElement = currentElement.NextElement;
                currentElement.NextElement = null;

                // Execute the delegate for each element
                action(currentElement);

                // Move on to the next element
                currentElement = nextElement;
            }

            // Reset counter
            Clear();
        }
        #endregion

        #region Remove where
        /// <summary>
        /// Removes all the items in the buffer that satisfy the condition defined by <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">A delegate that defines the condition to check for.</param>
        /// <remarks>Ideal for selectively picking all suitable elements from the pool.</remarks>
        public void RemoveWhere(Predicate<T> predicate)
        {
            #region Sanity checks
            if (predicate == null) throw new ArgumentNullException("predicate");
            #endregion

            // Get the first element and the object pointing to it
            IPoolable<T> previousElement = this;
            T currentElement = _firstElement;

            while (currentElement != null)
            {
                // Remove the linkage beforehand to allow the element to be easily added to a new Pool
                var nextElement = currentElement.NextElement;
                currentElement.NextElement = null;

                // Run the delegate to determine whether to remove this element
                if (predicate(currentElement))
                {
                    // Pull up the next entry
                    previousElement.NextElement = nextElement;
                    _count--;

                    // Move on to the next element (previous handle stays the same)
                    currentElement = nextElement;
                }
                else
                {
                    // Restore the linkage if the element wasn't removed
                    currentElement.NextElement = nextElement;

                    // Move on to the next element
                    previousElement = currentElement;
                    currentElement = nextElement;
                }
            }
        }
        #endregion

        #region Remove first
        /// <summary>
        /// Removes the first item in the buffer that satisfies the condition defined by <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">A delegate that defines the condition to check for.</param>
        /// <remarks>Ideal for selectively picking the first suitable element from the pool.</remarks>
        public void RemoveFirst(Predicate<T> predicate)
        {
            #region Sanity checks
            if (predicate == null) throw new ArgumentNullException("predicate");
            #endregion

            // Get the first element and the object pointing to it
            IPoolable<T> previousElement = this;
            T currentElement = _firstElement;

            while (currentElement != null)
            {
                // Remove the linkage beforehand to allow the element to be easily added to a new Pool
                var nextElement = currentElement.NextElement;
                currentElement.NextElement = null;

                // Run the delegate to determine whether to remove this element
                if (predicate(currentElement))
                {
                    // Pull up the next entry
                    previousElement.NextElement = nextElement;
                    _count--;

                    // Exit after first removal
                    return;
                }

                // Restore the linkage if the element wasn't removed
                currentElement.NextElement = nextElement;

                // Move on to the next element
                previousElement = currentElement;
                currentElement = nextElement;
            }
        }
        #endregion
    }
}
