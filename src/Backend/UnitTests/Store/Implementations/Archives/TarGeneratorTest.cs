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
            using (var archive = new TarInputStream(ArchiveReadStream))
            {
                Assert.AreEqual(expected: "Z", actual: archive.GetNextEntry().Name);
                Assert.AreEqual(expected: "x", actual: archive.GetNextEntry().Name);
                Assert.AreEqual(expected: "y", actual: archive.GetNextEntry().Name);
            }
        }

        protected override void VerifyFileTypes()
        {
            using (var archive = new TarInputStream(ArchiveReadStream))
            {
                var executable = archive.GetNextEntry();
                Assert.AreEqual(expected: "executable", actual: executable.Name);
                Assert.AreEqual(expected: Timestamp, actual: executable.ModTime);
                Assert.IsTrue((executable.TarHeader.Mode & TarExtractor.ExecuteMode) > 0);

                var normal = archive.GetNextEntry();
                Assert.AreEqual(expected: "normal", actual: normal.Name);
                Assert.AreEqual(expected: Timestamp, actual: normal.ModTime);
                Assert.IsFalse((normal.TarHeader.Mode & TarExtractor.ExecuteMode) > 0);

                var symlink = archive.GetNextEntry();
                Assert.AreEqual(expected: "symlink", actual: symlink.Name);
                Assert.AreEqual(expected: TarHeader.LF_SYMLINK, actual: symlink.TarHeader.TypeFlag);

                var directory = archive.GetNextEntry();
                Assert.AreEqual(expected: "dir", actual: directory.Name);
                Assert.IsTrue(directory.IsDirectory);

                var sub = archive.GetNextEntry();
                Assert.AreEqual(expected: "dir/sub", actual: sub.Name);
                Assert.AreEqual(expected: Timestamp, actual: sub.ModTime);
                Assert.IsFalse((sub.TarHeader.Mode & TarExtractor.ExecuteMode) > 0);
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
            using (var archive = new TarInputStream(ArchiveReadStream))
            {
                var file = archive.GetNextEntry();
                Assert.AreEqual(expected: "file", actual: file.Name);

                var hardlink = archive.GetNextEntry();
                Assert.AreEqual(expected: "hardlink", actual: hardlink.Name);
                Assert.AreEqual(expected: TarHeader.LF_LINK, actual:hardlink.TarHeader.TypeFlag);
                Assert.AreEqual(expected: "file", actual:hardlink.TarHeader.LinkName);
            }
        }
    }
}
