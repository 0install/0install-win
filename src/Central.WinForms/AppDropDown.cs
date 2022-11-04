// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Commands;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// Drop-down for adding/removing/integrating an app.
/// </summary>
public sealed partial class AppDropDown : DropDownContainer
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
    public AppDropDown(FeedUri interfaceUri, AppTileStatus status, bool machineWide)
    {
        InitializeComponent();
        Font = DefaultFonts.Modern;

        HandleCreated += delegate { RefreshStatus(); };

        _interfaceUri = interfaceUri;
        _machineWide = machineWide;
        _status = status;
    }

    private void RefreshStatus()
    {
        var scale = this.GetScaleFactor();

        void ShowButtons()
        {
            buttonIntegrate.Image = AppResources.IntegratedImage.Get(scale);
            buttonRemove.Image = AppResources.CandidateImage.Get(scale);
            buttonRemove.Show();
            if (!Locations.IsPortable)
            {
                buttonIntegrate.Show();
                buttonIntegrate.Focus();
            }
        }

        switch (_status)
        {
            case AppTileStatus.Candidate:
                AddApp();
                break;

            case AppTileStatus.Added:
                labelStatus.Text = AppResources.AddedText;
                buttonIntegrate.Text = AppResources.IntegrateText;
                buttonRemove.Text = AppResources.RemoveText;
                ShowButtons();
                break;

            case AppTileStatus.Integrated:
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

        if (await RunCommandAsync(Commands.Desktop.AddApp.Name, "--background") == ExitCode.OK)
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
        await RunCommandAsync(IntegrateApp.Name);
        Close();
    }

    private async void buttonRemove_Click(object sender, EventArgs e)
    {
        labelStatus.Text = AppResources.Working;
        Enabled = false;
        await RunCommandAsync(RemoveApp.Name);
        Close();
    }

    private Task<ExitCode> RunCommandAsync(params string[] args)
    {
        if (_machineWide) args = args.Append("--machine");
        return CommandUtils.RunAsync(args.Append(_interfaceUri.ToStringRfc()));
    }
}
