/*
 * Copyright 2006-2013 Bastian Eicher
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
using Common.Values.Design;

namespace Common.Collections
{
    /// <summary>
    /// A set of languages that can be serialized as a simple space-separated list of ISO language codes.
    /// </summary>
    /// <remarks>Uses Unix-style language codes with an underscore (_) separator.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "A Set is a special case of a Collection.")]
    [TypeConverter(typeof(StringConstructorConverter<LanguageSet>))]
    [Editor(typeof(LanguageSetEditor), typeof(UITypeEditor))]
    public sealed class LanguageSet : C5.TreeSet<CultureInfo>
    {
        #region Constants
        /// <summary>
        /// All valid languages in alphabetical order.
        /// </summary>
        internal static readonly IEnumerable<CultureInfo> AllValid;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Data must be sorted before use.")]
        static LanguageSet()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures);
            Array.Sort(cultures, new CultureComparer());
            AllValid = cultures.Skip(1);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new empty language collection.
        /// </summary>
        public LanguageSet() : base(new CultureComparer())
        {}
        
        /// <summary>
        /// Deserializes a space-separated list of languages codes (in the same format as used by the $LANG environment variable).
        /// </summary>
        public LanguageSet(string value) : this()
        {
            if (string.IsNullOrEmpty(value)) return;

            // Replace list by parsing input string split by spaces
            foreach (string language in value.Split(' '))
            {
                // .NET uses a hypen while Unix uses an underscore as a separator
                Add(new CultureInfo(language.Replace('_', '-')));
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
                // .NET uses a hypen while Unix uses an underscore as a separator
                output.Append(language.ToString().Replace('-', '_') + ' ');
            }

            // Return without trailing whitespaces
            return output.ToString().TrimEnd();
        }
        #endregion
    }
}
