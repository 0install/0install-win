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
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Icons
{
    /// <summary>
    /// Provides access to a disk-based cache of icon files that were downloaded via HTTP(S).
    /// </summary>
    public sealed class DiskIconCache : IIconCache
    {
        #region Variables
        private readonly object _lock = new object();
        #endregion

        #region Properties
        /// <summary>
        /// The directory containing the cached <see cref="Icon"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new disk-based cache.
        /// </summary>
        /// <param name="path">A fully qualified directory path.</param>
        public DiskIconCache(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            DirectoryPath = path;
        }
        #endregion

        //--------------------//

        #region Contains
        /// <inheritdoc/>
        public bool Contains(Uri iconUrl)
        {
            #region Sanity checks
            if (iconUrl == null) throw new ArgumentNullException("iconUrl");
            #endregion

            string path = Path.Combine(DirectoryPath, ModelUtils.Escape(iconUrl.ToString()));

            return File.Exists(path);
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
        public string GetIcon(Uri iconUrl, ITaskHandler handler)
        {
            #region Sanity checks
            if (iconUrl == null) throw new ArgumentNullException("iconUrl");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string path = Path.Combine(DirectoryPath, ModelUtils.Escape(iconUrl.ToString()));

            // Prevent file-exists race conditions
            lock (_lock)
            {
                // Download missing icons
                if (!File.Exists(path))
                {
                    using (var atomic = new AtomicWrite(path))
                    {
                        handler.RunTask(new DownloadFile(iconUrl, atomic.WritePath));
                        atomic.Commit();
                    }
                }
            }

            return path;
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public void Remove(Uri iconUrl)
        {
            #region Sanity checks
            if (iconUrl == null) throw new ArgumentNullException("iconUrl");
            #endregion

            string path = Path.Combine(DirectoryPath, ModelUtils.Escape(iconUrl.ToString()));

            lock (_lock)
            {
                File.Delete(path);
            }
        }
        #endregion
    }
}
