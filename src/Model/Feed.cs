﻿/*
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Common.Collections;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.Model
{
    /// <summary>
    /// A feed contains all the information required to download and execute an application.
    /// It is usually downloaded and updated from a specific URI but may also be a local file.
    /// Feeds downloaded from remote locations are protected from tampering by a OpenPGP signature.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [Description("A feed contains all the information required to download and execute an application.")]
    [Serializable]
    [XmlRoot("interface", Namespace = XmlNamespace), XmlType("interface", Namespace = XmlNamespace)]
    [XmlNamespace("xsi", XmlStorage.XsiNamespace)]
    //[XmlNamespace("caps", CapabilityList.XmlNamespace)]
    public sealed class Feed : XmlUnknown, IElementContainer, ISummaryContainer, IIconContainer, ICloneable, IEquatable<Feed>
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing feed/interface-related data.
        /// </summary>
        public const string XmlNamespace = "http://zero-install.sourceforge.net/2004/injector/interface";

        /// <summary>
        /// The URI to retrieve an XSD containing the XML Schema information for this class in serialized form.
        /// </summary>
        public const string XsdLocation = "http://0install.de/schema/injector/interface/interface.xsd";

        /// <summary>
        /// Provides XML Editors with location hints for XSD files.
        /// </summary>
        public const string XsiSchemaLocation = XmlNamespace + " " + XsdLocation + " " +
            // Advertise the complementary capabilities namespace
            CapabilityList.XmlNamespace + " " + CapabilityList.XsdLocation;

        /// <summary>
        /// Provides XML Editors with location hints for XSD files.
        /// </summary>
        [XmlAttribute("schemaLocation", Namespace = XmlStorage.XsiNamespace)]
        public string SchemaLocation = XsiSchemaLocation;
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
        [XmlAttribute("min-injector-version"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string MinInjectorVersionString { get { return (MinInjectorVersion == null ? null : MinInjectorVersion.ToString()); } set { MinInjectorVersion = string.IsNullOrEmpty(value) ? null : new ImplementationVersion(value); } }

        /// <summary>
        /// This attribute is only needed for remote feeds (fetched via HTTP). The value must exactly match the expected URL, to prevent an attacker replacing one correctly-signed feed with another (e.g., returning a feed for the shred program when the user asked for the backup program).
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public Uri Uri { get; set; }

        /// <summary>Used for XML serialization and PropertyGrid.</summary>
        /// <seealso cref="Uri"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [DisplayName(@"Uri"), Category("Feed"), Description("This attribute is only needed for remote feeds (fetched via HTTP). The value must exactly match the expected URL, to prevent an attacker replacing one correctly-signed feed with another (e.g., returning a feed for the shred program when the user asked for the backup program).")]
        [XmlAttribute("uri"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string UriString { get { return (Uri == null ? null : Uri.ToString()); } set { Uri = (string.IsNullOrEmpty(value) ? null : new Uri(value)); } }

        /// <summary>
        /// A short name to identify the interface (e.g. "Foo").
        /// </summary>
        [Category("Interface"), Description("A short name to identify the interface (e.g. \"Foo\").")]
        [XmlElement("name")]
        public string Name { get; set; }

        private readonly LocalizableStringCollection _summaries = new LocalizableStringCollection();

        /// <inheritdoc/>
        [Browsable(false)]
        [XmlElement("summary")]
        public LocalizableStringCollection Summaries { get { return _summaries; } }

        private readonly LocalizableStringCollection _descriptions = new LocalizableStringCollection();

        /// <inheritdoc/>
        [Browsable(false)]
        [XmlElement("description")]
        public LocalizableStringCollection Descriptions { get { return _descriptions; } }

        /// <summary>
        /// The URL of a web-page describing this interface in more detail.
        /// </summary>
        [XmlIgnore, Browsable(false)]
        public Uri Homepage { get; set; }

        /// <summary>Used for XML serialization and PropertyGrid.</summary>
        /// <seealso cref="Homepage"/>
        [DisplayName(@"Homepage"), Category("Interface"), Description("The URL of a web-page describing this interface in more detail.")]
        [XmlElement("homepage"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string HomepageString { get { return Homepage != null ? Homepage.ToString() : null; } set { Homepage = (string.IsNullOrEmpty(value) ? null : new Uri(value)); } }

        // Preserve order
        private readonly C5.ArrayList<Icon> _icons = new C5.ArrayList<Icon>();

        /// <summary>
        /// Zero or more icons representing the application. Used in the Catalog GUI as well as for desktop icons, menu entries, etc..
        /// </summary>
        [Browsable(false)]
        [XmlElement("icon")]
        public C5.ArrayList<Icon> Icons { get { return _icons; } }

        // Preserve order
        private readonly C5.ArrayList<Category> _categories = new C5.ArrayList<Category>();

        /// <summary>
        /// Zero or more classifications for the interface.
        /// </summary>
        [Browsable(false)]
        [XmlElement("category")]
        public C5.ArrayList<Category> Categories { get { return _categories; } }

        /// <summary>
        /// If <see langword="true"/>, indicates that the program requires a terminal in order to run. Graphical launchers should therefore run this program in a suitable terminal emulator.
        /// </summary>
        [Category("Interface"), Description("If true, indicates that the program requires a terminal in order to run. Graphical launchers should therefore run this program in a suitable terminal emulator.")]
        [XmlIgnore, DefaultValue(false)]
        public bool NeedsTerminal { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="NeedsTerminal"/>
        [XmlElement("needs-terminal"), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), EditorBrowsable(EditorBrowsableState.Never)]
        public string NeedsTerminalString { get { return (NeedsTerminal ? "" : null); } set { NeedsTerminal = (value != null); } }

        // Preserve order
        private readonly C5.ArrayList<FeedReference> _feeds = new C5.ArrayList<FeedReference>();

        /// <summary>
        /// Zero ore more additional feeds containing implementations of this interface.
        /// </summary>
        [Browsable(false)]
        [XmlElement("feed")]
        public C5.ArrayList<FeedReference> Feeds { get { return _feeds; } }

        // Preserve order
        private readonly C5.ArrayList<InterfaceReference> _feedFor = new C5.ArrayList<InterfaceReference>();

        /// <summary>
        /// The implementations in this feed are implementations of the given interface. This is used when adding a third-party feed.
        /// </summary>
        [Browsable(false)]
        [XmlElement("feed-for")]
        public C5.ArrayList<InterfaceReference> FeedFor { get { return _feedFor; } }

        /// <summary>
        /// This feed's interface <see cref="Uri"/> has been replaced by the given interface. Any references to the old URI should be updated to use the new one.
        /// </summary>
        /// <seealso cref="ImplementationBase.ManifestDigest"/>
        [Browsable(false)]
        [XmlElement("replaced-by")]
        public InterfaceReference ReplacedBy { get; set; }

        // Preserve order
        private readonly C5.ArrayList<Element> _elements = new C5.ArrayList<Element>();

        /// <summary>
        /// A list of <see cref="Group"/>s and <see cref="Implementation"/>s contained within this interface.
        /// </summary>
        [Browsable(false)]
        [XmlElement(typeof(Implementation)), XmlElement(typeof(PackageImplementation)), XmlElement(typeof(Group))]
        public C5.ArrayList<Element> Elements { get { return _elements; } }

        // Preserve order
        private readonly C5.ArrayList<EntryPoint> _entryPoints = new C5.ArrayList<EntryPoint>();

        /// <summary>
        /// A list of <see cref="EntryPoint"/>s for starting this interface.
        /// </summary>
        [Browsable(false)]
        [XmlElement("entry-point")]
        public C5.ArrayList<EntryPoint> EntryPoints { get { return _entryPoints; } }

        // Preserve order
        private readonly C5.ArrayList<CapabilityList> _capabilityLists = new C5.ArrayList<CapabilityList>();

        /// <summary>
        /// A set of <see cref="Capability"/> lists for different architectures.
        /// </summary>
        [Browsable(false)]
        [XmlElement("capabilities", Namespace = CapabilityList.XmlNamespace)]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<CapabilityList> CapabilityLists
        {
            get { return _capabilityLists; }
        }
        #endregion

        //--------------------//

        #region Normalize
        /// <summary>
        /// Flattens the <see cref="Group"/> inheritance structure and sets missing default values in <see cref="Implementation"/>s.
        /// </summary>
        /// <param name="feedID">The feed the data was originally loaded from.</param>
        /// <remarks>This method should be called to prepare a <see cref="Feed"/> for solver processing.
        /// It should not be called if you plan on serializing the feed again since it will may loose some of its structure.</remarks>
        public void Normalize(string feedID)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(feedID)) throw new ArgumentNullException("feedID");
            #endregion

            var collapsedElements = new List<Element>();

            foreach (var element in Elements)
            {
                // Flatten structure in groups, set missing default values in implementations
                element.Normalize(feedID);

                var group = element as Group;
                if (group != null)
                {
                    // Move implementations out of groups
                    collapsedElements.AddRange(group.Elements);
                }
                else collapsedElements.Add(element);
            }

            // Replace original elements list with the collapsed version
            Elements.Clear();
            Elements.AddAll(collapsedElements);
        }

        /// <summary>
        /// Strips the feed down to the application metadata removing specific <see cref="Implementation"/>s.
        /// </summary>
        public void Strip()
        {
            // TODO: Extract supported architectures
            Elements.Clear();

            // TODO: Extract supported file types
            CapabilityLists.Clear();

            SchemaLocation = null;
            UnknownAttributes = new XmlAttribute[0];
            UnknownElements = new XmlElement[0];
        }
        #endregion

        #region Query
        /// <summary>
        /// Returns the <see cref="Implementation"/> with a specific ID string.
        /// </summary>
        /// <param name="id">The <see cref="ImplementationBase.ID"/> to look for.</param>
        /// <returns>The identified <see cref="Implementation"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no <see cref="Implementation"/> matching <paramref name="id"/> was found in <see cref="Elements"/>.</exception>
        /// <remarks>Should only be called after <see cref="Normalize"/> has been called, otherwise nested <see cref="Implementation"/>s will be missed.</remarks>
        public Implementation this[string id]
        {
            get
            {
                #region Sanity checks
                if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
                #endregion

                return Elements.OfType<Implementation>().First(implementation => implementation.ID == id,
                    () => new KeyNotFoundException(string.Format("Unable to find implementation '{0}' in feed '{1}'.", id, Name)));
            }
        }

        /// <summary>
        /// Returns the first <see cref="Implementation"/> with a specific <see cref="ManifestDigest"/>.
        /// </summary>
        /// <param name="digest">The <see cref="ManifestDigest"/> to look for.</param>
        /// <returns>The identified <see cref="Implementation"/> or <see langword="null"/> no matching one was found.</returns>
        /// <remarks>Should only be called after <see cref="Normalize"/> has been called, otherwise nested <see cref="Implementation"/>s will be missed.</remarks>
        public Implementation this[ManifestDigest digest]
        {
            get
            {
                return Elements.OfType<Implementation>().First(implementation => implementation.ManifestDigest.PartialEquals(digest),
                    () => new KeyNotFoundException(string.Format("Unable to find implementation '{0}' in feed '{1}'.", digest, Name)));
            }
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

            return EntryPoints.FirstOrDefault(entryPoint => entryPoint.Command == command);
        }

        /// <summary>
        /// Returns the best matching name for a specific <see cref="Command"/>/<see cref="EntryPoint"/>. Will fall back to <see cref="Name"/>.
        /// </summary>
        /// <param name="language">The language to look for; use <see cref="CultureInfo.InvariantCulture"/> for none.</param>
        /// <param name="command">The name of the command the name should represent; may be <see langword="null"/>.</param>
        /// <returns>The best matching name that was found.</returns>
        public string GetBestName(CultureInfo language, string command = null)
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
        public string GetBestSummary(CultureInfo language, string command = null)
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
        /// <returns>The best matching icon that was found or <see langword="null"/> if no matching icon was found.</returns>
        public Icon GetIcon(string mimeType, string command = null)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            #endregion

            if (string.IsNullOrEmpty(command)) command = Command.NameRun;

            var entryPoint = GetEntryPoint(command);
            if (entryPoint != null)
            {
                var suitableCommandIcons = entryPoint.Icons.FindAll(icon => StringUtils.EqualsIgnoreCase(icon.MimeType, mimeType) && icon.Href != null);
                if (!suitableCommandIcons.IsEmpty) return suitableCommandIcons.First;
            }

            var suitableFeedIcons = Icons.FindAll(icon => StringUtils.EqualsIgnoreCase(icon.MimeType, mimeType) && icon.Href != null);
            if (!suitableFeedIcons.IsEmpty) return suitableFeedIcons.First;

            return null;
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Feed"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Feed"/>.</returns>
        public Feed Clone()
        {
            var feed = new Feed {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements, MinInjectorVersion = MinInjectorVersion, Uri = Uri, Name = Name, Homepage = Homepage, NeedsTerminal = NeedsTerminal};
            feed.Feeds.AddAll(Feeds.CloneElements());
            feed.FeedFor.AddAll(FeedFor.CloneElements());
            feed.Summaries.AddAll(Summaries.CloneElements());
            feed.Descriptions.AddAll(Descriptions.CloneElements());
            feed.Categories.AddAll(Categories);
            feed.Icons.AddAll(Icons);
            feed.Elements.AddAll(Elements.CloneElements());
            feed.EntryPoints.AddAll(EntryPoints.CloneElements());
            feed.CapabilityLists.AddAll(CapabilityLists.CloneElements());
            return feed;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Conversion
        /// <summary>
        /// Returns the feed/interface in the form "Name (Uri)". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Uri);
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Feed other)
        {
            if (other == null) return false;
            if (!base.Equals(other)) return false;
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
            if (!EntryPoints.SequencedEquals(other.EntryPoints)) return false;
            if (!CapabilityLists.SequencedEquals(other.CapabilityLists)) return false;
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Feed && Equals((Feed)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                if (MinInjectorVersion != null) result = (result * 397) ^ MinInjectorVersion.GetHashCode();
                if (Uri != null) result = (result * 397) ^ Uri.GetHashCode();
                if (Name != null) result = (result * 397) ^ Name.GetHashCode();
                result = (result * 397) ^ Summaries.GetSequencedHashCode();
                result = (result * 397) ^ Descriptions.GetSequencedHashCode();
                if (Homepage != null) result = (result * 397) ^ Homepage.GetHashCode();
                result = (result * 397) ^ NeedsTerminal.GetHashCode();
                result = (result * 397) ^ Feeds.GetSequencedHashCode();
                result = (result * 397) ^ Categories.GetSequencedHashCode();
                result = (result * 397) ^ Icons.GetSequencedHashCode();
                result = (result * 397) ^ Elements.GetSequencedHashCode();
                result = (result * 397) ^ EntryPoints.GetSequencedHashCode();
                result = (result * 397) ^ CapabilityLists.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
