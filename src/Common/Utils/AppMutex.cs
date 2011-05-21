/*
 * Copyright 2006-2011 Bastian Eicher
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
using System.Security.Cryptography;
using System.Text;
using Common.Storage;

namespace Common.Utils
{
    /// <summary>
    /// Allows control over the number of instances of an application running.
    /// </summary>
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
            foreach (var handle in _handles)
            {
                if (handle != IntPtr.Zero) WindowsUtils.CloseMutex(handle);
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

            if (!WindowsUtils.IsWindows) return false;

            // Always create the mutex both in the local session and globally and report if any instance already existed
            bool result = false;
            IntPtr handle;
            try { result |= WindowsUtils.CreateMutex(name, out handle); }
            catch (Win32Exception ex) { Log.Warn(ex.Message); }
            try { result |= WindowsUtils.CreateMutex("Global\\" + name, out handle); }
            catch (Win32Exception ex) { Log.Warn(ex.Message); }

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

            if (!WindowsUtils.IsWindows)
            {
                mutex = null;
                return false;
            }

            // Always create the mutex both in the local session and globally and report if any instance already existed
            bool result = false;
            IntPtr handle1 = IntPtr.Zero, handle2 = IntPtr.Zero;
            try { result |= WindowsUtils.CreateMutex(name, out handle1); }
            catch(Win32Exception ex) { Log.Warn(ex.Message); }
            try { result |= WindowsUtils.CreateMutex("Global\\" + name, out handle2); }
            catch (Win32Exception ex) { Log.Warn(ex.Message); }

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

            if (!WindowsUtils.IsWindows) return false;

            // Always create the mutex both in the local session and globally and report if any instance already existed
            bool result = false;
            try { result |= WindowsUtils.OpenMutex(name); }
            catch (Win32Exception ex) { Log.Warn(ex.Message); }
            try { result |= WindowsUtils.OpenMutex("Global\\" + name); }
            catch (Win32Exception ex) { Log.Warn(ex.Message); }
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

            if (!WindowsUtils.IsWindows) return false;

            // Always check for the mutex both in the local session and globally and report if any instance already existed
            bool result = false;
            try { result |= WindowsUtils.ProbeMutex(name); }
            catch (Win32Exception ex) { Log.Warn(ex.Message); }
            try { result |= WindowsUtils.ProbeMutex("Global\\" + name); }
            catch (Win32Exception ex) { Log.Warn(ex.Message); }
            return result;
        }

        /// <summary>
        /// Generates a mutex name using the prefix "mutex-" and a SHA-256 hash of a path.
        /// </summary>
        /// <param name="path">The path to use for generating the mutex; usually <see cref="Locations.InstallBase"/>.</param>
        /// <remarks>Use this to differentiate between instances of an application installed in different locations.</remarks>
        public static string GenerateName(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            var locationHash = SHA256.Create().ComputeHash(Encoding.ASCII.GetBytes(path));
            return "mutex-" + BitConverter.ToString(locationHash).Replace("-", "").ToLowerInvariant();
        }
    }
}
