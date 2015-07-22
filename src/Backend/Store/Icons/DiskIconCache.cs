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
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common.Net;
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
        /// <summary>
        /// The directory containing the cached <see cref="Icon"/>s.
        /// </summary>
        public string DirectoryPath { get; private set; }

        /// <summary>
        /// Creates a new disk-based cache.
        /// </summary>
        /// <param name="path">A fully qualified directory path.</param>
        public DiskIconCache([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            DirectoryPath = path;
        }

        //--------------------//

        private readonly object _lock = new object();

        /// <inheritdoc/>
        public bool Contains(Uri iconUrl)
        {
            #region Sanity checks
            if (iconUrl == null) throw new ArgumentNullException("iconUrl");
            #endregion

            string path = Path.Combine(DirectoryPath, new FeedUri(iconUrl).Escape());

            return File.Exists(path);
        }

        /// <inheritdoc/>
        public string GetIcon(Uri iconUrl, ITaskHandler handler)
        {
            #region Sanity checks
            if (iconUrl == null) throw new ArgumentNullException("iconUrl");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string path = Path.Combine(DirectoryPath, new FeedUri(iconUrl).Escape());

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

        /// <inheritdoc/>
        public void Remove(Uri iconUrl)
        {
            #region Sanity checks
            if (iconUrl == null) throw new ArgumentNullException("iconUrl");
            #endregion

            string path = Path.Combine(DirectoryPath, new FeedUri(iconUrl).Escape());

            lock (_lock)
            {
                File.Delete(path);
            }
        }
    }
}
