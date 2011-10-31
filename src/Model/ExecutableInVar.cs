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

namespace ZeroInstall.Model
{
    /// <summary>
    /// Make a chosen <see cref="Implementation"/> available as an executable path in an environment variable.
    /// </summary>
    [Serializable]
    [XmlType("executable-in-var", Namespace = Feed.XmlNamespace)]
    public sealed class ExecutableInVar : Binding, IEquatable<ExecutableInVar>
    {
        #region Properties
        /// <summary>
        /// The name of the environment variable.
        /// </summary>
        [Description("The name of the environment variable.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The <see cref="Command"/> in the <see cref="Implementation"/> to launch; leave <see langword="null"/> for <see cref="Model.Command.NameRun"/>.
        /// </summary>
        [Description("The command in the implementation to launch; leave null for 'run'.")]
        [XmlAttribute("command")]
        public string Command { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the binding in the form "ExecutableInVar: Name = Command". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("ExecutableInVar: {0} = {1}", Name, Command);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="ExecutableInVar"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="ExecutableInVar"/>.</returns>
        public override Binding CloneBinding()
        {
            return new ExecutableInVar {Name = Name, Command = Command};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ExecutableInVar other)
        {
            if (other == null) return false;

            return other.Name == Name || other.Command == Command;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ExecutableInVar) && Equals((ExecutableInVar)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Name != null ? Name.GetHashCode() : 0);
                result = (result * 397) ^ (Command ?? "").GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
