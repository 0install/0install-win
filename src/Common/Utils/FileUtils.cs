/*
 * Copyright 2006-2012 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Common.Properties;
#if FS_SECURITY
using System.Security.AccessControl;
using System.Security.Principal;

#endif

namespace Common.Utils
{
    /// <summary>
    /// Provides filesystem-related helper methods.
    /// </summary>
    public static class FileUtils
    {
        #region Paths
        /// <summary>
        /// Replaces forward slashes with <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        public static string UnifySlashes(string value)
        {
            if (value == null) return null;
            return value.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Works like <see cref="Path.Combine"/> but supports an arbitrary number of arguments.
        /// </summary>
        /// <returns><see langword="null"/> if <paramref name="parts"/> was <see langword="null"/> or empty.</returns>
        /// <exception cref="ArgumentException">Thrown if any of the <paramref name="parts"/> contains charachters <see cref="Path.GetInvalidPathChars"/>.</exception>
        public static string PathCombine(params string[] parts)
        {
            if (parts == null || parts.Length == 0) return null;

            string temp = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i] != null)
                    temp = Path.Combine(temp, parts[i]);
            }
            return temp;
        }

        /// <summary>
        /// Determines whether a path might escape its parent directory (by being absolute or using ..).
        /// </summary>
        public static bool IsBreakoutPath(string path)
        {
            return Path.IsPathRooted(path) || path.EndsWith("..") || path.Contains(".." + Path.DirectorySeparatorChar);
        }
        #endregion

        #region Time
        /// <summary>
        /// Converts a <see cref="DateTime"/> into the number of seconds since the Unix epoch (1970-1-1).
        /// </summary>
        public static long ToUnixTime(DateTime time)
        {
            TimeSpan timespan = (time - new DateTime(1970, 1, 1));
            return (long)timespan.TotalSeconds;
        }

        /// <summary>
        /// Converts a number of seconds since the Unix epoch (1970-1-1) into a <see cref="DateTime"/>.
        /// </summary>
        public static DateTime FromUnixTime(long time)
        {
            TimeSpan timespan = TimeSpan.FromSeconds(time);
            return new DateTime(1970, 1, 1) + timespan;
        }

        /// <summary>
        /// Determines the accuracy with which the filesystem underlying a specific directory can store file-changed times.
        /// </summary>
        /// <param name="path">The path of the directory to check.</param>
        /// <returns>The accuracy in number of seconds. (i.e. 0 = perfect, 1 = may be off by up to one second)</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified directory doesn't exist.</exception>
        /// <exception cref="IOException">Thrown if writing to the directory fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to write to the directory.</exception>
        public static int DetermineTimeAccuracy(string path)
        {
            // Prepare a file name and fake change time
            var referenceTime = new DateTime(2000, 1, 1, 0, 0, 1); // 1 second past mid-night on 1st of January 2000
            string tempFile = Path.Combine(path, Path.GetRandomFileName());

            File.WriteAllText(tempFile, @"a");
            File.SetLastWriteTimeUtc(tempFile, referenceTime);
            var resultTime = File.GetLastWriteTimeUtc(tempFile);
            File.Delete(tempFile);

            return Math.Abs((resultTime - referenceTime).Seconds);
        }
        #endregion

        #region Temp
        /// <summary>
        /// Creates a uniquely named, empty temporary file on disk and returns the full path of that file.
        /// </summary>
        /// <param name="prefix">A short string the filename should start with.</param>
        /// <returns>The full path of the newly created temporary file.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a file in <see cref="Path.GetTempPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a file in <see cref="Path.GetTempPath"/> is not permitted.</exception>
        /// <remarks>Use this method, because <see cref="Path.GetTempFileName"/> exhibits buggy behaviour in some Mono versions.</remarks>
        public static string GetTempFile(string prefix)
        {
            // Make sure there are no name collisions
            string path;
            do
            {
                path = Path.Combine(Path.GetTempPath(), prefix + '-' + Path.GetRandomFileName());
            } while (File.Exists(path));

            // Create the file to ensure nobody else uses the name
            File.WriteAllBytes(path, new byte[0]);

            return path;
        }

        /// <summary>
        /// Creates a uniquely named, empty temporary directory on disk and returns the full path of that directory.
        /// </summary>
        /// <param name="prefix">A short string the filename should start with.</param>
        /// <returns>The full path of the newly created temporary directory.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory in <see cref="Path.GetTempPath"/>.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory in <see cref="Path.GetTempPath"/> is not permitted.</exception>
        /// <remarks>Use this method, because <see cref="Path.GetTempFileName"/> exhibits buggy behaviour in some Mono versions.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Delivers a new value on each call")]
        public static string GetTempDirectory(string prefix)
        {
            string tempDir = GetTempFile(prefix);
            File.Delete(tempDir);
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }
        #endregion

        #region Copy
        /// <summary>
        /// Copies the content of a directory to a new location preserving the original file modification times.
        /// </summary>
        /// <param name="sourcePath">The path of source directory. Must exist!</param>
        /// <param name="destinationPath">The path of the target directory. Must not exist!</param>
        /// <param name="preserveDirectoryModificationTime"><see langword="true"/> to preserve the modification times for directories as well; <see langword="false"/> to preserve only the file modification times.</param>
        /// <param name="overwrite">Overwrite exisiting files and directories at the <paramref name="destinationPath"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sourcePath"/> and <paramref name="destinationPath"/> are equal.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="sourcePath"/> does not exist.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="destinationPath"/> already exists and <paramref name="overwrite"/> is <see langword="false"/>.</exception>
        public static void CopyDirectory(string sourcePath, string destinationPath, bool preserveDirectoryModificationTime, bool overwrite)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException("destinationPath");
            if (sourcePath == destinationPath) throw new ArgumentException(Resources.SourceDestinationEqual);
            if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException(Resources.SourceDirNotExist);
            if (!overwrite && Directory.Exists(destinationPath)) throw new IOException(Resources.DestinationDirExist);
            #endregion

            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

            // Copy individual files
            foreach (string sourceSubPath in Directory.GetFiles(sourcePath))
            {
                string destinationSubPath = Path.Combine(destinationPath, Path.GetFileName(sourceSubPath) ?? "");
                File.Copy(sourceSubPath, destinationSubPath, overwrite);
                File.SetLastWriteTimeUtc(destinationSubPath, File.GetLastWriteTimeUtc(sourceSubPath));
            }

            // Recurse into sub-direcories
            foreach (string sourceSubPath in Directory.GetDirectories(sourcePath))
            {
                string destinationSubPath = Path.Combine(destinationPath, Path.GetFileName(sourceSubPath) ?? "");
                CopyDirectory(sourceSubPath, destinationSubPath, preserveDirectoryModificationTime, overwrite);
            }

            if (preserveDirectoryModificationTime)
            {
                // Set directory write time as last step, since file changes within the directory may cause the OS to reset the value
                Directory.SetLastWriteTimeUtc(destinationPath, Directory.GetLastWriteTimeUtc(sourcePath));
            }
        }
        #endregion

        #region Replace
        /// <summary>
        /// Replaces one file with another. Rolls back in case of problems.
        /// </summary>
        /// <param name="sourcePath">The path of source directory. Must exist!</param>
        /// <param name="destinationPath">The path of the target directory. Must not exist!</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sourcePath"/> and <paramref name="destinationPath"/> are equal.</exception>
        /// <exception cref="IOException">Thrown if the file could not be replaced.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the read or write access to the files was denied.</exception>
        public static void Replace(string sourcePath, string destinationPath)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException("destinationPath");
            if (sourcePath == destinationPath) throw new ArgumentException(Resources.SourceDestinationEqual);
            #endregion

            // Simply move if the destination does not exist
            if (File.Exists(destinationPath))
            {
                // Prepend random string for temp file name
                string directory = Path.GetDirectoryName(Path.GetFullPath(destinationPath));
                string backupPath = directory + Path.DirectorySeparatorChar + "backup." + Path.GetRandomFileName() + "." + Path.GetFileName(destinationPath);

                try
                {
                    // Use native replacement method with temporary backup file for rollback
                    File.Replace(sourcePath, destinationPath, backupPath, true);
                    File.Delete(backupPath);
                }
                catch (PlatformNotSupportedException)
                {
                    // Emulate replacement method
                    File.Move(destinationPath, backupPath);
                    try
                    {
                        File.Move(sourcePath, destinationPath);
                        File.Delete(backupPath);
                    }
                    catch
                    {
                        // Rollback
                        File.Move(backupPath, destinationPath);
                        throw;
                    }
                }
            }
            else File.Move(sourcePath, destinationPath);
        }
        #endregion

        #region Directories
        /// <summary>
        /// Lists the names of all subdirectories contained within a directory.
        /// </summary>
        /// <param name="path">The path of the directory to search for subdirectories.</param>
        /// <returns>A C-sorted list of directory names.</returns>
        public static string[] GetSubdirectoryNames(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            var directoryNames = Array.ConvertAll(Directory.GetDirectories(path), Path.GetFileName);
            Array.Sort(directoryNames, StringComparer.Ordinal);
            return directoryNames;
        }

        /// <summary>
        /// Walks a directory structure recursivley and performs an action for every directory and file encountered.
        /// </summary>
        /// <param name="directory">The directory to walk.</param>
        /// <param name="dirAction">The action to perform for every found directory (including the starting <paramref name="directory"/>); may be <see langword="null"/>.</param>
        /// <param name="fileAction">The action to perform for every found file; may be <see langword="null"/>.</param>
        public static void WalkDirectory(DirectoryInfo directory, Action<DirectoryInfo> dirAction, Action<FileInfo> fileAction)
        {
            #region Sanity checks
            if (directory == null) throw new ArgumentNullException("directory");
            if (!directory.Exists) throw new DirectoryNotFoundException(Resources.SourceDirNotExist);
            #endregion

            if (dirAction != null) dirAction(directory);
            foreach (var subDir in directory.GetDirectories())
                WalkDirectory(subDir, dirAction, fileAction);

            if (fileAction != null)
            {
                foreach (var file in directory.GetFiles())
                    fileAction(file);
            }
        }
        #endregion

#if FS_SECURITY

        #region Permssions
        /// <summary>
        /// Uses the best means the current platform provides to prevent further write access to a directory (read-only attribute, ACLs, Unix octals, etc.).
        /// </summary>
        /// <remarks>May do nothing if the platform doesn't provide any known protection mechanisms.</remarks>
        /// <param name="path">The directory to protect.</param>
        /// <exception cref="IOException">Thrown if there was a problem applying the write protection.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to apply the write protection.</exception>
        public static void EnableWriteProtection(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(Resources.SourceDirNotExist);
            #endregion

            var directory = new DirectoryInfo(path);

            // Use only best method for platform
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    ToggleWriteProtectionUnix(directory, true);
                    break;

                case PlatformID.Win32NT:
                    ToggleWriteProtectionWinNT(directory, true);
                    break;
            }
        }

        /// <summary>
        /// Removes whatever means the current platform provides to prevent write access to a directory (read-only attribute, ACLs, Unix octals, etc.).
        /// </summary>
        /// <remarks>May do nothing if the platform doesn't provide any known protection mechanisms.</remarks>
        /// <param name="path">The directory to unprotect.</param>
        /// <exception cref="IOException">Thrown if there was a problem removing the write protection.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to remove the write protection.</exception>
        public static void DisableWriteProtection(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(Resources.SourceDirNotExist);
            #endregion

            var directory = new DirectoryInfo(path);

            // Disable all applicable methods
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    ToggleWriteProtectionUnix(directory, false);
                    break;

                case PlatformID.Win32NT:
                    ToggleWriteProtectionWinNT(directory, false);

                    // Remove any read-only attributes
                    try
                    {
                        WalkDirectory(directory, dir => dir.Attributes = FileAttributes.Normal, file => file.IsReadOnly = false);
                    }
                    catch (ArgumentException)
                    {}
                    break;
            }
        }

        #region Helpers
        private static void ToggleWriteProtectionUnix(DirectoryInfo directory, bool enable)
        {
            try
            {
                if (enable) WalkDirectory(directory, subDir => MonoUtils.MakeReadOnly(subDir.FullName), file => MonoUtils.MakeReadOnly(file.FullName));
                else WalkDirectory(directory, subDir => MonoUtils.MakeWritable(subDir.FullName), file => MonoUtils.MakeWritable(file.FullName));
            }
                #region Error handling
            catch (InvalidOperationException ex)
            {
                throw new IOException(Resources.UnixSubsystemFail, ex);
            }
            catch (IOException ex)
            {
                throw new IOException(Resources.UnixSubsystemFail, ex);
            }
            #endregion
        }

        private static void ToggleWriteProtectionWinNT(DirectoryInfo directory, bool enable)
        {
            // Add ACL to directory: Everyone = Deny write
            DirectorySecurity security = directory.GetAccessControl();
            var denyEveryoneWrite = new FileSystemAccessRule(new SecurityIdentifier("S-1-1-0" /*Everyone*/), FileSystemRights.Write, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny);
            if (enable) security.AddAccessRule(denyEveryoneWrite);
            else security.RemoveAccessRule(denyEveryoneWrite);
            directory.SetAccessControl(security);
        }
        #endregion

        #endregion

        #region Unix
        /// <summary>
        /// Checks whether a file is a regular file (i.e. not a device file, symbolic link, etc.).
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a regular file; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files.</remarks>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to query the file's properties.</exception>
        public static bool IsRegularFile(string path)
        {
            if (!File.Exists(path)) return false;

            // ToDo: Detect special files on Windows
            if (WindowsUtils.IsWindows)
                return true;

            if (MonoUtils.IsUnix)
            {
                try
                {
                    return MonoUtils.IsRegularFile(path);
                }
                    #region Error handling
                catch (InvalidOperationException ex)
                {
                    throw new IOException(Resources.UnixSubsystemFail, ex);
                }
                catch (IOException ex)
                {
                    throw new IOException(Resources.UnixSubsystemFail, ex);
                }
                #endregion
            }

            return true;
        }

        /// <summary>
        /// Checks whether a file is a Unix symbolic link.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <param name="target">Returns the target the symbolic link points to if it exists.</param>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a symbolic link; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files. Will always return <see langword="false"/> on non-Unixoid systems.</remarks>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to query the file's properties.</exception>
        public static bool IsSymlink(string path, out string target)
        {
            if (File.Exists(path) && MonoUtils.IsUnix)
            {
                try
                {
                    return MonoUtils.IsSymlink(path, out target);
                }
                    #region Error handling
                catch (InvalidOperationException ex)
                {
                    throw new IOException(Resources.UnixSubsystemFail, ex);
                }
                catch (IOException ex)
                {
                    throw new IOException(Resources.UnixSubsystemFail, ex);
                }
                #endregion
            }

            // Return default values
            target = null;
            return false;
        }

        /// <summary>
        /// Creates a new Unix symbolic link. Only works on Unixoid systems!
        /// </summary>
        /// <param name="path">The path of the file to create.</param>
        /// <param name="target">The target the symbolic link shall point to relative to <paramref name="path"/>.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a non-Unixoid system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to create the symbolic link.</exception>
        public static void CreateSymlink(string path, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            if (!MonoUtils.IsUnix) throw new PlatformNotSupportedException();

            try
            {
                MonoUtils.CreateSymlink(path, target);
            }
                #region Error handling
            catch (InvalidOperationException ex)
            {
                throw new IOException(Resources.UnixSubsystemFail, ex);
            }
            catch (IOException ex)
            {
                throw new IOException(Resources.UnixSubsystemFail, ex);
            }
            #endregion
        }

        /// <summary>
        /// Checks whether a file is marked as Unix-executable.
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to an executable; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files. Will always return <see langword="false"/> on non-Unixoid systems.</remarks>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to query the file's properties.</exception>
        public static bool IsExecutable(string path)
        {
            if (!File.Exists(path) || !MonoUtils.IsUnix) return false;

            try
            {
                return MonoUtils.IsExecutable(path);
            }
                #region Error handling
            catch (InvalidOperationException ex)
            {
                throw new IOException(Resources.UnixSubsystemFail, ex);
            }
            catch (IOException ex)
            {
                throw new IOException(Resources.UnixSubsystemFail, ex);
            }
            #endregion
        }

        /// <summary>
        /// Marks a file as Unix-executable or not Unix-executable. Only works on Unixoid systems!
        /// </summary>
        /// <param name="path">The file to mark as executable or not executable.</param>
        /// <param name="executable"><see lang="true"/> to mark the file as executable, <see lang="true"/> to mark it as not executable.</param>
        /// <exception cref="FileNotFoundException">Thrown if <paramref name="path"/> points to a file that does not exist or cannot be accessed.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a non-Unixoid system.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to change the file's properties.</exception>
        public static void SetExecutable(string path, bool executable)
        {
            #region Sanity checks
            if (!File.Exists(path)) throw new FileNotFoundException("", path);
            if (!MonoUtils.IsUnix) throw new PlatformNotSupportedException();
            #endregion

            try
            {
                MonoUtils.SetExecutable(path, executable);
            }
                #region Error handling
            catch (InvalidOperationException ex)
            {
                throw new IOException(Resources.UnixSubsystemFail, ex);
            }
            catch (IOException ex)
            {
                throw new IOException(Resources.UnixSubsystemFail, ex);
            }
            #endregion
        }
        #endregion

#endif
    }
}
