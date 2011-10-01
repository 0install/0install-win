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
using Common.Tasks;
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
        ///   <para>Setting this property will hook up event handlers to monitor the task.</para>
        ///   <para>Remember to set it back to <see langword="null"/> or to call <see cref="IDisposable.Dispose"/> when done, to remove the event handlers again.</para>
        ///   <para>The value must not be set from a background thread.</para>
        ///   <para>The value must not be set before <see cref="Control.Handle"/> has been created.</para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread or before the <see cref="Control.Handle"/> is created.</exception>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITask Task
        {
            set
            {
                #region Sanity checks
                if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
                if (!IsHandleCreated) throw new InvalidOperationException("Method called before control handle was created.");
                #endregion

                // Remove all delegates from old _task
                if (_task != null) HookOut();

                _task = value;

                if (_task != null) HookIn();
            }
            get { return _task; }
        }

        /// <summary>
        /// Starts tracking the progress of <see cref="_task"/>.
        /// </summary>
        private void HookIn()
        {
            // Get the initial values
            StateChanged(_task);
            ProgressChanged(_task);

            _task.StateChanged += StateChanged;
            _task.ProgressChanged += ProgressChanged;
        }

        /// <summary>
        /// Stops tracking the progress of <see cref="_task"/>.
        /// </summary>
        private void HookOut()
        {
            _task.StateChanged -= StateChanged;
            _task.ProgressChanged -= ProgressChanged;
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

            // Handle events coming from a non-UI thread, block caller
            Invoke(new SimpleEventHandler(delegate
            {
                IntPtr formHandle = ParentHandle;
                switch (state)
                {
                    case TaskState.Ready:
                        Style = ProgressBarStyle.Continuous;
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(formHandle, TaskbarProgressBarState.Paused);
                        break;

                    case TaskState.Header:
                        Style = ProgressBarStyle.Marquee;
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(formHandle, TaskbarProgressBarState.Indeterminate);
                        break;

                    case TaskState.Data:
                        if (sender.BytesTotal == -1)
                        {
                            Style = ProgressBarStyle.Marquee;
                            if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(formHandle, TaskbarProgressBarState.Indeterminate);
                        }
                        else
                        {
                            Style = ProgressBarStyle.Continuous;
                            if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(formHandle, TaskbarProgressBarState.Normal);
                        }
                        break;

                    case TaskState.IOError:
                    case TaskState.WebError:
                        Style = ProgressBarStyle.Continuous;
                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(formHandle, TaskbarProgressBarState.Error);
                        break;

                    case TaskState.Complete:
                        // When the status is complete the bar should always be full
                        Style = ProgressBarStyle.Continuous;
                        Value = 100;

                        if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressState(formHandle, TaskbarProgressBarState.NoProgress);
                        break;
                }
            }));
        }

        /// <summary>
        /// Changes the <see cref="ProgressBar.Value"/> and the taskbar depending on the number of already processed bytes.
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
            var currentValue = (int)(progress * 100);

            // When the status is complete the bar should always be full
            if (sender.State == TaskState.Complete) currentValue = 100;

            if (sender.State == TaskState.Data)
            {
                // Handle events coming from a non-UI thread, block caller
                Invoke(new SimpleEventHandler(delegate
                {
                    Value = currentValue;
                    IntPtr formHandle = ParentHandle;
                    if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressValue(formHandle, currentValue, 100);
                }));
            }
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (_task != null) HookOut();
            base.Dispose(disposing);
        }
        #endregion
    }
}
