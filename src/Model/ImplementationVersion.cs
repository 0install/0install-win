﻿/*
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
using System.Linq;
using System.Text;
using Common.Values.Design;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a version number consisting of dot-separated decimals and optional modifier strings.
    /// </summary>
    /// <remarks>
    /// <para>This class is immutable.</para>
    /// <para>
    ///   This is the syntax for valid version strings:
    ///   <code> 
    ///   Version := DottedList ("-" Modifier? DottedList?)*
    ///   DottedList := (Integer ("." Integer)*)
    ///   Modifier := "pre" | "rc" | "post"
    ///   </code>
    /// </para>
    /// </remarks>
    [Serializable]
    [TypeConverter(typeof(StringConstructorConverter<ImplementationVersion>))]
    public sealed class ImplementationVersion : IEquatable<ImplementationVersion>, IComparable<ImplementationVersion>
    {
        #region Variables
        /// <summary>The first part of the version number.</summary>
        private readonly VersionDottedList _firstPart;

        /// <summary>All additional parts of the version number.</summary>
        private readonly VersionPart[] _additionalParts;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new implementation version from a a string.
        /// </summary>
        /// <param name="value">The string containing the version information.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not a valid version string.</exception>
        public ImplementationVersion(string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            string[] parts = value.Split('-');

            // Ensure the first part is a dotted list
            if (!VersionDottedList.IsValid(parts[0])) throw new ArgumentException(Resources.MustStartWithDottedList);
            _firstPart = new VersionDottedList(parts[0]);

            // Iterate through all additional parts
            _additionalParts = new VersionPart[parts.Length - 1];
            for (int i = 1; i < parts.Length; i++)
                _additionalParts[i - 1] = new VersionPart(parts[i]);
        }

        /// <summary>
        /// Creates a new implementation version from a .NET <see cref="Version"/>.
        /// </summary>
        /// <param name="version">The .NET <see cref="Version"/> to convert.</param>
        public ImplementationVersion(Version version)
        {
            #region Sanity checks
            if (version == null) throw new ArgumentNullException("version");
            #endregion

            _firstPart = new VersionDottedList(version.ToString());
            _additionalParts = new VersionPart[0];
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new <see cref="ImplementationVersion"/> using the specified string representation.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="result">Returns the created <see cref="ImplementationVersion"/> if successfully; <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> if the <see cref="ImplementationVersion"/> was successfully created; <see langword="false"/> otherwise.</returns>
        public static bool TryCreate(string value, out ImplementationVersion result)
        {
            try
            {
                result = new ImplementationVersion(value);
                return true;
            }
            catch (ArgumentException)
            {
                result = null;
                return false;
            }
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns a string representation of the version. Safe for parsing!
        /// </summary>        
        public override string ToString()
        {
            var output = new StringBuilder();
            output.Append(_firstPart);

            // Separate additional parts with hyphens
            if (_additionalParts != null)
                foreach (var part in _additionalParts) output.Append("-" + part);

            return output.ToString();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ImplementationVersion other)
        {
            if (ReferenceEquals(null, other)) return false;

            // Cancel if the first part of the version or the number of additional parts don't match
            if (!_firstPart.Equals(other._firstPart) ||
                _additionalParts.Length != other._additionalParts.Length)
                return false;

            // Cacnel if one of the additional parts does not match
            return !_additionalParts.Where((part, i) => !part.Equals(other._additionalParts[i])).Any();

            // If we reach this, everything was equal
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ImplementationVersion && Equals((ImplementationVersion)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable LoopCanBeConvertedToQuery
                int result = _firstPart.GetHashCode();
                foreach (var part in _additionalParts) result = (result * 397) ^ part.GetHashCode();
                return result;
                // ReSharper restore LoopCanBeConvertedToQuery
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(ImplementationVersion left, ImplementationVersion right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(ImplementationVersion left, ImplementationVersion right)
        {
            return !Equals(left, right);
        }
        #endregion

        #region Comparison
        /// <inheritdoc/>
        public int CompareTo(ImplementationVersion other)
        {
            #region Sanity checks
            if (ReferenceEquals(null, other)) throw new ArgumentNullException("other");
            #endregion

            int firstPartCompared = _firstPart.CompareTo(other._firstPart);
            if (firstPartCompared != 0) return firstPartCompared;

            int leastNumberOfAdditionalParts = Math.Max(_additionalParts.Length, other._additionalParts.Length);
            for (int i = 0; i < leastNumberOfAdditionalParts; ++i)
            {
                var left = i >= _additionalParts.Length ? VersionPart.Default : _additionalParts[i];
                var right = i >= other._additionalParts.Length ? VersionPart.Default : other._additionalParts[i];
                int comparisonResult = left.CompareTo(right);
                if (comparisonResult != 0)
                    return comparisonResult;
            }
            return 0;
        }

        /// <inheritdoc/>
        public static bool operator <(ImplementationVersion left, ImplementationVersion right)
        {
            #region Sanity checks
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            #endregion

            return left.CompareTo(right) < 0;
        }

        /// <inheritdoc/>
        public static bool operator >(ImplementationVersion left, ImplementationVersion right)
        {
            #region Sanity checks
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            #endregion

            return left.CompareTo(right) > 0;
        }

        /// <inheritdoc/>
        public static bool operator <=(ImplementationVersion left, ImplementationVersion right)
        {
            #region Sanity checks
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            #endregion

            return left.CompareTo(right) <= 0;
        }

        /// <inheritdoc/>
        public static bool operator >=(ImplementationVersion left, ImplementationVersion right)
        {
            #region Sanity checks
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            #endregion

            return left.CompareTo(right) >= 0;
        }
        #endregion
    }
}
