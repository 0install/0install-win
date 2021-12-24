// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using ZeroInstall.Central.WinForms.Properties;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Icons;

namespace ZeroInstall.Central.WinForms
{
    /// <summary>
    /// The main GUI for Zero Install.
    /// </summary>
    internal partial class MainForm : Form
    {
        #region Variables
        private readonly MinimalTaskHandler _handler;

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

            _handler = new(this);
            var services = new ServiceProvider(_handler) {Config = {NetworkUse = NetworkLevel.Minimal}};
            _tileManagement = new AppTileManagement(
                services.FeedManager, services.CatalogManager, IconStores.Default(_handler),
                tileListMyApps, tileListCatalog, _machineWide);
        }
        #endregion

        //--------------------//

        #region Form
        private void MainForm_HandleCreated(object sender, EventArgs e)
        {
            if (Locations.IsPortable || ZeroInstallInstance.IsRunningFromCache)
                WindowsTaskbar.PreventPinning(Handle);
            else
            {
                string exePath = Path.Combine(Locations.InstallBase, "ZeroInstall.exe");
                string commandsExe = Path.Combine(Locations.InstallBase, "0install-win.exe");
                WindowsTaskbar.SetWindowAppID(Handle, "ZeroInstall", exePath.EscapeArgument(), exePath, "Zero Install");
                WindowsTaskbar.AddTaskLinks("ZeroInstall", new[]
                {
                    new WindowsTaskbar.ShellLink(buttonSync.Text.Replace("&", ""), commandsExe, SyncApps.Name),
                    new WindowsTaskbar.ShellLink(buttonUpdateAll.Text.Replace("&", ""), commandsExe, UpdateApps.Name),
                    new WindowsTaskbar.ShellLink(buttonStoreManage.Text.Replace("&", ""), commandsExe, StoreMan.Name + " manage")
                });
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            string brandingPath = Path.Combine(Locations.InstallBase, "_branding");
            if (File.Exists(brandingPath + ".txt")) Text = File.ReadAllText(brandingPath + ".txt");

            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
            if (_machineWide) Text += @" - " + Resources.MachineWideMode;
            labelVersion.Text = @"v" + ZeroInstallInstance.Version;
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            WindowsUtils.RegisterApplicationRestart(_machineWide ? "--restart --machine" : "--restart");

            UpdateAppListAsync();
            _tileManagement.LoadCachedCatalog();
            if (NetUtils.IsInternetConnected) LoadCatalogAsync();

            bool firstRun = OnFirstRun();
            if (_tileManagement.AppList.Entries.Count == 0)
            {
                if (firstRun)
                {
                    using var dialog = new IntroDialog();
                    dialog.ShowDialog(this);
                }

                // Show catalog automatically if AppList is empty
                tabControlApps.SelectTab(tabPageCatalog);
            }

            if (!ZeroInstallInstance.IsIntegrated) ShowDeployNotification();
            if (!ZeroInstallInstance.IsRunningFromCache) SelfUpdateCheck();
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
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (tabControlApps.SelectedTab == tabPageAppList) tileListMyApps.PerformScroll(e.Delta);
            else if (tabControlApps.SelectedTab == tabPageCatalog) tileListCatalog.PerformScroll(e.Delta);
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

        /// <summary>
        /// Adds a custom interface to <see cref="tileListCatalog"/>.
        /// </summary>
        /// <param name="interfaceUri">The URI of the interface to be added.</param>
        private void AddCustomInterface(string interfaceUri)
        {
            Program.RunCommandAsync(_machineWide, AddApp.Name, interfaceUri);
            tabControlApps.SelectTab(tabPageAppList);
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
            => e.Effect = (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.FileDrop))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
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
        public async void ShowNotificationBar(string message, Action clickHandler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));
            if (clickHandler == null) throw new ArgumentNullException(nameof(clickHandler));
            #endregion

            var targetLocation = labelNotificationBar.Location;
            labelNotificationBar.Location -= new Size(0, labelNotificationBar.Height);
            labelNotificationBar.Text = message;
            labelNotificationBar.Visible = true;
            while (labelNotificationBar.Location != targetLocation)
            {
                await Task.Delay(10);
                labelNotificationBar.Location += new Size(0, 1);
            }

            _notificationBarClickHandler = clickHandler;
        }

        private Action? _notificationBarClickHandler;

        private void labelNotificationBar_Click(object sender, EventArgs e)
            => _notificationBarClickHandler?.Invoke();
        #endregion

        #region Buttons
        private async void buttonSync_Click(object sender, EventArgs e)
        {
            if (Config.LoadSafe().IsSyncConfigured) await Program.RunCommandAsync(_machineWide, SyncApps.Name);
            else SyncWizard.Setup(_machineWide, this);
        }

        private async void buttonSyncSetup_Click(object sender, EventArgs e)
        {
            if (Config.LoadSafe().IsSyncConfigured)
            {
                if (!Msg.YesNo(this, Resources.SyncReplaceConfigAsk, MsgSeverity.Warn, Resources.SyncReplaceConfigYes, Resources.SyncReplaceConfigNo))
                {
                    await Program.RunCommandAsync(Configure.Name, "--tab=sync");
                    return;
                }
            }

            SyncWizard.Setup(_machineWide, this);
        }

        private void buttonSyncTroubleshoot_Click(object sender, EventArgs e)
        {
            if (Config.LoadSafe().IsSyncConfigured) SyncWizard.Troubleshooting(_machineWide, this);
            else Msg.Inform(this, Resources.SyncCompleteSetupFirst, MsgSeverity.Warn);
        }

        private async void buttonUpdateAll_Click(object sender, EventArgs e)
            => await Program.RunCommandAsync(_machineWide, UpdateApps.Name);

        private async void buttonUpdateAllClean_Click(object sender, EventArgs e)
        {
            if (Msg.YesNo(this, Resources.UpdateAllCleanWillRemove, MsgSeverity.Warn, Resources.Continue, Resources.Cancel))
                await Program.RunCommandAsync(_machineWide, UpdateApps.Name, "--clean");
        }

        private void buttonRefreshCatalog_Click(object sender, EventArgs e)
            => LoadCatalogAsync();

        private void buttonSearch_Click(object sender, EventArgs e)
            => Program.RunCommandAsync(Search.Name);

        private async void buttonAddFeed_Click(object sender, EventArgs e)
        {
            string interfaceUri = InputBox.Show(this, Text, Resources.EnterFeedUrl);
            if (!string.IsNullOrEmpty(interfaceUri)) await Program.RunCommandAsync(_machineWide, AddApp.Name, interfaceUri);
        }

        private void buttonAddCatalog_Click(object sender, EventArgs e)
            => Program.RunCommandAsync(Configure.Name, "--tab=catalog");

        private void buttonFeedEditor_Click(object sender, EventArgs e)
            => Program.RunCommandAsync(Run.Name, "https://apps.0install.net/0install/0publish-win.xml");

        private void buttonOptions_Click(object sender, EventArgs e)
            => Program.RunCommandAsync(Configure.Name);

        private void buttonStoreManage_Click(object sender, EventArgs e)
            => Program.RunCommandAsync(StoreMan.Name, "manage");

        private void buttonCommandLine_Click(object sender, EventArgs e)
            => new ProcessStartInfo("powershell.exe", new[] {"-NoExit", "-Command", $"Write-Host \"{Resources.CommandLineHint}\""}.JoinEscapeArguments())
            {
                UseShellExecute = false,
                WorkingDirectory = Locations.IsPortable ? Locations.PortableBase : Locations.HomeDir,
                Environment =
                {
                    ["Path"] = Locations.InstallBase + Path.PathSeparator + Environment.GetEnvironmentVariable("Path")
                }
            }.Start();

        private void buttonPortableCreator_Click(object sender, EventArgs e)
            => new PortableCreatorDialog().Show(this);

        private void buttonDocumentation_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://docs.0install.net/");
            }
            #region Error handling
            catch (Exception ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonIntro_Click(object sender, EventArgs e)
            => new IntroDialog().Show(this);
        #endregion

        //--------------------//

        #region Messages
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == IntegrationManager.ChangedWindowMessageID)
                BeginInvoke(new Action(UpdateAppListAsync));
            else if (m.Msg == AddApp.AddedNonCatalogAppWindowMessageID)
                tabControlApps.SelectedTab = tabPageAppList;

            base.WndProc(ref m);
        }
        #endregion

        #region Deploy
        private async void ShowDeployNotification()
        {
            await Task.Delay(1000);
            ShowNotificationBar(Resources.DeployNotification, () =>
            {
                bool machineWide;

                if (WindowsUtils.IsWindowsVista)
                { // Offer choice between per-user and machine-wide using Vista-style dialog box
                    switch (Msg.YesNoCancel(this, Commands.Properties.Resources.AskDeployZeroInstall, MsgSeverity.Info,
                        yesCaption: Resources.ForCurrentUser, noCaption: Resources.ForAllUsers))
                    {
                        case DialogResult.Yes:
                            machineWide = false;
                            break;

                        case DialogResult.No:
                            machineWide = true;
                            break;

                        default:
                            return;
                    }
                }
                else
                { // Inherit machine-wide state from Central on pre-Vista OSes
                    if (Msg.YesNoCancel(this, Commands.Properties.Resources.AskDeployZeroInstall, MsgSeverity.Info) != DialogResult.Yes)
                        return;
                    machineWide = _machineWide;
                }

                Program.RunCommandAsync(machineWide, Self.Name, Self.Deploy.Name, "--batch", "--restart-central");
                Close();
            });
        }
        #endregion

        #region Self-update
        private async void SelfUpdateCheck()
        {
            var availableVersion = await Task.Run(ZeroInstallInstance.SilentUpdateCheck);
            if (availableVersion == null) return;

            ShowNotificationBar(string.Format(Resources.SelfUpdateNotification, availableVersion), delegate
            {
                try
                {
                    Program.RunCommandAsync(Self.Name, Self.Update.Name, "--batch", "--restart-central");
                    Close();
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
        #endregion

        #region MyApps
        /// <summary>
        /// Loads the "My apps" list and displays it, loading additional data from feeds in the background.
        /// </summary>
        private async void UpdateAppListAsync()
        {
            foreach (var tile in _tileManagement.UpdateMyApps())
            {
                var feed = await Task.Run(() => _tileManagement.LoadFeedSafe(tile.InterfaceUri));
                if (feed != null) tile.Feed = feed;
            }
        }
        #endregion

        #region Catalog
        /// <summary>
        /// Loads the "new applications" catalog in the background and displays it.
        /// </summary>
        private async void LoadCatalogAsync()
        {
            buttonRefreshCatalog.Visible = false;
            labelLoadingCatalog.Visible = true;

            labelLastCatalogError.Visible = false;
            try
            {
                _tileManagement.SetCatalog(await Task.Run(_tileManagement.GetCatalogOnline));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                labelLastCatalogError.Text = ex.Message;
                labelLastCatalogError.Visible = true;
            }

            buttonRefreshCatalog.Visible = true;
            labelLoadingCatalog.Visible = false;
        }
        #endregion
    }
}
