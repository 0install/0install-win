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
    /// Make a chosen <see cref="Implementation"/> available as an executable path in an environment variable.
    /// </summary>
    [Description("Make a chosen implementation available as an executable path in an environment variable.")]
    [Serializable, XmlRoot("executable-in-var", Namespace = Feed.XmlNamespace), XmlType("executable-in-var", Namespace = Feed.XmlNamespace)]
    public sealed class ExecutableInVar : ExecutableInBinding, IEquatable<ExecutableInVar>
    {
        /// <summary>
        /// The name of the environment variable.
        /// </summary>
        [Description("The name of the environment variable.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        #region Conversion
        /// <summary>
        /// Returns the binding in the form "Name = Command". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} = {1}", Name, Command ?? Model.Command.NameRun);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ExecutableInVar"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ExecutableInVar"/>.</returns>
        public override Binding Clone()
        {
            return new ExecutableInVar {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Name = Name, Command = Command};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ExecutableInVar other)
        {
            if (other == null) return false;
            return base.Equals(other) && other.Name == Name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ExecutableInVar && Equals((ExecutableInVar)obj);
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
