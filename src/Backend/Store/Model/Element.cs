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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using NanoByte.Common.Collections;
using ZeroInstall.Store.Model.Design;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Model
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

        /// <summary>Set by user as a personal preference overriding other stability criteria.</summary>
        [XmlEnum("preferred")]
        Preferred,

        /// <summary>Indicates that an implementation is provided as a <see cref="PackageImplementation"/>.</summary>
        [XmlEnum("packaged")]
        Packaged,

        /// <summary>No serious problems.</summary>
        [XmlEnum("stable")]
        Stable,

        /// <summary>Any new release.</summary>
        [XmlEnum("testing")]
        Testing,

        /// <summary>More extreme version of <see cref="Testing"/>, expected to have bugs.</summary>
        [XmlEnum("developer")]
        Developer,

        /// <summary>Known bugs, none security-related.</summary>
        [XmlEnum("buggy")]
        Buggy,

        /// <summary>Known bugs, some or all security-related.</summary>
        [XmlEnum("insecure")]
        Insecure
    }
    #endregion

    /// <summary>
    /// Abstract base class for <see cref="ImplementationBase"/> and <see cref="Group"/>.
    /// Contains those parameters that can be transferred from a <see cref="Group"/> to an <see cref="Implementation"/>.
    /// </summary>
    [XmlType("element", Namespace = Feed.XmlNamespace)]
    public abstract class Element : TargetBase, IBindingContainer, IDependencyContainer, ICloneable
    {
        #region Constants
        /// <summary>
        /// The <see cref="string.Format(string,object[])"/> format used by <see cref="ReleasedString"/>
        /// </summary>
        public const string ReleaseDateFormat = "yyyy-MM-dd";
        #endregion

        /// <summary>
        /// The version number of the implementation.
        /// </summary>
        [Category("Release"), Description("The version number of the implementation.")]
        [XmlIgnore]
        public virtual ImplementationVersion Version { get; set; }

        #region XML serialization
        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string VersionString { get { return (Version == null ? null : Version.ToString()); } set { Version = string.IsNullOrEmpty(value) ? null : new ImplementationVersion(value); } }
        #endregion

        /// <seealso cref="VersionString"/>
        [Obsolete("Use VersionString instead")]
        [XmlIgnore, Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string VersionsString { get { return VersionString; } set { VersionString = value; } }

        /// <summary>
        /// A string to be appended to the version. The purpose of this is to allow complex version numbers (such as "1.0-rc2") in older versions of the injector.
        /// </summary>
        [Category("Release"), Description("A string to be appended to the version. The purpose of this is to allow complex version numbers (such as \"1.0-rc2\") in older versions of the injector.")]
        [XmlAttribute("version-modifier"), DefaultValue(""), CanBeNull]
        public string VersionModifier { get; set; }

        /// <summary>
        /// The date this implementation was made available. For development versions checked out from version control this attribute should not be present.
        /// </summary>
        [Category("Release"), Description("The date this implementation was made available. For development versions checked out from version control this attribute should not be present.")]
        [XmlIgnore]
        public virtual DateTime Released { get; set; }

        /// <summary>
        /// Used to store the unparsed release date string (instead of <see cref="Released"/>) if it <see cref="ModelUtils.ContainsTemplateVariables"/>.
        /// </summary>
        [CanBeNull]
        protected string ReleasedVerbatim;

        /// <summary>
        /// The string form of <see cref="Released"/>. Only use this if the string <see cref="ModelUtils.ContainsTemplateVariables"/>.
        /// </summary>
        /// <seealso cref="Released"/>
        [Category("Release"), Description("The string form of Released. Only use this if the string contains template variables.")]
        [XmlAttribute("released")]
        public virtual string ReleasedString
        {
            get
            {
                if (ReleasedVerbatim != null) return ReleasedVerbatim;
                else return (Released == default(DateTime) ? null : Released.ToString(ReleaseDateFormat));
            }
            set
            {
                if (ModelUtils.ContainsTemplateVariables(value)) ReleasedVerbatim = value;
                else Released = DateTime.ParseExact(value, ReleaseDateFormat, CultureInfo.InvariantCulture);
            }
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
        [TypeConverter(typeof(LicenseNameConverter))]
        [XmlAttribute("license"), DefaultValue("")]
        public string License { get; set; }

        /// <summary>
        /// The relative path of an executable inside the implementation that should be executed by default when the interface is run. If an implementation has no main setting, then it cannot be executed without specifying one manually. This typically means that the interface is for a library.
        /// </summary>
        /// <remarks>
        /// This is deprecated in favor of <see cref="Commands"/>.
        /// <see langword="null"/> corresponds to no <see cref="Command"/>s.
        /// An empty string corresponds to a <see cref="Command"/> with no <see cref="Command.Path"/>.
        /// </remarks>
        [Category("Execution"), Description("The relative path of an executable inside the implementation that should be executed by default when the interface is run. If an implementation has no main setting, then it cannot be executed without specifying one manually. This typically means that the interface is for a library.")]
        [XmlAttribute("main"), DefaultValue(""), CanBeNull]
        public string Main { get; set; }

        /// <summary>
        /// The relative path of an executable inside the implementation that can be executed to test the program. The program must be non-interactive (e.g. it can't open any windows or prompt for input). It should return with an exit status of zero if the tests pass. Any other status indicates failure.
        /// </summary>
        /// <remarks>
        /// This is deprecated in favor of <see cref="Commands"/>.
        /// <see langword="null"/> corresponds to no <see cref="Command"/>s.
        /// An empty string corresponds to a <see cref="Command"/> with no <see cref="Command.Path"/>.
        /// </remarks>
        [Category("Execution"), Description("The relative path of an executable inside the implementation that can be executed to test the program. The program must be non-interactive (e.g. it can't open any windows or prompt for input). It should return with an exit status of zero if the tests pass. Any other status indicates failure.")]
        [XmlAttribute("self-test"), DefaultValue("")]
        public string SelfTest { get; set; }

        /// <summary>
        /// The relative path of a directory inside the implementation that contains the package's documentation. This is the directory that would end up inside /usr/share/doc on a traditional Linux system.
        /// </summary>
        [Category("Execution"), Description("The relative path of a directory inside the implementation that contains the package's documentation. This is the directory that would end up inside /usr/share/doc on a traditional Linux system.")]
        [XmlAttribute("doc-dir"), DefaultValue("")]
        public string DocDir { get; set; }

        private readonly List<Dependency> _dependencies = new List<Dependency>();

        /// <summary>
        /// A list of interfaces this implementation depends upon.
        /// </summary>
        [Browsable(false)]
        [XmlElement("requires")]
        public List<Dependency> Dependencies { get { return _dependencies; } }

        private readonly List<Restriction> _restrictions = new List<Restriction>();

        /// <summary>
        /// A list of interfaces that are restricted to specific versions when used.
        /// </summary>
        [Browsable(false)]
        [XmlElement("restricts")]
        public List<Restriction> Restrictions { get { return _restrictions; } }

        private readonly List<Binding> _bindings = new List<Binding>();

        /// <summary>
        /// A list of <see cref="Binding"/>s for <see cref="Implementation"/>s to locate <see cref="Dependency"/>s.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(GenericBinding)), XmlElement(typeof(EnvironmentBinding)), XmlElement(typeof(OverlayBinding)), XmlElement(typeof(ExecutableInVar)), XmlElement(typeof(ExecutableInPath))]
        public List<Binding> Bindings { get { return _bindings; } }

        private readonly List<Command> _commands = new List<Command>();

        /// <summary>
        /// A list of commands that can be used to launch this implementation.
        /// </summary>
        /// <remarks>This will eventually replace <see cref="Main"/> and <see cref="SelfTest"/>.</remarks>
        [Browsable(false)]
        [XmlElement("command"), NotNull]
        public List<Command> Commands { get { return _commands; } }

        //--------------------//

        #region Normalize
        /// <summary>
        /// Sets missing default values and handles legacy elements.
        /// </summary>
        /// <param name="feedUri">The feed the data was originally loaded from; can be <see langword="null"/>.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing. Do not call it if you plan on serializing the feed again since it may loose some of its structure.</remarks>
        public virtual void Normalize([CanBeNull] FeedUri feedUri = null)
        {
            #region Sanity checks
            if (feedUri == null) throw new ArgumentNullException("feedUri");
            #endregion

            // Apply if-0install-version filter
            Commands.RemoveAll(FilterMismatch);
            Dependencies.RemoveAll(FilterMismatch);
            Restrictions.RemoveAll(FilterMismatch);
            Bindings.RemoveAll(FilterMismatch);

            // Convert legacy launch commands
            if (Main != null) Commands.Add(new Command {Name = Command.NameRun, Path = Main});
            if (SelfTest != null) Commands.Add(new Command {Name = Command.NameTest, Path = SelfTest});

            foreach (var command in Commands) command.Normalize();
            foreach (var dependency in Dependencies) dependency.Normalize();
            foreach (var restriction in Restrictions) restriction.Normalize();
        }

        /// <summary>
        /// Transfers attributes from another <see cref="Element"/> object to this one.
        /// Existing values are not replaced. Provides an inheritance-like relation.
        /// </summary>
        /// <param name="parent">The object to take the attributes from.</param>
        internal void InheritFrom([NotNull] Element parent)
        {
            #region Sanity checks
            if (parent == null) throw new ArgumentNullException("parent");
            #endregion

            // Check if values are unset and need inheritance)
            if (Version == null) Version = parent.Version;
            if (VersionModifier == null) VersionModifier = parent.VersionModifier;
            if (Released == default(DateTime)) Released = parent.Released;
            if (Main == null) Main = parent.Main;
            if (SelfTest == null) SelfTest = parent.SelfTest;
            if (DocDir == null) DocDir = parent.DocDir;
            if (License == null) License = parent.License;
            if (Stability == Stability.Unset) Stability = parent.Stability;
            if (Languages.Count == 0) Languages = new LanguageSet(parent.Languages);
            if (Architecture == default(Architecture)) Architecture = parent.Architecture;

            // Accumulate list entries
            Commands.AddRange(parent.Commands);
            Dependencies.AddRange(parent.Dependencies);
            Restrictions.AddRange(parent.Restrictions);
            Bindings.AddRange(parent.Bindings);
        }
        #endregion

        #region Query
        /// <summary>
        /// Determines whether <see cref="Commands"/> contains a <see cref="Command"/> with a specific name.
        /// </summary>
        /// <param name="name">The <see cref="Command.Name"/> to look for; <see cref="string.Empty"/> for none.</param>
        /// <returns><see langword="true"/> if a matching command was found or if <paramref name="name"/> is <see cref="string.Empty"/>; <see langword="false"/> otherwise.</returns>
        public bool ContainsCommand(string name)
        {
            #region Sanity checks
            if (name == null) throw new ArgumentNullException("name");
            #endregion

            if (name.Length == 0) return true;
            return Commands.Select(command => command.Name).Contains(name);
        }

        /// <summary>
        /// Returns the <see cref="Command"/> with a specific name.
        /// </summary>
        /// <param name="name">The <see cref="Command.Name"/> to look for; <see cref="string.Empty"/> for none.</param>
        /// <returns>The first matching command; <see langword="null"/> if <paramref name="name"/> is <see cref="string.Empty"/>.</returns>
        /// <exception cref="KeyNotFoundException">No matching <see cref="Command"/> was found.</exception>
        /// <remarks>Should only be called after <see cref="Normalize"/> has been called, otherwise nested <see cref="Implementation"/>s will not be considered.</remarks>
        [CanBeNull]
        public Command this[[NotNull] string name]
        {
            get
            {
                #region Sanity checks
                if (name == null) throw new ArgumentNullException("name");
                #endregion

                if (name.Length == 0) return null;
                try
                {
                    return Commands.First(command => command != null && command.Name == name);
                }
                    #region Error handling
                catch (InvalidOperationException)
                {
                    throw new KeyNotFoundException(string.Format(Resources.CommandNotFound, name));
                }
                #endregion
            }
        }

        /// <summary>
        /// Returns the <see cref="Command"/> with a specific name. Safe for missing elements.
        /// </summary>
        /// <param name="name">The <see cref="Command.Name"/> to look for.</param>
        /// <returns>The first matching command; <see langword="null"/> if no matching one was found.</returns>
        /// <remarks>Should only be called after <see cref="Normalize"/> has been called, otherwise nested <see cref="Implementation"/>s will not be considered.</remarks>
        [CanBeNull]
        public Command GetCommand([NotNull] string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(name);
            #endregion

            return Commands.FirstOrDefault(command => command != null && command.Name == name);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Element"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Element"/>.</returns>
        public abstract Element Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Copies all known values from one instance to another. Helper method for instance cloning.
        /// </summary>
        protected static void CloneFromTo([NotNull] Element from, [NotNull] Element to)
        {
            #region Sanity checks
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");
            #endregion

            TargetBase.CloneFromTo(from, to);
            to.Version = from.Version;
            to.VersionModifier = from.VersionModifier;
            to.Released = from.Released;
            to.ReleasedVerbatim = from.ReleasedVerbatim;
            to.Stability = from.Stability;
            to.License = from.License;
            to.Main = from.Main;
            to.SelfTest = from.SelfTest;
            to.DocDir = from.DocDir;
            to.Commands.AddRange(from.Commands.CloneElements());
            to.Dependencies.AddRange(from.Dependencies.CloneElements());
            to.Restrictions.AddRange(from.Restrictions.CloneElements());
            to.Bindings.AddRange(from.Bindings.CloneElements());
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(Element other)
        {
            if (other == null) return false;
            return base.Equals(other) &&
                   other.Version == Version && other.VersionModifier == VersionModifier && other.Released == Released && other.ReleasedVerbatim == ReleasedVerbatim && other.License == License && other.Main == Main && other.SelfTest == SelfTest && other.DocDir == DocDir &&
                   Commands.SequencedEquals(other.Commands) && Dependencies.SequencedEquals(other.Dependencies) && Restrictions.SequencedEquals(other.Restrictions) && Bindings.SequencedEquals(other.Bindings);
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
                if (ReleasedVerbatim != null) result = (result * 397) ^ ReleasedVerbatim.GetHashCode();
                result = (result * 397) ^ (License ?? "").GetHashCode();
                result = (result * 397) ^ (Main ?? "").GetHashCode();
                result = (result * 397) ^ (SelfTest ?? "").GetHashCode();
                result = (result * 397) ^ (DocDir ?? "").GetHashCode();
                result = (result * 397) ^ Commands.GetSequencedHashCode();
                result = (result * 397) ^ Dependencies.GetSequencedHashCode();
                result = (result * 397) ^ Restrictions.GetSequencedHashCode();
                result = (result * 397) ^ Bindings.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
