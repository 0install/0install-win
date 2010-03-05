using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a Zero Install feed containing information about...
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Interface")]
    [XmlRoot("interface", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    // ToDo: Suppress xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
    public sealed class Interface : IGroupContainer
    {
        #region Properties
        /// <summary>
        /// This attribute gives the oldest version of the injector that can read this file. Older versions will tell the user to upgrade if they are asked to read the file. Versions prior to 0.20 do not perform this check, however. If the attribute is not present, the file can be read by all versions.
        /// </summary>
        [Category("Feed"), Description("This attribute gives the oldest version of the injector that can read this file. Older versions will tell the user to upgrade if they are asked to read the file. Versions prior to 0.20 do not perform this check, however. If the attribute is not present, the file can be read by all versions.")]
        [XmlAttribute("min-injector-version")]
        public string MinInjectorVersion { get; set; }

        /// <summary>
        /// This attribute is only needed for remote feeds (fetched via HTTP). The value must exactly match the expected URL, to prevent an attacker replacing one correctly-signed feed with another (e.g., returning a feed for the shred program when the user asked for the backup program). 
        /// </summary>
        [Category("Feed"), Description("This attribute is only needed for remote feeds (fetched via HTTP). The value must exactly match the expected URL, to prevent an attacker replacing one correctly-signed feed with another (e.g., returning a feed for the shred program when the user asked for the backup program).")]
        [XmlIgnore]
        public Uri Uri { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("uri"), Browsable(false)]
        public String UriString
        {
            get { return (Uri == null ? null : Uri.ToString()); }
            set { Uri = new Uri(value); }
        }

        private readonly Collection<Feed> _feeds = new Collection<Feed>();
        /// <summary>
        /// Zero ore more feeds containing more implementations of this interface.
        /// </summary>
        [Category("Feed"), Description("Zero ore more feeds containing more implementations of this interface.")]
        [XmlElement("feed")]
        public Collection<Feed> Feeds { get { return _feeds; } }

        private readonly Collection<String> _feedFor = new Collection<String>();
        /// <summary>
        /// The implementations in this feed are implementations of the given interface. This is used when adding a third-party feed.
        /// </summary>
        [Category("Feed"), Description("The implementations in this feed are implementations of the given interface. This is used when adding a third-party feed.")]
        [XmlElement("feed-for")]
        public Collection<String> FeedFor { get { return _feedFor; } }

        /// <summary>
        /// A short name to identify the interface (e.g. "Foo").
        /// </summary>
        [Category("Interface"), Description("A short name to identify the interface (e.g. \"Foo\").")]
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// A short one-line description; the first word should not be upper-case unless it is a proper noun (e.g. "cures all ills").
        /// </summary>
        [Category("Interface"), Description("A short one-line description; the first word should not be upper-case unless it is a proper noun (e.g. \"cures all ills\").")]
        [XmlElement("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// A full description, which can be several paragraphs long (optional since 0.32, but recommended).
        /// </summary>
        [Category("Interface"), Description("A full description, which can be several paragraphs long (optional since 0.32, but recommended).")]
        //[Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [XmlElement("description")]
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// The URL of a web-page describing this interface in more detail.
        /// </summary>
        [Category("Interface"), Description("The URL of a web-page describing this interface in more detail.")]
        [XmlIgnore]
        public Uri Homepage { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Homepage"/>
        [XmlElement("homepage"), Browsable(false)]
        public String HomepageString
        {
            get { return Homepage != null ? Homepage.ToString() : null; }
            set { Homepage = new Uri(value); }
        }

        private readonly Collection<string> _categories = new Collection<string>();
        /// <summary>
        /// Zero or more categories as classification for the interface.
        /// </summary>
        [Category("Interface"), Description("Zero or more categories as classification for the interface.")]
        [XmlElement("category")]
        public Collection<string> Categories { get { return _categories; } }

        /// <summary>
        /// If <see langword="true"/>, this element indicates that the program requires a terminal in order to run. Graphical launchers should therefore run this program in a suitable terminal emulator.
        /// </summary>
        [Category("Interface"), Description("If true, this element indicates that the program requires a terminal in order to run. Graphical launchers should therefore run this program in a suitable terminal emulator.")]
        [XmlIgnore, DefaultValue(false)]
        public bool NeedsTerminal { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="NeedsTerminal"/>
        [XmlElement("needs-terminal"), Browsable(false)]
        public string NeedsTerminalString
        {
            get { return (NeedsTerminal ? "" : null); }
            set { NeedsTerminal = (value != null); }
        }

        private readonly Collection<Icon> _icons = new Collection<Icon>();
        /// <summary>
        /// Zero or more icons to use for the program.
        /// </summary>
        [Category("Interface"), Description("Zero or more icons to use for the program.")]
        [XmlElement("icon")]
        public Collection<Icon> Icons { get { return _icons; } }

        private readonly Collection<Group> _groups = new Collection<Group>();
        /// <summary>
        /// A list of <see cref="Group"/>s contained within this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of groups contained within this interface.")]
        [XmlElement("group")]
        public Collection<Group> Groups { get { return _groups; } }

        #region Implementations
        private readonly Collection<Implementation> _implementation = new Collection<Implementation>();
        /// <summary>
        /// A list of <see cref="Implementation"/>s contained within this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of implementations contained within this interface.")]
        [XmlElement("implementation")]
        public Collection<Implementation> Implementations { get { return _implementation; } }

        private readonly Collection<PackageImplementation> _packageImplementation = new Collection<PackageImplementation>();
        /// <summary>
        /// A list of distribution-provided <see cref="PackageImplementation"/>s contained within this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of distribution-provided package implementations contained within this interface.")]
        [XmlElement("package-implementation")]
        public Collection<PackageImplementation> PackageImplementations { get { return _packageImplementation; } }
        #endregion

        #endregion

        #region Inheritance
        /// <summary>
        /// Calls <see cref="ImplementationBase.InheritFrom"/> for all <see cref="Groups"/>, <see cref="Implementations"/> and <see cref="PackageImplementations"/>
        /// and moves the entries of <see cref="Groups"/> upwards.
        /// </summary>
        public void FlattenInheritance()
        {
            foreach (var group in Groups)
            {
                // Flatten structure in groups
                group.FlattenInheritance();

                // Move entries from groups upwards
                foreach (var implementation in group.Implementations) Implementations.Add(implementation);
                group.Implementations.Clear();
                foreach (var implementation in group.PackageImplementations) PackageImplementations.Add(implementation);
                group.Implementations.Clear();
            }

            // All groups are now empty and unnecessary
            Groups.Clear();
        }
        #endregion

        #region Simplify
        /// <summary>
        /// Calls <see cref="FlattenInheritance"/> and performs a number of other cleanups and simplifications to the interface structure.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the interface again since it will lose some of its structure.</remarks>
        public void Simplify()
        {
            FlattenInheritance();

            // Default stability rating to testing.
            foreach (var implementation in Implementations)
            {
                if (implementation.Stability == Stability.Unset) implementation.Stability = Stability.Testing;
            }

            // ToDo: Move digests from ID to ManifestDigest
            // ToDo: Default stability to testing
            // ToDo: Guess missing MIME archive types
        }
        #endregion
    }
}


