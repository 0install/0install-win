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

using System.Security.Cryptography;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NanoByte.Common.Utils;
using NUnit.Framework;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Contains test methods for <see cref="ImplementationUtils"/>.
    /// </summary>
    [TestFixture]
    public class ImplementationUtilsTest
    {
        private const string ArchiveSha256Digest = "TPD62FAK7ME7OCER5CHL3HQDZQMNJVENJUBL6E6IXX5UI44OXMJQ";

        private const string SingleFileData = "data";
        private const string SingleFileName = "file.dat";

        private static readonly string _singleFileSha256Digest = new Manifest(ManifestFormat.Sha256New,
            new ManifestNormalFile(SingleFileData.Hash(SHA256.Create()), 0, SingleFileData.Length, SingleFileName)).CalculateDigest();

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void BuildArchive()
        {
            using (var originalStream = TestData.GetResource("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = ImplementationUtils.Build(new Archive {Href = microServer.FileUri}, new SilentTaskHandler());
                Assert.AreEqual(ArchiveSha256Digest, implementation.ManifestDigest.Sha256New);

                var archive = (Archive)implementation.RetrievalMethods[0];
                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="SingleFile"/>s.
        /// </summary>
        [Test]
        public void BuildSingleFile()
        {
            using (var originalStream = SingleFileData.ToStream())
            using (var microServer = new MicroServer(SingleFileName, originalStream))
            {
                var implementation = ImplementationUtils.Build(new SingleFile {Href = microServer.FileUri, Destination = SingleFileName}, new SilentTaskHandler());
                Assert.AreEqual(_singleFileSha256Digest, "sha256new_" + implementation.ManifestDigest.Sha256New);

                var file = (SingleFile)implementation.RetrievalMethods[0];
                Assert.AreEqual(originalStream.Length, file.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void BuildRecipe()
        {
            using (var originalStream = TestData.GetResource("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = ImplementationUtils.Build(new Recipe {Steps = {new Archive {Href = microServer.FileUri}}}, new SilentTaskHandler());
                Assert.AreEqual(ArchiveSha256Digest, implementation.ManifestDigest.Sha256New);

                var archive = (Archive)((Recipe)implementation.RetrievalMethods[0]).Steps[0];
                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void AddMissingArchive()
        {
            using (var originalStream = TestData.GetResource("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {RetrievalMethods = {new Archive {Href = microServer.FileUri}}};
                implementation.AddMissing(new SilentTaskHandler());
                Assert.AreEqual(ArchiveSha256Digest, implementation.ManifestDigest.Sha256New);

                var archive = (Archive)implementation.RetrievalMethods[0];
                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="SingleFile"/>s.
        /// </summary>
        [Test]
        public void AddMissingSingleFile()
        {
            using (var originalStream = SingleFileData.ToStream())
            using (var microServer = new MicroServer(SingleFileName, originalStream))
            {
                var implementation = new Implementation {RetrievalMethods = {new SingleFile {Href = microServer.FileUri}}};
                implementation.AddMissing(new SilentTaskHandler());
                Assert.AreEqual(_singleFileSha256Digest, "sha256new_" + implementation.ManifestDigest.Sha256New);

                var file = (SingleFile)implementation.RetrievalMethods[0];
                Assert.AreEqual(originalStream.Length, file.Size);
                Assert.AreEqual(SingleFileName, file.Destination);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void AddMissingRecipe()
        {
            using (var originalStream = TestData.GetResource("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {RetrievalMethods = {new Recipe {Steps = {new Archive {Href = microServer.FileUri}}}}};
                implementation.AddMissing(new SilentTaskHandler());
                Assert.AreEqual(ArchiveSha256Digest, implementation.ManifestDigest.Sha256New);

                var archive = (Archive)((Recipe)implementation.RetrievalMethods[0]).Steps[0];
                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> throws <see cref="DigestMismatchException"/>s when appropriate.
        /// </summary>
        [Test]
        public void AddMissingExceptions()
        {
            using (var originalStream = TestData.GetResource("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {ManifestDigest = new ManifestDigest(sha1New: "invalid"), RetrievalMethods = {new Archive {Href = microServer.FileUri}}};
                Assert.Throws<DigestMismatchException>(() => implementation.AddMissing(new SilentTaskHandler()));
            }
        }
    }
}
