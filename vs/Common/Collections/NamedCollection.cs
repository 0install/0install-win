using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Common.Collections
{
    /// <summary>
    /// A keyed collection (pseudo-dictionary) of <see cref="INamed"/> objects.
    /// </summary>
    public class NamedCollection<T> : KeyedCollection<string, T>, INamedCollection<T>, ICloneable where T : INamed
    {
        #region Constructor
        /// <summary>
        /// Creates a new case-insensitive named collection.
        /// </summary>
        public NamedCollection() : base(StringComparer.OrdinalIgnoreCase)
        {}
        #endregion

        //--------------------//

        #region Access
        protected override string GetKeyForItem(T item)
        {
            return item.Name;
        }
        #endregion

        #region Sort
        /// <summary>
        /// Sorts all entries alphabetically by their name.
        /// </summary>
        public void Sort()
        {
            // Get list to sort
            var items = Items as List<T>;

            // Apply and set the sort, if items to sort
            if (items != null)
            {
                items.Sort((x, y) => x.Name.CompareTo(y.Name));
            }
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a shallow copy of this <see cref="NamedCollection{T}"/> (elements are not cloned).
        /// </summary>
        /// <returns>The cloned <see cref="NamedCollection{T}"/>.</returns>
        public virtual object Clone()
        {
            var newCollection = new NamedCollection<T>();
            foreach (T entry in this)
                newCollection.Add(entry);

            return newCollection;
        }
        #endregion
    }
}