/*
 * Copyright 2010-2014 Bastian Eicher
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
using System.Linq;
using NanoByte.Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Contains test methods for <see cref="FlagUtils"/>.
    /// </summary>
    [TestFixture]
    public class FlagUtilsTest
    {
        /// <summary>
        /// Ensures <see cref="FlagUtils.GetFiles"/> works correctly.
        /// </summary>
        [Test]
        public void TestGetFiles()
        {
            using (var flagDir = new TemporaryDirectory("0install-unit-tests"))
            {
                File.WriteAllText(Path.Combine(flagDir, ".xbit"), "/dir1/file1\n/dir2/file2\n");

                var expectedResult = new[]
                {
                    new[] {flagDir, "dir1", "file1"}.Aggregate(Path.Combine),
                    new[] {flagDir, "dir2", "file2"}.Aggregate(Path.Combine)
                };

                CollectionAssert.AreEquivalent(expectedResult, FlagUtils.GetFiles(".xbit", flagDir), "Should find .xbit file in same directory");
                CollectionAssert.AreEquivalent(expectedResult, FlagUtils.GetFiles(".xbit", Path.Combine(flagDir, "subdir")), "Should find .xbit file in parent directory");
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.Set"/> works correctly.
        /// </summary>
        [Test]
        public void TestSet()
        {
            using (var flagFile = new TemporaryFile("0install-unit-tests"))
            {
                FlagUtils.Set(flagFile, Path.Combine("dir1", "file1"));
                Assert.AreEqual("/dir1/file1\n", File.ReadAllText(flagFile));

                FlagUtils.Set(flagFile, Path.Combine("dir2", "file2"));
                Assert.AreEqual("/dir1/file1\n/dir2/file2\n", File.ReadAllText(flagFile));
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.Remove"/> works correctly.
        /// </summary>
        [Test]
        public void TestRemove()
        {
            using (var flagFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(flagFile, "/dir1/file1\n/dir2/file2\n");

                FlagUtils.Remove(flagFile, "dir");
                Assert.AreEqual("/dir1/file1\n/dir2/file2\n", File.ReadAllText(flagFile), "Partial match should not change anything");

                FlagUtils.Remove(flagFile, Path.Combine("dir1", "file1"));
                Assert.AreEqual("/dir2/file2\n", File.ReadAllText(flagFile));

                FlagUtils.Remove(flagFile, "dir2");
                Assert.AreEqual("", File.ReadAllText(flagFile));
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.Rename"/> works correctly.
        /// </summary>
        [Test]
        public void TestRename()
        {
            using (var flagFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(flagFile, "/dir/file1\n/dir/file2\n/dir2/file\n");

                FlagUtils.Rename(flagFile, "dir", "new_dir");
                Assert.AreEqual("/new_dir/file1\n/new_dir/file2\n/dir2/file\n", File.ReadAllText(flagFile));
            }
        }
    }
}
