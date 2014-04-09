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
using System.IO;
using System.Linq;
using NanoByte.Common.Properties;
using NanoByte.Common.Utils;

namespace NanoByte.Common.Tasks
{
    /// <summary>
    /// Copies the content of a directory to a new location preserving the original file modification times.
    /// </summary>
    public class CopyDirectory : TaskBase
    {
        /// <inheritdoc/>
        public override string Name { get { return Resources.CopyFiles; } }

        /// <inheritdoc/>
        protected override bool UnitsByte { get { return true; } }

        /// <summary>
        /// The path of source directory. Must exist!
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        /// The path of the target directory. May exist. Must be empty if <see cref="Overwrite"/> is <see langword="false"/>.
        /// </summary>
        public string DestinationPath { get; private set; }

        /// <summary>
        /// <see langword="true"/> to preserve the modification times for directories as well; <see langword="false"/> to preserve only the file modification times.
        /// </summary>
        public bool PreserveDirectoryTimestamps { get; set; }

        /// <summary>
        /// Overwrite exisiting files and directories at the <see cref="DestinationPath"/>. This will even replace read-only files!
        /// </summary>
        public bool Overwrite { get; private set; }

        /// <summary>
        /// Creates a new directory copy task.
        /// </summary>
        /// <param name="sourcePath">The path of source directory. Must exist!</param>
        /// <param name="destinationPath">The path of the target directory. May exist. Must be empty if <paramref name="overwrite"/> is <see langword="false"/>.</param>
        /// <param name="preserveDirectoryTimestamps"><see langword="true"/> to preserve the modification times for directories as well; <see langword="false"/> to preserve only the file modification times.</param>
        /// <param name="overwrite">Overwrite exisiting files and directories at the <paramref name="destinationPath"/>. This will even replace read-only files!</param>
        public CopyDirectory(string sourcePath, string destinationPath, bool preserveDirectoryTimestamps = true, bool overwrite = false)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException("destinationPath");
            if (sourcePath == destinationPath) throw new ArgumentException(Resources.SourceDestinationEqual);
            #endregion

            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            PreserveDirectoryTimestamps = preserveDirectoryTimestamps;
            Overwrite = overwrite;
        }

        private DirectoryInfo _source, _destination;

        /// <inheritdoc/>
        protected override void Execute()
        {
            _source = new DirectoryInfo(SourcePath);
            _destination = new DirectoryInfo(DestinationPath);
            if (!_source.Exists) throw new DirectoryNotFoundException(Resources.SourceDirNotExist);
            if (_destination.Exists)
            { // Fail if overwrite is off but the target directory already exists and contains elements
                if (!Overwrite && _destination.GetFileSystemInfos().Length > 0)
                    throw new IOException(Resources.DestinationDirExist);
            }
            else _destination.Create();

            Status = TaskStatus.Header;
            var sourceDirectories = new List<DirectoryInfo>();
            var sourceFiles = new List<FileInfo>();
            _source.Walk(sourceDirectories.Add, sourceFiles.Add);
            UnitsTotal = sourceFiles.Sum(file => file.Length);

            Status = TaskStatus.Data;
            CopyDirectories(sourceDirectories);
            CopyFiles(sourceFiles);
            if (PreserveDirectoryTimestamps)
                CopyDirectoryTimestamps(sourceDirectories);
            Status = TaskStatus.Complete;
        }

        private void CopyDirectories(IEnumerable<DirectoryInfo> sourceDirectories)
        {
            foreach (var sourceDirectory in sourceDirectories)
            {
                CancellationToken.ThrowIfCancellationRequested();
                Directory.CreateDirectory(PathInDestination(sourceDirectory));
            }
        }

        private void CopyFiles(IEnumerable<FileInfo> sourceFiles)
        {
            foreach (var sourceFile in sourceFiles)
            {
                CancellationToken.ThrowIfCancellationRequested();

                var destinationFile = new FileInfo(PathInDestination(sourceFile));
                if (destinationFile.Exists)
                {
                    if (!Overwrite)
                    {
                        UnitsProcessed += sourceFile.Length;
                        continue;
                    }
                    if (destinationFile.IsReadOnly) destinationFile.IsReadOnly = false;
                }

                CopyFile(sourceFile, destinationFile);

                destinationFile.Refresh();
                if (destinationFile.IsReadOnly) destinationFile.IsReadOnly = false;
                destinationFile.LastWriteTimeUtc = sourceFile.LastWriteTimeUtc;

                UnitsProcessed += sourceFile.Length;
            }
        }

        protected virtual void CopyFile(FileInfo sourceFile, FileInfo destinationFile)
        {
            #region Sanity checks
            if (sourceFile == null) throw new ArgumentNullException("sourceFile");
            if (destinationFile == null) throw new ArgumentNullException("destinationFile");
            #endregion

            sourceFile.CopyTo(destinationFile.FullName, Overwrite);
        }

        private void CopyDirectoryTimestamps(IEnumerable<DirectoryInfo> sourceDirectories)
        {
            foreach (var sourceDirectory in sourceDirectories)
            {
                CancellationToken.ThrowIfCancellationRequested();

                Directory.SetLastWriteTimeUtc(PathInDestination(sourceDirectory), sourceDirectory.LastWriteTimeUtc);
            }
        }

        private string PathInDestination(FileSystemInfo element)
        {
            return Path.Combine(DestinationPath, element.RelativeTo(_source).Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
