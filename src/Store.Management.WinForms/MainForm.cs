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

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Management.WinForms.Nodes;
using ZeroInstall.Store.Management.WinForms.Properties;

namespace ZeroInstall.Store.Management.WinForms
{
    /// <summary>
    /// Displays the content of caches (<see cref="IFeedCache"/> and <see cref="IStore"/>) in a combined tree view.
    /// </summary>
    public sealed partial class MainForm : Form, ITaskHandler
    {
        #region Variables
        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<StoreNode> _treeView = new FilteredTreeView<StoreNode> {Separator = '#', CheckBoxes = true, Dock = DockStyle.Fill};
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public MainForm()
        {
            InitializeComponent();

            _treeView.SelectedEntryChanged += OnSelectedEntryChanged;
            _treeView.CheckedEntriesChanged += OnCheckedEntriesChanged;
            splitContainer.Panel1.Controls.Add(_treeView);
        }
        #endregion

        #region Load
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Locations.IsPortable) Text += " - Portable mode";

            RefreshList();
        }
        #endregion

        //--------------------//

        #region Build tree list
        /// <summary>
        /// Fills the <see cref="_treeView"/> with entries.
        /// </summary>
        internal void RefreshList()
        {
            try
            {
                var nodes = new NamedCollection<StoreNode>();

                var feedCache = FeedCacheProvider.CreateDefault();
                var feeds = FeedUtils.GetFeeds(feedCache);
                foreach (Feed feed in feeds)
                    AddWithIncrement(nodes, new FeedNode(feedCache, feed, this));

                long totalSize = 0;
                IStore store;
                try { store = StoreProvider.CreateDefault(); }
                #region Error handling
                catch (IOException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    Close();
                    return;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    Close();
                    return;
                }
                #endregion

                foreach (var digest in store.ListAll())
                {
                    try
                    {
                        Feed feed;
                        var implementation = ImplementationUtils.GetImplementation(digest, feeds, out feed);

                        ImplementationNode implementationNode;
                        if (feed == null) implementationNode = new OrphanedImplementationNode(store, digest, this);
                        else implementationNode = new OwnedImplementationNode(store, digest, new FeedNode(feedCache, feed, this), implementation, this);

                        totalSize += implementationNode.Size;
                        AddWithIncrement(nodes, implementationNode);
                    }
                    #region Error handling
                    // ToDo: Display less intrusive messages
                    catch (FormatException ex)
                    {
                        Msg.Inform(this, string.Format("Problem processing the manifest file for '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
                    }
                    catch (IOException ex)
                    {
                        Msg.Inform(this, string.Format("Problem processing '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Msg.Inform(this, string.Format("Problem processing '{0}'.\n" + ex.Message, digest), MsgSeverity.Error);
                    }
                    #endregion
                }

                _treeView.Entries = nodes;
                _treeView.SelectedEntry = null;
                buttonVerify.Enabled = buttonRemove.Enabled = false;

                // Update total size
                textTotalSize.Text = StringUtils.FormatBytes(totalSize);
            }
            #region Error handling
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

            OnCheckedEntriesChanged(null, EventArgs.Empty);
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
                buttonVerify.Enabled = buttonRemove.Enabled = false;
                textCheckedSize.Text = @"-";
            }
            else
            {
                buttonVerify.Enabled = buttonRemove.Enabled = true;

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

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (!Msg.Ask(this, string.Format(Resources.DeleteCheckedEntries, _treeView.CheckedEntries.Length), MsgSeverity.Warn, Resources.YesDelete, Resources.NoKeep))
                return;

            try
            {
                foreach (StoreNode entry in _treeView.CheckedEntries)
                    entry.Delete(this);
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

        private void buttonVerify_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (StoreNode entry in _treeView.CheckedEntries)
                    entry.Verify(this);
            }
            #region Error handling
            catch (UserCancelException)
            {
                return;
            }
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                return;
            }
            catch (DigestMismatchException ex)
            {
                // ToDo: Display manifest diff
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                // ToDo: Provide option for deleting
                return;
            }
            #endregion

            Msg.Inform(this, Resources.AllImplementationsOK, MsgSeverity.Info);
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

        #region Handler
        /// <summary>
        /// Not used.
        /// </summary>
        public bool Batch { get; set; }

        /// <inheritdoc/>
        public void RunTask(ITask task, object tag)
        {
            // Handle events coming from a non-UI thread, block caller
            Invoke((SimpleEventHandler)(() => TrackingDialog.Run(this, task, Icon)));
        }
        #endregion
    }
}
