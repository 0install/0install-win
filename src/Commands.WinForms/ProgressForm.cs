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
using System.Windows.Forms;
using Common;
using Common.Storage;
using Common.Tasks;
using Common.Utils;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the progress of a <see cref="CommandBase"/> and <see cref="ITask"/>s.
    /// </summary>
    public partial class ProgressForm : Form
    {
        #region Variables
        /// <summary>To be called when the user wishes to cancel the current process.</summary>
        private readonly SimpleEventHandler _cancelCallback;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new progress tracking form.
        /// </summary>
        /// <param name="cancelCallback">To be called when the user wishes to cancel the current process.</param>
        public ProgressForm(SimpleEventHandler cancelCallback)
        {
            #region Sanity checks
            if (cancelCallback == null) throw new ArgumentNullException("cancelCallback");
            #endregion

            _cancelCallback = cancelCallback;

            // Start tracking when the window comes up the first time from tray icon mode
            Shown += delegate
            {
                SetupTaskTracking();
                WindowsUtils.SetForegroundWindow(this);
            };
        }

        /// <summary>
        /// Initializes the form and all its child controls without displaying it yet.
        /// </summary>
        public void Initialize()
        {
            InitializeComponent();
            CreateHandle();
            CreateControl();
        }
        #endregion

        //--------------------//

        #region Task tracking
        private ITask _currentTask;

        /// <summary>
        /// Registers an <see cref="ITask"/> for tracking.
        /// </summary>
        /// <param name="task">The task to be tracked. May or may not alreay be running.</param>
        /// <param name="tag">An object used to associate the <paramref name="task"/> with a specific process; may be <see langword="null"/>.</param>
        internal void TrackTask(ITask task, object tag)
        {
            _currentTask = task;
            SetupTaskTracking();
        }

        /// <summary>
        /// Helper method for setting up task tracking for <see cref="_currentTask"/>.
        /// </summary>
        private void SetupTaskTracking()
        {
            if (_currentTask == null) return;

            labelOperation.Text = _currentTask.Name + @"...";
            if (progressBar.IsHandleCreated) progressBar.Task = _currentTask;
            if (progressLabel.IsHandleCreated) progressLabel.Task = _currentTask;
        }
        #endregion

        #region Tray icon
        /// <summary>
        /// Shows the tray icon with an associated balloon message.
        /// </summary>
        /// <param name="information">The balloon message text.</param>
        /// <param name="messageType">The type icon to display next to the balloon message.</param>
        public void ShowTrayIcon(string information, ToolTipIcon messageType)
        {
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(7500, "Zero Install", information, messageType);
        }

        /// <summary>
        /// Hides the tray icon.
        /// </summary>
        public void HideTrayIcon()
        {
            notifyIcon.Visible = false;
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            Visible = true;
            WindowState = FormWindowState.Normal;
            HideTrayIcon();
        }

        private void buttonHide_Click(object sender, EventArgs e)
        {
            ShowTrayIcon(Text, ToolTipIcon.None);
            Visible = false;
        }
        #endregion

        #region Closing
        // Raised when the user tried to close the window
        private void ProgressForm_Closing(object sender, CancelEventArgs e)
        {
            // Never allow the user to directly close the window
            e.Cancel = true;

            // Start proper cancellation instead
            Cancel();
        }
        #endregion

        #region Cancelling
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        /// <summary>
        /// Hides the window and then runs <see cref="_cancelCallback"/>.
        /// </summary>
        private void Cancel()
        {
            Hide();
            HideTrayIcon();

            // Stop tracking tasks
            if (progressBar.IsHandleCreated) progressBar.Task = null;
            if (progressLabel.IsHandleCreated) progressLabel.Task = null;

            _cancelCallback();
        }
        #endregion
    }
}
