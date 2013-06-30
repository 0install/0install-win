/*
 * Copyright 2010-2013 Bastian Eicher
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
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Tasks;
using ZeroInstall.Backend;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Model;
using ZeroInstall.Model.Selection;
using ZeroInstall.Store;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Uses <see cref="System.Windows.Forms"/> to inform the user about the progress of tasks and ask the user questions.
    /// </summary>
    /// <remarks>This class manages a GUI thread with an independent message queue. Invoking methods on the right thread is handled automatically.</remarks>
    public sealed class GuiHandler : MarshalByRefObject, IBackendHandler, IDisposable
    {
        #region Variables
        private ProgressForm _form;

        /// <summary>Synchronization object used to prevent multiple concurrent generic <see cref="ITask"/>s.</summary>
        private readonly object _genericTaskLock = new object();

        /// <summary>A wait handle used by <see cref="AuditSelections"/> to be signaled once the user is satisfied with the <see cref="Selections"/>.</summary>
        private readonly AutoResetEvent _auditWaitHandle = new AutoResetEvent(false);
        #endregion

        #region Properties
        private readonly CancellationToken _cancellationToken = new CancellationToken();

        /// <inheritdoc/>
        public CancellationToken CancellationToken { get { return _cancellationToken; } }

        /// <inheritdoc />
        public int Verbosity { get; set; }

        /// <inheritdoc />
        public bool Batch { get; set; }

        private string _actionTitle;

        /// <inheritdoc />
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
        /// Creates a new GUI handler with an external <see cref="CancellationToken"/>.
        /// </summary>
        public GuiHandler(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Creates a new GUI handler with its own <see cref="CancellationToken"/>.
        /// </summary>
        public GuiHandler() : this(new CancellationToken())
        {}
        #endregion

        #region Dispose
        /// <inheritdoc/>
        public void Dispose()
        {
            _auditWaitHandle.Close();
            if (_form != null) _form.Dispose();
        }
        #endregion

        //--------------------//

        #region Task tracking
        /// <inheritdoc />
        public void RunTask(ITask task, object tag = null)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            if (tag is ManifestDigest)
            {
                // Handle events coming from a non-UI thread
                _form.Invoke(new Action(() => _form.TrackTask(task, (ManifestDigest)tag)));
            }
            else
            {
                lock (_genericTaskLock) // Prevent multiple concurrent generic tasks
                {
                    // Handle events coming from a non-UI thread
                    _form.Invoke(new Action(() => _form.TrackTask(task)));
                }
            }

            task.RunSync(_cancellationToken);
        }
        #endregion

        #region UI control
        /// <inheritdoc/>
        public void ShowProgressUI()
        {
            // Can only show GUI once
            if (_form != null) return;

            _form = new ProgressForm(delegate
            { // Cancel callback
                _cancellationToken.RequestCancellation();
                _auditWaitHandle.Set();
            }, _actionTitle);

            // Start GUI thread
            using (var guiReady = new ManualResetEvent(false))
            {
                var thread = new Thread(() =>
                {
                    _form.Initialize();

                    // Show the tray icon or the form
                    if (Batch) _form.ShowTrayIcon(_actionTitle, ToolTipIcon.None);
                    else _form.Show();
                    // ReSharper disable AccessToDisposedClosure
                    guiReady.Set(); // Signal that the GUI handles have been created
                    // ReSharper restore AccessToDisposedClosure

                    Application.Run();
                });
                thread.SetApartmentState(ApartmentState.STA); // Make COM work
                thread.Start();
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
        /// <inheritdoc />
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

        /// <inheritdoc/>
        public void AuditSelections(Func<Selections> solveCallback)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            #endregion

            if (_form == null) return;

            // Show selection auditing screen and then asynchronously wait until its done
            _form.Invoke(new Action(() =>
            {
                // Leave tray icon mode
                _form.Show();
                Application.DoEvents();

                _form.BeginAuditSelections(solveCallback, _auditWaitHandle);
            }));
            _auditWaitHandle.WaitOne();
        }
        #endregion

        #region Messages
        /// <inheritdoc />
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
        public void ShowIntegrateApp(IIntegrationManager integrationManager, AppEntry appEntry, Feed feed)
        {
            #region Sanity checks
            if (integrationManager == null) throw new ArgumentNullException("integrationManager");
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            if (_form == null) return;

            var integrationForm = new IntegrateAppForm(integrationManager, appEntry, feed);
            integrationForm.VisibleChanged += delegate
            { // The IntegrateAppForm and ProgressForm take turns in being visible
                if (integrationForm.Visible || (integrationForm.DialogResult == DialogResult.Cancel))
                {
                    _form.Invoke(new Action(() =>
                    {
                        _form.Visible = false;
                        _form.HideTrayIcon();
                    }));
                }
                else _form.Invoke(new Action(_form.Show));
            };
            integrationForm.ShowDialog();
        }

        /// <inheritdoc/>
        public bool ShowConfig(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            return ConfigForm.Edit(config);
        }
        #endregion
    }
}
