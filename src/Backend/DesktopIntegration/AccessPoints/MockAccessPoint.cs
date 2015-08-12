/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// A mock access point that does nothing (used for testing). Points to a <see cref="Store.Model.Capabilities.FileType"/>.
    /// </summary>
    [XmlType("mock", Namespace = AppList.XmlNamespace)]
    public class MockAccessPoint : DefaultAccessPoint, IEquatable<MockAccessPoint>
    {
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            return string.IsNullOrEmpty(ID) ? new string[0] : new[] {"mock:" + ID};
        }

        /// <summary>
        /// An indentifier that controls the result of <see cref="GetConflictIDs"/>.
        /// </summary>
        [XmlAttribute("id")]
        public string ID { get; set; }

        /// <summary>
        /// The path to a file to create when <see cref="Apply"/> is called.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag"), XmlAttribute("apply-flag-path")]
        public string ApplyFlagPath { get; set; }

        /// <summary>
        /// The path to a file to create when <see cref="Unapply"/> is called.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flag"), XmlAttribute("unapply-flag-path")]
        public string UnapplyFlagPath { get; set; }

        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            #endregion

            if (!string.IsNullOrEmpty(ID))
            {
                // Trigger exceptions in case invalid capabilities are referenced
                appEntry.LookupCapability<Store.Model.Capabilities.FileType>(Capability);
            }

            if (!string.IsNullOrEmpty(ApplyFlagPath)) FileUtils.Touch(ApplyFlagPath);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            #endregion

            if (!string.IsNullOrEmpty(ID))
            {
                // Trigger exceptions in case invalid capabilities are referenced
                appEntry.LookupCapability<Store.Model.Capabilities.FileType>(Capability);
            }

            if (!string.IsNullOrEmpty(UnapplyFlagPath)) FileUtils.Touch(UnapplyFlagPath);
        }

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "MockAccessPoint: ID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return $"MockAccessPoint: {ID} (ApplyFlagPath: {ApplyFlagPath}, UnapplyFlagPath: {UnapplyFlagPath})";
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new MockAccessPoint
            {
                ID = ID, Capability = Capability,
                ApplyFlagPath = ApplyFlagPath, UnapplyFlagPath = UnapplyFlagPath,
                UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements
            };
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(MockAccessPoint other)
        {
            if (other == null) return false;
            return other.ID == ID;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj.GetType() == typeof(MockAccessPoint) && Equals((MockAccessPoint)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (ID ?? "").GetHashCode();
            }
        }
        #endregion
    }
}
