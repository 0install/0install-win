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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Serialization;

namespace Common.Collections
{
    /// <summary>
    /// A string with an optionally associated language that can be XML serialized to an element with an xml:lang tag.
    /// </summary>
    public sealed class LocalizableString : IEquatable<LocalizableString>, ICloneable
    {
        #region Properties
        /// <summary>
        /// The actual string value to store.
        /// </summary>
        [XmlText]
        public string Value { get; set; }

        /// <summary>
        /// The language of the <see cref="Value"/>; use <see cref="CultureInfo.InvariantCulture"/> for none.
        /// </summary>
        [XmlIgnore]
        public CultureInfo Language { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Language"/>
        [XmlAttribute("lang", Namespace="http://www.w3.org/XML/1998/namespace", DataType = "language")] // Will be serialized as xml:lang, must be done this way for Mono
        public string LanguageString
        {
            get { return (Language == null || string.IsNullOrEmpty(Language.ToString())) ? null : Language.ToString(); }
            set { Language = string.IsNullOrEmpty(value) ? CultureInfo.InvariantCulture : new CultureInfo(value); }
        }
        #endregion

        #region Contructor
        /// <summary>
        /// Creates a new string with an associated language.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        /// <param name="language">The language of the <paramref name="value"/>.</param>
        public LocalizableString(string value, CultureInfo language)
        {
            Value = value;
            Language = language;
        }

        /// <summary>
        /// Creates a new string with no associated language.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        public LocalizableString(string value) : this(value, CultureInfo.InvariantCulture)
        {}

        /// <summary>
        /// Creates an empty string with no associated language.
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", Justification = "In this case the language is part of the data to be stored and not used for localizing the output formatting")]
        public LocalizableString() : this(null)
        {}
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return (Language == null) ? Value : Value + " (" + Language + ")";
        }
        #endregion

        #region Equality
        public bool Equals(LocalizableString other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Value == Value && other.LanguageString == LanguageString;
        }

        public static bool operator ==(LocalizableString left, LocalizableString right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LocalizableString left, LocalizableString right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(LocalizableString) && Equals((LocalizableString)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Value ?? "").GetHashCode() * 397) ^ (LanguageString ?? "").GetHashCode();
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a plain copy of this string.
        /// </summary>
        /// <returns>The cloned string.</returns>
        public LocalizableString CloneString()
        {
            return new LocalizableString(Value, Language);
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
    }
}
