/*
 * Copyright 2010 Simon E. Silva Lauinger
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
using System.Windows.Forms;
using Common.Tasks;

namespace Common.Controls
{
    /// <summary>
    /// Combines a <see cref="TrackingProgressBar"/> and a <see cref="TrackingLabel"/>.
    /// </summary>
    public partial class TrackingControl : UserControl
    {
        #region Properties
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
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ITask Task
        {
            set
            {
                #region Sanity checks
                if (InvokeRequired) throw new InvalidOperationException("Method called from a non UI thread.");
                if (!IsHandleCreated) throw new InvalidOperationException("Method called before control handle was created.");
                #endregion

                labelOperation.Text = (value == null ? "" : value.Name);
                toolTip.SetToolTip(labelOperation, labelOperation.Text); // Show as tooltip in case text is cut off
                progressBar.Task = value;
                progressLabel.Task = value;
            }
            get { return progressBar.Task; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new tracking control.
        /// </summary>
        public TrackingControl()
        {
            InitializeComponent();
        }
        #endregion
    }
}
