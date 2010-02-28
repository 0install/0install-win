using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.Collections
{
    /// <summary>
    /// A read-only wrapper around an <see cref="IDictionary"/>
    /// </summary>
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
    {
        private IDictionary<TKey, TValue> dict;
        private IDictionary idict;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyDictionary&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="dictionaryToWrap">The dictionary to wrap.</param>
        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionaryToWrap)
        {
            dict = dictionaryToWrap;
            idict = (IDictionary)dict;
        }

        /// <summary>
        /// Returns a read only dictionary.
        /// </summary>
        /// <param name="dictionaryToWrap">The dictionary to wrap.</param>
        /// <returns>.</returns>
        public static ReadOnlyDictionary<TKey, TValue> AsReadOnly(IDictionary<TKey, TValue> dictionaryToWrap)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionaryToWrap);
        }

        /// <exception cref="NotSupportedException">Is always thrown because this dictionary is read-only.</exception>
        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException("The dictionary is read-only");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2"></see> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            return dict.ContainsKey(key);
        }

        /// <summary>
        /// Gets a <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TKey> Keys
        {
            get
            {
                return dict.Keys;
            }
        }

        /// <exception cref="NotSupportedException">Is always thrown because this dictionary is read-only.</exception>
        public bool Remove(TKey key)
        {
            throw new NotSupportedException("The dictionary is read-only");
        }

        /// <summary>
        /// Tries the get value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return dict.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        public ICollection<TValue> Values
        {
            get
            {
                return dict.Values;
            }
        }

        /// <summary>
        /// Gets the <typerefparam name="TValue"/> with the specified key.
        /// </summary>
        /// <exception cref="NotSupportedException">Is thrown on set because this dictionary is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                return dict[key];
            }
            set
            {
                throw new NotSupportedException("The dictionary is read-only");
            }
        }

        /// <exception cref="NotSupportedException">Is always thrown because this dictionary is read-only.</exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("The dictionary is read-only");
        }

        /// <exception cref="NotSupportedException">Is always thrown because this dictionary is read-only.</exception>
        public void Clear()
        {
            throw new NotSupportedException("The dictionary is read-only");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return dict.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            dict.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get
            {
                return dict.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <value></value>
        /// <returns><see langword="true"/> if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        /// <exception cref="NotSupportedException">Is always thrown because this dictionary is read-only.</exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException("The dictionary is read-only");
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return dict.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return idict.GetEnumerator();
        }

        /// <exception cref="NotSupportedException">Is always thrown because this dictionary is read-only.</exception>
        public void Add(object key, object value)
        {
            throw new NotSupportedException("The dictionary is read-only");
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IDictionary"></see> object contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.IDictionary"></see> object.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IDictionary"></see> contains an element with the key; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">key is null. </exception>
        public bool Contains(object key)
        {
            return idict.Contains(key);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IDictionaryEnumerator"></see> object for the <see cref="T:System.Collections.IDictionary"></see> object.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return idict.GetEnumerator();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size.
        /// </summary>
        /// <value></value>
        /// <returns><see langword="true"/> if the <see cref="T:System.Collections.IDictionary"></see> object has a fixed size; otherwise, false.</returns>
        public bool IsFixedSize
        {
            get
            {
                return idict.IsFixedSize;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        ICollection IDictionary.Keys
        {
            get
            {
                return idict.Keys;
            }
        }

        /// <exception cref="NotSupportedException">Is always thrown because this dictionary is read-only.</exception>
        public void Remove(object key)
        {
            throw new NotSupportedException("The dictionary is read-only");
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.Collections.Generic.ICollection`1"></see> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"></see>.</returns>
        ICollection IDictionary.Values
        {
            get
            {
                return idict.Values;
            }
        }


        /// <summary>
        /// Gets the <see cref="Object"/> with the specified key.
        /// </summary>
        /// <exception cref="NotSupportedException">Is thrown on set because this dictionary is read-only.</exception>
        public object this[object key]
        {
            get
            {
                return idict[key];
            }
            set
            {
                throw new NotSupportedException("The dictionary is read-only");
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">array is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or- index is equal to or greater than the length of array.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"></see> is greater than the available space from index to the end of the destination array. </exception>
        /// <exception cref="T:System.InvalidCastException">The type of the source <see cref="T:System.Collections.ICollection"></see> cannot be cast automatically to the type of the destination array. </exception>
        public void CopyTo(Array array, int index)
        {
            idict.CopyTo(array, index);
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe).
        /// </summary>
        /// <value></value>
        /// <returns><see langword="true"/> if access to the <see cref="T:System.Collections.ICollection"></see> is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized
        {
            get
            {
                return idict.IsSynchronized;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"></see>.</returns>
        public object SyncRoot
        {
            get
            {
                return idict.SyncRoot;
            }
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"></see>) for this serialization.</param>
        /// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ISerializable serializable = dict as ISerializable;
            serializable.GetObjectData(info, context);
        }
    }
}
