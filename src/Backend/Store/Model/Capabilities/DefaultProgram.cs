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
using System.ComponentModel;
using System.Xml.Serialization;
using NanoByte.Common.Collections;

namespace ZeroInstall.Store.Model.Capabilities
{
    /// <summary>
    /// Can act as the default provider for a well-known service such web-browser, e-mail client.
    /// </summary>
    [Description("Can act as the default provider for a well-known service such web-browser, e-mail client.")]
    [Serializable]
    [XmlType("default-program", Namespace = CapabilityList.XmlNamespace)]
    public sealed class DefaultProgram : VerbCapability, IEquatable<DefaultProgram>
    {
        #region Constants
        /// <summary>
        /// Canonical <see cref="Service"/> for web browsers.
        /// </summary>
        public const string ServiceInternet = "StartMenuInternet";

        /// <summary>
        /// Canonical <see cref="Service"/> for mail clients.
        /// </summary>
        public const string ServiceMail = "Mail";

        /// <summary>
        /// Canonical <see cref="Service"/> for media players.
        /// </summary>
        public const string ServiceMedia = "Media";

        /// <summary>
        /// Canonical <see cref="Service"/> for instant messengers.
        /// </summary>
        public const string ServiceMessenger = "IM";

        /// <summary>
        /// Canonical <see cref="Service"/> for Java Virtual Machines.
        /// </summary>
        public const string ServiceJava = "JVM";

        /// <summary>
        /// Canonical <see cref="Service"/> for calender tools.
        /// </summary>
        public const string ServiceCalender = "Calender";

        /// <summary>
        /// Canonical <see cref="Service"/> for address books.
        /// </summary>
        public const string ServiceContacts = "Contacts";

        /// <summary>
        /// Canonical <see cref="Service"/> for internet call tools.
        /// </summary>
        public const string ServiceInternetCall = "Internet Call";
        #endregion

        #region Properties
        /// <inheritdoc/>
        [XmlIgnore]
        public override bool WindowsMachineWideOnly { get { return true; } }

        /// <summary>
        /// The name of the service (e.g. "StartMenuInternet", "Mail", "Media"). Always use a canonical name when possible.
        /// </summary>
        [Description("The name of the service (e.g. \"StartMenuInternet\", \"Mail\", \"Media\"). Always use a canonical name when possible.")]
        [XmlAttribute("service")]
        public string Service { get; set; }

        /// <summary>
        /// Lists the commands the application registeres for use by Windows' "Set Program Access and Defaults". Will be transparently replaced with Zero Install commands at runtime.
        /// </summary>
        /// <remarks>These strings are used for registry filtering. They are never actually executed.</remarks>
        [Description("Lists the commands the application registeres for use by Windows' \"Set Program Access and Defaults\". Will be transparently replaced with Zero Install commandss at runtime.")]
        [XmlElement("install-commands")]
        public InstallCommands InstallCommands { get; set; }

        /// <inheritdoc/>
        [XmlIgnore]
        public override IEnumerable<string> ConflictIDs { get { return new[] {"clients:" + Service + @"\" + ID}; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "Service (ID)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Service, ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability Clone()
        {
            var capability = new DefaultProgram {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, ID = ID, ExplicitOnly = ExplicitOnly, Service = Service};
            capability.Descriptions.AddRange(Descriptions.CloneElements());
            capability.Icons.AddRange(Icons);
            capability.Verbs.AddRange(Verbs.CloneElements());
            return capability;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(DefaultProgram other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Service == Service;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is DefaultProgram && Equals((DefaultProgram)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Service ?? "").GetHashCode();
            }
        }
        #endregion
    }
}
