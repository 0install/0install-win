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
using System.Xml.Serialization;
using NanoByte.Common.Properties;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// A key-value string pair for <see ref="XmlCollection" />.
    /// </summary>
    public sealed class XmlDictionaryEntry : IEquatable<XmlDictionaryEntry>, ICloneable
    {
        #region Variables
        /// <summary>
        /// The collection that owns this entry - set to enable automatic duplicate detection!
        /// </summary>
        internal XmlDictionary Parent;

        private string _key;
        #endregion

        #region Properties
        /// <summary>
        /// The unique text key. Warning: If this is changed the <see cref="XmlDictionary"/> must be rebuilt in order to update its internal hash table.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the new key value already exists in the <see cref="Parent"/> dictionary.</exception>
        [XmlAttribute("key")]
        public string Key
        {
            get { return _key; }
            set
            {
                if (Parent != null && Parent.ContainsKey(value))
                    throw new InvalidOperationException(Resources.KeyAlreadyPresent);
                _key = value;
            }
        }

        /// <summary>
        /// The text value.
        /// </summary>
        [XmlText]
        public string Value { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public XmlDictionaryEntry()
        {}

        /// <summary>
        /// Creates a new entry for <see ref="XmlCollection" />.
        /// </summary>
        /// <param name="key">The unique text key.</param>
        /// <param name="value">The text value.</param>
        public XmlDictionaryEntry(string key, string value)
        {
            _key = key;
            Value = value;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            return Key + ": " + Value;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(XmlDictionaryEntry other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Value == Value && other.Key == Key;
        }

        /// <inheritdoc/>
        public static bool operator ==(XmlDictionaryEntry left, XmlDictionaryEntry right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(XmlDictionaryEntry left, XmlDictionaryEntry right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(XmlDictionaryEntry)) return false;
            return Equals((XmlDictionaryEntry)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ (_key != null ? _key.GetHashCode() : 0);
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a plain copy of this entry.
        /// </summary>
        /// <returns>The cloned entry.</returns>
        public XmlDictionaryEntry Clone()
        {
            return new XmlDictionaryEntry(Key, Value);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
