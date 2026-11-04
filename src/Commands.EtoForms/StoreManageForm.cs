// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Services;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Displays the content of caches (<see cref="IFeedCache"/> and <see cref="IImplementationStore"/>) in a combined tree view.
/// </summary>
public sealed class StoreManageForm : Dialog
{
    private readonly FilteredTreeView<CacheNodeWithContextMenu> _treeView = new() {CheckBoxes = true};
    private readonly PropertyEditor _propertyEditor = new() {ReadOnly = true};
    private readonly Splitter _splitter;
    private readonly TextBox _textCurrentSize = new() {ReadOnly = true, Text = "-"};
    private readonly TextBox _textCheckedSize = new() {ReadOnly = true, Text = "-"};
    private readonly TextBox _textTotalSize = new() {ReadOnly = true, Text = "-"};
    private readonly Label _labelLoading = new() {Text = Resources.Loading, Visible = false};
    private readonly Button _buttonRunAsAdmin = new() {Text = Resources.RunAsAdmin, Visible = false};
    private readonly Button _buttonVerify = new() {Text = Resources.VerifyEntries, Enabled = false};
    private readonly Button _buttonRemove = new() {Text = Resources.RemoveEntries, Enabled = false};
    private readonly Button _buttonRefresh = new() {Text = Resources.Refresh};
    private readonly Button _buttonClose = new() {Text = Resources.Close};

    private readonly EtoDialogTaskHandler _handler;

    internal ServiceProvider Services { get; }

    /// <summary>
    /// Creates a new store management window.
    /// </summary>
    /// <param name="nodes">The initial nodes to show.</param>
    /// <exception cref="IOException">There was a problem accessing a configuration file or one of the stores.</exception>
    /// <exception cref="UnauthorizedAccessException">Access to a configuration file or one of the stores was not permitted.</exception>
    public StoreManageForm(IReadOnlyList<CacheNode> nodes)
    {
        Title = Resources.StoreManageTitle;
        Padding = 10;
        Resizable = true;
        MinimumSize = new(500, 350);
        this.PreventPinningIfNotIntegrated();

        if (Locations.IsPortable) Title += @" - " + Commands.Properties.Resources.PortableMode;
        if (WindowsUtils.IsWindows)
        {
            if (WindowsUtils.IsAdministrator) Title += @" (Administrator)";
            else if (WindowsUtils.HasUac) _buttonRunAsAdmin.Visible = true;
        }

        this.SetClientSize(new(780, 480));
        LoadComplete += delegate { _splitter.Position = 460; }; // Fixed panel would otherwise keep the width it got before the client size was applied
        Shown += delegate { RefreshList(nodes); };

        _treeView.SelectedEntryChanged += OnSelectedEntryChanged;
        _treeView.CheckedEntriesChanged += OnCheckedEntriesChanged;

        _buttonRunAsAdmin.Click += buttonRunAsAdmin_Click;
        _buttonVerify.Click += buttonVerify_Click;
        _buttonRemove.Click += buttonRemove_Click;
        _buttonRefresh.Click += delegate { RefreshList(); };
        _buttonClose.Click += delegate { Close(); };
        AbortButton = _buttonClose;

        Content = new StackLayout
        {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Spacing = 8,
            Items =
            {
                new StackLayoutItem(_splitter = new Splitter
                {
                    Orientation = Orientation.Horizontal,
                    FixedPanel = SplitterFixedPanel.Panel2,
                    Position = 460,
                    Panel1 = _treeView,
                    Panel2 = new StackLayout
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalContentAlignment = HorizontalAlignment.Stretch,
                        Spacing = 8,
                        Items =
                        {
                            new StackLayoutItem(_propertyEditor, expand: true),
                            new TableLayout
                            {
                                Spacing = new(6, 4),
                                Rows =
                                {
                                    new TableRow(new Label {Text = Resources.CurrentEntrySize, VerticalAlignment = VerticalAlignment.Center}, new TableCell(_textCurrentSize, scaleWidth: true)),
                                    new TableRow(new Label {Text = Resources.CheckedEntriesSize, VerticalAlignment = VerticalAlignment.Center}, _textCheckedSize),
                                    new TableRow(new Label {Text = Resources.TotalSize, VerticalAlignment = VerticalAlignment.Center}, _textTotalSize)
                                }
                            }
                        }
                    }
                }, expand: true),
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Spacing = 6,
                    Items = {_buttonRunAsAdmin, _labelLoading, new StackLayoutItem(null, expand: true), _buttonVerify, _buttonRemove, _buttonRefresh, _buttonClose}
                }
            }
        };
        Maximizable = Minimizable = true; // Set after Content to avoid flickering

        _handler = new(this);
        Services = new ServiceProvider(_handler);
    }

    /// <inheritdoc/>
    protected override void OnClosed(EventArgs e)
    {
        _handler.Dispose();
        base.OnClosed(e);
    }

    /// <summary>
    /// Fills the <see cref="_treeView"/> with entries.
    /// </summary>
    internal void RefreshList(IReadOnlyList<CacheNode>? nodes = null)
    {
        _labelLoading.Visible = true;

        try
        {
            nodes ??= new CacheNodeBuilder(Services.Handler, Services.FeedCache, Services.ImplementationStore).Build();
            _treeView.Nodes = new NamedCollection<CacheNodeWithContextMenu>(nodes.Select(x => new CacheNodeWithContextMenu(this, x)));
            _textTotalSize.Text = nodes.Sum(x => x.Size).FormatBytes(CultureInfo.CurrentCulture);

            OnCheckedEntriesChanged(null, EventArgs.Empty);
        }
        #region Error handling
        catch (OperationCanceledException) {}
        catch (Exception ex) when (ex is ImplementationNotFoundException or IOException or UnauthorizedAccessException)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        #endregion

        _labelLoading.Visible = false;
    }

    private void OnSelectedEntryChanged(object? sender, EventArgs e)
    {
        var node = _treeView.SelectedEntry?.InnerNode;
        _propertyEditor.SelectedObject = node;
        _textCurrentSize.Text = node?.Size.FormatBytes(CultureInfo.CurrentCulture) ?? "-";
    }

    private void OnCheckedEntriesChanged(object? sender, EventArgs e)
    {
        if (_treeView.CheckedEntries.Count == 0)
        {
            _buttonVerify.Enabled = _buttonRemove.Enabled = false;
            _textCheckedSize.Text = @"-";
        }
        else
        {
            _buttonVerify.Enabled = _buttonRemove.Enabled = true;
            _textCheckedSize.Text = _treeView.CheckedEntries
                                             .Select(x => x.InnerNode)
                                             .Sum(x => x.Size)
                                             .FormatBytes(CultureInfo.CurrentCulture);
        }
    }

    private void buttonRunAsAdmin_Click(object? sender, EventArgs e)
    {
        CommandUtils.StartAsAdmin(StoreMan.Name, "manage");
        Close();
    }

    private void buttonRemove_Click(object? sender, EventArgs e)
    {
        if (Msg.YesNo(this, string.Format(Commands.Properties.Resources.DeleteCheckedEntries, _treeView.CheckedEntries.Count), MsgSeverity.Warn))
        {
            try
            {
                foreach (var node in _treeView.CheckedEntries.ToList())
                    node.Remove();
            }
            catch (OperationCanceledException) {}
            finally { RefreshList(); }
        }
    }

    private void buttonVerify_Click(object? sender, EventArgs e)
    {
        try
        {
            foreach (var entry in _treeView.CheckedEntries.ToList())
                entry.Verify();
        }
        catch (OperationCanceledException) {}
    }
}
