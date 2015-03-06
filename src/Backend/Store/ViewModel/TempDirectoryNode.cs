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
using JetBrains.Annotations;
using NanoByte.Common.Storage;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.ViewModel
{
    /// <summary>
    /// Models information about a temporary directory in an <see cref="IStore"/> for display in a UI.
    /// </summary>
    public sealed class TempDirectoryNode : StoreNode
    {
        #region Dependencies
        private readonly string _path;

        /// <summary>
        /// Creates a new temporary directory node.
        /// </summary>
        /// <param name="path">The path of the directory in the store.</param>
        /// <param name="store">The <see cref="IStore"/> the directory is located in.</param>
        /// <exception cref="FormatException">The manifest file is not valid.</exception>
        /// <exception cref="IOException">The manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
        public TempDirectoryNode([NotNull] string path, [NotNull] IStore store)
            : base(store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _path = path;
        }
        #endregion

        /// <inheritdoc/>
        public override string Name { get { return Resources.TemporaryDirectories + "\\" + System.IO.Path.GetFileName(_path) + (SuffixCounter == 0 ? "" : " " + SuffixCounter); } set { throw new NotSupportedException(); } }

        /// <inheritdoc/>
        public override string Path { get { return _path; } }

        /// <summary>
        /// Deletes this temporary directory from the <see cref="IStore"/> it is located in.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">The directory could be found in the store.</exception>
        /// <exception cref="IOException">The directory could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Write access to the store is not permitted.</exception>
        public override void Delete()
        {
            try
            {
                DirectoryStore.DisableWriteProtection(_path);
                Directory.Delete(_path, recursive: true);
            }
                #region Error handling
            catch (ImplementationNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }
            #endregion
        }
    }
}
