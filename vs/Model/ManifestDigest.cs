﻿/*
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using Common.Helpers;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Stores digests of the .manifest file using various hashing algorithms.
    /// </summary>
    [TypeConverter(typeof(ManifestDigestConverter))]
    public struct ManifestDigest : IEquatable<ManifestDigest>
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
        [XmlAttribute("sha1")]
        public string Sha1Old { get; set; }

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

        #region Constructor
        /// <summary>
        /// Creates a new manifest digest structure by parsing an ID string.
        /// </summary>
        /// <param name="id">The ID string to parse. Digest values start with their format name followed by an equals sign and the actual hash.</param>
        /// <seealso cref="ParseID"/>
        public ManifestDigest(string id) : this()
        {
            ParseID(id);
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
        /// Parses an ID string, checking for digest values. The values will be stored in this instance if the corresponding digest value hasn't been set already.
        /// </summary>
        /// <param name="id">The ID string to parse. Digest values start with their format name followed by an equals sign and the actual hash.</param>
        public void ParseID(string id)
        {
            // Split the ID string
            string prefix = StringHelper.GetLeftPartAtFirstOccurrence(id, '=');
            string hash = StringHelper.GetRightPartAtFirstOccurrence(id, '=');

            // Check for known prefixes (and don't overwrite existing values)
            switch (prefix)
            {
                case Sha1OldPrefix:
                    if (string.IsNullOrEmpty(Sha1Old)) Sha1Old = hash;
                    break;
                case Sha1NewPrefix:
                    if (string.IsNullOrEmpty(Sha1New)) Sha1New = hash;
                    break;
                case Sha256Prefix:
                    if (string.IsNullOrEmpty(Sha256)) Sha256 = hash;
                    break;
            }
        }
        #endregion

        #region Access
        /// <summary>
        /// Lists all contained manifest digests (format=hash) sorted from best (safest) to worst.
        /// </summary>
        public IEnumerable<string> GetDigests()
        {
            ICollection<string> list = new LinkedList<string>();

            if (!string.IsNullOrEmpty(Sha256)) list.Add(Sha256Prefix + "=" + Sha256);
            if (!string.IsNullOrEmpty(Sha1New)) list.Add(Sha1NewPrefix + "=" + Sha1New);
            if (!string.IsNullOrEmpty(Sha1Old)) list.Add(Sha1OldPrefix + "=" + Sha1Old);

            return list;
        }

        /// <summary>
        /// Returns the best (safest) contained manifest digest (format=hash). <see langword="null"/> if none is set.
        /// </summary>
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
        #endregion

        //--------------------//

        #region Conversion
        public override string ToString()
        {
            return string.Format("sha1={0}, sha1new={1}, sha256={2}", Sha1Old, Sha1New, Sha256);
        }
        #endregion

        #region Equality
        public bool Equals(ManifestDigest other)
        {
            return other.Sha1Old == Sha1Old && other.Sha1New == Sha1New && other.Sha256 == Sha256;
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
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(ManifestDigest) && Equals((ManifestDigest)obj);
        }

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
    }
}
