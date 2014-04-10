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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NanoByte.Common.Properties;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// A collection that can easily be monitored for changes via events.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class MonitoredCollection<T> : Collection<T>
    {
        #region Events
        /// <summary>
        /// Occurs whenever something in the collection changes.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
        [Description("Occurs whenever something in the collection changes.")]
        public event Action Changed;

        /// <summary>
        /// Occurs when a new item has just been added to the collection.
        /// </summary>
        [Description("Occurs when a new item has just been added to the collection.")]
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "The normal event signature would have been unnecessarily verbose")]
        public event Action<T> Added;

        /// <summary>
        /// Occurs when an item is just about to be removed from the collection.
        /// </summary>
        [Description("Occurs when an item is just about to be removed from the collection.")]
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "The normal event signature would have been unnecessarily verbose")]
        public event Action<T> Removing;

        /// <summary>
        /// Occurs when an item has just been removed from the collection.
        /// </summary>
        [Description("Occurs when an item has just been removed from the collection.")]
        [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "The normal event signature would have been unnecessarily verbose")]
        public event Action<T> Removed;

        private void OnChanged()
        {
            if (!_dontRaiseEvents && Changed != null) Changed();
        }

        private void OnAdded(T item)
        {
            if (!_dontRaiseEvents && Added != null) Added(item);
        }

        private void OnRemoving(T item)
        {
            // Note: This event cannot be blocked!
            if (Removing != null) Removing(item);
        }

        private void OnRemoved(T item)
        {
            if (!_dontRaiseEvents && Removed != null) Removed(item);
        }
        #endregion

        #region Variables
        /// <summary>Do not raise the <see cref="Added"/> and <see cref="Removed"/> events while <see langword="true"/>.
        /// <see cref="Removing"/> cannot be blocked!</summary>
        private bool _dontRaiseEvents;
        #endregion

        #region Properties
        /// <summary>
        /// The maximum number of elements; 0 for no limit.
        /// </summary>
        public int MaxElements { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new monitored collection.
        /// </summary>
        public MonitoredCollection()
        {}

        /// <summary>
        /// Creates a new monitored collection with an upper limit to the number of elements.
        /// </summary>
        /// <param name="maxElements">The maximum number of elements; 0 for no limit.</param>
        public MonitoredCollection(int maxElements)
        {
            MaxElements = maxElements;
        }
        #endregion

        //--------------------//

        #region Hooks
        /// <inheritdoc/>
        protected override void InsertItem(int index, T item)
        {
            if (MaxElements != 0 && Count == MaxElements)
                throw new InvalidOperationException(Resources.MaxElementsExceeded);

            base.InsertItem(index, item);
            OnAdded(item);
            OnChanged();
        }

        /// <inheritdoc/>
        protected override void SetItem(int index, T item)
        {
            T oldItem = Items[index];

            OnRemoving(oldItem);
            base.SetItem(index, item);
            OnRemoved(oldItem);
            OnAdded(item);
            OnChanged();
        }

        /// <inheritdoc/>
        protected override void RemoveItem(int index)
        {
            var oldItem = Items[index];

            OnRemoving(oldItem);
            base.RemoveItem(index);
            OnRemoved(oldItem);
        }

        /// <inheritdoc/>
        protected override void ClearItems()
        {
            foreach (var item in this) OnRemoving(item);

            // Create backup of collection to be able to dispatch Removed events afterwards
            var oldItems = new List<T>(this);

            base.ClearItems();

            // Raise the events afterwards en bloc
            foreach (var item in oldItems) OnRemoved(item);

            OnChanged();
        }
        #endregion

        //--------------------//

        #region Mass access
        /// <summary>
        /// Adds all the items in <paramref name="collection"/> to the collection that weren't already there.
        /// </summary>
        /// <param name="collection">A collection of items to add to the collection.</param>
        /// <remarks>
        ///   <para>All events are raised en bloc after the items have been added.</para>
        ///   <para>After calling this method this collection will contain a superset of the items in <paramref name="collection"/>, but not necessarily in the same order.</para>
        /// </remarks>
        public void AddMany(IEnumerable<T> collection)
        {
            #region Sanity checks
            if (collection == null) throw new ArgumentNullException("collection");
            if (collection == this) throw new ArgumentException(Resources.CannotAddCollectionToSelf, "collection");
            #endregion

            // Create separate collection to be able to dispatch events afterwards
            var added = new LinkedList<T>();

            // Add all items without raising the events yet
            _dontRaiseEvents = true;
            foreach (var item in collection.Where(item => !Contains(item)))
            {
                Add(item);
                added.AddLast(item);
            }
            _dontRaiseEvents = false;

            // Raise the events afterwards en bloc
            foreach (var item in added) OnAdded(item);
            OnChanged();
        }

        /// <summary>
        /// Adds all the items in <paramref name="collection"/> to the collection that weren't already there and
        /// removes all items in the collection that are not in <paramref name="collection"/>. 
        /// </summary>
        /// <param name="collection">A collection of items to add to the collection.</param>
        /// <remarks>
        ///   <para>All events are raised en bloc after the items have been added.</para>
        ///   <para>After calling this method this collection will contain the same items as <paramref name="collection"/>, but not necessarily in the same order.</para>
        /// </remarks>
        public void SetMany(ICollection<T> collection)
        {
            #region Sanity checks
            if (collection == null) throw new ArgumentNullException("collection");
            if (collection == this) throw new ArgumentException(Resources.CannotAddCollectionToSelf, "collection");
            #endregion

            // Create backup of collection to be able to remove while enumerating
            var copy = new List<T>(this);

            // Create separate collection to be able to dispatch events afterwards
            var removed = new LinkedList<T>();

            // Remove superflous items without raising the events yet
            _dontRaiseEvents = true;
            foreach (var item in copy.Where(item => !collection.Contains(item)))
            {
                Remove(item);
                removed.AddLast(item);
            }
            _dontRaiseEvents = false;

            // Raise the events afterwards en bloc
            if (Removed != null)
                foreach (var item in removed) Removed(item);

            // Add any new items (raising all events en bloc)
            AddMany(collection);
        }
        #endregion
    }
}
