/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Collections;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A reference to an interface that is required by an <see cref="Command"/> as a runner.
    /// </summary>
    /// <seealso cref="Model.Command.Runner"/>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlRoot("runner", Namespace = Feed.XmlNamespace), XmlType("runner", Namespace = Feed.XmlNamespace)]
    public class Runner : Dependency, IArgBaseContainer, IEquatable<Runner>
    {
        #region Properties
        /// <summary>
        /// The name of the command in the <see cref="Restriction.Interface"/> to use.
        /// </summary>
        [Description("The name of the command in the interface to use.")]
        [XmlAttribute("command")]
        public string Command { get; set; }

        // Preserve order
        private readonly C5.ArrayList<ArgBase> _arguments = new C5.ArrayList<ArgBase>();

        /// <summary>
        /// A list of command-line arguments to be passed to the runner before the path of the implementation.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Arg)), XmlElement(typeof(ForEachArgs))]
        public C5.ArrayList<ArgBase> Arguments { get { return _arguments; } }
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
            runner.Bindings.AddAll(Bindings.CloneElements());
            runner.Constraints.AddAll(Constraints.CloneElements());
            runner.Arguments.AddAll(Arguments.CloneElements());
            return runner;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Runner"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Runner"/>.</returns>
        public override Restriction Clone()
        {
            return CloneRunner();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Runner other)
        {
            if (other == null) return false;
            if (!base.Equals(other)) return false;
            if (Command != other.Command) return false;
            if (!Arguments.SequencedEquals(other.Arguments)) return false;
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
                int result = base.GetHashCode();
                result = (result * 397) ^ (Command ?? "").GetHashCode();
                result = (result * 397) ^ Arguments.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
