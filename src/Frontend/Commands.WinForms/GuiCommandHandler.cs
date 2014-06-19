/*
 * Copyright 2010-2014 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Controls;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Uses <see cref="System.Windows.Forms"/> to allow users to interact with <see cref="FrontendCommand"/>s.
    /// </summary>
    /// <remarks>This class manages a GUI thread with an independent message queue. Invoking methods on the right thread is handled automatically.</remarks>
    public sealed class GuiCommandHandler : MarshalNoTimeout, ICommandHandler
    {
        #region Properties
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return _cancellationTokenSource.Token; } }

        /// <inheritdoc/>
        public int Verbosity { get; set; }

        /// <inheritdoc/>
        public bool Batch { get; set; }

        private string _actionTitle;

        /// <inheritdoc/>
        public void SetGuiHints(Func<string> actionTitle, int delay)
        {
            #region Sanity checks
            if (actionTitle == null) throw new ArgumentNullException("actionTitle");
            #endregion

            _actionTitle = actionTitle();
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new GUI handler with an external <see cref="CancellationTokenSource"/>.
        /// </summary>
        public GuiCommandHandler(CancellationTokenSource cancellationTokenSource)
        {
            _cancellationTokenSource = cancellationTokenSource;
        }

        /// <summary>
        /// Creates a new GUI handler with its own <see cref="CancellationTokenSource"/>.
        /// </summary>
        public GuiCommandHandler() : this(new CancellationTokenSource())
        {}
        #endregion

        #region Dispose
        /// <inheritdoc/>
        public void Dispose()
        {
            _modifySelectionsWaitHandle.Close();
            if (_form != null) _form.Dispose();
            _cancellationTokenSource.Dispose();
        }
        #endregion

        //--------------------//

        #region Task tracking
        /// <summary>Synchronization object used to prevent multiple concurrent generic <see cref="ITask"/>s.</summary>
        private readonly object _genericTaskLock = new object();

        /// <inheritdoc/>
        public void RunTask(ITask task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            IProgress<TaskSnapshot> progress = null;
            if (task.Tag is ManifestDigest)
            {
                // Handle events coming from a non-UI thread
                _form.Invoke(new Action(() => progress = _form.SetupProgress(task.Name, (ManifestDigest)task.Tag)));
            }
            else
            {
                lock (_genericTaskLock) // Prevent multiple concurrent generic tasks
                {
                    // Handle events coming from a non-UI thread
                    _form.Invoke(new Action(() => progress = _form.SetupProgress(task.Name)));
                }
            }

            task.Run(CancellationToken, progress);
        }
        #endregion

        #region UI control
        private ProgressForm _form;

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "handle", Justification = "Need to retrieve value from Form.Handle to force window handle creation")]
        public void ShowProgressUI()
        {
            // Can only show GUI once
            if (_form != null) return;

            // Start GUI thread
            using (var guiReady = new ManualResetEvent(false))
            {
                ProcessUtils.RunAsync(() =>
                {
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

                    _form = new ProgressForm(_cancellationTokenSource);

                    if (Batch)
                    {
                        // Force creation of handle for invisible form
                        // ReSharper disable once UnusedVariable
                        var handle = _form.Handle;

                        _form.ShowTrayIcon(_actionTitle, ToolTipIcon.None);
                    }
                    else _form.Show();

                    // Signal that the GUI handles have been created
                    // ReSharper disable once AccessToDisposedClosure
                    guiReady.Set();

                    Application.Run();
                }, "GuiHandler.ProgressUI");
                guiReady.WaitOne(); // Wait until the GUI handles have been created
            }
        }

        /// <inheritdoc/>
        public void DisableProgressUI()
        {
            if (_form == null) return;

            _form.Invoke(new Action(() => _form.Enabled = false));
        }

        /// <inheritdoc/>
        public void CloseProgressUI()
        {
            if (_form == null) return;

            try
            {
                var form = _form;
                _form = null;
                form.Invoke(new Action(() =>
                {
                    form.HideTrayIcon();
                    Application.ExitThread();
                    form.Dispose();
                }));
            }
                #region Error handling
            catch (InvalidOperationException)
            {
                // Don't worry if the form was disposed in the meantime
            }
            catch (RemotingException ex)
            {
                // Remoting exceptions on clean-up are not critical
                Log.Warn(ex);
            }
            #endregion
        }
        #endregion

        #region Question
        /// <inheritdoc/>
        public bool AskQuestion(string question, string batchInformation = null)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(question)) throw new ArgumentNullException("question");
            #endregion

            if (_form == null) return false;

            // Handle events coming from a non-UI thread, block caller until user has answered
            bool result = false;
            _form.Invoke(new Action(delegate
            {
                // Auto-deny unknown keys and inform via tray icon when in batch mode
                if (Batch && !string.IsNullOrEmpty(batchInformation)) _form.ShowTrayIcon(batchInformation, ToolTipIcon.Warning);
                else
                {
                    switch (Msg.YesNoCancel(_form, question, MsgSeverity.Info))
                    {
                        case DialogResult.Yes:
                            result = true;
                            break;
                        case DialogResult.No:
                            result = false;
                            break;
                        default:
                            throw new OperationCanceledException();
                    }
                }
            }));
            return result;
        }
        #endregion

        #region Selections UI
        /// <inheritdoc/>
        public void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            if (_form == null) return;

            try
            {
                _form.Invoke(new Action(() => _form.ShowSelections(selections, feedCache)));
            }
            catch (InvalidOperationException)
            {
                // Don't worry if the form was disposed in the meantime
            }
        }

        /// <summary>A wait handle used by <see cref="ModifySelections"/> to be signaled once the user is satisfied with the <see cref="Selections"/>.</summary>
        private readonly AutoResetEvent _modifySelectionsWaitHandle = new AutoResetEvent(false);

        /// <inheritdoc/>
        public void ModifySelections(Func<Selections> solveCallback)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            #endregion

            if (_form == null) return;

            // Show "modify selections" screen and then asynchronously wait until its done
            _form.Invoke(new Action(() =>
            {
                // Leave tray icon mode
                _form.Show();
                Application.DoEvents();

                _form.ModifySelections(solveCallback, _modifySelectionsWaitHandle);
            }));

            _modifySelectionsWaitHandle.WaitOne();
        }
        #endregion

        #region Messages
        /// <inheritdoc/>
        public void Output(string title, string information)
        {
            DisableProgressUI();
            if (Batch) ShowBalloonMessage(title, information);
            else OutputBox.Show(title, information);
        }

        /// <summary>
        /// Displays a tray icon with balloon message detached from the main GUI (will stick around even after the process ends).
        /// </summary>
        /// <param name="title">The title of the balloon message.</param>
        /// <param name="information">The balloon message text.</param>
        private void ShowBalloonMessage(string title, string information)
        {
            if (_form == null) return;

            // Remove existing tray icon to give new balloon priority
            _form.Invoke(new Action(() => _form.HideTrayIcon()));

            var icon = new NotifyIcon {Visible = true, Icon = Resources.TrayIcon};
            icon.ShowBalloonTip(10000, title, information, ToolTipIcon.Info);
        }
        #endregion

        #region Dialogs
        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state)
        {
            #region Sanity checks
            if (state == null) throw new ArgumentNullException("state");
            #endregion

            if (_form == null) return;

            _form.Invoke(new Action(() =>
            {
                var integrationForm = new IntegrateAppForm(state);

                // The IntegrateAppForm and ProgressForm take turns in being visible
                integrationForm.VisibleChanged += delegate
                {
                    if (integrationForm.Visible || (integrationForm.DialogResult == DialogResult.Cancel))
                    {
                        _form.Visible = false;
                        _form.HideTrayIcon();
                    }
                    else _form.Show();
                };

                integrationForm.ShowDialog();
            }));
        }

        /// <inheritdoc/>
        public void ShowConfig(Config config, ConfigTab configTab)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            using (var dialog = new ConfigDialog(config))
            {
                switch (configTab)
                {
                    case ConfigTab.Updates:
                        dialog.tabOptions.SelectTab(dialog.tabPageUpdates);
                        break;
                    case ConfigTab.Storage:
                        dialog.tabOptions.SelectTab(dialog.tabPageStorage);
                        break;
                    case ConfigTab.Catalog:
                        dialog.tabOptions.SelectTab(dialog.tabPageCatalog);
                        break;
                    case ConfigTab.Trust:
                        dialog.tabOptions.SelectTab(dialog.tabPageTrust);
                        break;
                    case ConfigTab.Sync:
                        dialog.tabOptions.SelectTab(dialog.tabPageSync);
                        break;
                    case ConfigTab.Advanced:
                        dialog.tabOptions.SelectTab(dialog.tabPageAdvanced);
                        break;
                }

                if (dialog.ShowDialog() != DialogResult.OK) throw new OperationCanceledException();
            }
        }

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            using (var form = new StoreManageForm(store, feedCache))
                form.ShowDialog();
        }
        #endregion
    }
}
