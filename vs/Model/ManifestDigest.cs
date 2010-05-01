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
    #region Enumerations
    /// <see cref="ManifestDigest.BestMethod"/>
    public enum HashMethod
    {
        None, Sha1, Sha1New, Sha256
    }
    #endregion

    /// <summary>
    /// Stores digests of the .manifest file using various hashing algorithms.
    /// </summary>
    [TypeConverter(typeof(ManifestDigestConverter))]
    public struct ManifestDigest : IEquatable<ManifestDigest>
    {
        #region Properties
        /// <summary>
        /// A SHA-1 hash of the old manifest format.
        /// </summary>
        [Description("A SHA-1 hash of the old manifest format.")]
        [XmlAttribute("sha1")]
        public string Sha1 { get; set; }

        /// <summary>
        /// A SHA-1 hash of the new manifest format.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        [Description("A SHA-1 hash of the new manifest format.")]
        [XmlAttribute("sha1new")]
        public string Sha1New { get; set; }

        /// <summary>
        /// A SHA-256 hash of the new manifest format. (most secure)
        /// </summary>
        [Description("A SHA-256 hash of the new manifest format. (most secure)")]
        [XmlAttribute("sha256")]
        public string Sha256 { get; set; }

        /// <summary>
        /// Indicates the best hash-method that provides a value.
        /// </summary>
        public HashMethod BestMethod
        {
            get
            {
                if (!string.IsNullOrEmpty(Sha256)) return HashMethod.Sha256;
                if (!string.IsNullOrEmpty(Sha1New)) return HashMethod.Sha1New;
                if (!string.IsNullOrEmpty(Sha1)) return HashMethod.Sha1;
                return HashMethod.None;
            }
        }
        #endregion
        
        #region Constructor
        /// <summary>
        /// Creates a new manifest digest sturcture with pre-set values.
        /// </summary>
        /// <param name="sha1">A SHA-1 hash of the old manifest format.</param>
        /// <param name="sha1New">A SHA-1 hash of the new manifest format.</param>
        /// <param name="sha256">A SHA-256 hash of the new manifest format. (most secure)</param>
        public ManifestDigest(string sha1, string sha1New, string sha256) : this()
        {
            Sha1 = sha1;
            Sha1New = sha1New;
            Sha256 = sha256;
        }
        #endregion

        //--------------------//
        
        #region Conversion
        public override string ToString()
        {
            return string.Format("sha1={0}, sha1new={1}, sha256={2}", Sha1, Sha1New, Sha256);
        }
        #endregion

        #region Equality
        public bool Equals(ManifestDigest other)
        {
            return other.Sha1 == Sha1 && other.Sha1New == Sha1New && other.Sha256 == Sha256;
        }

        public static bool operator ==(ManifestDigest left, ManifestDigest right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ManifestDigest left, ManifestDigest right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj.GetType() == typeof(ManifestDigest) && Equals((ManifestDigest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Sha1 != null ? Sha1.GetHashCode() : 0);
                result = (result * 397) ^ (Sha1New != null ? Sha1New.GetHashCode() : 0);
                result = (result * 397) ^ (Sha256 != null ? Sha256.GetHashCode() : 0);
                return result;
            }
        }
        #endregion
    }
}
