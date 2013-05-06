/*
 * Copyright 2010-2013 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Utils;

namespace Common.Collections
{
    /// <summary>
    /// A mapping wrapper around an <see cref="ICollection{T}"/> that uses automatic type conversion to and from <see cref="string"/>s.
    /// </summary>
    public class StringMappedCollection<T> : ICollection<string>
    {
        private readonly ICollection<T> _collection;
        private readonly IEnumerable<string> _stringView;

        public StringMappedCollection(ICollection<T> collection)
        {
            #region Sanity checks
            if (collection == null) throw new ArgumentNullException("collection");
            #endregion

            _collection = collection;
            _stringView = _collection.Select(element => element.ConvertToString());
        }

        public void Add(string item)
        {
            #region Sanity checks
            if (item == null) throw new ArgumentNullException("item");
            #endregion

            _collection.Add(item.ConvertFromString<T>());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _stringView.GetEnumerator();
        }

        public void Clear()
        {
            _collection.Clear();
        }

        public bool Contains(string item)
        {
            return _stringView.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(string item)
        {
            #region Sanity checks
            if (item == null) throw new ArgumentNullException("item");
            #endregion

            return _collection.Remove(item.ConvertFromString<T>());
        }

        public int Count { get { return _stringView.Count(); } }

        public bool IsReadOnly { get { return false; } }
    }
}
