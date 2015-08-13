/*
 * Copyright 2010-2015 Bastian Eicher
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

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Make a chosen <see cref="Implementation"/> available as an executable in the search PATH.
    /// </summary>
    [Description("Make a chosen implementation available as an executable in the search PATH.")]
    [Serializable, XmlRoot("executable-in-path", Namespace = Feed.XmlNamespace), XmlType("executable-in-path", Namespace = Feed.XmlNamespace)]
    public sealed class ExecutableInPath : ExecutableInBinding, IEquatable<ExecutableInPath>
    {
        #region Properties
        /// <summary>
        /// The name of the executable (without file extensions).
        /// </summary>
        [Description("The name of the executable (without file extensions).")]
        [XmlAttribute("name")]
        public string Name { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the binding in the form " Name = Command". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} = {1}", Name, Command ?? Model.Command.NameRun);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ExecutableInPath"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ExecutableInPath"/>.</returns>
        public override Binding Clone()
        {
            return new ExecutableInPath {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Name = Name, Command = Command};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ExecutableInPath other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Name == Name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ExecutableInPath && Equals((ExecutableInPath)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Name ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
