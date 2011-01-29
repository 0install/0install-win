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
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// A progress bar that automatically tracks the progress of an <see cref="ITask"/>.
    /// </summary>
    public class TrackingProgressBar : ProgressBar
    {
        #region Properties
        private ITask _task;
        /// <summary>
        /// The <see cref="ITask"/> to track.
        /// </summary>
        /// <remarks>
        /// Setting this property will hook up event handlers to monitor the task.
        /// Remember to set it back to <see langword="null"/> or to call <see cref="IDisposable.Dispose"/> when done, to remove the event handlers again.
        /// </remarks>
        [DefaultValue(null), Description("The IProgress object to track.")]
        public ITask Task
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
                    // Only hook up state event, progress tracking will be set up later
                    _task.StateChanged += StateChanged;

                    if (IsHandleCreated)
                    {
                        // Reset the display from any previous tasks
                        StateChanged(_task);
                        ProgressChanged(_task);
                    }
                }
            }
            get { return _task; }
        }

        /// <summary>
        /// Determines the handle of the <see cref="Form"/> containing this control.
        /// </summary>
        /// <returns>The handle of the parent <see cref="Form"/> or <see cref="IntPtr.Zero"/> if there is no parent.</returns>
        private IntPtr ParentHandle
        {
            get
            {
                Form parent = FindForm();
                return (parent == null ? IntPtr.Zero : parent.Handle);
            }
        }

        /// <summary>
        /// Show the progress in the Windows taskbar.
        /// </summary>
        /// <remarks>Use only once per window. Only works on Windows 7 or newer.</remarks>
        [Description("Show the progress in the Windows taskbar."), DefaultValue(false)]
        public bool UseTaskbar { set; get; }
        #endregion

        //--------------------//

        #region Event callbacks
        /// <summary>
        /// Changes the <see cref="ProgressBarStyle"/> and the taskbar based on the <see cref="TaskState"/> of <see cref="_task"/>.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <remarks>Taskbar only changes for Windows 7 or newer.</remarks>
        private void StateChanged(ITask sender)
        {
            // Copy value so it can be safely accessed from another thread
            TaskState state = sender.State;

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler) delegate
            {
                IntPtr formHandle = ParentHandle;
                switch (state)
                {
                    case TaskState.Ready:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(TaskbarProgressBarState.Paused, formHandle);
                        break;

                    case TaskState.Header:
                        Style = ProgressBarStyle.Marquee;
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(TaskbarProgressBarState.Indeterminate, formHandle);
                        break;

                    case TaskState.Data:
                        // Only track progress if the final size is known
                        if (sender.BytesTotal != -1)
                        {
                            _task.ProgressChanged += ProgressChanged;
                            Style = ProgressBarStyle.Continuous;
                            if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(TaskbarProgressBarState.Normal, formHandle);
                        }
                        break;

                    case TaskState.IOError:
                    case TaskState.WebError:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(TaskbarProgressBarState.Error, formHandle);
                        Style = ProgressBarStyle.Continuous;
                        Value = 0;
                        break;

                    case TaskState.Complete:
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(TaskbarProgressBarState.NoProgress, formHandle);

                        // When the status is complete the bar should always be full
                        Value = 100;
                        break;
                }
            });
        }

        /// <summary>
        /// Changes the <see cref="ProgressBar.Value"/> and the taskbar depending on the already processed bytes.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        /// <remarks>Taskbar only changes for Windows 7 or newer.</remarks>
        private void ProgressChanged(ITask sender)
        {
            // Clamp the progress to values between 0 and 1
            double progress = sender.Progress;
            if (progress < 0) progress = 0;
            else if (progress > 1) progress = 1;

            // Copy value so it can be safely accessed from another thread
            int currentValue = (int)(progress * 100);

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                Value = currentValue;
                IntPtr formHandle = ParentHandle;
                if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressValue(currentValue, 100, formHandle);
            });
        }
        #endregion

        //--------------------//

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Remove update hooks
                Task = null;
            }
            base.Dispose(disposing);
        }
    }
}
