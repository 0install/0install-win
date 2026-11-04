// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Collections.ObjectModel;
using System.Security;
using NanoByte.Common.Net;
using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Trust;
using Binding = Eto.Forms.Binding;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Dialog for modifying <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/>.
/// </summary>
public sealed class InterfaceDialog : Dialog
{
    #region Variables
    /// <summary>The interface to modify the preferences for.</summary>
    private readonly FeedUri _interfaceUri;

    /// <summary>The feed loaded from <see cref="_interfaceUri"/>.</summary>
    private readonly Feed _mainFeed;

    /// <summary>Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</summary>
    private readonly Func<Selections> _solveCallback;

    /// <summary>The interface preferences being modified.</summary>
    private readonly InterfacePreferences _interfacePreferences;

    /// <summary>The last implementation selected for this interface.</summary>
    private IEnumerable<SelectionCandidate> _candidates = [];

    /// <summary>The entries in <see cref="_gridFeeds"/>. <see cref="FeedUri"/>s are not removable in GUI, <see cref="FeedReference"/>s are.</summary>
    private readonly ObservableCollection<object> _feeds = [];
    #endregion

    #region Controls
    private readonly object[] _stabilityPolicies = [Commands.Properties.Resources.UseDefaultSetting, Stability.Stable, Stability.Testing, Stability.Developer];
    private readonly DropDown _dropDownStability = new() {ToolTip = Resources.PreferredStabilityToolTip};
    private readonly CheckBox _checkBoxShowAllVersions = new() {Text = Resources.ShowAllVersions, ToolTip = Resources.ShowAllVersionsToolTip};
    private readonly GridView _gridVersions = new() {GridLines = GridLines.Both};
    private readonly GridView _gridFeeds = new() {ShowHeader = false, AllowMultipleSelection = true, AllowDrop = true, BackgroundColor = SystemColors.ControlBackground};
    private readonly Button _buttonAddFeed = new() {Text = Resources.AddFeed, ToolTip = Resources.AddFeedToolTip};
    private readonly Button _buttonRemoveFeed = new() {Text = Resources.RemoveFeed, ToolTip = Resources.RemoveFeedToolTip, Enabled = false};
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new interface dialog.
    /// </summary>
    /// <param name="interfaceUri">The interface to modify the preferences for.</param>
    /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
    /// <param name="feedManager">The feed manager used to retrieve feeds for additional information about implementations.</param>
    private InterfaceDialog(FeedUri interfaceUri, Func<Selections> solveCallback, IFeedManager feedManager)
    {
        #region Sanity checks
        if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
        if (solveCallback == null) throw new ArgumentNullException(nameof(solveCallback));
        if (feedManager == null) throw new ArgumentNullException(nameof(feedManager));
        #endregion

        _interfaceUri = interfaceUri;
        _mainFeed = feedManager[_interfaceUri];
        _solveCallback = solveCallback;
        _interfacePreferences = InterfacePreferences.LoadForSafe(_interfaceUri);

        Title = string.Format(Commands.Properties.Resources.PropertiesFor, _mainFeed.Name);
        Padding = 10;
        Resizable = true;
        MinimumSize = new(550, 220);
        this.SetClientSize(new(734, 345));
        this.PreventPinningIfNotIntegrated();

        Content = new TabControl
        {
            Pages =
            {
                new TabPage {Text = Resources.TabVersions, Padding = 6, Content = BuildVersionsTab()},
                new TabPage {Text = Resources.TabFeeds, Padding = 6, Content = BuildFeedsTab()}
            }
        };
        BuildButtons();
        Maximizable = true; // Set after Content to avoid flickering

        Load += InterfaceDialog_Load;
    }

    private Control BuildVersionsTab()
    {
        _dropDownStability.DataStore = _stabilityPolicies;
        _checkBoxShowAllVersions.CheckedChanged += checkBoxShowAllVersions_CheckedChanged;

        _gridVersions.Columns.Add(TextColumn(Resources.ColumnVersion, x => x.Version.ToString()));
        _gridVersions.Columns.Add(TextColumn(Resources.ColumnReleased, x => x.Released == default ? "" : x.Released.ToString("d", CultureInfo.CurrentCulture)));
        _gridVersions.Columns.Add(TextColumn(Resources.ColumnStability, x => x.Stability.ToString()));
        _gridVersions.Columns.Add(new GridColumn
        {
            HeaderText = Resources.ColumnUserStability,
            Editable = true,
            DataCell = new ComboBoxCell
            {
                DataStore = new[] {Stability.Unset, Stability.Preferred, Stability.Stable, Stability.Testing, Stability.Developer, Stability.Buggy, Stability.Insecure}.Select(x => x.ToString()).ToList(),
                Binding = Binding.Delegate<SelectionCandidate, object>(
                    x => x.UserStability.ToString(),
                    (x, value) =>
                    {
                        if (value is string name && Enum.TryParse(name, out Stability stability)) x.UserStability = stability;
                    })
            }
        });
        _gridVersions.Columns.Add(TextColumn(Resources.ColumnArchitecture, x => x.Architecture));
        _gridVersions.Columns.Add(TextColumn(Resources.ColumnNotes, x => x.Notes));
        _gridVersions.Columns.Add(TextColumn(Resources.ColumnSource, x => x.FeedUri.ToStringRfc(), expand: true));

        return new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 6,
            Items =
            {
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Spacing = 6,
                    Items =
                    {
                        new Label {Text = Resources.PreferredStability},
                        _dropDownStability,
                        new StackLayoutItem(null, expand: true),
                        _checkBoxShowAllVersions
                    }
                },
                new StackLayoutItem(_gridVersions, expand: true)
            }
        };
    }

    private static GridColumn TextColumn(string headerText, Func<SelectionCandidate, string?> getValue, bool expand = false)
        => new() {HeaderText = headerText, Expand = expand, DataCell = new TextBoxCell {Binding = Binding.Delegate(getValue)}};

    private Control BuildFeedsTab()
    {
        _gridFeeds.Columns.Add(new GridColumn
        {
            Expand = true,
            DataCell = new TextBoxCell {Binding = Binding.Delegate<object, string>(x => x is FeedReference reference ? reference.Source.ToString() : x.ToString() ?? "")}
        });
        _gridFeeds.DataStore = _feeds;
        _gridFeeds.SelectionChanged += gridFeeds_SelectionChanged;
        _gridFeeds.DragEnter += gridFeeds_DragOver;
        _gridFeeds.DragOver += gridFeeds_DragOver;
        _gridFeeds.DragDrop += gridFeeds_DragDrop;

        _buttonAddFeed.Click += buttonAddFeed_Click;
        _buttonRemoveFeed.Click += buttonRemoveFeed_Click;

        return new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 6,
            Items =
            {
                new StackLayoutItem(_gridFeeds, expand: true),
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 6,
                    Items = {new StackLayoutItem(null, expand: true), _buttonAddFeed, _buttonRemoveFeed}
                }
            }
        };
    }

    private void BuildButtons()
    {
        var buttonOK = new Button {Text = NanoByte.Common.Properties.Resources.OK};
        buttonOK.Click += buttonOK_Click;
        var buttonApply = new Button {Text = Resources.Apply};
        buttonApply.Click += buttonApply_Click;
        var buttonCancel = new Button {Text = Commands.Properties.Resources.Cancel};
        buttonCancel.Click += delegate { Close(); };

        PositiveButtons.Add(buttonApply);
        PositiveButtons.Add(buttonOK);
        NegativeButtons.Add(buttonCancel);
        DefaultButton = buttonOK;
        AbortButton = buttonCancel;
    }

    private void InterfaceDialog_Load(object? sender, EventArgs e)
    {
        _dropDownStability.SelectedIndex = (_interfacePreferences.StabilityPolicy == Stability.Unset)
            ? 0
            : Math.Max(0, Array.IndexOf(_stabilityPolicies, _interfacePreferences.StabilityPolicy));

        LoadFeeds();
        Solve();
    }
    #endregion

    #region Static access
    /// <summary>
    /// Displays a dialog allowing the user to modify <see cref="InterfacePreferences"/> and <see cref="FeedPreferences"/> for an interface.
    /// </summary>
    /// <param name="owner">The parent window the displayed window is modal to.</param>
    /// <param name="interfaceUri">The interface to modify the preferences for.</param>
    /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
    /// <param name="feedManager">The feed manager used to retrieve feeds for additional information about implementations.</param>
    public static void Show(Control owner, FeedUri interfaceUri, Func<Selections> solveCallback, IFeedManager feedManager)
    {
        #region Sanity checks
        if (interfaceUri == null) throw new ArgumentNullException(nameof(interfaceUri));
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        if (solveCallback == null) throw new ArgumentNullException(nameof(solveCallback));
        if (feedManager == null) throw new ArgumentNullException(nameof(feedManager));
        #endregion

        using var dialog = new InterfaceDialog(interfaceUri, solveCallback, feedManager);
        dialog.ShowModal(owner);
    }
    #endregion

    //--------------------//

    #region Candidates
    /// <summary>
    /// Uses the <see cref="_solveCallback"/> to get the <see cref="SelectionCandidate"/>s.
    /// </summary>
    /// <returns><c>true</c> if the solver ran successfully; <c>false</c> if an error was reported to the user.</returns>
    private bool Solve()
    {
        bool success = true;
        try
        {
            _candidates = _solveCallback()[_interfaceUri].Candidates ?? [];
        }
        #region Error handling
        catch (Exception ex) when (ex is KeyNotFoundException or OperationCanceledException)
        {
            _candidates = [];
        }
        catch (Exception ex) when (ex is IOException or WebException or UnauthorizedAccessException or SignatureException or SolverException)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
            success = false;
        }
        #endregion

        UpdateGridVersions();
        return success;
    }

    /// <summary>
    /// Gets a list of <see cref="SelectionCandidate"/>s from the <see cref="ISolver"/> to populate <see cref="_gridVersions"/>.
    /// </summary>
    private void UpdateGridVersions()
        => _gridVersions.DataStore = _candidates.Where(candidate => candidate.IsSuitable || _checkBoxShowAllVersions.Checked == true).ToList();
    #endregion

    #region Preferences
    private void ApplySettings()
    {
        try
        {
            // Read interface stability policy from DropDown
            if (_dropDownStability.SelectedValue is Stability stability) _interfacePreferences.StabilityPolicy = stability;
            else _interfacePreferences.StabilityPolicy = Stability.Unset;

            // Save interface preferences
            _interfacePreferences.SaveFor(_interfaceUri);

            // Use one representative candidate for each feed to save preferences
            foreach (var candidate in _candidates.ToLookup(x => x.FeedUri).Select(x => x.First()))
            {
                candidate.FeedPreferences.Normalize();
                candidate.FeedPreferences.SaveFor(candidate.FeedUri);
            }
        }
        #region Error handling
        catch (IOException ex)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or SecurityException)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }
    #endregion

    #region Feeds
    /// <summary>
    /// Builds the initial list of feeds.
    /// </summary>
    private void LoadFeeds()
    {
        // Main feed
        _feeds.Add(_interfaceUri); // not removable in GUI

        // Feeds references from main feed
        foreach (var reference in _mainFeed.Feeds)
            _feeds.Add(reference.Source); // not removable in GUI

        // Custom feeds
        foreach (var reference in _interfacePreferences.Feeds)
            _feeds.Add(reference); // removable in GUI
    }

    /// <summary>
    /// Adds a feed to the list of custom feeds.
    /// </summary>
    private void AddCustomFeed(string input)
    {
        FeedUri feedUri;
        try
        {
            feedUri = new(input);
        }
        catch (UriFormatException ex)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
            return;
        }

        if (feedUri.IsFile)
        {
            if (!File.Exists(feedUri.LocalPath))
            {
                Msg.Inform(this, string.Format(Commands.Properties.Resources.FileOrDirNotFound, feedUri.LocalPath), MsgSeverity.Warn);
                return;
            }
        }
        else
        {
            Feed feed = default!;
            try
            {
                using var handler = new EtoDialogTaskHandler(this);
                handler.RunTask(new DownloadFile(feedUri,
                    stream =>
                    {
                        feed = XmlStorage.LoadXml<Feed>(stream);
                        feed.Normalize(feedUri);
                    }));
            }
            #region Error handling
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or WebException or NotSupportedException or InvalidDataException)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            #endregion

            // Ensure a matching <feed-for> is present for online feeds
            if (feed.FeedFor.All(entry => entry.Target != _interfaceUri))
                if (!Msg.YesNo(this, Commands.Properties.Resources.IgnoreMissingFeedFor, MsgSeverity.Warn))
                    return;
        }

        var reference = new FeedReference {Source = feedUri};
        if (_interfacePreferences.Feeds.Contains(reference)) return;
        _interfacePreferences.Feeds.Add(reference);
        _feeds.Add(reference);
    }

    /// <summary>
    /// Removes a feed from the list of custom feeds.
    /// </summary>
    private void RemoveCustomFeed(FeedReference feed)
    {
        _feeds.Remove(feed);
        _interfacePreferences.Feeds.Remove(feed);
    }
    #endregion

    //--------------------//

    #region Feed buttons
    private void gridFeeds_SelectionChanged(object? sender, EventArgs e)
        => _buttonRemoveFeed.Enabled = _gridFeeds.SelectedItems.OfType<FeedReference>().Any();

    private void buttonAddFeed_Click(object? sender, EventArgs e)
    {
        string? input = InputBox.Show(this, "Feed", Commands.Properties.Resources.EnterFeedUrl);
        if (!string.IsNullOrEmpty(input)) AddCustomFeed(input);
    }

    private void buttonRemoveFeed_Click(object? sender, EventArgs e)
    {
        var toRemove = _gridFeeds.SelectedItems.OfType<FeedReference>().ToList();

        if (!Msg.YesNo(this, string.Format(Commands.Properties.Resources.RemoveSelectedEntries, toRemove.Count), MsgSeverity.Warn)) return;
        foreach (var feed in toRemove) RemoveCustomFeed(feed);
    }
    #endregion

    #region Global buttons
    private void checkBoxShowAllVersions_CheckedChanged(object? sender, EventArgs e)
        => UpdateGridVersions();

    private void buttonApply_Click(object? sender, EventArgs e)
    {
        ApplySettings();
        Solve();
    }

    private void buttonOK_Click(object? sender, EventArgs e)
    {
        ApplySettings();
        if (Solve()) Close(); // Keep dialog open if there was a problem
    }
    #endregion

    #region Drag and drop handling
    private void gridFeeds_DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.ContainsUris)
        {
            foreach (var uri in e.Data.Uris)
                AddCustomFeed(uri.IsFile ? uri.LocalPath : uri.OriginalString);
        }
        else if (e.Data.ContainsText)
            AddCustomFeed(e.Data.Text);
    }

    private void gridFeeds_DragOver(object? sender, DragEventArgs e)
        => e.Effects = (e.Data.ContainsText || e.Data.ContainsUris)
            ? DragEffects.Copy
            : DragEffects.None;
    #endregion
}
