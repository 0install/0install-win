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
using FluentAssertions;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using NanoByte.Common.Undo;
using NUnit.Framework;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

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
            using (var stream = typeof(ExtractorTest).GetEmbedded("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", stream))
            {
                var archive = new Archive {Href = microServer.FileUri};
                archive.DownloadAndApply(new SilentTaskHandler()).Dispose();

                archive.MimeType.Should().Be(Archive.MimeTypeZip);
                archive.Size.Should().Be(stream.Length);
            }
        }

        /// <summary>
        /// Ensures <see cref="RetrievalMethodUtils.DownloadAndApply(DownloadRetrievalMethod,ITaskHandler,ICommandExecutor)"/> works correctly with <see cref="SingleFile"/>s.
        /// </summary>
        [Test]
        public void DownloadAndApplySingleFile()
        {
            using (var stream = SingleFileData.ToStream())
            using (var microServer = new MicroServer(SingleFileName, stream))
            {
                var file = new SingleFile {Href = microServer.FileUri, Destination = SingleFileName};
                file.DownloadAndApply(new SilentTaskHandler()).Dispose();

                file.Size.Should().Be(stream.Length);
            }
        }

        /// <summary>
        /// Ensures <see cref="RetrievalMethodUtils.DownloadAndApply(Recipe,ITaskHandler,ICommandExecutor)"/> works correctly with <seealso cref="Recipe"/>s.
        /// </summary>
        [Test]
        public void DownloadAndApplyRecipe()
        {
            using (var stream = typeof(ExtractorTest).GetEmbedded("testArchive.zip"))
            using (var microServer = new MicroServer("archive.zip", stream))
            {
                var archive = new Archive {Href = microServer.FileUri};
                var recipe = new Recipe {Steps = {archive}};
                recipe.DownloadAndApply(new SilentTaskHandler()).Dispose();

                archive.MimeType.Should().Be(Archive.MimeTypeZip);
                archive.Size.Should().Be(stream.Length);
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
                typeof(ExtractorTest).GetEmbedded("testArchive.zip").CopyToFile(tempFile);

                var archive = new Archive();
                using (var extractedDir = archive.LocalApply(tempFile, new SilentTaskHandler()))
                    File.Exists(Path.Combine(extractedDir, "symlink")).Should().BeTrue();

                archive.MimeType.Should().Be(Archive.MimeTypeZip);
                archive.Size.Should().Be(new FileInfo(tempFile).Length);

                File.Exists(tempFile).Should().BeTrue(because: "Local reference file should not be removed");
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
                using (var extractedDir = file.LocalApply(tempFile, new SilentTaskHandler()))
                    File.Exists(Path.Combine(extractedDir, "file")).Should().BeTrue();

                file.Destination.Should().Be("file");
                file.Size.Should().Be(3);

                File.Exists(tempFile).Should().BeTrue(because: "Local reference file should not be removed");
            }
        }
    }
}
