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
using System.Threading;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Tasks;
using ZeroInstall.Commands.WinForms.Properties;
using ZeroInstall.Injector;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Uses <see cref="System.Windows.Forms"/> to inform the user about the progress of tasks and ask the user questions.
    /// </summary>
    /// <remarks>
    /// This class is heavily multi-threaded. The UI is prepared in the background with a low priority to allow simultaneous continuation of computation.
    /// Any calls relying on the UI being reading will block automatically.
    /// </remarks>
    public class GuiHandler : MarshalByRefObject, IHandler
    {
        #region Variables
        private ProgressForm _form;

        /// <summary>A barrier that blocks threads until the GUI has been initialized.</summary>
        private readonly EventWaitHandle _guiAvailable = new EventWaitHandle(false, EventResetMode.ManualReset);
        #endregion

        /// <summary>
        /// A short title describing what the command being executed does.
        /// </summary>
        public virtual string ActionTitle { get; set; }

        /// <inheritdoc />
        public bool Batch { get; set; }

        /// <inheritdoc />
        public void RunTask(ITask task, object tag)
        {
            _guiAvailable.WaitOne();

            // Handle events coming from a non-UI thread, don't block caller
            _form.Invoke((SimpleEventHandler)(() => _form.TrackTask(task, tag)));

            task.RunSync();
        }

        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            if (Batch)
            {
                ShowBalloonMessage("Feed signature", "Feed signed with unknown keys!", ToolTipIcon.Warning);
                return false;
            }

            _guiAvailable.WaitOne();

            // Handle events coming from a non-UI thread, block caller until user has answered
            bool result = false;
            _form.Invoke((SimpleEventHandler)(() => result = Msg.Ask(_form, information, MsgSeverity.Info, "Accept\nTrust this new key", "Deny\nReject the key and cancel")));
            return result;
        }

        #region UI control
        /// <inheritdoc/>
        public void ShowProgressUI()
        {
            // Initialize GUI with a low priority
            new Thread(GuiThread) {Priority = ThreadPriority.Lowest}.Start();
        }

        /// <summary>
        /// Runs a message pump for the GUI.
        /// </summary>
        private void GuiThread()
        {
            _form = new ProgressForm();
            if (ActionTitle != null) _form.Text = ActionTitle;
            _form.Initialize();

            // Restore normal priority as soon as the GUI becomes visible
            _form.Shown += delegate { Thread.CurrentThread.Priority = ThreadPriority.Normal; };

            if (Batch) _form.ShowTrayIcon(ActionTitle);
            else _form.Show();

            _guiAvailable.Set();
            Application.Run();
        }

        /// <inheritdoc/>
        public void CloseProgressUI()
        {
            if (_form == null) return;
            _guiAvailable.WaitOne();

            _form.HideTrayIcon();
            Application.Exit();
            _form = null;
            _guiAvailable.Reset();
        }
        #endregion

        /// <inheritdoc />
        public void Output(string title, string information)
        {
            if (Batch) ShowBalloonMessage(title, information, ToolTipIcon.Info);
            else OutputBox.Show(null, title, information);
        }

        private void ShowBalloonMessage(string title, string information, ToolTipIcon type)
        {
            var icon = new NotifyIcon {Visible = true, Icon = Resources.TrayIcon};
            icon.ShowBalloonTip(10000, title, information, type);
        }
    }
}
