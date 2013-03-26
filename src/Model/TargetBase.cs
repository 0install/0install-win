/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Abstract base class for <see cref="Element"/> and <see cref="FeedReference"/>.
    /// Contains language and architecture parameters.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("target-base", Namespace = Feed.XmlNamespace)]
    public abstract class TargetBase : FeedElement
    {
        #region Properties
        // Order is always alphabetical, duplicate entries are not allowed
        private readonly LanguageCollection _languages = new LanguageCollection();

        /// <summary>
        /// The natural language(s) which an <see cref="Model.Implementation"/> supports.
        /// </summary>
        /// <example>For example, the value "en_GB fr" would be used for a package supporting British English and French.</example>
        [Category("Release"), Description("The natural language(s) which an implementation supports.")]
        [XmlIgnore]
        public LanguageCollection Languages { get { return _languages; } }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [XmlAttribute("langs"), DefaultValue("")]
        public string LanguagesString { get { return _languages.ToString(); } set { _languages.FromString(value); } }

        /// <summary>
        /// For platform-specific binaries, the platform for which an <see cref="Model.Implementation"/> was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU. 
        /// </summary>
        /// <remarks>The injector knows that certain platforms are backwards-compatible with others, so binaries with arch="Linux-i486"  will still be available on Linux-i686 machines, for example.</remarks>
        [Category("Release"), Description("For platform-specific binaries, the platform for which an implementation was compiled, in the form os-cpu. Either the os or cpu part may be *, which will make it available on any OS or CPU.")]
        [XmlIgnore]
        public Architecture Architecture { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Architecture"/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [XmlAttribute("arch"), DefaultValue("*-*")]
        public string ArchitectureString { get { return Architecture.ToString(); } set { Architecture = new Architecture(value); } }
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

            to.UnknownElements = from.UnknownElements;
            to.UnknownAttributes = from.UnknownAttributes;
            to.Languages.Clear();
            to.Languages.AddAll(from.Languages);
            to.ArchitectureString = from.ArchitectureString;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(TargetBase other)
        {
            if (other == null) return false;
            return base.Equals(other) && _languages.UnsequencedEquals(other._languages) && other.Architecture == Architecture;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Languages.GetUnsequencedHashCode();
                result = (result * 397) ^ Architecture.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
