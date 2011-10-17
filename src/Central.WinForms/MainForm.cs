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
        #region Constructor
        /// <summary>
        /// Initializes the main GUI.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            HandleCreated += delegate
            {
                Program.ConfigureTaskbar(this, Text, null, null);

                var cacheLink = new ShellLink(buttonCacheManagement.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, StoreExe + ".exe"), null);
                var configLink = new ShellLink(buttonConfiguration.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, CommandsExe + ".exe"), "config");
                try
                {
                    WindowsUtils.AddTaskLinks(Program.AppUserModelID, new[] {cacheLink, configLink});
                }
                    #region Sanity checks
                catch (IOException ex)
                {
                    Log.Error("Failed to set up task links:\n" + ex.Message);
                }
                #endregion
            };

            Load += delegate
            {
                if (Locations.IsPortable) Text += @" - Portable mode";
                labelVersion.Text = @"v" + Application.ProductVersion;

                appList.IconCache = catalogList.IconCache = IconCacheProvider.CreateDefault();
            };

            // Set up AppList file monitoring
            string appListPath = AppList.GetDefaultPath(false);
            appListWatcher.Path = Path.GetDirectoryName(appListPath);
            appListWatcher.Filter = Path.GetFileName(appListPath);
            
            Shown += delegate
            {
                SelfUpdateCheckAsync();
                LoadAppListAsnyc();
                LoadCatalogAsync();

                appListWatcher.EnableRaisingEvents = true;
            };
        }
        #endregion

        //--------------------//

        #region Self-update
        /// <summary>
        /// Runs a check for updates for Zero Install itself in the background.
        /// </summary>
        private void SelfUpdateCheckAsync()
        {
            // ToDo: Add option to turn self-update off

            // Don't check for updates when launched as a Zero Install implementation
            string topDir = Path.GetFileName(Locations.InstallBase) ?? Locations.InstallBase;
            if ((topDir.StartsWith("sha") && topDir.Contains("="))) return;

            FormClosing += delegate
            {
                Visible = false;
                while (selfUpdateWorker.IsBusy)
                    Application.DoEvents();
            };
            selfUpdateWorker.RunWorkerAsync();
        }

        private void selfUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = UpdateUtils.CheckSelfUpdate(Policy.CreateDefault(new SilentHandler()));
            }
                #region Error handling
            catch (UserCancelException)
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
        private void LoadAppListAsnyc()
        {
            // ToDo: Make async

            var feedCache = FeedCacheProvider.CreateDefault();
            var newAppList = AppList.Load(AppList.GetDefaultPath(false));

            // Update the displayed AppList based on changes detected between the current and the new AppList
            EnumerableUtils.Merge(
                newAppList.Entries, _currentAppList.Entries,
                addedEntry =>
                {
                    if (string.IsNullOrEmpty(addedEntry.InterfaceID) || addedEntry.Name == null) return;
                    try
                    {
                        var tile = appList.AddTile(addedEntry.InterfaceID, addedEntry.Name);
                        tile.InAppList = true;
                        tile.SetFeed(feedCache.GetFeed(addedEntry.InterfaceID));

                        // Update "added" status of tile in catalog list
                        var catalogTile = catalogList.GetTile(addedEntry.InterfaceID);
                        if (catalogTile != null) catalogTile.InAppList = true;
                    }
                        #region Error handling
                    catch (KeyNotFoundException)
                    {
                        Log.Warn("Unable to load feed for: " + addedEntry.InterfaceID);
                    }
                    catch (C5.DuplicateNotAllowedException)
                    {
                        Log.Warn("Ignoring duplicate AppList entry for: " + addedEntry.InterfaceID);
                    }
                    #endregion
                },
                removedEntry =>
                {
                    appList.RemoveTile(removedEntry.InterfaceID);

                    // Update "added" status of tile in catalog list
                    var catalogTile = catalogList.GetTile(removedEntry.InterfaceID);
                    if (catalogTile != null) catalogTile.InAppList = false;
                });
            appList.ColorTiles();
            _currentAppList = newAppList;
        }

        private void appListWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            LoadAppListAsnyc();
        }
        #endregion

        #region Catalog
        /// <summary>Stores the data currently displayed in <see cref="catalogList"/>. Used for comparison/merging when updating the list.</summary>
        private ICollection<Feed> _currentCatalogFeeds = new List<Feed>();

        /// <summary>
        /// Loads the "new applications" catalog in the background and displays it.
        /// </summary>
        private void LoadCatalogAsync()
        {
            catalogWorker.RunWorkerAsync();
        }

        private void catalogWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var mergedCatalog = new Catalog();

            try
            {
                // ToDo: Merge data from multiple catalogs and cache data
                mergedCatalog = Catalog.Load(new MemoryStream(new WebClient().DownloadData("http://0install.de/catalog/")));
            }
                #region Error handling
            catch (WebException ex)
            {
                Log.Warn("Unable to load application catalog:\n" + ex.Message);
            }
            #endregion

            // Copy the catalog feeds to a hashed list for faster comparisons
            var catalogFeeds = new C5.HashedLinkedList<Feed>();
            catalogFeeds.AddAll(mergedCatalog.Feeds);
            e.Result = catalogFeeds;
        }

        private void catalogWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var newCatalogFeeds = (ICollection<Feed>)e.Result;

            // Update the displayed catalog list based on changes detected between the current and the new catalog
            EnumerableUtils.Merge(
                newCatalogFeeds, _currentCatalogFeeds,
                addedFeed =>
                {
                    if (string.IsNullOrEmpty(addedFeed.UriString) || addedFeed.Name == null) return;
                    try
                    {
                        var tile = catalogList.AddTile(addedFeed.UriString, addedFeed.Name);
                        tile.SetFeed(addedFeed);

                        // Update "added" status of tile
                        tile.InAppList = _currentAppList.ContainsEntry(addedFeed.UriString);
                    }
                        #region Error handling
                    catch (C5.DuplicateNotAllowedException)
                    {
                        Log.Warn("Ignoring duplicate AppList entry for: " + addedFeed.Uri);
                    }
                    #endregion
                },
                removedFeed => catalogList.RemoveTile(removedFeed.UriString));
            catalogList.ColorTiles();
            _currentCatalogFeeds = newCatalogFeeds;
        }
        #endregion

        //--------------------//

        #region Buttons
        /// <summary>
        /// The EXE name (without the file ending) for the Windows Commands binary.
        /// </summary>
        private const string CommandsExe = "0install-win";

        /// <summary>
        /// The EXE name (without the file ending) for the Windows Store Management binary.
        /// </summary>
        private const string StoreExe = "0store-win";

        private void buttonRefreshAppList_Click(object sender, EventArgs e)
        {
            LoadAppListAsnyc();
        }

        private void buttonRefreshCatalog_Click(object sender, EventArgs e)
        {
            LoadCatalogAsync();
        }

        private void buttonOtherApp_Click(object sender, EventArgs e)
        {
            // ToDo: Show selection dialog

            string interfaceID = InputBox.Show(null, "Zero Install", Resources.EnterInterfaceUrl);
            if (string.IsNullOrEmpty(interfaceID)) return;

            LaunchFeed(interfaceID);
        }

        private void buttonCacheManagement_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(StoreExe, null);
        }

        private void buttonConfiguration_Click(object sender, EventArgs e)
        {
            LaunchHelperAssembly(CommandsExe, "config");
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
                LaunchFeed(EnumerableUtils.GetFirst(files));
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
                LaunchFeed((string)e.Data.GetData(DataFormats.Text));
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
        /// Attempts to launch a .NET helper assembly in the application's base directory. Displays friendly error messages if something goes wrong.
        /// </summary>
        /// <param name="assembly">The name of the assembly to launch (without the file extension).</param>
        /// <param name="arguments">The command-line arguments to pass to the assembly.</param>
        private void LaunchHelperAssembly(string assembly, string arguments)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(assembly)) throw new ArgumentNullException("assembly");
            #endregion

            try
            {
                ProcessUtils.LaunchHelperAssembly(assembly, arguments);
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, assembly), MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(this, string.Format(Resources.FailedToRun + "\n" + ex.Message, assembly), MsgSeverity.Error);
            }
            #endregion
        }

        /// <summary>
        /// Opens a URL in the system's default browser.
        /// </summary>
        /// <param name="url">The URL to open.</param>
        private void OpenInBrowser(string url)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
            #endregion

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
        /// Attempts to launch a feed and closes the main window.
        /// </summary>
        /// <param name="feedUri">The URI of the feed to be launched.</param>
        private void LaunchFeed(string feedUri)
        {
            LaunchHelperAssembly(CommandsExe, "run --gui --no-wait " + StringUtils.EscapeArgument(feedUri));
        }
        #endregion
    }
}
