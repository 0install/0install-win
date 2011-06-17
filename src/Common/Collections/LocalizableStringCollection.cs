/*
 * Copyright 2006-2011 Bastian Eicher
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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Common.Collections
{
    /// <summary>
    /// A collection of <see cref="LocalizableString"/>s with language-search methods.
    /// </summary>
    [Serializable]
    public class LocalizableStringCollection : C5.ArrayList<LocalizableString>
    {
        #region Add
        /// <summary>
        /// Adds a new <code>en</code> string to the collection.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo")]
        public void Add(string value)
        {
            Add(new LocalizableString(value));
        }

        /// <summary>
        /// Adds a new string with an associated language to the collection.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        /// <param name="language">The language of the <paramref name="value"/>.</param>
        public void Add(string value, CultureInfo language)
        {
            #region Sanity checks
            if (language == null) throw new ArgumentNullException("language");
            #endregion

            Add(new LocalizableString(value, language));
        }
        #endregion

        #region Contains
        /// <summary>
        /// Checks if the collection contains an entry exactly matching the specified language.
        /// </summary>
        /// <param name="language">The exact language to look for; use <see cref="CultureInfo.InvariantCulture"/> for none.</param>
        /// <returns><see langword="true"/> if an element with the specified language exists in the collection; <see langword="false"/> otherwise.</returns>
        /// <seealso cref="GetExactLanguage"/>
        public bool ContainsExactLanguage(CultureInfo language)
        {
            #region Sanity checks
            if (language == null) throw new ArgumentNullException("language");
            #endregion

            foreach (LocalizableString entry in this)
                if (Equals(language, entry.Language)) return true;
            return false;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes all entries in the collection exactly matching the specified language.
        /// </summary>
        /// <param name="language">The exact language to look for; use <see cref="CultureInfo.InvariantCulture"/> for none.</param>
        public void RemoveAll(CultureInfo language)
        {
            #region Sanity checks
            if (language == null) throw new ArgumentNullException("language");
            #endregion

            var toRemove = new LinkedList<LocalizableString>();
            foreach (LocalizableString entry in this)
                if (Equals(language, entry.Language)) toRemove.AddLast(entry);
            RemoveAll(toRemove);
        }
        #endregion

        #region Get
        /// <summary>
        /// Returns the first string in the collection exactly matching the specified language.
        /// </summary>
        /// <param name="language">The exact language to look for; use <see cref="CultureInfo.InvariantCulture"/> for none.</param>
        /// <returns>The string value found in the collection.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no matching strings were found in the collection.</exception>
        /// <seealso cref="ContainsExactLanguage"/>
        public string GetExactLanguage(CultureInfo language)
        {
            #region Sanity checks
            if (language == null) throw new ArgumentNullException("language");
            #endregion

            foreach (LocalizableString entry in this)
                if (Equals(language, entry.Language)) return entry.Value;
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Returns the best-fitting string in the collection for the specified language.
        /// </summary>
        /// <param name="language">The language to look for; use <see cref="CultureInfo.InvariantCulture"/> for none.</param>
        /// <returns>The best-fitting string value found in the collection; <see langword="null"/> if the collection is empty.</returns>
        /// <remarks>
        /// Language preferences in decreasing order:<br/>
        /// 1. exact match<br/>
        /// 2. same language with neutral culture<br/>
        /// 3. en<br/>
        /// 4. en-US<br/>
        /// 5. first entry in collection
        /// </remarks>
        public string GetBestLanguage(CultureInfo language)
        {
            #region Sanity checks
            if (language == null) throw new ArgumentNullException("language");
            #endregion

            // Try to find exact match
            foreach (LocalizableString entry in this)
                if (Equals(language, entry.Language)) return entry.Value;

            // Try to find same language with neutral culture
            foreach (LocalizableString entry in this)
            {
                if (entry.Language == null) continue;
                if (language.TwoLetterISOLanguageName == entry.Language.TwoLetterISOLanguageName && entry.Language.IsNeutralCulture) return entry.Value;
            }

            // Try to find "en"
            foreach (LocalizableString entry in this)
                if (Equals(entry.Language, new CultureInfo("en"))) return entry.Value;

            // Try to find "en-US"
            foreach (LocalizableString entry in this)
                if (Equals(entry.Language, new CultureInfo("en-US"))) return entry.Value;

            // Try to find first entry in collection
            return IsEmpty ? null : First.Value;
        }
        #endregion

        #region Set
        /// <summary>
        /// Sets a new <code>en</code> string in the collection. Preexisting <code>en</code> entries are removed.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo")]
        public void Set(string value)
        {
            RemoveAll(null);
            if (value != null) Add(value);
        }

        /// <summary>
        /// Adds a new string with an associated language to the collection. Preexisting entries with the same language are removed.
        /// </summary>
        /// <param name="value">The actual string value to store; use <see cref="CultureInfo.InvariantCulture"/> for none.</param>
        /// <param name="language">The language of the <paramref name="value"/>.</param>
        public void Set(string value, CultureInfo language)
        {
            #region Sanity checks
            if (language == null) throw new ArgumentNullException("language");
            #endregion

            RemoveAll(language);
            if (value != null) Add(value, language);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="LocalizableStringCollection"/> (elements are cloned).
        /// </summary>
        /// <returns>The cloned <see cref="LocalizableStringCollection"/>.</returns>
        public override object Clone()
        {
            var newDict = new LocalizableStringCollection();
            foreach (LocalizableString entry in this)
                newDict.Add(entry.CloneString());

            return newDict;
        }
        #endregion
    }
}