// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using NanoByte.Common.Native;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Services;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Displays the content of caches (<see cref="IFeedCache"/> and <see cref="IImplementationStore"/>) in a combined tree view.
/// </summary>
public sealed partial class StoreManageForm : Form
{
    // Don't use WinForms designer for this, since it doesn't understand generics
    private readonly FilteredTreeView<CacheNodeWithContextMenu> _treeView = new() {CheckBoxes = true, Dock = DockStyle.Fill};

    internal ServiceProvider Services { get; }

    /// <summary>
    /// Creates a new store management window.
    /// </summary>
    /// <param name="nodes">The initial nodes to show.</param>
    /// <exception cref="IOException">There was a problem accessing a configuration file or one of the stores.</exception>
    /// <exception cref="UnauthorizedAccessException">Access to a configuration file or one of the stores was not permitted.</exception>
    public StoreManageForm(IReadOnlyList<CacheNode> nodes)
    {
        InitializeComponent();
        Font = DefaultFonts.Modern;
        buttonRunAsAdmin.AddShieldIcon();
        this.PreventPinningIfNotIntegrated();

        if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
        if (WindowsUtils.IsAdministrator) Text += @" (Administrator)";
        else if (WindowsUtils.HasUac) buttonRunAsAdmin.Show();

        Shown += delegate { RefreshList(nodes); };

        _treeView.SelectedEntryChanged += OnSelectedEntryChanged;
        _treeView.CheckedEntriesChanged += OnCheckedEntriesChanged;
        splitContainer.Panel1.Controls.Add(_treeView);

        Services = new ServiceProvider(new DialogTaskHandler(this));
    }

    /// <summary>
    /// Fills the <see cref="_treeView"/> with entries.
    /// </summary>
    internal void RefreshList(IReadOnlyList<CacheNode>? nodes = null)
    {
        labelLoading.Show();

        try
        {
            nodes ??= new CacheNodeBuilder(Services.Handler, Services.FeedCache, Services.ImplementationStore).Build();
            _treeView.Nodes = new NamedCollection<CacheNodeWithContextMenu>(nodes.Select(x => new CacheNodeWithContextMenu(this, x)));
            textTotalSize.Text = nodes.Sum(x => x.Size).FormatBytes(CultureInfo.CurrentCulture);

            OnCheckedEntriesChanged(null, EventArgs.Empty);
        }
        #region Error handling
        catch (OperationCanceledException) {}
        catch (Exception ex) when (ex is ImplementationNotFoundException or IOException or UnauthorizedAccessException)
        {
            Msg.Inform(null, ex.Message, MsgSeverity.Error);
        }
        #endregion

        labelLoading.Hide();
    }

    private void OnSelectedEntryChanged(object? sender, EventArgs e)
    {
        var node = _treeView.SelectedEntry?.InnerNode;
        propertyGrid.SelectedObject = node;
        textCurrentSize.Text = node?.Size.FormatBytes(CultureInfo.CurrentCulture) ?? "-";
    }

    private void OnCheckedEntriesChanged(object? sender, EventArgs e)
    {
        if (_treeView.CheckedEntries.Count == 0)
        {
            buttonVerify.Enabled = buttonRemove.Enabled = false;
            textCheckedSize.Text = @"-";
        }
        else
        {
            buttonVerify.Enabled = buttonRemove.Enabled = true;
            textCheckedSize.Text = _treeView.CheckedEntries
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
        if (Msg.YesNo(this, string.Format(Resources.DeleteCheckedEntries, _treeView.CheckedEntries.Count), MsgSeverity.Warn))
        {
            try
            {
                foreach (var node in _treeView.CheckedEntries)
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
            foreach (var entry in _treeView.CheckedEntries)
                entry.Verify();
        }
        catch (OperationCanceledException) {}
    }

    private void buttonRefresh_Click(object sender, EventArgs e)
        => RefreshList();

    private void buttonClose_Click(object sender, EventArgs e)
        => Close();
}
