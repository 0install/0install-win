/*
 * Copyright 2010 Bastian Eicher, Roland Leopold Walkling
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
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Provides access to <see cref="IFetcher"/> implementations.
    /// </summary>
    public static class FetcherProvider
    {
        private static readonly IFetcher _default = new Fetcher(StoreProvider.Default);
        /// <summary>
        /// Returns an implementation of <see cref="IStore"/> that uses the default cache locations.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        public static IFetcher Default { get { return _default; } }
    }
}
