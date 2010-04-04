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
using System.IO;
using System.Xml.Serialization;
using Common.Storage;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a Zero Install feed containing information about...
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Interface")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be dispoed.")]
    [XmlRoot("interface", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    public sealed class Interface : IGroupContainer, ISimplifyable
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
            set { Uri = (value == null ? null : new Uri(value)); }
        }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<FeedReference> _feeds = new C5.HashedArrayList<FeedReference>();
        /// <summary>
        /// Zero ore more feeds containing more implementations of this interface.
        /// </summary>
        [Category("Feed"), Description("Zero ore more feeds containing more implementations of this interface.")]
        [XmlElement("feed")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<FeedReference> Feeds { get { return _feeds; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<InterfaceReference> _feedFor = new C5.HashedArrayList<InterfaceReference>();
        /// <summary>
        /// The implementations in this feed are implementations of the given interface. This is used when adding a third-party feed.
        /// </summary>
        [Category("Feed"), Description("The implementations in this feed are implementations of the given interface. This is used when adding a third-party feed.")]
        [XmlElement("feed-for")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<InterfaceReference> FeedFor { get { return _feedFor; } }

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
        public string Description { get; set; }

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
            set { Homepage = (value == null ? null : new Uri(value)); }
        }
        
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<string> _categories = new C5.HashedArrayList<string>();
        /// <summary>
        /// Zero or more categories as classification for the interface.
        /// </summary>
        [Category("Interface"), Description("Zero or more categories as classification for the interface.")]
        [XmlElement("category")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<string> Categories { get { return _categories; } }

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

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Icon> _icons = new C5.HashedArrayList<Icon>();
        /// <summary>
        /// Zero or more icons to use for the program.
        /// </summary>
        [Category("Interface"), Description("Zero or more icons to use for the program.")]
        [XmlElement("icon")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Icon> Icons { get { return _icons; } }
        
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Group> _groups = new C5.HashedArrayList<Group>();
        /// <summary>
        /// A list of <see cref="Group"/>s contained within this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of groups contained within this interface.")]
        [XmlElement("group")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Group> Groups { get { return _groups; } }

        #region Implementations
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<Implementation> _implementation = new C5.HashedArrayList<Implementation>();
        /// <summary>
        /// A list of <see cref="Implementation"/>s contained within this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of implementations contained within this interface.")]
        [XmlElement("implementation")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<Implementation> Implementations { get { return _implementation; } }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<PackageImplementation> _packageImplementation = new C5.HashedArrayList<PackageImplementation>();
        /// <summary>
        /// A list of distribution-provided <see cref="PackageImplementation"/>s contained within this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of distribution-provided package implementations contained within this interface.")]
        [XmlElement("package-implementation")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<PackageImplementation> PackageImplementations { get { return _packageImplementation; } }
        #endregion

        #endregion

        //--------------------//
        
        #region Simplify
        /// <summary>
        /// Flattens the <see cref="Group"/> inheritance structure and sets missing default values in <see cref="Implementation"/>s.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Interface"/> again since it will may some of its structure.</remarks>
        public void Simplify()
        {
            // Set missing default values
            foreach (var implementation in Implementations) implementation.Simplify();
            foreach (var implementation in PackageImplementations) implementation.Simplify();

            foreach (var group in Groups)
            {
                // Flatten structure in groups and set missing default values in contained implementations
                group.Simplify();

                // Move implementations out of groups
                foreach (var implementation in group.Implementations) Implementations.Add(implementation);
                group.Implementations.Clear();
                foreach (var implementation in group.PackageImplementations) PackageImplementations.Add(implementation);
                group.Implementations.Clear();
            }

            // All groups are now empty and unnecessary
            Groups.Clear();
        }
        #endregion

        //--------------------//

        // ToDo: Implement ToString and Equals
        
        //--------------------//

        #region Storage
        /// <summary>
        /// Loads an <see cref="Interface"/> from an XML file (feed).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Interface"/>.</returns>
        public static Interface Load(string path)
        {
            // Load the file
            return XmlStorage.Load<Interface>(path);
        }

        /// <summary>
        /// Loads an <see cref="Interface"/> from a stream containing an XML file (feed).
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Interface"/>.</returns>
        public static Interface Load(Stream stream)
        {
            // Load the file
            return XmlStorage.Load<Interface>(stream);
        }

        /// <summary>
        /// Saves this <see cref="Interface"/> in an XML file (feed).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        public void Save(string path)
        {
            XmlStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="Interface"/> in a stream as an XML file (feed).
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            XmlStorage.Save(stream, this);
        }
        #endregion
    }
}
