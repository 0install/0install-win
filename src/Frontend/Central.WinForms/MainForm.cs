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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Windows.Forms;
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
        private AppTileManagement _tileManagement;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the main GUI.
        /// </summary>
        /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
        public MainForm(bool machineWide)
        {
            InitializeComponent();
            HandleCreated += MainForm_HandleCreated;
            MouseWheel += MainForm_MouseWheel;

            _machineWide = machineWide;
        }
        #endregion

        //--------------------//

        #region Form
        private void MainForm_HandleCreated(object sender, EventArgs e)
        {
            Program.ConfigureTaskbar(this, Text);
            WindowsTaskbar.AddTaskLinks(Program.AppUserModelID, new[]
            {
                new WindowsTaskbar.ShellLink(buttonStoreManage.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, Commands.WinForms.Program.ExeName + ".exe"), StoreMan.Name + " manage"),
                new WindowsTaskbar.ShellLink(buttonOptions.Text.Replace("&", ""), Path.Combine(Locations.InstallBase, Commands.WinForms.Program.ExeName + ".exe"), Configure.Name)
            });
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Branding();

            try
            {
                SetupTileManagement();
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
        }

        private void Branding()
        {
            string brandingPath = Path.Combine(Locations.InstallBase, "_branding");
            if (File.Exists(brandingPath + ".txt")) Text = File.ReadAllText(brandingPath + ".txt");
            if (File.Exists(brandingPath + ".png")) pictureBoxLogo.Image = Image.FromFile(brandingPath + ".png");

            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
            if (_machineWide) Text += @" - " + Resources.MachineWideMode;
            labelVersion.Text = @"v" + AppInfo.Current.Version;
        }

        private void SetupTileManagement()
        {
            tileListMyApps.IconCache = tileListCatalog.IconCache = IconCacheProvider.GetInstance();
            var services = new ServiceLocator(new MinimalTaskHandler(this)) {Config = {NetworkUse = NetworkLevel.Minimal}};
            _tileManagement = new AppTileManagement(
                services.FeedManager, services.CatalogManager,
                tileListMyApps, tileListCatalog, _machineWide);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (!SelfUpdateUtils.NoAutoCheck && !SelfUpdateUtils.IsBlocked) selfUpdateWorker.RunWorkerAsync();

            UpdateAppListAsync();
            _tileManagement.LoadCachedCatalog();
            LoadCatalogAsync();

            bool firstRun = CentralUtils.OnFirstRun();
            if (_tileManagement.AppList.Entries.Count == 0)
            {
                if (firstRun) using (var dialog = new IntroDialog()) dialog.ShowDialog(this);

                // Show catalog automatically if AppList is empty
                tabControlApps.SelectTab(tabPageCatalog);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
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
                Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return;
            }
            #endregion

            if (!config.SyncServer.IsFile && (string.IsNullOrEmpty(config.SyncServerUsername) || string.IsNullOrEmpty(config.SyncServerPassword) || string.IsNullOrEmpty(config.SyncCryptoKey)))
                new Wizards.SyncSetupWizard(_machineWide).Show(this);
            else Program.RunCommand(_machineWide, SyncApps.Name);
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
                Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return;
            }
            #endregion

            if (!string.IsNullOrEmpty(config.SyncServerUsername) || !string.IsNullOrEmpty(config.SyncServerPassword) || !string.IsNullOrEmpty(config.SyncCryptoKey))
                if (!Msg.YesNo(this, Resources.SyncWillReplaceConfig, MsgSeverity.Warn, Resources.Continue, Resources.Cancel)) return;

            new Wizards.SyncSetupWizard(_machineWide).Show(this);
        }

        private void buttonSyncTroubleshoot_Click(object sender, EventArgs e)
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
                Msg.Inform(null, ex.Message + (ex.InnerException == null ? "" : "\n" + ex.InnerException.Message), MsgSeverity.Error);
                return;
            }
            #endregion

            if (string.IsNullOrEmpty(config.SyncServerUsername) || string.IsNullOrEmpty(config.SyncServerPassword) || string.IsNullOrEmpty(config.SyncCryptoKey))
                Msg.Inform(this, Resources.SyncCompleteSetupFirst, MsgSeverity.Warn);
            else
                new Wizards.SyncTroubleshootWizard(_machineWide).Show(this);
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

        private void buttonStoreAudit_Click(object sender, EventArgs e)
        {
            Program.RunCommand(StoreMan.Name, "audit");
        }

        private void buttonStoreOptimise_Click(object sender, EventArgs e)
        {
            Program.RunCommand(StoreMan.Name, "optimise");
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            Program.OpenInBrowser(this, "http://0install.de/help/");
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
                labelSelfUpdateMessage.Text = string.Format(Resources.SelfUpdateAvailable, selfUpdateVersion);
                labelSelfUpdateMessage.Visible = true;
            }
        }

        private void labelSelfUpdateMessage_Click(object sender, EventArgs e)
        {
            try
            {
                ProcessUtils.LaunchAssembly("0install-win", SelfUpdate.Name + " --batch --restart-central");
                Application.Exit();
            }
                #region Error handling
            catch (FileNotFoundException ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            catch (Win32Exception ex)
            {
                Msg.Inform(null, ex.Message, MsgSeverity.Error);
            }
            #endregion
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
