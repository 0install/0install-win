/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Trust
{
    /// <summary>
    /// A database of OpenPGP signature fingerprints the users trusts to sign <see cref="Feed"/>s coming from specific domains.
    /// </summary>
    [XmlRoot("trusted-keys", Namespace = XmlNamespace), XmlType("trusted-keys", Namespace = XmlNamespace)]
    [XmlNamespace("xsi", XmlStorage.XsiNamespace)]
    public sealed class TrustDB : ICloneable<TrustDB>, IEquatable<TrustDB>
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

        // Order is preserved, but ignore it when comparing

        /// <summary>
        /// A list of known <see cref="Key"/>s.
        /// </summary>
        [XmlElement("key"), NotNull]
        // Note: Can not use ICollection<T> interface with XML Serialization
        public List<Key> Keys { get; } = new List<Key>();

        /// <summary>
        /// Determines whether a key is trusted for a specific domain.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key to check.</param>
        /// <param name="domain">The domain the key should be valid for.</param>
        public bool IsTrusted([NotNull] string fingerprint, Domain domain)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException(nameof(fingerprint));
            #endregion

            return Keys.Any(key => key.Fingerprint == fingerprint && key.Domains.Contains(domain));
        }

        /// <summary>
        /// Marks a key as trusted for a specific domain.
        /// </summary>
        /// <param name="fingerprint">The fingerprint of the key to check.</param>
        /// <param name="domain">The domain the key should be valid for.</param>
        public void TrustKey([NotNull] string fingerprint, Domain domain)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException(nameof(fingerprint));
            #endregion

            Log.Debug($"Trusting {fingerprint} for {domain}");

            var targetKey = Keys.FirstOrDefault(key => key.Fingerprint == fingerprint);
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
        public void UntrustKey([NotNull] string fingerprint, Domain domain)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(fingerprint)) throw new ArgumentNullException(nameof(fingerprint));
            #endregion

            Log.Debug($"Untrusting {fingerprint} for {domain}");

            foreach (var key in Keys.Where(key => key.Fingerprint == fingerprint))
                key.Domains.Remove(domain);
        }

        #region Storage
        [CanBeNull]
        private string _filePath;

        /// <summary>
        /// Loads the <see cref="TrustDB"/> from a file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="TrustDB"/>.</returns>
        /// <exception cref="IOException">A problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        [NotNull]
        public static TrustDB Load([NotNull] string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            #endregion

            Log.Debug("Loading trust database from: " + path);
            var trustDB = XmlStorage.LoadXml<TrustDB>(path);
            trustDB._filePath = path;
            return trustDB;
        }

        /// <summary>
        /// Tries to load the <see cref="TrustDB"/> from the default location.
        /// </summary>
        /// <returns>The loaded <see cref="TrustDB"/>.</returns>
        /// <exception cref="IOException">A problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        [NotNull]
        public static TrustDB Load()
            => Load(Locations.GetSaveConfigPath("0install.net", true, "injector", "trustdb.xml"));

        /// <summary>
        /// Tries to load the <see cref="TrustDB"/> from the default location. Automatically falls back to defaults on errors.
        /// </summary>
        /// <returns>The loaded <see cref="TrustDB"/> or an empty <see cref="TrustDB"/> if there was a problem.</returns>
        [NotNull]
        public static TrustDB LoadSafe()
        {
            try
            {
                return Load();
            }
                #region Error handling
            catch (FileNotFoundException)
            {
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
        /// Saves the this <see cref="TrustDB"/> to the location it was loaded from if possible.
        /// </summary>
        /// <exception cref="IOException">A problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        public void Save()
        {
            if (_filePath == null)
            {
                Log.Warn("Trust database was not loaded from disk and can therefore not be saved");
                return;
            }

            Log.Debug("Saving trust database to: " + _filePath);
            this.SaveXml(_filePath);
        }
        #endregion

        #region Clone
        /// <summary>
        /// Creates a deep copy of this <see cref="TrustDB"/> instance.
        /// </summary>
        /// <returns>The new copy of the <see cref="TrustDB"/>.</returns>
        public TrustDB Clone()
        {
            var trust = new TrustDB();
            trust.Keys.AddRange(Keys.CloneElements());
            return trust;
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
            if (obj == null) return false;
            if (obj == this) return true;
            var other = obj as TrustDB;
            return other != null && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode() => Keys.GetUnsequencedHashCode();
        #endregion
    }
}
