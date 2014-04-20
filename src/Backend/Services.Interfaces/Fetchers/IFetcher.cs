/*
 * Copyright 2010-2014 Bastian Eicher, Roland Leopold Walkling
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
using System.Net;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Services.Fetchers
{
    /// <summary>
    /// Downloads <see cref="Implementation"/>s, extracts them and adds them to an <see cref="IStore"/>.
    /// </summary>
    /// <remarks>This is an application of the strategy pattern.</remarks>
    public interface IFetcher
    {
        /// <summary>
        /// Downloads a set of <see cref="Implementation"/>s to the <see cref="Store"/> and returns once this process is complete.
        /// </summary>
        /// <param name="implementations">The <see cref="Store.Model.Implementation"/>s to be downloaded.</param>
        /// <exception cref="OperationCanceledException">Thrown if a download or IO task was canceled from another thread.</exception>
        /// <exception cref="WebException">Thrown if a file could not be downloaded from the internet.</exception>
        /// <exception cref="NotSupportedException">Thrown if a file format, protocal, etc. is unknown or not supported.</exception>
        /// <exception cref="IOException">Thrown if a downloaded file could not be written to the disk or extracted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to <see cref="IStore"/> is not permitted.</exception>
        /// <exception cref="DigestMismatchException">Thrown an <see cref="Store.Model.Implementation"/>'s <see cref="Archive"/>s don't match the associated <see cref="ManifestDigest"/>.</exception>
        void Fetch(IEnumerable<Implementation> implementations);
    }
}
