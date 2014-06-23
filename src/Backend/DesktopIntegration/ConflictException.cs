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
using System.Runtime.Serialization;
using System.Security.Permissions;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Indicates a desktop integration operation could not be completed due to conflicting <see cref="AccessPoint"/>s.
    /// </summary>
    [Serializable]
    public sealed class ConflictException : Exception
    {
        #region Properties
        /// <summary>
        /// The existing entry that is preventing <see cref="NewEntry"/> from being applied.
        /// </summary>
        public ConflictData ExistingEntry { get; private set; }

        /// <summary>
        /// The new entry that is in conflict with <see cref="ExistingEntry"/>.
        /// </summary>
        public ConflictData NewEntry { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new desktop integration conflict exception.
        /// </summary>
        /// <param name="existingEntry">The existing entry that is preventing <paramref name="newEntry"/> from being applied.</param>
        /// <param name="newEntry">The new entry that is in conflict with <paramref name="existingEntry"/>.</param>
        public ConflictException(ConflictData existingEntry, ConflictData newEntry)
            : base(GetMessage(existingEntry, newEntry))
        {
            ExistingEntry = existingEntry;
            NewEntry = newEntry;
        }

        private static string GetMessage(ConflictData existingEntry, ConflictData newEntry)
        {
            return string.Format(Resources.AccessPointConflict,
                existingEntry.AccessPoint, existingEntry.AppEntry,
                newEntry.AccessPoint, newEntry.AppEntry);
        }

        /// <inheritdoc/>
        public ConflictException()
            : base("Unknown conflict")
        {}

        /// <inheritdoc/>
        public ConflictException(string message) : base(message)
        {}

        /// <inheritdoc/>
        public ConflictException(string message, Exception innerException) : base(message, innerException)
        {}

        /// <summary>
        /// Deserializes an exception.
        /// </summary>
        private ConflictException(SerializationInfo info, StreamingContext context) : base(info, context)
        {}
        #endregion

        #region Serialization
        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
        #endregion
    }
}
