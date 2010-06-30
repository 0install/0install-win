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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using Common.Collections;
using Common.Storage;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a Zero Install feed containing information about an application or library.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Interface")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlRoot("interface", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    public sealed class Feed : IGroupContainer, ISimplifyable, IEquatable<Feed>
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

        private readonly XmlLocalizableStringCollection _summaries = new XmlLocalizableStringCollection();
        /// <summary>
        /// Short one-line descriptions for different languages; the first word should not be upper-case unless it is a proper noun (e.g. "cures all ills").
        /// </summary>
        [Category("Interface"), Description("Short one-line descriptions for different languages; the first word should not be upper-case unless it is a proper noun (e.g. \"cures all ills\").")]
        [XmlElement("summary")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public XmlLocalizableStringCollection Summaries { get { return _summaries; } }

        private readonly XmlLocalizableStringCollection _descriptions = new XmlLocalizableStringCollection();
        /// <summary>
        /// Full descriptions for different languages, which can be several paragraphs long.
        /// </summary>
        [Category("Interface"), Description("Full descriptions for different languages, which can be several paragraphs long.")]
        [XmlElement("description")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public XmlLocalizableStringCollection Descriptions { get { return _descriptions; } }

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
        
        // Preserve order
        private readonly C5.ArrayList<Group> _groups = new C5.ArrayList<Group>();
        /// <summary>
        /// A list of <see cref="Group"/>s contained within this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of groups contained within this interface.")]
        [XmlElement("group")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<Group> Groups { get { return _groups; } }

        #region Implementations
        // Preserve order
        private readonly C5.ArrayList<Implementation> _implementations = new C5.ArrayList<Implementation>();
        /// <summary>
        /// A list of downloadable <see cref="Implementation"/>s for this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of downloadable implementations contained for this interface.")]
        [XmlElement("implementation")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<Implementation> Implementations { get { return _implementations; } }

        // Preserve order
        private readonly C5.ArrayList<PackageImplementation> _packageImplementation = new C5.ArrayList<PackageImplementation>();
        /// <summary>
        /// A list of distribution-provided <see cref="PackageImplementation"/>s for this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of distribution-provided package implementations for this interface.")]
        [XmlElement("package-implementation")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<PackageImplementation> PackageImplementations { get { return _packageImplementation; } }
        #endregion

        #endregion

        //--------------------//
        
        #region Simplify
        /// <summary>
        /// Flattens the <see cref="Group"/> inheritance structure and sets missing default values in <see cref="Implementation"/>s.
        /// </summary>
        /// <remarks>This should be called to prepare an interface for launch.
        /// It should not be called if you plan on serializing the <see cref="Feed"/> again since it will may some of its structure.</remarks>
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

        #region Query
        /// <summary>
        /// Returns the <see cref="Implementation"/> with a specific ID string.
        /// </summary>
        /// <param name="id">The <see cref="IDImplementation.ID"/> to look for.</param>
        /// <returns>The identified <see cref="Implementation"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no <see cref="Implementation"/> matching <paramref name="id"/> was found in <see cref="Implementations"/>.</exception>
        /// <remarks>Should only be called after <see cref="Simplify"/> has been called, otherwise nested <see cref="Implementation"/>s will be missed.</remarks>
        public Implementation GetImplementation(string id)
        {
            foreach (var implementation in Implementations)
            {
                if (implementation.ID == id) return implementation;
            }

            throw new KeyNotFoundException();
        }
        #endregion

        //--------------------//

        // ToDo: Implement ToString, Equals and Clone
        
        //--------------------//

        #region Storage
        /// <summary>
        /// Loads an <see cref="Feed"/> from an XML file (feed).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Feed"/>.</returns>
        /// <exception cref="IOException">Thrown if the file couldn't be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        public static Feed Load(string path)
        {
            return XmlStorage.Load<Feed>(path);
        }

        /// <summary>
        /// Loads an <see cref="Feed"/> from a stream containing an XML file (feed).
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Feed"/>.</returns>
        public static Feed Load(Stream stream)
        {
            return XmlStorage.Load<Feed>(stream);
        }

        /// <summary>
        /// Saves this <see cref="Feed"/> to an XML file (feed).
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if the file couldn't be created.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            XmlStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="Feed"/> to a stream as an XML file (feed).
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            XmlStorage.Save(stream, this);
        }
        #endregion

        #region Equality
        public bool Equals(Feed other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (MinInjectorVersion != other.MinInjectorVersion) return false;
            if (Uri != other.Uri) return false;
            if (Name != other.Name) return false;
            if (!Summaries.UnsequencedEquals(other.Summaries)) return false;
            if (!Descriptions.UnsequencedEquals(other.Descriptions)) return false;
            if (Homepage != other.Homepage) return false;
            if (NeedsTerminal != other.NeedsTerminal) return false;
            if (!Feeds.UnsequencedEquals(other.Feeds)) return false;
            if (!Categories.UnsequencedEquals(other.Categories)) return false;
            if (!Groups.UnsequencedEquals(other.Groups)) return false;
            if (!Icons.UnsequencedEquals(other.Icons)) return false;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Feed) && Equals((Feed)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (MinInjectorVersion ?? "").GetHashCode();
                result = (result * 397) ^ (UriString ?? "").GetHashCode();
                result = (result * 397) ^ (Name ?? "").GetHashCode();
                result = (result * 397) ^ Summaries.GetUnsequencedHashCode();
                result = (result * 397) ^ Descriptions.GetUnsequencedHashCode();
                result = (result * 397) ^ (HomepageString ?? "").GetHashCode();
                result = (result * 397) ^ NeedsTerminal.GetHashCode();
                result = (result * 397) ^ Feeds.GetUnsequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
