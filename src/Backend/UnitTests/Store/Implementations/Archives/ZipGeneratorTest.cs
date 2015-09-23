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
using ICSharpCode.SharpZipLib.Zip;
using NanoByte.Common.Values;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Contains test methods for <see cref="ZipGenerator"/>.
    /// </summary>
    [TestFixture]
    public class ZipGeneratorTest : ArchiveGeneratorTest<ZipGenerator>
    {
        protected override ZipGenerator CreateGenerator(string sourceDirectory, Stream stream)
        {
            return new ZipGenerator(sourceDirectory, stream);
        }

        protected override void VerifyFileOrder()
        {
            using (var archive = new ZipFile(ArchiveReadStream))
            {
                Assert.AreEqual(expected: "Z", actual: archive[0].Name);
                Assert.AreEqual(expected: "x", actual: archive[1].Name);
                Assert.AreEqual(expected: "y", actual: archive[2].Name);
            }
        }

        protected override void VerifyFileTypes()
        {
            using (var archive = new ZipFile(ArchiveReadStream))
            {
                var executable = archive[0];
                Assert.AreEqual(expected: "executable", actual: executable.Name);
                Assert.IsTrue(executable.IsFile);
                Assert.AreEqual(expected: Timestamp, actual: executable.DateTime);
                Assert.IsTrue(executable.ExternalFileAttributes.HasFlag(ZipExtractor.ExecuteAttributes));

                var normal = archive[1];
                Assert.AreEqual(expected: "normal", actual: normal.Name);
                Assert.IsTrue(normal.IsFile);
                Assert.AreEqual(expected: Timestamp, actual: normal.DateTime);
                Assert.IsFalse(normal.ExternalFileAttributes.HasFlag(ZipExtractor.ExecuteAttributes));

                var symlink = archive[2];
                Assert.AreEqual(expected: "symlink", actual: symlink.Name);
                Assert.IsTrue(symlink.ExternalFileAttributes.HasFlag(ZipExtractor.SymlinkAttributes));

                var directory = archive[3];
                Assert.AreEqual(expected: "dir/", actual: directory.Name);
                Assert.IsTrue(directory.IsDirectory);

                var sub = archive[4];
                Assert.AreEqual(expected: "dir/sub", actual: sub.Name);
                Assert.IsTrue(sub.IsFile);
                Assert.AreEqual(expected: Timestamp, actual: sub.DateTime);
                Assert.IsFalse(sub.ExternalFileAttributes.HasFlag(ZipExtractor.ExecuteAttributes));
            }
        }
    }
}
