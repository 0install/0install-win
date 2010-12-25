/*
 * Copyright 2010 Bastian Eicher
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
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using ZeroInstall.Store.Feed;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Management.WinForms.Nodes;
using ZeroInstall.Store.Management.WinForms.Properties;

namespace ZeroInstall.Store.Management.WinForms
{
    /// <summary>
    /// Displays the content of caches (<see cref="FeedCache"/> and <see cref="IStore"/>) in a combined tree view.
    /// </summary>
    public sealed partial class MainForm : Form
    {
        #region Variables
        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<StoreNode> _treeView = new FilteredTreeView<StoreNode> {Separator = '#', CheckBoxes = true, Dock = DockStyle.Fill};
        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();

            _treeView.Entries = GetStoreNodes();
            splitContainer.Panel1.Controls.Add(_treeView);

            _treeView.SelectedEntryChanged += delegate { propertyGrid.SelectedObject = _treeView.SelectedEntry; };
            _treeView.CheckedEntriesChanged += delegate { buttonDelete.Enabled = (_treeView.CheckedEntries.Length != 0); };
        }
        #endregion

        //--------------------//

        #region Build tree list
        private static INamedCollection<StoreNode> GetStoreNodes()
        {
            // ToDo: Add exception handling

            var nodes = new NamedCollection<StoreNode>();

            var cache = new FeedCache();
            var feeds = cache.GetAll();

            foreach (var feed in feeds)
            {
                feed.Simplify();
                nodes.Add(new InterfaceNode(cache, feed));
            }

            var store = StoreProvider.Default;
            foreach (var digest in store.ListAll())
            {
                bool parentFeedFound = false;
                foreach (var feed in feeds)
                {
                    var implementation = feed.GetImplementation(digest);
                    if (implementation != null)
                    {
                        nodes.Add(new OwnedImplementationNode(store, digest, new InterfaceNode(cache, feed), implementation));
                        parentFeedFound = true;
                        break;
                    }
                }

                if (!parentFeedFound) nodes.Add(new OrphanedImplementationNode(store, digest));
            }

            return nodes;
        }

        private void RefreshList()
        {
            _treeView.Entries = GetStoreNodes();
            _treeView.SelectedEntry = null;
            buttonDelete.Enabled = false;
        }
        #endregion

        #region Event handlers
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (!Msg.Ask(this, string.Format(Resources.DeleteCheckedEntries, _treeView.CheckedEntries.Length), MsgSeverity.Warning, Resources.YesDelete, Resources.NoKeep))
                return;

            try
            {
                foreach (StoreNode entry in _treeView.CheckedEntries)
                    entry.Delete();
            }
            #region Error handling
            catch (KeyNotFoundException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion

            RefreshList();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
