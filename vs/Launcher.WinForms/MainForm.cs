/*
 * Copyright 2010 Bastian Eicher
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

namespace ZeroInstall.Launcher.WinForms
{
    /// <summary>
    /// Uses GUI message boxes to ask the user questions.
    /// </summary>
    public partial class MainForm : Form, IHandler
    {
        #region Properties
        /// <summary>
        /// Silently answer all questions with "No".
        /// </summary>
        public bool Batch { get; set; }
        #endregion

        //--------------------//

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
                Dispose();
            }));
        }

        /// <summary>
        /// Displays an error messages in a dialog synchronous to the main GUI.
        /// </summary>
        /// <param name="message">The error message to be displayed.</param>
        public void ReportErrorAsync(string message)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, block caller until user has answered
            Invoke((SimpleEventHandler)(delegate
            {
                Msg.Inform(this, message, MsgSeverity.Error);

                buttonCancel.Enabled = true;
                Close();
            }));
        }
        #endregion

        #region Handler
        /// <inheritdoc />
        public bool AcceptNewKey(string information)
        {
            if (Batch) return false;

            bool result = false;

            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, block caller until user has answered
            Invoke((SimpleEventHandler)(() => result = Msg.Ask(this, information, MsgSeverity.Information, "Accept\nTrust this new key", "Deny\nReject the key and cancel")));

            return result;
        }

        /// <inheritdoc />
        public void StartingDownload(IProgress download)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = download.Name + @"...";
                progressBar.Task = download;
                buttonCancel.Enabled = true;
            });
        }

        /// <inheritdoc />
        public void StartingExtraction(IProgress extraction)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = extraction.Name + @"...";
                progressBar.Task = extraction;
                buttonCancel.Enabled = true;
            });
        }

        /// <inheritdoc />
        public void StartingManifest(IProgress manifest)
        {
            // Wait until the GUI is actually up and running
            while (!IsHandleCreated) Thread.Sleep(0);

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                labelOperation.Text = manifest.Name + @"...";
                progressBar.Task = manifest;
                buttonCancel.Enabled = true;
            });
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
