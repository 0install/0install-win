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
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Forms;
using NanoByte.Common.Properties;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// A dialog with a progress bar that runs and tracks the progress of an <see cref="ITask"/>.
    /// </summary>
    internal sealed partial class TaskRunDialog : Form
    {
        private readonly ITask _task;
        private readonly Thread _taskThread;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Progress<TaskSnapshot> _progress = new Progress<TaskSnapshot>();

        /// <summary>
        /// Creates a new progress-tracking dialog.
        /// </summary>
        /// <param name="task">The trackable task to execute and display.</param>
        /// <param name="cancellationTokenSource"></param>
        public TaskRunDialog(ITask task, CancellationTokenSource cancellationTokenSource)
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
            _progress.ProgressChanged += trackingProgressBar.Report;
            _progress.ProgressChanged += labelProgress.Report;
            _progress.ProgressChanged += progress => { if (progress.Status == TaskStatus.Complete) Close(); };

            _taskThread.Start();
        }

        /// <summary>
        /// An exception thrown by <see cref="ITask.Run"/>, if any.
        /// </summary>
        public Exception Exception { get; private set; }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions are rethrown on the UI thread.")]
        private void RunTask()
        {
            try
            {
                _task.Run(_cancellationTokenSource.Token, _progress);
            }
            catch (Exception ex)
            {
                Exception = ex;
                Invoke(new Action(Close));
            }
        }

        private void TrackingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Only close the window if the task is no longer running
            if (!_taskThread.IsAlive || Exception != null) return;

            if (_task.CanCancel)
            {
                buttonCancel.Enabled = false; // Don't tempt user to press Cancel again
                _cancellationTokenSource.Cancel();
            }

            e.Cancel = true; // Window cannot be closed yet
        }
    }
}
