/*
 * Copyright 2006-2014 Bastian Eicher
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
using System.Threading;
using System.Windows.Forms;
using Common.Properties;

namespace Common.Tasks
{
    /// <summary>
    /// A dialog with a progress bar that tracks the progress of an <see cref="ITask"/>.
    /// </summary>
    internal sealed partial class TrackingDialog : Form
    {
        private readonly ITask _task;
        private readonly Thread _taskThread;
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Creates a new progress-tracking dialog.
        /// </summary>
        /// <param name="task">The trackable task to execute and display.</param>
        /// <param name="cancellationTokenSource"></param>
        public TrackingDialog(ITask task, CancellationTokenSource cancellationTokenSource)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            InitializeComponent();

            buttonCancel.Text = Resources.Cancel;
            buttonCancel.Enabled = task.CanCancel;
            Text = task.Name;

            _task = task;
            _taskThread = new Thread(RunTask);
            _cancellationTokenSource = cancellationTokenSource;
        }

        private void TrackingDialog_Shown(object sender, EventArgs e)
        {
            // Hook up event tracking
            trackingProgressBar.Task = _task;
            labelProgress.Task = _task;
            _task.StateChanged += OnTaskStateChanged;

            _taskThread.Start();
        }

        /// <summary>
        /// An exception thrown by <see cref="ITask.RunSync"/>, if any.
        /// </summary>
        public Exception Exception { get; private set; }

        private void RunTask()
        {
            try
            {
                _task.RunSync(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }

        // NOTE: Must be public for IPC
        // ReSharper disable once MemberCanBePrivate.Global
        public void OnTaskStateChanged(ITask sender)
        {
            #region Sanity checks
            if (sender == null) throw new ArgumentNullException("sender");
            #endregion

            // Close window when the task has been completed
            if (sender.State >= TaskState.Complete) Invoke(new Action(Close));
        }

        private void TrackingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Only close the window if the task has been completed or canceled
            if (!_taskThread.IsAlive || _task.State >= TaskState.Complete) return;

            if (_task.CanCancel)
            {
                buttonCancel.Enabled = false; // Don't tempt user to press Cancel again
                _cancellationTokenSource.Cancel();
            }

            e.Cancel = true; // Window cannot be closed yet
        }
    }
}
