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
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Management.WinForms.Properties;

namespace ZeroInstall.Store.Management.WinForms.Nodes
{
    /// <summary>
    /// Models information about an implementation in an <see cref="IStore"/> for display in a GUI.
    /// </summary>
    public abstract class ImplementationNode : StoreNode
    {
        #region Variables
        private readonly ManifestDigest _digest;
        #endregion

        #region Properities
        /// <inheritdoc/>
        public override string Path
        {
            get { return Store.GetPath(_digest); }
        }

        /// <summary>
        /// The digest identifying the implementation in the store.
        /// </summary>
        [Description("The digest identifying the implementation in the store.")]
        public string Digest { get { return _digest.BestDigest; } }

        /// <summary>
        /// The total size of the implementation in bytes.
        /// </summary>
        [Browsable(false)]
        public long Size { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new implementation node.
        /// </summary>
        /// <param name="parent">The window containing this node. Used for callbacks.</param>
        /// <param name="store">The <see cref="IStore"/> the implementation is located in.</param>
        /// <param name="digest">The digest identifying the implementation.</param>
        /// <exception cref="FormatException">Thrown if the manifest file is not valid.</exception>
        /// <exception cref="IOException">Thrown if the manifest file could not be read.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        protected ImplementationNode(MainForm parent, IStore store, ManifestDigest digest)
            : base(parent, store)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            #endregion

            _digest = digest;

            // Determine the total size of an implementation via its manifest file
            string path = store.GetPath(digest);
            if (path == null) return;
            string manifestPath = System.IO.Path.Combine(path, ".manifest");
            Size = Manifest.Load(manifestPath, ManifestFormat.FromPrefix(digest.BestDigest)).TotalSize;
        }
        #endregion

        //--------------------//

        #region Delete
        /// <summary>
        /// Deletes this implementation from the <see cref="IStore"/> it is located in.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown if no matching implementation could be found in the <see cref="IStore"/>.</exception>
        /// <exception cref="IOException">Thrown if the implementation could not be deleted.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the store is not permitted.</exception>
        public override void Delete()
        {
            try
            {
                Store.Remove(_digest);
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
        /// <inheritdoc/>
        public override void Verify(ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            Store.Verify(_digest, handler);
        }
        #endregion

        #region Context menu
        /// <inheritdoc/>
        public override ContextMenu GetContextMenu()
        {
            var contextMenu = base.GetContextMenu();
            contextMenu.MenuItems.Add(
                new MenuItem(Resources.Verify, delegate
                {
                    try
                    {
                        Verify(Parent);
                        Msg.Inform(Parent, Resources.ImplementationOK, MsgSeverity.Info);
                    }
                        #region Error handling
                    catch (OperationCanceledException)
                    {}
                    catch (IOException ex)
                    {
                        Msg.Inform(Parent, ex.Message, MsgSeverity.Warn);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Msg.Inform(Parent, ex.Message, MsgSeverity.Warn);
                    }
                    catch (DigestMismatchException ex)
                    {
                        Msg.Inform(Parent, ex.Message, MsgSeverity.Error);
                        // ToDo: Provide option for deleting
                    }
                    #endregion
                }));
            return contextMenu;
        }
        #endregion
    }
}
