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

using System.Security.Cryptography;
using FluentAssertions;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.FileSystem;

namespace ZeroInstall.Store.Implementations.Manifests
{
    /// <summary>
    /// Contains test methods for <see cref="ManifestGenerator"/>.
    /// </summary>
    [TestFixture]
    public class ManifestGeneratorTest
    {
        private static readonly string _hash = TestFile.DefaultContents.Hash(SHA1.Create());

        [Test]
        public void TestFileOrder() => Test(
            new TestRoot {new TestFile("x"), new TestFile("y"), new TestFile("Z")},
            new ManifestNormalFile(_hash, TestFile.DefaultLastWrite, TestFile.DefaultContents.Length, "Z"),
            new ManifestNormalFile(_hash, TestFile.DefaultLastWrite, TestFile.DefaultContents.Length, "x"),
            new ManifestNormalFile(_hash, TestFile.DefaultLastWrite, TestFile.DefaultContents.Length, "y"));

        [Test]
        public void TestFileTypes() => Test(
            new TestRoot
            {
                new TestFile("executable") {IsExecutable = true},
                new TestFile("normal"),
                new TestSymlink("symlink", target: "abc"),
                new TestDirectory("dir") {new TestFile("sub")}
            },
            new ManifestExecutableFile(_hash, TestFile.DefaultLastWrite, TestFile.DefaultContents.Length, "executable"),
            new ManifestNormalFile(_hash, TestFile.DefaultLastWrite, TestFile.DefaultContents.Length, "normal"),
            new ManifestSymlink("abc".Hash(SHA1.Create()), TestFile.DefaultContents.Length, "symlink"),
            new ManifestDirectory("/dir"),
            new ManifestNormalFile(_hash, TestFile.DefaultLastWrite, TestFile.DefaultContents.Length, "sub"));

        private static void Test(TestRoot root, params ManifestNode[] expected)
        {
            using (var sourceDirectory = new TemporaryDirectory("0install-unit-tests"))
            {
                root.Build(sourceDirectory);
                var generator = new ManifestGenerator(sourceDirectory, ManifestFormat.Sha1New);
                generator.Run();
                generator.Manifest.Should().Equal(expected);
            }
        }
    }
}
