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
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Common.Storage;
using ZeroInstall.Injector.Properties;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Represents a set of <see cref="ImplementationBase"/>s chosen by an <see cref="ISolver"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlRoot("selections", Namespace = Feed.XmlNamespace)]
    [XmlType("selections", Namespace = Feed.XmlNamespace)]
    public sealed class Selections : IEquatable<Selections>, ICloneable
    {
        #region Properties
        /// <summary>
        /// The URI or local path of the interface this selection is based on.
        /// </summary>
        [Description("The URI or local path of the interface this selection is based on.")]
        [XmlAttribute("interface")]
        public string InterfaceID { get; set; }

        /// <summary>
        /// The name of the <see cref="Command"/> in the interface to be started.
        /// </summary>
        [Description("The name of the command in the interface to be started.")]
        [XmlAttribute("command")]
        public string Command { get; set; }

        // Preserve order
        private readonly C5.LinkedList<ImplementationSelection> _implementations = new C5.LinkedList<ImplementationSelection>();

        /// <summary>
        /// A list of <see cref="ImplementationSelection"/>s chosen in this selection.
        /// </summary>
        [Description("A list of implementations chosen in this selection.")]
        [XmlElement("selection")]
        // Note: Can not use ICollection<T> interface with XML Serialization
            public C5.LinkedList<ImplementationSelection> Implementations
        {
            get { return _implementations; }
        }
        #endregion

        //--------------------//

        #region Human readable
        /// <summary>
        /// Generates a human-readable representation of the implementation selection hierachy.
        /// </summary>
        /// <param name="store">A store to search for implementation storage locations.</param>
        public string GetHumanReadable(IStore store)
        {
            var builder = new StringBuilder();
            PrintNode(builder, new C5.HashSet<string>(), store, "", InterfaceID);
            return (builder.Length == 0 ? "" : builder.ToString(0, builder.Length - Environment.NewLine.Length)); // Remove trailing line-break
        }

        /// <summary>
        /// Helper method for <see cref="GetHumanReadable"/> that recursivley writes information about <see cref="ImplementationSelection"/>s to a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">The string builder to write the output to.</param>
        /// <param name="handled">A list of interface IDs that have already been handled; used to prevent infinite recursion.</param>
        /// <param name="store">A store to search for implementation storage locations.</param>
        /// <param name="indent">An indention prefix for the current recursion level (to create a visual hierachy).</param>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.InterfaceID"/> to look for.</param>
        private void PrintNode(StringBuilder builder, C5.HashSet<string> handled, IStore store, string indent, string interfaceID)
        {
            // Prevent infinite recursion
            if (handled.Contains(interfaceID)) return;
            handled.Add(interfaceID);

            builder.AppendLine(indent + "- URI: " + interfaceID);
            try
            {
                var implementation = this[interfaceID];
                builder.AppendLine(indent + "  Version: " + implementation.Version);
                builder.AppendLine(indent + "  Path: " + (implementation.GetPath(store) ?? Resources.NotCached));

                indent += "    ";

                // Recurse into regular dependencies
                foreach (var dependency in implementation.Dependencies)
                    PrintNode(builder, handled, store, indent, dependency.Interface);

                if (!implementation.Commands.IsEmpty)
                {
                    var command = implementation.Commands.First;

                    // Recurse into command dependencies
                    foreach (var dependency in command.Dependencies)
                        PrintNode(builder, handled, store, indent, dependency.Interface);

                    // Recurse into runner dependency
                    if (command.Runner != null)
                        PrintNode(builder, handled, store, indent, command.Runner.Interface);
                }
            }
            catch (KeyNotFoundException)
            {
                builder.AppendLine(indent + "  " + Resources.NoSelectedVersion);
            }
        }
        #endregion

        #region Query
        /// <summary>
        /// Returns the <see cref="ImplementationSelection"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.InterfaceID"/> to look for.</param>
        /// <exception cref="KeyNotFoundException">Thrown if no <see cref="ImplementationSelection"/> matching <paramref name="interfaceID"/> was found in <see cref="Implementations"/>.</exception>
        public ImplementationSelection this[string interfaceID]
        {
            get
            {
                foreach (var implementation in _implementations)
                    if (implementation.InterfaceID == interfaceID) return implementation;
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Determines whether an <see cref="ImplementationSelection"/> for a specific interface is listed in the selection.
        /// </summary>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.InterfaceID"/> to look for.</param>
        /// <returns><see langword="true"/> if an implementation was found; <see langword="false"/> otherwise.</returns>
        public bool ContainsImplementation(string interfaceID)
        {
            foreach (var implementation in _implementations)
                if (implementation.InterfaceID == interfaceID) return true;
            return false;
        }
        #endregion

        #region Implementations
        /// <summary>
        /// Returns a list of any selected downloadable <see cref="ImplementationBase"/>s that are missing from an <see cref="IStore"/>.
        /// </summary>
        /// <param name="searchStore">The locations to search for cached <see cref="Implementation"/>s.</param>
        /// <param name="feedCache">The cache to retreive <see cref="Model.Feed"/>s from.</param>
        /// <returns>An object that allows the main <see cref="ImplementationBase"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <remarks>Feed files may be downloaded, no implementations are downloaded.</remarks>
        /// <exception cref="KeyNotFoundException">Thrown if the requested feed was not found in the cache.</exception>
        /// <exception cref="IOException">Thrown if a problem occured while reading the feed file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidDataException">Thrown if the feed file could not be parsed.</exception>
        public IEnumerable<Implementation> ListUncachedImplementations(IStore searchStore, IFeedCache feedCache)
        {
            #region Sanity checks
            if (searchStore == null) throw new ArgumentNullException("searchStore");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            ICollection<Implementation> notCached = new LinkedList<Implementation>();
            foreach (var implementation in Implementations)
            {
                // Local paths are considered to be always available
                if (!string.IsNullOrEmpty(implementation.LocalPath)) continue;

                // Don't try to download PackageImplementations
                if (!string.IsNullOrEmpty(implementation.Package)) continue;

                // Don't try to fetch virutal feeds
                if (!string.IsNullOrEmpty(implementation.FromFeed) && implementation.FromFeed.StartsWith(ImplementationSelection.DistributionFeedPrefix)) continue;

                // Check if an implementation with a matching digest is available in the cache
                if (searchStore.Contains(implementation.ManifestDigest)) continue;

                // If not, get download information for the implementation by checking the original feed
                Feed feed = feedCache.GetFeed(implementation.FromFeed ?? implementation.InterfaceID);
                var downloadableImplementation = feed.GetImplementation(implementation.ID);
                if (downloadableImplementation != null) notCached.Add(downloadableImplementation);
            }

            return notCached;
        }
        #endregion

        #region Storage
        /// <summary>
        /// Loads <see cref="Selections"/> from an XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Selections"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Selections Load(string path)
        {
            return XmlStorage.Load<Selections>(path);
        }

        /// <summary>
        /// Loads <see cref="Selections"/> from a stream containing an XML file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Selections"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Selections Load(Stream stream)
        {
            return XmlStorage.Load<Selections>(stream);
        }

        /// <summary>
        /// Loads <see cref="Selections"/> from an XML string.
        /// </summary>
        /// <param name="data">The XML string to be parsed.</param>
        /// <returns>The loaded <see cref="Selections"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Selections LoadFromString(string data)
        {
            #region Sanity checks
            if (data == null) throw new ArgumentNullException("data");
            #endregion

            return XmlStorage.FromString<Selections>(data);
        }

        /// <summary>
        /// Saves these <see cref="Selections"/> to an XML file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            XmlStorage.Save(path, this);
        }

        /// <summary>
        /// Saves these <see cref="Selections"/> to a stream as an XML file.
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            XmlStorage.Save(stream, this);
        }

        /// <summary>
        /// Returns the <see cref="Selections"/> serialized to an XML string.
        /// </summary>
        public string WriteToString()
        {
            return XmlStorage.ToString(this);
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Selections"/>
        /// </summary>
        /// <returns>The cloned <see cref="Selections"/>.</returns>
        public Selections CloneSelections()
        {
            var newSelections = new Selections {InterfaceID = InterfaceID};
            foreach (var implementation in Implementations) newSelections.Implementations.Add(implementation.CloneImplementation());
            return newSelections;
        }

        /// <summary>
        /// Creates a deep copy of this <see cref="Selections"/>
        /// </summary>
        /// <returns>The cloned <see cref="Selections"/> casted to a generic <see cref="object"/>.</returns>
        public object Clone()
        {
            return CloneSelections();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(Selections other)
        {
            if (other == null) return false;

            return (InterfaceID == other.InterfaceID) && Implementations.SequencedEquals(other.Implementations);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Selections) && Equals((Selections)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = (InterfaceID != null ? InterfaceID.GetHashCode() : 0);
                result = (result * 397) ^ Implementations.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
