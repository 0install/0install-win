/*
 * Copyright 2006-2010 Bastian Eicher
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
using System.IO;
using System.Net;
using System.Windows.Forms;
using Common.Properties;

namespace Common.Controls
{
    /// <summary>
    /// A dialog with a progress bar that automatically tracks the progress of an <see cref="IProgress"/> task.
    /// </summary>
    public sealed partial class TrackingProgressDialog : Form
    {
        #region Constructor
        /// <summary>
        /// Creates a new tracking progress dialog.
        /// </summary>
        /// <param name="task">The trackable task to execute and display.</param>
        private TrackingProgressDialog(IProgress task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            InitializeComponent();
            buttonCancel.Text = Resources.Cancel;
            Text = task.Name;

            // Hook up event tracking
            trackingProgressBar.Task = task;
            Shown += delegate { task.Start(); };
            task.StateChanged += delegate { if (task.State >= ProgressState.Complete) Invoke((SimpleEventHandler)Close); };
            buttonCancel.Click += delegate { task.Cancel(); };
        }
        #endregion

        #region Static access
        /// <summary>
        /// Calls <see cref="IProgress.Start"/> on <paramref name="task"/>, displays the progress and returns after the task completes.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="task">The trackable task to execute and display.</param>
        /// <exception cref="UserCancelException">The task was cancelled from another thread.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="ProgressState.IOError"/>.</exception>
        /// <exception cref="WebException">Thrown if the task ended with <see cref="ProgressState.WebError"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="IProgress.State"/> is not <see cref="ProgressState.Ready"/>.</exception>
        public static void Run(IWin32Window owner, IProgress task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            if (task.State != ProgressState.Ready) throw new InvalidOperationException(Resources.StateMustBeReady);

            // Show the dialog and run the task
            using (var dialog = new TrackingProgressDialog(task))
                dialog.ShowDialog(owner);

            // Wait until the task has really finished and then check its state
            task.Join();
            switch (task.State)
            {
                case ProgressState.Complete: return;
                case ProgressState.WebError: throw new WebException(task.ErrorMessage);
                case ProgressState.IOError: throw new IOException(task.ErrorMessage);
                default: throw new UserCancelException();
            }
        }
        #endregion
    }
}
