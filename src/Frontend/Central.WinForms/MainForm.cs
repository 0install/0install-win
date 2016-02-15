/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Info;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using ZeroInstall.Central.Properties;
using ZeroInstall.Commands;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.WinForms;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Services;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store;
using ZeroInstall.Store.Icons;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// The main GUI for Zero Install.
    /// </summary>
    internal partial class MainForm : Form
    {
        #region Variables
        /// <summary>Apply operations machine-wide instead of just for the current user.</summary>
        private readonly bool _machineWide;

        /// <summary>Manages <see cref="IAppTileList"/>s.</summary>
        private readonly AppTileManagement _tileManagement;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the main GUI.
        /// </summary>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Failed to read a config file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        /// <exception cref="InvalidDataException">The config data is damaged.</exception>
        public MainForm(bool machineWide)
        {
            InitializeComponent();
            HandleCreated += MainForm_HandleCreated;
            MouseWheel += MainForm_MouseWheel;

            _machineWide = machineWide;

            var services = new ServiceLocator(new MinimalTaskHandler(this)) {Config = {NetworkUse = NetworkLevel.Minimal}};
            _tileManagement = new AppTileManagement(
                services.FeedManager, services.CatalogManager, IconCacheProvider.GetInstance(),
                tileListMyApps, tileListCatalog, _machineWide);
        }
        #endregion

        //--------------------//

        #region Form
        private void MainForm_HandleCreated(object sender, EventArgs e)
        {
            Program.ConfigureTaskbar(this, Text);
            WindowsTaskbar.AddTaskLinks(Program.AppUserModelID, new[]
            {
                new WindowsTaskbar.ShellLink(buttonSync.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, Commands.WinForms.Program.ExeName + ".exe"), SyncApps.Name),
                new WindowsTaskbar.ShellLink(buttonUpdateAll.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, Commands.WinForms.Program.ExeName + ".exe"), UpdateApps.Name)
            });
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string brandingPath = Path.Combine(Locations.InstallBase, "_branding");
            if (File.Exists(brandingPath + ".txt")) Text = File.ReadAllText(brandingPath + ".txt");

            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
            if (_machineWide) Text += @" - " + Resources.MachineWideMode;
            labelVersion.Text = @"v" + AppInfo.Current.Version;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            WindowsUtils.RegisterApplicationRestart(_machineWide ? "--restart --machine" : "--restart");

            UpdateAppListAsync();
            _tileManagement.LoadCachedCatalog();
            LoadCatalogAsync();

            bool firstRun = OnFirstRun();
            if (_tileManagement.AppList.Entries.Count == 0)
            {
                if (firstRun) using (var dialog = new IntroDialog()) dialog.ShowDialog(this);

                // Show catalog automatically if AppList is empty
                tabControlApps.SelectTab(tabPageCatalog);
            }

            if (!SelfUpdateUtils.NoAutoCheck && !ProgramUtils.IsRunningFromCache) selfUpdateWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Returns <c>true</c> the first time it is called and then always <c>false</c>.
        /// </summary>
        private static bool OnFirstRun()
        {
            bool firstRun = false;
            try
            {
                string firstRunFlag = Locations.GetSaveConfigPath("0install.net", true, "central", "intro_done");
                if (!File.Exists(firstRunFlag)) firstRun = true;
                FileUtils.Touch(firstRunFlag);
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
            }
            #endregion

            return firstRun;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowsUtils.UnregisterApplicationRestart();

            Visible = false;

            // Wait for background tasks to shut down
            appListWorker.CancelAsync();
            while (selfUpdateWorker.IsBusy || appListWorker.IsBusy || catalogWorker.IsBusy)
                Application.DoEvents();
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (tabControlApps.SelectedTab == tabPageAppList) tileListMyApps.PerformScroll(e.Delta);
            else if (tabControlApps.SelectedTab == tabPageCatalog) tileListCatalog.PerformScroll(e.Delta);
        }
        #endregion

        #region Drag and drop handling
        /// <summary>
        /// Deactivates drag and drop support. Used to prevent accidental copying within the same window.
        /// </summary>
        internal static bool DisableDragAndDrop { get; set; }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (DisableDragAndDrop) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string path in (string[])e.Data.GetData(DataFormats.FileDrop))
                    AddCustomInterface(path);
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
                AddCustomInterface((string)e.Data.GetData(DataFormats.Text));
        }

        /// <summary>
        /// Adds a custom interface to <see cref="tileListCatalog"/>.
        /// </summary>
        /// <param name="interfaceUri">The URI of the interface to be added.</param>
        private void AddCustomInterface(string interfaceUri)
        {
            Program.RunCommand(_machineWide, AddApp.Name, interfaceUri);
            tabControlApps.SelectTab(tabPageAppList);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            // Allow dropping of strings and files
            e.Effect = (!DisableDragAndDrop && (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.FileDrop)))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }
        #endregion

        #region Focus handling
        private void tabControlApps_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Redirect text input to appropriate search box
            if (tabControlApps.SelectedTab == tabPageAppList)
            {
                if (!tileListMyApps.TextSearch.Focused)
                {
                    tileListMyApps.TextSearch.Focus();
                    SendKeys.SendWait(e.KeyChar.ToString(CultureInfo.InvariantCulture));
                }
            }
            else if (tabControlApps.SelectedTab == tabPageCatalog)
            {
                if (!tileListCatalog.TextSearch.Focused)
                {
                    tileListCatalog.TextSearch.Focus();
                    SendKeys.SendWait(e.KeyChar.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
        #endregion

        #region Notification Bar
        /// <summary>
        /// Shows a notification bar at the top of the window.
        /// </summary>
        /// <param name="message">The message to display in the notification bar.</param>
        /// <param name="clickHandler">A callback to execute when the notification bar is clicked.</param>
        public void ShowNotificactionBar([NotNull] string message, [NotNull] Action clickHandler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException("message");
            if (clickHandler == null) throw new ArgumentNullException("clickHandler");
            #endregion

            var targetLocation = labelNotificationBar.Location;
            labelNotificationBar.Location -= new Size(0, labelNotificationBar.Height);
            labelNotificationBar.Text = message;
            labelNotificationBar.Visible = true;
            var timer = new Timer {Interval = 10, Enabled = true};
            components.Add(timer);
            timer.Tick += delegate
            {
                labelNotificationBar.Location += new Size(0, 1);
                if (labelNotificationBar.Location == targetLocation) timer.Enabled = false;
            };

            _notificationBarClickHandler = clickHandler;
        }

        private Action _notificationBarClickHandler;

        private void labelNotificationBar_Click(object sender, EventArgs e)
        {
            if (_notificationBarClickHandler != null) _notificationBarClickHandler();
        }
        #endregion

        #region Buttons
        private void buttonSync_Click(object sender, EventArgs e)
        {
            if (IsSyncConfigValid()) Program.RunCommand(_machineWide, SyncApps.Name);
            else SyncWizard.Setup(_machineWide, this);
        }

        private void buttonSyncSetup_Click(object sender, EventArgs e)
        {
            if (IsSyncConfigValid())
            {
                if (!Msg.YesNo(this, Resources.SyncReplaceConfigAsk, MsgSeverity.Warn, Resources.SyncReplaceConfigYes, Resources.SyncReplaceConfigNo))
                {
                    Program.RunCommand(Configure.Name, "--tab=sync");
                    return;
                }
            }

            SyncWizard.Setup(_machineWide, this);
        }

        private void buttonSyncTroubleshoot_Click(object sender, EventArgs e)
        {
            if (IsSyncConfigValid()) SyncWizard.Troubleshooting(_machineWide, this);
            else Msg.Inform(this, Resources.SyncCompleteSetupFirst, MsgSeverity.Warn);
        }

        private bool IsSyncConfigValid()
        {
            try
            {
                var config = Config.Load();
                return config.ToSyncServer().IsValid && !string.IsNullOrEmpty(config.SyncCryptoKey);
            }
                #region Error handling
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return true;
            }
            catch (InvalidDataException ex)
            {
                Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return true;
            }
            #endregion
        }

        private void buttonUpdateAll_Click(object sender, EventArgs e)
        {
            Program.RunCommand(_machineWide, UpdateApps.Name);
        }

        private void buttonUpdateAllClean_Click(object sender, EventArgs e)
        {
            if (Msg.YesNo(this, Resources.UpdateAllCleanWillRemove, MsgSeverity.Warn, Resources.Continue, Resources.Cancel))
                Program.RunCommand(_machineWide, UpdateApps.Name, "--clean");
        }

        private void buttonRefreshCatalog_Click(object sender, EventArgs e)
        {
            LoadCatalogAsync();
        }

        private void buttonMoreApps_Click(object sender, EventArgs e)
        {
            new MoreAppsDialog(_machineWide).Show(this);
        }

        private void buttonOptions_Click(object sender, EventArgs e)
        {
            Program.RunCommand(Configure.Name);
        }

        private void buttonStoreManage_Click(object sender, EventArgs e)
        {
            Program.RunCommand(StoreMan.Name, "manage");
        }

        private void buttonCommandLine_Click(object sender, EventArgs e)
        {
            var cmd = new ProcessStartInfo("cmd.exe", "/k echo " + Resources.CommandLineHint)
            {
                UseShellExecute = false,
                WorkingDirectory = Locations.IsPortable ? Locations.PortableBase : Locations.HomeDir
            };
            cmd.EnvironmentVariables["PATH"] = Locations.InstallBase + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH");
            cmd.Start();
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessUtils.Start("http://0install.de/help/");
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonIntro_Click(object sender, EventArgs e)
        {
            new IntroDialog().Show(this);
        }
        #endregion

        //--------------------//

        #region Self-update
        private void selfUpdateWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                e.Result = SelfUpdateUtils.SilentCheck();
            }
                #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            catch (WebException)
            {}
            catch (UnauthorizedAccessException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            catch (SignatureException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            catch (UriFormatException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            catch (SolverException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            catch (InvalidDataException ex)
            {
                Log.Warn(Resources.UnableToSelfUpdate);
                Log.Warn(ex);
            }
            #endregion
        }

        private void selfUpdateWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var selfUpdateVersion = e.Result as ImplementationVersion;
            if (selfUpdateVersion != null)
            {
                ShowNotificactionBar(string.Format(Resources.SelfUpdateNotification, selfUpdateVersion), delegate
                {
                    try
                    {
                        ProcessUtils.Assembly("0install-win", SelfUpdate.Name, "--batch", "--restart-central").Start();
                        Application.Exit();
                    }
                        #region Error handling
                    catch (OperationCanceledException)
                    {}
                    catch (IOException ex)
                    {
                        Msg.Inform(this, ex.Message, MsgSeverity.Error);
                    }
                    #endregion
                });
            }
        }
        #endregion

        #region MyApps
        /// <summary>
        /// Loads the "my applications" list and displays it, loading additional data from feeds in the background.
        /// </summary>
        private void UpdateAppListAsync()
        {
            if (appListWorker.IsBusy) return;

            var tiles = _tileManagement.UpdateMyApps();
            appListWorker.RunWorkerAsync(tiles);
        }

        private void appListWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var tiles = (IEnumerable<IAppTile>)e.Argument;
            foreach (var tile in tiles)
            {
                if (appListWorker.CancellationPending) return;

                IAppTile tile1 = tile;
                var feed = _tileManagement.LoadFeedSafe(tile.InterfaceUri);
                if (feed != null) BeginInvoke(new Action(() => tile1.Feed = feed));
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == IntegrationManager.ChangedWindowMessageID)
                BeginInvoke(new Action(UpdateAppListAsync));
            else if (m.Msg == AddApp.AddedNonCatalogAppWindowMessageID)
                tabControlApps.SelectedTab = tabPageAppList;

            base.WndProc(ref m);
        }
        #endregion

        #region Catalog
        /// <summary>
        /// Loads the "new applications" catalog in the background and displays it.
        /// </summary>
        private void LoadCatalogAsync()
        {
            if (catalogWorker.IsBusy) return;
            buttonRefreshCatalog.Visible = false;
            labelLoadingCatalog.Visible = true;

            labelLastCatalogError.Visible = false;
            catalogWorker.RunWorkerAsync();
        }

        private void catalogWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = _tileManagement.GetCatalogOnline();
        }

        private void catalogWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var newCatalog = e.Result as Catalog;
                if (newCatalog != null) _tileManagement.SetCatalog(newCatalog);
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
        #endregion
    }
}
