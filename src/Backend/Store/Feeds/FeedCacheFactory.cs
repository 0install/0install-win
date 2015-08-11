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
using NanoByte.Common.Storage;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Store.Feeds
{
    /// <summary>
    /// Creates <see cref="IFeedCache"/> instances.
    /// </summary>
    public static class FeedCacheFactory
    {
        /// <summary>
        /// Creates an <see cref="IFeedCache"/> instance that uses the default cache location in the user profile.
        /// </summary>
        /// <param name="openPgp">Provides access to an encryption/signature system compatible with the OpenPGP standard.</param>
        /// <exception cref="IOException">A problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Creating a directory is not permitted.</exception>
        public static IFeedCache CreateDefault(IOpenPgp openPgp)
        {
            return new DiskFeedCache(Locations.GetCacheDirPath("0install.net", machineWide: false, resource: "interfaces"), openPgp);
        }
    }
}
