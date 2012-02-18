/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Tasks;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Management.WinForms.Properties;

namespace ZeroInstall.Store.Management.WinForms.Nodes
{
    /// <summary>
    /// Models information about a temporary directory in an <see cref="IStore"/> for display in a GUI.
    /// </summary>
    public sealed class TempDirectoryNode : StoreNode
    {
        #region Variables
        private readonly IStore _store;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public override string Name { get { return Resources.TemporaryDirectories + "#" + Directory + (SuffixCounter == 0 ? "" : " " + SuffixCounter); } set { throw new NotSupportedException(); } }

        /// <summary>
        /// The name of the directory in the store.
        /// </summary>
        [Description("The name of the directory in the store.")]
        public string Directory { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new temporary directory node.
        /// </summary>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <param name="directory">The name of the directory in the store.</param>
        /// <param name="parent">The window containing this node. Used for callbacks.</param>
        /// <exception cref="FormatException">Thrown if the manifest file is not valid.</exception>
        /// <exception cref="IOException">Thrown if the manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        public TempDirectoryNode(IStore store, string directory, MainForm parent) : base(parent)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _store = store;
            Directory = directory;
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
                _store.Remove(Directory);
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
        public override void Verify(ITaskHandler handler)
        {}
        #endregion

        #region Context menu
        /// <inheritdoc/>
        public override ContextMenu GetContextMenu()
        {
            return new ContextMenu(new[]
            {
                new MenuItem(Resources.Remove, delegate
                {
                    if (Msg.YesNo(Parent, Resources.DeleteEntry, MsgSeverity.Warn, Resources.YesDelete, Resources.NoKeep))
                    {
                        try
                        {
                            Parent.RunTask(new SimpleTask(Resources.DeletingDirectory, Delete), null);
                        }
                            #region Error handling
                        catch (KeyNotFoundException ex)
                        {
                            Msg.Inform(Parent, ex.Message, MsgSeverity.Error);
                        }
                        catch (IOException ex)
                        {
                            Msg.Inform(Parent, ex.Message, MsgSeverity.Error);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Msg.Inform(Parent, ex.Message, MsgSeverity.Error);
                        }
                        #endregion

                        Parent.RefreshList();
                    }
                })
            });
        }
        #endregion
    }
}
