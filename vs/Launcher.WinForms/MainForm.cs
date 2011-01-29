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
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using Common;
using ZeroInstall.Injector;

namespace ZeroInstall.Launcher.WinForms
{
    /// <summary>
    /// Uses GUI message boxes to ask the user questions.
    /// </summary>
    public partial class MainForm : Form, IHandler
    {
        #region Async control
        /// <summary>
        /// Starts the GUI in a separate thread.
        /// </summary>
        public void ShowAsync()
        {
            // Initialize GUI with a low priority but restore normal priority as soon as it becomes visible
            new Thread(delegate()
            {
                InitializeComponent();
                Shown += delegate { Thread.CurrentThread.Priority = ThreadPriority.Normal; };
                Application.Run(this);
            }) {Priority = ThreadPriority.Lowest}.Start();
        }

        /// <summary>
        /// Begins closing the window running in the GUI thread. Does not wait for the window to finish closing.
        /// </summary>
        public void CloseAsync()
        {
            if (IsDisposed) return;

            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);
            
            // Handle events coming from a non-UI thread, block caller until user has answered
            Invoke((SimpleEventHandler)(delegate
            {
                buttonCancel.Enabled = true;
                Close();
            }));
        }
        #endregion

        #region Handler
        /// <summary>
        /// Silently answer all questions with "No".
        /// </summary>
        public bool Batch { get; set; }

        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            if (Batch) return false;

            bool result = false;

            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, block caller until user has answered
            Invoke((SimpleEventHandler)(() => result = Msg.Ask(this, information, MsgSeverity.Info, "Accept\nTrust this new key", "Deny\nReject the key and cancel")));

            return result;
        }

        /// <inheritdoc />
        public void RunDownloadTask(ITask task)
        {
            HookupTracking(task);
        }

        /// <inheritdoc />
        public void RunIOTask(ITask task)
        {
            HookupTracking(task);
        }

        /// <summary>
        /// Hooks up a new task with the GUI for tracking.
        /// </summary>
        private void HookupTracking(ITask task)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                //Thread.Sleep(1000);
                labelOperation.Text = task.Name + @"...";
                progressBar.Task = task;
                labelProgress.Task = task;
                buttonCancel.Enabled = true;
            });

            task.RunSync();
        }
        #endregion

        #region Event handling
        private void MainForm_Closing(object sender, CancelEventArgs e)
        {
            if (!buttonCancel.Enabled)
            {
                e.Cancel = true;
                return;
            }

            // Hide UI immediately so user doesn't notice anything freezing
            Visible = false;

            // Cancel any tasks that may still be running
            if (progressBar.Task != null) progressBar.Task.Cancel();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion
    }
}
