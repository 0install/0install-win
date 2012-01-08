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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A reference to an interface that is required by an <see cref="Command"/> as a runner.
    /// </summary>
    /// <seealso cref="Model.Command.Runner"/>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlType("runner", Namespace = Feed.XmlNamespace)]
    public class Runner : Dependency, IArgsContainer, IEquatable<Runner>
    {
        #region Properties
        /// <summary>
        /// The name of the command in the <see cref="Dependency.Interface"/> to use.
        /// </summary>
        [Description("The name of the command in the interface to use.")]
        [XmlAttribute("command")]
        public string Command { get; set; }

        // Preserve order
        private readonly C5.LinkedList<string> _arguments = new C5.LinkedList<string>();

        /// <summary>
        /// A list of command-line arguments to be passed to the executable.
        /// </summary>
        [Description("A list of command-line arguments to be passed to the executable.")]
        [XmlElement("arg")]
        public C5.LinkedList<string> Arguments { get { return _arguments; } }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the runner in the form "Runner: Interface (Command)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            string result = "Runner: " + Interface;
            if (!string.IsNullOrEmpty(Command)) result += " (" + Command + ")";
            return result;
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Runner"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Runner"/>.</returns>
        public Runner CloneRunner()
        {
            var runner = new Runner {Interface = Interface, Use = Use, Command = Command};
            foreach (var binding in Bindings) runner.Bindings.Add(binding.Clone());
            foreach (var constraint in Constraints) runner.Constraints.Add(constraint.Clone());
            foreach (var argument in Arguments) runner.Arguments.Add(argument);

            return runner;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Runner"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Runner"/>.</returns>
        public override Dependency Clone()
        {
            return CloneRunner();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Runner other)
        {
            if (other == null) return false;

            if (Interface != other.Interface) return false;
            if (Use != other.Use) return false;
            if (!Constraints.SequencedEquals(other.Constraints)) return false;
            if (!Bindings.SequencedEquals(other.Bindings)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Runner) && Equals((Runner)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Interface ?? "").GetHashCode();
                result = (result * 397) ^ (Use ?? "").GetHashCode();
                result = (result * 397) ^ Constraints.GetSequencedHashCode();
                result = (result * 397) ^ Bindings.GetSequencedHashCode();
                result = (result * 397) ^ Arguments.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
