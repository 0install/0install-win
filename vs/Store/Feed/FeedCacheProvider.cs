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
using System.IO;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// Provides access to <see cref="IFeedCache"/> implementations.
    /// </summary>
    public static class FeedCacheProvider
    {
        private static readonly IFeedCache _default = new DiskFeedCache();
        /// <summary>
        /// Returns an implementation of <see cref="IFeedCache"/> that uses the default cache location.
        /// </summary>
        /// <exception cref="IOException">Thrown if a problem occured while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static IFeedCache Default { get { return _default; } }
    }
}
