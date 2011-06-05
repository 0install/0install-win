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
    [XmlType("file-type", Namespace = XmlNamespace)]
    public class FileType : Capability, IEquatable<FileType>
    {
        #region Properties
        /// <inheritdoc/>
        public override bool MachineWideOnly { get { return false; } }

        /// <summary>
        /// A human-readable description such as "PNG image file".
        /// </summary>
        [Description("A human-readable description such as \"PNG image file\".")]
        [XmlAttribute("description")]
        public string Description { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Icon> _icons = new C5.ArrayList<Icon>();
        /// <summary>
        /// Zero or more icons to use for the file type.
        /// </summary>
        /// <remarks>The first compatible one is selected. If empty the application icon is used.</remarks>
        [Description("Zero or more icons to use for the file type. (The first compatible one is selected. If empty the application icon is used.)")]
        [XmlElement("icon", Namespace = Feed.XmlNamespace)]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<Icon> Icons { get { return _icons; } }

        /// <summary>
        /// A list of all operations available for this file type.
        /// </summary>
        [Description("A list of all operations available for this file type.")]
        [XmlElement("verb")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.HashedArrayList<FileTypeVerb> Verbs { get { return _verbs; } }

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
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "FileType: Description (ID)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("FileType : {0} ({1})", Description, ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability CloneCapability()
        {
            var capability = new FileType {ID = ID, Description = Description};
            capability.Icons.AddAll(Icons);
            capability.Verbs.AddAll(Verbs);
            capability.Extensions.AddAll(Extensions);
            return capability;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(FileType other)
        {
            if (other == null) return false;

            return base.Equals(other) &&
                other.Description == Description &&
                Icons.SequencedEquals(other.Icons) && Verbs.SequencedEquals(other.Verbs) && Extensions.SequencedEquals(other.Extensions);
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
                result = (result * 397) ^ Icons.GetSequencedHashCode();
                result = (result * 397) ^ Verbs.GetSequencedHashCode();
                result = (result * 397) ^ Extensions.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
