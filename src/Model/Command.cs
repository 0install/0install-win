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
    /// A command says how to run an <see cref="Implementation"/> as a program.
    /// </summary>
    /// <seealso cref="Element.Commands"/>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlRoot("command", Namespace = Feed.XmlNamespace), XmlType("command", Namespace = Feed.XmlNamespace)]
    public class Command : FeedElement, IArgBaseContainer, IBindingContainer, IDependencyContainer, ICloneable, IEquatable<Command>
    {
        #region Constants
        /// <summary>
        /// Canonical <see cref="Name"/> coressponding to <see cref="Element.Main"/>.
        /// </summary>
        public const string NameRun = "run";

        /// <summary>
        /// Canonical <see cref="Name"/> coressponding to <see cref="Element.SelfTest"/>.
        /// </summary>
        public const string NameTest = "test";

        /// <summary>
        /// Canonical <see cref="Name"/> used by 0compile.
        /// </summary>
        public const string NameCompile = "compile";
        #endregion

        #region Properties
        /// <summary>
        /// The name of the command. Well-known names are <see cref="NameRun"/>, <see cref="NameTest"/> and <see cref="NameCompile"/>.
        /// </summary>
        [Description("The name of the command.")]
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The relative path of an executable inside the implementation that should be executed to run this command.
        /// </summary>
        [Description("The relative path of an executable inside the implementation that should be executed to run this command.")]
        [XmlAttribute("path")]
        public string Path { get; set; }

        // Preserve order
        private readonly C5.ArrayList<ArgBase> _arguments = new C5.ArrayList<ArgBase>();

        /// <summary>
        /// A list of command-line arguments to be passed to an implementation executable.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Arg)), XmlElement(typeof(ForEachArgs))]
        public C5.ArrayList<ArgBase> Arguments { get { return _arguments; } }

        // Preserve order
        private readonly C5.ArrayList<Binding> _bindings = new C5.ArrayList<Binding>();

        /// <summary>
        /// A list of <see cref="Binding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(GenericBinding)), XmlElement(typeof(EnvironmentBinding)), XmlElement(typeof(OverlayBinding)), XmlElement(typeof(ExecutableInVar)), XmlElement(typeof(ExecutableInPath))]
        public C5.ArrayList<Binding> Bindings { get { return _bindings; } }

        /// <summary>
        /// Switches the working directory of the process on startup to a location within the <see cref="Model.Implementation"/>.
        /// </summary>
        [Browsable(false)]
        [XmlElement("working-dir")]
        public WorkingDir WorkingDir { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Dependency> _dependencies = new C5.ArrayList<Dependency>();

        /// <summary>
        /// A list of interfaces this command depends upon.
        /// </summary>
        [Browsable(false)]
        [XmlElement("requires")]
        public C5.ArrayList<Dependency> Dependencies { get { return _dependencies; } }

        // Preserve order
        private readonly C5.ArrayList<Restriction> _restrictions = new C5.ArrayList<Restriction>();

        /// <summary>
        /// A list of interfaces that are restricted to specific versions when used.
        /// </summary>
        [Browsable(false)]
        [XmlElement("restricts")]
        public C5.ArrayList<Restriction> Restrictions { get { return _restrictions; } }

        /// <summary>
        /// An interface that needs be used as a runner for this command. The <see cref="Path"/> is passed to that interface as an argument.
        /// </summary>
        /// <remarks>Usefull for launching things like Java JARs or Python scripts.</remarks>
        [Browsable(false)]
        [XmlElement("runner")]
        public Runner Runner { get; set; }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the Command in the form "Name (Path)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return Name + " (" + Path + ")";
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Command"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Command"/>.</returns>
        public Command Clone()
        {
            var newCommand = new Command {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, IfZeroInstallVersion = IfZeroInstallVersion, Name = Name, Path = Path};
            newCommand.Arguments.AddAll(Arguments.CloneElements());
            newCommand.Bindings.AddAll(Bindings.CloneElements());
            if (WorkingDir != null) newCommand.WorkingDir = WorkingDir.Clone();
            newCommand.Dependencies.AddAll(Dependencies.CloneElements());
            newCommand.Restrictions.AddAll(Restrictions.CloneElements());
            if (Runner != null) newCommand.Runner = Runner.CloneRunner();

            return newCommand;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Command other)
        {
            if (other == null) return false;
            if (!base.Equals(other)) return false;
            if (Name != other.Name) return false;
            if (Path != other.Path) return false;
            if (!Arguments.SequencedEquals(other.Arguments)) return false;
            if (!Bindings.SequencedEquals(other.Bindings)) return false;
            if (!Equals(WorkingDir, other.WorkingDir)) return false;
            if (!Dependencies.SequencedEquals(other.Dependencies)) return false;
            if (!Restrictions.SequencedEquals(other.Restrictions)) return false;
            if (!Equals(Runner, other.Runner)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Command) && Equals((Command)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Name ?? "").GetHashCode();
                result = (result * 397) ^ (Path ?? "").GetHashCode();
                result = (result * 397) ^ Arguments.GetSequencedHashCode();
                result = (result * 397) ^ Bindings.GetSequencedHashCode();
                if (WorkingDir != null) result = (result * 397) ^ WorkingDir.GetHashCode();
                result = (result * 397) ^ Dependencies.GetSequencedHashCode();
                result = (result * 397) ^ Restrictions.GetSequencedHashCode();
                if (Runner != null) result = (result * 397) ^ Runner.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
