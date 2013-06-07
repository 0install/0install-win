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
using System.IO;
using System.Xml.Serialization;
using Common.Utils;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Information for identifying an implementation of a <see cref="Feed"/>.
    /// Common base for <see cref="Implementation"/> and <see cref="PackageImplementation"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("implementation-base", Namespace = Feed.XmlNamespace)]
    public abstract class ImplementationBase : Element
    {
        #region Properties
        /// <summary>
        /// A unique identifier for this implementation. Used when storing implementation-specific user preferences.
        /// </summary>
        [Category("Identity"), Description("A unique identifier for this implementation. Used when storing implementation-specific user preferences.")]
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

        #region Normalize
        /// <summary>
        /// Sets missing default values and handles legacy elements.
        /// </summary>
        /// <param name="feedID">The feed the data was originally loaded from.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing.
        /// It should not be called if you plan on serializing the interface again since it may change some of its structure.</remarks>
        public override void Normalize(string feedID)
        {
            base.Normalize(feedID);

            // Merge the version modifier into the normal version attribute
            if (!string.IsNullOrEmpty(VersionModifier))
            {
                Version = new ImplementationVersion(Version + VersionModifier);
                VersionModifier = null;
            }

            // Default stability rating to testing
            if (Stability == Stability.Unset) Stability = Stability.Testing;

            // Make local paths absolute when possible
            string feedDir = null;
            try
            {
                if (Path.IsPathRooted(feedID)) feedDir = Path.GetDirectoryName(feedID);
            }
            catch (ArgumentException)
            {
                // Ignore non-filename IDs
            }
            if (!string.IsNullOrEmpty(feedDir))
            {
                if (!string.IsNullOrEmpty(LocalPath))
                    LocalPath = Path.GetFullPath(Path.Combine(feedDir, FileUtils.UnifySlashes(LocalPath)));
                else if (!string.IsNullOrEmpty(ID) && ID.StartsWith(".")) // Get local path from ID if missing
                    LocalPath = ID = Path.GetFullPath(Path.Combine(feedDir, FileUtils.UnifySlashes(ID)));
            }

            // Parse manifest digest from ID if missing
            if (!string.IsNullOrEmpty(ID)) ManifestDigest.ParseID(ID, ref _manifestDigest);
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
        /// Returns the implementation in the form "Implementation: Comma-seperated list of set values". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = "Implementation: " + ID + ", ";
            if (Architecture != default(Architecture)) result += Architecture + ", ";
            if (Version != null) result += Version + ", ";
            if (Released != default(DateTime)) result += Released.ToShortDateString() + ", ";
            if (Stability != default(Stability)) result += Stability + ", ";
            if (!string.IsNullOrEmpty(License)) result += License + ", ";
            if (Main != null) result += Main + ", ";

            // Remove last two characters
            return result.Substring(0, result.Length - 2);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(ImplementationBase other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.ID == ID && other.LocalPath == LocalPath && other.ManifestDigest == ManifestDigest;
        }

        /// <inheritdoc/>
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
