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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Commands.WinForms.StoreManagementNodes;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the content of caches (<see cref="IFeedCache"/> and <see cref="IStore"/>) in a combined tree view.
    /// </summary>
    public sealed partial class StoreManageForm : Form, IInteractionHandler
    {
        #region Variables
        private readonly IStore _store;
        private readonly IFeedCache _feedCache;

        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<Node> _treeView = new FilteredTreeView<Node> {Separator = '#', CheckBoxes = true, Dock = DockStyle.Fill};
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store management window.
        /// </summary>
        /// <param name="store">The <see cref="IStore"/> to manage.</param>
        /// <param name="feedCache">Information about implementations found in the <paramref name="store"/> are extracted from here.</param>
        public StoreManageForm(IStore store, IFeedCache feedCache)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            _store = store;
            _feedCache = feedCache;

            InitializeComponent();
            WindowsUtils.AddShieldIcon(buttonRunAsAdmin);

            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
            if (WindowsUtils.IsAdministrator) Text += @" (Administrator)";
            else if (WindowsUtils.IsWindowsNT) buttonRunAsAdmin.Visible = true;

            Shown += delegate { RefreshList(); };
            HandleCreated += delegate { Program.ConfigureTaskbar(this, Text); };

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
            try
            {
                _store.Flush();
                _feedCache.Flush();
                var listBuilder = new NodeListBuilder(this, _store, _feedCache);

                using (var handler = new GuiTaskHandler(this)) handler.RunTask(listBuilder);

                _treeView.Nodes = listBuilder.Nodes;
                _treeView.SelectedEntry = null;
                textTotalSize.Text = listBuilder.TotalSize.FormatBytes(CultureInfo.CurrentCulture);
                buttonVerify.Enabled = buttonRemove.Enabled = false;
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                Close();
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                Close();
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                Close();
            }
            #endregion

            OnCheckedEntriesChanged(null, EventArgs.Empty);
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
                Process.Start(new ProcessStartInfo(Path.Combine(Locations.InstallBase, "0install-win.exe"), "store manage") {Verb = "runas", ErrorDialog = true});
                Close();
            }
            catch (FileNotFoundException)
            {}
            catch (Win32Exception)
            {}
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            if (Msg.YesNo(this, string.Format(Resources.DeleteCheckedEntries, _treeView.CheckedEntries.Count), MsgSeverity.Warn))
            {
                try
                {
                    RunTask(new ForEachTask<Node>(Resources.DeletingEntries, _treeView.CheckedEntries, entry => entry.Delete()));
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

        #region IInteractionHandler
        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return default(CancellationToken); } }

        /// <summary>
        /// Always returns <see langword="false"/>.
        /// </summary>
        public bool Batch { get { return false; } set { } }

        /// <summary>
        /// Always returns 1. This ensures that information hidden by the GUI is at least retrievable from the log files.
        /// </summary>
        public int Verbosity { get { return 1; } set { } }

        public void ShowProgressUI()
        {}

        public void DisableProgressUI()
        {}

        public void CloseProgressUI()
        {}

        /// <inheritdoc/>
        public void RunTask(ITask task)
        {
            using (var handler = new GuiTaskHandler(this)) handler.RunTask(task);
        }

        public bool AskQuestion(string question, string batchInformation = null)
        {
            return Msg.YesNo(this, question, MsgSeverity.Warn);
        }

        public void Output(string title, string information)
        {
            Msg.Inform(this, information, MsgSeverity.Info);
        }
        #endregion
    }
}
