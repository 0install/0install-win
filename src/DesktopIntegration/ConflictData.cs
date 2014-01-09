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

using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Stores information about an <see cref="AccessPoint"/> conflict.
    /// </summary>
    public struct ConflictData
    {
        /// <summary>
        /// The application containing the <see cref="AccessPoint"/>.
        /// </summary>
        public readonly AppEntry AppEntry;

        /// <summary>
        /// The <see cref="AccessPoints.AccessPoint"/> causing the conflict.
        /// </summary>
        public readonly AccessPoint AccessPoint;

        /// <summary>
        /// Creates a new conflict data element.
        /// </summary>
        /// <param name="appEntry">The application containing the <paramref name="accessPoint"/>.</param>
        /// <param name="accessPoint">The <see cref="AccessPoints.AccessPoint"/> causing the conflict.</param>
        public ConflictData(AppEntry appEntry, AccessPoint accessPoint)
        {
            AppEntry = appEntry;
            AccessPoint = accessPoint;
        }

        /// <summary>
        /// Returns the entry in the form "AppEntry, AccessPoint". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}, {1}", AppEntry, AccessPoint);
        }
    }
}
