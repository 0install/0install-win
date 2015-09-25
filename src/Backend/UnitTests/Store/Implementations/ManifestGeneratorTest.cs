/*
 * Copyright 2010-2015 Bastian Eicher, Roland Leopold Walkling
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

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Contains test methods for <see cref="ManifestGenerator"/>.
    /// </summary>
    [TestFixture]
    public class ManifestGeneratorTest : DirectoryWalkTest<ManifestGenerator>
    {
        protected override ManifestGenerator CreateTarget(string sourceDirectory)
        {
            return new ManifestGenerator(sourceDirectory, ManifestFormat.Sha1New);
        }

        private static readonly string _hash = Contents.Hash(SHA1.Create());
        private static readonly long _timestamp = Timestamp.ToUnixTime();

        protected override void VerifyFileOrder()
        {
            Target.Manifest.Should().Equal(
                new ManifestNormalFile(_hash, _timestamp, Contents.Length, "Z"),
                new ManifestNormalFile(_hash, _timestamp, Contents.Length, "x"),
                new ManifestNormalFile(_hash, _timestamp, Contents.Length, "y"));
        }

        protected override void VerifyFileTypes()
        {
            Target.Manifest.Should().Equal(
                new ManifestExecutableFile(_hash, _timestamp, Contents.Length, "executable"),
                new ManifestNormalFile(_hash, _timestamp, Contents.Length, "normal"),
                new ManifestSymlink(_hash, Contents.Length, "symlink"),
                new ManifestDirectory("/dir"),
                new ManifestNormalFile(_hash, _timestamp, Contents.Length, "sub"));
        }
    }
}
