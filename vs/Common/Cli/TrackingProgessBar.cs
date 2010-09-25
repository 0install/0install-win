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

namespace Common.Cli
{
    /// <summary>
    /// A progress bar rendered on the <see cref="Console"/> that automatically tracks the progress of an <see cref="IProgress"/> object.
    /// </summary>
    public class TrackingProgressBar : ProgessBar
    {
        #region Variables
        private readonly IProgress _task;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new progress bar that automatically tracks an <see cref="IProgress"/> object and draws itself whenever it changes.
        /// </summary>
        /// <param name="task">The <see cref="IProgress"/> object to track.</param>
        public TrackingProgressBar(IProgress task)
        {
            #region Sanity checks
            if (task == null) throw new ArgumentNullException("task");
            #endregion

            _task = task;

            _task.StateChanged += StateChanged;
            _task.ProgressChanged += ProgressChanged;
        }
        #endregion

        //--------------------//

        #region Event callbacks
        /// <summary>
        /// Changes the look of the progress bar depending on the <see cref="ProgressState"/> of <see cref="_task"/>.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        private void StateChanged(IProgress sender)
        {
            State = sender.State;

            // When the status is complete the bar should always be full
            if (State == ProgressState.Complete) Value = Maximum;
        }

        /// <summary>
        /// Changes the value of the progress bar depending on the already proccessed bytes.
        /// </summary>
        /// <param name="sender">Object that called this method.</param>
        private void ProgressChanged(IProgress sender)
        {
            // Clamp the progress to values between 0 and 1
            double progress = sender.Progress;
            if (progress < 0) progress = 0;
            else if (progress > 1) progress = 1;

            Value = (int)(progress * Maximum);
        }
        #endregion
    }
}
