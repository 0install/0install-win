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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using NanoByte.Common.Values.Design;

namespace NanoByte.Common.Collections
{
    /// <summary>
    /// A set of languages that can be serialized as a simple space-separated list of ISO language codes.
    /// </summary>
    /// <remarks>Uses Unix-style language codes with an underscore (_) separator.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "A Set is a special case of a Collection.")]
    [TypeConverter(typeof(StringConstructorConverter<LanguageSet>))]
    [Editor(typeof(LanguageSetEditor), typeof(UITypeEditor))]
    public sealed class LanguageSet : SortedSet<CultureInfo>
    {
        #region Constants
        /// <summary>
        /// All known languages in alphabetical order.
        /// </summary>
        internal static readonly IEnumerable<CultureInfo> KnownLanguages;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Data must be sorted before use.")]
        static LanguageSet()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures);
            Array.Sort(cultures, new CultureComparer());
            KnownLanguages = cultures.Skip(1);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new empty language collection.
        /// </summary>
        public LanguageSet()
            : base(new CultureComparer())
        {}

        /// <summary>
        /// Creates a new language collection pre-filled with a set of languages.
        /// </summary>
        /// <param name="collection"></param>
        public LanguageSet(IEnumerable<CultureInfo> collection)
            : base(collection, new CultureComparer())
        {}

        /// <summary>
        /// Deserializes a space-separated list of languages codes (in the same format as used by the $LANG environment variable).
        /// </summary>
        public LanguageSet(string value)
            : this(ParseString(value))
        {}

        private static IEnumerable<CultureInfo> ParseString(string value)
        {
            if (string.IsNullOrEmpty(value)) yield break;

            // Replace list by parsing input string split by spaces
            foreach (string langCode in value.Split(' '))
            {
                CultureInfo language = null;
                try
                {
                    // Handle Unix-style language codes (even though they are not actually valid in XML)
                    language = new CultureInfo(langCode.Replace('_', '-'));
                }
                catch (ArgumentException)
                {
                    Log.Error("Ignoring unknown language code: " + language);
                }
                if (language != null) yield return language;
            }
        }
        #endregion

        //--------------------//

        #region Add
        /// <summary>
        /// Adds a language identified by a string to the collection.
        /// </summary>
        /// <param name="language">The string identifying the language to add.</param>
        /// <returns><see langword="true"/> if the language could be added, <see langword="false"/> otherwise.</returns>
        public bool Add(string language)
        {
            return Add(new CultureInfo(language));
        }
        #endregion

        #region Contains
        /// <summary>
        /// Determines whether this language set contains any of a set of target languages.
        /// Empty sets count as containing all languages.
        /// </summary>
        public bool ContainsAny(ICollection<CultureInfo> targets)
        {
            #region Sanity checks
            if (targets == null) throw new ArgumentNullException("targets");
            #endregion

            return Count == 0 || targets.Count == 0 || targets.Any(Contains);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Serializes the list as a space-separated list of languages codes.
        /// </summary>
        public override string ToString()
        {
            // Serialize list as string split by spaces
            var output = new StringBuilder();
            foreach (var language in this)
            {
                // .NET uses a hyphen while Unix uses an underscore as a separator
                output.Append(language.ToString().Replace('-', '_') + ' ');
            }

            // Return without trailing whitespaces
            return output.ToString().TrimEnd();
        }
        #endregion
    }
}
