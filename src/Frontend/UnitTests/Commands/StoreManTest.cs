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

using System;
using System.Globalization;
using System.IO;
using Moq;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Contains integration tests for <see cref="StoreMan"/>.
    /// </summary>
    [TestFixture]
    public class StoreManTest : FrontendCommandTest<StoreMan>
    {
        [Test]
        public void TestAddArchive()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path = tempFile;
                StoreMock.Setup(x => x.AddArchives(new[]
                {
                    new ArchiveFileInfo {Path = path}
                }, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "add", "sha256new_" + digest.Sha256New, path);
            }
        }

        [Test]
        public void TestAddArchiveRelativePath()
        {
            using (var tempDir = new TemporaryWorkingDirectory("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path = Path.Combine(tempDir, "archive");
                File.WriteAllText(path, "xyz");
                StoreMock.Setup(x => x.AddArchives(new[]
                {
                    new ArchiveFileInfo {Path = path}
                }, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "add", "sha256new_" + digest.Sha256New, "archive");
            }
        }

        [Test]
        public void TestAddArchiveExtract()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path = tempFile;
                StoreMock.Setup(x => x.AddArchives(new[]
                {
                    new ArchiveFileInfo {Path = path, SubDir = "extract"}
                }, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "add", "sha256new_" + digest.Sha256New, path, "extract");
            }
        }

        [Test]
        public void TestAddArchiveExtractMime()
        {
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path = tempFile;
                StoreMock.Setup(x => x.AddArchives(new[]
                {
                    new ArchiveFileInfo {Path = path, SubDir = "extract", MimeType = "mime"}
                }, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "add", "sha256new_" + digest.Sha256New, path, "extract", "mime");
            }
        }

        [Test]
        public void TestAddMultipleArchives()
        {
            using (var tempFile1 = new TemporaryFile("0install-unit-tests"))
            using (var tempFile2 = new TemporaryFile("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path1 = tempFile1;
                string path2 = tempFile2;
                StoreMock.Setup(x => x.AddArchives(new[]
                {
                    new ArchiveFileInfo {Path = path1, SubDir = "extract1", MimeType = "mime1"},
                    new ArchiveFileInfo {Path = path2, SubDir = "extract2", MimeType = "mime2"}
                }, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "add", "sha256new_" + digest.Sha256New,
                    path1, "extract1", "mime1",
                    path2, "extract2", "mime2");
            }
        }

        [Test]
        public void TestAddDirectory()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path = tempDir;
                StoreMock.Setup(x => x.AddDirectory(path, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "add", "sha256new_" + digest.Sha256New, path);
            }
        }

        [Test]
        public void TestAddDirectoryRelativePath()
        {
            using (var tempDir = new TemporaryWorkingDirectory("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path = tempDir;
                StoreMock.Setup(x => x.AddDirectory(path, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "add", "sha256new_" + digest.Sha256New, ".");
            }
        }

        [Test]
        public void TestAudit()
        {
            var storeMock = StoreMock;
            storeMock.Setup(x => x.ListAll()).Returns(new[] {new ManifestDigest("sha256new_123AB")});
            storeMock.Setup(x => x.Verify(new ManifestDigest("sha256new_123AB"), MockHandler));

            RunAndAssert(null, (int)StoreErrorLevel.OK,
                "audit");
        }

        [Test]
        public void TestCopy()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path = Path.Combine(tempDir, "sha256new_" + digest.Sha256New);
                StoreMock.Setup(x => x.AddDirectory(path, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "copy", path);
            }
        }

        [Test]
        public void TestCopyyRelativePath()
        {
            using (var tempDir = new TemporaryWorkingDirectory("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                string path = Path.Combine(tempDir, "sha256new_" + digest.Sha256New);
                StoreMock.Setup(x => x.AddDirectory(path, digest, Resolve<ICommandHandler>()));

                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "copy", "sha256new_" + digest.Sha256New);
            }
        }

        [Test]
        public void TestFind()
        {
            var digest = new ManifestDigest(sha256New: "abc");
            StoreMock.Setup(x => x.GetPath(digest)).Returns("path");

            RunAndAssert("path", (int)StoreErrorLevel.OK,
                "find", "sha256new_abc");
        }

        [Test]
        public void TestList()
        {
            var digest1 = new ManifestDigest(sha256New: "1");
            var digest2 = new ManifestDigest(sha256New: "2");
            StoreMock.Setup(x => x.ListAll()).Returns(new[] {digest1, digest2});
            StoreMock.Setup(x => x.GetPath(It.IsAny<ManifestDigest>())).Returns((ManifestDigest x) => x.Sha256New);

            RunAndAssert("1" + Environment.NewLine + "2", (int)StoreErrorLevel.OK,
                "list");
        }

        [Test]
        public void TestOptimise()
        {
            StoreMock.Setup(x => x.Optimise(Resolve<ICommandHandler>())).Returns(123);

            RunAndAssert(string.Format(Resources.StorageReclaimed, FileUtils.FormatBytes(123, CultureInfo.CurrentCulture)), (int)StoreErrorLevel.OK,
                "optimise");
        }

        [Test]
        public void TestPurge()
        {
            using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
            {
                var digest = new ManifestDigest(sha256New: "abc");
                StoreMock.Setup(x => x.ListAll()).Returns(new[] {digest});
                StoreMock.Setup(x => x.Remove(digest)).Returns(true);
                StoreMock.Setup(x => x.ListAllTemp()).Returns(new[] {tempDir.Path});

                MockHandler.AnswerQuestionWith = true;
                RunAndAssert(null, (int)StoreErrorLevel.OK,
                    "purge");
                Assert.IsFalse(Directory.Exists(tempDir), "Temporary directory should have been deleted");
            }
        }

        [Test]
        public void TestRemove()
        {
            var digest = new ManifestDigest(sha256New: "abc");
            StoreMock.Setup(x => x.Remove(digest)).Returns(true);

            RunAndAssert(null, (int)StoreErrorLevel.OK,
                "remove", "sha256new_abc");
        }

        [Test]
        public void TestVerifyPass()
        {
            var digest = new ManifestDigest(sha256New: "abc");
            StoreMock.Setup(x => x.Verify(digest, Resolve<ICommandHandler>()));

            RunAndAssert(null, (int)StoreErrorLevel.OK,
                "verify", "sha256new_" + digest.Sha256New);
        }

        [Test]
        public void TestVerifyFail()
        {
            var digest = new ManifestDigest(sha256New: "abc");
            StoreMock.Setup(x => x.Verify(digest, Resolve<ICommandHandler>())).Throws<DigestMismatchException>();

            RunAndAssert(new DigestMismatchException().Message, (int)StoreErrorLevel.DigestMismatch,
                "verify", "sha256new_" + digest.Sha256New);
        }
    }
}
