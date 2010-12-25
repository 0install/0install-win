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
using Common.Utils;
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

            _treeView.SelectedEntryChanged += OnSelectedEntryChanged;
            _treeView.CheckedEntriesChanged += OnCheckedEntriesChanged;
            splitContainer.Panel1.Controls.Add(_treeView);

            try { RefreshList(); }
            #region Sanity checks
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (InvalidOperationException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion

        //--------------------//

        #region Build tree list
        /// <summary>
        /// Fills the <see cref="_treeView"/> with entries.
        /// </summary>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the cache is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        private void RefreshList()
        {
            var nodes = new NamedCollection<StoreNode>();

            #region Interface node
            var cache = new FeedCache();
            var feeds = cache.GetAll();
            foreach (var feed in feeds)
            {
                feed.Simplify();
                AddWithIncrement(nodes, new InterfaceNode(cache, feed));
            }
            #endregion

            long totalSize = 0;
            var store = StoreProvider.Default;
            foreach (var digest in store.ListAll())
            {
                #region Owned implementation node
                ImplementationNode implementationNode = null;
                foreach (var feed in feeds)
                {
                    var implementation = feed.GetImplementation(digest);
                    if (implementation != null)
                    {
                        implementationNode = new OwnedImplementationNode(store, digest, new InterfaceNode(cache, feed), implementation);
                        break;
                    }
                }
                #endregion

                #region Orphaned implementation node
                if (implementationNode == null)
                    implementationNode = new OrphanedImplementationNode(store, digest);
                #endregion

                totalSize += implementationNode.Size;
                AddWithIncrement(nodes, implementationNode);
            }

            _treeView.Entries = nodes;
            _treeView.SelectedEntry = null;
            buttonDelete.Enabled = false;

            // Update total size
            textTotalSize.Text = StringUtils.FormatBytes(totalSize);
        }

        /// <summary>
        /// Adds a <see cref="StoreNode"/> to a collection, incrementing <see cref="StoreNode.SuffixCounter"/> to prevent naming collisions.
        /// </summary>
        private static void AddWithIncrement(NamedCollection<StoreNode> collection, StoreNode entry)
        {
            while (collection.Contains(entry.Name))
                entry.SuffixCounter++;

            collection.Add(entry);
        }
        #endregion

        #region Event handlers
        private void OnSelectedEntryChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = _treeView.SelectedEntry;

            // Update current entry size
            var implementationEntry = _treeView.SelectedEntry as ImplementationNode;
            textCurrentSize.Text = (implementationEntry != null) ? StringUtils.FormatBytes(implementationEntry.Size) : "-";
        }

        private void OnCheckedEntriesChanged(object sender, EventArgs e)
        {
            if (_treeView.CheckedEntries.Length == 0)
            {
                buttonDelete.Enabled = false;
                textCheckedSize.Text = "-";
            }
            else
            {
                buttonDelete.Enabled = true;

                // Update selected entries size
                long totalSize = 0;
                foreach (var entry in _treeView.CheckedEntries)
                {
                    var implementationEntry = entry as ImplementationNode;
                    if (implementationEntry != null) totalSize += implementationEntry.Size;
                }
                textCheckedSize.Text = StringUtils.FormatBytes(totalSize);
            }
        }

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

            try { RefreshList(); }
            #region Sanity checks
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (InvalidOperationException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            try { RefreshList(); }
            #region Sanity checks
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (InvalidOperationException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
