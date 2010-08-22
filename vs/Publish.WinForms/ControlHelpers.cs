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
using ZeroInstall.Model;

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
        /// Checks if <paramref name="prooveUrl"/> is a valid archive url (begins with http:// , https:// or ftp://).
        /// </summary>
        /// <param name="prooveUrl">Url to check.</param>
        /// <returns><see langword="true"/> if <paramref name="prooveUrl"/> is a valid archive
        /// url, else <see langword="false"/></returns>
        public static bool IsValidArchiveUrl(string prooveUrl)
        {
            Uri uri;
            return Uri.TryCreate(prooveUrl, UriKind.Absolute, out uri) && IsValidArchiveUrl(uri);
        }

        /// <summary>
        /// Checks if <paramref name="prooveUrl"/> is a valid archive url (begins with http:// , https:// or ftp://).
        /// </summary>
        /// <param name="prooveUrl">Url to check.</param>
        /// <returns><see langword="true"/> if <paramref name="prooveUrl"/> is a valid archive
        /// url, else <see langword="false"/></returns>
        public static bool IsValidArchiveUrl(Uri prooveUrl)
        {
            Uri uri;
            return IsValidFeedUrl(prooveUrl.AbsolutePath, out uri);
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
            if (prooveUrl == null) throw new ArgumentNullException("prooveUrl");

            if (Uri.TryCreate(prooveUrl, UriKind.Absolute, out uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeFtp || uri.Scheme == Uri.UriSchemeHttps;
            }
            return false;
        }

        /// <summary>
        /// Checks if the properties of a <see cref="Archive"/> have their default values.
        /// </summary>
        /// <param name="toCheck"><see cref="Archive"/> to check.</param>
        /// <returns>true, if Archive has default values.</returns>
        public static bool IsEmpty(Archive toCheck)
        {
            if (toCheck == null) throw new ArgumentNullException("toCheck");

            return (toCheck.Extract == default(String) && toCheck.Location == default(Uri) && toCheck.MimeType == default(String) && toCheck.Size == default(long) && toCheck.StartOffset == default(long));
        }

        /// <summary>
        /// Checks if at least one hash in two <see cref="ManifestDigest"/>s is equal.
        /// </summary>
        /// <param name="manifestDigest1"><see cref="ManifestDigest"/> to check.</param>
        /// <param name="manifestDigest2"><see cref="ManifestDigest"/> to check.</param>
        /// <returns><see langword="true"/>, if at least on hash is equal, <see langword="false"/> if at least one hash is not equal.</returns>
        public static bool CompareManifestDigests(ManifestDigest manifestDigest1, ManifestDigest manifestDigest2)
        {
            if (IsEmpty(manifestDigest1) || IsEmpty(manifestDigest2)) return false;
            if (!String.IsNullOrEmpty(manifestDigest1.Sha256) && !String.IsNullOrEmpty(manifestDigest2.Sha256))
            {
                if (manifestDigest1.Sha256 != manifestDigest2.Sha256) return false;
            }

            if (!String.IsNullOrEmpty(manifestDigest1.Sha1New) && !String.IsNullOrEmpty(manifestDigest2.Sha1New))
            {
                if (manifestDigest1.Sha1New != manifestDigest2.Sha1New) return false;
            }

            if (!String.IsNullOrEmpty(manifestDigest1.Sha1Old) && !String.IsNullOrEmpty(manifestDigest2.Sha1Old))
            {
                if (manifestDigest1.Sha1Old != manifestDigest2.Sha1Old) return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if no <see cref="ManifestDigest"/> hash is set.
        /// </summary>
        /// <param name="toCheck"><see cref="ManifestDigest"/> to check.</param>
        /// <returns><see langword="true"/>, if no hash was setted, else <see langword="false"/></returns>
        public static bool IsEmpty(ManifestDigest toCheck)
        {
            return String.IsNullOrEmpty(toCheck.Sha1New) && String.IsNullOrEmpty(toCheck.Sha1Old) &&
                   String.IsNullOrEmpty(toCheck.Sha256);
        }
    }
}
