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
using System.Windows.Forms;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// A progress bar that takes <see cref="TaskSnapshot"/> inputs.
    /// </summary>
    public sealed class TaskProgressBar : ProgressBar
    {
        #region Properties
        /// <summary>
        /// Determines the handle of the <see cref="Form"/> containing this control.
        /// </summary>
        /// <returns>The handle of the parent <see cref="Form"/> or <see cref="IntPtr.Zero"/> if there is no parent.</returns>
        private IntPtr ParentHandle
        {
            get
            {
                Form parent = FindForm();
                return (parent == null ? IntPtr.Zero : parent.Handle);
            }
        }

        /// <summary>
        /// Show the progress in the Windows taskbar.
        /// </summary>
        /// <remarks>Use only once per window. Only works on Windows 7 or newer.</remarks>
        [Description("Show the progress in the Windows taskbar."), DefaultValue(false)]
        public bool UseTaskbar { set; get; }
        #endregion

        public TaskProgressBar()
        {
            CreateHandle();
        }

        /// <summary>
        /// Sets the current progress to be displayed.
        /// </summary>
        public void Report(TaskSnapshot snapshot)
        {
            switch (snapshot.Status)
            {
                case TaskStatus.Ready:
                    // When the status is complete the bar should always be empty
                    Style = ProgressBarStyle.Continuous;
                    Value = 0;

                    if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.NoProgress);
                    break;

                case TaskStatus.Started:
                case TaskStatus.Header:
                    Style = ProgressBarStyle.Marquee;
                    if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.Indeterminate);
                    break;

                case TaskStatus.Data:
                    if (snapshot.UnitsTotal == -1)
                    {
                        Style = ProgressBarStyle.Marquee;
                        if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.Indeterminate);
                    }
                    else
                    {
                        Style = ProgressBarStyle.Continuous;
                        if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.Normal);
                    }
                    break;

                case TaskStatus.IOError:
                case TaskStatus.WebError:
                    Style = ProgressBarStyle.Continuous;
                    if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.Error);
                    break;

                case TaskStatus.Complete:
                    // When the status is complete the bar should always be full
                    Style = ProgressBarStyle.Continuous;
                    Value = 100;

                    if (UseTaskbar && ParentHandle != IntPtr.Zero) WindowsUtils.SetProgressState(ParentHandle, WindowsUtils.TaskbarProgressBarState.NoProgress);
                    break;
            }

            var currentValue = (int)(snapshot.Value * 100);
            if (currentValue < 0) currentValue = 0;
            else if (currentValue > 100) currentValue = 100;

            // When the status is complete the bar should always be full
            if (snapshot.Status == TaskStatus.Complete) currentValue = 100;

            Value = currentValue;
            IntPtr formHandle = ParentHandle;
            if (UseTaskbar && formHandle != IntPtr.Zero) WindowsUtils.SetProgressValue(formHandle, currentValue, 100);
        }
    }
}
