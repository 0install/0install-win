// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Collections;
using NanoByte.Common.Controls;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Basic;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the content of caches (<see cref="IFeedCache"/> and <see cref="IImplementationStore"/>) in a combined tree view.
    /// </summary>
    [SuppressMessage("ReSharper", "AsyncVoidLambda")]
    public sealed partial class StoreManageForm : Form
    {
        #region Variables
        private readonly IImplementationStore _store;
        private readonly IFeedCache _feedCache;

        // Don't use WinForms designer for this, since it doesn't understand generics
        private readonly FilteredTreeView<StoreManageNode> _treeView = new() {CheckBoxes = true, Dock = DockStyle.Fill};
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store management window.
        /// </summary>
        /// <param name="store">The <see cref="IImplementationStore"/> to manage.</param>
        /// <param name="feedCache">Information about implementations found in the <paramref name="store"/> are extracted from here.</param>
        public StoreManageForm(IImplementationStore store, IFeedCache feedCache)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _feedCache = feedCache ?? throw new ArgumentNullException(nameof(feedCache));

            InitializeComponent();
            buttonRunAsAdmin.AddShieldIcon();

            HandleCreated += delegate
            {
                if (Locations.IsPortable || ZeroInstallInstance.IsRunningFromCache) WindowsTaskbar.PreventPinning(Handle);
                if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;
                if (WindowsUtils.IsAdministrator) Text += @" (Administrator)";
                else if (WindowsUtils.HasUac) buttonRunAsAdmin.Visible = true;
            };

            Shown += async delegate { await RefreshListAsync(); };

            _treeView.SelectedEntryChanged += OnSelectedEntryChanged;
            _treeView.CheckedEntriesChanged += OnCheckedEntriesChanged;
            splitContainer.Panel1.Controls.Add(_treeView);
        }
        #endregion

        //--------------------//

        #region Build tree list
        /// <summary>
        /// Fills the <see cref="_treeView"/> with entries.
        /// </summary>
        internal async Task RefreshListAsync()
        {
            buttonRefresh.Enabled = false;
            labelLoading.Visible = true;

            try
            {
                var nodeBuilder = await Task.Run(() =>
                {
                    var builder = new CacheNodeBuilder(_store, _feedCache);
                    builder.Run();
                    return builder;
                });

                var nodes = nodeBuilder.Nodes!.Select(x => new StoreManageNode(x, this));
                _treeView.Nodes = new NamedCollection<StoreManageNode>(nodes);
                textTotalSize.Text = nodeBuilder.TotalSize.FormatBytes(CultureInfo.CurrentCulture);

                OnCheckedEntriesChanged(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
            }

            labelLoading.Visible = false;
            buttonRefresh.Enabled = true;
        }
        #endregion

        #region Event handlers
        private void OnSelectedEntryChanged(object? sender, EventArgs e)
        {
            var node = _treeView.SelectedEntry?.BackingNode;
            propertyGrid.SelectedObject = node;

            // Update current entry size
            var implementationEntry = node as ImplementationNode;
            textCurrentSize.Text = implementationEntry?.Size.FormatBytes(CultureInfo.CurrentCulture) ?? "-";
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

                // Update selected entries size
                var nodes = _treeView.CheckedEntries.Select(x => x.BackingNode);
                long totalSize = nodes.OfType<ImplementationNode>().Sum(x => x.Size);
                textCheckedSize.Text = totalSize.FormatBytes(CultureInfo.CurrentCulture);
            }
        }

        private void buttonRunAsAdmin_Click(object? sender, EventArgs e)
        {
            try
            {
                ProcessUtils.Assembly("0install-win", StoreMan.Name, "manage").AsAdmin().Start();
            }
            catch (PlatformNotSupportedException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Error);
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            Close();
        }

        private async void buttonRemove_Click(object? sender, EventArgs e)
        {
            if (Msg.YesNo(this, string.Format(Resources.DeleteCheckedEntries, _treeView.CheckedEntries.Count), MsgSeverity.Warn))
            {
                try
                {
                    using var handler = new DialogTaskHandler(this);
                    foreach (var node in _treeView.CheckedEntries.Select(x => x.BackingNode).ToList())
                        node.Delete(handler);
                }
                #region Error handling
                catch (OperationCanceledException)
                {}
                catch (KeyNotFoundException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Error);
                }
                catch (IOException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Msg.Inform(this, ex.Message, MsgSeverity.Warn);
                }
                #endregion

                finally
                {
                    await RefreshListAsync();
                }
            }
        }

        private async void buttonVerify_Click(object? sender, EventArgs e)
        {
            try
            {
                using var handler = new DialogTaskHandler(this);
                foreach (var entry in _treeView.CheckedEntries.Select(x => x.BackingNode).OfType<ImplementationNode>())
                    entry.Verify(handler);
            }
            #region Error handling
            catch (OperationCanceledException)
            {}
            catch (IOException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            catch (UnauthorizedAccessException ex)
            {
                Msg.Inform(this, ex.Message, MsgSeverity.Warn);
            }
            #endregion

            await RefreshListAsync();
        }

        private async void buttonRefresh_Click(object sender, EventArgs e)
            => await RefreshListAsync();

        private void buttonClose_Click(object sender, EventArgs e)
            => Close();
        #endregion
    }
}
