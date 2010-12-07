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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Serialization;
using Common.Storage;
using ZeroInstall.Model;

namespace ZeroInstall.Store.Feed
{
    /// <summary>
    /// A database of trusted GPG signatures for <see cref="Feed"/>s.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "C5 collections don't need to be disposed.")]
    [XmlRoot("trusted-keys", Namespace = XmlNamespace)]
    [XmlType("trusted-keys", Namespace = XmlNamespace)]
    public sealed class Trust : ICloneable, IEquatable<Trust>
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing trust-related data.
        /// </summary>
        public const string XmlNamespace = "http://zero-install.sourceforge.net/2007/injector/trust";
        #endregion

        #region Properties
        // Order is preserved, but ignore it when comparing
        private readonly C5.ArrayList<Key> _keys = new C5.ArrayList<Key>();
        /// <summary>
        /// A list of <see cref="Domain"/>s this key is valid for.
        /// </summary>
        [XmlElement("key")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public C5.ArrayList<Key> Keys { get { return _keys; } }
        #endregion

        //--------------------//

        #region Storage

        #region Load
        /// <summary>
        /// Loads a <see cref="Trust"/> database from an XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Trust"/> databse.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public static Trust Load(string path)
        {
            return XmlStorage.Load<Trust>(path);
        }
        #endregion

        #region Save
        /// <summary>
        /// Saves the this <see cref="Trust"/> database to an XML file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            XmlStorage.Save(path, this);
        }
        #endregion

        #endregion
        
        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="Trust"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="Trust"/>.</returns>
        public Trust CloneTrust()
        {
            var trust = new Trust();
            foreach (var key in Keys) trust.Keys.Add(key.CloneKey());

            return trust;
        }

        public object Clone()
        {
            return CloneTrust();
        }
        #endregion

        #region Equality
        public bool Equals(Trust other)
        {
            if (ReferenceEquals(null, other)) return false;

            return Keys.UnsequencedEquals(other.Keys);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(Trust) && Equals((Trust)obj);
        }

        public override int GetHashCode()
        {
            return Keys.GetUnsequencedHashCode();
        }
        #endregion
    }
}