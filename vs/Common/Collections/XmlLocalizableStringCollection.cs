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

using System.Collections.Generic;
using System.Globalization;

namespace Common.Collections
{
    /// <summary>
    /// An unsorted collection of <see cref="XmlLocalizableString"/>s with language-search methods.
    /// </summary>
    public class XmlLocalizableStringCollection : C5.ArrayList<XmlLocalizableString>
    {
        #region Add
        /// <summary>
        /// Adds a new string with no associated language to the collection.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        public void Add(string value)
        {
            Add(new XmlLocalizableString(value));
        }

        /// <summary>
        /// Adds a new string with an associated language to the collection.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        /// <param name="language">The language of the <paramref name="value"/>.</param>
        public void Add(string value, CultureInfo language)
        {
            Add(new XmlLocalizableString(value, language));
        }
        #endregion

        #region Contains
        /// <summary>
        /// Checks if the collection contains an entry with a specific language.
        /// </summary>
        /// <param name="language">The exact language to look for.</param>
        /// <returns><see langword="true"/> if an element with the specified language exists in the collection; <see langword="false"/> otherwise.</returns>
        public bool ContainsLanguage(CultureInfo language)
        {
            foreach (XmlLocalizableString entry in this)
                if (Equals(language, entry.Language)) return true;
            return false;
        }
        #endregion

        #region Indexer
        /// <summary>
        /// Returns the first string in the collection associated to a specific language.
        /// </summary>
        /// <param name="language">The exact language to look for.</param>
        /// <returns>The first string value found in the collection that is associated to the language specified.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no strings in the collection have the specified <paramref name="language"/>.</exception>
        public string GetLanguage(CultureInfo language)
        {
            foreach (XmlLocalizableString entry in this)
                if (Equals(language, entry.Language)) return entry.Value;
            throw new KeyNotFoundException();
        }
        #endregion

        //--------------------//

        #region Clone
        public override object Clone()
        {
            var newDict = new XmlLocalizableStringCollection();
            foreach (XmlLocalizableString entry in this)
                newDict.Add(entry.CloneString());

            return newDict;
        }
        #endregion
    }
}