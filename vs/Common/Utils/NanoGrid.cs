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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Common.Properties;

namespace Common.Utils
{
    /// <summary>
    /// Provides access to operations provided by NanoGrid. For details about NanoGrid see: http://www.nano-byte.de/info/nanogrid/
    /// </summary>
    /// <remarks>This class should only be used by <see cref="System.Windows.Forms"/> applications.</remarks>
    public static class NanoGrid
    {
        #region Properties
        /// <summary>
        /// <see langword="true"/> if NanoGrid is installed on this system and can be used; <see langword="false"/> otherwise.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                // NanoGrid is only available for the Windows platform
                if (!WindowsUtils.IsWindows) return false;

                try
                {
                    // Locate the NanoGrid executable via the Windows registry
                    var nanoGridInfo = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\NanoByte\\NanoGrid\\Info");
                    if (nanoGridInfo == null) return false;
                    string nanoGridPath = nanoGridInfo.GetValue("Position") as string;

                    // Check if the file exists and is accessible
                    return File.Exists(nanoGridPath);
                }
                #region Error handling
                catch (UnauthorizedAccessException)
                {
                    return false;
                }
                #endregion
            }
        }
        #endregion

        #region Operations
        /// <summary>
        /// Launches an application via NanoGrid. Exceptions are handled and reported via message boxes.
        /// </summary>
        /// <param name="appName">The name of the application to launch via NanoGrid.</param>
        /// <param name="args">Arguments to be passed on to the launched application.</param>
        public static void Launch(string appName, string args)
        {
            try
            {
                string nanoGridCommand = string.Format(CultureInfo.InvariantCulture, "nanogrid:/launch/\"{0}:{1}\" /autoClose", appName, args);
                Process.Start(nanoGridCommand);
            }
            catch (Win32Exception)
            {
                Msg.Inform(null, Resources.MissingNanoGrid, MsgSeverity.Error);
            }
            catch (FileNotFoundException)
            {
                Msg.Inform(null, Resources.MissingNanoGrid, MsgSeverity.Error);
            }
        }

        /// <summary>
        /// Uploads a file using NanoGrid. Exceptions are handled and reported via message boxes.
        /// </summary>
        /// <param name="path">The path to the file to be uploaded.</param>
        public static void Upload(string path)
        {
            try
            {
                string nanoGridCommand = string.Format(CultureInfo.InvariantCulture, "nanogrid:/upload/\"{0}\" /autoClose", path);
                Process.Start(nanoGridCommand);
            }
            catch (Win32Exception)
            {
                Msg.Inform(null, Resources.MissingNanoGrid, MsgSeverity.Error);
            }
            catch (FileNotFoundException)
            {
                Msg.Inform(null, Resources.MissingNanoGrid, MsgSeverity.Error);
            }
        }
        #endregion
    }
}
