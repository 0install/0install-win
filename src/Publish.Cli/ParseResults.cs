/*
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

using System.Collections.Generic;
using System.IO;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Publish.Cli
{
    /// <summary>
    /// List of operational modes for the feed editor that can be selected via command-line arguments.
    /// </summary>
    public enum OperationMode
    {
        /// <summary>Modify an existing <see cref="Feed"/> or create a new one.</summary>
        Normal,

        /// <summary>Combine all specified <see cref="Feed"/>s into a single <see cref="Catalog"/> file.</summary>
        Catalog
    }

    /// <summary>
    /// Structure for storing user-selected arguments for a feed editor operation.
    /// </summary>
    public struct ParseResults
    {
        /// <summary>
        /// The operational mode for the feed editor.
        /// </summary>
        public OperationMode Mode;

        /// <summary>
        /// The feeds to apply the operation on.
        /// </summary>
        public ICollection<FileInfo> Feeds;

        /// <summary>
        /// The file to store the aggregated <see cref="Catalog"/> data in.
        /// </summary>
        public string CatalogFile;

        /// <summary>
        /// Download missing archives, calculate manifest digests, etc..
        /// </summary>
        public bool AddMissing;

        /// <summary>
        /// Add any downloaded archives to the implementation store.
        /// </summary>
        public bool StoreDownloads;

        /// <summary>
        /// Add XML signature blocks to the feesd.
        /// </summary>
        public bool XmlSign;

        /// <summary>
        /// Remove any existing signatures from the feeds.
        /// </summary>
        public bool Unsign;

        /// <summary>
        /// A key specifier (key ID, fingerprint or any part of a user ID) for the secret key to use to sign the feeds.
        /// </summary>
        /// <remarks>Will use existing key or default key when left at <see langword="null"/>.</remarks>
        public string Key;

        /// <summary>
        /// The passphrase used to unlock the <see cref="OpenPgpSecretKey"/>.
        /// </summary>
        public string OpenPgpPassphrase;
    }
}
