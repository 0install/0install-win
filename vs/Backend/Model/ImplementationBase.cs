using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;

namespace ZeroInstall.Backend.Model
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

        /// <summary>Used only by <see cref="PackageImplementation"/></summary>
        [XmlIgnore]
        Packaged,

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
    public abstract class ImplementationBase : ReleaseBase, IBindingContainer
    {
        #region Properties
        /// <summary>
        /// The version number.
        /// </summary>
        [Description("The version number.")]
        [XmlIgnore]
        public virtual Version Version { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("version"), Browsable(false)]
        public string VersionString
        {
            get { return (Version == null ? null : Version.ToString()); }
            set { Version = new Version((value)); }
        }

        /// <summary>
        /// The date this implementation was made available. For development versions checked out from version control this attribute should not be present.
        /// </summary>
        [Description("The date this implementation was made available. For development versions checked out from version control this attribute should not be present.")]
        [XmlIgnore]
        public DateTime Released { get; set; }

        private const string ReleaseDateFormat = "yyyy-MM-dd";
        /// <summary>Used for XML serialization..</summary>
        /// <seealso cref="Version"/>
        [XmlAttribute("released"), Browsable(false)]
        public string ReleasedString
        {
            get { return (Released == default(DateTime) ? null : Released.ToString(ReleaseDateFormat)); }
            set { Released = DateTime.ParseExact(value, ReleaseDateFormat, CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// The relative path of an executable inside the implementation that should be executed by default when the interface is run. If an implementation has no main setting, then it cannot be executed without specifying one manually. This typically means that the interface is for a library.
        /// </summary>
        [Description("The relative path of an executable inside the implementation that should be executed by default when the interface is run. If an implementation has no main setting, then it cannot be executed without specifying one manually. This typically means that the interface is for a library.")]
        [XmlAttribute("main")]
        public string Main { get; set; }

        /// <summary>
        /// The relative path of an executable inside the implementation that can be executed to test the program. The program must be non-interactive (e.g. it can't open any windows or prompt for input). It should return with an exit status of zero if the tests pass. Any other status indicates failure.
        /// </summary>
        [Description("The relative path of an executable inside the implementation that can be executed to test the program. The program must be non-interactive (e.g. it can't open any windows or prompt for input). It should return with an exit status of zero if the tests pass. Any other status indicates failure.")]
        [XmlAttribute("self-test")]
        public string SelfTest { get; set; }

        /// <summary>
        /// The relative path of a directory inside the implementation that contains the package's documentation. This is the directory that would end up inside /usr/share/doc on a traditional Linux system.
        /// </summary>
        [Description("The relative path of a directory inside the implementation that contains the package's documentation. This is the directory that would end up inside /usr/share/doc on a traditional Linux system.")]
        [XmlAttribute("doc-dir")]
        public string DocDir { get; set; }

        /// <summary>
        /// License terms (typically a Trove category, as used on freshmeat.net).
        /// </summary>
        [Description("License terms (typically a Trove category, as used on freshmeat.net).")]
        [XmlAttribute("license")]
        public string License { get; set; }

        private Stability _stability = Stability.Testing;

        /// <summary>
        /// The default stability rating for this implementation.
        /// </summary>
        [Description("The default stability rating for this implementation.")]
        [XmlAttribute("stability")]
        public virtual Stability Stability { get { return _stability; } set { _stability = value; } }

        private readonly Collection<Dependency> _dependencies = new Collection<Dependency>();
        /// <summary>
        /// A list of <see cref="Interface"/>s this implementation depends upon.
        /// </summary>
        [Description("A list of interfaces this implementation depends upon.")]
        [XmlElement("requires")]
        public Collection<Dependency> Dependencies { get { return _dependencies; } }
        
        private readonly Collection<EnvironmentBinding> _environmentBindings = new Collection<EnvironmentBinding>();
        /// <summary>
        /// A list of <see cref="EnvironmentBinding"/>s for this implementation to locate itself.
        /// </summary>
        [Description("A list of bindings for this implementation to locate itself.")]
        [XmlElement("environment")]
        public Collection<EnvironmentBinding> EnvironmentBindings { get { return _environmentBindings; } }

        private readonly Collection<OverlayBinding> _overlayBindings = new Collection<OverlayBinding>();
        /// <summary>
        /// A list of <see cref="OverlayBinding"/>s for this implementation to locate itself.
        /// </summary>
        [Description("A list of bindings for this implementation to locate itself.")]
        [XmlElement("overlay")]
        public Collection<OverlayBinding> OverlayBindings { get { return _overlayBindings; } }
        #endregion

        #region Inheritance
        /// <summary>
        /// Transfers attributes from another <see cref="ImplementationBase"/> object to this one.
        /// Existing values are not replaced. Provides an inheritance-like relation.
        /// </summary>
        /// <param name="parent">The object to take the attributes from.</param>
        internal void InheritFrom(ImplementationBase parent)
        {
            // Check if values are unset and need inheritance
            if (Version == null) Version = parent.Version;
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
    }
}
