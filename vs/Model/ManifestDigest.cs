using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Stores digests of the .manifest file using various hashing algorithms.
    /// </summary>
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
