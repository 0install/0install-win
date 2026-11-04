// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Feeds;
using Binding = Eto.Forms.Binding;

namespace ZeroInstall.Commands.EtoForms;

/// <summary>
/// Displays the results of a feed search to the user and allows them to perform additional searches.
/// </summary>
public sealed class FeedSearchDialog : Dialog
{
    private readonly TextBox _textKeywords = new() {PlaceholderText = Resources.Keywords};
    private readonly GridView _dataGrid = new() {GridLines = GridLines.Horizontal, BackgroundColor = SystemColors.ControlBackground};
    private readonly GridColumn _columnUri;
    private readonly ContextMenu _contextMenu;

    /// <summary>
    /// Creates a new feed search dialog.
    /// </summary>
    /// <param name="keywords">The keywords that were searched for.</param>
    /// <param name="results">The results of the search.</param>
    public FeedSearchDialog(string? keywords, IEnumerable<SearchResult> results)
    {
        Title = Resources.FeedSearchTitle;
        if (Locations.IsPortable) Title += @" - " + Commands.Properties.Resources.PortableMode;
        Padding = 10;
        Resizable = true;
        MinimumSize = new(400, 250);
        this.SetClientSize(new(760, 420));
        this.PreventPinningIfNotIntegrated();

        _textKeywords.Text = keywords ?? "";
        _textKeywords.TextChanged += textKeywords_TextChanged;

        _dataGrid.Columns.Add(_columnUri = TextColumn(Resources.ColumnUri, x => x.Uri.ToStringRfc()));
        _dataGrid.Columns.Add(TextColumn(Resources.ColumnName, x => x.Name));
        _dataGrid.Columns.Add(TextColumn(Resources.ColumnScore, x => x.Score.ToString(CultureInfo.CurrentCulture)));
        _dataGrid.Columns.Add(TextColumn(Resources.ColumnSummary, x => x.Summary, expand: true));
        _dataGrid.Columns.Add(TextColumn(Resources.ColumnCategories, x => x.CategoriesString));
        _dataGrid.DataStore = results.ToList();

        _contextMenu = new(
            MenuItem(Resources.Run, buttonRun_Click),
            new SeparatorMenuItem(),
            MenuItem(Resources.AddToMyApps, buttonAdd_Click),
            MenuItem(Resources.SetupIntegration, buttonIntegrate_Click),
            new SeparatorMenuItem(),
            MenuItem(Resources.Details, buttonDetails_Click));
        _dataGrid.ContextMenu = _contextMenu;
        _dataGrid.CellClick += dataGrid_CellClick;

        Content = new TableLayout
        {
            Spacing = new(0, 8),
            Rows =
            {
                new Label {Text = Resources.FeedSearchInfo, Wrap = WrapMode.Word}.ReflowOnResize(),
                _textKeywords,
                new TableRow(_dataGrid) {ScaleHeight = true}
            }
        };
        Maximizable = Minimizable = true; // Set after Content to avoid flickering
    }

    private static GridColumn TextColumn(string headerText, Func<SearchResult, string?> getValue, bool expand = false)
        => new() {HeaderText = headerText, Expand = expand, DataCell = new TextBoxCell {Binding = Binding.Delegate(getValue)}};

    private static ButtonMenuItem MenuItem(string text, EventHandler<EventArgs> onClick)
    {
        var item = new ButtonMenuItem {Text = text};
        item.Click += onClick;
        return item;
    }

    private async void textKeywords_TextChanged(object? sender, EventArgs e)
    {
        var cancellationToken = PreventConcurrentRequests();
        string keywords = _textKeywords.Text;

        try
        {
            await Task.Delay(200, cancellationToken);
            _dataGrid.DataStore = await Task.Run(() => SearchResults.Query(Config.Load(), keywords), cancellationToken);
            _textKeywords.TextColor = SystemColors.ControlText;
            _textKeywords.ToolTip = null;
        }
        catch (OperationCanceledException)
        {}
        catch (Exception ex)
        {
            _textKeywords.TextColor = Colors.Red;
            _textKeywords.ToolTip = ex.Message;
        }
    }

    private CancellationTokenSource _cts = new();

    private CancellationToken PreventConcurrentRequests()
    {
        _cts.Cancel();
        _cts = new();
        return _cts.Token;
    }

    private void dataGrid_CellClick(object? sender, GridCellMouseEventArgs e)
    {
        if (e.GridColumn == _columnUri && e.Row >= 0)
        {
            _dataGrid.SelectedRow = e.Row;
            _contextMenu.Show(_dataGrid);
        }
    }

    private void buttonRun_Click(object? sender, EventArgs e)
    {
        if (_dataGrid.SelectedItem is not SearchResult result) return;

        CommandUtils.Start(Run.Name, "--no-wait", result.Uri.ToStringRfc());
    }

    private void buttonAdd_Click(object? sender, EventArgs e)
    {
        if (_dataGrid.SelectedItem is not SearchResult result) return;

        CommandUtils.Start(AddApp.Name, result.Uri.ToStringRfc());
    }

    private void buttonIntegrate_Click(object? sender, EventArgs e)
    {
        if (_dataGrid.SelectedItem is not SearchResult result) return;

        CommandUtils.Start(IntegrateApp.Name, result.Uri.ToStringRfc());
    }

    private void buttonDetails_Click(object? sender, EventArgs e)
    {
        if (_dataGrid.SelectedItem is not SearchResult result) return;

        CommandUtils.Open(this, result.Uri.ToStringRfc());
    }
}
