// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Central.WinForms;

/// <summary>
/// A dialog box allowing the user to specify additional options for running a feed.
/// </summary>
public sealed partial class SelectCommandDialog : OKCancelDialog
{
    private readonly FeedUri _feedUri;

    public SelectCommandDialog(FeedUri feedUri)
    {
        _feedUri = feedUri;

        InitializeComponent();
        Font = DefaultFonts.Modern;
        Text = Resources.Run;

        UpdateLabels(new(), EventArgs.Empty);
    }

    private async void SelectCommandDialog_Load(object sender, EventArgs e)
    {
        this.CenterOnParent();

        try
        {
            await SolveAsync(refresh: false);
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or WebException or UnauthorizedAccessException or InvalidDataException or SignatureException or SolverException)
        {
            Log.Info($"Failed to run Solver to show Run options for ${_feedUri}", ex);
        }
        catch (OperationCanceledException)
        {}
        #endregion
    }

    private async void buttonReload_Click(object sender, EventArgs e)
    {
        try
        {
            await SolveAsync(refresh: true);
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or WebException or UnauthorizedAccessException or InvalidDataException or SignatureException or SolverException)
        {
            if (!IsDisposed) // Window might have been closed in the meantime
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
        }
        catch (OperationCanceledException)
        {}
        #endregion

        comboBoxVersion.Focus();
    }

    private async Task SolveAsync(bool refresh)
    {
        using var handler = new DialogTaskHandler(this);
        var services = new ServiceProvider(handler) {FeedManager = {Refresh = refresh}};
        var selections = await Task.Run(() => services.Solver.Solve(_feedUri));
        var feed = services.FeedCache.GetFeed(_feedUri);
        if (feed == null) return;

        Text = Resources.Run + @" " + feed.Name;

        comboBoxVersion.Items.Clear();
        if (selections.MainImplementation.Candidates?.GetSuitableVersions() is {} versions)
            comboBoxVersion.Items.AddRange(versions.ToArray<object>());

        comboBoxCommand.Items.Clear();
        if (feed.EntryPoints is []) comboBoxCommand.Items.Add(new EntryPointWrapper(feed, Command.NameRun));
        else comboBoxCommand.Items.AddRange(feed.EntryPoints.Select(entryPoint => new EntryPointWrapper(feed, entryPoint)).ToArray<object>());
        comboBoxCommand.SelectedIndex = 0;
    }

    private void UpdateLabels(object sender, EventArgs e)
    {
        labelSummary.Text = (comboBoxCommand.SelectedItem as EntryPointWrapper)?.GetSummary();
        textBoxCommandLine.Text = GetArgs().Except("--no-wait").Prepend("0install").JoinEscapeArguments();
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
        CommandUtils.Start(GetArgs().ToArray());
        Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private IEnumerable<string> GetArgs()
    {
        yield return "run";
        yield return "--no-wait";

        string version = comboBoxVersion.SelectedItem?.ToString() ?? comboBoxVersion.Text;
        if (!string.IsNullOrEmpty(version))
        {
            yield return "--version";
            yield return version;
        }

        string command = (comboBoxCommand.SelectedItem as EntryPointWrapper)?.GetCommand() ?? comboBoxCommand.Text;
        if (!string.IsNullOrEmpty(command) && command != Command.NameRun)
        {
            yield return "--command";
            yield return command;
        }

        if (checkBoxCustomize.Checked)
            yield return "--customize";

        if (checkBoxRefresh.Checked)
            yield return "--refresh";

        yield return _feedUri.ToStringRfc();

        if (!string.IsNullOrEmpty(textBoxArgs.Text))
        {
            foreach (string arg in WindowsUtils.SplitArgs(textBoxArgs.Text))
                yield return arg;
        }
    }

    private class EntryPointWrapper(Feed feed, EntryPoint entryPoint)
    {
        public EntryPointWrapper(Feed feed, string commandName)
            : this(feed, new EntryPoint {Command = commandName})
        {}

        public string? GetSummary() => feed.GetBestSummary(CultureInfo.CurrentUICulture, entryPoint.Command);

        public override string ToString() => feed.GetBestName(CultureInfo.CurrentUICulture, entryPoint.Command);

        public string GetCommand() => entryPoint.Command;
    }
}
