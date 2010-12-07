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
using System.Collections;
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
        /// Checks whether a file is a regular file (i.e. not a device file, symbolic link, etc.).
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a regular file; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files.</remarks>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        public static bool IsRegularFile(string path)
        {
            return new UnixFileInfo(path).IsRegularFile;
        }

        /// <summary>
        /// Checks whether a file is a Unix symbolic link.
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a symbolic link; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files.</remarks>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        public static bool IsSymlink(string path, out string contents, out long length)
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
        /// <summary>A combination of bit flags to grant everyone writing permissions.</summary>
        private const FileAccessPermissions AllWritePermission = FileAccessPermissions.UserWrite | FileAccessPermissions.GroupWrite | FileAccessPermissions.OtherWrite;

        /// <summary>
        /// Removes write permissions for everyone on a filesystem object (file or directory).
        /// </summary>
        /// <param name="path">The filesystem object (file or directory) to make read-only.</param>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying Unix subsystem failed to process the request (e.g. because of insufficient rights).</exception>
        public static void MakeReadOnly(string path)
        {
            var fileSysInfo = UnixFileSystemInfo.GetFileSystemEntry(path);
            fileSysInfo.FileAccessPermissions = fileSysInfo.FileAccessPermissions & ~AllWritePermission;
        }

        /// <summary>
        /// Sets write permissions for the owner on a filesystem object (file or directory).
        /// </summary>
        /// <param name="path">The filesystem object (file or directory) to make writeable by the owner.</param>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying Unix subsystem failed to process the request (e.g. because of insufficient rights).</exception>
        public static void MakeWritable(string path)
        {
            var fileSysInfo = UnixFileSystemInfo.GetFileSystemEntry(path);
            fileSysInfo.FileAccessPermissions = fileSysInfo.FileAccessPermissions | FileAccessPermissions.UserWrite;
        }

        /// <summary>A combination of bit flags to grant everyone executing permissions.</summary>
        private const FileAccessPermissions AllExecutePermission = FileAccessPermissions.UserExecute | FileAccessPermissions.GroupExecute | FileAccessPermissions.OtherExecute;
        
        /// <summary>
        /// Checks whether a file is marked as Unix-executable.
        /// </summary>
        /// <param name="path">The file to check for executable rights.</param>
        /// <return><see lang="true"/> if <paramref name="path"/> points to an executable; <see lang="false"/> otherwise.</return>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying Unix subsystem failed to process the request (e.g. because of insufficient rights).</exception>
        /// <remarks>Will return <see langword="false"/> for non-existing files.</remarks>
        public static bool IsExecutable(string path)
        {
            // Check if any execution rights are set
            var fileInfo = new UnixFileInfo(path);
            return ((fileInfo.FileAccessPermissions & AllExecutePermission) > 0);
        }

        /// <summary>
        /// Marks a file as Unix-executable or not Unix-executable.
        /// </summary>
        /// <param name="path">The file to mark as executable or not executable.</param>
        /// <param name="executable"><see lang="true"/> to mark the file as executable, <see lang="true"/> to mark it as not executable.</param>
        /// <exception cref="IOException">Thrown if the Mono libraries could not be loaded.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the underlying Unix subsystem failed to process the request (e.g. because of insufficient rights).</exception>
        public static void SetExecutable(string path, bool executable)
        {
            var fileInfo = new UnixFileInfo(path);
            if (executable) fileInfo.FileAccessPermissions = fileInfo.FileAccessPermissions | AllExecutePermission; // Set all execution rights
            else fileInfo.FileAccessPermissions = fileInfo.FileAccessPermissions & ~AllExecutePermission; // Unset all execution rights
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
        /// <remarks>This method does not return on success. Warning: Any concurrent threads will be terminated!</remarks>
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
        /// Turns a string dictionary into a flat "name=value" array.
        /// </summary>
        private static string[] GetEnv(StringDictionary environment)
        {
            var env = new string[environment.Count];
            int i = 0;
            foreach (DictionaryEntry variable in environment)
                env[i++] = variable.Key.ToString().Replace("=", "\\=") + "=" + variable.Value;
            return env;
        }
        #endregion
    }
}
