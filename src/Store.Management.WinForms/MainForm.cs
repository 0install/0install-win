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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
        private readonly FilteredTreeView<Node> _treeView = new FilteredTreeView<Node> {Separator = '#', CheckBoxes = true, Dock = DockStyle.Fill};
        #endregion

        #region Constructor
        /// <inheritdoc/>
        public MainForm()
        {
            InitializeComponent();
            WindowsUtils.AddShieldIcon(buttonRunAsAdmin);

            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
            if (WindowsUtils.IsAdministrator) Text += @" (Administrator)";
            else if (WindowsUtils.IsWindowsNT) buttonRunAsAdmin.Visible = true;

            Shown += delegate { RefreshList(); };
            HandleCreated += delegate { Program.ConfigureTaskbar(this, Text, null, null); };

            _treeView.SelectedEntryChanged += OnSelectedEntryChanged;
            _treeView.CheckedEntriesChanged += OnCheckedEntriesChanged;
            splitContainer.Panel1.Controls.Add(_treeView);
        }
        #endregion

        //--------------------//

        #region Build tree list
        /// <summary>
        /// Fills the <see cref="_treeView"/> with entries.
        /// </summary>
        internal void RefreshList()
        {
            var waitDialog = new AsyncWaitDialog(Resources.ReadingCache, Icon);
            waitDialog.Start();
            try
            {
                var nodes = new NamedCollection<Node>();

                // List feeds/interfaces
                var feedCache = FeedCacheFactory.CreateDefault(OpenPgpFactory.CreateDefault());
                var feeds = FeedUtils.GetFeeds(feedCache);
                foreach (Feed feed in feeds)
                    AddWithIncrement(nodes, new FeedNode(this, feedCache, feed));

                long totalSize = 0;
                IStore store;
                try
                {
                    store = StoreFactory.CreateDefault();
                }
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

                // List implementations
                foreach (var digest in store.ListAll())
                {
                    try
                    {
                        Feed feed;
                        var implementation = ImplementationUtils.GetImplementation(digest, feeds, out feed);

                        ImplementationNode implementationNode;
                        if (feed == null) implementationNode = new OrphanedImplementationNode(this, store, digest);
                        else implementationNode = new OwnedImplementationNode(this, store, digest, new FeedNode(this, feedCache, feed), implementation);

                        totalSize += implementationNode.Size;
                        AddWithIncrement(nodes, implementationNode);
                    }
                        #region Error handling
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

                // List temporary directories
                foreach (string directory in store.ListAllTemp())
                    AddWithIncrement(nodes, new TempDirectoryNode(this, store, directory));

                _treeView.Nodes = nodes;
                _treeView.SelectedEntry = null;
                buttonVerify.Enabled = buttonRemove.Enabled = false;

                // Update total size
                textTotalSize.Text = totalSize.FormatBytes(CultureInfo.CurrentCulture);
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
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
            }
            #endregion

            waitDialog.Stop();

            OnCheckedEntriesChanged(null, EventArgs.Empty);
        }

        /// <summary>
        /// Adds a <see cref="Node"/> to a collection, incrementing <see cref="Node.SuffixCounter"/> to prevent naming collisions.
        /// </summary>
        private static void AddWithIncrement(NamedCollection<Node> collection, Node entry)
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
            textCurrentSize.Text = (implementationEntry != null) ? implementationEntry.Size.FormatBytes(CultureInfo.CurrentCulture) : "-";
        }

        private void OnCheckedEntriesChanged(object sender, EventArgs e)
        {
            if (_treeView.CheckedEntries.Count == 0)
            {
                buttonVerify.Enabled = buttonRemove.Enabled = false;
                textCheckedSize.Text = @"-";
            }
            else
            {
                buttonVerify.Enabled = buttonRemove.Enabled = true;

                // Update selected entries size
                long totalSize = _treeView.CheckedEntries.OfType<ImplementationNode>().Sum(implementationEntry => implementationEntry.Size);
                textCheckedSize.Text = totalSize.FormatBytes(CultureInfo.CurrentCulture);
            }
        }

        private void buttonRunAsAdmin_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(Path.Combine(Locations.InstallBase, "0store-win.exe")) {Verb = "runas", ErrorDialog = true});
                Close();
            }
            catch (FileNotFoundException)
            {}
            catch (Win32Exception)
            {}
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (Msg.YesNo(this, string.Format(Resources.DeleteCheckedEntries, _treeView.CheckedEntries.Count), MsgSeverity.Warn, Resources.YesDelete, Resources.NoKeep))
            {
                try
                {
                    RunTask(new ForEachTask<Node>(Resources.DeletingEntries, _treeView.CheckedEntries, entry => entry.Delete()), null);
                }
                    #region Error handling
                catch (OperationCanceledException)
                {}
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
        }

        private void buttonVerify_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (Node entry in _treeView.CheckedEntries)
                    entry.Verify(this);
            }
                #region Error handling
            catch (OperationCanceledException)
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
        /// Always returns 1.
        /// </summary>
        public int Verbosity { get { return 1; } set { } }

        private readonly CancellationToken _cancellationToken = new CancellationToken();

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return _cancellationToken; } }

        /// <inheritdoc/>
        public void RunTask(ITask task, object tag)
        {
            // Handle events coming from a non-UI thread, block caller
            Invoke(new Action(() => TrackingDialog.Run(this, task, Icon)));
        }
        #endregion
    }
}
