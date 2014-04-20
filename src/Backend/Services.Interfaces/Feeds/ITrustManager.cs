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
        /// <param name="uri">The URI the feed or catalog file originally came from.</param>
        /// <param name="data">The data of the file.</param>
        /// <param name="mirrorUri">The URI or local file path the file was actually loaded from; <see langword="null"/> if it is identical to <paramref name="uri"/>.</param>
        /// <exception cref="SignatureException">Thrown if no trusted signature was found.</exception>
        ValidSignature CheckTrust(Uri uri, byte[] data, Uri mirrorUri = null);
    }
}
