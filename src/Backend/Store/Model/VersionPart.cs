/*
 * Copyright 2010-2015 Bastian Eicher
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
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
{

    #region Enumerations
    /// <see cref="VersionPart.Modifier"/>
    internal enum VersionModifier
    {
        /// <summary>No modifier; empty string</summary>
        None = 0,

        /// <summary>Pre-release</summary>
        Pre = -2,

        /// <summary>Release candidate</summary>
        RC = -1,

        /// <summary>Post-release</summary>
        Post = 1
    }
    #endregion

    /// <summary>
    /// Represents a part of a <see cref="ImplementationVersion"/> containing nothing, a <see cref="VersionModifier"/>, a <see cref="DottedList"/> or both.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    [Serializable]
    internal struct VersionPart : IEquatable<VersionPart>, IComparable<VersionPart>
    {
        #region Constants
        /// <summary>
        /// A version number with the value -1.
        /// </summary>
        /// <remarks>-1 or "not set" has an even lower value than a set "0".</remarks>
        public static readonly VersionPart Default = new VersionPart("-1");
        #endregion

        /// <summary>
        /// The modifier part of the version part.
        /// </summary>
        public VersionModifier Modifier { get; private set; }

        /// <summary>
        /// The dotted list part of the version part.
        /// </summary>
        public VersionDottedList DottedList { get; private set; }

        /// <summary>
        /// Creates a new dotted-list from a a string.
        /// </summary>
        /// <param name="value">The string containing the dotted-list.</param>
        public VersionPart(string value) : this()
        {
            // Detect and trim version modifiers
            if (value.StartsWith("pre"))
            {
                value = value.Substring("pre".Length);
                Modifier = VersionModifier.Pre;
            }
            else if (value.StartsWith("rc"))
            {
                value = value.Substring("rc".Length);
                Modifier = VersionModifier.RC;
            }
            else if (value.StartsWith("post"))
            {
                value = value.Substring("post".Length);
                Modifier = VersionModifier.Post;
            }
            else
                Modifier = VersionModifier.None;

            // Parse any rest as dotted list
            if (!string.IsNullOrEmpty(value)) DottedList = new VersionDottedList(value);
        }

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            string result;
            switch (Modifier)
            {
                case VersionModifier.None:
                    result = "";
                    break;
                case VersionModifier.Pre:
                    result = "pre";
                    break;
                case VersionModifier.RC:
                    result = "rc";
                    break;
                case VersionModifier.Post:
                    result = "post";
                    break;
                default:
                    throw new InvalidOperationException(Resources.UnknownModifier);
            }

            // Combine both parts without any separator
            result += DottedList.ToString();

            return result;
        }
        #endregion

        #region Equality
        public bool Equals(VersionPart other)
        {
            return Modifier == other.Modifier && DottedList == other.DottedList;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is VersionPart && Equals((VersionPart)obj);
        }

        public static bool operator ==(VersionPart left, VersionPart right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VersionPart left, VersionPart right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Modifier * 397) ^ DottedList.GetHashCode();
            }
        }
        #endregion

        #region Comparison
        public int CompareTo(VersionPart other)
        {
            var modifierComparison = ((int)Modifier).CompareTo((int)other.Modifier);
            if (modifierComparison != 0) return modifierComparison;

            return DottedList.CompareTo(other.DottedList);
        }
        #endregion
    }
}
