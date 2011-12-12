﻿/*
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
using System.IO;
using Common.Storage;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Creates <see cref="IFeedCache"/> instances.
    /// </summary>
    public static class FeedCacheProvider
    {
        private static readonly IFeedCache _feedCache = new DiskFeedCache(Locations.GetCacheDirPath("0install.net", "interfaces"));

        /// <summary>
        /// Creates an <see cref="IFeedCache"/> instance that uses the default cache location in the user profile.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static IFeedCache CreateDefault()
        {
            // Can share one instance globally, since it is thread-safe and the directory does not change at runtime
            // Recreate memory-caching layer for each call to prevent long-running caches from going out of sync with disk because of changes made by other processes
            return new MemoryFeedCache(_feedCache);
        }
    }
}
