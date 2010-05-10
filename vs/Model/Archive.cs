/*
 * Copyright 2010 Bastian Eicher
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
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents an archive to be downloaded and extracted.
    /// </summary>
    public sealed class Archive : RetrievalStep, IEquatable<Archive>, ICloneable
    {
        #region Properties
        /// <summary>
        /// The URL used to locate the archive.
        /// </summary>
        [Description("The URL used to locate the archive.")]
        [XmlIgnore]
        public Uri Location { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Location"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("href"), Browsable(false)]
        public String LocationString
        {
            get { return (Location == null ? null : Location.ToString()); }
            set { Location = (value == null ? null : new Uri(value)); }
        }

        /// <summary>
        /// The type of the archive as a MIME type in the type attribute. If missing, the type is guessed from the extension on the <see cref="Location"/> attribute.
        /// </summary>
        [Description("The type of the archive as a MIME type in the type attribute. If missing, the type is guessed from the extension on the location attribute.")]
        [XmlAttribute("type")]
        public string MimeType { get; set; }

        /// <summary>
        /// The size of the archive in bytes. The archive must have the given size or it will be rejected.
        /// </summary>
        [Description("The size of the archive in bytes. The archive must have the given size or it will be rejected.")]
        [XmlAttribute("size")]
        public long Size { get; set; }

        /// <summary>
        /// The number of bytes at the beginning of the file which should be ignored. The value in the size attribute does not include the skipped bytes. 
        /// </summary>
        /// <remarks>This is useful for some self-extracting archives which are made up of a shell script followed by a normal archive in a single file.</remarks>
        [Description("The number of bytes at the beginning of the file which should be ignored. The value in the size attribute does not include the skipped bytes.")]
        [XmlAttribute("start-offset"), DefaultValue(0L)]
        public long StartOffset { get; set; }

        /// <summary>
        /// The name of the subdirectory in the archive to extract; <see langword="null"/> for entire archive.
        /// </summary>
        [Description("The name of the subdirectory in the archive to extract; null for entire archive.")]
        [XmlAttribute("extract")]
        public string Extract { get; set; }
        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Guesses missing default values.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Interface"/> again since it will may some of its structure.</remarks>
        public override void Simplify()
        {
            // If the MIME type is already set or the location is missing, we have nothing to do here
            if (!string.IsNullOrEmpty(MimeType) || string.IsNullOrEmpty(LocationString)) return;

            // Guess the MIME type based on the file ending
            if (LocationString.EndsWith(".zip")) MimeType = "application/x-zip";
            else if (LocationString.EndsWith(".cab")) MimeType = "application/vnd.ms-cab-compressed";
            else if (LocationString.EndsWith(".tar")) MimeType = "application/x-tar";
            else if (LocationString.EndsWith(".tar.gz") || LocationString.EndsWith(".tgz")) MimeType = "application/x-compressed-tar";
            else if (LocationString.EndsWith(".tar.bz2")) MimeType = "application/x-bzip-compressed-tar";
            else if (LocationString.EndsWith(".tar.lzma")) MimeType = "application/x-lzma-compressed-tar";
            else if (LocationString.EndsWith(".deb")) MimeType = "application/x-deb";
            else if (LocationString.EndsWith(".rpm")) MimeType = "application/x-rpm";
            else if (LocationString.EndsWith(".dmg")) MimeType = "application/x-apple-diskimage";
        }
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return string.Format("{0} ({1}, {2}) + {3} => {4}", Location, MimeType, Size, StartOffset, Extract);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a copy of this <see cref="Archive"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Archive"/>.</returns>
        public Archive CloneArchive()
        {
            return new Archive {Location = Location, MimeType = MimeType, Size = Size, StartOffset = StartOffset, Extract = Extract};
        }

        public object Clone()
        {
            return CloneArchive();
        }
        #endregion

        #region Equality
        public bool Equals(Archive other)
        {
            if (ReferenceEquals(null, other)) return false;

            return other.Location == Location && other.Size == Size && other.Extract == Extract && other.MimeType == MimeType && other.StartOffset == StartOffset;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Archive) && Equals((Archive)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Location != null ? Location.GetHashCode() : 0);
                result = (result * 397) ^ Size.GetHashCode();
                result = (result * 397) ^ (Extract != null ? Extract.GetHashCode() : 0);
                result = (result * 397) ^ (MimeType != null ? MimeType.GetHashCode() : 0);
                result = (result * 397) ^ StartOffset.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
