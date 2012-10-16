/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Xml.Serialization;
using Common.Tasks;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// A mock access point that does nothing (used for testing). Points to a <see cref="Capabilities.FileType"/>.
    /// </summary>
    [XmlType("mock", Namespace = AppList.XmlNamespace)]
    public class MockAccessPoint : DefaultAccessPoint, IEquatable<MockAccessPoint>
    {
        #region Properties
        /// <summary>
        /// An indentifier that controls the result of <see cref="GetConflictIDs"/>.
        /// </summary>
        [XmlAttribute("id")]
        public string ID { get; set; }
        #endregion

        #region Conflict ID
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            return new[] {"mock:" + ID};
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, bool machineWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            // Trigger exceptions in case invalid capabilities are references
            appEntry.GetCapability<Capabilities.FileType>(Capability);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            // Trigger exceptions in case invalid capabilities are references
            appEntry.GetCapability<Capabilities.FileType>(Capability);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "MockAccessPoint: ID". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("MockAccessPoint: {0}", ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new MockAccessPoint {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID};
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
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
