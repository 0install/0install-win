/*
 * Copyright 2010-2015 Bastian Eicher
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
using System.Text;
using NanoByte.Common.Storage;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Copies the content of a directory to a new location preserving the original file modification times.
    /// Uses <see cref="FlagUtils"/> to preserve executable bits and symlinks if the destination filesystem cannot hold them.
    /// </summary>
    public class CopyDirectoryPosix : CopyDirectory
    {
        /// <summary>Indicates whether <see cref="CopyDirectory.SourcePath"/> is located on a filesystem with support for Unixoid features such as executable bits.</summary>
        private readonly bool _sourceIsUnixFS;

        /// <summary>Indicates whether <see cref="CopyDirectory.DestinationPath"/> is located on a filesystem with support for Unixoid features such as executable bits.</summary>
        private readonly bool _destinationIsUnixFS;

        public CopyDirectoryPosix(string sourcePath, string destinationPath, bool preserveDirectoryTimestamps = true, bool overwrite = false)
            : base(sourcePath, destinationPath, preserveDirectoryTimestamps, overwrite)
        {
            _sourceIsUnixFS = FlagUtils.IsUnixFS(SourcePath);
            _destinationIsUnixFS = FlagUtils.IsUnixFS(DestinationPath);
        }

        protected override void CopyFile(FileInfo sourceFile, FileInfo destinationFile)
        {
            #region Sanity checks
            if (sourceFile == null) throw new ArgumentNullException("sourceFile");
            if (destinationFile == null) throw new ArgumentNullException("destinationFile");
            #endregion

            base.CopyFile(sourceFile, destinationFile);

            if (_sourceIsUnixFS && !_destinationIsUnixFS && FileUtils.IsExecutable(sourceFile.FullName))
                FlagUtils.Set(Path.Combine(DestinationPath, FlagUtils.XbitFile), destinationFile.RelativeTo(new DirectoryInfo(DestinationPath)));
        }

        protected override void CreateSymlink(string linkPath, string linkTarget)
        {
            if (_destinationIsUnixFS) base.CreateSymlink(linkPath, linkTarget);
            else
            {
                File.WriteAllText(linkPath, linkTarget, Encoding.UTF8);
                FlagUtils.Set(Path.Combine(DestinationPath, FlagUtils.SymlinkFile), new FileInfo(linkPath).RelativeTo(new DirectoryInfo(DestinationPath)));
            }
        }
    }
}
