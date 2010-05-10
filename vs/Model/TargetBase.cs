/*
 * Copyright 2010 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A common base class for <see cref="ImplementationBase"/> and <see cref="FeedReference"/>.
    /// Contains language and architecture parameters.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    public abstract class TargetBase : IEquatable<TargetBase>
    {
        #region Properties
        // Order is always alphabetical, duplicate entries are not allowed
        private readonly C5.TreeSet<CultureInfo> _languages = new C5.TreeSet<CultureInfo>(new CultureComparer());
        /// <summary>
        /// The natural language(s) which an <see cref="Implementation"/> supports, as a space-separated list of languages codes (in the same format as used by the $LANG environment variable).
        /// </summary>
        /// <example>For example, the value "en_GB fr" would be used for a package supporting British English and French.</example>
        [Category("Release"), Description("The natural language(s) which an implementation supports, as a space-separated list of languages codes (in the same format as used by the $LANG environment variable).")]
        [XmlIgnore]
        public ICollection<CultureInfo> Languages { get { return _languages; } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [XmlAttribute("langs"), DefaultValue(""), Browsable(false)]
        public string LanguagesString
        {
            get
            {
                // Serialize list as string split by spaces
                var output = new StringBuilder();
                foreach (var language in _languages)
                {
                    // .NET uses a hypen while Zero Install uses an underscore as a seperator
                    output.Append(language.ToString().Replace('-', '_') + ' ');
                }
                // Return without trailing space
                return output.ToString().TrimEnd();
            }
            set
            {
                _languages.Clear();
                if (string.IsNullOrEmpty(value)) return;

                // Replace list by parsing input string split by spaces
                foreach (string language in value.Split(' '))
                {
                    // .NET uses a hypen while Zero Install uses an underscore as a seperator
                    _languages.Add(new CultureInfo(language.Replace('_', '-')));
                }
            }
        }

        /// <summary>
        /// For platform-specific binaries, the platform for which an <see cref="Implementation"/> was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. 
        /// </summary>
        /// <remarks>The injector knows that certain platforms are backwards-compatible with others, so binaries with arch="Linux-i486"  will still be available on Linux-i686 machines, for example.</remarks>
        [Category("Release"), Description("For platform-specific binaries, the platform for which an implementation was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. ")]
        [XmlIgnore]
        public Architecture Architecture { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false)]
        [XmlAttribute("arch"), DefaultValue("*-*")]
        public string ArchitectureString
        {
            get { return Architecture.ToString(); }
            set { Architecture = new Architecture(value); }
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Copies all known values from one instance to another. Helper method for instance cloning.
        /// </summary>
        protected static void CloneFromTo(TargetBase from, TargetBase to)
        {
            #region Sanity checks
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");
            #endregion

            to.LanguagesString = from.LanguagesString;
            to.ArchitectureString = from.ArchitectureString;
        }
        #endregion
        
        #region Equality
        public bool Equals(TargetBase other)
        {
            if (ReferenceEquals(null, other)) return false;

            return _languages.UnsequencedEquals(other._languages) && other.Architecture == Architecture;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LanguagesString != null ? LanguagesString.GetHashCode() : 0) * 397) ^ Architecture.GetHashCode();
            }
        }
        #endregion
    }
}
