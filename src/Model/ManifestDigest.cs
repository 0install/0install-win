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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using Common.Utils;
using ZeroInstall.Model.Design;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Stores digests of the .manifest file using various hashing algorithms.
    /// </summary>
    /// <remarks>A manifest digest is a means of uniquely identifying an <see cref="Implementation"/> and verifying its contents.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes", Justification = "Comparison only used for string sorting in UI lists")]
    [TypeConverter(typeof(ManifestDigestConverter))]
    [Serializable]
    [XmlType("manifest-digest", Namespace = Feed.XmlNamespace)]
    public struct ManifestDigest : IEquatable<ManifestDigest>, IComparable<ManifestDigest>
    {
        #region Constants
        /// <summary>The prefix used to identify the <see cref="Sha1Old"/> format.</summary>
        public const string Sha1OldPrefix = "sha1";

        /// <summary>The prefix used to identify the <see cref="Sha1New"/> format.</summary>
        public const string Sha1NewPrefix = "sha1new";

        /// <summary>The prefix used to identify the <see cref="Sha256"/> format.</summary>
        public const string Sha256Prefix = "sha256";
        #endregion

        #region Properties
        /// <summary>
        /// A SHA-1 hash of the old manifest format.
        /// </summary>
        [Description("A SHA-1 hash of the old manifest format.")]
        [XmlAttribute("sha1"), DefaultValue("")]
        public string Sha1Old { get; set; }

        /// <summary>
        /// A SHA-1 hash of the new manifest format.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        [Description("A SHA-1 hash of the new manifest format.")]
        [XmlAttribute("sha1new"), DefaultValue("")]
        public string Sha1New { get; set; }

        /// <summary>
        /// A SHA-256 hash of the new manifest format. (most secure)
        /// </summary>
        [Description("A SHA-256 hash of the new manifest format. (most secure)")]
        [XmlAttribute("sha256"), DefaultValue("")]
        public string Sha256 { get; set; }

        /// <summary>
        /// Lists all contained manifest digests (format=hash) sorted from best (safest) to worst.
        /// </summary>
        [XmlIgnore]
        public IEnumerable<string> AvailableDigests
        {
            get
            {
                ICollection<string> list = new LinkedList<string>();

                if (!string.IsNullOrEmpty(Sha256)) list.Add(Sha256Prefix + "=" + Sha256);
                if (!string.IsNullOrEmpty(Sha1New)) list.Add(Sha1NewPrefix + "=" + Sha1New);
                if (!string.IsNullOrEmpty(Sha1Old)) list.Add(Sha1OldPrefix + "=" + Sha1Old);

                return list;
            }
        }

        /// <summary>
        /// Returns the best (safest) contained manifest digest (format=hash). <see langword="null"/> if none is set.
        /// </summary>
        [XmlIgnore]
        public string BestDigest
        {
            get
            {
                if (!string.IsNullOrEmpty(Sha256)) return Sha256Prefix + "=" + Sha256;
                if (!string.IsNullOrEmpty(Sha1New)) return Sha1NewPrefix + "=" + Sha1New;
                if (!string.IsNullOrEmpty(Sha1Old)) return Sha1OldPrefix + "=" + Sha1Old;
                return null;
            }
        }

        /// <summary>
        /// Returns the prefix of the best (safest) contained manifest digest. <see langword="null"/> if none is set.
        /// </summary>
        [XmlIgnore]
        public string BestPrefix
        {
            get
            {
                if (!string.IsNullOrEmpty(Sha256)) return Sha256Prefix;
                if (!string.IsNullOrEmpty(Sha1New)) return Sha1NewPrefix;
                if (!string.IsNullOrEmpty(Sha1Old)) return Sha1OldPrefix;
                return null;
            }
        }

        /// <summary>
        /// Contains any unknown hash algorithms specified as pure XML attributes.
        /// </summary>
        [XmlAnyAttribute, NonSerialized]
        public XmlAttribute[] UnknownAlgorithms;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new manifest digest structure by parsing an ID string.
        /// </summary>
        /// <param name="id">The ID string to parse. Digest values start with their format name followed by an equals sign and the actual hash.</param>
        /// <seealso cref="ParseID"/>
        public ManifestDigest(string id) : this()
        {
            ParseID(id, ref this);
        }

        /// <summary>
        /// Creates a new manifest digest structure with pre-set values.
        /// </summary>
        /// <param name="sha1Old">A SHA-1 hash of the old manifest format.</param>
        /// <param name="sha1New">A SHA-1 hash of the new manifest format.</param>
        /// <param name="sha256">A SHA-256 hash of the new manifest format. (most secure)</param>
        public ManifestDigest(string sha1Old, string sha1New, string sha256) : this()
        {
            Sha1Old = sha1Old;
            Sha1New = sha1New;
            Sha256 = sha256;
        }
        #endregion

        //--------------------//

        #region Parsing
        /// <summary>
        /// Parses an ID string, checking for digest values. The values will be stored in a <see cref="ManifestDigest"/> if the corresponding digest value hasn't been set already.
        /// </summary>
        /// <param name="id">The ID string to parse. Digest values start with their format name followed by an equals sign and the actual hash.</param>
        /// <param name="target">The <see cref="ManifestDigest"/> to store the values in.</param>
        public static void ParseID(string id, ref ManifestDigest target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            if (!id.Contains("=")) throw new ArgumentException(string.Format(Resources.InvalidDigest, id));
            #endregion

            // Split the ID string
            string prefix = StringUtils.GetLeftPartAtFirstOccurrence(id, '=');
            string hash = StringUtils.GetRightPartAtFirstOccurrence(id, '=');

            // Check for known prefixes (and don't overwrite existing values)
            switch (prefix)
            {
                case Sha1OldPrefix:
                    if (string.IsNullOrEmpty(target.Sha1Old)) target.Sha1Old = hash;
                    break;
                case Sha1NewPrefix:
                    if (string.IsNullOrEmpty(target.Sha1New)) target.Sha1New = hash;
                    break;
                case Sha256Prefix:
                    if (string.IsNullOrEmpty(target.Sha256)) target.Sha256 = hash;
                    break;
            }
        }
        #endregion

        #region Access

        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the manifest digests in the form "Algorithm1=Hash1, Algorithm2=Hash2, ...". Safe for parsing.
        /// </summary>
        public override string ToString()
        {
            return string.Format("sha1={0}, sha1new={1}, sha256={2}", Sha1Old, Sha1New, Sha256);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ManifestDigest other)
        {
            return other.Sha1Old == Sha1Old && other.Sha1New == Sha1New && other.Sha256 == Sha256;
        }

        /// <summary>
        /// Indicates whether this digest is at least partially equal to another one.
        /// </summary>
        /// <remarks>Two digests are considered partially equal if at least one digest format matches and no values are contradictory.</remarks>
        public bool PartialEquals(ManifestDigest other)
        {
            int matchCounter = 0;
            return
                PartialEqualsHelper(ref matchCounter, Sha1Old, other.Sha1Old) &&
                PartialEqualsHelper(ref matchCounter, Sha1New, other.Sha1New) &&
                PartialEqualsHelper(ref matchCounter, Sha256, other.Sha256) &&
                (matchCounter > 0);
        }

        /// <summary>
        /// Compares to values as a helper method for <see cref="PartialEquals"/>.
        /// </summary>
        /// <param name="matchCounter">This value is incremented by one of the values match and are not <see langword="null"/>.</param>
        /// <param name="left">The first value to be compared.</param>
        /// <param name="right">The second value to be compared.</param>
        /// <returns><see langword="true"/> if the values either match or one of them is <see langword="null"/>.</returns>
        private static bool PartialEqualsHelper(ref int matchCounter, string left, string right)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) return true;
            if (left == right)
            {
                matchCounter++;
                return true;
            }
            else return false;
        }

        /// <inheritdoc/>
        public static bool operator ==(ManifestDigest left, ManifestDigest right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(ManifestDigest left, ManifestDigest right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(ManifestDigest) && Equals((ManifestDigest)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Sha1Old != null ? Sha1Old.GetHashCode() : 0);
                result = (result * 397) ^ (Sha1New != null ? Sha1New.GetHashCode() : 0);
                result = (result * 397) ^ (Sha256 != null ? Sha256.GetHashCode() : 0);
                return result;
            }
        }
        #endregion

        #region Comparison
        int IComparable<ManifestDigest>.CompareTo(ManifestDigest other)
        {
            if (Equals(other)) return 0;

            // Sort based on the best digest algorithm available
            int distance = BestDigest.CompareTo(other.BestDigest);
            
            // Only return 0 for true equality
            if (distance == 0) distance = 1;

            return distance;
        }
        #endregion
    }
}
