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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Common.Collections
{
    /// <summary>
    /// A collection of <see cref="LocalizableString"/>s with language-search methods.
    /// </summary>
    public class LocalizableStringCollection : C5.ArrayList<LocalizableString>
    {
        #region Add
        /// <summary>
        /// Adds a new string with no associated language to the collection.
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
            Add(new LocalizableString(value, language));
        }
        #endregion

        #region Contains
        /// <summary>
        /// Checks if the collection contains an entry exactly matching the specified language.
        /// </summary>
        /// <param name="language">The exact language to look for; may be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if an element with the specified language exists in the collection; <see langword="false"/> otherwise.</returns>
        /// <seealso cref="GetExactLanguage"/>
        public bool ContainsExactLanguage(CultureInfo language)
        {
            foreach (LocalizableString entry in this)
                if (Equals(language, entry.Language)) return true;
            return false;
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes all entries in the collection exactly matching the specified language.
        /// </summary>
        /// <param name="language">The exact language to look for; may be <see langword="null"/>.</param>
        public void RemoveAll(CultureInfo language)
        {
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
        /// <param name="language">The exact language to look for; may be <see langword="null"/>.</param>
        /// <returns>The string value found in the collection.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no matching strings were found in the collection.</exception>
        /// <seealso cref="ContainsExactLanguage"/>
        public string GetExactLanguage(CultureInfo language)
        {
            foreach (LocalizableString entry in this)
                if (Equals(language, entry.Language)) return entry.Value;
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Returns the best-fitting string in the collection for the specified language.
        /// </summary>
        /// <param name="language">The language to look for; may be <see langword="null"/>.</param>
        /// <returns>The best-fitting string value found in the collection; <see langword="null"/> if the collection is empty.</returns>
        /// <remarks>
        /// Language preferences in decreasing order:
        /// exact match,
        /// same country code and region-neutral,
        /// same country code,
        /// no language specified,
        /// en-US,
        /// first entry in collection
        /// </remarks>
        public string GetBestLanguage(CultureInfo language)
        {
            // Try to find exact match
            foreach (LocalizableString entry in this)
                if (Equals(language, entry.Language)) return entry.Value;

            // No language only qualifies for exact match
            if (language == null) throw new KeyNotFoundException();

            // Try to find same country code and region-neutral
            foreach (LocalizableString entry in this)
            {
                if (entry.Language == null) continue;
                if (language.TwoLetterISOLanguageName == entry.Language.TwoLetterISOLanguageName && entry.Language.IsNeutralCulture) return entry.Value;
            }

            // Try to find same country code
            foreach (LocalizableString entry in this)
            {
                if (entry.Language == null) continue;
                if (language.TwoLetterISOLanguageName == entry.Language.TwoLetterISOLanguageName) return entry.Value;
            }

            // Try to find "no language specified"
            foreach (LocalizableString entry in this)
                if (entry.Language == null) return entry.Value;

            // Try to find "en-US"
            foreach (LocalizableString entry in this)
                if (Equals(entry.Language, new CultureInfo("en-US"))) return entry.Value;

            // Try to find first entry in collection
            return IsEmpty ? null : First.Value;
        }
        #endregion

        #region Set
        /// <summary>
        /// Sets a new string with no associated language in the collection. Pre-existing entries with no associated language are removed.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo")]
        public void Set(string value)
        {
            RemoveAll(null);
            Add(value);
        }

        /// <summary>
        /// Adds a new string with an associated language to the collection. Pre-existing entries with the same language are removed.
        /// </summary>
        /// <param name="value">The actual string value to store.</param>
        /// <param name="language">The language of the <paramref name="value"/>.</param>
        public void Set(string value, CultureInfo language)
        {
            RemoveAll(language);
            Add(value, language);
        }
        #endregion

        //--------------------//

        #region Clone
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