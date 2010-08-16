/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using Common.Helpers;

namespace Common.Controls
{
    /// <summary>
    /// A progress bar that automatically tracks the progress of an <see cref="IProgress"/> object.
    /// </summary>
    public partial class TrackingProgressBar : UserControl
    {
        #region Properties
        private IProgress _task;
        /// <summary>
        /// The <see cref="IProgress"/> object to track.
        /// </summary>
        [Description("The IProgress object to track.")]
        public IProgress Task
        {
            set
            {
                // Remove all delegates from old _downloadFile
                if (_task != null)
                {
                    _task.StateChanged -= DownloadStateChanged;
                    _task.StateChanged -= DownloadBytesRecivedChanged;
                }

                _task = value;

                if (value != null)
                {
                    // Set delegates to the new _downloadFile
                    _task.StateChanged += DownloadStateChanged;
                }
            }
            get { return _task; }
        }

        /// <summary>
        /// Show the progress in the Windows taskbar.
        /// </summary>
        /// <remarks>Use only once per window. Only works on Windows 7 or newer.</remarks>
        [Description("Show the progress in the Windows taskbar.")]
        public bool UseTaskbar { set; get; }
        #endregion

        #region Constructor
        public TrackingProgressBar()
        {
            InitializeComponent();
        }
        #endregion

        //--------------------//

        /// <summary>
        /// Determines the handle of the <see cref="Form"/> containing this control.
        /// </summary>
        /// <returns>The handle of the parent <see cref="Form"/> or <see cref="IntPtr.Zero"/> if there is no parent.</returns>
        private IntPtr GetFormHandle()
        {
            Form parent = FindForm();
            return (parent == null ? IntPtr.Zero : parent.Handle);
        }

        #region Event callbacks
        /// <summary>
        /// Changes the <see cref="ProgressBarStyle"/> of <see cref="progressBar"/> and the taskbar depending on the <see cref="ProgressState"/> of <see cref="_task"/>.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <remarks>Taskbar only changes for Windows 7 or newer.</remarks>
        private void DownloadStateChanged(IProgress sender)
        {
            // Handle events coming from a non-UI thread
            progressBar.Invoke((SimpleEventHandler) delegate
            {
                IntPtr formHandle = GetFormHandle();
                switch (_task.State)
                {
                    case ProgressState.Ready:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Paused, formHandle);
                        break;

                    case ProgressState.GettingHeaders:
                        progressBar.Style = ProgressBarStyle.Marquee;
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Indeterminate, formHandle);
                        break;

                    case ProgressState.GettingData:
                        // Is the final size known?
                        if (sender.BytesTotal > 0)
                        {
                            _task.BytesReceivedChanged += DownloadBytesRecivedChanged;
                            progressBar.Style = ProgressBarStyle.Continuous;
                            if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Normal, formHandle);
                        }
                        break;

                    case ProgressState.IOError:
                    case ProgressState.WebError:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Error, formHandle);
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Value = 0;
                        break;

                    case ProgressState.Complete:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.NoProgress, formHandle);
                        break;
                }
            });
        }

        /// <summary>
        /// Changes the value of the <see cref="progressBar"/> and the taskbar depending on the already downloaded bytes.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <remarks>Taskbar only changes for Windows 7 or newer.</remarks>
        private void DownloadBytesRecivedChanged(IProgress sender)
        {
            int currentValue = (int)(_task.Progress * 100);

            // Handle events coming from a non-UI thread
            progressBar.Invoke((SimpleEventHandler)delegate
            {
                progressBar.Value = currentValue;
                IntPtr formHandle = GetFormHandle();
                if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressValue(currentValue, 100, formHandle);
            });
        }
        #endregion
    }
}
