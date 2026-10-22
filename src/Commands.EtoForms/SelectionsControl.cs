// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Visualizes <see cref="Selections"/> and allows modifications to <see cref="FeedPreferences"/> and <see cref="InterfacePreferences"/>.
/// </summary>
public sealed class SelectionsControl : Panel
{
    private readonly Scrollable _scrollable = new() {Border = BorderType.None, ExpandContentWidth = true, ExpandContentHeight = false};
    private TableLayout _tableLayout = new();

    /// <summary>
    /// Creates a new <see cref="Selections"/> control.
    /// </summary>
    public SelectionsControl()
    {
        Content = _scrollable;

        TaskControls = new(CreateTaskControls);
    }

    /// <summary>The selections being visualized.</summary>
    private Selections? _selections;

    /// <summary>The feed cache used to retrieve feeds for additional information about implementations.</summary>
    private IFeedManager? _feedManager;

    /// <summary>
    /// Shows the user the <see cref="Selections"/> made by the <see cref="ISolver"/>.
    /// </summary>
    /// <param name="selections">The <see cref="Selections"/> as provided by the <see cref="ISolver"/>.</param>
    /// <param name="feedManager">The feed manager used to retrieve feeds for additional information about implementations.</param>
    /// <remarks>This method must not be called from a background thread.</remarks>
    [MemberNotNull(nameof(_selections))]
    [MemberNotNull(nameof(_feedManager))]
    public void SetSelections(Selections selections, IFeedManager feedManager)
    {
        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
        _feedManager = feedManager ?? throw new ArgumentNullException(nameof(feedManager));

        BuildTable();
        if (_solveCallback != null) CreateLinkButtons();
    }

    /// <summary>The height of each table row. Fixed so rows don't resize when task progress bars are added.</summary>
    private const int RowHeight = 58;

    private void BuildTable()
    {
        if (_selections == null || _feedManager == null) return;

        int count = _selections.Implementations.Count;

        // Additional last row fills remaining space, keeping other rows at top
        _tableLayout = new(3, count + 1) {Spacing = new(12, 0)};
        _tableLayout.SetColumnScale(2);
        _tableLayout.SetRowScale(count);

        for (int i = 0; i < count; i++)
        {
            // Get feed for each selected implementation
            var implementation = _selections.Implementations[i];
            var feed = _feedManager[implementation.InterfaceUri];

            // Display application name and implementation version
            _tableLayout.Add(new Label {Text = feed.Name, VerticalAlignment = VerticalAlignment.Center, Height = RowHeight}, 0, i);
            _tableLayout.Add(new Label {Text = implementation.Version.ToString(), VerticalAlignment = VerticalAlignment.Center}, 1, i);
        }

        _scrollable.Content = _tableLayout;
    }

    /// <summary>
    /// Called after preferences have been changed and the <see cref="ISolver"/> needs to be rerun.
    /// Is set between <see cref="BeginCustomizeSelections"/> and <see cref="EndCustomizeSelections"/>; is <c>null</c> otherwise.
    /// </summary>
    private Func<Selections>? _solveCallback;

    /// <summary>
    /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the <see cref="ISolver"/> if desired.
    /// </summary>
    /// <param name="solveCallback">Called after preferences have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
    /// <remarks>This method must not be called from a background thread.</remarks>
    [MemberNotNull(nameof(_solveCallback))]
    public void BeginCustomizeSelections(Func<Selections> solveCallback)
    {
        _solveCallback = solveCallback ?? throw new ArgumentNullException(nameof(solveCallback));
        CreateLinkButtons();
    }

    private void CreateLinkButtons()
    {
        if (_selections == null) return;

        for (int i = 0; i < _selections.Implementations.Count; i++)
            _tableLayout.Add(CreateLinkButton(_selections.Implementations[i].InterfaceUri), 2, i);
    }

    private Control CreateLinkButton(FeedUri interfaceUri)
    {
        var linkButton = new LinkButton {Text = Commands.Properties.Resources.Change};
        linkButton.Click += delegate { InterfaceDialog.Show(this, interfaceUri, _solveCallback!, _feedManager!); };

        // Wrap to center vertically within table row
        return new StackLayout {Orientation = Orientation.Horizontal, VerticalContentAlignment = VerticalAlignment.Center, Items = {linkButton}};
    }

    /// <summary>
    /// Removes the additional UI added by <see cref="BeginCustomizeSelections"/>.
    /// </summary>
    public void EndCustomizeSelections()
    {
        _solveCallback = null;
        BuildTable();
    }

    /// <summary>
    /// A list of <see cref="TaskControl"/>s addressable by associated <see cref="Implementation"/> via <see cref="ManifestDigest"/>.
    /// Missing entries are transparently created on request.
    /// </summary>
    internal readonly TransparentCache<string, TaskControl> TaskControls;

    private TaskControl CreateTaskControls(string tag)
    {
        var trackingControl = new TaskControl();

        int index = _selections!.Implementations.FindIndex(x => x.ManifestDigest.Best == tag);
        if (index >= 0) _tableLayout.Add(trackingControl, 2, index);

        return trackingControl;
    }
}
