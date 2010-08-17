/*
 * Copyright 2010 Simon E. Silva Lauinger
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

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// Contains methods for all feed editor's controls.
    /// </summary>
    public static class ControlHelpers
    {
        /// <summary>
        /// Checks if <paramref name="url"/> is a valid feed url (begins with http:// or https:// and has an URL format).
        /// Stores a created <see cref="Uri"/> object from <paramref name="url"/> in <paramref name="uri"/> if succeeded.
        /// </summary>
        /// <param name="url"><see cref="String"/> to check for a feed URL.</param>
        /// <param name="uri">Object to store the <paramref name="uri"/>.</param>
        /// <returns><see langword="true"/>, if <paramref name="url"/> is a valid feed URL.</returns>
        public static bool IsValidFeedUrl(string url, out Uri uri)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            return false;
        }

        /// <summary>
        /// Checks if <paramref name="url"/> is a valid feed URL (begins with http:// or https:// and has an uri format).
        /// </summary>
        /// <param name="url"><see cref="String"/> to check for a feed URL.</param>
        /// <returns><see langword="true"/>, if <paramref name="url"/> is a valid feed URL.</returns>
        public static bool IsValidFeedUrl(string url)
        {
            Uri uri;
            return IsValidFeedUrl(url, out uri);
        }

        /// <summary>
        /// Checks if <paramref name="prooveUrl"/> is a valid internet url and creates a new
        /// <see cref="Uri"/> object.
        /// </summary>
        /// <param name="prooveUrl">Url to check.</param>
        /// <param name="uri">New <see cref="Uri"/> object with <paramref name="prooveUrl"/>
        /// </param>
        /// <returns><see langword="true"/> if <paramref name="prooveUrl"/> is a valid internet
        /// url, else <see langword="false"/></returns>
        public static bool IsValidArchiveUrl(string prooveUrl, out Uri uri)
        {
            if (Uri.TryCreate(prooveUrl, UriKind.Absolute, out uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeFtp || uri.Scheme == Uri.UriSchemeHttps;
            }
            return false;
        }
    }
}
