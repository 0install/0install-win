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
using System.Threading;
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
        #region Variables
        /// <summary>A barrier that blocks threads until the window handle is ready.</summary>
        private readonly EventWaitHandle _handleReady = new EventWaitHandle(false, EventResetMode.ManualReset);
        #endregion

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
        /// <exception cref="InvalidOperationException">Thrown if the value is set from a thread other than the UI thread.</exception>
        [DefaultValue(null), Description("The IProgress object to track.")]
        public ITask Task
        {
            set
            {
                if (InvokeRequired) throw new InvalidOperationException("Must set this from UI thread");

                // Remove all delegates from old _task
                HookOut();

                _task = value;

                // Only start tracking if the handle is available
                if (IsHandleCreated) HookIn();
            }
            get { return _task; }
        }

        /// <summary>
        /// Starts tracking the progress of <see cref="_task"/>.
        /// </summary>
        /// <remarks>This may only be called after <see cref="Control.HandleCreated"/> has been raised.</remarks>
        private void HookIn()
        {
            if (_task == null) return;

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
            if (_task == null) return;

            _task.StateChanged -= StateChanged;
            _task.ProgressChanged -= ProgressChanged;
        }
        #endregion

        #region Constructor
        public TrackingLabel()
        {
            // Track when events can be passed to the WinForms code
            HandleCreated += delegate
            {
                _handleReady.Set();
                HookIn();
            };
            HandleDestroyed += delegate { _handleReady.Reset(); };
        }
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
            _handleReady.WaitOne();
            BeginInvoke(new SimpleEventHandler(delegate
            {
                switch (state)
                {
                    case TaskState.Ready:
                        ForeColor = SystemColors.ControlText;
                        Text = Resources.StateReady;
                        break;

                    case TaskState.Header:
                        ForeColor = SystemColors.ControlText;
                        Text = Resources.StateHeader;
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
                        ForeColor = Color.Red;
                        Text = Resources.StateWebError;
                        break;

                    case TaskState.IOError:
                        ForeColor = Color.Red;
                        Text = Resources.StateIOError;
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
            _handleReady.WaitOne();
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
                // Remove update hooks
                Task = null;
            }
            try { base.Dispose(disposing); }
            finally { _handleReady.Close(); }
        }
        #endregion
    }
}
