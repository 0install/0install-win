/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Model.Design;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Retrieves an implementation by downloading and extracting an archive.
    /// </summary>
    [Description("Retrieves an implementation by downloading and extracting an archive.")]
    [Serializable, XmlRoot("archive", Namespace = Feed.XmlNamespace), XmlType("archive", Namespace = Feed.XmlNamespace)]
    public sealed class Archive : DownloadRetrievalMethod, IEquatable<Archive>
    {
        #region Constants
        /// <summary>
        /// A <see cref="MimeType"/> value for archives.
        /// </summary>
        public const string
            MimeTypeZip = "application/zip",
            MimeTypeTar = "application/x-tar",
            MimeTypeTarGzip = "application/x-compressed-tar",
            MimeTypeTarBzip = "application/x-bzip-compressed-tar",
            MimeTypeTarLzma = "application/x-lzma-compressed-tar",
            MimeTypeTarXz = "application/x-xz-compressed-tar",
            MimeTypeRubyGem = "application/x-ruby-gem",
            MimeType7Z = "application/x-7z-compressed",
            MimeTypeCab = "application/vnd.ms-cab-compressed",
            MimeTypeMsi = "application/x-msi",
            MimeTypeDeb = "application/x-deb",
            MimeTypeRpm = "application/x-rpm",
            MimeTypeDmg = "application/x-apple-diskimage";

        /// <summary>
        /// All known <see cref="MimeType"/> values for archives.
        /// </summary>
        public static readonly IEnumerable<string> KnownMimeTypes = new [] {MimeTypeZip, MimeTypeTar, MimeTypeTarGzip, MimeTypeTarBzip, MimeTypeTarLzma, MimeTypeTarXz, MimeTypeRubyGem, MimeType7Z, MimeTypeCab, MimeTypeMsi, MimeTypeDeb, MimeTypeRpm, MimeTypeDmg};

        /// <summary>
        /// Tries to guess the MIME type of an archive file by looking at its file extension.
        /// </summary>
        /// <param name="fileName">The file name to analyze.</param>
        /// <returns>The MIME type if it could be guessed; <c>null</c> otherwise.</returns>
        /// <remarks>The file's content is not analyzed.</remarks>
        [CanBeNull]
        public static string GuessMimeType([NotNull] string fileName)
        {
            #region Sanity checks
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            #endregion

            if (fileName.EndsWithIgnoreCase(".zip")) return MimeTypeZip;
            if (fileName.EndsWithIgnoreCase(".tar")) return MimeTypeTar;
            if (fileName.EndsWithIgnoreCase(".tar.gz") || fileName.EndsWithIgnoreCase(".tgz")) return MimeTypeTarGzip;
            if (fileName.EndsWithIgnoreCase(".tar.bz2") || fileName.EndsWithIgnoreCase(".tbz2") || fileName.EndsWithIgnoreCase(".tbz")) return MimeTypeTarBzip;
            if (fileName.EndsWithIgnoreCase(".tar.lzma") || fileName.EndsWithIgnoreCase(".tlzma")) return MimeTypeTarLzma;
            if (fileName.EndsWithIgnoreCase(".tar.xz") || fileName.EndsWithIgnoreCase(".txz")) return MimeTypeTarXz;
            if (fileName.EndsWithIgnoreCase(".gem")) return MimeTypeRubyGem;
            if (fileName.EndsWithIgnoreCase(".7z")) return MimeType7Z;
            if (fileName.EndsWithIgnoreCase(".cab")) return MimeTypeCab;
            if (fileName.EndsWithIgnoreCase(".msi")) return MimeTypeMsi;
            if (fileName.EndsWithIgnoreCase(".deb")) return MimeTypeDeb;
            if (fileName.EndsWithIgnoreCase(".rpm")) return MimeTypeRpm;
            if (fileName.EndsWithIgnoreCase(".dmg")) return MimeTypeDmg;
            return null;
        }

        /// <summary>
        /// Gets the default file extension for a particular archive MIME type.
        /// </summary>
        /// <param name="mimeType">The MIME type to get the extension for.</param>
        /// <returns>The file extension including the leading dot, e.g. '.zip'.</returns>
        /// <exception cref="NotSupportedException">The <paramref name="mimeType"/> is not in the list of <see cref="KnownMimeTypes"/>.</exception>
        [NotNull]
        public static string GetDefaultExtension([NotNull] string mimeType)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException(nameof(mimeType));
            #endregion

            switch (mimeType)
            {
                case MimeTypeZip:
                    return ".zip";
                case MimeTypeTar:
                    return ".tar";
                case MimeTypeTarGzip:
                    return ".tar.gz";
                case MimeTypeTarBzip:
                    return ".tar.bz2";
                case MimeTypeTarLzma:
                    return ".tar.lzma";
                case MimeTypeRubyGem:
                    return ".gem";
                case MimeType7Z:
                    return ".7z";
                case MimeTypeCab:
                    return ".cab";
                case MimeTypeMsi:
                    return ".msi";
                default:
                    throw new NotSupportedException(string.Format(Resources.UnsupportedArchiveMimeType, mimeType));
            }
        }
        #endregion

        /// <summary>
        /// The type of the archive as a MIME type. If missing, the type is guessed from the extension on the <see cref="DownloadRetrievalMethod.Href"/> attribute. This value is case-insensitive.
        /// </summary>
        [Description("The type of the archive as a MIME type. If missing, the type is guessed from the extension on the location attribute. This value is case-insensitive.")]
        [TypeConverter(typeof(ArchiveMimeTypeConverter))]
        [XmlAttribute("type"), DefaultValue(""), CanBeNull]
        public string MimeType { get; set; }

        /// <summary>
        /// The number of bytes at the beginning of the file which should be ignored. The value in <see cref="DownloadRetrievalMethod.Size"/> does not include the skipped bytes.
        /// </summary>
        /// <remarks>This is useful for some self-extracting archives which are made up of a shell script followed by a normal archive in a single file.</remarks>
        [Description("The number of bytes at the beginning of the file which should be ignored. The value in the size attribute does not include the skipped bytes.")]
        [XmlAttribute("start-offset"), DefaultValue(0L)]
        public long StartOffset { get; set; }

        /// <inheritdoc/>
        public override long DownloadSize => Size + StartOffset;

        /// <summary>
        /// The name of the subdirectory in the archive to extract; <c>null</c> or <see cref="string.Empty"/> for entire archive.
        /// </summary>
        [Description("The name of the subdirectory in the archive to extract; null for entire archive.")]
        [XmlAttribute("extract"), DefaultValue(""), CanBeNull]
        public string Extract { get; set; }

        /// <summary>
        /// The subdirectory within the implementation directory to extract this archive to; can be <c>null</c>.
        /// </summary>
        [Description("The subdirectory within the implementation directory to extract this archive to; can be null.")]
        [XmlAttribute("dest"), CanBeNull]
        public string Destination { get; set; }

        #region Normalize
        /// <inheritdoc/>
        public override void Normalize(FeedUri feedUri)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException(nameof(feedUri));
            #endregion

            base.Normalize(feedUri);

            // If the MIME type is already set or the location is missing, we have nothing to do here
            if (!string.IsNullOrEmpty(MimeType) || Href == null) return;

            // Guess the MIME type based on the file extension
            MimeType = GuessMimeType(Href.OriginalString);
        }

        protected override string XmlTagName => "archive";
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "Location (MimeType, Size + StartOffset, Extract) => Destination". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = $"{Href} ({MimeType}, {Size} + {StartOffset}, {Extract})";
            if (!string.IsNullOrEmpty(Destination)) result += " => " + Destination;
            return result;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Archive"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Archive"/>.</returns>
        private Archive CloneArchive() => new Archive {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Href = Href, Size = Size, MimeType = MimeType, StartOffset = StartOffset, Extract = Extract, Destination = Destination};

        /// <summary>
        /// Creates a deep copy of this <see cref="Archive"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Archive"/>.</returns>
        public override IRecipeStep CloneRecipeStep() => CloneArchive();

        /// <summary>
        /// Creates a deep copy of this <see cref="Archive"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Archive"/>.</returns>
        public override RetrievalMethod Clone() => CloneArchive();
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Archive other)
        {
            if (other == null) return false;
            return base.Equals(other) && StringUtils.EqualsIgnoreCase(other.MimeType, MimeType) && other.StartOffset == StartOffset && other.Extract == Extract && other.Destination == Destination;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is Archive && Equals((Archive)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (MimeType != null) result = (result * 397) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(MimeType);
                result = (result * 397) ^ StartOffset.GetHashCode();
                result = (result * 397) ^ Extract?.GetHashCode() ?? 0;
                result = (result * 397) ^ Destination?.GetHashCode() ?? 0;
                return result;
            }
        }
        #endregion
    }
}
