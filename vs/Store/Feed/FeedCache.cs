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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Storage;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// Provides access to a disk-based cache of <see cref="Feed"/>s that were downloaded via HTTP(S).
    /// </summary>
    /// <remarks>
    ///   <para>Local feed files are not handled by this cache.</para>
    ///   <para>Once a feed has been added to this cache it is considered trusted (signature is not checked again).</para>
    /// </remarks>
    public class FeedCache
    {
        #region Properties
        /// <summary>
        /// The directory containing the cached <see cref="Feed"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new cache based on the given path to a cache directory.
        /// </summary>
        /// <param name="path">A fully qualified directory path.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="path"/> doesn't point to an existing directory.</exception>
        public FeedCache(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(string.Format(Resources.DirectoryNotFound, path));
            
            DirectoryPath = path;
        }

        /// <summary>
        /// Creates a new cache using the default path (generally in the user-profile).
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occured while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public FeedCache() : this(Locations.GetCachePath("0install.net", "interfaces"))
        {}
        #endregion

        //--------------------//
        
        #region Contains
        /// <summary>
        /// Determines whether this cache contains a local copy of a feed identified by a specific URL.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed. Must be an HTTP(s) URL and not a local file path.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified feed is available in this cache;
        ///   <see langword="false"/> if the specified feed is not available in this cache.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown if the requested <paramref name="feedUrl"/> is not a valid URL.</exception>
        public bool Contains(Uri feedUrl)
        {
            #region Sanity checks
            if (feedUrl == null) throw new ArgumentNullException("feedUrl");
            if (!Model.Feed.IsValidUrl(feedUrl)) throw new ArgumentException(Resources.InvalidUrl, "feedUrl");
            #endregion

            string escapedUrl = Uri.EscapeDataString(feedUrl.ToString());
            string path = Path.Combine(DirectoryPath, escapedUrl);

            return File.Exists(path);
        }
        #endregion

        #region List all
        /// <summary>
        /// Returns a list of all <see cref="Feed"/>s stored in this cache.
        /// </summary>
        /// <returns>
        /// A list of feed URLs (e.g. "http://somedomain.net/interface.xml") in C-sorted order (ordinal comparison, increasing).
        /// Usually these can also be considered interface URIs.
        /// This list will always reflect the current state in the filesystem and can not be modified!
        /// </returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        public IEnumerable<Uri> ListAll()
        {
            // Find all files whose names begin with an URL protocol
            string[] files = Directory.GetFiles(DirectoryPath, "http*");

            for (int i = 0; i < files.Length; i++)
            {
                // Take the file name itself and use URL encoding to get the original URL
                files[i] = Uri.UnescapeDataString(Path.GetFileName(files[i]) ?? "");
            }

            // Return as a C-sorted array
            Array.Sort(files, StringComparer.Ordinal);

            // Convert strings to URLs (with sanity checks)
            for (int i = 0; i < files.Length; i++)
            {
                Uri url;
                if (Model.Feed.IsValidUrl(files[i], out url)) yield return url;
            }
        }
        #endregion

        #region Get
        /// <summary>
        /// Gets a specific <see cref="Feed"/> from this cache.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed. Must be an HTTP(s) URL and not a local file path.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <exception cref="ArgumentException">Thrown if the requested <paramref name="feedUrl"/> is not a valid URL.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedUrl"/> was not found in the cache.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public Model.Feed Get(Uri feedUrl)
        {
            #region Sanity checks
            if (feedUrl == null) throw new ArgumentNullException("feedUrl");
            if (!Model.Feed.IsValidUrl(feedUrl)) throw new ArgumentException(Resources.InvalidUrl, "feedUrl");
            #endregion

            string path = Path.Combine(DirectoryPath, Uri.EscapeDataString(feedUrl.ToString()));

            if (!File.Exists(path)) throw new KeyNotFoundException(string.Format(Resources.FeedNotInCache, feedUrl));

            return Model.Feed.Load(path);
        }
        #endregion

        #region Get all
        /// <summary>
        /// Loads all <see cref="Feed"/>s currently in this cache.
        /// </summary>
        /// <returns>A list of <see cref="Feed"/>s  in C-sorted order (ordinal comparison, increasing).</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs disk IO, may take some time to process and always creates new objects")]
        public IEnumerable<Model.Feed> GetAll()
        {
            ICollection<Model.Feed> feeds = new LinkedList<Model.Feed>();
            foreach (Uri url in ListAll())
                feeds.Add(Get(url));
            return feeds;
        }
        #endregion

        #region Add
        /// <summary>
        /// Adds a new <see cref="Feed"/> file to the cache. Only do this after the feed source has been verified and trusted!
        /// </summary>
        /// <param name="path">The path of the file to be added.</param>
        /// <exception cref="ReplayAttackException">Thrown if the file to be added is older than a version already located in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public void Add(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            Model.Feed.Load(path);

            // ToDo: Implement)
            throw new NotImplementedException();
        }
        #endregion

        #region Remove
        /// <summary>
        /// Removes a specific <see cref="Feed"/> from this cache.
        /// </summary>
        /// <param name="feedUrl">The URL of the feed. Must be an HTTP(s) URL and not a local file path.</param>
        /// <exception cref="ArgumentException">Thrown if the requested <paramref name="feedUrl"/> is not a valid URL.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedUrl"/> was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if the feed could not be deleted because it was in use.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the cache is not permitted.</exception>
        public void Remove(Uri feedUrl)
        {
            #region Sanity checks
            if (feedUrl == null) throw new ArgumentNullException("feedUrl");
            if (!Model.Feed.IsValidUrl(feedUrl)) throw new ArgumentException(Resources.InvalidUrl, "feedUrl");
            #endregion

            string path = Path.Combine(DirectoryPath, Uri.EscapeDataString(feedUrl.ToString()));
            if (!File.Exists(path)) throw new KeyNotFoundException(string.Format(Resources.FeedNotInCache, feedUrl));
            File.Delete(path);
        }
        #endregion
    }
}
