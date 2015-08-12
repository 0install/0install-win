/*
 * Copyright 2010-2016 Bastian Eicher
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the content of caches (<see cref="IFeedCache"/> and <see cref="IStore"/>) in a combined tree view.
    /// </summary>
    public sealed partial class StoreManageForm : Form
    {
        #region Variables
        private readonly IStore _store;
        private readonly IFeedCache _feedCache;

        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<StoreManageNode> _treeView = new FilteredTreeView<StoreManageNode> {Separator = '\\', CheckBoxes = true, Dock = DockStyle.Fill};
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store management window.
        /// </summary>
        /// <param name="store">The <see cref="IStore"/> to manage.</param>
        /// <param name="feedCache">Information about implementations found in the <paramref name="store"/> are extracted from here.</param>
        public StoreManageForm([NotNull] IStore store, [NotNull] IFeedCache feedCache)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (feedCache == null) throw new ArgumentNullException(nameof(feedCache));
            #endregion

            _store = store;
            _feedCache = feedCache;

            InitializeComponent();
            buttonRunAsAdmin.AddShieldIcon();

            HandleCreated += delegate
            {
                Program.ConfigureTaskbar(this, Text, subCommand: ".Store.Manage", arguments: StoreMan.Name + " manage");
                if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
                if (WindowsUtils.IsAdministrator) Text += @" (Administrator)";
                else if (WindowsUtils.HasUac) buttonRunAsAdmin.Visible = true;
            };

            Shown += delegate { RefreshList(); };

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
            buttonRefresh.Enabled = false;
            labelLoading.Visible = true;
            refreshListWorker.RunWorkerAsync();
        }

        private void refreshListWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _store.Flush();
            _feedCache.Flush();

            var nodeBuilder = new CacheNodeBuilder(_store, _feedCache);
            nodeBuilder.Run();
            e.Result = nodeBuilder;
        }

        private void refreshListWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            #region Error handling
            var ex = e.Error;
            if (ex is IOException || ex is UnauthorizedAccessException || ex is InvalidDataException)
            {
                Msg.Inform(this, ex.Message + (ex.InnerException == null ? "" : Environment.NewLine + ex.InnerException.Message), MsgSeverity.Error);
                Close();
            }
            else if (ex != null) ex.Rethrow();
            #endregion

            var nodeListBuilder = (CacheNodeBuilder)e.Result;
            var nodes = nodeListBuilder.Nodes.Select(x => new StoreManageNode(x, this));

            _treeView.Nodes = new NamedCollection<StoreManageNode>(nodes);
            textTotalSize.Text = nodeListBuilder.TotalSize.FormatBytes(CultureInfo.CurrentCulture);

            OnCheckedEntriesChanged(null, EventArgs.Empty);
            labelLoading.Visible = false;
            buttonRefresh.Enabled = true;
        }
        #endregion

        #region Event handlers
        private void OnSelectedEntryChanged(object sender, EventArgs e)
        {
            var node = (_treeView.SelectedEntry == null) ? null : _treeView.SelectedEntry.BackingNode;
            propertyGrid.SelectedObject = node;

            // Update current entry size
            var implementationEntry = node as ImplementationNode;
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
                var nodes = _treeView.CheckedEntries.Select(x => x.BackingNode);
                long totalSize = nodes.OfType<ImplementationNode>().Sum(x => x.Size);
                textCheckedSize.Text = totalSize.FormatBytes(CultureInfo.CurrentCulture);
            }
        }

        private void buttonRunAsAdmin_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessUtils.Assembly(Program.ExeName, StoreMan.Name, "manage").AsAdmin().Start();
            }
            catch (PlatformNotSupportedException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            Close();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (Msg.YesNo(this, string.Format(Resources.DeleteCheckedEntries, _treeView.CheckedEntries.Count), MsgSeverity.Warn))
            {
                try
                {
                    using (var handler = new DialogTaskHandler(this))
                    {
                        foreach (var node in _treeView.CheckedEntries.Select(x => x.BackingNode).ToList())
                            node.Delete(handler);
                    }
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
                    Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                }
                    #endregion

                finally
                {
                    RefreshList();
                }
            }
        }

        private void buttonVerify_Click(object sender, EventArgs e)
        {
            try
            {
                using (var handler = new DialogTaskHandler(this))
                {
                    foreach (var entry in _treeView.CheckedEntries.Select(x => x.BackingNode).OfType<ImplementationNode>())
                        entry.Verify(handler);
                }
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
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
