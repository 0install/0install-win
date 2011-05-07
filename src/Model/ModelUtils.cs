using System;
using System.IO;
using System.Web;
using Common.Utils;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
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
            try { if (Path.IsPathRooted(value)) return; }
            catch (ArgumentException) { return; }

            // URIs must be HTTP(S) and have a slash after the host name
            if (!value.StartsWith("http:") && !value.StartsWith("https:")) throw new InvalidInterfaceIDException(string.Format(Resources.InvalidInterfaceID, value));
            if (StringUtils.CountOccurences(value, '/') < 3) throw new InvalidInterfaceIDException(string.Format(Resources.MissingSlashInUri, value));

            // Perform more in-depth URI validation
            Uri temp;
            if (!TryParseUri(value, out temp)) throw new InvalidInterfaceIDException(string.Format(Resources.InvalidInterfaceID, value));
        }
        #endregion

        #region Escaping
        /// <summary>
        /// Escapes a value using URL encoding.
        /// </summary>
        public static string Escape(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            return HttpUtility.UrlEncode(value);
        }

        /// <summary>
        /// Unescapes a value using URL encoding.
        /// </summary>
        public static string Unescape(string escaped)
        {
            #region Sanity checks
            if (escaped == null) throw new ArgumentNullException("escaped");
            #endregion

            return HttpUtility.UrlDecode(escaped);
        }

        /// <summary>
        /// Escapes a value using URL encoding except for slashes (encoded as #) and colons (left as-is on POSIX systems).
        /// </summary>
        public static string PrettyEscape(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            string result = Escape(value);

            // Encode slash as #
            result = result.Replace("%2f", "#");

            // Do not encode : on Unix-like systems
            if (MonoUtils.IsUnix) result = result.Replace("%3a", ":");

            return result;
        }

        /// <summary>
        /// Unescapes a value using URL encoding except for slashes (encoded as #).
        /// </summary>
        public static string PrettyUnescape(string escaped)
        {
            #region Sanity checks
            if (escaped == null) throw new ArgumentNullException("escaped");
            #endregion

            // Decode # as slash
            return Unescape(escaped.Replace("#", "%2f"));
        }
        #endregion

        #region IDs
        /// <summary>
        /// Converts and interface or feed ID it its canonical representation.
        /// </summary>
        /// <remarks>aliases are resolved to the IDs they represent, existing local paths are converted to absolute paths. Everything else stays unchanged.</remarks>
        public static string CanonicalID(string id)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            #endregion

            if (id.StartsWith("alias:"))
            {
                // ToDo: Handle alias lookup
                return id;
            }
            else if (File.Exists(id))
            { // Use absolute paths for local files
                return Path.GetFullPath(id);
            }
            else
            { // Assume a normal URI
                return id;
            }
        }
        #endregion
    }
}
