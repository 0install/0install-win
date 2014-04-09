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

using System.ComponentModel;
using System.Windows.Forms;
using NanoByte.Common.Tasks;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// Combines a <see cref="TaskProgressBar"/> and a <see cref="TaskLabel"/>.
    /// </summary>
    public sealed partial class TaskControl : UserControl
    {
        /// <summary>
        /// The name of the task being tracked.
        /// </summary>
        [Description("The name of the task being tracked.")]
        [DefaultValue("")]
        public string TaskName
        {
            get { return labelOperation.Text; }
            set
            {
                labelOperation.Text = (value ?? "");
                toolTip.SetToolTip(labelOperation, labelOperation.Text); // Show as tooltip in case text is cut off
            }
        }

        /// <summary>
        /// Creates a new tracking control.
        /// </summary>
        public TaskControl()
        {
            InitializeComponent();
            CreateHandle();
        }

        /// <summary>
        /// Sets the current progress to be displayed.
        /// </summary>
        public void Report(TaskSnapshot snapshot)
        {
            progressBar.Report(snapshot);
            progressLabel.Report(snapshot);
        }
    }
}
