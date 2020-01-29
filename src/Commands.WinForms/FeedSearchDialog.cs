// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        private void queryTimer_Tick(object sender, EventArgs e)
        {
            if (backgroundWorker.IsBusy) return;

            queryTimer.Stop();
            backgroundWorker.RunWorkerAsync(textKeywords.Text);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
            => e.Result = SearchQuery.Perform(Config.Load(), (string)e.Argument);

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                errorProvider.Clear();
                dataGrid.DataSource = _results = ((SearchQuery)e.Result).Results;
            }
            else
            {
                errorProvider.SetIconAlignment(textKeywords, ErrorIconAlignment.MiddleLeft);
                errorProvider.SetError(textKeywords, e.Error.Message);
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
                ProcessUtils.Assembly(Program.ExeName, "run", "--no-wait", result.Uri.ToStringRfc()).Start();
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
                ProcessUtils.Assembly(Program.ExeName, "add", result.Uri.ToStringRfc()).Start();
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
                ProcessUtils.Assembly(Program.ExeName, "integrate", result.Uri.ToStringRfc()).Start();
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
