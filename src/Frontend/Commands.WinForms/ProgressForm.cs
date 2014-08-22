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
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Services.Solvers;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Preferences;
using ZeroInstall.Store.Model.Selection;

namespace ZeroInstall.Commands.WinForms
{
    /// <summary>
    /// Displays the progress of a <see cref="FrontendCommand"/> and <see cref="ITask"/>s.
    /// </summary>
    public partial class ProgressForm : Form
    {
        #region Variables
        /// <summary>Signaled when the user wishes to cancel the current process.</summary>
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>A short title describing what the command being executed does.</summary>
        private readonly string _actionTitle;

        /// <summary>A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/> after <see cref="ModifySelections"/>.</summary>
        private EventWaitHandle _modifySelectionsWaitHandle;

        /// <summary>Indicates whether <see cref="selectionsControl"/> is intended to be visible or not. Will work even if the form itself is invisible (tray icon mode).</summary>
        private bool _selectionsShown;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new progress tracking form.
        /// </summary>
        /// <param name="cancellationTokenSource">Used to signal when the user wishes to cancel the current process.</param>
        /// <param name="actionTitle">A short title describing what the command being executed does; may be <see langword="null"/>.</param>
        public ProgressForm(CancellationTokenSource cancellationTokenSource, string actionTitle = null)
        {
            #region Sanity checks
            if (cancellationTokenSource == null) throw new ArgumentNullException("cancellationTokenSource");
            #endregion

            _cancellationTokenSource = cancellationTokenSource;

            InitializeComponent();

            labelWorking.Text = _actionTitle = (actionTitle ?? Resources.Working);
            buttonModifySelectionsDone.Text = Resources.Done;
            buttonHide.Text = Resources.Hide;
            buttonCancel.Text = Resources.Cancel;

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            if (Locations.IsPortable) Text += @" - " + Resources.PortableMode;

            Shown += delegate { this.SetForegroundWindow(); };
        }
        #endregion

        //--------------------//

        #region Selections UI
        /// <summary>
        /// Shows the user the <see cref="Selections"/> made by the <see cref="ISolver"/>.
        /// Returns immediately.
        /// </summary>
        /// <param name="selections">The <see cref="Selections"/> as provided by the <see cref="ISolver"/>.</param>
        /// <param name="feedCache">The feed cache used to retrieve feeds for additional information about implementations.</param>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public void ShowSelections(Selections selections, IFeedCache feedCache)
        {
            #region Sanity checks
            if (selections == null) throw new ArgumentNullException("selections");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            trackingControl.Visible = false;
            _selectionsShown = selectionsControl.Visible = true;
            selectionsControl.SetSelections(selections, feedCache);
        }

        /// <summary>
        /// Allows the user to modify the <see cref="InterfacePreferences"/> and rerun the <see cref="ISolver"/> if desired.
        /// Returns immediatley.
        /// </summary>
        /// <param name="solveCallback">Called after <see cref="InterfacePreferences"/> have been changed and the <see cref="ISolver"/> needs to be rerun.</param>
        /// <param name="waitHandle">A wait handle to be signaled once the user is satisfied with the <see cref="Selections"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public void ModifySelections(Func<Selections> solveCallback, EventWaitHandle waitHandle)
        {
            #region Sanity checks
            if (solveCallback == null) throw new ArgumentNullException("solveCallback");
            if (waitHandle == null) throw new ArgumentNullException("waitHandle");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            Visible = true;
            WindowState = FormWindowState.Normal;
            HideTrayIcon();

            _modifySelectionsWaitHandle = waitHandle;

            // Show "modify selections" UI
            selectionsControl.BeginModifySelections(solveCallback);
            buttonModifySelectionsDone.Visible = true;
            buttonModifySelectionsDone.Focus();
        }

        private void buttonModifySelectionsDone_Click(object sender, EventArgs e)
        {
            buttonModifySelectionsDone.Visible = false;
            selectionsControl.EndModifySelections();

            // Signal the waiting thread modification is complete
            if (_modifySelectionsWaitHandle != null)
            {
                _modifySelectionsWaitHandle.Set();
                _modifySelectionsWaitHandle = null;
            }
        }
        #endregion

        #region Task tracking
        /// <summary>
        /// Creates a new GUI element for tracking a <see cref="ITask"/> and returns a handle to it. Should only be one running at a time.
        /// </summary>
        /// <param name="taskName">The name of the task to be tracked.</param>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public IProgress<TaskSnapshot> SetupProgress(string taskName)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException("taskName");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            trackingControl.Visible = true;

            // Hide other stuff
            labelWorking.Visible = progressBarWorking.Visible = false;
            if (_selectionsShown) selectionsControl.Hide();

            trackingControl.TaskName = taskName;
            return new Progress<TaskSnapshot>(trackingControl.Report);
        }

        /// <summary>
        /// Creates a new GUI element for tracking a <see cref="ITask"/> for a specific implementation and returns a handle to it. May run multiple in parallel.
        /// </summary>
        /// <param name="taskName">The name of the task to be tracked.</param>
        /// <param name="tag">A digest used to associate the task with a specific implementation.</param>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public IProgress<TaskSnapshot> SetupProgress(string taskName, ManifestDigest tag)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(taskName)) throw new ArgumentNullException("taskName");
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            // Hide other stuff
            trackingControl.Hide();

            if (_selectionsShown)
            {
                var control = selectionsControl.TrackingControls[tag];
                control.TaskName = taskName;
                return new Progress<TaskSnapshot>(control.Report);
            }
            else return SetupProgress(taskName);
        }

        /// <summary>
        /// Restores the UI activated by <see cref="ShowSelections"/> and hidden by <see cref="SetupProgress(string)"/> after an <see cref="ITask.Run"/> completes.
        /// </summary>
        public void RestoreSelections()
        {
            if (_selectionsShown)
            {
                trackingControl.Visible = false;
                selectionsControl.Visible = true;
            }
        }
        #endregion

        #region Tray icon
        /// <summary>
        /// Shows the tray icon with an associated balloon message.
        /// </summary>
        /// <param name="information">The balloon message text.</param>
        /// <param name="messageType">The type icon to display next to the balloon message.</param>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public void ShowTrayIcon(string information, ToolTipIcon messageType)
        {
            #region Sanity checks
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

            notifyIcon.Visible = true;
            if (!string.IsNullOrEmpty(information)) notifyIcon.ShowBalloonTip(7500, "Zero Install", information, messageType);
        }

        /// <summary>
        /// Hides the tray icon.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        /// <remarks>This method must not be called from a background thread.</remarks>
        public void HideTrayIcon()
        {
            #region Sanity checks
            if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
            #endregion

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
            ShowTrayIcon(_actionTitle, ToolTipIcon.None);
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
        /// Hides the window and then starts canceling the current process asynchronously.
        /// </summary>
        private void Cancel()
        {
            Hide();
            HideTrayIcon();

            _cancellationTokenSource.Cancel();

            // Unblock any waiting thread
            if (_modifySelectionsWaitHandle != null) _modifySelectionsWaitHandle.Set();
        }
        #endregion
    }
}
