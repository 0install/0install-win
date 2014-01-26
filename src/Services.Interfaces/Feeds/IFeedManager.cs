/*
 * Copyright 2010-2014 Bastian Eicher
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
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Feed"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public interface IFeedManager
    {
        /// <summary>
        /// Set to <see langword="true"/> to re-download <see cref="Feed"/>s even if they are already in the <see cref="IFeedCache"/>.
        /// </summary>
        bool Refresh { get; set; }

        /// <summary>
        /// Is set to <see langword="true"/> if any <see cref="Feed"/> returned by <see cref="GetFeed"/> is getting stale and should be updated by setting <see cref="Refresh"/> to <see langword="true"/>.
        /// </summary>
        /// <remarks><see cref="Config.Freshness"/> controls the time span after which a feed is considered stale. The check is only performed when <see cref="Config.NetworkUse"/> is set to <see cref="NetworkLevel.Full"/>.</remarks>
        bool Stale { get; set; }

        /// <summary>
        /// Returns a specific <see cref="Feed"/>. Automatically handles downloading and caching. Updates the <see cref="Stale"/> indicator.
        /// </summary>
        /// <param name="feedID">The canonical ID used to identify the feed.</param>
        /// <returns>The parsed <see cref="Feed"/> object.</returns>
        /// <remarks><see cref="Feed"/>s are always served from the <see cref="IFeedCache"/> if possible, unless <see cref="Refresh"/> is set to <see langword="true"/>.</remarks>
        /// <exception cref="OperationCanceledException">Thrown if the user canceled the process.</exception>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="feedID"/> is an invalid interface ID.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while fetching the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the cache is not permitted.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data of a remote feed file could not be verified.</exception>
        Feed GetFeed(string feedID);

        /// <summary>
        /// Imports a remote <see cref="Feed"/> into the <see cref="IFeedCache"/> after verifying its signature.
        /// </summary>
        /// <param name="uri">The URI the feed originally came from.</param>
        /// <param name="data">The data of the feed.</param>
        /// <param name="mirrorUri">The URI or local file path the feed was actually loaded from; <see langword="null"/> if it is identical to <paramref name="uri"/>.</param>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="data"/> list the same URI as <paramref name="uri"/>.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if access to the feed file or the cache is not permitted.</exception>
        /// <exception cref="SignatureException">Thrown if the signature data of the feed file could not be handled or if no signatures were trusted.</exception>
        void ImportFeed(Uri uri, byte[] data, Uri mirrorUri = null);
    }
}
