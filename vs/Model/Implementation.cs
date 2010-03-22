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

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A specific (executable) implementation of an <see cref="Interface"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be dispoed.")]
    public sealed class Implementation : ImplementationBase
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
        [XmlAttribute("local-path")]
        public string LocalPath { get; set; }

        /// <summary>
        /// Digests of the .manifest file using various hashing algorithms.
        /// </summary>
        [Category("Identity"), Description("Digests of the .manifest file using various hashing algorithms.")]
        [XmlElement("manifest-digest")]
        public ManifestDigest ManifestDigest { get; set; }

        #region Retrieval methods
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Archive> _archives = new C5.HashedArrayList<Archive>();
        /// <summary>
        /// A list of <see cref="Archive"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of archives as retrieval methods.")]
        [XmlElement("archive")]
        // Don't use ICollection<T> interface to make XML Serialization work
        public C5.HashedArrayList<Archive> Archives { get { return _archives; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Recipe> _recipes = new C5.HashedArrayList<Recipe>();
        /// <summary>
        /// A list of <see cref="Recipe"/>s as <see cref="RetrievalMethod"/>s.
        /// </summary>
        [Category("Retrieval"), Description("A list of recipes as retrieval methods.")]
        [XmlElement("recipe")]
        // Don't use ICollection<T> interface to make XML Serialization work
        public C5.HashedArrayList<Recipe> Recipes { get { return _recipes; } }
        #endregion

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
            // Transfer the version modifier to the normal version attribute
            if (!string.IsNullOrEmpty(VersionModifier))
            {
                Version += VersionModifier;
                VersionModifier = null;
            }

            // Default stability rating to testing
            if (Stability == Stability.Unset) Stability = Stability.Testing;

            // Check if stuff may be read from the ID
            if (string.IsNullOrEmpty(ID)) return;

            const string sha1Prefix = "sha1=";
            const string sha1NewPrefix = "sha1new=";
            const string sha256Prefix = "sha256=";

            // Fill in values (only if missing) using legacy entries (indentified by prefixes)
            var manifestDigest = ManifestDigest;
            if (string.IsNullOrEmpty(LocalPath) && (ID.StartsWith(".") || ID.StartsWith("/"))) LocalPath = ID;
            else if (string.IsNullOrEmpty(manifestDigest.Sha1) && ID.StartsWith(sha1Prefix)) manifestDigest.Sha1 = ID.Substring(sha1Prefix.Length);
            else if (string.IsNullOrEmpty(manifestDigest.Sha1New) && ID.StartsWith(sha1NewPrefix)) manifestDigest.Sha1New = ID.Substring(sha1NewPrefix.Length);
            else if (string.IsNullOrEmpty(manifestDigest.Sha256) && ID.StartsWith(sha256Prefix)) manifestDigest.Sha256 = ID.Substring(sha256Prefix.Length);
            ManifestDigest = manifestDigest;

            // Simplify retrieval methods
            foreach (var archive in Archives) archive.Simplify();
            foreach (var recipe in Recipes) recipe.Simplify();
        }
        #endregion

        //--------------------//

        // ToDo: Implement ToString, Equals and Clone
    }
}
