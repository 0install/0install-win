/*
 * Copyright 2006-2010 Bastian Eicher
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

#if !DEBUG
using System.Threading;
using System.Windows.Forms;
using Common.Properties;
#endif

namespace Common.Utils
{
    /// <summary>
    /// Prevents an application from running multiple instances.
    /// </summary>
    public static class AppMutex
    {
#if !DEBUG
        private static Mutex _mutex;
#endif

        /// <summary>
        /// Tries to acquire a mutex for a specific application name.
        /// </summary>
        /// <param name="appName">The name of the application (used as a Mutex identifier).</param>
        /// <returns><see langword="true"/> if the Mutex was acquired successfully, <see langword="false"/> if another instance is already running.</returns>
        /// <remarks>Before <see langword="false"/> is returned this method already has displayed a MessageBox.</remarks>
        /// <exception cref="InvalidOperationException">Throw if this method is called more than once without a <see cref="Release"/> in between.</exception>
        public static bool Acquire(string appName)
        {
#if !DEBUG
            if (_mutex != null) throw new InvalidOperationException("Mutex already acquired");

            // Create a new mutex or acquire a handle to a previous one
            _mutex = new Mutex(false, appName);

            // Try to lock the mutex
            if (!_mutex.WaitOne(0, false))
            { // Previous instance detected

                // Note: Don't use Msg.Inform() because it would mess up the existing log file
                MessageBox.Show(Resources.AppAlreadyRunning, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }
#endif

            return true;
        }

        /// <summary>
        /// Releases the Mutex acquired by <see cref="Acquire"/>.
        /// </summary>
        public static void Release()
        {
#if !DEBUG
            try { _mutex.ReleaseMutex(); }
            catch (ApplicationException) {}

            _mutex = null;
#endif
        }
    }
}
