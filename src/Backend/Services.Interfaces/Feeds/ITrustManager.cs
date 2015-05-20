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
using ZeroInstall.Store;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Methods for verifying signatures and user trust.
    /// </summary>
    public interface ITrustManager
    {
        /// <summary>
        /// Checks whether a remote feed or catalog file has a a valid and trusted signature. Downloads missing GPG keys for verification and interactivley asks the user to approve new keys.
        /// </summary>
        /// <param name="data">The data of the file.</param>
        /// <param name="uri">The URI the feed or catalog file originally came from.</param>
        /// <param name="mirrorUrl">The URL or local file path the file was fetched from; <see langword="null"/> if it is identical to <paramref name="uri"/>.</param>
        /// <exception cref="SignatureException">No trusted signature was found.</exception>
        /// <exception cref="IOException">The OpenPGP implementation could not be launched.</exception>
        /// <exception cref="UriFormatException"><paramref name="uri"/> is a local file.</exception>
        [NotNull]
        ValidSignature CheckTrust([NotNull] byte[] data, [NotNull] FeedUri uri, [CanBeNull] FeedUri mirrorUrl = null);
    }
}
