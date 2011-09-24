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

using System.Collections.Generic;
using System.IO;
using ZeroInstall.Model;

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
        public OperationMode Mode;

        public ICollection<FileInfo> Feeds;

        public string CatalogFile;

        public bool AddMissing;

        public bool XmlSign;

        public bool Unsign;

        public string Key;

        public string GnuPGPassphrase;
    }
}
