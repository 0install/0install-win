/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Tasks;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.Injector;
using ZeroInstall.Injector.Solver;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Uses <see cref="System.Windows.Forms"/> to inform the user about the progress of tasks and ask the user questions.
    /// </summary>
    /// <remarks>
    /// This class is heavily multi-threaded. The UI is prepared in the background with a low priority to allow simultaneous continuation of computation.
    /// Any calls relying on the UI being reading will block automatically.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Disposal is handled sufficiently by GC in this case")]
    public class GuiHandler : MarshalByRefObject, IHandler
    {
        #region Variables
        private ProgressForm _form;

        /// <summary>A barrier that blocks threads until the <see cref="_form"/>'s handle is ready.</summary>
        private readonly EventWaitHandle _guiReady = new EventWaitHandle(false, EventResetMode.ManualReset);
        #endregion

        #region Properties
        /// <summary>
        /// A short title describing what the command being executed does.
        /// </summary>
        public virtual string ActionTitle { get; set; }

        /// <inheritdoc />
        public bool Batch { get; set; }
        #endregion

        //--------------------//

        #region Task tracking
        /// <inheritdoc />
        public void RunTask(ITask task, object tag)
        {
            _guiReady.WaitOne();

            // Handle events coming from a non-UI thread, don't block caller
            _form.BeginInvoke(new SimpleEventHandler(() => _form.TrackTask(task, tag)));

            task.RunSync();
        }
        #endregion

        #region UI control
        /// <inheritdoc/>
        public void ShowProgressUI(SimpleEventHandler cancelCallback)
        {
            // Can only show GUI once
            if (_form != null) return;

            _form = new ProgressForm(cancelCallback);

            // Initialize GUI with a low priority
            new Thread(GuiThread) {Priority = ThreadPriority.Lowest}.Start();
        }

        /// <summary>
        /// Runs a message pump for the GUI.
        /// </summary>
        private void GuiThread()
        {
            _form.Initialize();
            if (ActionTitle != null) _form.Text = ActionTitle;

            // Restore normal priority as soon as the GUI becomes visible
            _form.Shown += delegate { Thread.CurrentThread.Priority = ThreadPriority.Normal; };

            // Show the tray icon or the form
            if (Batch) _form.ShowTrayIcon(ActionTitle, ToolTipIcon.None);
            else _form.Show();

            // Start the message loop and set the wait handle as soon as it is running
            Application.Idle += delegate { _guiReady.Set(); };
            Application.Run();
        }

        /// <inheritdoc/>
        public void CloseProgressUI()
        {
            // If GUI doesn't even exist cancel, otherwise wait until it's ready
            if (_form == null) return;
            _guiReady.WaitOne();

            _guiReady.Reset();
            _form.HideTrayIcon();
            _form = null;
            Application.Exit();
        }
        #endregion

        #region Key control
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(information)) throw new ArgumentNullException("information");
            #endregion

            // If GUI doesn't even exist cancel, otherwise wait until it's ready
            if (_form == null) return false;
            _guiReady.WaitOne();

            // Handle events coming from a non-UI thread, block caller until user has answered
            bool result = false;
            _form.Invoke(new SimpleEventHandler(delegate
            {
                // Auto-deny unknown keys and inform via tray icon when in batch mode
                if (Batch) _form.ShowTrayIcon("Feed signed with unknown keys!", ToolTipIcon.Warning);
                else result = Msg.Ask(_form, information, MsgSeverity.Info, "Accept\nTrust this new key", "Deny\nReject the key and cancel");
            }));
            return result;
        }
        #endregion

        #region Selections UI
        /// <inheritdoc/>
        public void ShowSelections(Selections selections)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            #endregion

            // If GUI doesn't even exist cancel, otherwise wait until it's ready
            if (_form == null) return;
            _guiReady.WaitOne();

            _form.Invoke(new SimpleEventHandler(delegate
            {
                // ToDo: Implement
            }));
        }

        /// <inheritdoc/>
        public void AuditSelections(SimpleResult<Selections> solveCallback)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            #endregion

            // If GUI doesn't even exist cancel, otherwise wait until it's ready
            if (_form == null) return;
            _guiReady.WaitOne();

            _form.Invoke(new SimpleEventHandler(delegate
            {
                // ToDo: Implement
            }));
        }
        #endregion

        #region Messages
        /// <inheritdoc />
        public void Output(string title, string information)
        {
            if (Batch) ShowBalloonMessage(title, information);
            else OutputBox.Show(null, title, information);
        }

        /// <summary>
        /// Displays a tray icon with balloon message detached from the main GUI (will stick around even after the process ends).
        /// </summary>
        /// <param name="title">The title of the balloon message.</param>
        /// <param name="information">The balloon message text.</param>
        private static void ShowBalloonMessage(string title, string information)
        {
            var icon = new NotifyIcon {Visible = true, Icon = Resources.TrayIcon};
            icon.ShowBalloonTip(10000, title, information, ToolTipIcon.Info);
        }
        #endregion

        #region Configuration
        /// <inheritdoc/>
        public bool ShowConfig(Config config)
        {
            #region Sanity checks
            if (config == null) throw new ArgumentNullException("config");
            #endregion

            return (ConfigForm.ShowDialog(config) == DialogResult.OK);
        }
        #endregion
    }
}
