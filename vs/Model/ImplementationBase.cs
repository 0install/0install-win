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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Information for identifying an implementation of an <see cref="Feed"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("implementation-base", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    public abstract class ImplementationBase : Element, IEquatable<ImplementationBase>
    {
        #region Properties
        /// <summary>
        /// A unique identifier for this implementation.
        /// </summary>
        /// <remarks>For example, when the user marks a particular version as buggy this identifier is used to keep track of it, and saving and restoring selections uses it.</remarks>
        [Category("Identity"), Description("A unique identifier for this implementation.")]
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// If the feed file is a local file (the interface 'uri' starts with /) then the local-path attribute may contain the pathname of a local directory (either an absolute path or a path relative to the directory containing the feed file).
        /// </summary>
        [Category("Identity"), Description("If the feed file is a local file (the interface 'uri' starts with /) then the local-path attribute may contain the pathname of a local directory (either an absolute path or a path relative to the directory containing the feed file).")]
        [XmlAttribute("local-path"), DefaultValue("")]
        public string LocalPath { get; set; }

        private ManifestDigest _manifestDigest;
        /// <summary>
        /// Digests of the .manifest file using various hashing algorithms.
        /// </summary>
        [Category("Identity"), Description("Digests of the .manifest file using various hashing algorithms.")]
        [XmlElement("manifest-digest")]
        public ManifestDigest ManifestDigest { get { return _manifestDigest; } set { _manifestDigest = value; } }
        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Sets missing default values.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the interface again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            // Merge the version modifier to the normal version attribute
            if (!string.IsNullOrEmpty(VersionModifier))
            {
                Version = new ImplementationVersion(Version + VersionModifier);
                VersionModifier = null;
            }

            // Default stability rating to testing
            if (Stability == Stability.Unset) Stability = Stability.Testing;

            // Check if stuff may be read from the ID
            if (string.IsNullOrEmpty(ID)) return;

            // Fill in values (only if missing) using legacy entries (indentified by prefixes)
            if (string.IsNullOrEmpty(LocalPath) && (ID.StartsWith(".") || ID.StartsWith("/"))) LocalPath = ID;
            else ManifestDigest.ParseID(ID, ref _manifestDigest);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Copies all known values from one instance to another. Helper method for instance cloning.
        /// </summary>
        protected static void CloneFromTo(ImplementationBase from, ImplementationBase to)
        {
            #region Sanity checks
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");
            #endregion

            Element.CloneFromTo(from, to);
            to.ID = from.ID;
            to.LocalPath = from.LocalPath;
            to.ManifestDigest = from.ManifestDigest;
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the implementation in the form "Implementation: Version (ID)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("Implementation: {0} ({1})", Version, ID);
        }
        #endregion

        #region Equality
        public bool Equals(ImplementationBase other)
        {
            if (ReferenceEquals(null, other)) return false;

            return base.Equals(other) && other.ID == ID && other.LocalPath == LocalPath && other.ManifestDigest == ManifestDigest;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ImplementationBase) && Equals((ImplementationBase)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (ID ?? "").GetHashCode();
                result = (result * 397) ^ (LocalPath ?? "").GetHashCode();
                result = (result * 397) ^ ManifestDigest.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
