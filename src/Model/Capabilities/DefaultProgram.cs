/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's ability to provide some service (e.g. default web-browser, default e-mail client, ...).
    /// </summary>
    [XmlType("default-program", Namespace = XmlNamespace)]
    public class DefaultProgram : VerbCapability, IEquatable<DefaultProgram>
    {
        #region Constants
        /// <summary>
        /// Canonical <see cref="Service"/> for web browsers.
        /// </summary>
        public const string ServiceInternet = "Internet";

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
        public override bool GlobalOnly { get { return true; } }

        /// <summary>
        /// The name of the service such as "Internet", "Mail", "Media" etc.. Always use a canonical name when possible.
        /// </summary>
        [Description("The name of the service such as \"Internet\", \"Mail\", \"Media\", etc..  Always use a canonical name when possible.")]
        [XmlAttribute("service")]
        public string Service { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "DefaultProgram: Service (ID)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("DefaultProgram : {0} ({1})", Service, ID);
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability CloneCapability()
        {
            var capability = new DefaultProgram {ID = ID, Description = Description, Service = Service};
            capability.Icons.AddAll(Icons);
            capability.Verbs.AddAll(Verbs);
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
            return obj.GetType() == typeof(DefaultProgram) && Equals((DefaultProgram)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Service ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
