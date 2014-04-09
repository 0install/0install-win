/*
 * Copyright 2006-2014 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using NanoByte.Common.Properties;
using NanoByte.Common.Values;

namespace NanoByte.Common.Utils
{
    /// <summary>
    /// Provides filesystem-related helper methods.
    /// </summary>
    public static class FileUtils
    {
        #region Paths
        /// <summary>
        /// Replaces Unix-style directory slashes with <see cref="Path.DirectorySeparatorChar"/>.
        /// </summary>
        public static string UnifySlashes(string value)
        {
            if (value == null) return null;
            return value.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Determines whether a path might escape its parent directory (by being absolute or using ..).
        /// </summary>
        public static bool IsBreakoutPath(string path)
        {
            #region Sanity checks
            if (path == null) throw new ArgumentNullException("path");
            #endregion

            path = UnifySlashes(path);
            return Path.IsPathRooted(path) || path.Split(Path.DirectorySeparatorChar).Contains("..");
        }

        /// <summary>
        /// Returns a relative path pointing to <paramref name="targetPath"/> from <paramref name="basePath"/> using Unix-style directory separators.
        /// </summary>
        public static string RelativeTo(this FileSystemInfo targetPath, FileSystemInfo basePath)
        {
            #region Sanity checks
            if (targetPath == null) throw new ArgumentNullException("targetPath");
            if (basePath == null) throw new ArgumentNullException("basePath");
            #endregion

            string trimmed = targetPath.FullName.Substring(basePath.FullName.Length);
            if (trimmed.StartsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture))) trimmed = trimmed.Substring(1);
            return trimmed.Replace(Path.DirectorySeparatorChar, '/');
        }
        #endregion

        #region Exists
        /// <summary>
        /// Like <see cref="File.Exists"/> but case-sensitive, even on Windows.
        /// </summary>
        public static bool ExistsCaseSensitive(string path)
        {
            return File.Exists(path) &&
                   // Make sure the file found is a string-exact match
                   Directory.GetFiles(Path.GetDirectoryName(path) ?? Environment.CurrentDirectory, Path.GetFileName(path) ?? "").Contains(path);
        }
        #endregion

        #region Time
        /// <summary>
        /// Converts a <see cref="DateTime"/> into the number of seconds since the Unix epoch (1970-1-1).
        /// </summary>
        public static long ToUnixTime(this DateTime time)
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

        #region Replace
        /// <summary>
        /// Replaces one file with another. Rolls back in case of problems.
        /// </summary>
        /// <param name="sourcePath">The path of source directory.</param>
        /// <param name="destinationPath">The path of the target directory. Must reside on the same file system as <paramref name="sourcePath"/>.</param>
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

            // Prepend random string for temp file name
            string directory = Path.GetDirectoryName(Path.GetFullPath(destinationPath));
            string backupPath = directory + Path.DirectorySeparatorChar + "backup." + Path.GetRandomFileName() + "." + Path.GetFileName(destinationPath);

            if (WindowsUtils.IsWindowsNT)
            {
                if (File.Exists(destinationPath))
                {
                    File.Replace(sourcePath, destinationPath, backupPath, ignoreMetadataErrors: true);
                    File.Delete(backupPath);
                }
                else File.Move(sourcePath, destinationPath);
            }
            else if (UnixUtils.IsUnix) UnixUtils.Rename(sourcePath, destinationPath);
            else
            {
                if (File.Exists(destinationPath)) File.Move(destinationPath, backupPath);
                try
                {
                    File.Move(sourcePath, destinationPath);
                    if (File.Exists(backupPath)) File.Delete(backupPath);
                }
                catch
                { // Rollback
                    if (File.Exists(backupPath)) File.Move(backupPath, destinationPath);
                    throw;
                }
            }
        }
        #endregion

        #region Read
        /// <summary>
        /// Reads the first line of text from a file.
        /// </summary>
        /// <param name="file">The file to read from.</param>
        /// <param name="encoding">The text encoding to use for reading.</param>
        /// <returns>The first line of text in the file; <see langword="null"/> if decoding does not work on the contents.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        public static string ReadFirstLine(this FileInfo file, Encoding encoding)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            if (encoding == null) throw new ArgumentNullException("encoding");
            #endregion

            using (var stream = file.OpenRead())
                return new StreamReader(stream, encoding).ReadLine();
        }
        #endregion

        #region Directories
        /// <summary>
        /// Lists the paths of all subdirectories contained within a directory.
        /// </summary>
        /// <param name="path">The path of the directory to search for subdirectories.</param>
        /// <returns>A C-sorted list of directory paths.</returns>
        public static IEnumerable<string> GetDirectories(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            var paths = Directory.GetDirectories(path);
            Array.Sort(paths, StringComparer.Ordinal);
            return paths;
        }

        /// <summary>
        /// Walks a directory structure recursivley and performs an action for every directory and file encountered.
        /// </summary>
        /// <param name="directory">The directory to walk.</param>
        /// <param name="dirAction">The action to perform for every found directory (including the starting <paramref name="directory"/>); may be <see langword="null"/>.</param>
        /// <param name="fileAction">The action to perform for every found file; may be <see langword="null"/>.</param>
        public static void Walk(this DirectoryInfo directory, Action<DirectoryInfo> dirAction = null, Action<FileInfo> fileAction = null)
        {
            #region Sanity checks
            if (directory == null) throw new ArgumentNullException("directory");
            if (!directory.Exists) throw new DirectoryNotFoundException(Resources.SourceDirNotExist);
            #endregion

            if (dirAction != null) dirAction(directory);

            if (fileAction != null)
            {
                foreach (var file in directory.GetFiles())
                    fileAction(file);
            }

            foreach (var subDir in directory.GetDirectories())
                Walk(subDir, dirAction, fileAction);
        }
        #endregion

        #region ACLs
        /// <summary>
        /// Removes any custom ACLs a user may have set, restores ACL inheritance and sets the Administrators group as the owner.
        /// </summary>
        public static void ResetAcl(this DirectoryInfo directoryInfo)
        {
            directoryInfo.Walk(
                dir => ResetAcl(dir.GetAccessControl, dir.SetAccessControl),
                file => ResetAcl(file.GetAccessControl, file.SetAccessControl));
        }

        /// <summary>
        /// Helper method for <see cref="ResetAcl(DirectoryInfo)"/>.
        /// </summary>
        private static void ResetAcl<T>(Func<T> getAcl, Action<T> setAcl) where T : FileSystemSecurity
        {
            // Give ownership to administrators
            var acl = getAcl();
            acl.SetOwner(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null));
            setAcl(acl);

            // Inherit rules from container and remove any custom rules
            acl = getAcl();
            acl.CanonicalizeAcl();
            acl.SetAccessRuleProtection(isProtected: false, preserveInheritance: true);
            foreach (FileSystemAccessRule rule in acl.GetAccessRules(true, false, typeof(NTAccount)))
                acl.RemoveAccessRule(rule);
            setAcl(acl);
        }

        /// <summary>
        /// Fixes ACLs that are not canonical (not ordered correctly).
        /// </summary>
        public static void CanonicalizeAcl(this ObjectSecurity objectSecurity)
        {
            #region Sanity checks
            if (objectSecurity == null) throw new ArgumentNullException("objectSecurity");
            #endregion

            if (objectSecurity.AreAccessRulesCanonical) return;

            var securityDescriptor = new RawSecurityDescriptor(objectSecurity.GetSecurityDescriptorSddlForm(AccessControlSections.Access));
            var denied = new List<CommonAce>();
            var deniedObject = new List<CommonAce>();
            var allowed = new List<CommonAce>();
            var allowedObject = new List<CommonAce>();
            var inherited = new List<CommonAce>();
            foreach (var ace in securityDescriptor.DiscretionaryAcl.Cast<CommonAce>())
            {
                if (ace.AceFlags.HasFlag(AceFlags.Inherited)) inherited.Add(ace);
                else
                {
                    switch (ace.AceType)
                    {
                        case AceType.AccessDenied:
                            denied.Add(ace);
                            break;
                        case AceType.AccessDeniedObject:
                            deniedObject.Add(ace);
                            break;
                        case AceType.AccessAllowed:
                            allowed.Add(ace);
                            break;
                        case AceType.AccessAllowedObject:
                            allowedObject.Add(ace);
                            break;
                    }
                }
            }

            int aceIndex = 0;
            var newDacl = new RawAcl(securityDescriptor.DiscretionaryAcl.Revision, securityDescriptor.DiscretionaryAcl.Count);
            denied.ForEach(ace => newDacl.InsertAce(aceIndex++, ace));
            deniedObject.ForEach(ace => newDacl.InsertAce(aceIndex++, ace));
            allowed.ForEach(ace => newDacl.InsertAce(aceIndex++, ace));
            allowedObject.ForEach(ace => newDacl.InsertAce(aceIndex++, ace));
            inherited.ForEach(ace => newDacl.InsertAce(aceIndex++, ace));

            if (aceIndex != securityDescriptor.DiscretionaryAcl.Count) throw new InvalidOperationException(Resources.CannotCanonicalizeDacl);
            securityDescriptor.DiscretionaryAcl = newDacl;
            objectSecurity.SetSecurityDescriptorSddlForm(securityDescriptor.GetSddlForm(AccessControlSections.Access), AccessControlSections.Access);
        }
        #endregion

        #region Write protection
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

            if (UnixUtils.IsUnix) directory.ToggleWriteProtectionUnix(true);
            else if (WindowsUtils.IsWindowsNT) directory.ToggleWriteProtectionWinNT(true);
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

            if (UnixUtils.IsUnix) directory.ToggleWriteProtectionUnix(false);
            else if (WindowsUtils.IsWindows)
            {
                if (WindowsUtils.IsWindowsNT) directory.ToggleWriteProtectionWinNT(false);

                // Remove classic read-only attributes
                try
                {
                    Walk(directory,
                        dir => dir.Attributes = FileAttributes.Normal,
                        file => file.IsReadOnly = false);
                }
                catch (ArgumentException)
                {}
            }
        }

        #region Helpers
        private static void ToggleWriteProtectionUnix(this DirectoryInfo directory, bool enable)
        {
            try
            {
                if (enable) Walk(directory, dir => UnixUtils.MakeReadOnly(dir.FullName), file => UnixUtils.MakeReadOnly(file.FullName));
                else Walk(directory, dir => UnixUtils.MakeWritable(dir.FullName), file => UnixUtils.MakeWritable(file.FullName));
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

        private static readonly FileSystemAccessRule _denyEveryoneWrite = new FileSystemAccessRule(new SecurityIdentifier("S-1-1-0" /*Everyone*/), FileSystemRights.Write | FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny);

        private static void ToggleWriteProtectionWinNT(this DirectoryInfo directory, bool enable)
        {
            try
            {
                var acl = directory.GetAccessControl();
                acl.CanonicalizeAcl();
                if (enable) acl.AddAccessRule(_denyEveryoneWrite);
                else acl.RemoveAccessRule(_denyEveryoneWrite);
                directory.SetAccessControl(acl);
            }
            catch (ArgumentException)
            {
                // WORKAROUND: .NET API glitch
            }
        }
        #endregion

        #endregion

        #region Links
        /// <summary>
        /// Creates a new symbolic link to a file or directory.
        /// </summary>
        /// <param name="source">The path of the link to create.</param>
        /// <param name="target">The path of the existing file or directory to point to (relative to <paramref name="source"/>).</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a system with no symbolic link support.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to create the symbolic link.</exception>
        public static void CreateSymlink(string source, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            if (UnixUtils.IsUnix)
            {
                try
                {
                    UnixUtils.CreateSymlink(source, target);
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
            else if (WindowsUtils.IsWindowsVista)
            {
                try
                {
                    WindowsUtils.CreateSymlink(source, target);
                }
                    #region Error handling
                catch (Win32Exception ex)
                {
                    throw new IOException(ex.Message, ex);
                }
                #endregion
            }
            else throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Creates a new hard link between two files.
        /// </summary>
        /// <param name="source">The path of the link to create.</param>
        /// <param name="target">The absolute path of the existing file to point to.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a system with no hard link support.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to create the hard link.</exception>
        public static void CreateHardlink(string source, string target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(source)) throw new ArgumentNullException("source");
            if (string.IsNullOrEmpty(target)) throw new ArgumentNullException("target");
            #endregion

            if (UnixUtils.IsUnix)
            {
                try
                {
                    UnixUtils.CreateHardlink(source, target);
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
            else if (WindowsUtils.IsWindowsNT)
            {
                try
                {
                    WindowsUtils.CreateHardlink(source, target);
                }
                    #region Error handling
                catch (Win32Exception ex)
                {
                    throw new IOException(ex.Message, ex);
                }
                #endregion
            }
            else throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Determines whether to files are hardlinked.
        /// </summary>
        /// <param name="path1">The path of the first file.</param>
        /// <param name="path2">The path of the second file.</param>
        public static bool AreHardlinked(string path1, string path2)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path1)) throw new ArgumentNullException("path1");
            if (string.IsNullOrEmpty(path2)) throw new ArgumentNullException("path2");
            #endregion

            if (UnixUtils.IsUnix)
            {
                try
                {
                    return UnixUtils.AreHardlinked(path1, path2);
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
            else if (WindowsUtils.IsWindowsNT)
            {
                try
                {
                    return WindowsUtils.AreHardlinked(path1, path2);
                }
                    #region Error handling
                catch (Win32Exception ex)
                {
                    throw new IOException(ex.Message, ex);
                }
                #endregion
            }
            else return false;
        }
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

            // TODO: Detect special files on Windows
            if (!UnixUtils.IsUnix) return true;

            try
            {
                return UnixUtils.IsRegularFile(path);
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
        /// Checks whether a file is a Unix symbolic link.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a symbolic link; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files. Will always return <see langword="false"/> on non-Unixoid systems.</remarks>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to query the file's properties.</exception>
        public static bool IsSymlink(string path)
        {
            if ((!File.Exists(path) && !Directory.Exists(path)) || !UnixUtils.IsUnix) return false;

            try
            {
                return UnixUtils.IsSymlink(path);
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
        /// Checks whether a file is a Unix symbolic link.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <param name="target">Returns the target the symbolic link points to if it exists.</param>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a symbolic link; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files. Will always return <see langword="false"/> on non-Unixoid systems.</remarks>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to query the file's properties.</exception>
        public static bool IsSymlink(string path, out string target)
        {
            if ((!File.Exists(path) && !Directory.Exists(path)) || !UnixUtils.IsUnix)
            {
                target = null;
                return false;
            }

            try
            {
                return UnixUtils.IsSymlink(path, out target);
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
            if (!File.Exists(path) || !UnixUtils.IsUnix) return false;

            try
            {
                return UnixUtils.IsExecutable(path);
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
            if (!UnixUtils.IsUnix) throw new PlatformNotSupportedException();
            #endregion

            try
            {
                UnixUtils.SetExecutable(path, executable);
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
    }
}
