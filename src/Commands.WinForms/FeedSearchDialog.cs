// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Diagnostics;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.Desktop;
using ZeroInstall.Store.Configuration;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands.WinForms;

/// <summary>
/// Displays the results of a feed search to the user and allows them to perform additional searches.
/// </summary>
public sealed partial class FeedSearchDialog : Form
{
    private List<SearchResult> _results;

    /// <summary>
    /// Creates a new feed search dialog.
    /// </summary>
    /// <param name="keywords">The keywords that were searched for.</param>
    /// <param name="results">The results of the search.</param>
    public FeedSearchDialog(string? keywords, IEnumerable<SearchResult> results)
    {
        InitializeComponent();
        Font = DefaultFonts.Modern;
        this.PreventPinningIfNotIntegrated();

        textKeywords.Text = keywords ?? "";
        textKeywords.TextChanged += textKeywords_TextChanged;

        dataGrid.AutoGenerateColumns = false;
        dataGrid.DataSource = _results = results.ToList();
    }

    private async void textKeywords_TextChanged(object sender, EventArgs e)
    {
        var cancellationToken = PreventConcurrentRequests();
        string keywords = textKeywords.Text;

        try
        {
            await Task.Delay(200, cancellationToken);
            dataGrid.DataSource = _results = await Task.Run(() => SearchResults.Query(Config.Load(), keywords), cancellationToken);
        }
        catch (OperationCanceledException)
        {}
        catch (Exception ex)
        {
            errorProvider.SetIconAlignment(textKeywords, ErrorIconAlignment.MiddleLeft);
            errorProvider.SetError(textKeywords, ex.Message);
        }
    }

    private CancellationTokenSource _cts = new();

    private CancellationToken PreventConcurrentRequests()
    {
        _cts.Cancel();
        _cts = new();
        return _cts.Token;
    }

    private void dataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == columnUri.Index)
            contextMenu.Show(Cursor.Position);
    }

    private void buttonRun_Click(object sender, EventArgs e)
    {
        if (dataGrid.CurrentRow == null) return;
        var result = _results[dataGrid.CurrentRow.Index];

        CommandUtils.Start(Run.Name, "--no-wait", result.Uri!.ToStringRfc());
    }

    private void buttonAdd_Click(object sender, EventArgs e)
    {
        if (dataGrid.CurrentRow == null) return;
        var result = _results[dataGrid.CurrentRow.Index];

        CommandUtils.Start(AddApp.Name, result.Uri!.ToStringRfc());
    }

    private void buttonIntegrate_Click(object sender, EventArgs e)
    {
        if (dataGrid.CurrentRow == null) return;
        var result = _results[dataGrid.CurrentRow.Index];

        CommandUtils.Start(IntegrateApp.Name, result.Uri!.ToStringRfc());
    }

    private void buttonDetails_Click(object sender, EventArgs e)
    {
        if (dataGrid.CurrentRow == null) return;
        var result = _results[dataGrid.CurrentRow.Index];

        try
        {
            Process.Start(result.Uri!.ToStringRfc());
        }
        #region Error handling
        catch (Exception ex)
        {
            Msg.Inform(this, ex.Message, MsgSeverity.Error);
        }
        #endregion
    }
}
