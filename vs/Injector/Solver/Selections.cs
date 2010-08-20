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

namespace ZeroInstall.Injector.Solver
{
    /// <summary>
    /// Represents a number of <see cref="ImplementationBase"/>s chosen for executing an <see cref="Model.Feed"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlRoot("selections", Namespace = "http://zero-install.sourceforge.net/2004/injector/interface")]
    public sealed class Selections : IEquatable<Selections>, ICloneable
    {
        #region Properties
        /// <summary>
        /// The URI or local path of the interface this selection is based on.
        /// </summary>
        [Description("The URI or local path of the interface this selection is based on.")]
        [XmlAttribute("interface")]
        public string Interface { get; set; }
        
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
        /// Returns the <see cref="ImplementationSelection"/> for a specific interface.
        /// </summary>
        /// <param name="interfaceID">The <see cref="ImplementationSelection.Interface"/> to look for.</param>
        /// <returns>The identified <see cref="ImplementationBase"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if no <see cref="ImplementationSelection"/> matching <paramref name="interfaceID"/> was found in <see cref="Implementations"/>.</exception>
        public ImplementationSelection GetSelection(string interfaceID)
        {
            foreach (var implementation in _implementations)
            {
                if (implementation.Interface == interfaceID) return implementation;
            }
            throw new KeyNotFoundException();
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
        /// Loads <see cref="Selections"/> from an XML string.
        /// </summary>
        /// <param name="data">The XML string to be parsed.</param>
        /// <returns>The loaded <see cref="Selections"/>.</returns>
        public static Selections LoadFromString(string data)
        {
            return XmlStorage.FromString<Selections>(data);
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

            return (Interface == other.Interface) && Implementations.SequencedEquals(other.Implementations);
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
                result = (result * 397) ^ Implementations.GetSequencedHashCode();
                return result;
            }
        }
        #endregion
    }
}
