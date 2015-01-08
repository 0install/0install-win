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
using System.Linq;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Values.Design;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Represents a (possibly disjoint) range of <see cref="ImplementationVersion"/>s.
    /// </summary>
    /// <remarks>
    /// <para>This class is immutable.</para>
    /// <para>
    ///   Ranges are separated by pipes (|).
    ///   Each range is in the form "START..!END". The range matches versions where START &lt;= VERSION &lt; END.
    ///   The start or end may be omitted. A single version number may be used instead of a range to match only that version,
    ///   or !VERSION to match everything except that version.
    /// </para>
    /// </remarks>
    [TypeConverter(typeof(StringConstructorConverter<VersionRange>))]
    [Serializable]
    public sealed class VersionRange : IEquatable<VersionRange>
    {
        #region Variables
        /// <summary>The individual non-disjoint range parts.</summary>
        private readonly VersionRangePart[] _parts;

        /// <summary>
        /// An "impossible" range matching no versions.
        /// </summary>
        public static readonly VersionRange None = new VersionRange(new VersionRangeRange(new ImplementationVersion("0"), new ImplementationVersion("0")));
        #endregion

        #region Constructor
        private VersionRange([NotNull] params VersionRangePart[] parts)
        {
            #region Sanity checks
            if (parts == null) throw new ArgumentNullException("parts");
            #endregion

            _parts = parts;
        }

        /// <summary>
        /// Creates an empty version range (matches anything).
        /// </summary>
        public VersionRange()
        {
            _parts = new VersionRangePart[0];
        }

        /// <summary>
        /// Creates a new version range from a a string.
        /// </summary>
        /// <param name="value">The string containing the version information.</param>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a valid version range string.</exception>
        public VersionRange([NotNull] string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            // Iterate through all parts
            _parts = Array.ConvertAll(value.Split('|'), part => VersionRangePart.FromString(part.Trim()));
        }

        /// <summary>
        /// Creates a single interval version range.
        /// </summary>
        /// <param name="notBefore">The lower, inclusive border of the range; can be <see langword="null"/>.</param>
        /// <param name="before">The upper, exclusive border of the range; can be <see langword="null"/>.</param>
        public VersionRange([CanBeNull] ImplementationVersion notBefore, [CanBeNull] ImplementationVersion before)
        {
            _parts = new VersionRangePart[] {new VersionRangeRange(notBefore, before)};
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new <see cref="VersionRange"/> using the specified string representation.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="result">Returns the created <see cref="VersionRange"/> if successfully; <see langword="null"/> otherwise.</param>
        /// <returns><see langword="true"/> if the <see cref="VersionRange"/> was successfully created; <see langword="false"/> otherwise.</returns>
        [ContractAnnotation("=>false,result:null; =>true,result:notnull")]
        public static bool TryCreate(string value, out VersionRange result)
        {
            try
            {
                result = new VersionRange(value);
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

        #region Intersect
        /// <summary>
        /// Intersects a <see cref="Constraint"/> with this range and returns the result as a new range.
        /// </summary>
        public VersionRange Intersect(Constraint constraint)
        {
            #region Sanity checks
            if (constraint == null) throw new ArgumentNullException("constraint");
            #endregion

            if (_parts.Length == 0) return new VersionRange(new VersionRangeRange(constraint.NotBefore, constraint.Before));

            var parts = _parts.Select(part => part.Intersects(constraint)).WhereNotNull();
            return parts.Any() ? new VersionRange(parts.ToArray()) : None;
        }
        #endregion

        #region Match
        /// <summary>
        /// Determines whether a specific version lies within this range.
        /// </summary>
        public bool Match(ImplementationVersion version)
        {
            #region Sanity checks
            if (version == null) throw new ArgumentNullException("version");
            #endregion

            if (_parts.Length == 0) return true;
            return _parts.Any(part => part.Match(version));
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns a string representation of the version range. Safe for parsing!
        /// </summary>
        public override string ToString()
        {
            // Separate parts with colons
            return StringUtils.Join("|", _parts.Select(part => part.ToString()));
        }

        /// <summary>
        /// Convenience cast for turning <see cref="ImplementationVersion"/>s into lower bounds for <see cref="VersionRange"/>s.
        /// </summary>
        public static explicit operator VersionRange(ImplementationVersion version)
        {
            if (version == null) return null;
            return new VersionRange(new VersionRangeRange(version, null));
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(VersionRange other)
        {
            if (ReferenceEquals(null, other)) return false;

            // Cancel if the number of parts don't match
            if (_parts.Length != other._parts.Length)
                return false;

            // Cacnel if one of the parts does not match
            for (int i = 0; i < _parts.Length; i++)
            {
                if (!_parts[i].Equals(other._parts[i]))
                    return false;
            }

            // If we reach this, everything was equal
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is VersionRange && Equals((VersionRange)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = 397;
                foreach (VersionRangePart part in _parts)
                    result = (result * 397) ^ part.GetHashCode();
                return result;
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(VersionRange left, VersionRange right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(VersionRange left, VersionRange right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
