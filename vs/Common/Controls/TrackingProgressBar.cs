/*
 * Copyright 2006-2010 Simon E. Silva Lauinger, Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
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
        /// <remarks>
        /// Setting this property will hook up event handlers to monitor the task.
        /// Remember to set it back to <see langword="null"/> or to call <see cref="Dispose"/> when done, to remove the event handlers again.
        /// </remarks>
        [DefaultValue(null), Description("The IProgress object to track.")]
        public IProgress Task
        {
            set
            {
                // Remove all delegates from old _task
                if (_task != null)
                {
                    _task.StateChanged -= StateChanged;
                    _task.ProgressChanged -= ProgressChanged;
                }

                _task = value;

                if (value != null)
                {
                    // Set delegates to the new _task
                    _task.StateChanged += StateChanged;
                }
            }
            get { return _task; }
        }

        /// <summary>
        /// Show the progress in the Windows taskbar.
        /// </summary>
        /// <remarks>Use only once per window. Only works on Windows 7 or newer.</remarks>
        [Description("Show the progress in the Windows taskbar."), DefaultValue(false)]
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
        private void StateChanged(IProgress sender)
        {
            // Handle events coming from a non-UI thread, block caller so we can safely retreive properties
            progressBar.Invoke((SimpleEventHandler) delegate
            {
                IntPtr formHandle = GetFormHandle();
                switch (_task.State)
                {
                    case ProgressState.Ready:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Paused, formHandle);
                        break;

                    case ProgressState.Header:
                        progressBar.Style = ProgressBarStyle.Marquee;
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsHelper.SetProgressState(TaskbarProgressBarState.Indeterminate, formHandle);
                        break;

                    case ProgressState.Data:
                        // Is the final size known?
                        if (sender.Progress != -1)
                        {
                            _task.ProgressChanged += ProgressChanged;
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
        /// Changes the value of the <see cref="progressBar"/> and the taskbar depending on the already proccessed bytes.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <remarks>Taskbar only changes for Windows 7 or newer.</remarks>
        private void ProgressChanged(IProgress sender)
        {
            int currentValue = (int)(_task.Progress * 100);

            // Handle events coming from a non-UI thread, block caller so we can safely retreive properties
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
