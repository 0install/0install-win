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
using System.Drawing;
using System.Windows.Forms;
using Common.Properties;
using Common.Tasks;
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// A label that automatically tracks the progress of an <see cref="ITask"/>.
    /// </summary>
    public class TrackingLabel : Label
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
        [DefaultValue(null), Description("The ITask object to track.")]
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
        /// The state currently reported by <see cref="Label.Text"/>.
        /// </summary>
        public TaskState CurrentState { get; private set; }
        #endregion

        //--------------------//

        #region Event callbacks
        /// <summary>
        /// Changes the <see cref="Label.Text"/> based on the <see cref="TaskState"/> of <see cref="_task"/>.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        private void StateChanged(ITask sender)
        {
            // Copy value so it can be safely accessed from another thread
            TaskState state = sender.State;

            // Handle events coming from a non-UI thread, don't block caller
            Invoke(new SimpleEventHandler(delegate
            {
                CurrentState = state;
                switch (state)
                {
                    case TaskState.Ready:
                        Text = Resources.StateReady;
                        ForeColor = SystemColors.ControlText;
                        break;

                    case TaskState.Header:
                        Text = Resources.StateHeader;
                        ForeColor = SystemColors.ControlText;
                        break;

                    case TaskState.Data:
                        Text = Resources.StateData;
                        ForeColor = SystemColors.ControlText;
                        break;

                    case TaskState.Complete:
                        Text = Resources.StateComplete;
                        ForeColor = Color.Green;
                        break;

                    case TaskState.WebError:
                        Text = Resources.StateWebError;
                        ForeColor = Color.Red;
                        break;

                    case TaskState.IOError:
                        Text = Resources.StateIOError;
                        ForeColor = Color.Red;
                        break;
                }
            }));
        }

        /// <summary>
        /// Changes the <see cref="Label.Text"/> based on the already processed bytes.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        private void ProgressChanged(ITask sender)
        {
            // Only track bytes in data state
            if (sender.State != TaskState.Data) return;

            // Copy value so it can be safely accessed from another thread
            long bytesProcessed  = sender.BytesProcessed;
            long bytesTotal = sender.BytesTotal;

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke(new SimpleEventHandler(delegate
            {
                Text = StringUtils.FormatBytes(bytesProcessed);
                if (bytesTotal != -1) Text += @" / " + StringUtils.FormatBytes(bytesTotal);
            }));
        }
        #endregion

        //--------------------//

        #region Dispose
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_task != null) HookOut();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
