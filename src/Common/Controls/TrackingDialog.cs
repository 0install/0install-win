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
using System.Threading;
using System.Windows.Forms;
using Common.Properties;
using Common.Tasks;

namespace Common.Controls
{
    /// <summary>
    /// A dialog with a progress bar that tracks the progress of an <see cref="ITask"/>.
    /// </summary>
    public sealed partial class TrackingDialog : Form
    {
        #region Variables
        /// <summary>Indicates that the task has been canceled and that the window may be closed.</summary>
        private volatile bool _canceled;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new progress-tracking dialog.
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
            buttonCancel.Enabled = task.CanCancel;

            Icon = icon;
            Text = task.Name;

            // Start and stop the task with the dialog
            Shown += delegate
            {
                // Hook up event tracking
                trackingProgressBar.Task = task;
                labelProgress.Task = task;
                task.StateChanged += delegate
                {
                    // Close window when the task has been completed
                    if (task.State >= TaskState.Complete) Invoke(new SimpleEventHandler(Close));
                };

                task.Start();
            };
            FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                // Only close the window if the task has been completed or canceled
                if (task.State >= TaskState.Complete || _canceled) return;

                if (task.CanCancel)
                {
                    // Note: Must perform cancellation on a separate thread because it might send messages back to the GUI thread (which therefor must not be blocked)
                    new Thread(() =>
                    {
                        task.Cancel();
                        _canceled = true;
                        Invoke(new SimpleEventHandler(Close));
                    }).Start();
                }
                e.Cancel = true;
            };
        }
        #endregion

        #region Static access
        /// <summary>
        /// Runs the <paramref name="task"/>, displays the progress and returns after the task completes. Equivalent to calling <see cref="ITask.RunSync"/>.
        /// </summary>
        /// <param name="owner">The parent window the displayed window is modal to; may be <see langword="null"/>.</param>
        /// <param name="task">The trackable task to execute and display.</param>
        /// <param name="icon">The icon for the dialog to display in the task bar; may be <see langword="null"/>.</param>
        /// <exception cref="OperationCanceledException">Thrown if the user clicked the "Cancel" button.</exception>
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
                case TaskState.Complete:
                    return;
                case TaskState.WebError:
                    throw new WebException(task.ErrorMessage);
                case TaskState.IOError:
                    throw new IOException(task.ErrorMessage);
                default:
                    throw new OperationCanceledException();
            }
        }
        #endregion
    }
}
