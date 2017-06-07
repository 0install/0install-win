/*
 * Copyright 2010-2016 Bastian Eicher
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

using System.IO;
using FluentAssertions;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using Xunit;
using ZeroInstall.Store.Implementations.Manifests;

namespace ZeroInstall.Store.Implementations.Deployment
{
    /// <summary>
    /// Contains test methods for <see cref="ClearDirectory"/>.
    /// </summary>
    public class ClearDirectoryTest : DirectoryOperationTestBase
    {
        [Fact]
        public void StageAndCommit()
        {
            string manifestPath = Path.Combine(TempDir, Manifest.ManifestFile);
            FileUtils.Touch(manifestPath); // Content of Manifest file is not read, in-memory Manifest is used instead

            using (var operation = new ClearDirectory(TempDir, Manifest, new SilentTaskHandler()))
            {
                operation.Stage();
                File.Exists(manifestPath).Should().BeFalse(because: "Original manifest file should be gone after staging.");
                File.Exists(File1Path).Should().BeFalse(because: "Original file should be gone after staging.");
                Directory.Exists(SubdirPath).Should().BeTrue(because: "Directories should be left intact after staging.");
                File.Exists(File2Path).Should().BeFalse(because: "Original file should be gone after staging.");
                Directory.GetFileSystemEntries(TempDir).Length.Should().Be(3, because: "Backup files should be preset after staging.");
                Directory.GetFileSystemEntries(SubdirPath).Length.Should().Be(1, because: "Backup files should be preset after staging.");

                operation.Commit();
            }

            Directory.Exists(TempDir).Should().BeFalse(because: "Entire directory should be gone after commit.");
        }

        [Fact]
        public void StageAndRollBack()
        {
            using (var operation = new ClearDirectory(TempDir, Manifest, new SilentTaskHandler()))
            {
                operation.Stage();
                // Missing .Commit() automatically triggers rollback
            }

            File.Exists(File1Path).Should().BeTrue(because: "Original file should be back after rollback.");
            File.Exists(File2Path).Should().BeTrue(because: "Original file should be back after rollback.");
        }

        [SkippableFact]
        public void ReadOnlyAttribute()
        {
            Skip.IfNot(WindowsUtils.IsWindows, "Read-only file attribute is only available on Windows");

            new FileInfo(File1Path).IsReadOnly = true;

            using (var operation = new ClearDirectory(TempDir, Manifest, new SilentTaskHandler()))
            {
                operation.Stage();
                operation.Commit();
            }

            Directory.Exists(TempDir).Should().BeFalse(because: "Entire directory should be gone after commit.");
        }

        [Fact]
        public void UntrackedFilesInTarget()
        {
            FileUtils.Touch(Path.Combine(TempDir, "untracked"));

            using (var operation = new ClearDirectory(TempDir, Manifest, new SilentTaskHandler()))
            {
                operation.Stage();
                operation.Commit();
            }

            Directory.GetFileSystemEntries(TempDir).Length.Should().Be(1, because: "Only untracked file should be left after commit.");
        }

        [Fact]
        public void MissingFiles()
        {
            TempDir.Dispose();

            using (var operation = new ClearDirectory(TempDir, Manifest, new SilentTaskHandler()))
            {
                operation.Stage();
                operation.Commit();
            }
        }
    }
}