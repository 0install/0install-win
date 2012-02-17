/*
 * Copyright 2010-2012 Bastian Eicher
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
using Common.Compression;
using Common.Utils;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents an archive to be downloaded and extracted.
    /// </summary>
    [Serializable]
    [XmlType("archive", Namespace = Feed.XmlNamespace)]
    public sealed class Archive : RecipeStep, IEquatable<Archive>
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
        [XmlAttribute("href"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LocationString { get { return (Location == null ? null : Location.ToString()); } set { Location = (value == null ? null : new Uri(value)); } }

        /// <summary>
        /// The type of the archive as a MIME type. If missing, the type is guessed from the extension on the <see cref="Location"/> attribute. This value is case-insensitive.
        /// </summary>
        [Description("The type of the archive as a MIME type. If missing, the type is guessed from the extension on the location attribute. This value is case-insensitive.")]
        [XmlAttribute("type"), DefaultValue("")]
        public string MimeType { get; set; }

        /// <summary>
        /// The size of the archive in bytes. The archive must have the given size or it will be rejected.
        /// </summary>
        [Description("The size of the archive in bytes. The archive must have the given size or it will be rejected.")]
        [XmlAttribute("size"), DefaultValue(0L)]
        public long Size { get; set; }

        /// <summary>
        /// The number of bytes at the beginning of the file which should be ignored. The value in <see cref="Size"/> does not include the skipped bytes. 
        /// </summary>
        /// <remarks>This is useful for some self-extracting archives which are made up of a shell script followed by a normal archive in a single file.</remarks>
        [Description("The number of bytes at the beginning of the file which should be ignored. The value in the size attribute does not include the skipped bytes.")]
        [XmlAttribute("start-offset"), DefaultValue(0L)]
        public long StartOffset { get; set; }

        /// <summary>
        /// The name of the subdirectory in the archive to extract; <see langword="null"/> or <see cref="string.Empty"/> for entire archive.
        /// </summary>
        [Description("The name of the subdirectory in the archive to extract; null for entire archive.")]
        [XmlAttribute("extract"), DefaultValue("")]
        public string Extract { get; set; }
        #endregion

        //--------------------//

        #region Normalize
        /// <summary>
        /// Guesses missing default values.
        /// </summary>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing.
        /// It should not be called if you plan on serializing the feed again since it will may loose some of its structure.</remarks>
        public override void Normalize()
        {
            // If the MIME type is already set or the location is missing, we have nothing to do here
            if (!string.IsNullOrEmpty(MimeType) || string.IsNullOrEmpty(LocationString)) return;

            // Guess the MIME type based on the file extension
            MimeType = ArchiveUtils.GuessMimeType(LocationString);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the archive in the form "Archive: Location (MimeType, Size + StartOffset) => Extract". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = string.Format("Archive: {0} ({1}, {2} + {3})", Location, MimeType, Size, StartOffset);
            if (!string.IsNullOrEmpty(Extract)) result += " => " + Extract;
            return result;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Archive"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Archive"/>.</returns>
        public override RecipeStep CloneRecipeStep()
        {
            return new Archive {Location = Location, MimeType = MimeType, Size = Size, StartOffset = StartOffset, Extract = Extract};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Archive other)
        {
            if (other == null) return false;

            return other.Location == Location && other.Size == Size && other.Extract == Extract && StringUtils.Compare(other.MimeType, MimeType) && other.StartOffset == StartOffset;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Archive && Equals((Archive)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Location != null ? Location.GetHashCode() : 0);
                result = (result * 397) ^ Size.GetHashCode();
                result = (result * 397) ^ (Extract ?? "").GetHashCode();
                result = (result * 397) ^ (MimeType ?? "").ToLowerInvariant().GetHashCode();
                result = (result * 397) ^ StartOffset.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
