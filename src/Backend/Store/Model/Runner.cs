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
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// A special kind of dependency: the program that is used to run this one. For example, a Python program might specify Python as its runner.
    /// </summary>
    /// <seealso cref="Model.Command.Runner"/>
    [Description("A special kind of dependency: the program that is used to run this one. For example, a Python program might specify Python as its runner.")]
    [Serializable, XmlRoot("runner", Namespace = Feed.XmlNamespace), XmlType("runner", Namespace = Feed.XmlNamespace)]
    public class Runner : Dependency, IArgBaseContainer, IEquatable<Runner>
    {
        /// <summary>
        /// The name of the command in the <see cref="Restriction.InterfaceUri"/> to use; leave <c>null</c> for <see cref="Store.Model.Command.NameRun"/>.
        /// </summary>
        [Description("The name of the command in the interface to use; leave empty for 'run'.")]
        [TypeConverter(typeof(CommandNameConverter))]
        [XmlAttribute("command"), DefaultValue(""), CanBeNull]
        public string Command { get; set; }

        private readonly List<ArgBase> _arguments = new List<ArgBase>();

        /// <summary>
        /// A list of command-line arguments to be passed to the runner before the path of the implementation.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Arg)), XmlElement(typeof(ForEachArgs))]
        public List<ArgBase> Arguments { get { return _arguments; } }

        #region Conversion
        /// <summary>
        /// Returns the runner in the form "Interface (Command)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", InterfaceUri, Command ?? Model.Command.NameRun);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Runner"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Runner"/>.</returns>
        public Runner CloneRunner()
        {
            var runner = new Runner {InterfaceUri = InterfaceUri, Use = Use, Command = Command, Versions = Versions};
            runner.Bindings.AddRange(Bindings.CloneElements());
            runner.Constraints.AddRange(Constraints.CloneElements());
            runner.Arguments.AddRange(Arguments.CloneElements());
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
            if (obj == null) return false;
            if (obj == this) return true;
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
