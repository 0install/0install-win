/*
 * Copyright 2010-2013 Bastian Eicher
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
using Common.Storage;
using NUnit.Framework;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Contains test methods for <see cref="FlagUtils"/>.
    /// </summary>
    [TestFixture]
    public class FlagUtilsTest
    {
        /// <summary>
        /// Ensures <see cref="FlagUtils.GetExternalFlags"/> works correctly.
        /// </summary>
        [Test]
        public void TestGetExternalFlags()
        {
            using (var flagDir = new TemporaryDirectory("0isntall-unit-tests"))
            {
                // ReSharper disable LocalizableElement
                File.WriteAllText(Path.Combine(flagDir.Path, ".xbit"), "/dir1/file1\n/dir2/file2\n");
                // ReSharper restore LocalizableElement

                var expectedResult = new[]
                {
                    new[] {flagDir.Path, "dir1", "file1"}.Aggregate(Path.Combine),
                    new[] {flagDir.Path, "dir2", "file2"}.Aggregate(Path.Combine)
                };

                CollectionAssert.AreEquivalent(expectedResult, FlagUtils.GetExternalFlags(".xbit", flagDir.Path), "Should find .xbit file in same directory");
                CollectionAssert.AreEquivalent(expectedResult, FlagUtils.GetExternalFlags(".xbit", Path.Combine(flagDir.Path, "subdir")), "Should find .xbit file in parent directory");
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.SetExternalFlag"/> works correctly.
        /// </summary>
        [Test]
        public void TestSetExternalFlag()
        {
            using (var flagFile = new TemporaryFile("0isntall-unit-tests"))
            {
                FlagUtils.SetExternalFlag(flagFile.Path, Path.Combine("dir1", "file1"));
                Assert.AreEqual("/dir1/file1\n", File.ReadAllText(flagFile.Path));

                FlagUtils.SetExternalFlag(flagFile.Path, Path.Combine("dir2", "file2"));
                Assert.AreEqual("/dir1/file1\n/dir2/file2\n", File.ReadAllText(flagFile.Path));
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.RemoveExternalFlag"/> works correctly.
        /// </summary>
        [Test]
        public void TestRemoveExternalFlag()
        {
            using (var flagFile = new TemporaryFile("0isntall-unit-tests"))
            {
                // ReSharper disable LocalizableElement
                File.WriteAllText(flagFile.Path, "/dir1/file1\n/dir2/file2\n");
                // ReSharper restore LocalizableElement

                FlagUtils.RemoveExternalFlag(flagFile.Path, "dir");
                Assert.AreEqual("/dir1/file1\n/dir2/file2\n", File.ReadAllText(flagFile.Path), "Partial match should not change anything");

                FlagUtils.RemoveExternalFlag(flagFile.Path, Path.Combine("dir1", "file1"));
                Assert.AreEqual("/dir2/file2\n", File.ReadAllText(flagFile.Path));

                FlagUtils.RemoveExternalFlag(flagFile.Path, "dir2");
                Assert.AreEqual("", File.ReadAllText(flagFile.Path));
            }
        }

        /// <summary>
        /// Ensures <see cref="FlagUtils.PrefixExternalFlags"/> works correctly.
        /// </summary>
        [Test]
        public void TestPrefixExternalFlags()
        {
            using (var flagFile = new TemporaryFile("0isntall-unit-tests"))
            {
                // ReSharper disable LocalizableElement
                File.WriteAllText(flagFile.Path, "/dir1/file1\n/dir2/file2\n");
                // ReSharper restore LocalizableElement

                FlagUtils.PrefixExternalFlags(flagFile.Path, Path.Combine("pre", "fix"));
                Assert.AreEqual("/pre/fix/dir1/file1\n/pre/fix/dir2/file2\n", File.ReadAllText(flagFile.Path));
            }
        }
    }
}
