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

using System.IO;
using FluentAssertions;
using ICSharpCode.SharpZipLib.Tar;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Contains test methods for <see cref="TarGenerator"/>.
    /// </summary>
    [TestFixture]
    public class TarGeneratorTest : ArchiveGeneratorTest<TarGenerator>
    {
        protected override TarGenerator CreateGenerator(string sourceDirectory, Stream stream)
        {
            return new TarGenerator(sourceDirectory, stream);
        }

        protected override void VerifyFileOrder()
        {
            using (var archive = new TarInputStream(OpenArchive()))
            {
                archive.GetNextEntry().Name.Should().Be("Z");
                archive.GetNextEntry().Name.Should().Be("x");
                archive.GetNextEntry().Name.Should().Be("y");
            }
        }

        protected override void VerifyFileTypes()
        {
            using (var archive = new TarInputStream(OpenArchive()))
            {
                var executable = archive.GetNextEntry();
                executable.Name.Should().Be("executable");
                executable.ModTime.Should().Be(Timestamp);
                executable.TarHeader.Mode.Should().Be(TarExtractor.DefaultMode | TarExtractor.ExecuteMode);

                var normal = archive.GetNextEntry();
                normal.Name.Should().Be("normal");
                normal.ModTime.Should().Be(Timestamp);
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
                sub.ModTime.Should().Be(Timestamp);
                sub.TarHeader.Mode.Should().Be(TarExtractor.DefaultMode);
            }
        }

        [Test]
        public void TestHardlink()
        {
            WriteFile("file");
            CreateHardlink("hardlink", target: "file");

            Execute();

            VerifyHardlink();
        }

        private void VerifyHardlink()
        {
            using (var archive = new TarInputStream(OpenArchive()))
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
