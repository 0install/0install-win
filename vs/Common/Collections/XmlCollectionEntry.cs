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
using System.Xml.Serialization;
using Common.Properties;

namespace Common.Collections
{
    /// <summary>
    /// A key-value string pair for <see ref="XmlCollection" />
    /// </summary>
    [XmlType("Entry")]
    public sealed class XmlCollectionEntry : ICloneable
    {
        #region Variables
        /// <summary>
        /// The collection that owns this entry - set to enable automatic duplicate detection!
        /// </summary>
        internal XmlCollection Parent;
        
        private string _key;
        #endregion

        #region Properties
        /// <summary>
        /// The unique text key
        /// </summary>
        [XmlAttribute]
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
        /// The text value
        /// </summary>
        [XmlAttribute]
        public string Value { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Base-constructor for XML serialization. Do not call manually!
        /// </summary>
        public XmlCollectionEntry()
        {}

        /// <summary>
        /// Creates a new entry for <see ref="XmlCollection" />
        /// </summary>
        /// <param name="key">The unique text key</param>
        /// <param name="value">The text value</param>
        public XmlCollectionEntry(string key, string value)
        {
            _key = key;
            Value = value;
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a plain copy of this entry.
        /// </summary>
        /// <returns>The cloned entry.</returns>
        public XmlCollectionEntry CloneEntry()
        {
            // Perform initial shallow copy
            var entry = (XmlCollectionEntry)MemberwiseClone();

            // Remove parent reference (no longer realted due to cloning)
            entry.Parent = null;
            
            return entry;
        }

        /// <summary>
        /// Creates a plain copy of this entry.
        /// </summary>
        /// <returns>The cloned entry casted to a generic <see cref="object"/>.</returns>
        public object Clone()
        {
            return CloneEntry();
        }
        #endregion
    }
}
