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

using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using NanoByte.Common.Properties;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Controls
{
    /// <summary>
    /// A progress label that takes <see cref="TaskSnapshot"/> inputs.
    /// </summary>
    public sealed class TaskLabel : Label
    {
        public TaskLabel()
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
                case TaskStatus.Started:
                    Text = "";
                    ForeColor = SystemColors.ControlText;
                    break;

                case TaskStatus.Header:
                    Text = Resources.StateHeader;
                    ForeColor = SystemColors.ControlText;
                    break;

                case TaskStatus.Data:
                    Text = GetDataText(snapshot);
                    ForeColor = SystemColors.ControlText;
                    break;

                case TaskStatus.Complete:
                    Text = Resources.StateComplete;
                    ForeColor = Color.Green;
                    break;

                case TaskStatus.WebError:
                    Text = Resources.StateWebError;
                    ForeColor = Color.Red;
                    break;

                case TaskStatus.IOError:
                    Text = Resources.StateIOError;
                    ForeColor = Color.Red;
                    break;
            }
        }

        private static string GetDataText(TaskSnapshot snapshot)
        {
            if (snapshot.UnitsProcessed == 0 && snapshot.UnitsTotal == -1) return Resources.StateData;

            string dataText = snapshot.UnitsByte
                ? snapshot.UnitsProcessed.FormatBytes(CultureInfo.CurrentCulture)
                : snapshot.UnitsProcessed.ToString(CultureInfo.CurrentCulture);
            if (snapshot.UnitsTotal != -1)
            {
                return dataText + @" / " + (snapshot.UnitsByte
                    ? snapshot.UnitsTotal.FormatBytes(CultureInfo.CurrentCulture)
                    : snapshot.UnitsTotal.ToString(CultureInfo.CurrentCulture));
            }
            return dataText;
        }
    }
}
