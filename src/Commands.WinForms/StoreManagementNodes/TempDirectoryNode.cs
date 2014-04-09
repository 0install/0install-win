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
using NanoByte.Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands.WinForms.StoreManagementNodes
{
    /// <summary>
    /// Models information about a temporary directory in an <see cref="IStore"/> for display in a GUI.
    /// </summary>
    public sealed class TempDirectoryNode : StoreNode
    {
        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return Resources.TemporaryDirectories + "#" + System.IO.Path.GetFileName(_path) + (SuffixCounter == 0 ? "" : " " + SuffixCounter); } set { throw new NotSupportedException(); } }

        private readonly string _path;

        /// <inheritdoc/>
        public override string Path { get { return _path; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new temporary directory node.
        /// </summary>
        /// <param name="parent">The window containing this node. Used for callbacks.</param>
        /// <param name="store">The <see cref="IStore"/> the directory is located in.</param>
        /// <param name="path">The path of the directory in the store.</param>
        /// <exception cref="FormatException">Thrown if the manifest file is not valid.</exception>
        /// <exception cref="IOException">Thrown if the manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        public TempDirectoryNode(StoreManageForm parent, IStore store, string path) : base(parent, store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _path = path;
        }
        #endregion

        //--------------------//

        #region Delete
        /// <summary>
        /// Deletes this temporary directory from the <see cref="IStore"/> it is located in.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException">Thrown if the directory could be found in the store.</exception>
        /// <exception cref="IOException">Thrown if the directory could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the store is not permitted.</exception>
        public override void Delete()
        {
            try
            {
                try
                {
                    FileUtils.DisableWriteProtection(_path);
                }
                    #region Error handling
                catch (IOException)
                {
                    // Ignore since we may be able to delete it anyway
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignore since we may be able to delete it anyway
                }
                #endregion

                Directory.Delete(_path, recursive: true);
            }
                #region Error handling
            catch (ImplementationNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }
            #endregion
        }
        #endregion

        #region Verify
        /// <summary>
        /// Does nothing.
        /// </summary>
        public override void Verify(IInteractionHandler handler)
        {}
        #endregion
    }
}
