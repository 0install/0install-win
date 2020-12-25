// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoByte.Common;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the results of a feed search to the user and allows them to perform additional searches.
    /// </summary>
    public partial class FeedSearchDialog : Form
    {
        private List<SearchResult> _results;

        /// <summary>
        /// Creates a new feed search dialog.
        /// </summary>
        /// <param name="query">An already executed query to visualize.</param>
        public FeedSearchDialog(SearchQuery query)
        {
            #region Sanity checks
            if (query == null) throw new ArgumentNullException(nameof(query));
            #endregion

            InitializeComponent();

            textKeywords.Text = query.Keywords;
            textKeywords.TextChanged += delegate
            {
                queryTimer.Stop();
                queryTimer.Start();
            };

            dataGrid.AutoGenerateColumns = false;
            dataGrid.DataSource = _results = query.Results;
        }

        private static readonly SemaphoreSlim _querySemaphore = new(initialCount: 1);

        private async void queryTimer_Tick(object sender, EventArgs e)
        {
            queryTimer.Stop();

            await _querySemaphore.WaitAsync(); // Prevent concurrent queries
            try
            {
                _results = await Task.Run(() => SearchQuery.Perform(Config.Load(), textKeywords.Text).Results);
            }
            catch (Exception ex)
            {
                errorProvider.SetIconAlignment(textKeywords, ErrorIconAlignment.MiddleLeft);
                errorProvider.SetError(textKeywords, ex.Message);
            }
            finally
            {
                _querySemaphore.Release();
            }
        }

        #region Context menu
        private void dataGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == columnUri.Index)
                contextMenu.Show(Cursor.Position);
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentRow == null) return;
            var result = _results[dataGrid.CurrentRow.Index];

            try
            {
                ProcessUtils.Assembly("0install-win", "run", "--no-wait", result.Uri.ToStringRfc()).Start();
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentRow == null) return;
            var result = _results[dataGrid.CurrentRow.Index];

            try
            {
                ProcessUtils.Assembly("0install-win", "add", result.Uri.ToStringRfc()).Start();
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonIntegrate_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentRow == null) return;
            var result = _results[dataGrid.CurrentRow.Index];

            try
            {
                ProcessUtils.Assembly("0install-win", "integrate", result.Uri.ToStringRfc()).Start();
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }

        private void buttonDetails_Click(object sender, EventArgs e)
        {
            if (dataGrid.CurrentRow == null) return;
            var result = _results[dataGrid.CurrentRow.Index];

            try
            {
                ProcessUtils.Start(result.Uri.ToStringRfc());
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }
            #endregion
        }
        #endregion
    }
}
