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
using System.Net;
using System.Windows.Forms;
using Common;
using Common.Collections;
using Common.Controls;
using Common.Storage;
using Common.Utils;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Commands.WinForms;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Feeds;
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
        /// <summary>Apply operations sachine-wide instead of just for the current user.</summary>
        private readonly bool _machineWide;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the main GUI.
        /// </summary>
        /// <param name="machineWide">Apply operations sachine-wide instead of just for the current user.</param>
        public MainForm(bool machineWide)
        {
            _machineWide = machineWide;

            InitializeComponent();

            HandleCreated += delegate
            {
                Program.ConfigureTaskbar(this, Text, null, null);

                var syncLink = new WindowsUtils.ShellLink(buttonSync.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, Commands.WinForms.Program.ExeName + ".exe"), Commands.SyncApps.Name);
                var cacheLink = new WindowsUtils.ShellLink(buttonCacheManagement.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, Store.Management.WinForms.Program.ExeName + ".exe"), null);
                WindowsUtils.AddTaskLinks(Program.AppUserModelID, new[] {syncLink, cacheLink});
            };

            Load += delegate
            {
                if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
                if (machineWide) Text += @" - " + Resources.MachineWideMode;
                labelVersion.Text = @"v" + Application.ProductVersion;

                try
                {
                    // Ensure all relevant directories are created
                    Policy.CreateDefault(new SilentHandler());

                    appList.IconCache = catalogList.IconCache = IconCacheProvider.CreateDefault();
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
                    Msg.Inform(this, ex.Message +
                        (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                    Close();
                }
                #endregion
            };

            Shown += delegate
            {
                if (SelfUpdateUtils.AutoActive) selfUpdateWorker.RunWorkerAsync();
                LoadAppList();
                LoadCatalogCached();
                LoadCatalogAsync();

                string introDoneFlag = Locations.GetSaveConfigPath("0install.net", true, "central", "intro_done");
                if (_currentAppList.Entries.IsEmpty)
                {
                    // Show intro video automatically on first start
                    if (!File.Exists(introDoneFlag))
                        new IntroDialog().ShowDialog(this);

                    // Show catalog automatically if AppList is empty
                    tabControlApps.SelectTab(tabPageCatalog);
                }
                File.WriteAllText(introDoneFlag, "");
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
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            catch (SolverException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
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
                    ProcessUtils.LaunchAssembly("0install-win", "self-update");
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
                newAppList = XmlStorage.LoadXml<AppList>(AppList.GetDefaultPath(_machineWide));
            }
                #region Error handling
            catch (FileNotFoundException)
            {
                newAppList = new AppList();
            }
            catch (IOException ex)
            {
                Log.Warn(Resources.UnableToLoadAppList);
                Log.Warn(ex);
                newAppList = new AppList();
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(Resources.UnableToLoadAppList);
                Log.Warn(ex);
                newAppList = new AppList();
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(Resources.UnableToLoadAppList);
                Log.Warn(ex);
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
                        var tile = appList.QueueNewTile(_machineWide, addedEntry.InterfaceID, addedEntry.Name, status);
                        feedsToLoad.Add(tile, addedEntry.InterfaceID);

                        // Update "added" status of tile in catalog list
                        var catalogTile = catalogList.GetTile(addedEntry.InterfaceID);
                        if (catalogTile != null) catalogTile.Status = tile.Status;
                    }
                        #region Error handling
                    catch (KeyNotFoundException)
                    {
                        Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, addedEntry.InterfaceID));
                    }
                    catch (C5.DuplicateNotAllowedException)
                    {
                        Log.Warn(string.Format(Resources.IgnoringDuplicateAppListEntry, addedEntry.InterfaceID));
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
            var policy = Policy.CreateDefault(new MinimalHandler(this));
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
                    BeginInvoke(new Action(() => tile.Feed = feed));
                }
                    #region Error handling
                catch (OperationCanceledException)
                {}
                catch (InvalidInterfaceIDException ex)
                {
                    Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, pair.Value));
                    Log.Warn(ex);
                }
                catch (IOException ex)
                {
                    Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, pair.Value));
                    Log.Warn(ex);
                }
                catch (WebException ex)
                {
                    Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, pair.Value));
                    Log.Warn(ex);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, pair.Value));
                    Log.Warn(ex);
                }
                catch (SignatureException ex)
                {
                    Log.Warn(string.Format(Resources.UnableToLoadFeedForApp, pair.Value));
                    Log.Warn(ex);
                }
                #endregion
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Detect changes made to the AppList by other threads or processes
            if (m.Msg == IntegrationManager.ChangedWindowMessageID) LoadAppList();
            else base.WndProc(ref m);
        }
        #endregion

        #region Catalog
        /// <summary>Stores the data currently displayed in <see cref="catalogList"/>. Used for comparison/merging when updating the list.</summary>
        private Catalog _currentCatalog;

        /// <summary>
        /// Loads a cached version of the "new applications" catalog from the disk.
        /// </summary>
        private void LoadCatalogCached()
        {
            _currentCatalog = CatalogManager.GetCached();
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

            labelLastCatalogError.Visible = false;
            catalogWorker.RunWorkerAsync();
        }

        private void catalogWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = CatalogManager.GetOnline(Policy.CreateDefault(new MinimalHandler(this)));
            }
                #region Error handling
            catch (WebException ex)
            {
                Log.Warn(Resources.UnableToDownloadCatalog);
                Log.Warn(ex);
            }
            #endregion
        }

        private void catalogWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
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
            }
            else
            {
                Log.Error(e.Error);
                labelLastCatalogError.Text = e.Error.Message;
                labelLastCatalogError.Visible = true;
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
                var status = _currentAppList.Contains(interfaceID)
                    ? ((_currentAppList[interfaceID].AccessPoints == null) ? AppStatus.Added : AppStatus.Integrated)
                    : AppStatus.Candidate;
                var tile = catalogList.QueueNewTile(_machineWide, interfaceID, feed.Name, status);
                tile.Feed = feed;
            }
                #region Error handling
            catch (C5.DuplicateNotAllowedException)
            {
                Log.Warn(string.Format(Resources.IgnoringDuplicateAppListEntry, feed.Uri));
            }
            #endregion
        }
        #endregion

        //--------------------//

        #region Buttons
        private void buttonSync_Click(object sender, EventArgs e)
        {
            Config config;
            try
            {
                config = Config.Load();
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return;
            }
            #endregion

            if (string.IsNullOrEmpty(config.SyncServerUsername) || string.IsNullOrEmpty(config.SyncServerPassword) || string.IsNullOrEmpty(config.SyncCryptoKey))
            {
                using (var wizard = new SyncConfig.SetupWizard(_machineWide))
                    wizard.ShowDialog(this);
            }
            else
            {
                ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(_machineWide
                    ? new[] {Commands.SyncApps.Name, "--machine"}
                    : new[] {Commands.SyncApps.Name}));
            }
        }

        private void butonSyncSetup_Click(object sender, EventArgs e)
        {
            Config config;
            try
            {
                config = Config.Load();
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return;
            }
            #endregion

            if (!string.IsNullOrEmpty(config.SyncServerUsername) || !string.IsNullOrEmpty(config.SyncServerPassword) || !string.IsNullOrEmpty(config.SyncCryptoKey))
                if (!Msg.YesNo(this, Resources.SyncWillReplaceConfig, MsgSeverity.Warn, Resources.Continue, Resources.Cancel)) return;

            using (var wizard = new SyncConfig.SetupWizard(_machineWide))
                wizard.ShowDialog(this);
        }

        private void buttonSyncReset_Click(object sender, EventArgs e)
        {
            Config config;
            try
            {
                config = Config.Load();
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(this, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return;
            }
            #endregion

            if (string.IsNullOrEmpty(config.SyncServerUsername) || string.IsNullOrEmpty(config.SyncServerPassword) || string.IsNullOrEmpty(config.SyncCryptoKey))
                Msg.Inform(this, Resources.SyncCompleteSetupFirst, MsgSeverity.Warn);
            else
            {
                using (var wizard = new SyncConfig.ResetWizard(_machineWide))
                    wizard.ShowDialog(this);
            }
        }

        private void buttonUpdateAll_Click(object sender, EventArgs e)
        {
            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(_machineWide
                ? new[] {Commands.UpdateApps.Name, "--machine"}
                : new[] {Commands.UpdateApps.Name}));
        }

        private void buttonUpdateAllClean_Click(object sender, EventArgs e)
        {
            if (!Msg.YesNo(this, Resources.UpdateAllCleanWillRemove, MsgSeverity.Warn, Resources.Continue, Resources.Cancel)) return;

            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(_machineWide
                ? new[] {Commands.UpdateApps.Name, "--clean", "--machine"}
                : new[] {Commands.UpdateApps.Name, "--clean"}));
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
            LoadCatalogAsync();
        }

        private void buttonOptionsAdvanced_Click(object sender, EventArgs e)
        {
            if (!Msg.YesNo(this, Resources.OptionsAdvancedWarn, MsgSeverity.Warn, Resources.Continue, Resources.Cancel)) return;

            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(new[] {Commands.Configure.Name}));
        }

        private void buttonCacheManagement_Click(object sender, EventArgs e)
        {
            ProcessUtils.RunAsync(Store.Management.WinForms.Program.Main);
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            OpenInBrowser("http://0install.de/help/");
        }

        private void buttonIntro_Click(object sender, EventArgs e)
        {
            new IntroDialog().ShowDialog(this);
        }
        #endregion

        #region Focus handling
        private void tabControlApps_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Redirect text input to appropriate search box
            if (tabControlApps.SelectedTab == tabPageAppList)
            {
                if (!appList.TextSearch.Focused)
                {
                    appList.TextSearch.Focus();
                    SendKeys.SendWait(e.KeyChar.ToString(CultureInfo.InvariantCulture));
                }
            }
            else if (tabControlApps.SelectedTab == tabPageCatalog)
            {
                if (!catalogList.TextSearch.Focused)
                {
                    catalogList.TextSearch.Focus();
                    SendKeys.SendWait(e.KeyChar.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
        #endregion

        #region Drag and drop handling
        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string path in (string[])e.Data.GetData(DataFormats.FileDrop))
                    AddCustomInterface(path);
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

        /// <summary>
        /// Adds a custom interface to <see cref="catalogList"/>.
        /// </summary>
        /// <param name="interfaceID">The URI of the interface to be added.</param>
        private void AddCustomInterface(string interfaceID)
        {
            ProcessUtils.RunAsync(() => Commands.WinForms.Program.Main(_machineWide
                ? new[] {Commands.AddApp.Name, "--machine", interfaceID}
                : new[] {Commands.AddApp.Name, interfaceID}));
            tabControlApps.SelectTab(tabPageAppList);
        }
        #endregion
    }
}
