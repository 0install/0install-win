/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using System.Security.Cryptography;
using NanoByte.Common;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to a disk-based cache of <see cref="Feed"/>s that were downloaded via HTTP(S).
    /// </summary>
    /// <remarks>
    ///   <para>Local feed files are simply passed through this cache.</para>
    ///   <para>Once a feed has been added to this cache it is considered trusted (signatures are not checked again).</para>
    /// </remarks>
    public sealed class DiskFeedCache : IFeedCache
    {
        #region Dependencies
        private readonly IOpenPgp _openPgp;

        /// <summary>
        /// Creates a new disk-based cache.
        /// </summary>
        /// <param name="path">A fully qualified directory path.</param>
        /// <param name="openPgp">Provides access to an encryption/signature system compatible with the OpenPGP standard.</param>
        public DiskFeedCache(string path, IOpenPgp openPgp)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            DirectoryPath = path;
            _openPgp = openPgp;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The directory containing the cached <see cref="Feed"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }
        #endregion

        //--------------------//

        #region Contains
        /// <inheritdoc/>
        public bool Contains(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            #endregion

            try
            {
                ModelUtils.ValidateInterfaceID(feedID);
            }
            catch (InvalidInterfaceIDException)
            {
                return false;
            }

            return FileUtils.ExistsCaseSensitive(Path.Combine(DirectoryPath, ModelUtils.Escape(feedID))) ||
                   // Local files are passed through directly
                   File.Exists(feedID);
        }
        #endregion

        #region List all
        /// <inheritdoc/>
        public IEnumerable<string> ListAll()
        {
            if (!Directory.Exists(DirectoryPath)) return Enumerable.Empty<string>();

            // Find all files whose names begin with an URL protocol
            return Directory.GetFiles(DirectoryPath, "http*")
                // Take the file name itself and use URL encoding to get the original URL
                .Select(path => ModelUtils.Unescape(Path.GetFileName(path) ?? ""))
                // Filter out temporary/junk files
                .Where(ModelUtils.IsValidUri).ToList();
        }
        #endregion

        #region Get
        /// <inheritdoc/>
        public Feed GetFeed(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            #endregion

            ModelUtils.ValidateInterfaceID(feedID);
            var feed = XmlStorage.LoadXml<Feed>(GetPath(feedID));
            feed.Normalize(feedID);
            return feed;
        }

        /// <inheritdoc/>
        public IEnumerable<OpenPgpSignature> GetSignatures(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            #endregion

            ModelUtils.ValidateInterfaceID(feedID);
            return FeedUtils.GetSignatures(_openPgp, File.ReadAllBytes(GetPath(feedID)));
        }

        /// <summary>
        /// Determines the file path used to store a feed with a particular ID.
        /// </summary>
        /// <exception cref="KeyNotFoundException">The requested <paramref name="feedID"/> was not found in the cache.</exception>
        private string GetPath(string feedID)
        {
            if (ModelUtils.IsValidUri(feedID))
            {
                string fileName = ModelUtils.Escape(feedID);
                string path = Path.Combine(DirectoryPath, fileName);
                if (FileUtils.ExistsCaseSensitive(path)) return path;
                else throw new KeyNotFoundException(string.Format(Resources.FeedNotInCache, feedID, path));
            }
            else
            { // Assume invalid URIs are local paths
                return feedID;
            }
        }
        #endregion

        #region Add
        /// <inheritdoc/>
        public void Add(string feedID, byte[] data)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            ModelUtils.ValidateInterfaceID(feedID);
            if (!Directory.Exists(DirectoryPath)) Directory.CreateDirectory(DirectoryPath);

            try
            {
                WriteToFile(data, Path.Combine(DirectoryPath, ModelUtils.Escape(feedID)));
            }
            catch (PathTooLongException)
            {
                // Contract too long file paths using a hash of the feed ID
                WriteToFile(data, Path.Combine(DirectoryPath, feedID.Hash(SHA256.Create())));
            }
        }

        private readonly object _replaceLock = new object();

        /// <summary>
        /// Writes the entire content of a byte array to file atomically.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="path">The file to write to.</param>
        private void WriteToFile(byte[] data, string path)
        {
            lock (_replaceLock)
                using (var atomic = new AtomicWrite(path))
                {
                    File.WriteAllBytes(atomic.WritePath, data);
                    atomic.Commit();
                }
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public void Remove(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            #endregion

            ModelUtils.ValidateInterfaceID(feedID);
            File.Delete(GetPath(feedID));
        }
        #endregion

        #region Flush
        // No in-memory cache
        void IFeedCache.Flush()
        {}
        #endregion
    }
}
