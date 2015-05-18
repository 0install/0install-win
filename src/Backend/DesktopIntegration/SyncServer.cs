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
using System.Net;
using NanoByte.Common.Net;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains access information for a Sync server used by <see cref="SyncIntegrationManager"/>.
    /// </summary>
    public struct SyncServer
    {
        private Uri _uri;

        /// <summary>
        /// The base URI of the sync server. Automatically ensures the URI ends with a slash (/).
        /// </summary>
        public Uri Uri { get { return _uri; } set { _uri = value.EnsureTrailingSlash(); } }

        /// <summary>
        /// The username to authenticate with against the server at <see cref="Uri"/>.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password to authenticate with against the server at <see cref="Uri"/>.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Combines <see cref="Username"/> and <see cref="Password"/>.
        /// </summary>
        public NetworkCredential Credentials { get { return new NetworkCredential(Username, Password); } }
    }
}
