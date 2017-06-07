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
using NanoByte.Common.Storage;
using Xunit;
using ZeroInstall.FileSystem;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Contains test methods for <see cref="CloneDirectory"/>.
    /// </summary>
    public class CloneDirectoryTest : CloneTestBase
    {
        [Fact]
        public void Copy()
        {
            var root = new TestRoot
            {
                new TestDirectory("dir")
                {
                    new TestFile("file"),
                    new TestFile("executable") {IsExecutable = true},
                    new TestSymlink("symlink", "target")
                }
            };
            root.Build(SourceDirectory);

            new CloneDirectory(SourceDirectory, TargetDirectory).Run();

            root.Verify(TargetDirectory);
        }

        [Fact]
        public void CopySuffix()
        {
            var root = new TestRoot
            {
                new TestDirectory("dir")
                {
                    new TestFile("file"),
                    new TestFile("executable") {IsExecutable = true},
                    new TestSymlink("symlink", "target")
                }
            };
            root.Build(SourceDirectory);

            new CloneDirectory(SourceDirectory, TargetDirectory) {TargetSuffix = "suffix"}.Run();

            root.Verify(Path.Combine(TargetDirectory, "suffix"));
        }

        [Fact]
        public void Hardlink()
        {
            var root = new TestRoot
            {
                new TestDirectory("dir")
                {
                    new TestFile("file"),
                    new TestFile("executable") {IsExecutable = true},
                    new TestSymlink("symlink", "target")
                }
            };
            root.Build(SourceDirectory);

            FileUtils.EnableWriteProtection(SourceDirectory); // Hardlinking logic should work around write-protection by temporarily removing it
            try
            {
                new CloneDirectory(SourceDirectory, TargetDirectory) {UseHardlinks = true}.Run();
            }
            finally
            {
                FileUtils.DisableWriteProtection(SourceDirectory);
            }

            root.Verify(TargetDirectory);
            FileUtils.AreHardlinked(Path.Combine(SourceDirectory, "dir", "file"), Path.Combine(TargetDirectory, "dir", "file"));
            FileUtils.AreHardlinked(Path.Combine(SourceDirectory, "dir", "executable"), Path.Combine(TargetDirectory, "dir", "executable"));
        }

        [Fact]
        public void OverwriteFile()
        {
            var root = new TestRoot {new TestFile("fileA")};
            root.Build(SourceDirectory);
            new TestRoot {new TestFile("fileB") {LastWrite = new DateTime(2000, 2, 2), Contents = "wrong", IsExecutable = true}}.Build(TargetDirectory);

            new CloneDirectory(SourceDirectory, TargetDirectory).Run();

            root.Verify(TargetDirectory);
        }

        [Fact]
        public void OverwriteSymlink()
        {
            var root = new TestRoot {new TestFile("fileA")};
            root.Build(SourceDirectory);
            new TestRoot {new TestSymlink("fileB", "target")}.Build(TargetDirectory);

            new CloneDirectory(SourceDirectory, TargetDirectory).Run();

            root.Verify(TargetDirectory);
        }

        [Fact]
        public void OverwriteWithSymlink()
        {
            var root = new TestRoot {new TestSymlink("fileA", "target")};
            root.Build(SourceDirectory);
            new TestRoot {new TestFile("fileB")}.Build(TargetDirectory);

            new CloneDirectory(SourceDirectory, TargetDirectory).Run();

            root.Verify(TargetDirectory);
        }
    }
}
