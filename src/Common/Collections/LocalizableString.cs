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
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// A string with an associated language that can be XML serialized to an element with an xml:lang tag.
    /// </summary>
    public sealed class LocalizableString : IEquatable<LocalizableString>, ICloneable
    {
        #region Variables
        /// <summary>
        /// The default language: english with an invariant country.
        /// </summary>
        public static readonly CultureInfo DefaultLanguage = new CultureInfo("en");
        #endregion

        #region Properties
        /// <summary>
        /// The actual string value to store.
        /// </summary>
        [Description("The actual string value to store.")]
        [XmlText]
        public string Value { get; set; }

        private CultureInfo _language = DefaultLanguage;

        /// <summary>
        /// The language of the <see cref="Value"/>; must not be <see langword="null"/>.
        /// </summary>
        [Description("The language of the Value.")]
        [XmlIgnore]
        public CultureInfo Language
        {
            get { return _language; }
            set
            {
                #region Sanity checks
                if (value == null) throw new ArgumentNullException("value");
                #endregion

                _language = value.Equals(CultureInfo.InvariantCulture) ? DefaultLanguage : value;
            }
        }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Language"/>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        [XmlAttribute("lang", Namespace = "http://www.w3.org/XML/1998/namespace", DataType = "language") /* Will be serialized as xml:lang, must be done this way for Mono */]
        public string LanguageString
        {
            get { return Language.ToString(); }
            set
            {
                try
                {
                    Language = string.IsNullOrEmpty(value)
                        // Default to English language
                        ? DefaultLanguage
                        // Handle Unix-style language codes (even though they are not actually valid in XML)
                        : new CultureInfo(value.Replace("_", "-"));
                }
                catch (ArgumentException)
                {
                    Log.Error("Ignoring unknown language code: " + value);
                }
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            return (Language == null) ? Value : Value + " (" + Language + ")";
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(LocalizableString other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Value == Value && Language.Equals(other.Language);
        }

        /// <inheritdoc/>
        public static bool operator ==(LocalizableString left, LocalizableString right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(LocalizableString left, LocalizableString right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is LocalizableString && Equals((LocalizableString)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = Language.GetHashCode();
                if (Value != null) result = (result * 397) ^ Value.GetHashCode();
                return result;
            }
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a plain copy of this string.
        /// </summary>
        /// <returns>The cloned string.</returns>
        public LocalizableString Clone()
        {
            return new LocalizableString {Language = Language, Value = Value};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }
}
