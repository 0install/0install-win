/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.IO;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to a disk-based cache of <see cref="Model.Feed"/>s that were downloaded via HTTP(S).
    /// </summary>
    /// <remarks>
    ///   <para>Local feed files are simply passed through this cache.</para>
    ///   <para>Once a feed has been added to this cache it is considered trusted (signature is not checked again).</para>
    /// </remarks>
    public sealed class DiskFeedCache : IFeedCache
    {
        #region Properties
        /// <summary>
        /// The directory containing the cached <see cref="Model.Feed"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new cache based on the given path to a cache directory.
        /// </summary>
        /// <param name="path">A fully qualified directory path.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="path"/> doesn't point to an existing directory.</exception>
        public DiskFeedCache(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(string.Format(Resources.DirectoryNotFound, path));
            
            DirectoryPath = path;
        }
        #endregion

        //--------------------//
        
        #region Contains
        /// <inheritdoc/>
        public bool Contains(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            ModelUtils.ValidateInterfaceID(feedID);
            #endregion

            // Local files are passed through directly
            return File.Exists(feedID) || File.Exists(Path.Combine(DirectoryPath, ModelUtils.Escape(feedID)));
        }
        #endregion

        #region List all
        /// <inheritdoc/>
        public IEnumerable<string> ListAll()
        {
            // Find all files whose names begin with an URL protocol
            string[] files = Directory.GetFiles(DirectoryPath, "http*");

            var result = new List<string>(files.Length);

            for (int i = 0; i < files.Length; i++)
            {
                // Take the file name itself and use URL encoding to get the original URL
                string uri = ModelUtils.Unescape(Path.GetFileName(files[i]) ?? "");
                Uri temp;
                if (ModelUtils.TryParseUri(uri, out temp)) result.Add(uri);
            }

            // Return as a C-sorted list
            result.Sort(StringComparer.Ordinal);
            return result;
        }
        #endregion

        #region Get
        /// <inheritdoc/>
        public Feed GetFeed(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            ModelUtils.ValidateInterfaceID(feedID);
            #endregion

            // Local files are passed through directly
            string path = File.Exists(feedID) ? feedID : Path.Combine(DirectoryPath, ModelUtils.Escape(feedID));

            if (!File.Exists(path)) throw new KeyNotFoundException(string.Format(Resources.FeedNotInCache, feedID, path));
            
            var feed = Feed.Load(path);
            feed.Simplify();
            return feed;
        }
        #endregion

        #region Add
        /// <inheritdoc/>
        public void Add(string feedID, string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            ModelUtils.ValidateInterfaceID(feedID);
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            // Don't cache local files
            if (feedID == path) return;

            string targetPath = Path.Combine(DirectoryPath, ModelUtils.Escape(feedID));

            // Detect replay attacks
            var oldTime = File.GetLastWriteTimeUtc(targetPath);
            var newTime = File.GetLastWriteTimeUtc(path);
            if (oldTime > newTime)
                throw new ReplayAttackException(string.Format(Resources.ReplayAttack, feedID, oldTime, newTime));

            File.Copy(path, targetPath);
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public void Remove(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            ModelUtils.ValidateInterfaceID(feedID);
            #endregion

            string path = Path.Combine(DirectoryPath, ModelUtils.Escape(feedID));

            if (!File.Exists(path)) throw new KeyNotFoundException(string.Format(Resources.FeedNotInCache, feedID, path));
            File.Delete(path);
        }
        #endregion
    }
}
