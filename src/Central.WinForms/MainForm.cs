// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using NanoByte.Common.Native;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Configuration;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// The main GUI for Zero Install.
/// </summary>
internal sealed partial class MainForm : Form
{
    #region Variables
    private readonly MinimalTaskHandler _handler;

    /// <summary>Apply operations machine-wide instead of just for the current user.</summary>
    private readonly bool _machineWide;

    /// <summary>Manages <see cref="AppTileList"/>s.</summary>
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
        Font = DefaultFonts.Modern;

        if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
        if (machineWide) Text += @" - " + Resources.MachineWideMode;
        labelVersion.Text = @"v" + ZeroInstallInstance.Version;

        HandleCreated += MainForm_HandleCreated;
        MouseWheel += MainForm_MouseWheel;

        _machineWide = machineWide;

        _handler = new(this);
        _tileManagement = new AppTileManagement(tileListMyApps, tileListCatalog, _machineWide, _handler);
    }
    #endregion

    //--------------------//

    #region Form
    private void MainForm_HandleCreated(object sender, EventArgs e)
    {
        if (ZeroInstallInstance.IsIntegrated)
        {
            string exePath = Path.Combine(Locations.InstallBase, "ZeroInstall.exe");
            string commandsExe = Path.Combine(Locations.InstallBase, "0install-win.exe");
            WindowsTaskbar.SetWindowAppID(Handle, "ZeroInstall", exePath.EscapeArgument(), exePath, "Zero Install");
            WindowsTaskbar.AddTaskLinks("ZeroInstall", [
                new WindowsTaskbar.ShellLink(buttonSync.Text.Replace("&", ""), commandsExe, SyncApps.Name),
                new WindowsTaskbar.ShellLink(buttonUpdateAll.Text.Replace("&", ""), commandsExe, UpdateApps.Name),
                new WindowsTaskbar.ShellLink(buttonStoreManage.Text.Replace("&", ""), commandsExe, StoreMan.Name + " manage")
            ]);
        }
        else WindowsTaskbar.PreventPinning(Handle);
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        WindowsUtils.RegisterApplicationRestart(_machineWide ? "--restart --machine" : "--restart");

        var config = Config.LoadSafe();

        if (config.KioskMode)
            buttonSync.Visible = buttonMoreApps.Visible = buttonOptions.Visible = buttonPortableCreator.Visible = buttonCommandLine.Visible = false;

        _tileManagement.UpdateMyApps();
        _tileManagement.LoadCachedCatalog();
        if (config.EffectiveNetworkUse == NetworkLevel.Full)
            _ = LoadCatalogAsync();

        bool firstRun = OnFirstRun();
        if (_tileManagement.IsMyAppsEmpty)
        {
            if (firstRun && !Locations.IsPortable && !config.KioskMode)
            {
                using var dialog = new IntroDialog();
                dialog.ShowDialog(this);
            }

            // Show catalog automatically if AppList is empty
            tabControlApps.SelectTab(tabPageCatalog);
        }

        if (!ZeroInstallInstance.IsIntegrated && !Locations.IsPortable && !config.KioskMode) ShowDeployNotification();
        if (ZeroInstallInstance.IsDeployed) SelfUpdateCheck();
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
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Log.Error("Failed to first-run flag file", ex);
        }
        #endregion

        return firstRun;
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        WindowsUtils.UnregisterApplicationRestart();

        Hide();
        _handler.Cancel();
    }
    #endregion

    #region Drag and drop handling
    private async void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        bool added = false;

        async Task AddAsync(string interfaceUri)
            => added |= await RunAppCommandAsync(AddApp.Name, interfaceUri) == ExitCode.OK;

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            foreach (string path in (string[])e.Data.GetData(DataFormats.FileDrop))
                await AddAsync(path);
        }
        else if (e.Data.GetDataPresent(DataFormats.Text))
            await AddAsync((string)e.Data.GetData(DataFormats.Text));

        if (added) tabControlApps.SelectTab(tabPageAppList);
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
        => e.Effect = (e.Data.GetDataPresent(DataFormats.Text) || e.Data.GetDataPresent(DataFormats.FileDrop))
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    #endregion

    #region Focus handling
    /// <summary>
    /// The <see cref="AppTileList"/> on the currently selected tab.
    /// </summary>
    private AppTileList? ActiveTileList
        => tabControlApps.SelectedTab == tabPageAppList
            ? tileListMyApps
            : tabControlApps.SelectedTab == tabPageCatalog
                ? tileListCatalog
                : null;

    private void tabControlApps_KeyPress(object sender, KeyPressEventArgs e)
    {
        // Note: TabControl.ProcessKeyPreview() also routes key presses from controls inside the tabs through here, but AppTileList handles those itself.
        // Only input while the tab headers have the focus is left over.
        if (!tabControlApps.Focused) return;

        if (ActiveTileList?.HandleTypedChar(e.KeyChar) == true)
            e.Handled = true;
    }

    private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        => ActiveTileList?.PerformScroll(e.Delta);
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

        // Set up front, so clicks during the slide-in are not lost
        _notificationBarClickHandler = clickHandler;
        labelNotificationBar.Text = message;

        var targetLocation = labelNotificationBar.Location;
        int height = labelNotificationBar.Height;
        labelNotificationBar.Location = targetLocation - new Size(0, height);
        labelNotificationBar.Show();

        const int steps = 8;
        for (int i = 1; i <= steps; i++)
        {
            await Task.Delay(15);
            labelNotificationBar.Location = targetLocation - new Size(0, height * (steps - i) / steps);
        }
    }

    private Action? _notificationBarClickHandler;

    private void labelNotificationBar_Click(object sender, EventArgs e)
        => _notificationBarClickHandler?.Invoke();
    #endregion

    #region Buttons
    private void buttonSync_Click(object sender, EventArgs e)
    {
        if (Config.LoadSafe().IsSyncConfigured) RunAppCommandAsync(SyncApps.Name);
        else SyncWizard.Setup(_machineWide, this);
    }

    private void buttonSyncSetup_Click(object sender, EventArgs e)
    {
        if (Config.LoadSafe().IsSyncConfigured)
        {
            if (!Msg.YesNo(this, Resources.SyncReplaceConfigAsk, MsgSeverity.Warn, Resources.SyncReplaceConfigYes, Resources.SyncReplaceConfigNo))
            {
                CommandUtils.Start(Configure.Name, "--tab=sync");
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

    private void buttonUpdateAll_Click(object sender, EventArgs e)
        => RunAppCommandAsync(UpdateApps.Name);

    private void buttonUpdateAllClean_Click(object sender, EventArgs e)
    {
        if (Msg.YesNo(this, Resources.UpdateAllCleanWillRemove, MsgSeverity.Warn, Resources.Continue, Resources.Cancel))
            RunAppCommandAsync(UpdateApps.Name, "--clean");
    }

    private async void buttonRefreshCatalog_Click(object sender, EventArgs e)
        => await LoadCatalogAsync();

    private void buttonSearch_Click(object sender, EventArgs e)
        => CommandUtils.Start(Search.Name);

    private void buttonAddFeed_Click(object sender, EventArgs e)
    {
        string interfaceUri = InputBox.Show(this, Text, Resources.EnterFeedUrl);
        if (!string.IsNullOrEmpty(interfaceUri)) RunAppCommandAsync(AddApp.Name, interfaceUri);
    }

    private async void buttonAddCatalog_Click(object sender, EventArgs e)
    {
        if (await CommandUtils.RunAsync(Configure.Name, "--tab=catalog") == ExitCode.OK)
            await LoadCatalogAsync();
    }

    private void buttonFeedEditor_Click(object sender, EventArgs e)
        => CommandUtils.Start(Run.Name, "https://apps.0install.net/0install/0publish-gui.xml");

    private async void buttonOptions_Click(object sender, EventArgs e)
    {
        if (await CommandUtils.RunAsync(Configure.Name) == ExitCode.OK)
            await LoadCatalogAsync();
    }

    private void buttonStoreManage_Click(object sender, EventArgs e)
        => CommandUtils.Start(StoreMan.Name, "manage");

    private void buttonCommandLine_Click(object sender, EventArgs e)
        => new ProcessStartInfo("powershell.exe", new[] {"-NoExit", "-Command", $"Write-Host \"{string.Format(Resources.CommandLineHint, "0install --help")}\""}.JoinEscapeArguments())
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
    private bool _updateMyAppsPending;

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == IntegrationManager.ChangedWindowMessageID)
        {
            // Coalesce bursts of change notifications
            if (!_updateMyAppsPending)
            {
                _updateMyAppsPending = true;
                BeginInvoke(new Action(() =>
                {
                    _updateMyAppsPending = false;
                    _tileManagement.UpdateMyApps();
                }));
            }
        }
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

            CommandUtils.Start(machineWide
                ? [Self.Name, Self.Deploy.Name, "--batch", "--restart-central", "--machine"]
                : [Self.Name, Self.Deploy.Name, "--batch", "--restart-central"]);
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
                CommandUtils.Start(Self.Name, Self.Update.Name, "--batch", "--restart-central");
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

    #region Catalog
    /// <summary>
    /// Loads the "new applications" catalog in the background and displays it.
    /// </summary>
    private async Task LoadCatalogAsync()
    {
        buttonRefreshCatalog.Hide();
        labelLoadingCatalog.Show();

        labelLastCatalogError.Hide();
        try
        {
            await _tileManagement.UpdateCatalogAsync();
        }
        catch (Exception ex)
        {
            Log.Error("Failed to update catalog", ex);
            labelLastCatalogError.Text = ex.Message;
            labelLastCatalogError.Show();
        }

        buttonRefreshCatalog.Show();
        labelLoadingCatalog.Hide();
    }
    #endregion

    private Task<ExitCode> RunAppCommandAsync(params string[] args)
        => CommandUtils.RunAsync(_machineWide ? [..args, "--machine"] : args);
}
