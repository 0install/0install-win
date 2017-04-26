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
using ICSharpCode.SharpZipLib.Tar;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.FileSystem;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Contains test methods for <see cref="TarGenerator"/>.
    /// </summary>
    [TestFixture]
    public class TarGeneratorTest : ArchiveGeneratorTest<TarGenerator>
    {
        protected override TarGenerator CreateGenerator(string sourceDirectory, Stream stream) => new TarGenerator(sourceDirectory, stream);

        [Test]
        public void TestFileOrder()
        {
            var stream = BuildArchive(new TestRoot {new TestFile("x"), new TestFile("y"), new TestFile("Z")});

            using (var archive = new TarInputStream(stream))
            {
                archive.GetNextEntry().Name.Should().Be("Z");
                archive.GetNextEntry().Name.Should().Be("x");
                archive.GetNextEntry().Name.Should().Be("y");
            }
        }

        [Test]
        public void TestFileTypes()
        {
            var stream = BuildArchive(new TestRoot
            {
                new TestFile("executable") {IsExecutable = true},
                new TestFile("normal"),
                new TestSymlink("symlink", target: "abc"),
                new TestDirectory("dir") {new TestFile("sub")}
            });

            using (var archive = new TarInputStream(stream))
            {
                var executable = archive.GetNextEntry();
                executable.Name.Should().Be("executable");
                executable.ModTime.Should().Be(TestFile.DefaultLastWrite);
                executable.TarHeader.Mode.Should().Be(TarExtractor.DefaultMode | TarExtractor.ExecuteMode);

                var normal = archive.GetNextEntry();
                normal.Name.Should().Be("normal");
                normal.ModTime.Should().Be(TestFile.DefaultLastWrite);
                normal.TarHeader.Mode.Should().Be(TarExtractor.DefaultMode);

                var symlink = archive.GetNextEntry();
                symlink.Name.Should().Be("symlink");
                symlink.TarHeader.TypeFlag.Should().Be(TarHeader.LF_SYMLINK);

                var directory = archive.GetNextEntry();
                directory.Name.Should().Be("dir");
                directory.IsDirectory.Should().BeTrue();
                directory.TarHeader.Mode.Should().Be(TarExtractor.DefaultMode | TarExtractor.ExecuteMode);

                var sub = archive.GetNextEntry();
                sub.Name.Should().Be("dir/sub");
                sub.ModTime.Should().Be(TestFile.DefaultLastWrite);
                sub.TarHeader.Mode.Should().Be(TarExtractor.DefaultMode);
            }
        }

        [Test]
        public void TestHardlink()
        {
            Stream stream;
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                new TestRoot {new TestFile("file")}.Build(tempDir);
                FileUtils.CreateHardlink(
                    sourcePath: Path.Combine(tempDir, "hardlink"),
                    targetPath: Path.Combine(tempDir, "file"));
                stream = BuildArchive(tempDir);
            }

            using (var archive = new TarInputStream(stream))
            {
                var file = archive.GetNextEntry();
                file.Name.Should().Be("file");

                var hardlink = archive.GetNextEntry();
                hardlink.Name.Should().Be("hardlink");
                hardlink.TarHeader.TypeFlag.Should().Be(TarHeader.LF_LINK);
                hardlink.TarHeader.LinkName.Should().Be("file");
            }
        }
    }
}
