/*
 * Copyright 2006-2011 Bastian Eicher
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
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Common.Properties;

namespace Common.Controls
{
    /// <summary>
    /// A dialog with a progress bar that tracks the progress of an <see cref="ITask"/>.
    /// </summary>
    public sealed partial class TrackingDialog : Form
    {
        #region Constructor
        /// <summary>
        /// Creates a new tracking progress dialog.
        /// </summary>
        /// <param name="task">The trackable task to execute and display.</param>
        /// <param name="icon">The icon for the dialog to display in the task bar; may be <see langword="null"/>.</param>
        private TrackingDialog(ITask task, Icon icon)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            InitializeComponent();
            buttonCancel.Text = Resources.Cancel;
            buttonCancel.Visible = task.CanCancel;
            Text = task.Name;
            Icon = icon;

            // Start and stop the task with the dialog
            Shown += delegate { task.Start(); };
            FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                if (task.State < TaskState.Complete)
                {
                    if (task.CanCancel) task.Cancel(); 
                    else e.Cancel = true;
                }
            };

            // Hook up event tracking
            trackingProgressBar.Task = task;
            labelProgress.Task = task;
            task.StateChanged += delegate
            {
                if (task.State >= TaskState.Complete)
                {
                    // Handle events coming from a non-UI thread, don't block caller
                    BeginInvoke((SimpleEventHandler)Close);
                }
            };
        }
        #endregion

        #region Static access
        /// <summary>
        /// Runs the <paramref name="task"/>, displays the progress and returns after the task completes. Equivalent to calling <see cref="ITask.RunSync"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to.</param>
        /// <param name="task">The trackable task to execute and display.</param>
        /// <param name="icon">The icon for the dialog to display in the task bar; may be <see langword="null"/>.</param>
        /// <exception cref="UserCancelException">Thrown if the user clicked the "Cancel" button.</exception>
        /// <exception cref="IOException">Thrown if the task ended with <see cref="TaskState.IOError"/>.</exception>
        /// <exception cref="WebException">Thrown if the task ended with <see cref="TaskState.WebError"/>.</exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ITask.State"/> is not <see cref="TaskState.Ready"/>.</exception>
        public static void Run(IWin32Window owner, ITask task, Icon icon)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            if (task.State != TaskState.Ready) throw new InvalidOperationException(Resources.StateMustBeReady);

            // Show the dialog and run the task
            using (var dialog = new TrackingDialog(task, icon))
                dialog.ShowDialog(owner);

            // Wait until the task has really finished and then check its state
            task.Join();
            switch (task.State)
            {
                case TaskState.Complete: return;
                case TaskState.WebError: throw new WebException(task.ErrorMessage);
                case TaskState.IOError: throw new IOException(task.ErrorMessage);
                default: throw new UserCancelException();
            }
        }
        #endregion
    }
}
