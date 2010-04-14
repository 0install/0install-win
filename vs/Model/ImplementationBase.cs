/*
 * Copyright 2010 Bastian Eicher
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
using System.Text.RegularExpressions;

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
    /// A common base class for <see cref="Implementation"/> and <see cref="Group"/>.
    /// Contains those parameters that can be transferred from a <see cref="Group"/> to an <see cref="Implementation"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be dispoed.")]
    public abstract class ImplementationBase : TargetBase, IBindingContainer, ISimplifyable
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
        [XmlAttribute("version")]
        public virtual string Version { get; set; }

        /// <summary>
        /// A string to be appended to the version. The purpose of this is to allow complex version numbers (such as "1.0-rc2").
        /// </summary>
        [Category("Release"), Description("A string to be appended to the version. The purpose of this is to allow complex version numbers (such as \"1.0-rc2\").")]
        [XmlAttribute("version-modifier")]
        public string VersionModifier { get; set; }

        /// <summary>
        /// The date this implementation was made available. For development versions checked out from version control this attribute should not be present.
        /// </summary>
        [Category("Release"), Description("The date this implementation was made available. For development versions checked out from version control this attribute should not be present.")]
        [XmlIgnore]
        public virtual DateTime Released { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("released"), Browsable(false)]
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
        [XmlAttribute("license")]
        public string License { get; set; }

        /// <summary>
        /// The relative path of an executable inside the implementation that should be executed by default when the interface is run. If an implementation has no main setting, then it cannot be executed without specifying one manually. This typically means that the interface is for a library.
        /// </summary>
        [Category("Execution"), Description("The relative path of an executable inside the implementation that should be executed by default when the interface is run. If an implementation has no main setting, then it cannot be executed without specifying one manually. This typically means that the interface is for a library.")]
        [XmlAttribute("main")]
        public string Main { get; set; }

        /// <summary>
        /// The relative path of an executable inside the implementation that can be executed to test the program. The program must be non-interactive (e.g. it can't open any windows or prompt for input). It should return with an exit status of zero if the tests pass. Any other status indicates failure.
        /// </summary>
        [Category("Execution"), Description("The relative path of an executable inside the implementation that can be executed to test the program. The program must be non-interactive (e.g. it can't open any windows or prompt for input). It should return with an exit status of zero if the tests pass. Any other status indicates failure.")]
        [XmlAttribute("self-test")]
        public string SelfTest { get; set; }

        /// <summary>
        /// The relative path of a directory inside the implementation that contains the package's documentation. This is the directory that would end up inside /usr/share/doc on a traditional Linux system.
        /// </summary>
        [Category("Execution"), Description("The relative path of a directory inside the implementation that contains the package's documentation. This is the directory that would end up inside /usr/share/doc on a traditional Linux system.")]
        [XmlAttribute("doc-dir")]
        public string DocDir { get; set; }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedLinkedList<Dependency> _dependencies = new C5.HashedLinkedList<Dependency>();
        /// <summary>
        /// A list of <see cref="Interface"/>s this implementation depends upon.
        /// </summary>
        [Category("Execution"), Description("A list of interfaces this implementation depends upon.")]
        [XmlElement("requires")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedLinkedList<Dependency> Dependencies { get { return _dependencies; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedLinkedList<EnvironmentBinding> _environmentBindings = new C5.HashedLinkedList<EnvironmentBinding>();
        /// <summary>
        /// A list of <see cref="EnvironmentBinding"/>s for this implementation to locate itself.
        /// </summary>
        [Category("Execution"), Description("A list of bindings for this implementation to locate itself.")]
        [XmlElement("environment")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedLinkedList<EnvironmentBinding> EnvironmentBindings { get { return _environmentBindings; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedLinkedList<OverlayBinding> _overlayBindings = new C5.HashedLinkedList<OverlayBinding>();
        /// <summary>
        /// A list of <see cref="OverlayBinding"/>s for this implementation to locate itself.
        /// </summary>
        [Category("Execution"), Description("A list of bindings for this implementation to locate itself.")]
        [XmlElement("overlay")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedLinkedList<OverlayBinding> OverlayBindings { get { return _overlayBindings; } }
        #endregion

        //--------------------//

        #region Simplify
        /// <summary>
        /// Sets missing default values, removes inheritance structures, etc.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Interface"/> again since it will may some of its structure.</remarks>
        public abstract void Simplify();

        /// <summary>
        /// Transfers attributes from another <see cref="ImplementationBase"/> object to this one.
        /// Existing values are not replaced. Provides an inheritance-like relation.
        /// </summary>
        /// <param name="parent">The object to take the attributes from.</param>
        internal void InheritFrom(ImplementationBase parent)
        {
            // Check if values are unset and need inheritance
            if (Version == null) Version = parent.Version;
            if (VersionModifier == null) VersionModifier = parent.VersionModifier;
            if (Released == default(DateTime)) Released = parent.Released;
            if (Main == null) Main = parent.Main;
            if (SelfTest == null) SelfTest = parent.SelfTest;
            if (DocDir == null) DocDir = parent.DocDir;
            if (License == null) License = parent.License;
            if (Stability == Stability.Unset) Stability = parent.Stability;

            // Accumulate list entries
            foreach (var dependency in parent.Dependencies) Dependencies.Add(dependency);
            foreach (var bindings in parent.EnvironmentBindings) EnvironmentBindings.Add(bindings);
            foreach (var bindings in parent.OverlayBindings) OverlayBindings.Add(bindings);
        }
        #endregion

        #region Helper Methodes

        /// <summary>
        /// Tests if version is a valid version specificated by http://0install.net/interface-spec.html .
        /// </summary>
        /// <param name="version">String to test.</param>
        /// <returns>Is a valid version.</returns>
        public static bool IsVersion(String version)
        {
            String DottedList = @"(\d{*}(\.\d{*})*)";
            String Modifier = @"pre|rc|post";
            String VersionRegEx = DottedList + "(-" + Modifier + "?" + DottedList + "?)*";
            return Regex.IsMatch(version, VersionRegEx);
        }

        /// <summary>
        /// Tests if versionModifier is a valid version modifiere specificated by http://0install.net/interface-spec.html .
        /// </summary>
        /// <param name="versionModifier">String to test</param>
        /// <returns>Is a valid version modifier.</returns>
        public static bool IsVersionModifier(String versionModifier)
        {
            String Modifier = @"pre|rc|post";
            return Regex.IsMatch(versionModifier, Modifier);
        }

        #endregion
    }
}
