// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Commands.WinForms;
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
    #region Inner class
    private class EntryPointWrapper
    {
        private readonly Feed _feed;
        private readonly EntryPoint _entryPoint;

        public EntryPointWrapper(Feed feed, EntryPoint entryPoint)
        {
            _feed = feed;
            _entryPoint = entryPoint;
        }

        public EntryPointWrapper(Feed feed, string commandName)
        {
            _feed = feed;
            _entryPoint = new() {Command = commandName};
        }

        public string? GetSummary() => _feed.GetBestSummary(CultureInfo.CurrentUICulture, _entryPoint.Command);

        public override string ToString() => _feed.GetBestName(CultureInfo.CurrentUICulture, _entryPoint.Command);

        public string GetCommand() => _entryPoint.Command;
    }
    #endregion

    private readonly FeedUri _feedUri;
    private Feed _feed;
    private Selections? _selections;

    /// <summary>
    /// Creates a dialog box for asking the the user to select an <see cref="Command"/>.
    /// </summary>
    /// <param name="target">The application to be launched.</param>
    public SelectCommandDialog(FeedTarget target)
    {
        _feedUri = target.Uri;
        _feed = target.Feed;

        InitializeComponent();
        Font = DefaultFonts.Modern;
        Text = Resources.Run + @" " + _feed.Name;
    }

    private async void SelectCommandDialog_Load(object sender, EventArgs e)
    {
        this.CenterOnParent();

        try
        {
            await SolveAsync(refresh: false);
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or WebException or UnauthorizedAccessException or SignatureException or SolverException)
        {
            Log.Info($"Failed to run Solver to show Run options for ${_feed}", ex);
        }
        catch (OperationCanceledException)
        {}
        #endregion

        UpdateComboBoxes();
    }

    private async void buttonRefresh_Click(object sender, EventArgs e)
    {
        try
        {
            await SolveAsync(refresh: true);
            UpdateComboBoxes();
        }
        #region Error handling
        catch (Exception ex) when (ex is IOException or WebException or UnauthorizedAccessException or SignatureException or SolverException)
        {
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
        var services = new ServiceProvider(handler) {FeedManager = { Refresh = refresh}};
        _selections = await Task.Run(() => services.Solver.Solve(_feedUri));
        _feed = services.FeedCache.GetFeed(_feedUri) ?? _feed;
    }

    private void UpdateComboBoxes()
    {
        comboBoxVersion.Items.Clear();
        if (_selections?.MainImplementation.Candidates?.GetSuitableVersions() is {} versions)
            comboBoxVersion.Items.AddRange(versions.Cast<object>().ToArray());

        comboBoxCommand.Items.Clear();
        if (_feed.EntryPoints.Count == 0) comboBoxCommand.Items.Add(new EntryPointWrapper(_feed, Command.NameRun));
        else comboBoxCommand.Items.AddRange(_feed.EntryPoints.Select(entryPoint => new EntryPointWrapper(_feed, entryPoint)).Cast<object>().ToArray());
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
}
