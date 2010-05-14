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
using Common.Storage;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Interface;

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Represents a number of <see cref="Implementation"/>s chosen for executing an <see cref="Model.Interface"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlRoot("selections", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    public sealed class Selections : IEquatable<Selections>, ICloneable
    {
        #region Properties
        /// <summary>
        /// The URL of the interface this selection is based on.
        /// </summary>
        [Description("The URL of the interface this selection is based on.")]
        [XmlIgnore]
        public Uri Interface
        { get; set; }

        /// <summary>Used for XML serialization.</summary>
        /// <seealso cref="Interface"/>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Used for XML serialization")]
        [XmlAttribute("interface"), Browsable(false)]
        public String InterfaceString
        {
            get { return (Interface == null ? null : Interface.ToString()); }
            set { Interface = (value == null ? null : new Uri(value)); }
        }

        // Preserve order, duplicate entries are not allowed
        private readonly C5.HashedArrayList<ImplementationSelection> _implementations = new C5.HashedArrayList<ImplementationSelection>();
        /// <summary>
        /// A list of <see cref="ImplementationSelection"/>s chosen in this selection.
        /// </summary>
        [Description("A list of implementations chosen in this selection.")]
        [XmlElement("selection")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.HashedArrayList<ImplementationSelection> Implementations { get { return _implementations; } }
        #endregion

        //--------------------//

        #region Query
        /// <summary>
        /// Lists <see cref="Implementation"/>s that are references here but not available in <paramref name="implementationStore"/>.
        /// </summary>
        /// <param name="implementationStore">The store to to check for cached implementations.</param>
        /// <param name="interfaceProvider">The interface source used when additional information about <see cref="ImplementationSelection"/>s need to be requested.</param>
        /// <returns>The actual <see cref="Implementation"/>s (taken from <see cref="InterfaceProvider"/>) instead of the <see cref="ImplementationSelection"/>s.</returns>
        public IEnumerable<Implementation> GetUncachedImplementations(IStore implementationStore, InterfaceProvider interfaceProvider)
        {
            #region Sanity checks
            if (implementationStore == null) throw new ArgumentNullException("implementationStore");
            if (interfaceProvider == null) throw new ArgumentNullException("interfaceProvider");
            #endregion

            ICollection<Implementation> notCached = new LinkedList<Implementation>();

            foreach (var implementation in Implementations)
            {
                // Local paths are considered to be always available
                if (!string.IsNullOrEmpty(implementation.LocalPath)) continue;

                // Check if an implementation with a matching digest is available in the cache
                if (implementationStore.Contains(implementation.ManifestDigest)) continue;

                // If not, get download information for the implementation by checking the original interface file
                var interfaceInfo = interfaceProvider.GetInterface(implementation.Interface);
                interfaceInfo.Simplify();
                notCached.Add(interfaceInfo.GetImplementation(implementation.ID));
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
        /// <exception cref="IOException">Thrown if the file couldn't be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        public static Selections Load(string path)
        {
            return XmlStorage.Load<Selections>(path);
        }

        /// <summary>
        /// Loads <see cref="Selections"/> from a stream containing an XML file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Selections"/>.</returns>
        public static Selections Load(Stream stream)
        {
            return XmlStorage.Load<Selections>(stream);
        }

        /// <summary>
        /// Saves these <see cref="Selections"/> to an XML file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if the file couldn't be created.</exception>
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
            var newSelections = new Selections {Interface = Interface};
            foreach (var implementation in Implementations)
            {
                newSelections.Implementations.Add(implementation.CloneImplementation());
            }
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
        public bool Equals(Selections other)
        {
            if (ReferenceEquals(null, other)) return false;

            if (Interface != other.Interface) return false;

            if (other._implementations.Count != _implementations.Count) return false;
            for (int i = 0; i < _implementations.Count; i++)
            {
                // If any implementation pair does not match, the selections are not equal
                if (!Equals(_implementations[i], other._implementations[i])) return false;
            }

            // If the for-loop ran through, all node pairs are identical and the selections are equal
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Selections) && Equals((Selections)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Interface != null ? Interface.GetHashCode() : 0);
                foreach (var implementation in _implementations)
                    result = (result * 397) ^ implementation.GetHashCode();
                //foreach (var implementation in _packageImplementation)
                //    result = (result * 397) ^ implementation.GetHashCode();
                return result;
            }
        }
        #endregion
    }
}
