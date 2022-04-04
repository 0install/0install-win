﻿// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Commands;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.Commands.WinForms;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// Popup window for adding/removing/integrating an app.
/// </summary>
public sealed partial class AppPopup : Form
{
    private readonly FeedUri _interfaceUri;
    private readonly bool _machineWide;
    private AppTileStatus _status;

    /// <summary>
    /// Creates a new app popup.
    /// </summary>
    /// <param name="interfaceUri">The interface URI of the application.</param>
    /// <param name="status">Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.</param>
    /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
    public AppPopup(FeedUri interfaceUri, AppTileStatus status, bool machineWide)
    {
        InitializeComponent();
        Font = DefaultFonts.Modern;
        Deactivate += delegate { Close(); };

        _interfaceUri = interfaceUri;
        _machineWide = machineWide;
        _status = status;

        HandleCreated += delegate
        {
            DpiScalingWorkaround();
            RefreshStatus();
        };
    }

    /// <summary>
    /// Shows the popup at the screen coordinates of the specified <paramref name="control"/>.
    /// </summary>
    public void ShowAt(Control control)
    {
        Location = control.PointToScreen(new(control.Width - Width, 0));
        Show(control);
    }

    private void DpiScalingWorkaround()
    {
        iconStatus.Location -= this.GetScaleFactor().Width switch
        {
            2f => new(4, 2),
            1.75f => new(4, 1),
            1.5f => new(1, 1),
            1.25f => new(2, 1),
            _ => new()
        };
    }

    private void RefreshStatus()
    {
        var scale = this.GetScaleFactor();

        void ShowButtons()
        {
            buttonIntegrate.Image = AppResources.IntegratedImage.Get(scale);
            buttonRemove.Image = AppResources.CandidateImage.Get(scale);
            buttonRemove.Show();
            if (Locations.IsPortable)
                buttonClose.Focus();
            else
            {
                buttonIntegrate.Show();
                buttonIntegrate.Focus();
            }
        }

        switch (_status)
        {
            case AppTileStatus.Candidate:
                AddApp();
                iconStatus.Image = AppResources.CandidateImage.Get(scale);
                break;

            case AppTileStatus.Added:
                iconStatus.Image = AppResources.AddedImage.Get(scale);
                labelStatus.Text = AppResources.AddedText;
                buttonIntegrate.Text = AppResources.IntegrateText;
                buttonRemove.Text = AppResources.RemoveText;
                ShowButtons();
                break;

            case AppTileStatus.Integrated:
                iconStatus.Image = AppResources.IntegratedImage.Get(scale);
                labelStatus.Text = AppResources.IntegratedText;
                buttonIntegrate.Text = AppResources.ModifyText;
                buttonRemove.Text = AppResources.RemoveText;
                ShowButtons();
                break;
        }
    }

    private async void AddApp()
    {
        labelStatus.Text = AppResources.Working;

        var exitCode = await RunCommandAsync(Commands.Desktop.AddApp.Name, "--background", _interfaceUri.ToStringRfc());
        if (exitCode == ExitCode.OK)
        {
            _status = AppTileStatus.Added;
            RefreshStatus();
        }
        else Close();
    }

    private async void buttonIntegrate_Click(object sender, EventArgs e)
    {
        labelStatus.Text = AppResources.Working;
        Enabled = false;
        await RunCommandAsync(IntegrateApp.Name, _interfaceUri.ToStringRfc());
        Close();
    }

    private async void buttonRemove_Click(object sender, EventArgs e)
    {
        labelStatus.Text = AppResources.Working;
        Enabled = false;
        await RunCommandAsync(RemoveApp.Name, _interfaceUri.ToStringRfc());
        Close();
    }

    private Task<ExitCode> RunCommandAsync(params string[] args)
    {
        if (_machineWide) args = args.Append("--machine");
        return CommandUtils.RunAsync(args);
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
        Close();
    }
}
