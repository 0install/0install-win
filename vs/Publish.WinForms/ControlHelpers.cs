using System;

namespace ZeroInstall.Publish.WinForms
{
    /// <summary>
    /// Contains methods for all feed editor's controls.
    /// </summary>
    static class ControlHelpers
    {
        /// <summary>
        /// Checks if <paramref name="url"/> is a valid feed url (begins with http or https and has an uri format).
        /// Creates an <see cref="Uri"/> Object with <paramref name="url"/> if succeeded.
        /// </summary>
        /// <param name="url"><see cref="String"/> to check for a feed url.</param>
        /// <param name="uri">Object to store the <paramref name="uri"/>.</param>
        /// <returns><see langword="true"/>, if <paramref name="url"/> is a valid feed url.</returns>
        public static bool IsValidFeedUrl(string url, out Uri uri)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            return false;
        }

        /// <summary>
        /// Checks if <paramref name="url"/> is a valid feed url (begins with http or https and has an uri format).
        /// </summary>
        /// <param name="url"><see cref="String"/> to check for a feed url.</param>
        /// <returns><see langword="true"/>, if <paramref name="url"/> is a valid feed url.</returns>
        public static bool IsValidFeedUrl(string url)
        {
            Uri uri;
            return IsValidFeedUrl(url, out uri);
        }

    }
}
