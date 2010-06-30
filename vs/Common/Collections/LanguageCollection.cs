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
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Common.Collections
{
    /// <summary>
    /// A collection of languages that can be serialized as a simple space-separated list of ISO language codes with an underscore (_) separator.
    /// </summary>
    public sealed class LanguageCollection : C5.TreeSet<CultureInfo>, IXmlSerializable
    {
        #region Constructor
        /// <summary>
        /// Creates a new empty language collection.
        /// </summary>
        public LanguageCollection() : base(new CultureComparer())
        {}
        #endregion
        
        //--------------------//

        /// <summary>
        /// Adds a language identified by a string to the collection.
        /// </summary>
        /// <param name="language">The string identifying the language to add.</param>
        /// <returns><see langword="true"/> if the language could be added, <see langword="false"/> otherwise.</returns>
        public bool Add(string language)
        {
            return Add(new CultureInfo(language));
        }

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            // Serialize list as string split by spaces
            var output = new StringBuilder();
            foreach (var language in this)
            {
                // .NET uses a hypen while Zero Install uses an underscore as a seperator
                output.Append(language.ToString().Replace('-', '_') + ' ');
            }

            // Return without trailing whitespaces
            return output.ToString().TrimEnd();
        }

        public static LanguageCollection FromString(string value)
        {
            var result = new LanguageCollection();

            if (string.IsNullOrEmpty(value)) return result;

            // Replace list by parsing input string split by spaces
            foreach (string language in value.Split(' '))
            {
                // .NET uses a hypen while Zero Install uses an underscore as a seperator
                result.Add(new CultureInfo(language.Replace('_', '-')));
            }

            return result;
        }
        #endregion

        #region XML Serialization
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            #region Sanity checks
            if (reader == null) throw new ArgumentNullException("reader");
            #endregion

            Clear();

            string value = reader.ReadElementContentAsString();
            if (string.IsNullOrEmpty(value)) return;

            // Replace list by parsing input string split by spaces
            foreach (string language in value.Split(' '))
            {
                // .NET uses a hypen while Zero Install uses an underscore as a seperator
                Add(new CultureInfo(language.Replace('_', '-')));
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            #region Sanity checks
            if (writer == null) throw new ArgumentNullException("writer");
            #endregion
            
            writer.WriteString(ToString());
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }
        #endregion
    }
}
