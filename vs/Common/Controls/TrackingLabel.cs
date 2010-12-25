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

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Common.Properties;
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// A label that automatically tracks the progress of an <see cref="IProgress"/> task.
    /// </summary>
    public class TrackingLabel : Label
    {
        #region Properties
        private IProgress _task;
        /// <summary>
        /// The <see cref="IProgress"/> object to track.
        /// </summary>
        /// <remarks>
        /// Setting this property will hook up event handlers to monitor the task.
        /// Remember to set it back to <see langword="null"/> or to call <see cref="Dispose"/> when done, to remove the event handlers again.
        /// </remarks>
        [DefaultValue(null), Description("The IProgress object to track.")]
        public IProgress Task
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
        #endregion

        //--------------------//

        #region Event callbacks
        /// <summary>
        /// Changes the <see cref="Label.Text"/> based on the <see cref="ProgressState"/> of <see cref="_task"/>.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        private void StateChanged(IProgress sender)
        {
            // Copy value so it can be safely accessed from another thread
            ProgressState state = sender.State;

            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                switch (state)
                {
                    case ProgressState.Ready:
                        ForeColor = SystemColors.ControlText;
                        Text = Resources.StateReady;
                        break;

                    case ProgressState.Header:
                        ForeColor = SystemColors.ControlText;
                        Text = Resources.StateHeader;
                        break;

                    case ProgressState.Data:
                        ForeColor = SystemColors.ControlText;
                        
                        // Only track progress if the final size is known
                        if (sender.BytesTotal != -1) _task.ProgressChanged += ProgressChanged;
                        else Text = Resources.StateData;
                        break;

                    case ProgressState.Complete:
                        Text = Resources.StateComplete;
                        ForeColor = Color.Green;
                        break;

                    case ProgressState.WebError:
                        ForeColor = Color.Red;
                        Text = Resources.StateWebError;
                        break;

                    case ProgressState.IOError:
                        ForeColor = Color.Red;
                        Text = Resources.StateIOError;
                        break;
                }
            });
        }

        /// <summary>
        /// Changes the <see cref="Label.Text"/> based on the already proccessed bytes.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        private void ProgressChanged(IProgress sender)
        {
            // Handle events coming from a non-UI thread, don't block caller
            BeginInvoke((SimpleEventHandler)delegate
            {
                Text = StringUtils.FormatBytes(sender.BytesProcessed) + @" / " + StringUtils.FormatBytes(sender.BytesTotal);
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
