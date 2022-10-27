// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using NanoByte.Common.Native;
using ZeroInstall.Commands;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.Commands.WinForms;
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
            WindowsTaskbar.AddTaskLinks("ZeroInstall", new[]
            {
                new WindowsTaskbar.ShellLink(buttonSync.Text.Replace("&", ""), commandsExe, SyncApps.Name),
                new WindowsTaskbar.ShellLink(buttonUpdateAll.Text.Replace("&", ""), commandsExe, UpdateApps.Name),
                new WindowsTaskbar.ShellLink(buttonStoreManage.Text.Replace("&", ""), commandsExe, StoreMan.Name + " manage")
            });
        }
        else WindowsTaskbar.PreventPinning(Handle);
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        WindowsUtils.RegisterApplicationRestart(_machineWide ? "--restart --machine" : "--restart");

        _tileManagement.UpdateMyApps();
        _tileManagement.LoadCachedCatalog();
        if (Config.LoadSafe().EffectiveNetworkUse == NetworkLevel.Full)
            LoadCatalogAsync();

        bool firstRun = OnFirstRun();
        if (_tileManagement.IsMyAppsEmpty)
        {
            if (firstRun && !Locations.IsPortable)
            {
                using var dialog = new IntroDialog();
                dialog.ShowDialog(this);
            }

            // Show catalog automatically if AppList is empty
            tabControlApps.SelectTab(tabPageCatalog);
        }

        if (!ZeroInstallInstance.IsIntegrated && !Locations.IsPortable) ShowDeployNotification();
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

    private void MainForm_MouseWheel(object sender, MouseEventArgs e)
    {
        if (tabControlApps.SelectedTab == tabPageAppList) tileListMyApps.PerformScroll(e.Delta);
        else if (tabControlApps.SelectedTab == tabPageCatalog) tileListCatalog.PerformScroll(e.Delta);
    }
    #endregion

    #region Drag and drop handling
    private async void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        bool added = false;

        async Task AddAsync(string interfaceUri)
            => added |= await RunCommandAsync(AddApp.Name, interfaceUri) == ExitCode.OK;

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
        labelNotificationBar.Show();
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
    private void buttonSync_Click(object sender, EventArgs e)
    {
        if (Config.LoadSafe().IsSyncConfigured) RunCommandAsync(SyncApps.Name);
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
        => RunCommandAsync(UpdateApps.Name);

    private void buttonUpdateAllClean_Click(object sender, EventArgs e)
    {
        if (Msg.YesNo(this, Resources.UpdateAllCleanWillRemove, MsgSeverity.Warn, Resources.Continue, Resources.Cancel))
            RunCommandAsync(UpdateApps.Name, "--clean");
    }

    private void buttonRefreshCatalog_Click(object sender, EventArgs e)
        => LoadCatalogAsync();

    private void buttonSearch_Click(object sender, EventArgs e)
        => CommandUtils.Start(Search.Name);

    private void buttonAddFeed_Click(object sender, EventArgs e)
    {
        string interfaceUri = InputBox.Show(this, Text, Resources.EnterFeedUrl);
        if (!string.IsNullOrEmpty(interfaceUri)) RunCommandAsync(AddApp.Name, interfaceUri);
    }

    private void buttonAddCatalog_Click(object sender, EventArgs e)
        => CommandUtils.Start(Configure.Name, "--tab=catalog");

    private void buttonFeedEditor_Click(object sender, EventArgs e)
        => CommandUtils.Start(Run.Name, "https://apps.0install.net/0install/0publish-win.xml");

    private void buttonOptions_Click(object sender, EventArgs e)
        => CommandUtils.Start(Configure.Name);

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
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == IntegrationManager.ChangedWindowMessageID)
            BeginInvoke(_tileManagement.UpdateMyApps);
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
                ? new[] {Self.Name, Self.Deploy.Name, "--batch", "--restart-central", "--machine"}
                : new[] {Self.Name, Self.Deploy.Name, "--batch", "--restart-central"});
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
    private async void LoadCatalogAsync()
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

    private Task<ExitCode> RunCommandAsync(params string[] args)
    {
        if (_machineWide) args = args.Append("--machine");
        return CommandUtils.RunAsync(args);
    }
}
