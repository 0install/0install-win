/*
 * Copyright 2010-2011 Bastian Eicher, Roland Leopold Walkling
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
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Fetchers
{
    /// <summary>
    /// Downloads <see cref="Implementation"/>s, extracts them and adds them to the <see cref="Store"/>.
    /// </summary>
    public interface IFetcher
    {
        /// <summary>
        /// The location to store the downloaded and unpacked <see cref="Model.Implementation"/>s in.
        /// </summary>
        IStore Store { get; set; }

        /// <summary>
        /// Starts executing a request. <see cref="Join"/> must be called to complete the process!
        /// </summary>
        /// <param name="fetchRequest">The request to be executed.</param>
        void Start(FetchRequest fetchRequest);

        /// <summary>
        /// Blocks until the execution of a request started by <see cref="Start"/> is complete.
        /// </summary>
        /// <param name="fetchRequest">The request to wait for.</param>
        /// <exception cref="OperationCanceledException">Thrown if a download or IO task was canceled from another thread.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">Thrown if a file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to <see cref="Fetcher.Store"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown an <see cref="Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        void Join(FetchRequest fetchRequest);

        /// <summary>
        /// Cancels the execution of a request started by <see cref="Start"/>.
        /// </summary>
        /// <param name="fetchRequest">The request to be canceled.</param>
        /// <remarks>Multiple calls or calls for non-running <see cref="FetchRequest"/>s have no effect.</remarks>
        void Cancel(FetchRequest fetchRequest);
    }
}
