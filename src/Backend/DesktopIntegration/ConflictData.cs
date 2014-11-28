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
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Stores information about an <see cref="AccessPoint"/> conflict.
    /// </summary>
    public struct ConflictData : IEquatable<ConflictData>
    {
        /// <summary>
        /// The <see cref="AccessPoints.AccessPoint"/> causing the conflict.
        /// </summary>
        public readonly AccessPoint AccessPoint;

        /// <summary>
        /// The application containing the <see cref="AccessPoint"/>.
        /// </summary>
        public readonly AppEntry AppEntry;

        /// <summary>
        /// Creates a new conflict data element.
        /// </summary>
        /// <param name="accessPoint">The <see cref="AccessPoints.AccessPoint"/> causing the conflict.</param>
        /// <param name="appEntry">The application containing the <paramref name="accessPoint"/>.</param>
        public ConflictData(AccessPoint accessPoint, AppEntry appEntry)
        {
            AppEntry = appEntry;
            AccessPoint = accessPoint;
        }

        /// <summary>
        /// Returns the entry in the form "AccessPoint in AppEntry". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} in {1}", AccessPoint, AppEntry);
        }

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ConflictData other)
        {
            if (AppEntry == null || other.AppEntry == null || AccessPoint == null) return false;

            return
                AccessPoint.Equals(other.AccessPoint) &&
                AppEntry.InterfaceUri == other.AppEntry.InterfaceUri;
        }

        /// <inheritdoc/>
        public static bool operator ==(ConflictData left, ConflictData right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(ConflictData left, ConflictData right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ConflictData && Equals((ConflictData)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = AccessPoint.GetHashCode();
                if (AppEntry != null && AppEntry.InterfaceUri != null) result = (result * 397) ^ AppEntry.InterfaceUri.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
