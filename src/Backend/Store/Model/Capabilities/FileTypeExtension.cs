/*
 * Copyright 2010-2014 Bastian Eicher
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

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// A specific file extension used to identify a file type.
    /// </summary>
    [Description("A specific file extension used to identify a file type.")]
    [Serializable]
    [XmlRoot("extension", Namespace = CapabilityList.XmlNamespace), XmlType("extension", Namespace = CapabilityList.XmlNamespace)]
    public class FileTypeExtension : XmlUnknown, ICloneable, IEquatable<FileTypeExtension>
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

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="FileTypeExtension"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="FileTypeExtension"/>.</returns>
        public FileTypeExtension Clone()
        {
            return new FileTypeExtension {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, Value = Value, MimeType = MimeType, PerceivedType = PerceivedType};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FileTypeExtension other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Value == Value && other.MimeType == MimeType && other.PerceivedType == PerceivedType;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FileTypeExtension && Equals((FileTypeExtension)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Value ?? "").GetHashCode();
                result = (result * 397) ^ (MimeType ?? "").GetHashCode();
                result = (result * 397) ^ (PerceivedType ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
