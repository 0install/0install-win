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
using System.Xml.Serialization;
using Common.Storage;
using ZeroInstall.Model;
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
        
        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<ImplementationSelection> _implementations = new C5.HashedArrayList<ImplementationSelection>();
        /// <summary>
        /// A list of <see cref="ImplementationSelection"/>s chosen in this selection.
        /// </summary>
        [Description("A list of implementations chosen in this selection.")]
        [XmlElement("selection")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<ImplementationSelection> Implementations { get { return _implementations; } }

        // Preserve order
        private readonly C5.ArrayList<Command> _commands = new C5.ArrayList<Command>();
        /// <summary>
        /// A set of commands required to execute the selection. The first is for the program, the second is the program's runner, and so on.
        /// </summary>
        [Category("Execution"), Description("A set of commands required to execute the selection. The first is for the program, the second is the program's runner, and so on.")]
        [XmlElement("command")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<Command> Commands { get { return _commands; } }
        #endregion

        //--------------------//

        #region Query
        /// <summary>
        /// Returns the <see cref="ImplementationSelection"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.InterfaceID"/> to look for.</param>
        /// <returns>The identified <see cref="ImplementationBase"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no <see cref="ImplementationSelection"/> matching <paramref name="interfaceID"/> was found in <see cref="Implementations"/>.</exception>
        public ImplementationSelection GetImplementation(string interfaceID)
        {
            foreach (var implementation in _implementations)
            {
                if (implementation.InterfaceID == interfaceID) return implementation;
            }
            throw new KeyNotFoundException();
        }
        #endregion

        #region Implementations
        /// <summary>
        /// Returns a list of any selected <see cref="ImplementationBase"/>s that are missing from an <see cref="IStore"/>.
        /// </summary>
        /// <param name="policy">Combines configuration and resources used to solve dependencies and download implementations.</param>
        /// <returns>An object that allows the main <see cref="ImplementationBase"/> to be executed with all its <see cref="Dependency"/>s injected.</returns>
        /// <remarks>Feed files may be downloaded, no implementations are downloaded.</remarks>
        public IEnumerable<Implementation> ListUncachedImplementations(Policy policy)
        {
            #region Sanity checks
            if (policy == null) throw new ArgumentNullException("policy");
            #endregion

            ICollection<Implementation> notCached = new LinkedList<Implementation>();

            foreach (var implementation in Implementations)
            {
                // Local paths are considered to be always available
                if (!string.IsNullOrEmpty(implementation.LocalPath)) continue;

                // Don't try to download PackageImplementations
                // ToDo: PackageKit integration
                if (!string.IsNullOrEmpty(implementation.Package)) continue;

                // Check if an implementation with a matching digest is available in the cache
                if (policy.SearchStore.Contains(implementation.ManifestDigest)) continue;

                // If not, get download information for the implementation by checking the original feed
                string feedUrl = implementation.FromFeed ?? implementation.InterfaceID;
                Feed feed = File.Exists(feedUrl) ? Feed.Load(feedUrl) : policy.FeedManager.Cache.GetFeed(new Uri(feedUrl));
                feed.Simplify();
                notCached.Add(feed.GetImplementation(implementation.ID));
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
            foreach (var command in Commands) newSelections.Commands.Add(command.CloneCommand());
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
            if (ReferenceEquals(null, other)) return false;

            return (InterfaceID == other.InterfaceID) &&
                Implementations.SequencedEquals(other.Implementations) && Commands.SequencedEquals(other.Commands);
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
                result = (result * 397) ^ Commands.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
