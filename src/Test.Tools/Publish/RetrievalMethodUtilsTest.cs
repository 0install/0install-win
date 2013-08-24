﻿/*
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
using Common.Storage;
using Common.Tasks;
using Common.Undo;
using Common.Utils;
using NUnit.Framework;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Publish
{
    /// <summary>
    /// Contains test methods for <see cref="RetrievalMethodUtils"/>.
    /// </summary>
    [TestFixture]
    public class RetrievalMethodUtilsTest
    {
        private const string SingleFileData = "data";
        private const string SingleFileName = "file.dat";

        /// <summary>
        /// Ensures <see cref="RetrievalMethodUtils.DownloadAndApply(DownloadRetrievalMethod,ITaskHandler,ICommandExecutor)"/> works correctly with <see cref="Archive"/>s.
        /// </summary>
        [Test]
        public void DownloadAndApplyArchive()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var archive = new Archive {Href = microServer.FileUri};
                RetrievalMethodUtils.DownloadAndApply(archive, new SilentTaskHandler()).Dispose();

                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="RetrievalMethodUtils.DownloadAndApply(DownloadRetrievalMethod,ITaskHandler,ICommandExecutor)"/> works correctly with <see cref="SingleFile"/>s.
        /// </summary>
        [Test]
        public void DownloadAndApplySingleFile()
        {
            using (var originalStream = SingleFileData.ToStream())
            using (var microServer = new MicroServer(SingleFileName, originalStream))
            {
                var file = new SingleFile {Href = microServer.FileUri, Destination = SingleFileName};
                RetrievalMethodUtils.DownloadAndApply(file, new SilentTaskHandler()).Dispose();

                Assert.AreEqual(originalStream.Length, file.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="RetrievalMethodUtils.DownloadAndApply(Recipe,ITaskHandler,ICommandExecutor)"/> works correctly with <seealso cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void DownloadAndApplyRecipe()
        {
            using (var originalStream = TestData.GetTestZipArchiveStream())
            using (var microServer = new MicroServer("archive.zip", originalStream))
            {
                var archive = new Archive {Href = microServer.FileUri};
                var recipe = new Recipe {Steps = {archive}};
                RetrievalMethodUtils.DownloadAndApply(recipe, new SilentTaskHandler()).Dispose();

                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(originalStream.Length, archive.Size);
            }
        }

        /// <summary>
        /// Ensures <see cref="RetrievalMethodUtils.LocalApply"/> handles <see cref="Archive"/>s without downloading them.
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
                using (var extractedDir = RetrievalMethodUtils.LocalApply(archive, tempFile, new SilentTaskHandler()))
                    Assert.IsTrue(File.Exists(Path.Combine(extractedDir, "symlink")));

                Assert.AreEqual(Archive.MimeTypeZip, archive.MimeType);
                Assert.AreEqual(new FileInfo(tempFile).Length, archive.Size);

                Assert.IsTrue(File.Exists(tempFile), "Local reference file should not be removed");
            }
        }

        /// <summary>
        /// Ensures <see cref="RetrievalMethodUtils.LocalApply"/> handles <see cref="SingleFile"/>s without downloading them.
        /// </summary>
        [Test]
        public void LocalApplySingleFile()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                string tempFile = Path.Combine(tempDir, "file");
                File.WriteAllText(tempFile, @"abc");

                var file = new SingleFile();
                using (var extractedDir = RetrievalMethodUtils.LocalApply(file, tempFile, new SilentTaskHandler()))
                    Assert.IsTrue(File.Exists(Path.Combine(extractedDir, "file")));

                Assert.AreEqual("file", file.Destination);
                Assert.AreEqual(3, file.Size);

                Assert.IsTrue(File.Exists(tempFile), "Local reference file should not be removed");
            }
        }
    }
}
