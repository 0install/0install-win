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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Model.Capabilities;
using ZeroInstall.Model.Properties;

namespace ZeroInstall.Model
{
    /// <summary>
    /// Represents a Zero Install feed containing information about an application or library.
    /// </summary>
    /// <remarks>A feed contains all the information required to download and execute an application. It is usually downloaded and updated from a specific URI.</remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Serializable]
    [XmlRoot("interface", Namespace = XmlNamespace)]
    [XmlType("interface", Namespace = XmlNamespace)]
    public sealed class Feed : XmlUnknown, IElementContainer, ICloneable, IEquatable<Feed>
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing feed/interface-related data.
        /// </summary>
        public const string XmlNamespace = "http://zero-install.sourceforge.net/2004/injector/interface";
        #endregion

        #region Properties
        /// <summary>
        /// This attribute gives the oldest version of the injector that can read this file. Older versions will tell the user to upgrade if they are asked to read the file. Versions prior to 0.20 do not perform this check, however. If the attribute is not present, the file can be read by all versions.
        /// </summary>
        [Category("Feed"), Description("This attribute gives the oldest version of the injector that can read this file. Older versions will tell the user to upgrade if they are asked to read the file. Versions prior to 0.20 do not perform this check, however. If the attribute is not present, the file can be read by all versions.")]
        [XmlIgnore]
        public ImplementationVersion MinInjectorVersion { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="MinInjectorVersion"/>
        [XmlAttribute("min-injector-version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string MinInjectorVersionString
        {
            get { return (MinInjectorVersion == null ? null : MinInjectorVersion.ToString()); }
            set { MinInjectorVersion = string.IsNullOrEmpty(value) ? null : new ImplementationVersion(value); }
        }

        /// <summary>
        /// This attribute is only needed for remote feeds (fetched via HTTP). The value must exactly match the expected URL, to prevent an attacker replacing one correctly-signed feed with another (e.g., returning a feed for the shred program when the user asked for the backup program).
        /// </summary>
        [Category("Feed"), Description("This attribute is only needed for remote feeds (fetched via HTTP). The value must exactly match the expected URL, to prevent an attacker replacing one correctly-signed feed with another (e.g., returning a feed for the shred program when the user asked for the backup program).")]
        [XmlIgnore]
        public Uri Uri { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("uri"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string UriString
        {
            get { return (Uri == null ? null : Uri.ToString()); }
            set { Uri = (value == null ? null : new Uri(value)); }
        }

        /// <summary>
        /// A short name to identify the interface (e.g. "Foo").
        /// </summary>
        [Category("Interface"), Description("A short name to identify the interface (e.g. \"Foo\").")]
        [XmlElement("name")]
        public string Name { get; set; }

        private readonly LocalizableStringCollection _summaries = new LocalizableStringCollection();
        /// <summary>
        /// Short one-line descriptions for different languages; the first word should not be upper-case unless it is a proper noun (e.g. "cures all ills").
        /// </summary>
        [Category("Interface"), Description("Short one-line descriptions for different languages; the first word should not be upper-case unless it is a proper noun (e.g. \"cures all ills\").")]
        [XmlElement("summary")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public LocalizableStringCollection Summaries { get { return _summaries; } }

        private readonly LocalizableStringCollection _descriptions = new LocalizableStringCollection();
        /// <summary>
        /// Full descriptions for different languages, which can be several paragraphs long.
        /// </summary>
        [Category("Interface"), Description("Full descriptions for different languages, which can be several paragraphs long.")]
        [XmlElement("description")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public LocalizableStringCollection Descriptions { get { return _descriptions; } }

        /// <summary>
        /// The URL of a web-page describing this interface in more detail.
        /// </summary>
        [Category("Interface"), Description("The URL of a web-page describing this interface in more detail.")]
        [XmlIgnore]
        public Uri Homepage { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Homepage"/>
        [XmlElement("homepage"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string HomepageString
        {
            get { return Homepage != null ? Homepage.ToString() : null; }
            set { Homepage = (value == null ? null : new Uri(value)); }
        }

        // Preserve order
        private readonly C5.LinkedList<Icon> _icons = new C5.LinkedList<Icon>();
        /// <summary>
        /// Zero or more icons to use for the program.
        /// </summary>
        [Category("Interface"), Description("Zero or more icons to use for the program.")]
        [XmlElement("icon")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.LinkedList<Icon> Icons { get { return _icons; } }

        // Order is preserved, duplicate entries are not intended
        private readonly C5.HashedLinkedList<string> _categories = new C5.HashedLinkedList<string>();
        /// <summary>
        /// Zero or more categories as defined by the freedesktop.org menu specification.
        /// </summary>
        [Category("Interface"), Description("Zero or more categories as defined by the freedesktop.org menu specification.")]
        [XmlElement("category")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.HashedLinkedList<string> Categories { get { return _categories; } }

        /// <summary>
        /// If <see langword="true"/>, this element indicates that the program requires a terminal in order to run. Graphical launchers should therefore run this program in a suitable terminal emulator.
        /// </summary>
        [Category("Interface"), Description("If true, this element indicates that the program requires a terminal in order to run. Graphical launchers should therefore run this program in a suitable terminal emulator.")]
        [XmlIgnore, DefaultValue(false)]
        public bool NeedsTerminal { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="NeedsTerminal"/>
        [XmlElement("needs-terminal"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NeedsTerminalString
        {
            get { return (NeedsTerminal ? "" : null); }
            set { NeedsTerminal = (value != null); }
        }

        // Preserve order
        private readonly C5.LinkedList<FeedReference> _feeds = new C5.LinkedList<FeedReference>();
        /// <summary>
        /// Zero ore more additional feeds containing implementations of this interface.
        /// </summary>
        [Category("Feed"), Description("Zero ore more additional feeds containing implementations of this interface.")]
        [XmlElement("feed")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.LinkedList<FeedReference> Feeds { get { return _feeds; } }

        // Preserve order
        private readonly C5.LinkedList<InterfaceReference> _feedFor = new C5.LinkedList<InterfaceReference>();
        /// <summary>
        /// The implementations in this feed are implementations of the given interface. This is used when adding a third-party feed.
        /// </summary>
        [Category("Feed"), Description("The implementations in this feed are implementations of the given interface. This is used when adding a third-party feed.")]
        [XmlElement("feed-for")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.LinkedList<InterfaceReference> FeedFor { get { return _feedFor; } }

        // Preserve order
        private readonly C5.LinkedList<Element> _elements = new C5.LinkedList<Element>();
        /// <summary>
        /// A list of <see cref="Group"/>s and <see cref="Implementation"/>s contained within this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of groups and implementations contained within this interface.")]
        [XmlElement(typeof(Implementation)), XmlElement(typeof(PackageImplementation)), XmlElement(typeof(Group))]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.LinkedList<Element> Elements { get { return _elements; } }

        // Preserve order
        private readonly C5.LinkedList<EntryPoint> _entryPoints = new C5.LinkedList<EntryPoint>();
        /// <summary>
        /// A list of <see cref="EntryPoint"/>s for starting this interface.
        /// </summary>
        [Category("Implementation"), Description("A list of EntryPoints for starting this interface.")]
        [XmlElement("entry-point")]
        // Note: Can not use ICollection<T> interface because of XML Serialization
        public C5.LinkedList<EntryPoint> EntryPoints { get { return _entryPoints; } }

        // Preserve order
        private readonly C5.LinkedList<CapabilityList> _capabilityLists = new C5.LinkedList<CapabilityList>();
        /// <summary>
        /// A list of <see cref="Capability"/>s to be registered in the desktop environment.
        /// </summary>
        [Description("A list of capabilities to be registered in the desktop environment.")]
        [XmlElement("capabilities", Namespace = Capability.XmlNamespace)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.LinkedList<CapabilityList> CapabilityLists { get { return _capabilityLists; } }
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
            var collapsedElements = new C5.LinkedList<Element>();

            foreach (var element in Elements)
            {
                // Flatten structure in groups, set missing default values in implementations
                element.Simplify();

                var group = element as Group;
                if (group != null)
                {
                    // Move implementations out of groups
                    foreach (var groupElement in group.Elements)
                        collapsedElements.Add(groupElement);
                }
                else collapsedElements.Add(element);
            }

            // Replace original elements list with the collapsed version
            Elements.Clear();
            Elements.AddAll(collapsedElements);
        }
        #endregion

        #region Query
        /// <summary>
        /// Returns the <see cref="Implementation"/> with a specific ID string.
        /// </summary>
        /// <param name="id">The <see cref="ImplementationBase.ID"/> to look for.</param>
        /// <returns>The identified <see cref="Implementation"/> or <see langword="null"/> no matching one was found.</returns>
        /// <remarks>Should only be called after <see cref="Simplify"/> has been called, otherwise nested <see cref="Implementation"/>s will be missed.</remarks>
        public Implementation GetImplementation(string id)
        {
            foreach (var element in Elements)
            {
                var implementation = element as Implementation;
                if (implementation != null && implementation.ID == id) return implementation;
            }

            return null;
        }

        /// <summary>
        /// Returns the first <see cref="Implementation"/> with a specific <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="digest">The <see cref="ManifestDigest"/> to look for.</param>
        /// <returns>The identified <see cref="Implementation"/> or <see langword="null"/> no matching one was found.</returns>
        /// <remarks>Should only be called after <see cref="Simplify"/> has been called, otherwise nested <see cref="Implementation"/>s will be missed.</remarks>
        public Implementation GetImplementation(ManifestDigest digest)
        {
            foreach (var element in Elements)
            {
                var implementation = element as Implementation;
                if (implementation != null && implementation.ManifestDigest.PartialEquals(digest))
                    return implementation;
            }

            return null;
        }

        /// <summary>
        /// Returns the first <see cref="EntryPoint"/> referencing a specific <see cref="Command"/>.
        /// </summary>
        /// <param name="command">The command name to search for.</param>
        /// <returns>The identified <see cref="EntryPoint"/> or <see langword="null"/> no matching one was found.</returns>
        public EntryPoint GetEntryPoint(string command)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(command)) throw new ArgumentNullException("command");
            #endregion

            EntryPoint foundEntryPoint;
            EntryPoints.Find(entryPoint => entryPoint.Command == command, out foundEntryPoint);
            return foundEntryPoint;
        }

        /// <summary>
        /// Returns the best matching name for a specific <see cref="Command"/>/<see cref="EntryPoint"/>. Will fall back to <see cref="Name"/>.
        /// </summary>
        /// <param name="language">The language to look for; use <see cref="CultureInfo.InvariantCulture"/> for none.</param>
        /// <param name="command">The name of the command the name should represent; may be <see langword="null"/>.</param>
        /// <returns>The best matching name that was found.</returns>
        public string GetName(CultureInfo language, string command)
        {
            if (string.IsNullOrEmpty(command)) command = Command.NameRun;

            var entryPoint = GetEntryPoint(command);
            if (entryPoint != null)
            {
                string name = entryPoint.Names.GetBestLanguage(language);
                if (!string.IsNullOrEmpty(name)) return name;
            }

            return Name;
        }

        /// <summary>
        /// Returns the best matching summary for a specific <see cref="Command"/>/<see cref="EntryPoint"/>. Will fall back to <see cref="Summaries"/>.
        /// </summary>
        /// <param name="language">The language to look for; use <see cref="CultureInfo.InvariantCulture"/> for none.</param>
        /// <param name="command">The name of the command the summary should represent; may be <see langword="null"/>.</param>
        /// <returns>The best matching summary that was found; <see langword="null"/> if no matching summary was found.</returns>
        public string GetSummary(CultureInfo language, string command)
        {
            if (string.IsNullOrEmpty(command)) command = Command.NameRun;

            var entryPoint = GetEntryPoint(command);
            if (entryPoint != null)
            {
                string summary = entryPoint.Summaries.GetBestLanguage(language);
                if (!string.IsNullOrEmpty(summary)) return summary;
            }

            return Summaries.GetBestLanguage(language);
        }

        /// <summary>
        /// Returns the best matching icon for a specific <see cref="Command"/>/<see cref="EntryPoint"/>. Will fall back to <see cref="Icons"/>.
        /// </summary>
        /// <param name="mimeType">The <see cref="Icon.MimeType"/> to try to find. Will only return exact matches.</param>
        /// <param name="command">The name of the command the icon should represent; may be <see langword="null"/>.</param>
        /// <returns>The best matching icon that was found.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no matching icon was found.</exception>
        public Icon GetIcon(string mimeType, string command)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            if (string.IsNullOrEmpty(command)) command = Command.NameRun;

            var entryPoint = GetEntryPoint(command);
            if (entryPoint != null)
            {
                var suitableCommandIcons = entryPoint.Icons.FindAll(icon => StringUtils.Compare(icon.MimeType, mimeType) && icon.Location != null);
                if (!suitableCommandIcons.IsEmpty) return suitableCommandIcons.First;
            }

            var suitableFeedIcons = Icons.FindAll(icon => StringUtils.Compare(icon.MimeType, mimeType) && icon.Location != null);
            if (!suitableFeedIcons.IsEmpty) return suitableFeedIcons.First;

            throw new KeyNotFoundException(Resources.NoSuitableIconFound);
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a <see cref="Feed"/> from an XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Feed"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Feed Load(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return XmlStorage.Load<Feed>(path);
        }

        /// <summary>
        /// Loads a <see cref="Feed"/> from a stream containing an XML file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Feed"/>.</returns>
        public static Feed Load(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return XmlStorage.Load<Feed>(stream);
        }

        /// <summary>
        /// Saves this <see cref="Feed"/> to an XML file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            XmlStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="Feed"/> to a stream as an XML file.
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            XmlStorage.Save(stream, this);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Feed"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Feed"/>.</returns>
        public Feed CloneFeed()
        {
            var feed = new Feed {MinInjectorVersion = MinInjectorVersion, Uri = Uri, Name = Name, Homepage = Homepage, NeedsTerminal = NeedsTerminal};
            foreach (var feedReference in Feeds) feed.Feeds.Add(feedReference.CloneFeedPreferences());
            foreach (var interfaceReference in FeedFor) feed.FeedFor.Add(interfaceReference.CloneReference());
            foreach (var summary in Summaries) feed.Summaries.Add(summary.CloneString());
            foreach (var description in Descriptions) feed.Descriptions.Add(description.CloneString());
            feed.Categories.AddAll(Categories);
            feed.Icons.AddAll(Icons);
            foreach (var element in Elements) feed.Elements.Add(element.CloneElement());
            foreach (var capabilityList in CapabilityLists) feed.CapabilityLists.Add(capabilityList.CloneCapabilityList());

            return feed;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Feed"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Feed"/>.</returns>
        public object Clone()
        {
            return CloneFeed();
        }
        #endregion
        
        #region Conversion
        /// <summary>
        /// Returns the feed/interface in the form "Inteface Name (Uri)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("Interface {0} ({1})", Name, Uri);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Feed other)
        {
            if (other == null) return false;

            if (MinInjectorVersion != other.MinInjectorVersion) return false;
            if (Uri != other.Uri) return false;
            if (Name != other.Name) return false;
            if (!Summaries.SequencedEquals(other.Summaries)) return false;
            if (!Descriptions.SequencedEquals(other.Descriptions)) return false;
            if (Homepage != other.Homepage) return false;
            if (NeedsTerminal != other.NeedsTerminal) return false;
            if (!Feeds.SequencedEquals(other.Feeds)) return false;
            if (!Categories.SequencedEquals(other.Categories)) return false;
            if (!Icons.SequencedEquals(other.Icons)) return false;
            if (!Elements.SequencedEquals(other.Elements)) return false;
            if (!CapabilityLists.SequencedEquals(other.CapabilityLists)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Feed) && Equals((Feed)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (MinInjectorVersion ?? new ImplementationVersion("0.1")).GetHashCode();
                result = (result * 397) ^ (UriString ?? "").GetHashCode();
                result = (result * 397) ^ (Name ?? "").GetHashCode();
                result = (result * 397) ^ Summaries.GetSequencedHashCode();
                result = (result * 397) ^ Descriptions.GetSequencedHashCode();
                result = (result * 397) ^ (HomepageString ?? "").GetHashCode();
                result = (result * 397) ^ NeedsTerminal.GetHashCode();
                result = (result * 397) ^ Feeds.GetSequencedHashCode();
                result = (result * 397) ^ Categories.GetSequencedHashCode();
                result = (result * 397) ^ Icons.GetSequencedHashCode();
                result = (result * 397) ^ Elements.GetSequencedHashCode();
                result = (result * 397) ^ CapabilityLists.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
