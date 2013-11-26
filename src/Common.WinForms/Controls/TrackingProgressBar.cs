/*
 * Copyright 2006-2013 Bastian Eicher
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
using System.Runtime.Remoting;
using System.Windows.Forms;
using Common.Tasks;
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// A progress bar that automatically tracks the progress of an <see cref="ITask"/>.
    /// </summary>
    public sealed class TrackingProgressBar : ProgressBar
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
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread or before the <see cref="Control.Handle"/> is created.</exception>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITask Task
        {
            set
            {
                #region Sanity checks
                if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
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
            // Handle the initial values
            OnStateChanged(_task.State, _task.UnitsTotal);
            if (_task.State == TaskState.Data) OnProgressChanged(_task.State, _task.Progress);

            _task.StateChanged += OnStateChanged;
            _task.ProgressChanged += OnProgressChanged;
        }

        /// <summary>
        /// Stops tracking the progress of <see cref="_task"/>.
        /// </summary>
        private void HookOut()
        {
            try
            {
                _task.StateChanged -= OnStateChanged;
                _task.ProgressChanged -= OnProgressChanged;
            }
            catch (RemotingException)
            {
                // Ignore timed out connections or terminated processes if the task is completed or being canceled anyway
            }
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

        #region Constructor
        public TrackingProgressBar()
        {
            CreateHandle();
        }
        #endregion

        //--------------------//

        #region Event callbacks
        /// <summary>
        /// Handles <see cref="ITask.StateChanged"/> events.
        /// </summary>
        /// <param name="sender">The <see cref="ITask"/> that called this method.</param>
        /// <remarks>May be called from any thread.</remarks>
        // Must be public for IPC
        // ReSharper disable MemberCanBePrivate.Global
        public void OnStateChanged(ITask sender)
            // ReSharper restore MemberCanBePrivate.Global
        {
            #region Sanity checks
            if (sender == null) throw new ArgumentNullException("sender");
            #endregion

            // Copy values so they can be safely accessed from another thread
            TaskState state = sender.State;
            long unitsTotal = sender.UnitsTotal;

            // Handle events coming from a non-UI thread, block caller
            Invoke(new Action(() => OnStateChanged(state, unitsTotal)));
        }

        /// <summary>
        /// Updates the <see cref="ProgressBarStyle"/> and the taskbar (on Windows 7 and later). Called internally by <see cref="OnStateChanged(ITask)"/>.
        /// </summary>
        /// <remarks>Must be called from the UI thread.</remarks>
        private void OnStateChanged(TaskState state, long unitsTotal)
        {
            switch (state)
            {
                case TaskState.Ready:
                    // When the status is complete the bar should always be empty
                    Style = ProgressBarStyle.Continuous;
                    Value = 0;

                    if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.NoProgress);
                    break;

                case TaskState.Started:
                case TaskState.Header:
                    Style = ProgressBarStyle.Marquee;
                    if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.Indeterminate);
                    break;

                case TaskState.Data:
                    if (unitsTotal == -1)
                    {
                        Style = ProgressBarStyle.Marquee;
                        if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.Indeterminate);
                    }
                    else
                    {
                        Style = ProgressBarStyle.Continuous;
                        if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.Normal);
                    }
                    break;

                case TaskState.IOError:
                case TaskState.WebError:
                    Style = ProgressBarStyle.Continuous;
                    if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.Error);
                    break;

                case TaskState.Complete:
                    // When the status is complete the bar should always be full
                    Style = ProgressBarStyle.Continuous;
                    Value = 100;

                    if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.NoProgress);
                    break;
            }
        }

        /// <summary>
        /// Handles <see cref="ITask.ProgressChanged"/> events.
        /// </summary>
        /// <param name="sender">The <see cref="ITask"/> that called this method.</param>
        /// <remarks>May be called from any thread.</remarks>
        // ReSharper disable MemberCanBePrivate.Global
        // Must be public for IPC
        public void OnProgressChanged(ITask sender)
            // ReSharper restore MemberCanBePrivate.Global
        {
            #region Sanity checks
            if (sender == null) throw new ArgumentNullException("sender");
            #endregion

            // Only track units in data state
            if (sender.State != TaskState.Data) return;

            // Copy values so they can be safely accessed from another thread
            TaskState state = sender.State;
            double progress = sender.Progress;

            // Handle events coming from a non-UI thread, block caller
            Invoke(new Action(() => OnProgressChanged(state, progress)));
        }

        /// <summary>
        /// Updates the <see cref="ProgressBarStyle"/> and the taskbar (on Windows 7 and later). Called internally by <see cref="OnProgressChanged(ITask)"/>.
        /// </summary>
        /// <remarks>Must be called from the UI thread. Only call if <see cref="ITask.State"/> is <see cref="TaskState.Data"/>.</remarks>
        private void OnProgressChanged(TaskState state, double progress)
        {
            // Clamp the progress to values between 0 and 1
            if (progress < 0) progress = 0;
            else if (progress > 1) progress = 1;
            var currentValue = (int)(progress * 100);

            // When the status is complete the bar should always be full
            if (state == TaskState.Complete) currentValue = 100;

            Value = currentValue;
            IntPtr formHandle = ParentHandle;
            if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressValue(formHandle, currentValue, 100);
        }
        #endregion

        #region IPC timeout
        /// <inheritdoc/>
        public override object InitializeLifetimeService()
        {
            return null; // Do not timeout progress reporting callbacks
        }
        #endregion

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
