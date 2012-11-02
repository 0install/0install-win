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
using System.IO;
using Common.Storage;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Creates <see cref="IIconCache"/> instances.
    /// </summary>
    public static class IconCacheProvider
    {
        private static readonly object _lock = new object();

        private static volatile IIconCache _iconCache;

        /// <summary>
        /// Creates an <see cref="IIconCache"/> instance that uses the default cache location in the user profile.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static IIconCache CreateDefault()
        {
            // Share one instance globally to prevent race-conditions
            // Thread-safe singleton with double-check
            if (_iconCache == null)
            {
                lock (_lock)
                {
                    if (_iconCache == null)
                        _iconCache = new DiskIconCache(Locations.GetCacheDirPath("0install.net", "interface_icons"));
                }
            }
            return _iconCache;
        }
    }
}
