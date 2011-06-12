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
using Common.Net;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Provides access to a disk-based cache of <see cref="Icon"/>s that were downloaded via HTTP(S).
    /// </summary>
    public sealed class DiskIconCache : IIconCache
    {
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
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="path"/> does not point to an existing directory.</exception>
        public DiskIconCache(string path)
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
        public string GetIcon(Uri iconUrl, ITaskHandler handler)
        {
            #region Sanity checks
            if (iconUrl == null) throw new ArgumentNullException("iconUrl");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string path = Path.Combine(DirectoryPath, ModelUtils.Escape(iconUrl.ToString()));

            if (!File.Exists(path))
            {
                // Download missing icon
                handler.RunTask(new DownloadFile(iconUrl, path), null);
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

            if (!File.Exists(path)) throw new KeyNotFoundException(string.Format(Resources.IconNotInCache, iconUrl, path));
            File.Delete(path);
        }
        #endregion
    }
}
