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
using System.Diagnostics;

namespace NanoByte.Common
{
    /// <summary>
    /// Structure that allows you to log timed execution blocks.
    /// </summary>
    /// <example>
    ///   <code>using(new LogEvent("Message")) {}</code>
    /// </example>
    public struct TimedLogEvent : IDisposable
    {
        #region Variables
        private readonly Stopwatch _timer;
        private readonly string _entry;
        #endregion

        #region Event control
        /// <summary>
        /// Starts a new log event.
        /// </summary>
        /// <param name="entry">The entry for the log file. Elapsed time will automatically be appended.</param>
        public TimedLogEvent(string entry)
        {
            _entry = entry;
            _timer = Stopwatch.StartNew();
        }

        /// <summary>
        /// Ends the log event.
        /// </summary>
        public void Dispose()
        {
            _timer.Stop();
            Log.Info(_entry + " => " + (float)_timer.Elapsed.TotalSeconds + "s");
        }
        #endregion
    }
}
