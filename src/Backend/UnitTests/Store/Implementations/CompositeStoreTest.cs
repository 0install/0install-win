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
using Moq;
using NanoByte.Common;
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
            _mockStore1 = MockRepository.Create<IStore>();
            _mockStore2 = MockRepository.Create<IStore>();

            _testStore = new CompositeStore(new[] {_mockStore1.Object, _mockStore2.Object});
        }

        #region List all
        [Test]
        public void TestListAll()
        {
            _mockStore1.Setup(x => x.ListAll()).Returns(new[] {_digest1});
            _mockStore2.Setup(x => x.ListAll()).Returns(new[] {_digest2});
            CollectionAssert.AreEquivalent(new[] {_digest1, _digest2}, _testStore.ListAll(), "Should combine results from all stores");
        }

        [Test]
        public void TestListAllTemp()
        {
            _mockStore1.Setup(x => x.ListAllTemp()).Returns(new[] {"abc"});
            _mockStore2.Setup(x => x.ListAllTemp()).Returns(new[] {"def"});
            CollectionAssert.AreEquivalent(new[] {"abc", "def"}, _testStore.ListAllTemp(), "Should combine results from all stores");
        }
        #endregion

        #region Contains
        [Test]
        public void TestContainsFirst()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(true);
            Assert.IsTrue(_testStore.Contains(_digest1));

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(true);
            Assert.IsTrue(_testStore.Contains("dir1"));
        }

        [Test]
        public void TestContainsSecond()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(true);
            Assert.IsTrue(_testStore.Contains(_digest1));

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(false);
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(true);
            Assert.IsTrue(_testStore.Contains("dir1"));
        }

        [Test]
        public void TestContainsFalse()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(false);
            Assert.IsFalse(_testStore.Contains(_digest1));

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(false);
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(false);
            Assert.IsFalse(_testStore.Contains("dir1"));
        }

        [Test]
        public void TestContainsCache()
        {
            // First have underlying store report true
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(true);
            Assert.IsTrue(_testStore.Contains(_digest1));

            // Then check the composite cached the result
            _mockStore1.Setup(x => x.Contains(_digest1)).Throws(new AssertionException("Should not call underlying store when result is cached"));
            Assert.IsTrue(_testStore.Contains(_digest1));

            // Then clear cache and report different result
            _testStore.Flush();
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(false);
            Assert.IsFalse(_testStore.Contains(_digest1));
        }
        #endregion

        #region Get path
        [Test]
        public void TestGetPathFirst()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns("path");
            Assert.AreEqual("path", _testStore.GetPath(_digest1), "Should get path from first mock");
        }

        [Test]
        public void TestGetPathSecond()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns((string)null);
            _mockStore2.Setup(x => x.GetPath(_digest1)).Returns("path");
            Assert.AreEqual("path", _testStore.GetPath(_digest1), "Should get path from second mock");
        }

        [Test]
        public void TestGetPathFail()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns((string)null);
            _mockStore2.Setup(x => x.GetPath(_digest1)).Returns((string)null);
            Assert.IsNull(_testStore.GetPath(_digest1));
        }
        #endregion

        #region Add directory
        [Test]
        public void TestAddDirectoryFirst()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler));
            _testStore.AddDirectory("path", _digest1, _handler);
        }

        [Test]
        public void TestAddDirectorySecond()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _mockStore1.Setup(x => x.AddDirectory("path", _digest1, _handler));
            _testStore.AddDirectory("path", _digest1, _handler);
        }

        [Test]
        public void TestAddDirectoryFail()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _mockStore1.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            Assert.Throws<IOException>(() => _testStore.AddDirectory("path", _digest1, _handler), "Should pass through fatal exceptions");
        }
        #endregion

        #region Add archive
        [Test]
        public void TestAddArchivesFirst()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler));
            _testStore.AddArchives(_archives, _digest1, _handler);
        }

        [Test]
        public void TestAddArchivesSecond()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _mockStore1.Setup(x => x.AddArchives(_archives, _digest1, _handler));
            _testStore.AddArchives(_archives, _digest1, _handler);
        }

        [Test]
        public void TestAddArchivesFail()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            _mockStore1.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing"));
            Assert.Throws<IOException>(() => _testStore.AddArchives(_archives, _digest1, _handler), "Should pass through fatal exceptions");
        }
        #endregion

        #region Remove
        [Test]
        public void TestRemoveBoth()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(true);
            _mockStore1.Setup(x => x.Remove(_digest1));
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(true);
            _mockStore2.Setup(x => x.Remove(_digest1));
            _testStore.Remove(_digest1);
        }

        [Test]
        public void TestRemoveSecond()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(true);
            _mockStore2.Setup(x => x.Remove(_digest1));
            _testStore.Remove(_digest1);
        }

        [Test]
        public void TestRemoveFail()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false);
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(false);
            Assert.Throws<ImplementationNotFoundException>(() => _testStore.Remove(_digest1), "Should report if none of the stores contained the implementation");
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
