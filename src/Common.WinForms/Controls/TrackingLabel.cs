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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Windows.Forms;
using Common.Properties;
using Common.Tasks;
using Common.Utils;

namespace Common.Controls
{
    /// <summary>
    /// A label that automatically tracks the progress of an <see cref="ITask"/>.
    /// </summary>
    public sealed class TrackingLabel : Label
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
            OnStateChanged(_task.State);
            if (_task.State == TaskState.Data) OnProgressChanged(_task.UnitsByte, _task.UnitsProcessed, _task.UnitsTotal);

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
        /// The state currently reported by <see cref="Label.Text"/>.
        /// </summary>
        public TaskState CurrentState { get; private set; }
        #endregion

        #region Constructor
        public TrackingLabel()
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
        // Note: Must be public for IPC
        // ReSharper disable MemberCanBePrivate.Global
        public void OnStateChanged(ITask sender)
            // ReSharper restore MemberCanBePrivate.Global
        {
            #region Sanity checks
            if (sender == null) throw new ArgumentNullException("sender");
            #endregion

            // Copy values so they can be safely accessed from another thread
            TaskState state = sender.State;

            // Handle events coming from a non-UI thread, block caller
            Invoke(new Action(() => OnStateChanged(state)));
        }

        /// <summary>
        /// Updates the <see cref="Label.Text"/>. Called internally by <see cref="OnStateChanged(ITask)"/>.
        /// </summary>
        /// <remarks>Must be called from the UI thread.</remarks>
        private void OnStateChanged(TaskState state)
        {
            CurrentState = state;
            switch (state)
            {
                case TaskState.Ready:
                    Text = Resources.StateReady;
                    ForeColor = SystemColors.ControlText;
                    break;

                case TaskState.Started:
                    Text = "";
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
        }

        /// <summary>
        /// Handles <see cref="ITask.ProgressChanged"/> events.
        /// </summary>
        /// <param name="sender">The <see cref="ITask"/> that called this method.</param>
        /// <remarks>May be called from any thread.</remarks>
        // ReSharper disable MemberCanBePrivate.Global
        // Note: Must be public for IPC
        public void OnProgressChanged(ITask sender)
            // ReSharper restore MemberCanBePrivate.Global
        {
            #region Sanity checks
            if (sender == null) throw new ArgumentNullException("sender");
            #endregion

            // Only track units in data state
            if (sender.State != TaskState.Data) return;

            // Copy values so they can be safely accessed from another thread
            bool unitsByte = sender.UnitsByte;
            long unitsProcessed = sender.UnitsProcessed;
            long unitsTotal = sender.UnitsTotal;

            // Handle events coming from a non-UI thread, block caller
            Invoke(new Action(() => OnProgressChanged(unitsByte, unitsProcessed, unitsTotal)));
        }

        /// <summary>
        /// Updates the <see cref="Label.Text"/>. Called internally by <see cref="OnProgressChanged(ITask)"/>.
        /// </summary>
        /// <remarks>Must be called from the UI thread. Only call if <see cref="ITask.State"/> is <see cref="TaskState.Data"/>.</remarks>
        private void OnProgressChanged(bool unitsByte, long unitsProcessed, long unitsTotal)
        {
            string text = (unitsByte
                ? unitsProcessed.FormatBytes(CultureInfo.CurrentCulture)
                : unitsProcessed.ToString(CultureInfo.CurrentCulture));
            if (unitsTotal != -1)
            {
                text += @" / " + (unitsByte
                    ? unitsTotal.FormatBytes(CultureInfo.CurrentCulture)
                    : unitsTotal.ToString(CultureInfo.CurrentCulture));
            }
            Text = text;
        }
        #endregion

        #region IPC timeout
        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null; // Do not timeout progress reporting callbacks
        }
        #endregion

        #region Dispose
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                    if (_task != null) HookOut();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion
    }
}
