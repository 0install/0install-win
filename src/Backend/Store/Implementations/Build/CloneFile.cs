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
using System.IO;
using JetBrains.Annotations;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Copies the content of a single file to a new location preserving the original file modification time, executable bit and/or symlink status.
    /// </summary>
    public class CloneFile : CloneDirectory
    {
        /// <summary>
        /// The name of the original file to read without any directory information.
        /// </summary>
        [NotNull]
        public string SourceFileName { get; }

        /// <summary>
        /// The name of the new file to write without any directory information.
        /// </summary>
        [NotNull]
        public string TargetFileName { get; set; }

        /// <summary>
        /// Creates a new file cloning task.
        /// </summary>
        /// <param name="sourceFilePath">The path of the original file to read.</param>
        /// <param name="targetDirPath">The path of the new directory to clone the file to.</param>
        public CloneFile([NotNull] string sourceFilePath, [NotNull] string targetDirPath)
            : base(Path.GetDirectoryName(sourceFilePath), targetDirPath)
        {
            SourceFileName = Path.GetFileName(sourceFilePath);
            TargetFileName = SourceFileName;
        }

        /// <inheritdoc/>
        protected override void HandleFile(FileInfo file, bool executable = false)
        {
            if ((file ?? throw new ArgumentNullException(nameof(file))).Name == SourceFileName)
                base.HandleFile(file, executable);
        }

        /// <inheritdoc/>
        protected override void HandleSymlink(FileSystemInfo symlink, string target)
        {
            if ((symlink ?? throw new ArgumentNullException(nameof(symlink))) .Name == SourceFileName)
                DirectoryBuilder.CreateSymlink(TargetFileName, target ?? throw new ArgumentNullException(nameof(target)));
        }

        /// <inheritdoc/>
        protected override string NewFilePath(FileInfo file, bool executable)
            => DirectoryBuilder.NewFilePath(
                TargetFileName,
                (file ?? throw new ArgumentNullException(nameof(file))).LastWriteTimeUtc, executable);
    }
}