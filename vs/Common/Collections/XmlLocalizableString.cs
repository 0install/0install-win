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
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Common.Collections
{
    /// <summary>
    /// A string with an optionally associated language that can be XML serialized to an element with an xml:lang tag.
    /// </summary>
    public struct XmlLocalizableString : IEquatable<XmlLocalizableString>, ICloneable, IXmlSerializable
    {
        #region Properties
        /// <summary>
        /// The actual string value to store.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The language of the <see cref="Value"/>; may be <see langword="null"/>.
        /// </summary>
        public CultureInfo Language { get; set; }
        #endregion

        #region Contructor
        /// <summary>
        /// Creates a new string with no associated language.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        public XmlLocalizableString(string value) : this()
        {
            Value = value;
        }

        /// <summary>
        /// Creates a new string with an associated language.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        /// <param name="language">The language of the <paramref name="value"/>.</param>
        public XmlLocalizableString(string value, CultureInfo language) : this(value)
        {
            Language = language;
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return (Language == null) ? Value : Value + " (" + Language + ")";
        }
        #endregion

        #region Equality
        public bool Equals(XmlLocalizableString other)
        {
            return other.Value == Value && Equals(other.Language, Language);
        }

        public static bool operator ==(XmlLocalizableString left, XmlLocalizableString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(XmlLocalizableString left, XmlLocalizableString right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(XmlLocalizableString)) return false;
            return Equals((XmlLocalizableString)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value != null ? Value.GetHashCode() : 0) * 397) ^ (Language != null ? Language.GetHashCode() : 0);
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a plain copy of this string.
        /// </summary>
        /// <returns>The cloned string.</returns>
        public XmlLocalizableString CloneString()
        {
            return new XmlLocalizableString(Value, Language);
        }

        /// <summary>
        /// Creates a plain copy of this string.
        /// </summary>
        /// <returns>The cloned string casted to a generic <see cref="object"/>.</returns>
        public object Clone()
        {
            return CloneString();
        }
        #endregion
        
        //--------------------//

        #region XML Serialization
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            #region Sanity checks
            if (reader == null) throw new ArgumentNullException("reader");
            #endregion

            // Read xml:lang attribute
            if (!string.IsNullOrEmpty(reader.XmlLang)) Language = new CultureInfo(reader.XmlLang);

            // Read actual string value
            Value = reader.ReadElementContentAsString();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            #region Sanity checks
            if (writer == null) throw new ArgumentNullException("writer");
            #endregion

            // Write xml:lang attribute
            if (Language != null) writer.WriteAttributeString("xml", "lang", "", Language.ToString());

            // Write actual string value
            writer.WriteString(Value);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }
        #endregion
    }
}
