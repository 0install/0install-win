using System;
using System.Collections.Generic;
using System.ComponentModel;
using Common.Properties;
using Common.Storage;

namespace Common.Collections
{
    /// <summary>
    /// A string dictionary that supports data-binding and can be XML serialized
    /// </summary>
    public class XmlCollection : BindingList<XmlCollectionEntry>, ICloneable
    {
        #region Add
        /// <summary>
        /// Adds a new value and links it to a key
        /// </summary>
        /// <param name="key">The key object</param>
        /// <param name="value">The value</param>
        public void Add(string key, string value)
        {
            if (!string.IsNullOrEmpty(key) && ContainsKey(key))
                throw new ArgumentException(Resources.KeyAlreadyPresent, "key");
            Add(new XmlCollectionEntry(key, value));
        }

        protected override void InsertItem(int index, XmlCollectionEntry item)
        {
            if (!string.IsNullOrEmpty(item.Key) && ContainsKey(item.Key))
                throw new ArgumentException(Resources.KeyAlreadyPresent, "item");
            item.Parent = this;

            base.InsertItem(index, item);
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes all values asigned to this key
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns><see langword="true"/> if one or more elements were removed; otherwise, false</returns>
        public bool Remove(string key)
        {
            // Build a list of elements to remove
            var pendingRemove = new LinkedList<XmlCollectionEntry>();
            foreach (XmlCollectionEntry pair in this)
                if (pair.Key.Equals(key)) pendingRemove.AddLast(pair);

            // Remove the elements one-by-one
            foreach (XmlCollectionEntry pair in pendingRemove) Remove(pair);

            // Were any elements removed?
            return (pendingRemove.Count > 0);
        }
        #endregion

        #region Contains
        /// <summary>
        /// Checks whether this collection contains a certain key
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns><see langword="true"/> if the key was found</returns>
        public bool ContainsKey(string key)
        {
            foreach (XmlCollectionEntry entry in this)
                if (entry.Key == key) return true;
            return false;
        }

        /// <summary>
        /// Checks whether this collection contains a certain value
        /// </summary>
        /// <param name="value">The value to look for</param>
        /// <returns><see langword="true"/> if the value was found</returns>
        public bool ContainsValue(string value)
        {
            foreach (XmlCollectionEntry entry in this)
                if (entry.Value == value) return true;
            return false;
        }
        #endregion

        #region Sort
        /// <summary>
        /// Sorts all entries alphabetically by their key
        /// </summary>
        public void Sort()
        {
            // Get list to sort
            var items = Items as List<XmlCollectionEntry>;

            // Apply the sorting algorithms, if items are sortable
            if (items != null)
            {
                items.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase));
            }

            // Let bound controls know they should refresh their views
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        #endregion

        #region Indexer
        /// <summary>
        /// Returns the value associated to a specific key
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns>The value associated to <paramref name="key"/>.</returns>
        /// <exception cref="KeyNotFoundException">Throw if <paramref name="key"/> was not found in the collection.</exception>
        public string GetValue(string key)
        {
            foreach (XmlCollectionEntry pair in this)
                if (pair.Key.Equals(key)) return pair.Value;
            throw new KeyNotFoundException();
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Convert this <see cref="XmlCollection"/> to a <see cref="Dictionary{TKey,TValue}"/> for better lookup-performance
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> containing the same data as this collection</returns>
        public IDictionary<string, string> ToDictionary()
        {
            var dictionary = new Dictionary<string, string>();
            foreach (XmlCollectionEntry entry in this)
                dictionary.Add(entry.Key, entry.Value);
            return dictionary;
        }
        #endregion

        #region Storage
        /// <summary>
        /// Loads an XML collection from an XML file
        /// </summary>
        /// <param name="path">The file to load from</param>
        /// <returns>The loaded table</returns>
        public static XmlCollection Load(string path)
        {
            return XmlStorage.Load<XmlCollection>(path);
        }

        /// <summary>
        /// Saves this XML collection in an XML file
        /// </summary>
        /// <param name="path">The file to save in</param>
        public void Save(string path)
        {
            XmlStorage.Save(path, this);
        }
        #endregion

        #region Clone
        public virtual object Clone()
        {
            var newDict = new XmlCollection();
            foreach (XmlCollectionEntry entry in this)
                newDict.Add(entry.CloneEntry());

            return newDict;
        }
        #endregion
    }
}