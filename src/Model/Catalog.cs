/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using Common.Storage;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Contains a list of <see cref="Feed"/>s, optionally reduced to only contain information relevant for listings.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlRoot("catalog", Namespace = XmlNamespace)]
    [XmlType("catalog", Namespace = XmlNamespace)]
    public class Catalog : XmlUnknown, ICloneable, IEquatable<Catalog>
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing feed catalogs. Used in combination with <see cref="Feed.XmlNamespace"/>.
        /// </summary>
        public const string XmlNamespace = "http://0install.de/schema/injector/catalog";
        #endregion

        #region Properties
        // Preserve order
        private readonly C5.ArrayList<Feed> _feeds = new C5.ArrayList<Feed>();

        /// <summary>
        /// A list of <see cref="Feed"/>s contained within this catalog.
        /// </summary>
        [XmlElement("interface", typeof(Feed), Namespace = Feed.XmlNamespace)]
        public C5.ArrayList<Feed> Feeds { get { return _feeds; } }
        #endregion

        #region Factory methods
        /// <summary>
        /// Merges the content of multiple <see cref="Catalog"/>s.
        /// </summary>
        /// <remarks>In case of duplicate <see cref="Feed.Uri"/>s only the first instance is kept.</remarks>
        public static Catalog Merge(IEnumerable<Catalog> catalogs)
        {
            #region Sanity checks
            if (catalogs == null) throw new ArgumentNullException("catalogs");
            #endregion

            var newCatalog = new Catalog();
            var feedUris = new C5.HashSet<Uri>(); // Hash interface URIs to detect duplicates quicker

            foreach (var catalog in catalogs)
            {
                foreach (var feed in catalog.Feeds)
                {
                    if (feedUris.Contains(feed.Uri)) continue; // Drop duplicates
                    newCatalog.Feeds.Add(feed);
                    feedUris.Add(feed.Uri);
                }
            }

            return newCatalog;
        }
        #endregion

        //--------------------//

        #region Query
        /// <summary>
        /// Returns the <see cref="Feed"/> with a specific URI.
        /// </summary>
        /// <param name="uri">The <see cref="Feed.Uri"/> to look for.</param>
        /// <returns>The identified <see cref="Feed"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no <see cref="Feed"/> matching <paramref name="uri"/> was found in <see cref="Feeds"/>.</exception>
        public Feed GetFeed(Uri uri)
        {
            foreach (var feed in Feeds)
                if (feed.Uri == uri) return feed;

            throw new KeyNotFoundException();
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads an <see cref="Catalog"/> from an XML file (catalog).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Catalog"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Catalog Load(string path)
        {
            return XmlStorage.Load<Catalog>(path);
        }

        /// <summary>
        /// Loads an <see cref="Catalog"/> from a stream containing an XML file (catalog).
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Catalog"/>.</returns>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Catalog Load(Stream stream)
        {
            return XmlStorage.Load<Catalog>(stream);
        }

        /// <summary>
        /// Saves this <see cref="Catalog"/> to an XML file (catalog).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            XmlStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="Catalog"/> to a stream as an XML file (catalog).
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            XmlStorage.Save(stream, this);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Catalog"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Catalog"/>.</returns>
        public Catalog Clone()
        {
            var catalog = new Catalog();
            foreach (var feed in Feeds) catalog.Feeds.Add(feed.Clone());

            return catalog;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Catalog other)
        {
            if (other == null) return false;

            return Feeds.SequencedEquals(other.Feeds);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Catalog) && Equals((Catalog)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Feeds.GetSequencedHashCode();
        }
        #endregion
    }
}
