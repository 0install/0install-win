// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.WinForms;
using ZeroInstall.DesktopIntegration;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// Represents an application as a tile with buttons for launching, managing, etc..
/// </summary>
public sealed partial class AppTile : UserControl
{
    /// <summary>Apply operations machine-wide instead of just for the current user.</summary>
    private readonly bool _machineWide;

    /// <summary>
    /// The interface URI of the application this tile represents.
    /// </summary>
    public FeedUri InterfaceUri { get; }

    /// <summary>
    /// The name of the application this tile represents.
    /// </summary>
    public string AppName => labelName.Text;

    /// <summary>
    /// A summary of the application this tile represents.
    /// </summary>
    public string AppSummary => labelSummary.Text;

    private AppTileStatus _status;

    /// <summary>
    /// Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.
    /// </summary>
    /// <exception cref="InvalidOperationException">The value is set from a thread other than the UI thread.</exception>
    /// <remarks>This method must not be called from a background thread.</remarks>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AppTileStatus Status
    {
        get => _status;
        set
        {
            if (InvokeRequired) throw new InvalidOperationException("Property set from a non UI thread.");
            _status = value;
            RefreshStatus();
        }
    }

    /// <summary>
    /// Creates a new application tile.
    /// </summary>
    /// <param name="interfaceUri">The interface URI of the application this tile represents.</param>
    /// <param name="appName">The name of the application this tile represents.</param>
    /// <param name="status">Describes whether the application is listed in the <see cref="AppList"/> and if so whether it is integrated.</param>
    /// <param name="machineWide">Apply operations machine-wide instead of just for the current user.</param>
    public AppTile(FeedUri interfaceUri, string appName, AppTileStatus status, bool machineWide = false)
    {
        _machineWide = machineWide;

        InitializeComponent();
        Font = DefaultFonts.Modern;
        buttonRun.Text = buttonRun2.Text = AppResources.RunText;
        buttonRunWithOptions.Text = AppResources.RunWithOptionsText;
        buttonUpdate.Text = AppResources.UpdateText;

        InterfaceUri = interfaceUri ?? throw new ArgumentNullException(nameof(interfaceUri));
        labelName.Text = appName ?? throw new ArgumentNullException(nameof(appName));
        labelSummary.Text = "";
        _status = status;

        HandleCreated += delegate { RefreshStatus(); };

        CreateHandle();
    }

    private Feed? _feed;

    /// <summary>
    /// Sets the <see cref="Feed"/> from which the tile extracts relevant application metadata such as summaries.
    /// </summary>
    public AppTile SetFeed(Feed? feed)
    {
        _feed = feed;
        if (feed == null)
        {
            buttonRunWithOptions.Visible = false;
        }
        else
        {
            buttonRunWithOptions.Visible = true;
            labelSummary.Text = feed.Summaries.GetBestLanguage(CultureInfo.CurrentUICulture);
        }
        return this;
    }

    /// <summary>
    /// Shows an icon on the tile.
    /// </summary>
    public void SetIcon(Image icon)
        => pictureBoxIcon.Image = icon ?? throw new ArgumentNullException(nameof(icon));

    /// <summary>
    /// Shows an icon on the tile loaded from a file on disk.
    /// </summary>
    public void SetIcon(string path)
    {
        try
        {
            pictureBoxIcon.LoadAsync(path);
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }

    private void RefreshStatus()
    {
        (string text, var image) = (_status switch
        {
            AppTileStatus.Candidate => (AppResources.CandidateText, AppResources.CandidateImage),
            AppTileStatus.Added => (AppResources.AddedText, AppResources.AddedImage),
            AppTileStatus.Integrated => (AppResources.IntegrateText, AppResources.IntegratedImage),
            _ => throw new InvalidOperationException()
        });

        buttonIntegrate.AccessibleName = text;
        buttonIntegrate.Image = image.Get(this.GetScaleFactor());
        toolTip.SetToolTip(buttonIntegrate, text);
    }

    private void LinkClicked(object sender, EventArgs e)
    {
        if (InterfaceUri.IsFake) return;
        try
        {
            Process.Start(InterfaceUri.OriginalString);
        }
        #region Error handling
        catch (Exception ex)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }

    private void buttonRun_Click(object sender, EventArgs e)
    {
        if (InterfaceUri.IsFake) return;
        if (_feed is {NeedsTerminal: true}) new SelectCommandDialog(new(InterfaceUri, _feed)).Show(this);
        else CommandUtils.Start(Run.Name, "--no-wait", InterfaceUri.ToStringRfc());
    }

    private void buttonRunWithOptions_Click(object sender, EventArgs e)
    {
        if (InterfaceUri.IsFake || _feed == null) return;
        new SelectCommandDialog(new(InterfaceUri, _feed)).Show(this);
    }

    private void buttonUpdate_Click(object sender, EventArgs e)
    {
        if (InterfaceUri.IsFake) return;
        CommandUtils.Start(Commands.Basic.Update.Name, InterfaceUri.ToStringRfc());
    }

    private void buttonIntegrate_Click(object sender, EventArgs e)
    {
        if (InterfaceUri.IsFake) return;
        new AppPopup(InterfaceUri, Status, _machineWide)
           .ShowAt(buttonIntegrate);
    }
}
