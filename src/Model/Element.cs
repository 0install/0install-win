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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    #region Enumerations
    /// <summary>
    /// A stability rating for an <see cref="Implementation"/>.
    /// </summary>
    public enum Stability
    {
        /// <summary>Inherit stabilitiy from <see cref="Group"/> or default to <see cref="Testing"/></summary>
        [XmlIgnore]
        Unset,

        /// <summary>Set by user as a personal preference</summary>
        [XmlEnum("preferred")]
        Preferred,

        /// <summary>No serious problems</summary>
        [XmlEnum("stable")]
        Stable,

        /// <summary>Any new release</summary>
        [XmlEnum("testing")]
        Testing,

        /// <summary>More extreme version of <see cref="Testing"/>, expected to have bugs</summary>
        [XmlEnum("developer")]
        Developer,

        /// <summary>Known bugs, none security-related</summary>
        [XmlEnum("buggy")]
        Buggy,

        /// <summary>Known bugs, some ore all security-related</summary>
        [XmlEnum("insecure")]
        Insecure
    }
    #endregion

    /// <summary>
    /// Abstract base class for <see cref="ImplementationBase"/> and <see cref="Group"/>.
    /// Contains those parameters that can be transferred from a <see cref="Group"/> to an <see cref="Implementation"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlType("element", Namespace = Feed.XmlNamespace)]
    public abstract class Element : TargetBase, IBindingContainer, IDependencyContainer, ISimplifyable, ICloneable
    {
        #region Constants
        /// <summary>
        /// The <see cref="string.Format(string,object[])"/> format used by <see cref="ReleasedString"/>
        /// </summary>
        public const string ReleaseDateFormat = "yyyy-MM-dd";
        #endregion

        #region Properties
        /// <summary>
        /// The version number of the implementation.
        /// </summary>
        [Category("Release"), Description("The version number of the implementation.")]
        [XmlIgnore]
        public virtual ImplementationVersion Version { get; set; }
        
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string VersionString
        {
            get { return (Version == null ? null : Version.ToString()); }
            set { Version = string.IsNullOrEmpty(value) ? null : new ImplementationVersion(value); }
        }

        /// <summary>
        /// A string to be appended to the version. The purpose of this is to allow complex version numbers (such as "1.0-rc2") in older versions of the injector.
        /// </summary>
        [Category("Release"), Description("A string to be appended to the version. The purpose of this is to allow complex version numbers (such as \"1.0-rc2\") in older versions of the injector.")]
        [XmlAttribute("version-modifier"), DefaultValue("")]
        public string VersionModifier { get; set; }

        /// <summary>
        /// The date this implementation was made available. For development versions checked out from version control this attribute should not be present.
        /// </summary>
        [Category("Release"), Description("The date this implementation was made available. For development versions checked out from version control this attribute should not be present.")]
        [XmlIgnore]
        public virtual DateTime Released { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Released"/>
        [XmlAttribute("released"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public virtual string ReleasedString
        {
            get { return (Released == default(DateTime) ? null : Released.ToString(ReleaseDateFormat)); }
            set { Released = DateTime.ParseExact(value, ReleaseDateFormat, CultureInfo.InvariantCulture); }
        }

        private Stability _stability = Stability.Unset;
        /// <summary>
        /// The default stability rating for this implementation.
        /// </summary>
        [Category("Release"), Description("The default stability rating for this implementation.")]
        [XmlAttribute("stability"), DefaultValue(typeof(Stability), "Unset")]
        public virtual Stability Stability { get { return _stability; } set { _stability = value; } }

        /// <summary>
        /// License terms (typically a Trove category, as used on freshmeat.net).
        /// </summary>
        [Category("Release"), Description("License terms (typically a Trove category, as used on freshmeat.net).")]
        [XmlAttribute("license"), DefaultValue("")]
        public string License { get; set; }

        /// <summary>
        /// The relative path of an executable inside the implementation that should be executed by default when the interface is run. If an implementation has no main setting, then it cannot be executed without specifying one manually. This typically means that the interface is for a library.
        /// </summary>
        /// <remarks>This will be deprecated in favor of <see cref="Commands"/>.</remarks>
        [Category("Execution"), Description("The relative path of an executable inside the implementation that should be executed by default when the interface is run. If an implementation has no main setting, then it cannot be executed without specifying one manually. This typically means that the interface is for a library.")]
        [XmlAttribute("main"), DefaultValue("")]
        public string Main { get; set; }

        /// <summary>
        /// The relative path of an executable inside the implementation that can be executed to test the program. The program must be non-interactive (e.g. it can't open any windows or prompt for input). It should return with an exit status of zero if the tests pass. Any other status indicates failure.
        /// </summary>
        /// <remarks>This will be deprecated in favor of <see cref="Commands"/>.</remarks>
        [Category("Execution"), Description("The relative path of an executable inside the implementation that can be executed to test the program. The program must be non-interactive (e.g. it can't open any windows or prompt for input). It should return with an exit status of zero if the tests pass. Any other status indicates failure.")]
        [XmlAttribute("self-test"), DefaultValue("")]
        public string SelfTest { get; set; }

        /// <summary>
        /// The relative path of a directory inside the implementation that contains the package's documentation. This is the directory that would end up inside /usr/share/doc on a traditional Linux system.
        /// </summary>
        [Category("Execution"), Description("The relative path of a directory inside the implementation that contains the package's documentation. This is the directory that would end up inside /usr/share/doc on a traditional Linux system.")]
        [XmlAttribute("doc-dir"), DefaultValue("")]
        public string DocDir { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Dependency> _dependencies = new C5.ArrayList<Dependency>();
        /// <summary>
        /// A list of interfaces this implementation depends upon.
        /// </summary>
        [Category("Execution"), Description("A list of interfaces this implementation depends upon.")]
        [XmlElement("requires")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<Dependency> Dependencies { get { return _dependencies; } }

        // Preserve order
        private readonly C5.ArrayList<Binding> _bindings = new C5.ArrayList<Binding>();
        /// <summary>
        /// A list of <see cref="Binding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Description("A list of bindings for implementations to locate dependencies.")]
        [XmlElement(typeof(EnvironmentBinding)), XmlElement(typeof(OverlayBinding))]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<Binding> Bindings { get { return _bindings; } }

        // Preserve order
        private readonly C5.ArrayList<Command> _commands = new C5.ArrayList<Command>();
        /// <summary>
        /// A list of commands that can be used to launch this implementation.
        /// </summary>
        /// <remarks>This will eventually replace <see cref="Main"/> and <see cref="SelfTest"/>.</remarks>
        [Category("Execution"), Description("A list of commands that can be used to launch this implementation.")]
        [XmlElement("command")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.ArrayList<Command> Commands { get { return _commands; } }
        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Sets missing default values and handles legacy elements.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the interface again since it will may some of its structure.</remarks>
        public virtual void Simplify()
        {
            // Convert legacy launch commands
            if (Commands.IsEmpty)
            {
                if (!string.IsNullOrEmpty(Main)) Commands.Add(new Command {Name = Command.NameRun, Path = Main});
                if (!string.IsNullOrEmpty(SelfTest)) Commands.Add(new Command {Name = Command.NameTest, Path = SelfTest});
            }
        }

        /// <summary>
        /// Transfers attributes from another <see cref="Element"/> object to this one.
        /// Existing values are not replaced. Provides an inheritance-like relation.
        /// </summary>
        /// <param name="parent">The object to take the attributes from.</param>
        internal void InheritFrom(Element parent)
        {
            // Check if values are unset and need inheritance)
            if (Version == null) Version = parent.Version;
            if (VersionModifier == null) VersionModifier = parent.VersionModifier;
            if (Released == default(DateTime)) Released = parent.Released;
            if (Main == null) Main = parent.Main;
            if (SelfTest == null) SelfTest = parent.SelfTest;
            if (DocDir == null) DocDir = parent.DocDir;
            if (License == null) License = parent.License;
            if (Stability == Stability.Unset) Stability = parent.Stability;
            if (Languages.IsEmpty) Languages.AddAll(parent.Languages);
            if (Architecture == default(Architecture)) Architecture = parent.Architecture;

            // Accumulate list entries
            foreach (var command in parent.Commands) Commands.Add(command);
            foreach (var dependency in parent.Dependencies) Dependencies.Add(dependency);
            foreach (var bindings in parent.Bindings) Bindings.Add(bindings);
        }
        #endregion

        #region Query
        /// <summary>
        /// Returns the <see cref="Command"/> with a specific name.
        /// </summary>
        /// <param name="name">The <see cref="Command.Name"/> to look for. Well-known names are <see cref="Command.NameRun"/>, <see cref="Command.NameTest"/> and <see cref="Command.NameCompile"/>.</param>
        /// <returns>The identified <see cref="Command"/> or <see langword="null"/> no matching one was found.</returns>
        /// <remarks>Should only be called after <see cref="Simplify"/> has been called, otherwise nested <see cref="Implementation"/>s will be missed.</remarks>
        public Command GetCommand(string name)
        {
            foreach (var command in Commands)
            {
                if (command != null && command.Name == name) return command;
            }

            return null;
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Element"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Element"/>.</returns>
        public abstract Element CloneElement();

        /// <summary>
        /// Creates a deep copy of this <see cref="Element"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Element"/>.</returns>
        public object Clone()
        {
            return CloneElement();
        }

        /// <summary>
        /// Copies all known values from one instance to another. Helper method for instance cloning.
        /// </summary>
        protected static void CloneFromTo(Element from, Element to)
        {
            #region Sanity checks
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");
            #endregion

            TargetBase.CloneFromTo(from, to);
            to.Version = from.Version;
            to.VersionModifier = from.VersionModifier;
            to.Released = from.Released;
            to.Stability = from.Stability;
            to.License = from.License;
            to.Main = from.Main;
            to.SelfTest = from.SelfTest;
            to.DocDir = from.DocDir;
            foreach (var command in from.Commands) to.Commands.Add(command.CloneCommand());
            foreach (var dependency in from.Dependencies) to.Dependencies.Add(dependency.CloneDependency());
            foreach (var binding in from.Bindings) to.Bindings.Add(binding.CloneBinding());
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(Element other)
        {
            if (other == null) return false;

            return base.Equals(other) &&
                Equals(other.Version, Version) && other.VersionModifier == VersionModifier && other.Released == Released && other.License == License && other.Main == Main && other.SelfTest == SelfTest && other.DocDir == DocDir &&
                Commands.SequencedEquals(other.Commands) && Dependencies.SequencedEquals(other.Dependencies) && Bindings.SequencedEquals(other.Bindings);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                result = (result * 397) ^ (VersionModifier != null ? VersionModifier.GetHashCode() : 0);
                result = (result * 397) ^ Released.GetHashCode();
                result = (result * 397) ^ (License ?? "").GetHashCode();
                result = (result * 397) ^ (Main ?? "").GetHashCode();
                result = (result * 397) ^ (SelfTest ?? "").GetHashCode();
                result = (result * 397) ^ (DocDir ?? "").GetHashCode();
                result = (result * 397) ^ Commands.GetSequencedHashCode();
                result = (result * 397) ^ Dependencies.GetSequencedHashCode();
                result = (result * 397) ^ Bindings.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
