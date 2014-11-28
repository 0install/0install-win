using System;
using System.Globalization;
using System.IO;
using System.Text;
using NanoByte.Common;
using NanoByte.Common.Native;
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
        /// <exception cref="InvalidInterfaceIDException"><paramref name="value"/> is an invalid interface ID.</exception>
        public static void ValidateInterfaceID(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new InvalidInterfaceIDException(new ArgumentNullException("value").Message);

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

        #region URL escaping
        /// <summary>
        /// Escapes a value using URL encoding.
        /// </summary>
        public static string Escape(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            var builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '-':
                    case '_':
                    case '.':
                        builder.Append(value[i]);
                        break;

                    default:
                        if (char.IsLetterOrDigit(value[i]))
                            builder.Append(value[i]);
                        else
                        {
                            builder.Append('%');
                            builder.Append(((int)value[i]).ToString("x"));
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Escapes a value using URL encoding except for slashes (encoded as #) and colons (left as-is on POSIX systems).
        /// </summary>
        public static string PrettyEscape(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            var builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '/':
                        builder.Append('#');
                        break;

                    case ':':
                        if (UnixUtils.IsUnix) builder.Append(':');
                        else builder.Append("%3a");
                        break;

                    case '-':
                    case '_':
                    case '.':
                        builder.Append(value[i]);
                        break;

                    default:
                        if (char.IsLetterOrDigit(value[i]))
                            builder.Append(value[i]);
                        else
                        {
                            builder.Append('%');
                            builder.Append(((int)value[i]).ToString("x"));
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Unescapes a value using URI encoding.
        /// </summary>
        public static string Unescape(string escaped)
        {
            #region Sanity checks
            if (escaped == null) throw new ArgumentNullException("escaped");
            #endregion

            var builder = new StringBuilder();
            for (int i = 0; i < escaped.Length; i++)
            {
                switch (escaped[i])
                {
                    case '%':
                        if (escaped.Length > i + 2)
                        {
                            builder.Append((char)int.Parse(escaped.Substring(i + 1, 2), NumberStyles.HexNumber));
                            i += 2;
                        }
                        break;

                    default:
                        builder.Append(escaped[i]);
                        break;
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Unescapes a value using URI encoding except for slashes (encoded as #).
        /// </summary>
        public static string PrettyUnescape(string escaped)
        {
            #region Sanity checks
            if (escaped == null) throw new ArgumentNullException("escaped");
            #endregion

            var builder = new StringBuilder();
            for (int i = 0; i < escaped.Length; i++)
            {
                switch (escaped[i])
                {
                    case '#':
                        builder.Append('/');
                        break;

                    case '%':
                        if (escaped.Length > i + 2)
                        {
                            builder.Append((char)int.Parse(escaped.Substring(i + 1, 2), NumberStyles.HexNumber));
                            i += 2;
                        }
                        break;

                    default:
                        builder.Append(escaped[i]);
                        break;
                }
            }
            return builder.ToString();
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

        #region Templates
        /// <summary>
        /// Determines whether a string contains a template variable (a substring enclosed in curly brackets, e.g {var}).
        /// </summary>
        public static bool ContainsTemplateVariables(string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            int openingBracket = value.IndexOf('{');
            if (openingBracket == -1) return false;
            return (value.IndexOf('}', openingBracket) != -1);
        }
        #endregion
    }
}
