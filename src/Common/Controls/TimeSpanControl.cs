﻿/*
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
using System.Windows.Forms;

namespace Common.Controls
{
    /// <summary>
    /// Allows the input of a <see cref="TimeSpan"/> using <see cref="NumericUpDown"/> boxes.
    /// </summary>
    public partial class TimeSpanControl : UserControl
    {
        /// <summary>
        /// The time span currently represented by the control.
        /// </summary>
        public TimeSpan Value
        {
            get { return new TimeSpan((int)upDownDays.Value, (int)upDownHours.Value, (int)upDownMinutes.Value, (int)upDownSeconds.Value); }
            set
            {
                upDownDays.Value = value.Days;
                upDownHours.Value = value.Hours;
                upDownMinutes.Value = value.Minutes;
                upDownSeconds.Value = value.Seconds;
            }
        }

        public TimeSpanControl()
        {
            InitializeComponent();
        }
    }
}
