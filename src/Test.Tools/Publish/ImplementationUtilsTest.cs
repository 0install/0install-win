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
using Common;
using Common.Tasks;
using Common.Utils;
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Contains test methods for <see cref="ImplementationUtils"/>.
    /// </summary>
    [TestFixture]
    public class ImplementationUtilsTest
    {
        private const string ArchiveSha256Digest = "TPD62FAK7ME7OCER5CHL3HQDZQMNJVENJUBL6E6IXX5UI44OXMJQ";

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.DownloadArchive"/> works correctly.
        /// </summary>
        [Test]
        public void TestDownloadArchive()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var archive = new Archive {Location = microServer.FileUri};
                using (var tempFile = ImplementationUtils.DownloadArchive(archive, new SilentTaskHandler()))
                using (var downloadedStream = File.OpenRead(tempFile.Path))
                    Assert.IsTrue(StreamUtils.Equals(originalStream, downloadedStream), "Original and downloaded stream should be equal");

                Assert.AreEqual("application/zip", archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.DownloadAndExtractArchive"/> works correctly.
        /// </summary>
        [Test]
        public void TestDownloadAndExtractArchive()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var archive = new Archive {Location = microServer.FileUri};
                ImplementationUtils.DownloadArchive(archive, new SilentTaskHandler()).Dispose();

                Assert.AreEqual("application/zip", archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.ApplyRecipe"/> works correctly.
        /// </summary>
        [Test]
        public void TestApplyRecipe()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var archive = new Archive {Location = microServer.FileUri};
                var recipe = new Recipe {Steps = {archive}};
                ImplementationUtils.ApplyRecipe(recipe, new SilentTaskHandler()).Dispose();

                Assert.AreEqual("application/zip", archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void TestBuildArchive()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = ImplementationUtils.Build(new Archive {Location = microServer.FileUri}, false, new SilentTaskHandler());
                Assert.AreEqual(ArchiveSha256Digest, implementation.ManifestDigest.Sha256New);

                var archive = (Archive)implementation.RetrievalMethods[0];
                Assert.AreEqual("application/zip", archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void TestBuildRecipe()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = ImplementationUtils.Build(new Recipe {Steps = {new Archive {Location = microServer.FileUri}}}, false, new SilentTaskHandler());
                Assert.AreEqual(ArchiveSha256Digest, implementation.ManifestDigest.Sha256New);

                var archive = (Archive)((Recipe)implementation.RetrievalMethods[0]).Steps[0];
                Assert.AreEqual("application/zip", archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void TestAddMissingArchive()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {RetrievalMethods = {new Archive {Location = microServer.FileUri}}};
                ImplementationUtils.AddMissing(implementation, false, new SilentTaskHandler());
                Assert.AreEqual(ArchiveSha256Digest, implementation.ManifestDigest.Sha256New);

                var archive = (Archive)implementation.RetrievalMethods[0];
                Assert.AreEqual("application/zip", archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void TestAddMissingRecipe()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {RetrievalMethods = {new Recipe {Steps = {new Archive {Location = microServer.FileUri}}}}};
                ImplementationUtils.AddMissing(implementation, false, new SilentTaskHandler());
                Assert.AreEqual(ArchiveSha256Digest, implementation.ManifestDigest.Sha256New);

                var archive = (Archive)((Recipe)implementation.RetrievalMethods[0]).Steps[0];
                Assert.AreEqual("application/zip", archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> throws <see cref="DigestMismatchException"/>s when appropriate.
        /// </summary>
        [Test]
        public void TestAddMissingExceptions()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {ManifestDigest = new ManifestDigest(sha1New: "invalid"), RetrievalMethods = {new Archive {Location = microServer.FileUri}}};
                Assert.Throws<DigestMismatchException>(() => ImplementationUtils.AddMissing(implementation, false, new SilentTaskHandler()));
            }
        }
    }
}
