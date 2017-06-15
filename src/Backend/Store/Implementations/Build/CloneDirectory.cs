/*
 * Copyright 2010-2017 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Copies the content of a directory to a new location preserving the original file modification times, executable bits and symlinks.
    /// </summary>
    public class CloneDirectory : DirectoryTaskBase
    {
        /// <summary>Used to build the target directory with support for flag files.</summary>
        protected readonly DirectoryBuilder DirectoryBuilder;

        /// <summary>
        /// The path to the directory to clone to.
        /// </summary>
        [NotNull]
        public string TargetPath => DirectoryBuilder.TargetPath;

        /// <summary>
        /// Sub-path to be appended to <see cref="TargetPath"/> without affecting location of flag files; <c>null</c> for none.
        /// </summary>
        [CanBeNull]
        public string TargetSuffix { get => DirectoryBuilder.TargetSuffix; set => DirectoryBuilder.TargetSuffix = value; }

        /// <summary>
        /// Use hardlinks instead of copying files when possible.
        /// Only use this if you are sure the source files will not be modified!
        /// </summary>
        public bool UseHardlinks { get; set; }

        /// <summary>
        /// Creates a new directory cloning task.
        /// </summary>
        /// <param name="sourcePath">The path of the original directory to read.</param>
        /// <param name="targetPath">The path of the new directory to create.</param>
        public CloneDirectory([NotNull] string sourcePath, string targetPath) : base(sourcePath) => DirectoryBuilder = new DirectoryBuilder(targetPath);

        /// <inheritdoc/>
        public override string Name => Resources.CopyFiles;

        /// <inheritdoc/>
        protected override void HandleEntries(IEnumerable<FileSystemInfo> entries)
        {
            DirectoryBuilder.Initialize();

            using (TryUnsealImplementation())
            {
                base.HandleEntries(entries);
                DirectoryBuilder.CompletePending();
            }

            // Tries to remove write-protection on the directory, if it is located in a Store to allow creating hardlinks pointing into it.
            IDisposable TryUnsealImplementation()
            {
                string path = StoreUtils.DetectImplementationPath(SourceDirectory.FullName);
                if (path == null) return null;

                try
                {
                    FileUtils.DisableWriteProtection(path);
                    return new Disposable(() =>
                    {
                        try
                        {
                            FileUtils.EnableWriteProtection(path);
                        }
                            #region Error handling
                        catch (IOException ex)
                        {
                            Log.Info("Unable to restore write protection after creating hardlinks");
                            Log.Error(ex);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Log.Info("Unable to restore write protection after creating hardlinks");
                            Log.Error(ex);
                        }
                        catch (InvalidOperationException ex)
                        {
                            Log.Info("Unable to restore write protection after creating hardlinks");
                            Log.Error(ex);
                        }
                        #endregion
                    });
                }
                    #region Error handling
                catch (IOException ex)
                {
                    Log.Info("Unable to remove write protection for creating hardlinks");
                    Log.Info(ex);
                    return null;
                }
                catch (UnauthorizedAccessException ex)
                {
                    Log.Info("Unable to remove write protection for creating hardlinks");
                    Log.Info(ex);
                    return null;
                }
                catch (InvalidOperationException ex)
                {
                    Log.Info("Unable to remove write protection for creating hardlinks");
                    Log.Info(ex);
                    return null;
                }
                #endregion
            }
        }

        /// <inheritdoc/>
        protected override void HandleFile(FileInfo file, bool executable = false)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException(nameof(file));
            #endregion

            if (UseHardlinks)
                CopyFileAsHardlink(file.FullName, NewFilePath(file, executable));
            else
                CopyFile(file.FullName, NewFilePath(file, executable));

            void CopyFileAsHardlink(string existingPath, string newPath)
            {
                try
                {
                    FileUtils.CreateHardlink(newPath, existingPath);
                }
                catch (PlatformNotSupportedException)
                {
                    CopyFile(existingPath, newPath);
                }
                catch (UnauthorizedAccessException)
                {
                    CopyFile(existingPath, newPath);
                }
            }

            void CopyFile(string existingPath, string newPath)
            {
                using (var readStream = File.OpenRead(existingPath))
                using (var writeStream = File.Create(newPath))
                    readStream.CopyToEx(writeStream, cancellationToken: CancellationToken);
            }
        }

        /// <summary>
        /// Prepares a new file path in the directory without creating the file itself yet.
        /// </summary>
        /// <param name="file">The original file to base the new one on.</param>
        /// <param name="executable"><c>true</c> if the file's executable bit is to be set; <c>false</c> otherwise.</param>
        /// <returns>An absolute file path.</returns>
        protected virtual string NewFilePath(FileInfo file, bool executable)
            => DirectoryBuilder.NewFilePath(
                (file ?? throw new ArgumentNullException(nameof(file))).RelativeTo(SourceDirectory),
                file.LastWriteTimeUtc,
                executable);

        /// <inheritdoc/>
        protected override void HandleSymlink(FileSystemInfo symlink, string target)
            => DirectoryBuilder.CreateSymlink(
                (symlink ?? throw new ArgumentNullException(nameof(symlink))).RelativeTo(SourceDirectory),
                target ?? throw new ArgumentNullException(nameof(target)));

        /// <inheritdoc/>
        protected override void HandleDirectory(DirectoryInfo directory)
            => DirectoryBuilder.CreateDirectory(
                (directory ?? throw new ArgumentNullException(nameof(directory))).RelativeTo(SourceDirectory),
                directory.LastWriteTimeUtc);
    }
}