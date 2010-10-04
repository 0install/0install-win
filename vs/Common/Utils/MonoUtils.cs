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
using System.Collections.Specialized;
using System.IO;
using Common.Properties;
using Mono.Unix;
using Mono.Unix.Native;

namespace Common.Utils
{
    /// <summary>
    /// Provides helper methods for Unix-specific features of the Mono library. Make sure you are running a Unix-like system before calling any methods in this class!
    /// </summary>
    public static class MonoUtils
    {
        #region OS
        /// <summary>
        /// <see langword="true"/> if the current operating system is a Unix-like system (e.g. Linux or MacOS X).
        /// </summary>
        public static bool IsUnix
        { get { return Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX; } }
        #endregion

        #region File type
        /// <summary>
        /// Checks whether a file is a regular file (i.e. not a device file, symbolic link, etc.). Don't call on non-Unix-like systems!
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a regular file; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files.</remarks>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        internal static bool IsRegularFile(string path)
        {
            return new UnixFileInfo(path).IsRegularFile;
        }

        /// <summary>
        /// Checks whether a file is a Unix symbolic link. Don't call on non-Unix-like systems!
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a symbolic link; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files.</remarks>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        internal static bool IsSymlink(string path, out string contents, out long length)
        {
            bool result = UnixFileSystemInfo.GetFileSystemEntry(path).IsSymbolicLink;
            
            if (result)
            {
                var symlinkInfo = new UnixSymbolicLinkInfo(path);
                length = symlinkInfo.Length;
                contents = symlinkInfo.ContentsPath;
            }
            else
            {
                contents = null;
                length = 0;
            }

            return result;
        }
        #endregion

        #region Permissions
        /// <summary>A combination of bit flags to grant everyone executing permissions.</summary>
        private const FileAccessPermissions AllExecutePermission = FileAccessPermissions.UserExecute | FileAccessPermissions.GroupExecute | FileAccessPermissions.OtherExecute;
        
        /// <summary>
        /// Checks whether a file is marked as Unix-executable. Don't call on non-Unix-like systems!
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to an executable; <see lang="false"/> otherwise.</return>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        /// <remarks>Will return <see langword="false"/> for non-existing files.</remarks>
        internal static bool IsExecutable(string path)
        {
            // Check if any execution rights are set
            var fileInfo = new UnixFileInfo(path);
            return ((fileInfo.FileAccessPermissions & AllExecutePermission) > 0);
        }

        /// <summary>
        /// Marks a file as Unix-executable or not Unix-executable. Don't call on non-Unix-like systems!
        /// </summary>
        /// <param name="path">The file to mark as executable or not executable.</param>
        /// <param name="executable"><see lang="true"/> to mark the file as executable, <see lang="true"/> to mark it as not executable.</param>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        internal static void SetExecutable(string path, bool executable)
        {
            // Set or unset all execution rights
            var fileInfo = new UnixFileInfo(path);
            if (executable) fileInfo.FileAccessPermissions = fileInfo.FileAccessPermissions | AllExecutePermission;
            else fileInfo.FileAccessPermissions = fileInfo.FileAccessPermissions & ~AllExecutePermission;
        }
        #endregion

        #region Execute
        /// <summary>
        /// Replaces the currently executing process with a new one.
        /// </summary>
        /// <param name="path">The file containing the executable image for the new process.</param>
        /// <param name="arguments">Command-line arguments to pass to the new process.</param>
        /// <param name="environment">The environment variable values the new process should start off with.</param>
        /// <exception cref="IOException">Thrown if the process could not be replaced.</exception>
        public static void ProcessReplace(string path, string arguments, StringDictionary environment)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (environment == null) throw new ArgumentNullException("environment");
            #endregion

            if (Syscall.execve(path, arguments.Split(' '), GetEnv(environment)) == -1)
                throw new IOException(string.Format(Resources.FailedToLaunch, path));
        }

        /// <summary>
        /// Launches a new child process and detaches it so it can continue running after the parent terminates.
        /// </summary>
        /// <param name="path">The file containing the executable image for the new process.</param>
        /// <param name="arguments">Command-line arguments to pass to the new process.</param>
        /// <param name="environment">The environment variable values the new process should start off with.</param>
        /// <exception cref="IOException">Thrown if the process could not be replaced.</exception>
        public static void ProcessDetach(string path, string arguments, StringDictionary environment)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (arguments == null) throw new ArgumentNullException("arguments");
            if (environment == null) throw new ArgumentNullException("environment");
            #endregion

            // ToDo: Implement
            throw new NotImplementedException();
        }

        /// <summary>
        /// Turns a string dictionary into a flat "name=value" array.
        /// </summary>
        private static string[] GetEnv(StringDictionary environment)
        {
            var env = new string[environment.Count];
            int i = 0;
            foreach (string varName in environment)
                env[i++] = varName.Replace("=", "\\=") + "=" + environment[varName];
            return env;
        }
        #endregion
    }
}
