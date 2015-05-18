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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Values.Design;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store
{
    /// <summary>
    /// Represents a feed or interface URI or local path. Unlike <see cref="System.Uri"/> this class only accepts HTTP(S) URLs and absolute local paths.
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(StringConstructorConverter<FeedUri>))]
    public sealed class FeedUri : Uri, IEquatable<FeedUri>, ISerializable
    {
        #region Constants
        /// <summary>
        /// This is prepended to a <see cref="FeedUri"/> if it is meant for demo data and should not be used to actually fetch a feed.
        /// </summary>
        /// <seealso cref="IsFake"/>
        public const string FakePrefix = "fake:";

        /// <summary>
        /// This is prepended to <see cref="ImplementationSelection.FromFeed"/> if data was pulled from a native package manager rather than the feed itself.
        /// </summary>
        /// <seealso cref="PackageImplementation"/>
        /// <seealso cref="IsFromDistribution"/>
        public const string FromDistributionPrefix = "distribution:";
        #endregion

        #region Properties
        /// <summary>
        /// Indicates whether this is a fake identifier meant for demo data and should not be used to actually fetch a feed.
        /// </summary>
        /// <seealso cref="FakePrefix"/>
        public bool IsFake { get; private set; }

        /// <summary>
        /// Indicates that an <see cref="ImplementationSelection"/> was generated with data from a native package manager rather than the feed itself.
        /// </summary>
        /// <seealso cref="FromDistributionPrefix"/>
        public bool IsFromDistribution { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a feed URI from a string.
        /// </summary>
        /// <param name="value">A string to parse as an HTTP(S) URL or an absolute local path.</param>
        /// <exception cref="UriFormatException"><paramref name="value"/> is not a valid HTTP(S) URL or an absolute local path.</exception>
        [SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads")]
        public FeedUri([NotNull, Localizable(false)] string value) : base(TrimPrefix(value), UriKind.Absolute)
        {
            if (string.IsNullOrEmpty(value)) throw new UriFormatException();

            IsFake = value.StartsWith(FakePrefix);
            IsFromDistribution = value.StartsWith(FromDistributionPrefix);

            switch (Scheme)
            {
                case "http":
                case "https":
                case "file":
                    break;

                default:
                    throw new UriFormatException(string.Format(Resources.InvalidFeedUri, this));
            }
        }

        private static string TrimPrefix(string value)
        {
            if (value.StartsWith(FakePrefix)) return value.Substring(FakePrefix.Length);
            else if (value.StartsWith(FromDistributionPrefix)) return value.Substring(FromDistributionPrefix.Length);
            else return value;
        }

        /// <summary>
        /// Creates a feed URI from an existing <see cref="Uri"/>.
        /// </summary>
        /// <param name="value">An existing <see cref="Uri"/>.</param>
        /// <exception cref="UriFormatException"><paramref name="value"/> is not a valid HTTP(S) URL or an absolute local path.</exception>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public FeedUri([NotNull] Uri value) : this(value.OriginalString)
        {}

        /// <summary>
        /// Passing a <see cref="FeedUri"/> instance into the <see cref="FeedUri"/> constructor does nothing useful. Just use the original object.
        /// </summary>
        [Obsolete("Passing a FeedUri instance into the FeedUri constructor does nothing useful. Just use the original object.")]
        public FeedUri(FeedUri value) : this((Uri)value)
        {}
        #endregion

        #region Serialization
        private FeedUri(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
            IsFake = serializationInfo.GetBoolean("IsFake");
            IsFromDistribution = serializationInfo.GetBoolean("IsFromDistribution");
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            #region Sanity checks
            if (serializationInfo == null) throw new ArgumentNullException("serializationInfo");
            #endregion

            GetObjectData(serializationInfo, streamingContext);
            serializationInfo.AddValue("IsFake", IsFake);
            serializationInfo.AddValue("IsFromDistribution", IsFromDistribution);
        }
        #endregion

        //--------------------//

        #region Escaping
        /// <summary>
        /// Escapes an identifier using URL encoding.
        /// </summary>
        [Pure, NotNull]
        public static string Escape([NotNull] string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            var builder = new StringBuilder();
            foreach (char t in value)
            {
                switch (t)
                {
                    case '-':
                    case '_':
                    case '.':
                        builder.Append(t);
                        break;

                    default:
                        if (char.IsLetterOrDigit(t))
                            builder.Append(t);
                        else
                        {
                            builder.Append('%');
                            builder.Append(((int)t).ToString("x"));
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Escapes the identifier using URL encoding.
        /// </summary>
        [NotNull]
        public new string Escape()
        {
            return Escape(AbsoluteUri);
        }

        /// <summary>
        /// Unescapes an identifier using URL encoding.
        /// </summary>
        /// <exception cref="UriFormatException">The unescaped string is not a valid HTTP(S) URL or an absolute local path.</exception>
        [NotNull]
        public new static FeedUri Unescape([NotNull] string escaped)
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
            return new FeedUri(builder.ToString());
        }

        /// <summary>
        /// Escapes an identifier using URL encoding except for slashes (encoded as #) and colons (left as-is on POSIX systems).
        /// </summary>
        [Pure, NotNull]
        public static string PrettyEscape([NotNull] string value)
        {
            #region Sanity checks
            if (value == null) throw new ArgumentNullException("value");
            #endregion

            var builder = new StringBuilder();
            foreach (char t in value)
            {
                switch (t)
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
                        builder.Append(t);
                        break;

                    default:
                        if (char.IsLetterOrDigit(t))
                            builder.Append(t);
                        else
                        {
                            builder.Append('%');
                            builder.Append(((int)t).ToString("x"));
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Escapes the identifier using URL encoding except for slashes (encoded as #) and colons (left as-is on POSIX systems).
        /// </summary>
        [NotNull]
        public string PrettyEscape()
        {
            return PrettyEscape(AbsoluteUri);
        }

        /// <summary>
        /// Unescapes an identifier using URL encoding except for slashes (encoded as #).
        /// </summary>
        [Pure, NotNull]
        public static FeedUri PrettyUnescape([NotNull] string escaped)
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
            return new FeedUri(builder.ToString());
        }

        /// <summary>
        /// Escape troublesome characters. The result is a valid file leaf name (i.e. does not contain / etc).
        /// Letters, digits, '-', '.', and characters > 127 are copied unmodified.
        /// '/' becomes '__'. Other characters become '_code_', where code is the
        /// lowercase hex value of the character in Unicode.
        /// </summary>
        private static string UnderscoreEscape(string value)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '/':
                    case '\\':
                        builder.Append("__");
                        break;

                    case '.':

                        //Avoid creating hidden files, or specials (. and ..)
                        builder.Append(i == 0 ? "_2e_" : ".");
                        break;

                    default:
                        // Top-bit-set chars don't cause any trouble
                        if (value[i] > 127 || char.IsLetterOrDigit(value[i]))
                            builder.Append(value[i]);
                        else
                        {
                            builder.Append('_');
                            builder.Append(((int)value[i]).ToString("x"));
                            builder.Append('_');
                        }
                        break;
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Convert the identifier to a list of path components.
        /// e.g. "http://example.com/foo.xml" becomes ["http", "example.com", "foo.xml"], while
        /// "/root/feed.xml" becomes ["file", "root__feed.xml"].
        /// The number of components is determined by the scheme (three for http, two for file).
        /// Uses [underscore_escape] to escape each component.
        /// </summary>
        public string[] EscapeComponent()
        {
            switch (Scheme)
            {
                case "http":
                case "https":
                    return new[] {"http", UnderscoreEscape(Host), UnderscoreEscape(LocalPath.Substring(1))};

                case "file":
                    return new[] {"file", UnderscoreEscape(WindowsUtils.IsWindows ? LocalPath : LocalPath.Substring(1))};

                default:
                    // Should never reach this
                    throw new InvalidOperationException();
            }
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns a string representation of the URI, not adhering to the escaping rules of RFC 2396.
        /// Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = IsFile ? LocalPath : base.ToString();
            if (IsFake) return FakePrefix + result;
            else if (IsFromDistribution) return FromDistributionPrefix + result;
            else return result;
        }

        /// <summary>
        /// An alternate version of <see cref="ToString"/> that produces results escaped according to RFC 2396.
        /// Safe for parsing!
        /// </summary>
        public string ToStringRfc()
        {
            string result = IsFile ? LocalPath : AbsoluteUri;
            if (IsFake) return FakePrefix + result;
            else if (IsFromDistribution) return FromDistributionPrefix + result;
            else return result;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FeedUri other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (IsFake != other.IsFake || IsFromDistribution != other.IsFromDistribution) return false;

            return (WindowsUtils.IsWindows && IsFile && other.IsFile)
                // File names on Windows are case-insensitive
                ? StringUtils.EqualsIgnoreCase(LocalPath, other.LocalPath)
                : base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FeedUri && Equals((FeedUri)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (WindowsUtils.IsWindows && IsFile)
                // File names on Windows are case-insensitive
                ? StringComparer.OrdinalIgnoreCase.GetHashCode(LocalPath)
                : base.GetHashCode();
        }

        /// <inheritdoc/>
        public static bool operator ==(FeedUri left, FeedUri right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(FeedUri left, FeedUri right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
