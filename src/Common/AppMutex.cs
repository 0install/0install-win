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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NanoByte.Common.Properties;
using NanoByte.Common.Utils;

namespace NanoByte.Common
{
    /// <summary>
    /// Provides a cross-process object allowing easy dection of application instances (e.g., for use by installers and update tools).
    /// </summary>
    /// <remarks><see cref="System.Threading.Mutex"/> is intended for synchronizing access to shared resources while this class is intended to detect application instances.</remarks>
    // ReSharper disable UnusedMethodReturnValue.Global
    public sealed class AppMutex
    {
        #region Handles
        private readonly List<IntPtr> _handles;

        private AppMutex(IEnumerable<IntPtr> handles)
        {
            _handles = new List<IntPtr>(handles);
        }

        /// <summary>
        /// Closes all contained handles, allowing the mutex to be released.
        /// </summary>
        public void Close()
        {
            foreach (var handle in _handles.Where(handle => handle != IntPtr.Zero).ToList())
            {
                WindowsUtils.CloseMutex(handle);
                _handles.Remove(handle);
            }
        }
        #endregion

        /// <summary>
        /// Creates or opens a mutex (local and global) to signal that an application is running.
        /// </summary>
        /// <param name="name">The name to be used as a mutex identifier.</param>
        /// <returns><see langword="true"/> if an existing mutex was opened; <see langword="false"/> if a new one was created.</returns>
        /// <remarks>The mutex will automatically be released once the process terminates. You can check the return value to prevent multiple instances from running.</remarks>
        public static bool Create(string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            if (!WindowsUtils.IsWindowsNT) return false;

            // Always create the mutex both in the local session and globally and report if any instance already existed
            bool result = false;
            IntPtr handle;
            try
            {
                result = WindowsUtils.CreateMutex("Global\\" + name, out handle);
            }
            catch (Win32Exception)
            {}
            try
            {
                result |= WindowsUtils.CreateMutex(name, out handle);
            }
            catch (Win32Exception ex)
            {
                Log.Warn(Resources.UnableToCreateMutex);
                Log.Warn(ex);
            }

            return result;
        }

        /// <summary>
        /// Creates or opens a mutex (local and global) to signal that an application is running.
        /// </summary>
        /// <param name="name">The name to be used as a mutex identifier.</param>
        /// <param name="mutex">A pointer to the mutex.</param>
        /// <returns><see langword="true"/> if an existing mutex was opened; <see langword="false"/> if a new one was created.</returns>
        /// <remarks>The mutex will automatically be released once the process terminates or you call <see cref="Close"/> on <paramref name="mutex"/>.</remarks>
        public static bool Create(string name, out AppMutex mutex)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            if (!WindowsUtils.IsWindowsNT)
            {
                mutex = null;
                return false;
            }

            // Always create the mutex both in the local session and globally and report if any instance already existed
            bool result = false;
            IntPtr handle1 = IntPtr.Zero, handle2 = IntPtr.Zero;
            try
            {
                result = WindowsUtils.CreateMutex("Global\\" + name, out handle1);
            }
            catch (Win32Exception ex)
            {
                Log.Warn(ex.Message);
            }
            try
            {
                result |= WindowsUtils.CreateMutex(name, out handle2);
            }
            catch (Win32Exception ex)
            {
                Log.Warn(ex.Message);
            }

            mutex = new AppMutex(new[] {handle1, handle2});
            return result;
        }

        /// <summary>
        /// Tries to open an existing mutex (local and global) signaling that an application is running.
        /// </summary>
        /// <param name="name">The name to be used as a mutex identifier.</param>
        /// <returns><see langword="true"/> if an existing mutex was opened; <see langword="false"/> if none existed.</returns>
        /// <remarks>Opening a mutex creates an additional handle to it, keeping it alive until the process terminates.</remarks>
        public static bool Open(string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            if (!WindowsUtils.IsWindowsNT) return false;

            // Always create the mutex both in the local session and globally and report if any instance already existed
            bool result = false;
            try
            {
                result = WindowsUtils.OpenMutex("Global\\" + name);
            }
            catch (Win32Exception ex)
            {
                Log.Warn(ex.Message);
            }
            try
            {
                result |= WindowsUtils.OpenMutex(name);
            }
            catch (Win32Exception ex)
            {
                Log.Warn(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Checks whether a specific mutex exists (local or global) without openining a lasting handle.
        /// </summary>
        /// <param name="name">The name to be used as a mutex identifier.</param>
        /// <returns><see langword="true"/> if an existing mutex was found; <see langword="false"/> if none existed.</returns>
        public static bool Probe(string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            #endregion

            if (!WindowsUtils.IsWindowsNT) return false;

            // Always check for the mutex both in the local session and globally and report if any instance already existed
            bool result = false;
            try
            {
                result = WindowsUtils.ProbeMutex("Global\\" + name);
            }
            catch (Win32Exception ex)
            {
                Log.Warn(ex.Message);
            }
            try
            {
                result |= WindowsUtils.ProbeMutex(name);
            }
            catch (Win32Exception ex)
            {
                Log.Warn(ex.Message);
            }
            return result;
        }
    }

    // ReSharper restore UnusedMethodReturnValue.Global
}
