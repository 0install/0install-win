// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Model.Preferences;
using ZeroInstall.Model.Selection;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Services.Solvers;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Visualizes <see cref="Selections"/> and allows modifications to <see cref="FeedPreferences"/> and <see cref="InterfacePreferences"/>.
/// </summary>
public sealed partial class SelectionsControl : UserControl
{
    /// <summary>
    /// Creates a new <see cref="Selections"/> control.
    /// </summary>
    public SelectionsControl()
    {
        InitializeComponent();
        CreateHandle();

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
    /// <remarks>
    ///   <para>This method must not be called from a background thread.</para>
    ///   <para>This method must not be called before <see cref="Control.Handle"/> has been created.</para>
    /// </remarks>
    [MemberNotNull(nameof(_selections))]
    [MemberNotNull(nameof(_feedManager))]
    public void SetSelections(Selections selections, IFeedManager feedManager)
    {
        if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");

        _selections = selections ?? throw new ArgumentNullException(nameof(selections));
        _feedManager = feedManager;

        BuildTable();
        if (_solveCallback != null) CreateLinkLabels();
    }

    private void BuildTable()
    {
        if (_selections == null || _feedManager == null) return;

        tableLayout.SuspendLayout();

        tableLayout.Controls.Clear();
        tableLayout.RowStyles.Clear();

        tableLayout.RowCount = _selections.Implementations.Count;
        for (int i = 0; i < _selections.Implementations.Count; i++)
        {
            // Give lines a fixed width so they don't resize when task progress bars are added
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58 * this.GetScaleFactor().Height));

            // Get feed for each selected implementation
            var implementation = _selections.Implementations[i];
            var feed = _feedManager[implementation.InterfaceUri];

            // Display application name and implementation version
            tableLayout.Controls.Add(new Label {Text = feed.Name, Dock = DockStyle.Fill, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft}, 0, i);
            tableLayout.Controls.Add(new Label {Text = implementation.Version.ToString(), Dock = DockStyle.Fill, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft}, 1, i);
        }

        tableLayout.ResumeLayout();
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
    /// <remarks>
    ///   <para>This method must not be called from a background thread.</para>
    ///   <para>This method must not be called before <see cref="Control.Handle"/> has been created.</para>
    /// </remarks>
    [MemberNotNull(nameof(_solveCallback))]
    public void BeginCustomizeSelections(Func<Selections> solveCallback)
    {
        if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");

        _solveCallback = solveCallback ?? throw new ArgumentNullException(nameof(solveCallback));
        CreateLinkLabels();
    }

    private void CreateLinkLabels()
    {
        for (int i = 0; i < _selections!.Implementations.Count; i++)
            tableLayout.Controls.Add(CreateLinkLabel(_selections.Implementations[i].InterfaceUri), 2, i);
    }

    private LinkLabel CreateLinkLabel(FeedUri interfaceUri)
    {
        var linkLabel = new LinkLabel {Text = Resources.Change, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft};
        linkLabel.LinkClicked += delegate { InterfaceDialog.Show(this, interfaceUri, _solveCallback!, _feedManager!); };
        return linkLabel;
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
        var trackingControl = new TaskControl {Dock = DockStyle.Fill};
        trackingControl.CreateGraphics(); // Ensure control initialization even in tray icon mode

        int index = _selections!.Implementations.FindIndex(x => x.ManifestDigest.Best == tag);
        tableLayout.Controls.Add(trackingControl, 2, index);

        return trackingControl;
    }
}
