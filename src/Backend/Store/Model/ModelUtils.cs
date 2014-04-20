using System;
using System.IO;
using System.Web;
using NanoByte.Common.Utils;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Provides utiltity methods for interface and feed IDs and URIs.
    /// </summary>
    public static class ModelUtils
    {
        #region URI validation
        /// <summary>
        /// Determines whether an URI is a valid feed reference. Must be absolute and use the HTTP(S) protocol.
        /// </summary>
        /// <param name="value">The URI to check for validity.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is valid; <see langword="false"/> otherwise.</returns>
        public static bool IsValidUri(string value)
        {
            Uri result;
            return Uri.TryCreate(value, UriKind.Absolute, out result) && IsValidUri(result);
        }

        /// <summary>
        /// Determines whether an URI is a valid feed reference. Must be absolute and use the HTTP(S) protocol.
        /// </summary>
        /// <param name="value">The URI to check for validity.</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is valid; <see langword="false"/> otherwise.</returns>
        public static bool IsValidUri(Uri value)
        {
            return value != null && value.IsAbsoluteUri && (value.Scheme == Uri.UriSchemeHttp || value.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Parses an URI as a feed reference. Must be absolute and use the HTTP(S) protocol.
        /// </summary>
        /// <param name="value">The URI to check for validity.</param>
        /// <param name="result">The parsed URI. Only use this if the result was <see langword="true"/>!</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is valid; <see langword="false"/> otherwise.</returns>
        public static bool TryParseUri(string value, out Uri result)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out result) && IsValidUri(result);
        }

        /// <summary>
        /// Ensures that an interface ID is valid.
        /// </summary>
        /// <param name="value">The interface ID to check for validity.</param>
        /// <exception cref="InvalidInterfaceIDException">Thrown if <paramref name="value"/> is an invalid interface ID.</exception>
        public static void ValidateInterfaceID(string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            // Valid local paths are always ok
            try
            {
                if (Path.IsPathRooted(value)) return;
            }
            catch (ArgumentException)
            {
                return;
            }

            // URIs must be HTTP(S) and have a slash after the host name
            if (!value.StartsWith("http://") && !value.StartsWith("https://")) throw new InvalidInterfaceIDException(string.Format(Resources.InvalidInterfaceID, value));
            if (value.CountOccurences('/') < 3) throw new InvalidInterfaceIDException(string.Format(Resources.MissingSlashInUri, value));

            // Perform more in-depth URI validation
            if (!IsValidUri(value)) throw new InvalidInterfaceIDException(string.Format(Resources.InvalidInterfaceID, value));
        }
        #endregion

        #region URI escaping
        /// <summary>
        /// Escapes a value using URI encoding.
        /// </summary>
        public static string Escape(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            return HttpUtility.UrlEncode(value);
        }

        /// <summary>
        /// Unescapes a value using URI encoding.
        /// </summary>
        public static string Unescape(string escaped)
        {
            #region Sanity checks
            if (escaped == null) throw new ArgumentNullException("escaped");
            #endregion

            return HttpUtility.UrlDecode(escaped);
        }

        /// <summary>
        /// Escapes a value using URI encoding except for slashes (encoded as #) and colons (left as-is on POSIX systems).
        /// </summary>
        public static string PrettyEscape(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            string result = Escape(value);

            // Encode slash as #
            result = result.Replace("%2f", "#");

            // Do not encode : on Unixoid systems
            if (UnixUtils.IsUnix) result = result.Replace("%3a", ":");

            return result;
        }

        /// <summary>
        /// Unescapes a value using URI encoding except for slashes (encoded as #).
        /// </summary>
        public static string PrettyUnescape(string escaped)
        {
            #region Sanity checks
            if (escaped == null) throw new ArgumentNullException("escaped");
            #endregion

            // Do not encode : on Unixoid systems
            if (UnixUtils.IsUnix) escaped = escaped.Replace(":", "%3a");

            // Decode # as slash
            return Unescape(escaped.Replace("#", "%2f"));
        }
        #endregion

        #region ID comparison
        /// <summary>
        /// Compares to interface IDs and determines whether they are equivalent (using URI or file path comparison as appropriate).
        /// </summary>
        public static bool IDEquals(string idA, string idB)
        {
            Uri uriA, uriB;
            if (TryParseUri(idA, out uriA) && TryParseUri(idB, out uriB))
            { // Use URI comparison when possible
                return (uriA == uriB);
            }
            else if (WindowsUtils.IsWindows && Path.IsPathRooted(idA) && Path.IsPathRooted(idB))
            { // Use case-insensitive comparison for file paths on Windows
                return StringUtils.EqualsIgnoreCase(idA, idB);
            }
            else return idA == idB;
        }
        #endregion

        #region URI sanitization
        /// <summary>
        /// Reparses a URI (generated via conversion) to ensure it is a valid absolute URI.
        /// </summary>
        public static Uri Sanitize(this Uri uri)
        {
            return (uri == null) ? null : new Uri(uri.OriginalString, UriKind.Absolute);
        }
        #endregion
    }
}
