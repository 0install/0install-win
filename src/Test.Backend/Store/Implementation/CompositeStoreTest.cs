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
using Common.Tasks;
using NUnit.Framework;
using Moq;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Uses mocking to ensure <see cref="CompositeStore"/> correctly delegates work to its child <see cref="IStore"/>s.
    /// </summary>
    [TestFixture]
    public class CompositeStoreTest
    {
        #region Shared
        // Dummy data used by the tests
        private static readonly ManifestDigest _digest1 = new ManifestDigest(null, "abc", null);
        private static readonly ManifestDigest _digest2 = new ManifestDigest(null, "123", null);
        private static readonly ArchiveFileInfo _archive1 = new ArchiveFileInfo {Path = "path1"};
        private static readonly ArchiveFileInfo _archive2 = new ArchiveFileInfo {Path = "path2"};
        private static readonly IEnumerable<ArchiveFileInfo> _archives = new[] {_archive1, _archive2};
        private static readonly ITaskHandler _handler = new SilentHandler();

        private Mock<IStore> _mockStore1, _mockStore2;
        private CompositeStore _testStore;

        [SetUp]
        public void SetUp()
        {
            // Prepare mock objects that will be injected with methods in the tests
            _mockStore1 = new Mock<IStore>(MockBehavior.Strict);
            _mockStore2 = new Mock<IStore>(MockBehavior.Strict);

            _testStore = new CompositeStore(_mockStore1.Object, _mockStore2.Object);
        }

        [TearDown]
        public void TearDown()
        {
            // Ensure no method calls were left out
            _mockStore1.Verify();
            _mockStore2.Verify();
        }
        #endregion

        #region List all
        [Test]
        public void TestListAll()
        {
            _mockStore1.Setup(x => x.ListAll()).Returns(new[] {_digest1}).Verifiable();
            _mockStore2.Setup(x => x.ListAll()).Returns(new[] {_digest2}).Verifiable();
            CollectionAssert.AreEquivalent(new[] {_digest1, _digest2}, _testStore.ListAll(), "Should combine results from all stores");
        }
        #endregion

        #region Contains
        [Test]
        public void TestContainsFirst()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(true).Verifiable();
            Assert.IsTrue(_testStore.Contains(_digest1));

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(true).Verifiable();
            Assert.IsTrue(_testStore.Contains("dir1"));
        }

        [Test]
        public void TestContainsSecond()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false).Verifiable();
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(true).Verifiable();
            Assert.IsTrue(_testStore.Contains(_digest1));

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(false).Verifiable();
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(true).Verifiable();
            Assert.IsTrue(_testStore.Contains("dir1"));
        }

        [Test]
        public void TestContainsFalse()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false).Verifiable();
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(false).Verifiable();
            Assert.IsFalse(_testStore.Contains(_digest1));

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(false).Verifiable();
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(false).Verifiable();
            Assert.IsFalse(_testStore.Contains("dir1"));
        }
        #endregion

        #region Get path
        [Test]
        public void TestGetPathFirst()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns("path").Verifiable();
            Assert.AreEqual("path", _testStore.GetPath(_digest1), "Should get path from first mock");
        }

        [Test]
        public void TestGetPathSecond()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns((string)null).Verifiable();
            _mockStore2.Setup(x => x.GetPath(_digest1)).Returns("path").Verifiable();
            Assert.AreEqual("path", _testStore.GetPath(_digest1), "Should get path from second mock");
        }

        [Test]
        public void TestGetPathFail()
        {
            _mockStore1.Setup(x => x.GetPath(_digest1)).Returns((string)null).Verifiable();
            _mockStore2.Setup(x => x.GetPath(_digest1)).Returns((string)null).Verifiable();
            Assert.IsNull(_testStore.GetPath(_digest1));
        }
        #endregion

        #region Add directory
        [Test]
        public void TestAddDirectoryFirst()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler)).Verifiable();
            _testStore.AddDirectory("path", _digest1, _handler);
        }

        [Test]
        public void TestAddDirectorySecond()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing")).Verifiable();
            _mockStore1.Setup(x => x.AddDirectory("path", _digest1, _handler)).Verifiable();
            _testStore.AddDirectory("path", _digest1, _handler);
        }

        [Test]
        public void TestAddDirectoryFail()
        {
            _mockStore2.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing")).Verifiable();
            _mockStore1.Setup(x => x.AddDirectory("path", _digest1, _handler)).Throws(new IOException("Fake IO exception for testing")).Verifiable();
            Assert.Throws<IOException>(() => _testStore.AddDirectory("path", _digest1, _handler), "Should pass through fatal exceptions");
        }
        #endregion

        #region Add archive
        [Test]
        public void TestAddArchivesFirst()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Verifiable();
            _testStore.AddArchives(_archives, _digest1, _handler);
        }

        [Test]
        public void TestAddArchivesSecond()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing")).Verifiable();
            _mockStore1.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Verifiable();
            _testStore.AddArchives(_archives, _digest1, _handler);
        }

        [Test]
        public void TestAddArchivesFail()
        {
            _mockStore2.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing")).Verifiable();
            _mockStore1.Setup(x => x.AddArchives(_archives, _digest1, _handler)).Throws(new IOException("Fake IO exception for testing")).Verifiable();
            Assert.Throws<IOException>(() => _testStore.AddArchives(_archives, _digest1, _handler), "Should pass through fatal exceptions");
        }
        #endregion

        #region Remove
        [Test]
        public void TestRemoveBoth()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(true).Verifiable();
            _mockStore1.Setup(x => x.Remove(_digest1, _handler)).Verifiable();
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(true).Verifiable();
            _mockStore2.Setup(x => x.Remove(_digest1, _handler)).Verifiable();
            _testStore.Remove(_digest1, _handler);

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(true).Verifiable();
            _mockStore1.Setup(x => x.Remove("dir1", _handler)).Verifiable();
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(true).Verifiable();
            _mockStore2.Setup(x => x.Remove("dir1", _handler)).Verifiable();
            _testStore.Remove("dir1", _handler);
        }

        [Test]
        public void TestRemoveSecond()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false).Verifiable();
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(true).Verifiable();
            _mockStore2.Setup(x => x.Remove(_digest1, _handler)).Verifiable();
            _testStore.Remove(_digest1, _handler);

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(false).Verifiable();
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(true).Verifiable();
            _mockStore2.Setup(x => x.Remove("dir1", _handler)).Verifiable();
            _testStore.Remove("dir1", _handler);
        }

        [Test]
        public void TestRemoveFail()
        {
            _mockStore1.Setup(x => x.Contains(_digest1)).Returns(false).Verifiable();
            _mockStore2.Setup(x => x.Contains(_digest1)).Returns(false).Verifiable();
            Assert.Throws<ImplementationNotFoundException>(() => _testStore.Remove(_digest1, _handler), "Should report if none of the stores contained the implementation");

            _mockStore1.Setup(x => x.Contains("dir1")).Returns(false).Verifiable();
            _mockStore2.Setup(x => x.Contains("dir1")).Returns(false).Verifiable();
            Assert.Throws<DirectoryNotFoundException>(() => _testStore.Remove("dir1", _handler), "Should report if none of the stores contained the implementation");
        }
        #endregion

        #region Optimise
        [Test]
        public void TestOptimise()
        {
            _mockStore1.Setup(x => x.Optimise(_handler)).Throws(new IOException("Fake IO exception for testing")).Verifiable();
            _mockStore2.Setup(x => x.Optimise(_handler)).Verifiable();
            Assert.DoesNotThrow(() => _testStore.Optimise(_handler), "Exceptions should be caught and logged");
        }
        #endregion

        #region Verify
        [Test]
        public void TestVerify()
        {
            _mockStore1.Setup(x => x.Verify(_digest1, _handler)).Verifiable();
            _mockStore2.Setup(x => x.Verify(_digest1, _handler)).Verifiable();
            _testStore.Verify(_digest1, _handler);
        }
        #endregion

        #region Audit
        [Test]
        public void TestAuditBoth()
        {
            var problem1 = new DigestMismatchException("Problem 1");
            var problem2 = new DigestMismatchException("Problem 2");
            _mockStore1.Setup(x => x.Audit(_handler)).Returns(new[] {problem1}).Verifiable();
            _mockStore2.Setup(x => x.Audit(_handler)).Returns(new[] {problem2}).Verifiable();

            // Copy the result into a list to force the enumerator to run through
            var problems = new List<DigestMismatchException>(_testStore.Audit(_handler));
            CollectionAssert.AreEquivalent(new[] {problem1, problem2}, problems, "Should combine results from all stores");
        }

        [Test]
        public void TestAuditPartial()
        {
            var problem = new DigestMismatchException("Problem 1");
            _mockStore1.Setup(x => x.Audit(_handler)).Returns(() => null).Verifiable();
            _mockStore2.Setup(x => x.Audit(_handler)).Returns(new[] {problem}).Verifiable();

            // Copy the result into a list to force the enumerator to run through
            var problems = new List<DigestMismatchException>(_testStore.Audit(_handler));
            CollectionAssert.AreEquivalent(new[] {problem}, problems, "Should combine results from all stores");
        }
        #endregion
    }
}
