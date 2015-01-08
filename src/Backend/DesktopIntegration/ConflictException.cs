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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using JetBrains.Annotations;
using NanoByte.Common;
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
        /// <summary>
        /// The entries that are in conflict with each other.
        /// </summary>
        [PublicAPI]
        public IEnumerable<ConflictData> Entries { get; private set; }

        /// <summary>
        /// Creates an exception indicating a new desktop integration conflict.
        /// </summary>
        /// <param name="existingEntry">The existing entry that is preventing <paramref name="newEntry"/> from being applied.</param>
        /// <param name="newEntry">The new entry that is in conflict with <paramref name="existingEntry"/>.</param>
        public static ConflictException NewConflict(ConflictData existingEntry, ConflictData newEntry)
        {
            #region Sanity checks
            if (existingEntry == null) throw new ArgumentNullException("existingEntry");
            if (newEntry == null) throw new ArgumentNullException("newEntry");
            #endregion

            string message = string.Format(Resources.AccessPointNewConflict, existingEntry, newEntry);
            return new ConflictException(message) {Entries = new[] {existingEntry, newEntry}};
        }

        /// <summary>
        /// Creates an exception indicating an inner desktop integration conflict.
        /// </summary>
        /// <param name="entries">The entries that are in conflict with each other.</param>
        public static ConflictException InnerConflict([NotNull] params ConflictData[] entries)
        {
            #region Sanity checks
            if (entries == null) throw new ArgumentNullException("entries");
            #endregion

            string message = string.Format(Resources.AccessPointInnerConflict, entries[0].AppEntry) + Environment.NewLine +
                             StringUtils.Join(Environment.NewLine, entries.Select(x => x.AccessPoint.ToString()));
            return new ConflictException(message) {Entries = entries};
        }

        /// <summary>
        /// Creates an exception indicating an existing desktop integration conflict.
        /// </summary>
        /// <param name="entries">The entries that are in conflict with each other.</param>
        public static ConflictException ExistingConflict([NotNull] params ConflictData[] entries)
        {
            #region Sanity checks
            if (entries == null) throw new ArgumentNullException("entries");
            #endregion

            string message = Resources.AccessPointExistingConflict + Environment.NewLine +
                             StringUtils.Join(Environment.NewLine, entries.Select(x => x.ToString()));
            return new ConflictException(message) {Entries = entries};
        }

        /// <inheritdoc/>
        public ConflictException() : base("Unknown conflict")
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
