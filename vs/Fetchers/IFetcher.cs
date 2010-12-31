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
using System.Net;
using Common;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Manages one or more <see cref="FetchRequest"/>s and keeps clients informed of the progress. Files are downloaded and added to <see cref="Store"/> automatically.
    /// </summary>
    public interface IFetcher
    {
        /// <summary>
        /// The location to store the downloaded and unpacked <see cref="Model.Implementation"/>s in.
        /// </summary>
        IStore Store { get; }

        /// <summary>
        /// Execute a complete request and block until it is done.
        /// </summary>
        /// <param name="fetchRequest">The download request to be executed.</param>
        /// <param name="handler">A callback object used when the the user needs to be informed about progress.</param>
        /// <exception cref="UserCancelException">Thrown if a download or IO task was cancelled from another thread.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to <see cref="Fetcher.Store"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        /// <exception cref="FetcherException"></exception>
        void RunSync(FetchRequest fetchRequest, IFetchHandler handler);
    }
}
