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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Commands.WinForms.StoreManagementNodes
{
    /// <summary>
    /// Models information about elements in a cache for display in a GUI.
    /// </summary>
    public abstract class StoreNode : Node
    {
        #region Variables
        /// <summary>
        /// The store containing the element.
        /// </summary>
        protected readonly IStore Store;
        #endregion

        #region Properties
        /// <summary>
        /// The file system path of the element.
        /// </summary>
        [Description("The file system path of the element.")]
        public abstract string Path { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store node.
        /// </summary>
        /// <param name="parent">The window containing this node. Used for callbacks.</param>
        /// <param name="store">The store containing the element.</param>
        protected StoreNode(StoreManageForm parent, IStore store) : base(parent)
        {
            #region Sanity checks
            if (parent == null) throw new ArgumentNullException("parent");
            #endregion

            Store = store;
        }
        #endregion

        //--------------------//

        #region Context menu
        /// <inheritdoc/>
        public override ContextMenu GetContextMenu()
        {
            return new ContextMenu(new[]
            {
                new MenuItem(Resources.OpenInFileManager, delegate { if (Path != null) Process.Start(Path); }),
                new MenuItem(Resources.Remove, delegate
                {
                    if (Msg.YesNo(Parent, Resources.DeleteEntry, MsgSeverity.Warn))
                    {
                        try
                        {
                            Parent.RunTask(new SimpleTask(Resources.DeletingImplementations, Delete));
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
