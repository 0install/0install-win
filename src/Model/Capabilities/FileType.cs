/*
 * Copyright 2010-2011 Bastian Eicher
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

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's ability to handle a certain file type.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 types only need to be disposed when using snapshots")]
    public class FileType : Capability, IEquatable<FileType>
    {
        #region Properties
        /// <summary>
        /// A human-readable description such as "PNG image file".
        /// </summary>
        [Description("A human-readable description such as \"PNG image file\".")]
        [XmlAttribute("description")]
        public string Description { get; set; }

        /// <summary>
        /// The programmatic identifier used by Windows to identify this file type.
        /// </summary>
        [Description("The programmatic identifier used by Windows to identify this file type.")]
        [XmlAttribute("prog-id")]
        public string ProgID { get; set; }

        /// <summary>
        /// The icon to use in file managers when displaying files of this type.
        /// </summary>
        [Description("The icon to use in file managers when displaying files of this type.")]
        [XmlElement("icon", Namespace = Feed.XmlNamespace)]
        public Icon Icon { get; set; }

        // Preserve order, duplicate string entries are not allowed
        private readonly C5.HashedArrayList<FileTypeExtension> _extensions = new C5.HashedArrayList<FileTypeExtension>();
        /// <summary>
        /// A list of all file extensions associated with this file type.
        /// </summary>
        [Description("A list of all file extensions associated with this file type.")]
        [XmlElement("extension")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.HashedArrayList<FileTypeExtension> Extensions { get { return _extensions; } }

        // Preserve order, duplicate string entries are not allowed
        private readonly C5.HashedArrayList<FileTypeVerb> _verbs = new C5.HashedArrayList<FileTypeVerb>();
        /// <summary>
        /// A list of all operations available for this file type.
        /// </summary>
        [Description("A list of all operations available for this file type.")]
        [XmlElement("verb")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.HashedArrayList<FileTypeVerb> Verbs { get { return _verbs; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "FileType". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("FileType");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability CloneCapability()
        {
            var capability = new FileType {ID = ID, Description = Description, ProgID = ProgID, Icon = Icon};
            capability.Extensions.AddAll(Extensions);
            capability.Verbs.AddAll(Verbs);
            return capability;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FileType other)
        {
            if (other == null) return false;

            return base.Equals(other) &&
                other.Description == Description && other.ProgID == ProgID && other.Icon == Icon &&
                Extensions.SequencedEquals(other.Extensions) && Verbs.SequencedEquals(other.Verbs);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(FileType) && Equals((FileType)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Description ?? "").GetHashCode();
                result = (result * 397) ^ Icon.GetHashCode();
                result = (result * 397) ^ (ProgID ?? "").GetHashCode();
                result = (result * 397) ^ Extensions.GetSequencedHashCode();
                result = (result * 397) ^ Verbs.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
