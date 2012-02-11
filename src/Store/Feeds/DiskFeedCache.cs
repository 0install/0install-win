﻿/*
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
using System.IO;
using System.Security.Cryptography;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

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
        #region Properties
        /// <summary>
        /// The directory containing the cached <see cref="Feed"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new disk-based cache.
        /// </summary>
        /// <param name="path">A fully qualified directory path.</param>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="path"/> does not point to an existing directory.</exception>
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
            #endregion

            try
            {
                ModelUtils.ValidateInterfaceID(feedID);
            }
            catch (InvalidInterfaceIDException)
            {
                return false;
            }

            return File.Exists(Path.Combine(DirectoryPath, ModelUtils.Escape(feedID))) ||
                // Too long file paths may have been contracted using a hash of the feed ID
                File.Exists(Path.Combine(DirectoryPath, StringUtils.Hash(feedID, SHA256.Create()))) ||
                    // Local files are passed through directly
                    File.Exists(feedID);
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
                if (ModelUtils.IsValidUri(uri)) result.Add(uri);
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

            var feed = Feed.Load(GetPath(feedID));
            feed.Simplify();
            return feed;
        }

        /// <inheritdoc/>
        public IEnumerable<OpenPgpSignature> GetSignatures(string feedID, IOpenPgp openPgp)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            ModelUtils.ValidateInterfaceID(feedID);
            if (openPgp == null) throw new ArgumentNullException("openPgp");
            #endregion

            return FeedUtils.GetSignatures(openPgp, File.ReadAllBytes(GetPath(feedID)));
        }

        /// <summary>
        /// Determines the file path used to store a feed with a particular ID.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if the requested <paramref name="feedID"/> was not found in the cache.</exception>
        private string GetPath(string feedID)
        {
            if (ModelUtils.IsValidUri(feedID))
            {
                string path = Path.Combine(DirectoryPath, ModelUtils.Escape(feedID));
                if (File.Exists(path)) return path;
                else
                {
                    // Too long file paths may have been contracted using a hash of the feed ID
                    string altPath = Path.Combine(DirectoryPath, StringUtils.Hash(feedID, SHA256.Create()));
                    if (File.Exists(altPath)) return altPath;

                    throw new KeyNotFoundException(string.Format(Resources.FeedNotInCache, feedID, path));
                }
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
            ModelUtils.ValidateInterfaceID(feedID);
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            try
            {
                WriteToFile(data, Path.Combine(DirectoryPath, ModelUtils.Escape(feedID)));
            }
            catch (PathTooLongException)
            {
                // Contract too long file paths using a hash of the feed ID
                WriteToFile(data, Path.Combine(DirectoryPath, StringUtils.Hash(feedID, SHA256.Create())));
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
            // Prepend random string for temp file name
            string tempPath = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + "temp." + Path.GetRandomFileName() + "." + Path.GetFileName(path);

            try
            {
                // Write to temporary file first
                File.WriteAllBytes(tempPath, data);
                lock (_replaceLock) // Prevent race-conditions when adding the same feed twice
                    FileUtils.Replace(tempPath, path);
            }
                #region Error handling
            catch
            {
                // Clean up failed transactions
                if (File.Exists(tempPath)) File.Delete(tempPath);

                throw;
            }
            #endregion
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

            // Delete from both regular and contracted path
            try
            {
                File.Delete(Path.Combine(DirectoryPath, ModelUtils.Escape(feedID)));
            }
            catch (PathTooLongException)
            {}
            try
            {
                File.Delete(Path.Combine(DirectoryPath, StringUtils.Hash(feedID, SHA256.Create())));
            }
            catch (PathTooLongException)
            {}
        }
        #endregion
    }
}
