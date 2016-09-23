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
using System.ComponentModel;
using System.Xml.Serialization;
using NanoByte.Common.Native;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// Indicates that an application should be listed in the "Set your Default Programs" UI (Windows Vista and later).
    /// </summary>
    /// <remarks>The actual integration information is pulled from the other <see cref="Capability"/>s.</remarks>
    [Description("Indicates that an application should be listed in the \"Set your Default Programs\" UI (Windows Vista and later).")]
    [Serializable, XmlRoot("registration", Namespace = CapabilityList.XmlNamespace), XmlType("registration", Namespace = CapabilityList.XmlNamespace)]
    public sealed class AppRegistration : Capability, IEquatable<AppRegistration>
    {
        /// <inheritdoc/>
        public override bool WindowsMachineWideOnly => !WindowsUtils.IsWindows8;

        /// <summary>
        /// The registry path relative to HKEY_LOCAL_MACHINE which is used to store the application's capability registration information.
        /// </summary>
        [Description("The registry path relative to HKEY_LOCAL_MACHINE which is used to store the application's capability registration information.")]
        [XmlAttribute("capability-reg-path")]
        public string CapabilityRegPath { get; set; }

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs => new[] {"registered-apps:" + ID, "hklm:" + CapabilityRegPath};

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "CapabilityRegPath". Not safe for parsing!
        /// </summary>
        public override string ToString() => CapabilityRegPath;
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone() => new AppRegistration {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID, CapabilityRegPath = CapabilityRegPath};
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(AppRegistration other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.CapabilityRegPath == CapabilityRegPath;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj == this) return true;
            return obj is AppRegistration && Equals((AppRegistration)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (CapabilityRegPath ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
