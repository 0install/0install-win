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
using System.Xml.Serialization;

namespace ZeroInstall.Model.Capabilities
{
    /// <summary>
    /// Represents an application's ability to act as default program of some kind (e.g. default web-browser, default e-mail client, ...).
    /// </summary>
    [XmlType("default-program", Namespace = XmlNamespace)]
    public class DefaultProgram : Capability, IEquatable<DefaultProgram>
    {
        #region Properties
        // ToDo
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the capability in the form "DefaultProgram". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("DefaultProgam");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override Capability CloneCapability()
        {
            return new DefaultProgram();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(DefaultProgram other)
        {
            if (other == null) return false;

            return true;
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
                return 0;
            }
        }
        #endregion
    }
}
