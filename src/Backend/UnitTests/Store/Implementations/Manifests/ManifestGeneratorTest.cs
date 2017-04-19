/*
 * Copyright 2010-2016 Bastian Eicher, Roland Leopold Walkling
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
using System.Security.Cryptography;
using FluentAssertions;
using NanoByte.Common;
using NUnit.Framework;
using ZeroInstall.Store.Implementations.Build;

namespace ZeroInstall.Store.Implementations.Manifests
{
    /// <summary>
    /// Contains test methods for <see cref="ManifestGenerator"/>.
    /// </summary>
    [TestFixture]
    public class ManifestGeneratorTest : DirectoryTaskTestBase<ManifestGenerator>
    {
        protected override ManifestGenerator InitSut(string sourceDirectory) => new ManifestGenerator(sourceDirectory, ManifestFormat.Sha1New);

        private static readonly string _hash = Contents.Hash(SHA1.Create());

        [Test]
        public void TestFileOrder()
        {
            WriteFile("x");
            WriteFile("y");
            WriteFile("Z");

            Execute();

            Sut.Manifest.Should().Equal(
                new ManifestNormalFile(_hash, Timestamp, Contents.Length, "Z"),
                new ManifestNormalFile(_hash, Timestamp, Contents.Length, "x"),
                new ManifestNormalFile(_hash, Timestamp, Contents.Length, "y"));
        }

        [Test]
        public void TestFileTypes()
        {
            WriteFile("executable", executable: true);
            WriteFile("normal");
            CreateSymlink("symlink");
            CreateDir("dir");
            WriteFile(Path.Combine("dir", "sub"));

            Execute();

            Sut.Manifest.Should().Equal(
                new ManifestExecutableFile(_hash, Timestamp, Contents.Length, "executable"),
                new ManifestNormalFile(_hash, Timestamp, Contents.Length, "normal"),
                new ManifestSymlink(_hash, Contents.Length, "symlink"),
                new ManifestDirectory("/dir"),
                new ManifestNormalFile(_hash, Timestamp, Contents.Length, "sub"));
        }
    }
}
