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
using System.Windows.Forms;
using Common.Download;
using Common.Helpers;

namespace Common.Controls
{
    /// <summary>
    /// A progress bar that tracks the progress of a <see cref="DownloadFile"/>.
    /// </summary>
    public partial class DownloadProgressBar : UserControl
    {
        #region Variables
        /// <summary>The download file to show the download progress by <see cref="progressBar"/>.</summary>
        private DownloadFile _downloadFile;
        #endregion

        #region Properties
        /// <summary>
        /// The download file to show the download progress by <see cref="progressBar"/>.
        /// </summary>
        public DownloadFile Download
        {
            set
            {
                // Remove all delegates from old _downloadFile
                if (_downloadFile != null)
                {
                    _downloadFile.StateChanged -= DownloadStateChanged;
                    _downloadFile.StateChanged -= DownloadBytesRecivedChanged;
                }

                _downloadFile = value;

                if (value != null)
                {
                    // Set delegates to the new _downloadFile
                    _downloadFile.StateChanged += DownloadStateChanged;
                }
            }
            get { return _downloadFile; }
        }

        /// <summary>
        /// Show the download progress in the Windows taskbar.
        /// </summary>
        /// <remarks>Use only once per window. Only used in Windows 7 or newer.</remarks>
        public bool UseTaskbar { set; get; }
        #endregion

        #region Constructor
        public DownloadProgressBar()
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
        /// Changes the <see cref="ProgressBarStyle"/> of <see cref="progressBar"/> and the taskbar depending on the <see cref="DownloadState"/> of <see cref="_downloadFile"/>.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <remarks>Taskbar only changes for Windows 7 or newer.</remarks>
        private void DownloadStateChanged(DownloadFile sender)
        {
            // Handle events coming from a non-UI thread
            progressBar.Invoke((SimpleEventHandler) delegate
            {
                IntPtr formHandle = GetFormHandle();
                switch (_downloadFile.State)
                {
                    case DownloadState.Ready:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Paused, formHandle);
                        break;

                    case DownloadState.GettingHeaders:
                        progressBar.Style = ProgressBarStyle.Marquee;
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Indeterminate, formHandle);
                        break;

                    case DownloadState.GettingData:
                        // Is the final download size known?
                        if (sender.BytesTotal > 0)
                        {
                            _downloadFile.BytesReceivedChanged += DownloadBytesRecivedChanged;
                            progressBar.Style = ProgressBarStyle.Continuous;
                            if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Normal, formHandle);
                        }
                        break;

                    case DownloadState.IOError:
                    case DownloadState.WebError:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Error, formHandle);
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Value = 0;
                        break;

                    case DownloadState.Complete:
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
        private void DownloadBytesRecivedChanged(DownloadFile sender)
        {
            int currentValue = (int)(_downloadFile.Progress * 100);

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
