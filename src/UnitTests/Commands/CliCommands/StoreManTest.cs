/*
 * Copyright 2010-2016 Bastian Eicher
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

using System.Globalization;
using System.IO;
using Moq;
using NanoByte.Common.Storage;
using Xunit;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store.Feeds;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.ViewModel;

namespace ZeroInstall.Commands.CliCommands
{
    /// <summary>
    /// Contains integration tests for <see cref="StoreMan"/>.
    /// </summary>
    [Collection("WorkingDir")]
    public class StoreManTest
    {
        internal abstract class StoreSubCommand<T> : CliCommandTest<T>
            where T:StoreMan.StoreSubCommand
        {
            protected Mock<IStore> StoreMock => GetMock<IStore>();
        }

            internal class Add : StoreSubCommand<StoreMan.Add>
        {
            [Fact]
            public void Archive()
            {
                using (var tempFile = new TemporaryFile("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path = tempFile;
                    StoreMock.Setup(x => x.AddArchives(new[]
                    {
                        new ArchiveFileInfo {Path = path}
                    }, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK,
                        "sha256new_" + digest.Sha256New, path);
                }
            }

            [Fact]
            public void ArchiveRelativePath()
            {
                using (var tempDir = new TemporaryWorkingDirectory("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path = Path.Combine(tempDir, "archive");
                    File.WriteAllText(path, "xyz");
                    StoreMock.Setup(x => x.AddArchives(new[]
                    {
                        new ArchiveFileInfo {Path = path}
                    }, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK,
                        "sha256new_" + digest.Sha256New, "archive");
                }
            }

            [Fact]
            public void ArchiveExtract()
            {
                using (var tempFile = new TemporaryFile("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path = tempFile;
                    StoreMock.Setup(x => x.AddArchives(new[]
                    {
                        new ArchiveFileInfo {Path = path, Extract = "extract"}
                    }, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK,
                        "sha256new_" + digest.Sha256New, path, "extract");
                }
            }

            [Fact]
            public void ArchiveExtractMime()
            {
                using (var tempFile = new TemporaryFile("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path = tempFile;
                    StoreMock.Setup(x => x.AddArchives(new[]
                    {
                        new ArchiveFileInfo {Path = path, Extract = "extract", MimeType = "mime"}
                    }, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK,
                        "sha256new_" + digest.Sha256New, path, "extract", "mime");
                }
            }

            [Fact]
            public void MultipleArchives()
            {
                using (var tempFile1 = new TemporaryFile("0install-unit-tests"))
                using (var tempFile2 = new TemporaryFile("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path1 = tempFile1;
                    string path2 = tempFile2;
                    StoreMock.Setup(x => x.AddArchives(new[]
                    {
                        new ArchiveFileInfo {Path = path1, Extract = "extract1", MimeType = "mime1"},
                        new ArchiveFileInfo {Path = path2, Extract = "extract2", MimeType = "mime2"}
                    }, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK,
                        "sha256new_" + digest.Sha256New,
                        path1, "extract1", "mime1",
                        path2, "extract2", "mime2");
                }
            }

            [Fact]
            public void Directory()
            {
                using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path = tempDir;
                    StoreMock.Setup(x => x.AddDirectory(path, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK,
                        "sha256new_" + digest.Sha256New, path);
                }
            }

            [Fact]
            public void DirectoryRelativePath()
            {
                using (var tempDir = new TemporaryWorkingDirectory("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path = tempDir;
                    StoreMock.Setup(x => x.AddDirectory(path, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK,
                        "sha256new_" + digest.Sha256New, ".");
                }
            }
        }

            internal class Audit : StoreSubCommand<StoreMan.Audit>
        {
            [Fact]
            public void TestAudit()
            {
                var storeMock = StoreMock;
                storeMock.Setup(x => x.ListAll()).Returns(new[] {new ManifestDigest("sha256new_123AB")});
                storeMock.Setup(x => x.Verify(new ManifestDigest("sha256new_123AB"), Handler));

                RunAndAssert(null, ExitCode.OK);
            }
        }

            internal class Copy : StoreSubCommand<StoreMan.Copy>
        {
            [Fact]
            public void Normal()
            {
                using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path = Path.Combine(tempDir, "sha256new_" + digest.Sha256New);
                    StoreMock.Setup(x => x.AddDirectory(path, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK, path);
                }
            }

            [Fact]
            public void RelativePath()
            {
                using (var tempDir = new TemporaryWorkingDirectory("0install-unit-tests"))
                {
                    var digest = new ManifestDigest(sha256New: "abc");
                    string path = Path.Combine(tempDir, "sha256new_" + digest.Sha256New);
                    StoreMock.Setup(x => x.AddDirectory(path, digest, Handler)).Returns("");

                    RunAndAssert(null, ExitCode.OK,
                        "sha256new_" + digest.Sha256New);
                }
            }
        }

            internal class Find : StoreSubCommand<StoreMan.Find>
        {
            [Fact]
            public void Test()
            {
                var digest = new ManifestDigest(sha256New: "abc");
                StoreMock.Setup(x => x.GetPath(digest)).Returns("path");

                RunAndAssert("path", ExitCode.OK,
                    "sha256new_abc");
            }
        }

            internal class List : StoreSubCommand<StoreMan.List>
        {
            [Fact]
            public void Test()
            {
                RunAndAssert(new[] {StoreMock.Object}, ExitCode.OK);
            }
        }

            internal class ListImplementations : StoreSubCommand<StoreMan.ListImplementations>
        {
            [Fact]
            public void ListAll()
            {
                var testFeed = Fake.Feed;
                var testImplementation = (Implementation)testFeed.Elements[0];
                var digest1 = testImplementation.ManifestDigest;
                var digest2 = new ManifestDigest(sha256New: "2");

                using (var tempDir = new TemporaryDirectory("0install-unit-tests"))
                {
                    GetMock<IFeedCache>().Setup(x => x.ListAll()).Returns(new[] {testFeed.Uri});
                    GetMock<IFeedCache>().Setup(x => x.GetFeed(testFeed.Uri)).Returns(testFeed);
                    StoreMock.Setup(x => x.ListAll()).Returns(new[] {digest1, digest2});
                    StoreMock.Setup(x => x.ListAllTemp()).Returns(new string[0]);
                    StoreMock.Setup(x => x.GetPath(It.IsAny<ManifestDigest>())).Returns(tempDir);
                    FileUtils.Touch(Path.Combine(tempDir, ".manifest"));

                    var feedNode = new FeedNode(testFeed, Sut.FeedCache);
                    RunAndAssert(new ImplementationNode[] {new OwnedImplementationNode(digest1, testImplementation, feedNode, Sut.Store), new OrphanedImplementationNode(digest2, Sut.Store)}, ExitCode.OK);
                }
            }
        }

            internal class Optimise : StoreSubCommand<StoreMan.Optimise>
        {
            [Fact]
            public void Test()
            {
                StoreMock.Setup(x => x.Optimise(Handler)).Returns(123);

                RunAndAssert(string.Format(Resources.StorageReclaimed, FileUtils.FormatBytes(123, CultureInfo.CurrentCulture)), ExitCode.OK);
            }
        }

            internal class Purge : StoreSubCommand<StoreMan.Purge>
        {
            [Fact]
            public void Test()
            {
                var digest = new ManifestDigest(sha256New: "abc");
                StoreMock.Setup(x => x.ListAll()).Returns(new[] {digest});
                StoreMock.Setup(x => x.Remove(digest, Handler)).Returns(true);

                Handler.AnswerQuestionWith = true;
                RunAndAssert(null, ExitCode.OK);
            }
        }

            internal class Remove : StoreSubCommand<StoreMan.Remove>
        {
            [Fact]
            public void Test()
            {
                var digest = new ManifestDigest(sha256New: "abc");
                StoreMock.Setup(x => x.Remove(digest, Handler)).Returns(true);

                RunAndAssert(null, ExitCode.OK,
                    "sha256new_abc");
            }
        }

            internal class Verify : StoreSubCommand<StoreMan.Verify>
        {
            [Fact]
            public void Pass()
            {
                var digest = new ManifestDigest(sha256New: "abc");
                StoreMock.Setup(x => x.Verify(digest, Handler));

                RunAndAssert(null, ExitCode.OK,
                    "sha256new_" + digest.Sha256New);
            }

            [Fact]
            public void Fail()
            {
                var digest = new ManifestDigest(sha256New: "abc");
                StoreMock.Setup(x => x.Verify(digest, Handler)).Throws<DigestMismatchException>();

                RunAndAssert(new DigestMismatchException().Message, ExitCode.DigestMismatch,
                    "sha256new_" + digest.Sha256New);
            }
        }
    }
}
