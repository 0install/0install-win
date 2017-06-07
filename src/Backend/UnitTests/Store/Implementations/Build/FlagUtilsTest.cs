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
using Xunit;

namespace ZeroInstall.Store.Implementations.Build
{
    /// <summary>
    /// Contains test methods for <see cref="FlagUtils"/>.
    /// </summary>
    public class FlagUtilsTest
    {
        /// <summary>
        /// Ensures <see cref="FlagUtils.IsUnixFS"/> and <see cref="FlagUtils.MarkAsNoUnixFS"/> work correctly.
        /// </summary>
        [Fact]
        public void TestIsUnixFS()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                if (UnixUtils.IsUnix)
                {
                    FlagUtils.IsUnixFS(tempDir).Should().BeTrue();

                    FlagUtils.MarkAsNoUnixFS(tempDir);
                    FlagUtils.IsUnixFS(tempDir).Should().BeFalse();
                }
                else FlagUtils.IsUnixFS(tempDir).Should().BeFalse();
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.GetFiles"/> works correctly.
        /// </summary>
        [Fact]
        public void TestGetFiles()
        {
            using (var flagDir = new TemporaryDirectory("0install-unit-tests"))
            {
                File.WriteAllText(Path.Combine(flagDir, FlagUtils.XbitFile), "/dir1/file1\n/dir2/file2\n");

                var expectedResult = new[]
                {
                    Path.Combine(flagDir, "dir1", "file1"),
                    Path.Combine(flagDir, "dir2", "file2")
                };

                FlagUtils.GetFiles(FlagUtils.XbitFile, flagDir)
                    .Should().BeEquivalentTo(expectedResult, because: "Should find .xbit file in same directory");
                FlagUtils.GetFiles(FlagUtils.XbitFile, Path.Combine(flagDir, "subdir"))
                    .Should().BeEquivalentTo(expectedResult, because: "Should find .xbit file in parent directory");
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.IsFlagged"/> works correctly.
        /// </summary>
        [Fact]
        public void TestIsFlagged()
        {
            using (var flagDir = new TemporaryDirectory("0install-unit-tests"))
            {
                File.WriteAllText(Path.Combine(flagDir, FlagUtils.XbitFile), "/dir1/file1\n/dir2/file2\n");

                FlagUtils.IsFlagged(FlagUtils.XbitFile, Path.Combine(flagDir, "dir1", "file1")).Should().BeTrue();
                FlagUtils.IsFlagged(FlagUtils.XbitFile, Path.Combine(flagDir, "dir2", "file2")).Should().BeTrue();
                FlagUtils.IsFlagged(FlagUtils.XbitFile, Path.Combine(flagDir, "dir1", "file2")).Should().BeFalse();
                FlagUtils.IsFlagged(FlagUtils.SymlinkFile, Path.Combine(flagDir, "dir1", "file1")).Should().BeFalse();
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.Set"/> works correctly.
        /// </summary>
        [Fact]
        public void TestSet()
        {
            using (var flagFile = new TemporaryFile("0install-unit-tests"))
            {
                FlagUtils.Set(flagFile, Path.Combine("dir1", "file1"));
                File.ReadAllText(flagFile).Should().Be("/dir1/file1\n");

                FlagUtils.Set(flagFile, Path.Combine("dir2", "file2"));
                File.ReadAllText(flagFile).Should().Be("/dir1/file1\n/dir2/file2\n");
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.SetAuto"/> works correctly.
        /// </summary>
        [Fact]
        public void TestSetAuto()
        {
            using (var flagDir = new TemporaryDirectory("0install-unit-tests"))
            {
                FlagUtils.SetAuto(FlagUtils.XbitFile, Path.Combine(flagDir, "file1"));
                FlagUtils.SetAuto(FlagUtils.XbitFile, Path.Combine(flagDir, "dir", "file2"));
                File.ReadAllText(Path.Combine(flagDir, FlagUtils.XbitFile)).Should().Be("/file1\n/dir/file2\n");
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.Remove"/> works correctly.
        /// </summary>
        [Fact]
        public void TestRemove()
        {
            using (var flagFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(flagFile, "/dir1/file1\n/dir2/file2\n");

                FlagUtils.Remove(flagFile, "dir");
                File.ReadAllText(flagFile).Should().Be("/dir1/file1\n/dir2/file2\n", because: "Partial match should not change anything");

                FlagUtils.Remove(flagFile, Path.Combine("dir1", "file1"));
                File.ReadAllText(flagFile).Should().Be("/dir2/file2\n");

                FlagUtils.Remove(flagFile, "dir2");
                File.ReadAllText(flagFile).Should().Be("");
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.Rename"/> works correctly.
        /// </summary>
        [Fact]
        public void TestRename()
        {
            using (var flagFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(flagFile, "/dir/file1\n/dir/file2\n/dir2/file\n");

                FlagUtils.Rename(flagFile, "dir", "new_dir");
                File.ReadAllText(flagFile).Should().Be("/new_dir/file1\n/new_dir/file2\n/dir2/file\n");
            }
        }
    }
}
