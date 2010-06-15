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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Common.Download;
using Common.Helpers;

namespace ZeroInstall.Publish.WinForms
{
    public partial class DownloadProgressBar : UserControl
    {
        #region Attributes

        /// <summary>
        /// The download file to show the download progress by <see cref="progressBar"/>.
        /// </summary>
        private DownloadFile _downloadFile;

        #endregion

        #region Proporties

        /// <summary>
        /// The download file to show the download progress by <see cref="progressBar"/>.
        /// </summary>
        public DownloadFile Download
        {
            set {
                // remove all delegates from old _downloadFile
                if (_downloadFile != null)
                {
                    _downloadFile.StateChanged -= DownloadStateChanged;
                    _downloadFile.StateChanged -= DownloadBytesRecivedChanged;
                }

                _downloadFile = value;

                if (value != null)
                {
                    // set delegates to the new _downloadFile
                    _downloadFile.StateChanged += DownloadStateChanged;
                }
            }
            get { return _downloadFile; }
        }

        /// <summary>
        /// Show the download progress in the Windows taskbar.
        /// </summary>
        /// <remarks>Use only once per window. <br/>
        /// Only used in Windows 7 or newer.
        /// </remarks>
        public bool UseTaskbar { set; get; }

        #endregion

        public DownloadProgressBar()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Changes the <see cref="ProgressBarStyle"/> of <see cref="progressBar"/> and the Taskbar depending on the <see cref="DownloadState"/> of <see cref="_downloadFile"/>
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <remarks>Taskbar only changes for Windows 7 or newer.</remarks>
        private void DownloadStateChanged(DownloadFile sender) {
            progressBar.Invoke((SimpleEventHandler) delegate
            {
                switch (_downloadFile.State)
                {
                    case DownloadState.Ready:
                        if (UseTaskbar)
                            WindowsHelper.SetProgressState(TaskbarProgressBarState.Paused, FindForm().Handle);
                        break;
                    case DownloadState.GettingData:
                        {
                            // is download size available?
                            if (sender.BytesTotal == -1)
                            {
                                progressBar.Style = ProgressBarStyle.Marquee;
                                if (UseTaskbar)
                                    WindowsHelper.SetProgressState(TaskbarProgressBarState.Indeterminate, FindForm().Handle);
                            }
                            else
                            {
                                _downloadFile.BytesReceivedChanged += DownloadBytesRecivedChanged;
                                progressBar.Style = ProgressBarStyle.Continuous;
                                if (UseTaskbar)
                                    WindowsHelper.SetProgressState(TaskbarProgressBarState.Normal, FindForm().Handle);
                            }
                        }
                        break;
                    case DownloadState.IOError:
                    case DownloadState.WebError:
                        if(UseTaskbar)
                            WindowsHelper.SetProgressState(TaskbarProgressBarState.Error, FindForm().Handle);
                        break;
                    case DownloadState.Complete:
                        if (UseTaskbar)
                            WindowsHelper.SetProgressState(TaskbarProgressBarState.NoProgress, FindForm().Handle);
                        break;

                    default: return;
                }
            });
        }

        /// <summary>
        /// Changes the value of the <see cref="progressBar"/> and the Taskbar depending on the allready downloaded bytes.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <remarks>Taskbar only changes for Windows 7 or newer.</remarks>
        private void DownloadBytesRecivedChanged(DownloadFile sender)
        {
            int currentValue = (int)(_downloadFile.Progress * 100f);
            progressBar.Invoke((SimpleEventHandler) delegate {
                progressBar.Value = currentValue;
                WindowsHelper.SetProgressValue(currentValue, 100, FindForm().Handle);
            });
        }
    }
}
