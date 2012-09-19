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
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// The main GUI for Zero Install.
    /// </summary>
    internal partial class MainForm : Form
    {
        #region Variables
        /// <summary>Apply operations system-wide instead of just for the current user.</summary>
        private readonly bool _systemWide;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the main GUI.
        /// </summary>
        /// <param name="systemWide">Apply operations system-wide instead of just for the current user.</param>
        public MainForm(bool systemWide)
        {
            _systemWide = systemWide;

            InitializeComponent();

            HandleCreated += delegate
            {
                Program.ConfigureTaskbar(this, Text, null, null);

                var syncLink = new WindowsUtils.ShellLink(buttonSync.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, Commands.WinForms.Program.ExeName + ".exe"), "sync");
                var cacheLink = new WindowsUtils.ShellLink(buttonCacheManagement.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, Store.Management.WinForms.Program.ExeName + ".exe"), null);
                WindowsUtils.AddTaskLinks(Program.AppUserModelID, new[] {syncLink, cacheLink});
            };

            Load += delegate
            {
                if (systemWide) Text += @" - System-wide mode";
                if (Locations.IsPortable) Text += @" - Portable mode";
                labelVersion.Text = @"v" + Application.ProductVersion;

                appList.IconCache = catalogList.IconCache = IconCacheProvider.CreateDefault();
            };

            Shown += delegate
            {
                if (SelfUpdateUtils.AutoActive) selfUpdateWorker.RunWorkerAsync();
                LoadAppList();
                LoadCatalogCached();
                LoadCatalogAsync();

                // Show "new apps" list if "my apps" list is empty
                if (_currentAppList.Entries.IsEmpty) tabControlApps.SelectedTab = tabPageCatalog;
            };

            FormClosing += delegate
            {
                Visible = false;

                // Wait for background tasks to shutdown
                appListWorker.CancelAsync();
                while (selfUpdateWorker.IsBusy || appListWorker.IsBusy || catalogWorker.IsBusy)
                    Application.DoEvents();
            };

            // Redirect mouse wheel input to AppTileLists
            MouseWheel += delegate(object sender, MouseEventArgs e)
            {
                if (tabControlApps.SelectedTab == tabPageAppList) appList.PerformScroll(e.Delta);
                else if (tabControlApps.SelectedTab == tabPageCatalog) catalogList.PerformScroll(e.Delta);
            };
        }
        #endregion

        //--------------------//

        #region Self-update
        private void selfUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = SelfUpdateUtils.Check();
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Log.Warn("Unable to perform self-update check:\n" + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Unable to perform self-update check:\n" + ex.Message);
            }
            catch (InvalidDataException ex)
            {
                Log.Warn("Unable to perform self-update check:\n" + ex.Message);
            }
            catch (SolverException ex)
            {
                Log.Warn("Unable to perform self-update check:\n" + ex.Message);
            }
            #endregion
        }

        private void selfUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var selfUpdateVersion = e.Result as ImplementationVersion;
            if (selfUpdateVersion == null || !Visible) return;
            if (Msg.YesNo(this, string.Format(Resources.SelfUpdateAvailable, selfUpdateVersion), MsgSeverity.Info, Resources.SelfUpdateYes, Resources.SelfUpdateNo))
            {
                try
                {
                    ProcessUtils.LaunchHelperAssembly("0install-win", "self-update");
                    Application.Exit();
                }
                    #region Error handling
                catch (FileNotFoundException ex)
                {
                    Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, "0install-win"), MsgSeverity.Error);
                }
                catch (Win32Exception ex)
                {
                    Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, "0install-win"), MsgSeverity.Error);
                }
                #endregion
            }
        }
        #endregion

        #region AppList
        /// <summary>Stores the data currently displayed in <see cref="appList"/>. Used for comparison/merging when updating the list.</summary>
        private AppList _currentAppList = new AppList();

        /// <summary>
        /// Loads the "my applications" list and displays it, loading additional data from feeds in the background.
        /// </summary>
        private void LoadAppList()
        {
            // Prevent multiple concurrent refreshes
            if (appListWorker.IsBusy) return;

            AppList newAppList;
            try
            {
                newAppList = AppList.Load(AppList.GetDefaultPath(_systemWide));
            }
                #region Error handling
            catch (FileNotFoundException)
            {
                newAppList = new AppList();
            }
            catch (IOException ex)
            {
                Log.Warn("Unable to load application list XML:\n" + ex.Message);
                newAppList = new AppList();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Unable to load application list XML:\n" + ex.Message);
                newAppList = new AppList();
            }
            catch (InvalidDataException ex)
            {
                Log.Warn("Unable to load application list XML:\n" + ex.Message);
                newAppList = new AppList();
            }
            #endregion

            var feedsToLoad = new Dictionary<AppTile, string>();

            // Update the displayed AppList based on changes detected between the current and the new AppList
            EnumerableUtils.Merge(
                newAppList.Entries, _currentAppList.Entries,
                addedEntry =>
                {
                    if (string.IsNullOrEmpty(addedEntry.InterfaceID) || addedEntry.Name == null) return;
                    try
                    {
                        var status = (addedEntry.AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated;
                        var tile = appList.QueueNewTile(_systemWide, addedEntry.InterfaceID, addedEntry.Name, status);
                        feedsToLoad.Add(tile, addedEntry.InterfaceID);

                        // Update "added" status of tile in catalog list
                        var catalogTile = catalogList.GetTile(addedEntry.InterfaceID);
                        if (catalogTile != null) catalogTile.Status = tile.Status;
                    }
                        #region Error handling
                    catch (KeyNotFoundException)
                    {
                        Log.Warn("Unable to load feed for: " + addedEntry.InterfaceID);
                    }
                    catch (C5.DuplicateNotAllowedException)
                    {
                        Log.Warn("Ignoring duplicate application list entry for: " + addedEntry.InterfaceID);
                    }
                    #endregion
                },
                removedEntry =>
                {
                    appList.RemoveTile(removedEntry.InterfaceID);

                    // Update "added" status of tile in catalog list
                    var catalogTile = catalogList.GetTile(removedEntry.InterfaceID);
                    if (catalogTile != null) catalogTile.Status = AppStatus.Candidate;
                });
            appList.AddQueuedTiles();
            _currentAppList = newAppList;

            // Load additional data from feeds in background
            appListWorker.RunWorkerAsync(feedsToLoad);
        }

        private void appListWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var policy = Policy.CreateDefault(new SilentHandler());
            policy.Config.NetworkUse = NetworkLevel.Minimal; // Don't update already cached feeds, even if they are stale

            var feedsToLoad = (IDictionary<AppTile, string>)e.Argument;
            foreach (var pair in feedsToLoad)
            {
                if (appListWorker.CancellationPending) return;

                try
                {
                    // Load and parse the feed
                    var feed = policy.FeedManager.GetFeed(pair.Value, policy);

                    // Send it to the UI thread
                    var tile = pair.Key;
                    BeginInvoke((SimpleEventHandler)(() => tile.Feed = feed));
                }
                    #region Error handling
                catch (OperationCanceledException)
                {}
                catch (InvalidInterfaceIDException ex)
                {
                    Log.Warn("Unable to load feed for application list entry '" + pair.Value + "':\n" + ex.Message);
                }
                catch (IOException ex)
                {
                    Log.Warn("Unable to load feed for application list entry '" + pair.Value + "':\n" + ex.Message);
                }
                catch (WebException ex)
                {
                    Log.Warn("Unable to load feed for application list entry '" + pair.Value + "':\n" + ex.Message);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warn("Unable to load feed for application list entry '" + pair.Value + "':\n" + ex.Message);
                }
                catch (SignatureException ex)
                {
                    Log.Warn("Unable to load feed for application list entry '" + pair.Value + "':\n" + ex.Message);
                }
                #endregion
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Detect changes made to the AppList by other threads or processes
            if (m.Msg == IntegrationManager.ChangedWindowMessageID) LoadAppList();

            base.WndProc(ref m);
        }
        #endregion

        #region Catalog
        /// <summary>Stores the data currently displayed in <see cref="catalogList"/>. Used for comparison/merging when updating the list.</summary>
        private Catalog _currentCatalog = new Catalog();

        /// <summary>
        /// Loads a cached version of the "new applications" catalog from the disk.
        /// </summary>
        private void LoadCatalogCached()
        {
            try
            {
                _currentCatalog = Catalog.Load(Path.Combine(Locations.GetCacheDirPath("0install.net"), "catalog.xml"));
            }
                #region Error handling
            catch (FileNotFoundException)
            {
                return;
            }
            catch (IOException ex)
            {
                Log.Warn("Unable to load cached application catalog from disk:\n" + ex.Message);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Unable to load cached application catalog from disk:\n" + ex.Message);
            }
            catch (InvalidDataException ex)
            {
                Log.Warn("Unable to parse cached application catalog:\n" + ex.Message);
            }
            #endregion

            _currentCatalog.Feeds.Apply(QueueCatalogTile);
            catalogList.AddQueuedTiles();
            catalogList.ShowCategories();
        }

        /// <summary>
        /// Loads the "new applications" catalog in the background and displays it.
        /// </summary>
        private void LoadCatalogAsync()
        {
            // Prevent multiple concurrent refreshes
            if (catalogWorker.IsBusy) return;
            buttonRefreshCatalog.Visible = false;
            labelLoadingCatalog.Visible = true;

            catalogWorker.RunWorkerAsync();
        }

        private void catalogWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                // ToDo: Merge multiple catalogs from custom sources
                var catalog = Catalog.Load(new MemoryStream(new WebClientTimeout().DownloadData("http://0install.de/catalog/")));
                catalog.Save(Path.Combine(Locations.GetCacheDirPath("0install.net"), "catalog.xml"));

                e.Result = catalog;
            }
                #region Error handling
            catch (WebException ex)
            {
                Log.Warn("Unable to download application catalog:\n" + ex.Message);
            }
            catch (IOException ex)
            {
                Log.Warn("Unable to cache downloaded application catalog:\n" + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn("Unable to cache downloaded application catalog:\n" + ex.Message);
            }
            catch (InvalidDataException ex)
            {
                Log.Warn("Unable to parse downloaded application catalog:\n" + ex.Message);
            }
            #endregion
        }

        private void catalogWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var newCatalog = e.Result as Catalog;
            if (newCatalog != null)
            {
                // Update the displayed catalog list based on changes detected between the current and the new catalog
                EnumerableUtils.Merge(
                    newCatalog.Feeds, _currentCatalog.Feeds,
                    QueueCatalogTile, removedFeed => catalogList.RemoveTile(removedFeed.UriString));
                catalogList.AddQueuedTiles();
                catalogList.ShowCategories();
                _currentCatalog = newCatalog;
            }

            buttonRefreshCatalog.Visible = true;
            labelLoadingCatalog.Visible = false;
        }

        /// <summary>
        /// Queues a new tile for the <paramref name="feed"/> on the <see cref="catalogList"/>.
        /// </summary>
        private void QueueCatalogTile(Feed feed)
        {
            if (string.IsNullOrEmpty(feed.UriString) || feed.Name == null) return;
            try
            {
                string interfaceID = feed.UriString;
                var status = _currentAppList.ContainsEntry(interfaceID)
                    ? ((_currentAppList.GetEntry(interfaceID).AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated)
                    : AppStatus.Candidate;
                var tile = catalogList.QueueNewTile(_systemWide, interfaceID, feed.Name, status);
                tile.Feed = feed;
            }
                #region Error handling
            catch (C5.DuplicateNotAllowedException)
            {
                Log.Warn("Ignoring duplicate application list entry for: " + feed.Uri);
            }
            #endregion
        }
        #endregion

        //--------------------//

        #region Buttons
        private void buttonSync_Click(object sender, EventArgs e)
        {
            var config = Config.Load();
            if (string.IsNullOrEmpty(config.SyncServerUsername) || string.IsNullOrEmpty(config.SyncServerPassword) || string.IsNullOrEmpty(config.SyncCryptoKey))
            {
                using (var wizard = new SyncConfig.SetupWizard())
                    wizard.ShowDialog(this);
            }
            else ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(new[] {"sync"}));
        }

        private void buttonRefreshCatalog_Click(object sender, EventArgs e)
        {
            LoadCatalogAsync();
        }

        private void buttonAddOtherApp_Click(object sender, EventArgs e)
        {
            string interfaceID = InputBox.Show(this, "Zero Install", Resources.EnterInterfaceUrl);
            if (string.IsNullOrEmpty(interfaceID)) return;

            AddCustomInterface(interfaceID);
        }

        private void buttonOptions_Click(object sender, EventArgs e)
        {
            using (var dialog = new OptionsDialog())
                dialog.ShowDialog(this);
        }

        private void buttonCacheManagement_Click(object sender, EventArgs e)
        {
            ProcessUtils.RunAsync(Store.Management.WinForms.Program.Main);
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            OpenInBrowser("http://0install.de/help/");
        }
        #endregion

        #region Drag and drop handling
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                AddCustomInterface(EnumerableUtils.First(files));
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
                AddCustomInterface((string)e.Data.GetData(DataFormats.Text));
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.FileDrop))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Adds a custom interface to <see cref="catalogList"/>.
        /// </summary>
        /// <param name="interfaceID">The URI of the interface to be added.</param>
        private void AddCustomInterface(string interfaceID)
        {
            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(new[] {"add-app", interfaceID}));
            tabControlApps.SelectTab(tabPageAppList);
        }

        /// <summary>
        /// Opens a URL in the system's default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenInBrowser(string url)
        {
            try
            {
                Process.Start(url);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion
    }
}
