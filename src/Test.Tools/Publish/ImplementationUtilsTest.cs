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
using System.Security.Cryptography;
using Common;
using Common.Storage;
using Common.Tasks;
using Common.Undo;
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

        private const string SingleFileData = "data";
        private const string SingleFileName = "file.dat";

        private static readonly string _singleFileSha256Digest = new Manifest(ManifestFormat.Sha256New,
            new ManifestNormalFile(SingleFileData.Hash(SHA256.Create()), 0, SingleFileData.Length, SingleFileName)).CalculateDigest();

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.DownloadAndApply(DownloadRetrievalMethod,ITaskHandler,ICommandExecutor)"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void DownloadAndApplyArchive()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var archive = new Archive {Href = microServer.FileUri};
                ImplementationUtils.DownloadAndApply(archive, new SilentTaskHandler()).Dispose();

                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.DownloadAndApply(DownloadRetrievalMethod,ITaskHandler,ICommandExecutor)"/> works correctly with <see cref="SingleFile"/>s.
        /// </summary>
        [Test]
        public void DownloadAndApplySingleFile()
        {
            using (var originalStream = SingleFileData.ToStream())
            using (var microServer = new MicroServer(SingleFileName, originalStream))
            {
                var file = new SingleFile {Href = microServer.FileUri, Destination = SingleFileName};
                ImplementationUtils.DownloadAndApply(file, new SilentTaskHandler()).Dispose();

                Assert.AreEqual(originalStream.Length, file.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.DownloadAndApply(Recipe,ITaskHandler,ICommandExecutor)"/> works correctly with <seealso cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void DownloadAndApplyRecipe()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var archive = new Archive {Href = microServer.FileUri};
                var recipe = new Recipe {Steps = {archive}};
                ImplementationUtils.DownloadAndApply(recipe, new SilentTaskHandler()).Dispose();

                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.Build"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void BuildArchive()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = ImplementationUtils.Build(new Archive {Href = microServer.FileUri}, false, new SilentTaskHandler());
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
                var implementation = ImplementationUtils.Build(new SingleFile {Href = microServer.FileUri, Destination = SingleFileName}, false, new SilentTaskHandler());
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
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = ImplementationUtils.Build(new Recipe {Steps = {new Archive {Href = microServer.FileUri}}}, false, new SilentTaskHandler());
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
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {RetrievalMethods = {new Archive {Href = microServer.FileUri}}};
                ImplementationUtils.AddMissing(implementation, false, new SilentTaskHandler());
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
                var implementation = new Implementation {RetrievalMethods = {new SingleFile {Href = microServer.FileUri, Destination = SingleFileName}}};
                ImplementationUtils.AddMissing(implementation, false, new SilentTaskHandler());
                Assert.AreEqual(_singleFileSha256Digest, "sha256new_" + implementation.ManifestDigest.Sha256New);

                var file = (SingleFile)implementation.RetrievalMethods[0];
                Assert.AreEqual(originalStream.Length, file.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.AddMissing"/> works correctly with <see cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void AddMissingRecipe()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {RetrievalMethods = {new Recipe {Steps = {new Archive {Href = microServer.FileUri}}}}};
                ImplementationUtils.AddMissing(implementation, false, new SilentTaskHandler());
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
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var implementation = new Implementation {ManifestDigest = new ManifestDigest(sha1New: "invalid"), RetrievalMethods = {new Archive {Href = microServer.FileUri}}};
                Assert.Throws<DigestMismatchException>(() => ImplementationUtils.AddMissing(implementation, false, new SilentTaskHandler()));
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.LocalApply"/> handles <see cref="Archive"/>s without downloading them.
        /// </summary>
        [Test]
        public void LocalApplyArchive()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                string tempFile = Path.Combine(tempDir, "archive.zip");
                using (var memoryStream = TestData.GetTestZipArchiveStream())
                    memoryStream.WriteToFile(tempFile);

                var archive = new Archive();
                using (var extractedDir = ImplementationUtils.LocalApply(archive, tempFile, new SilentTaskHandler()))
                    Assert.IsTrue(File.Exists(Path.Combine(extractedDir, "symlink")));

                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(new FileInfo(tempFile).Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="ImplementationUtils.LocalApply"/> handles <see cref="SingleFile"/>s without downloading them.
        /// </summary>
        [Test]
        public void LocalApplySingleFile()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                File.WriteAllText(tempFile, @"abc");

                var file = new SingleFile {Destination = "file"};
                using (var extractedDir = ImplementationUtils.LocalApply(file, tempFile, new SilentTaskHandler()))
                    Assert.IsTrue(File.Exists(Path.Combine(extractedDir, "file")));

                Assert.AreEqual(3, file.Size);
            }
        }
    }
}
