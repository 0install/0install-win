/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.CliCommands;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.DesktopIntegration.ViewModel;
using ZeroInstall.Services.Feeds;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Uses <see cref="System.Windows.Forms"/> to allow users to interact with <see cref="CliCommand"/>s.
    /// </summary>
    /// <remarks>This class manages a GUI thread with an independent message queue. Invoking methods on the right thread is handled automatically.</remarks>
    public sealed class GuiCommandHandler : GuiTaskHandler, ICommandHandler
    {
        #region Resources
        private readonly AsyncFormWrapper<ProgressForm> _wrapper;

        /// <summary>A wait handle used by <see cref="CustomizeSelections"/> to be signaled once the user is satisfied with the <see cref="Selections"/>.</summary>
        private readonly AutoResetEvent _modifySelectionsWaitHandle = new AutoResetEvent(false);

        public GuiCommandHandler()
        {
            _wrapper = new AsyncFormWrapper<ProgressForm>(delegate
            {
                var form = new ProgressForm(CancellationTokenSource);
                if (Background) form.ShowTrayIcon();
                else form.Show();
                return form;
            });
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _modifySelectionsWaitHandle.Close();
                    _wrapper.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion

        /// <inheritdoc/>
        public bool Background { get; set; }

        #region Task tracking
        /// <inheritdoc/>
        public override void RunTask(ITask task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            Log.Debug("Task: " + task.Name);

            var progress = _wrapper.Post(form => (task.Tag is ManifestDigest)
                ? form.GetProgressControl(task.Name, (ManifestDigest)task.Tag)
                : form.GetProgressControl(task.Name));

            task.Run(CancellationToken, CredentialProvider, progress);

            _wrapper.Post(form => form.RestoreSelections());
        }
        #endregion

        #region UI control
        private bool _disabled;

        /// <inheritdoc/>
        public void DisableUI()
        {
            _disabled = true;
            _wrapper.SendLow(x => x.Enabled = false);
        }

        /// <inheritdoc/>
        public void CloseUI()
        {
            _wrapper.Close();
        }
        #endregion

        #region Question
        /// <inheritdoc/>
        public override bool Ask(string question)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(question)) throw new ArgumentNullException("question");
            #endregion

            Log.Debug("Question: " + question);
            using (var future = _wrapper.Post(form => form.Ask(question)))
            {
                switch (future.Get())
                {
                    case DialogResult.Yes:
                        Log.Debug("Answer: Yes");
                        return true;
                    case DialogResult.No:
                        Log.Debug("Answer: No");
                        return false;
                    case DialogResult.Cancel:
                    default:
                        Log.Debug("Answer: Cancel");
                        throw new OperationCanceledException();
                }
            }
        }
        #endregion

        #region Selections UI
        /// <inheritdoc/>
        public void ShowSelections(Selections selections, IFeedManager feedManager)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (feedManager == null) throw new ArgumentNullException("feedManager");
            #endregion

            _wrapper.Post(form => form.ShowSelections(selections, feedManager));
        }

        /// <inheritdoc/>
        public void CustomizeSelections(Func<Selections> solveCallback)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            #endregion

            // Show "modify selections" screen and then asynchronously wait until it's done
            _wrapper.Post(form =>
            {
                // Leave tray icon mode
                form.Show();
                Application.DoEvents();

                form.ModifySelections(solveCallback, _modifySelectionsWaitHandle);
            });

            _modifySelectionsWaitHandle.WaitOne();
        }
        #endregion

        #region Output
        /// <inheritdoc/>
        public override void Output(string title, string message)
        {
            DisableUI();

            if (Background) OutputBalloon(title, message);
            else base.Output(title, message);
        }

        /// <inheritdoc/>
        public override void Output<T>(string title, IEnumerable<T> data)
        {
            DisableUI();

            if (Background)
            {
                string message = StringUtils.Join(Environment.NewLine, data.Select(x => x.ToString()));
                OutputBalloon(title, message);
            }
            else base.Output(title, data);
        }

        /// <summary>
        /// Displays a tray icon with balloon message detached from the main GUI. Will stick around even after the process ends until the user mouses over.
        /// </summary>
        /// <param name="title">The title of the balloon message.</param>
        /// <param name="message">The balloon message text.</param>
        private static void OutputBalloon(string title, string message)
        {
            var icon = new NotifyIcon {Visible = true, Icon = Resources.TrayIcon};
            icon.ShowBalloonTip(10000, title, message, ToolTipIcon.Info);
        }
        #endregion

        #region Dialogs
        /// <inheritdoc/>
        public void ShowIntegrateApp(IntegrationState state)
        {
            #region Sanity checks
            if (state == null) throw new ArgumentNullException("state");
            #endregion

            var result = _wrapper.Post(form =>
            {
                var integrationForm = new IntegrateAppForm(state);

                // The progress form and integration form take turns in being visible
                form.Hide();

                return integrationForm.ShowDialog();
            });

            if (result == DialogResult.OK) _wrapper.Post(form => form.Show());
            else throw new OperationCanceledException();
        }

        /// <inheritdoc/>
        public void ShowFeedSearch(SearchQuery query)
        {
            #region Sanity checks
            if (query == null) throw new ArgumentNullException("query");
            #endregion

            ThreadUtils.RunSta(() =>
            {
                using (var dialog = new FeedSearchDialog(query))
                    dialog.ShowDialog();
            });
        }

        /// <inheritdoc/>
        public void ShowConfig(Config config, ConfigTab configTab)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            ThreadUtils.RunSta(() =>
            {
                using (var dialog = new ConfigDialog(config))
                {
                    dialog.SelectTab(configTab);
                    if (dialog.ShowDialog() != DialogResult.OK) throw new OperationCanceledException();
                }
            });
        }

        /// <inheritdoc/>
        public void ManageStore(IStore store, IFeedCache feedCache)
        {
            #region Sanity checks
            if (store == null) throw new ArgumentNullException("store");
            if (feedCache == null) throw new ArgumentNullException("feedCache");
            #endregion

            ThreadUtils.RunSta(() =>
            {
                using (var form = new StoreManageForm(store, feedCache))
                    form.ShowDialog();
            });
        }
        #endregion

        #region Log handler
        /// <summary>
        /// Outputs <see cref="Log"/> messages as balloon tips based on their <see cref="LogSeverity"/> and the current <see cref="Verbosity"/> level.
        /// </summary>
        /// <param name="severity">The type/severity of the entry.</param>
        /// <param name="message">The message text of the entry.</param>
        protected override void LogHandler(LogSeverity severity, string message)
        {
            if (_disabled) return;

            base.LogHandler(severity, message);

            switch (severity)
            {
                case LogSeverity.Debug:
                    if (Verbosity >= Verbosity.Debug) _wrapper.SendLow(form => form.ShowTrayIcon(message));
                    break;
                case LogSeverity.Info:
                    if (Verbosity >= Verbosity.Verbose) _wrapper.SendLow(form => form.ShowTrayIcon(message, ToolTipIcon.Info));
                    break;
                case LogSeverity.Warn:
                    _wrapper.SendLow(form => form.ShowTrayIcon(message, ToolTipIcon.Warning));
                    break;
                case LogSeverity.Error:
                    _wrapper.SendLow(form => form.ShowTrayIcon(message, ToolTipIcon.Error));
                    break;
            }
        }
        #endregion
    }
}
