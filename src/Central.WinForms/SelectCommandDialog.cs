// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Commands.WinForms;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Store.Feeds;
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
            _entryPoint = new EntryPoint {Command = commandName};
        }

        public string? GetSummary() => _feed.GetBestSummary(CultureInfo.CurrentUICulture, _entryPoint.Command);

        public override string ToString() => _feed.GetBestName(CultureInfo.CurrentUICulture, _entryPoint.Command);

        public string GetCommand() => _entryPoint.Command;
    }
    #endregion

    private readonly IFeedCache _feedCache = FeedCaches.Default(OpenPgp.Verifying());
    private readonly FeedUri _feedUri;
    private readonly Feed _feed;

    /// <summary>
    /// Creates a dialog box for asking the the user to select an <see cref="Command"/>.
    /// </summary>
    /// <param name="target">The application to be launched.</param>
    public SelectCommandDialog(FeedTarget target)
    {
        _feedUri = target.Uri;
        _feed = _feedCache.GetFeed(target.Uri) ?? target.Feed;

        InitializeComponent();
        Font = DefaultFonts.Modern;
    }

    private void SelectCommandDialog_Load(object sender, EventArgs e)
    {
        Text = Resources.Run + @" " + _feed.Name;

        this.CenterOnParent();

        comboBoxVersion.Items.AddRange(GetVersions().Cast<object>().ToArray());

        foreach (var entryPoint in _feed.EntryPoints)
            comboBoxCommand.Items.Add(new EntryPointWrapper(_feed, entryPoint));

        if (comboBoxCommand.Items.Count == 0)
            comboBoxCommand.Items.Add(new EntryPointWrapper(_feed, Command.NameRun));

        comboBoxCommand.SelectedIndex = 0;
    }

    private IEnumerable<ImplementationVersion> GetVersions()
    {
        var additionalFeeds =
            InterfacePreferences.LoadForSafe(_feedUri)
                                .Feeds
                                .TrySelect(x => _feedCache.GetFeed(x.Source), (Exception ex) => Log.Warn("Failed to load feed from cache", ex))
                                .WhereNotNull();

        return _feed.Implementations
               .Concat(additionalFeeds.SelectMany(x => x.Implementations))
               .Select(x => x.Version)
               .WhereNotNull()
               .Distinct()
               .OrderByDescending(x => x);
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
        CommandUtils.Start(GetArgs().ToArray());
        Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
        => Close();

    private void Update(object sender, EventArgs e)
    {
        labelSummary.Text = (comboBoxCommand.SelectedItem as EntryPointWrapper)?.GetSummary();
        textBoxCommandLine.Text = GetArgs().Except("--no-wait").Prepend("0install").JoinEscapeArguments();
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
