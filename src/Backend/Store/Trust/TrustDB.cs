/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// A database of OpenPGP the users trusts to sign <see cref="Feed"/>s.
    /// </summary>
    [XmlRoot("trusted-keys", Namespace = XmlNamespace), XmlType("trusted-keys", Namespace = XmlNamespace)]
    [XmlNamespace("xsi", XmlStorage.XsiNamespace)]
    public sealed class TrustDB : ICloneable, IEquatable<TrustDB>
    {
        #region Constants
        /// <summary>
        /// The XML namespace used for storing trust-related data.
        /// </summary>
        public const string XmlNamespace = "http://zero-install.sourceforge.net/2007/injector/trust";

        /// <summary>
        /// The URI to retrieve an XSD containing the XML Schema information for this class in serialized form.
        /// </summary>
        public const string XsdLocation = "http://0install.de/schema/injector/trust/trust.xsd";

        /// <summary>
        /// Provides XML Editors with location hints for XSD files.
        /// </summary>
        [XmlAttribute("schemaLocation", Namespace = XmlStorage.XsiNamespace)]
        public string XsiSchemaLocation = XmlNamespace + " " + XsdLocation;
        #endregion

        #region Properties
        // Order is preserved, but ignore it when comparing
        private readonly List<Key> _keys = new List<Key>();

        /// <summary>
        /// A list of known <see cref="Key"/>s.
        /// </summary>
        [XmlElement("key")]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public List<Key> Keys
        {
            get { return _keys; }
        }
        #endregion

        //--------------------//

        #region Access
        /// <summary>
        /// Determines whether a key is trusted for a specific domain.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key to check.</param>
        /// <param name="domain">The domain the key should be valid for.</param>
        public bool IsTrusted(string fingerprint, Domain domain)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException("fingerprint");
            #endregion

            return Keys.Any(key => key.Fingerprint == fingerprint && key.Domains.Contains(domain));
        }

        /// <summary>
        /// Marks a key as trusted for a specific domain.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key to check.</param>
        /// <param name="domain">The domain the key should be valid for.</param>
        public void TrustKey(string fingerprint, Domain domain)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException("fingerprint");
            #endregion

            Key targetKey = Keys.FirstOrDefault(key => key.Fingerprint == fingerprint);
            if (targetKey == null)
            {
                targetKey = new Key {Fingerprint = fingerprint};
                Keys.Add(targetKey);
            }

            targetKey.Domains.Add(domain);
        }

        /// <summary>
        /// Marks a key as no longer trusted for a specific domain.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key to check.</param>
        /// <param name="domain">The domain the key should be valid for.</param>
        public void UntrustKey(string fingerprint, Domain domain)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException("fingerprint");
            #endregion

            foreach (Key key in Keys.Where(key => key.Fingerprint == fingerprint))
                key.Domains.Remove(domain);
        }
        #endregion

        #region Storage
        /// <summary>
        /// Loads a <see cref="TrustDB"/> from an XML file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="TrustDB"/>.</returns>
        /// <exception cref="IOException">A problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        public static TrustDB Load(string path)
        {
            return XmlStorage.LoadXml<TrustDB>(path);
        }

        /// <summary>
        /// Loads the <see cref="TrustDB"/> from its default location.
        /// </summary>
        /// <returns>The loaded <see cref="TrustDB"/>.</returns>
        /// <exception cref="IOException">A problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        public static TrustDB Load()
        {
            return Load(Locations.GetSaveConfigPath("0install.net", true, "injector", "trustdb.xml"));
        }

        /// <summary>
        /// Tries to load the <see cref="TrustDB"/> from its default location. Automatically falls back to defaults on errors.
        /// </summary>
        /// <returns>The loaded <see cref="TrustDB"/>.</returns>
        public static TrustDB LoadSafe()
        {
            try
            {
                return Load();
            }
                #region Error handling
            catch (FileNotFoundException)
            {
                // Creat new trust database
                return new TrustDB();
            }
            catch (IOException ex)
            {
                Log.Warn(Resources.ErrorLoadingTrustDB);
                Log.Warn(ex);
                return new TrustDB();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(Resources.ErrorLoadingTrustDB);
                Log.Warn(ex);
                return new TrustDB();
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(Resources.ErrorLoadingTrustDB);
                Log.Warn(ex);
                return new TrustDB();
            }
            #endregion
        }

        /// <summary>
        /// Saves the this <see cref="TrustDB"/> to an XML file.
        /// </summary>
        /// <exception cref="IOException">A problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        public void Save()
        {
            this.SaveXml(Locations.GetSaveConfigPath("0install.net", true, "injector", "trustdb.xml"));
        }
        #endregion

        //--------------------//

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="TrustDB"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="TrustDB"/>.</returns>
        public TrustDB Clone()
        {
            var trust = new TrustDB();
            foreach (var key in Keys) trust.Keys.Add(key.Clone());

            return trust;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(TrustDB other)
        {
            if (other == null) return false;
            return Keys.UnsequencedEquals(other.Keys);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is TrustDB && Equals((TrustDB)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Keys.GetUnsequencedHashCode();
        }
        #endregion
    }
}
