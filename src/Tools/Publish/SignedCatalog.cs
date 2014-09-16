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
using System.IO;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// A wrapper around a <see cref="Catalog"/> adding and XSL stylesheet and a digital signature.
    /// </summary>
    [Serializable]
    public class SignedCatalog
    {
        #region Properties
        /// <summary>
        /// The wrapped <see cref="Catalog"/>.
        /// </summary>
        public Catalog Catalog { get; private set; }

        /// <summary>
        /// The secret key used to sign the <see cref="Catalog"/>; <see langword="null"/> for no signature.
        /// </summary>
        public OpenPgpSecretKey SecretKey { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new signed catalog.
        /// </summary>
        /// <param name="catalog">The wrapped <see cref="Catalog"/>.</param>
        /// <param name="secretKey">The secret key used to sign the <see cref="Catalog"/>; <see langword="null"/> for no signature.</param>
        public SignedCatalog(Catalog catalog, OpenPgpSecretKey secretKey)
        {
            #region Sanity checks
            if (catalog == null) throw new ArgumentNullException("catalog");
            #endregion

            Catalog = catalog;
            SecretKey = secretKey;
        }
        #endregion

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a <see cref="Catalog"/> from an XML file and identifies the signature (if any).
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="SignedCatalog"/>.</returns>
        /// <exception cref="IOException">A problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        public static SignedCatalog Load(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return new SignedCatalog(XmlStorage.LoadXml<Catalog>(path), FeedUtils.GetKey(path, OpenPgpFactory.CreateDefault()));
        }

        /// <summary>
        /// Saves <see cref="Catalog"/> to an XML file, adds the default stylesheet and sign it it with <see cref="SecretKey"/> (if specified).
        /// </summary>
        /// <remarks>Writing and signing the catalog file are performed as an atomic operation (i.e. if signing fails an existing file remains unchanged).</remarks>
        /// <param name="path">The file to save in.</param>
        /// <param name="passphrase">The passphrase to use to unlock the secret key; may be <see langword="null"/> if <see cref="SecretKey"/> is <see langword="null"/>.</param>
        /// <exception cref="IOException">A problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
        /// <exception cref="WrongPassphraseException">Passphrase was incorrect.</exception>
        public void Save(string path, string passphrase = null)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            using (var atomic = new AtomicWrite(path))
            {
                // Write to temporary file first
                Catalog.SaveXml(atomic.WritePath);

                XmlStorage.AddStylesheet(atomic.WritePath, "catalog.xsl");
                if (SecretKey != null) FeedUtils.SignFeed(atomic.WritePath, SecretKey, passphrase, OpenPgpFactory.CreateDefault());

                atomic.Commit();
            }
        }
        #endregion
    }
}
