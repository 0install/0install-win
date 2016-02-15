/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Globalization;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Information for identifying an implementation of a <see cref="Feed"/>.
    /// Common base for <see cref="Implementation"/> and <see cref="ImplementationSelection"/>.
    /// </summary>
    [XmlType("implementation-base", Namespace = Feed.XmlNamespace)]
    public abstract class ImplementationBase : Element
    {
        /// <summary>
        /// A unique identifier for this implementation. Used when storing implementation-specific user preferences.
        /// </summary>
        [Category("Identity"), Description("A unique identifier for this implementation. Used when storing implementation-specific user preferences.")]
        [XmlAttribute("id"), DefaultValue("")]
        public string ID { get; set; }

        /// <summary>
        /// If the feed file is a local file (the interface 'uri' starts with /) then the local-path attribute may contain the pathname of a local directory (either an absolute path or a path relative to the directory containing the feed file).
        /// </summary>
        [Category("Identity"), Description("If the feed file is a local file (the interface 'uri' starts with /) then the local-path attribute may contain the pathname of a local directory (either an absolute path or a path relative to the directory containing the feed file).")]
        [XmlAttribute("local-path"), DefaultValue("")]
        public string LocalPath { get; set; }

        private ManifestDigest _manifestDigest;

        /// <summary>
        /// A manifest digest is a means of uniquely identifying an <see cref="Implementation"/> and verifying its contents.
        /// </summary>
        [Category("Identity"), Description("A manifest digest is a means of uniquely identifying an Implementation and verifying its contents.")]
        [XmlElement("manifest-digest")]
        public ManifestDigest ManifestDigest { get { return _manifestDigest; } set { _manifestDigest = value; } }

        #region Normalize
        /// <summary>
        /// Sets missing default values and handles legacy elements.
        /// </summary>
        /// <param name="feedUri">The feed the data was originally loaded from.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing. Do not call it if you plan on serializing the feed again since it may loose some of its structure.</remarks>
        public override void Normalize(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            base.Normalize(feedUri);

            // Merge the version modifier into the normal version attribute
            if (!string.IsNullOrEmpty(VersionModifier))
            {
                Version = new ImplementationVersion(Version + VersionModifier);
                VersionModifier = null;
            }

            // Default stability rating to testing
            if (Stability == Stability.Unset) Stability = Stability.Testing;

            // Make local paths absolute
            try
            {
                if (!string.IsNullOrEmpty(LocalPath))
                    LocalPath = ModelUtils.GetAbsolutePath(LocalPath, feedUri);
                else if (!string.IsNullOrEmpty(ID) && ID.StartsWith(".")) // Get local path from ID
                    LocalPath = ID = ModelUtils.GetAbsolutePath(ID, feedUri);
            }
                #region Error handling
            catch (UriFormatException ex)
            {
                Log.Error(ex);
                LocalPath = null;
            }
            #endregion

            // Parse manifest digest from ID if missing
            if (!string.IsNullOrEmpty(ID)) _manifestDigest.ParseID(ID);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Copies all known values from one instance to another. Helper method for instance cloning.
        /// </summary>
        protected static void CloneFromTo([NotNull] ImplementationBase from, [NotNull] ImplementationBase to)
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
        /// Returns the implementation in the form "Comma-seperated list of set values". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(ID)) parts.Add(ID);
            if (Architecture != default(Architecture)) parts.Add(Architecture.ToString());
            if (Version != null) parts.Add(Version.ToString());
            if (Released != default(DateTime)) parts.Add(Released.ToString("d", CultureInfo.InvariantCulture));
            if (ReleasedVerbatim != null) parts.Add(ReleasedVerbatim);
            if (Stability != default(Stability)) parts.Add(Stability.ToString());
            if (!string.IsNullOrEmpty(License)) parts.Add(License);
            if (!string.IsNullOrEmpty(Main)) parts.Add(Main);
            return StringUtils.Join(", ", parts);
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
