/*
 * Copyright 2010 Roland Leopold Walkling, Bastian Eicher
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

using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ZeroInstall.Services;
using ZeroInstall.Store.Implementations.Archives;
using ZeroInstall.Store.Model;

namespace ZeroInstall.Store.Implementations
{
    /// <summary>
    /// Uses mocking to ensure <see cref="CompositeStore"/> correctly delegates work to its child <see cref="IStore"/>s.
    /// </summary>
    [TestFixture]
    public class CompositeStoreTest : TestWithMocks
    {
        #region Constants
        private static readonly ManifestDigest _digest1 = new ManifestDigest(sha1New: "abc");
        private static readonly ManifestDigest _digest2 = new ManifestDigest(sha1New: "123");
        private static readonly ArchiveFileInfo _archive1 = new ArchiveFileInfo {Path = "path1"};
        private static readonly ArchiveFileInfo _archive2 = new ArchiveFileInfo {Path = "path2"};
        private static readonly IEnumerable<ArchiveFileInfo> _archives = new[] {_archive1, _archive2};
        #endregion

        private MockTaskHandler _handler;
        private Mock<IStore> _mockStore1, _mockStore2;
        private CompositeStore _testStore;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _handler = new MockTaskHandler();

            // Prepare mock objects that will be injected with methods in the tests
            _mockStore1 = CreateMock<IStore>();
            _mockStore2 = CreateMock<IStore>();

            _testStore = new CompositeStore(new[] {_mockStore1.Object, _mockStore2.Object});
        }

        #region List all
        [Test]
        public void TestListAll()
        {
            _mockStore1.Setup(x => x.ListAll()).Returns(new[] {_digest1});
            _mockStore2.Setup(x => x.ListAll()).Returns(new[] {_digest2});
            _testStore.ListAll().Should().BeEquivalentTo(new[] {_digest1, _digest2}, because: "Should combine results from all stores");
        }

        [Test]
        public void TestListAllTemp()
        {
            _mockStore1.Setup(x => x.ListAllTemp()).Returns(new[] {"abc"});
            _mockStore2.Setup(x => x.ListAllTemp()).Returns(new[] {"def"});
            _testStore.ListAllTemp().Should().BeEquivalentTo(new[] {"abc", "def"}, because: "Should combine results from all stores");
        }
        #endregion

        #region Contains
        [Test]
        public void TestContainsFirst()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(true);
            _testStore.Contains(_digest1).Should().BeTrue();

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(true);
            _testStore.Contains("dir1").Should().BeTrue();
        }

        [Test]
        public void TestContainsSecond()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(true);
            _testStore.Contains(_digest1).Should().BeTrue();

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(false);
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(true);
            _testStore.Contains("dir1").Should().BeTrue();
        }

        [Test]
        public void TestContainsFalse()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(false);
            _testStore.Contains(_digest1).Should().BeFalse();

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(false);
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(false);
            _testStore.Contains("dir1").Should().BeFalse();
        }

        [Test]
        public void TestContainsCache()
        {
            // First have underlying store report true
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(true);
            _testStore.Contains(_digest1).Should().BeTrue();

            // Then check the composite cached the result
            _mockStore1.Setup(x => x.Contains(_digest1)).Throws(new AssertionException("Should not call underlying store when result is cached"));
            _testStore.Contains(_digest1).Should().BeTrue();

            // Then clear cache and report different result
            _testStore.Flush();
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(false);
            _testStore.Contains(_digest1).Should().BeFalse();
        }
        #endregion

        #region Get path
        [Test]
        public void TestGetPathFirst()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns("path");
            _testStore.GetPath(_digest1).Should().Be("path", because: "Should get path from first mock");
        }

        [Test]
        public void TestGetPathSecond()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns<string>(null);
            _mockStore2.Setup(x => x.GetPath(_digest1)).Returns("path");
            _testStore.GetPath(_digest1).Should().Be("path", because: "Should get path from second mock");
        }

        [Test]
        public void TestGetPathFail()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns<string>(null);
            _mockStore2.Setup(x => x.GetPath(_digest1)).Returns<string>(null);
            _testStore.GetPath(_digest1).Should().BeNull();
        }
        #endregion

        #region Add directory
        [Test]
        public void TestAddDirectoryFirst()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler)).Returns("");
            _testStore.AddDirectory("path", _digest1, _handler);
        }

        [Test]
        public void TestAddDirectorySecond()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _mockStore1.Setup(x => x.AddDirectory("path", _digest1, _handler)).Returns("");
            _testStore.AddDirectory("path", _digest1, _handler);
        }

        [Test]
        public void TestAddDirectoryFail()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _mockStore1.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _testStore.Invoking(x => x.AddDirectory("path", _digest1, _handler)).ShouldThrow<IOException>(because: "Should pass through fatal exceptions");
        }
        #endregion

        #region Add archive
        [Test]
        public void TestAddArchivesFirst()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Returns("");
            _testStore.AddArchives(_archives, _digest1, _handler);
        }

        [Test]
        public void TestAddArchivesSecond()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _mockStore1.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Returns("");
            _testStore.AddArchives(_archives, _digest1, _handler);
        }

        [Test]
        public void TestAddArchivesFail()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _mockStore1.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _testStore.Invoking(x => x.AddArchives(_archives, _digest1, _handler)).ShouldThrow<IOException>(because: "Should pass through fatal exceptions");
        }
        #endregion

        #region Remove
        [Test]
        public void TestRemoveTwo()
        {
            _mockStore1.Setup(x => x.Remove(_digest1, _handler)).Returns(true);
            _mockStore2.Setup(x => x.Remove(_digest1, _handler)).Returns(true);
            _testStore.Remove(_digest1, _handler).Should().BeTrue();
        }

        [Test]
        public void TestRemoveOne()
        {
            _mockStore1.Setup(x => x.Remove(_digest1, _handler)).Returns(false);
            _mockStore2.Setup(x => x.Remove(_digest1, _handler)).Returns(true);
            _testStore.Remove(_digest1, _handler).Should().BeTrue();
        }

        [Test]
        public void TestRemoveNone()
        {
            _mockStore1.Setup(x => x.Remove(_digest1, _handler)).Returns(false);
            _mockStore2.Setup(x => x.Remove(_digest1, _handler)).Returns(false);
            _testStore.Remove(_digest1, _handler).Should().BeFalse();
        }
        #endregion

        #region Verify
        [Test]
        public void TestVerify()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(true);
            _mockStore2.Setup(x => x.Verify(_digest1, _handler));

            _testStore.Verify(_digest1, _handler);
        }
        #endregion
    }
}
