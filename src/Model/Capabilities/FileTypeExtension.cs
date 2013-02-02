/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Describes a specific file extension.
    /// </summary>
    [XmlType("extension", Namespace = CapabilityList.XmlNamespace)]
    public struct FileTypeExtension : IEquatable<FileTypeExtension>
    {
        #region Constants
        /// <summary>
        /// Canonical <see cref="PerceivedType"/>.
        /// </summary>
        public const string TypeFolder = "folder", TypeText = "text", TypeImage = "image", TypeAudio = "audio", TypeVideo = "video", TypeCompressed = "compressed", TypeDocument = "document", TypeSystem = "system", TypeApplication = "application", TypeGameMedia = "gamemedia", TypeContacts = "contacts";
        #endregion

        #region Properties
        /// <summary>
        /// The file extension including the leading dot (e.g. ".png").
        /// </summary>
        [Description("The file extension including the leading dot (e.g. \".png\").")]
        [XmlAttribute("value")]
        public string Value { get; set; }

        /// <summary>
        /// The MIME type associated with the file extension.
        /// </summary>
        [Description("The MIME type associated with the file extension.")]
        [XmlAttribute("mime-type"), DefaultValue("")]
        public string MimeType { get; set; }

        /// <summary>
        /// Defines the broad category of file types (e.g. text, image, audio) this extension falls into. Should always be a canonical type.
        /// </summary>
        [Description("Defines the broad category of file (e.g. text, image, audio) types this extension falls into. Should always be a canonical type.")]
        [XmlAttribute("perceived-type"), DefaultValue("")]
        public string PerceivedType { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the extension in the form "Value (MimeType)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Value, MimeType);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FileTypeExtension other)
        {
            return other.Value == Value && other.MimeType == MimeType && other.PerceivedType == PerceivedType;
        }

        /// <inheritdoc/>
        public static bool operator ==(FileTypeExtension left, FileTypeExtension right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(FileTypeExtension left, FileTypeExtension right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FileTypeExtension && Equals((FileTypeExtension)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Value ?? "").GetHashCode();
                result = (result * 397) ^ (MimeType ?? "").GetHashCode();
                result = (result * 397) ^ (PerceivedType ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
