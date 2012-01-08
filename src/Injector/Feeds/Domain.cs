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
using System.Xml.Serialization;
using ZeroInstall.Injector.Properties;

namespace ZeroInstall.Injector.Feeds
{
    /// <summary>
    /// A domain-name associated to a <see cref="Domain"/>.
    /// </summary>
    [XmlType("domain", Namespace = TrustDB.XmlNamespace)]
    public struct Domain : ICloneable, IEquatable<Domain>
    {
        #region Properties
        private string _value;

        /// <summary>
        /// A valid domain name (not a full <see cref="Uri"/>!).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the value is not a DNS-style hostname.</exception>
        [XmlAttribute("value")]
        public string Value
        {
            get { return _value; }
            set
            {
                #region Sanity checks
                if (Uri.CheckHostName(value) != UriHostNameType.Dns) throw new ArgumentException(Resources.NotValidDomain, "value");
                #endregion

                _value = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new domain entry.
        /// </summary>
        /// <param name="value">A valid domain name (not a full <see cref="Uri"/>!).</param>
        /// <exception cref="ArgumentException">Thrown if the value is not a DNS-style hostname.</exception>
        public Domain(string value)
        {
            #region Sanity checks
            if (Uri.CheckHostName(value) != UriHostNameType.Dns) throw new ArgumentException(Resources.NotValidDomain, "value");
            #endregion

            _value = value;
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            return Value;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Domain"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Domain"/>.</returns>
        public Domain Clone()
        {
            return new Domain {Value = Value};
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Domain other)
        {
            return other.Value == Value;
        }

        /// <inheritdoc/>
        public static bool operator ==(Domain left, Domain right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(Domain left, Domain right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj.GetType() == typeof(Domain) && Equals((Domain)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Value ?? "").GetHashCode();
        }
        #endregion
    }
}
