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
using JetBrains.Annotations;
using NanoByte.Common;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Represents an individual non-disjoint part of a <see cref="VersionRange"/>.
    /// </summary>
    /// <remarks>This class is immutable and thread-safe.</remarks>
    [Serializable]
    internal abstract class VersionRangePart
    {
        #region Factory
        /// <summary>
        /// Parses a string into a <see cref="VersionRange"/> part.
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="value"/> is not a valid version range string.</exception>
        [NotNull]
        public static VersionRangePart FromString([NotNull] string value)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
            #endregion

            if (value.Contains(".."))
            {
                string start = value.GetLeftPartAtFirstOccurrence("..");
                var startVersion = string.IsNullOrEmpty(start) ? null : new ImplementationVersion(start);

                ImplementationVersion endVersion;
                string end = value.GetRightPartAtFirstOccurrence("..");
                if (string.IsNullOrEmpty(end)) endVersion = null;
                else
                {
                    if (!end.StartsWith("!")) throw new ArgumentException(string.Format(Resources.VersionRangeEndNotExclusive, end));
                    endVersion = new ImplementationVersion(end.Substring(1));
                }

                return new VersionRangeRange(startVersion, endVersion);
            }
            else if (value.StartsWith("!"))
            {
                return new VersionRangeExclude(
                    new ImplementationVersion(value.Substring(1)));
            }
            else
            {
                return new VersionRangeExact(
                    new ImplementationVersion(value));
            }
        }
        #endregion

        /// <summary>
        /// Intersects a <see cref="Constraint"/> with this range and returns the result as a new range.
        /// </summary>
        [CanBeNull]
        public abstract VersionRangePart Intersects([NotNull] Constraint constraint);

        /// <summary>
        /// Determines whether a specific version lies within this range.
        /// </summary>
        public abstract bool Match([NotNull] ImplementationVersion version);
    }

    #region Specific types
    [Serializable]
    internal sealed class VersionRangeExact : VersionRangePart
    {
        private readonly ImplementationVersion _version;

        public VersionRangeExact(ImplementationVersion version)
        {
            #region Sanity checks
            if (version == null) throw new ArgumentNullException("version");
            #endregion

            _version = version;
        }

        public override VersionRangePart Intersects(Constraint constraint)
        {
            #region Sanity checks
            if (constraint == null) throw new ArgumentNullException("constraint");
            #endregion

            // If the exact version lies within the constraint, the exact version remains
            if (constraint.NotBefore != null && _version < constraint.NotBefore) return null;
            if (constraint.Before != null && _version >= constraint.Before) return null;
            return this;
        }

        public override bool Match(ImplementationVersion version)
        {
            #region Sanity checks
            if (version == null) throw new ArgumentNullException("version");
            #endregion

            return _version.Equals(version);
        }

        public override string ToString()
        {
            return _version.ToString();
        }

        #region Equality
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is VersionRangeExact && _version.Equals(((VersionRangeExact)obj)._version);
        }

        public override int GetHashCode()
        {
            return _version.GetHashCode();
        }
        #endregion
    }

    [Serializable]
    internal sealed class VersionRangeExclude : VersionRangePart
    {
        private readonly ImplementationVersion _version;

        public VersionRangeExclude(ImplementationVersion version)
        {
            #region Sanity checks
            if (version == null) throw new ArgumentNullException("version");
            #endregion

            _version = version;
        }

        public override VersionRangePart Intersects(Constraint constraint)
        {
            #region Sanity checks
            if (constraint == null) throw new ArgumentNullException("constraint");
            #endregion

            // If the exclude version lies outside the constraint, the constraint remains
            if ((constraint.NotBefore == null || _version >= constraint.NotBefore) &&
                (constraint.Before == null || _version < constraint.Before)) return null;
            return new VersionRangeRange(constraint.NotBefore, constraint.Before);
        }

        public override bool Match(ImplementationVersion version)
        {
            #region Sanity checks
            if (version == null) throw new ArgumentNullException("version");
            #endregion

            return !_version.Equals(version);
        }

        public override string ToString()
        {
            return "!" + _version;
        }

        #region Equality
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is VersionRangeExclude && _version.Equals(((VersionRangeExclude)obj)._version);
        }

        public override int GetHashCode()
        {
            return _version.GetHashCode();
        }
        #endregion
    }

    [Serializable]
    internal sealed class VersionRangeRange : VersionRangePart
    {
        private readonly ImplementationVersion _startVersion;
        private readonly ImplementationVersion _endVersion;

        public VersionRangeRange(ImplementationVersion startVersion, ImplementationVersion endVersion)
        {
            _startVersion = startVersion;
            _endVersion = endVersion;
        }

        public override VersionRangePart Intersects(Constraint constraint)
        {
            #region Sanity checks
            if (constraint == null) throw new ArgumentNullException("constraint");
            #endregion

            // Keep the highest lower bound
            ImplementationVersion startVersion;
            if (_startVersion == null || (constraint.NotBefore != null && constraint.NotBefore > _startVersion)) startVersion = constraint.NotBefore;
            else startVersion = _startVersion;

            // Keep the lowest upper bound
            ImplementationVersion endVersion;
            if (_endVersion == null || (constraint.Before != null && constraint.Before < _endVersion)) endVersion = constraint.Before;
            else endVersion = _endVersion;

            // Exclude impossible ranges
            if (startVersion != null && endVersion != null && startVersion >= endVersion) return null;
            return new VersionRangeRange(startVersion, endVersion);
        }

        public override bool Match(ImplementationVersion version)
        {
            #region Sanity checks
            if (version == null) throw new ArgumentNullException("version");
            #endregion

            if (_startVersion != null && version < _startVersion) return false;
            if (_endVersion != null && version >= _endVersion) return false;
            return true;
        }

        public override string ToString()
        {
            string result = _startVersion + "..";
            if (_endVersion != null) result += "!" + _endVersion;
            return result;
        }

        #region Equality
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var other = obj as VersionRangeRange;
            if (other == null) return false;
            return Equals(_startVersion, other._startVersion) && Equals(_endVersion, other._endVersion);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_startVersion != null ? _startVersion.GetHashCode() : 0) * 397) ^ (_endVersion != null ? _endVersion.GetHashCode() : 0);
            }
        }
        #endregion
    }
    #endregion
}
