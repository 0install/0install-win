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

using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Mocks;
using ZeroInstall.Launcher;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Uses mocking to ensure <see cref="StoreSet"/> correctly delegates work to its child <see cref="IStore"/>s.
    /// </summary>
    [TestFixture]
    public class StoreSetTest
    {
        #region Shared
        // Dummy data used by the tests
        private static readonly ManifestDigest _digest1 = new ManifestDigest(null, "abc", null);
        private static readonly ManifestDigest _digest2 = new ManifestDigest(null, "123", null);
        private static readonly ArchiveFileInfo _archive1 = new ArchiveFileInfo { Path = "path1" };
        private static readonly ArchiveFileInfo _archive2 = new ArchiveFileInfo { Path = "path2" };
        private static readonly IEnumerable<ArchiveFileInfo> _archives = new[] { _archive1, _archive2 };
        private static readonly IIOHandler _handler = new SilentHandler();

        private DynamicMock _mock1, _mock2;

        /// <summary>
        /// A <see cref="StoreSet"/> composed of <see cref="_mock1"/> and <see cref="_mock2"/>.
        /// </summary>
        private StoreSet TestStore
        {
            get { return new StoreSet(new[] {(IStore)_mock1.MockInstance, (IStore)_mock2.MockInstance}); }
        }

        [SetUp]
        public void SetUp()
        {
            // Prepare mock objects that will be injected with methods in the tests
            _mock1 = new DynamicMock("MockStore1", typeof(IStore));
            _mock2 = new DynamicMock("MockStore2", typeof(IStore));
        }

        [TearDown]
        public void TearDown()
        {
            // Ensure no method calls were left out
            _mock1.Verify();
            _mock2.Verify();
        }
        #endregion

        #region List all
        [Test]
        public void TestListAll()
        {
            _mock1.ExpectAndReturn("ListAll", new[] {_digest1});
            _mock2.ExpectAndReturn("ListAll", new[] {_digest2});
            CollectionAssert.AreEquivalent(new[] {_digest1, _digest2}, TestStore.ListAll(), "Should combine results from all stores");
        }
        #endregion

        #region Contains
        [Test]
        public void TestContainsFirst()
        {
            _mock1.ExpectAndReturn("Contains", false, _digest1);
            _mock2.ExpectAndReturn("Contains", true, _digest1);
            Assert.IsTrue(TestStore.Contains(_digest1));
        }

        [Test]
        public void TestContainsSecond()
        {
            _mock1.ExpectAndReturn("Contains", true, _digest1);
            _mock2.ExpectNoCall("Contains"); // Don't waste further time once a hit was found
            Assert.IsTrue(TestStore.Contains(_digest1));
        }

        [Test]
        public void TestContainsFalse()
        {
            _mock1.ExpectAndReturn("Contains", false, _digest1);
            _mock2.ExpectAndReturn("Contains", false, _digest1);
            Assert.IsFalse(TestStore.Contains(_digest1));
        }
        #endregion

        #region Get path
        [Test]
        public void TestGetPathFirst()
        {
            _mock1.ExpectAndReturn("Contains", true, _digest1);
            _mock1.ExpectAndReturn("GetPath", "path", _digest1);
            _mock2.ExpectNoCall("Contains"); // Don't waste further time once a hit was found
            _mock2.ExpectNoCall("GetPath");
            Assert.AreEqual("path", TestStore.GetPath(_digest1), "Should get path from first mock");
        }

        [Test]
        public void TestGetPathSecond()
        {
            _mock1.ExpectAndReturn("Contains", false, _digest1);
            _mock1.ExpectNoCall("GetPath");
            _mock2.ExpectAndReturn("Contains", true, _digest1);
            _mock2.ExpectAndReturn("GetPath", "path", _digest1);
            Assert.AreEqual("path", TestStore.GetPath(_digest1), "Should get path from second mock");
        }

        [Test]
        public void TestGetPathFail()
        {
            _mock1.ExpectAndReturn("Contains", false, _digest1);
            _mock1.ExpectNoCall("GetPath");
            _mock2.ExpectAndReturn("Contains", false, _digest1);
            _mock2.ExpectNoCall("GetPath");
            Assert.Throws<ImplementationNotFoundException>(() => TestStore.GetPath(_digest1), "Should pass through fatal exceptions");
        }
        #endregion

        #region Add directory
        [Test]
        public void TestAddDirectoryFirst()
        {
            _mock1.Expect("AddDirectory", "path", _digest1, _handler);
            _mock2.ExpectNoCall("AddDirectory"); // Only add once
            TestStore.AddDirectory("path", _digest1, _handler);
        }

        [Test]
        public void TestAddDirectorySecond()
        {
            _mock1.ExpectAndThrow("AddDirectory", new UnauthorizedAccessException(), "path", _digest1, _handler);
            _mock2.Expect("AddDirectory", "path", _digest1, _handler);
            TestStore.AddDirectory("path", _digest1, _handler);
        }

        [Test]
        public void TestAddDirectoryFail()
        {
            _mock1.ExpectAndThrow("AddDirectory", new UnauthorizedAccessException(), "path", _digest1, _handler);
            _mock2.ExpectAndThrow("AddDirectory", new UnauthorizedAccessException(), "path", _digest1, _handler);
            Assert.Throws<UnauthorizedAccessException>(() => TestStore.AddDirectory("path", _digest1, _handler), "Should pass through fatal exceptions");
        }
        #endregion

        #region Add archive
        [Test]
        public void TestAddArchiveFirst()
        {
            _mock1.Expect("AddArchive", _archive1, _digest1, _handler);
            _mock2.ExpectNoCall("AddArchive"); // Only add once
            TestStore.AddArchive(_archive1, _digest1, _handler);
        }

        [Test]
        public void TestAddArchiveSecond()
        {
            _mock1.ExpectAndThrow("AddArchive", new UnauthorizedAccessException(), _archive1, _digest1, _handler);
            _mock2.Expect("AddArchive", _archive1, _digest1, _handler);
            TestStore.AddArchive(_archive1, _digest1, _handler);
        }

        [Test]
        public void TestAddArchiveFail()
        {
            _mock1.ExpectAndThrow("AddArchive", new UnauthorizedAccessException(), _archive1, _digest1, _handler);
            _mock2.ExpectAndThrow("AddArchive", new UnauthorizedAccessException(), _archive1, _digest1, _handler);
            Assert.Throws<UnauthorizedAccessException>(() => TestStore.AddArchive(_archive1, _digest1, _handler), "Should pass through fatal exceptions");
        }

        [Test]
        public void TestAddMultipleArchivesFirst()
        {
            _mock1.Expect("AddMultipleArchives", _archives, _digest1, _handler);
            _mock2.ExpectNoCall("AddMultipleArchives"); // Only add once
            TestStore.AddMultipleArchives(_archives, _digest1, _handler);
        }

        [Test]
        public void TestAddMultipleArchivesSecond()
        {
            _mock1.ExpectAndThrow("AddMultipleArchives", new UnauthorizedAccessException(), _archives, _digest1, _handler);
            _mock2.Expect("AddMultipleArchives", _archives, _digest1, _handler);
            TestStore.AddMultipleArchives(_archives, _digest1, _handler);
        }

        [Test]
        public void TestAddMultipleArchivesFail()
        {
            _mock1.ExpectAndThrow("AddMultipleArchives", new UnauthorizedAccessException(), _archives, _digest1, _handler);
            _mock2.ExpectAndThrow("AddMultipleArchives", new UnauthorizedAccessException(), _archives, _digest1, _handler);
            Assert.Throws<UnauthorizedAccessException>(() => TestStore.AddMultipleArchives(_archives, _digest1, _handler), "Should pass through fatal exceptions");
        }
        #endregion

        #region Remove
        [Test]
        public void TestRemoveBoth()
        {
            _mock1.ExpectAndReturn("Contains", true, _digest1);
            _mock1.Expect("Remove");
            _mock2.ExpectAndReturn("Contains", true, _digest1);
            _mock2.Expect("Remove");
            TestStore.Remove(_digest1, _handler);
        }

        [Test]
        public void TestRemoveSecond()
        {
            _mock1.ExpectAndReturn("Contains", false, _digest1);
            _mock1.ExpectNoCall("Remove");
            _mock2.ExpectAndReturn("Contains", true, _digest1);
            _mock2.Expect("Remove");
            TestStore.Remove(_digest1, _handler);
        }

        [Test]
        public void TestRemoveFail()
        {
            _mock1.ExpectAndReturn("Contains", false, _digest1);
            _mock1.ExpectNoCall("Remove");
            _mock2.ExpectAndReturn("Contains", false, _digest1);
            _mock2.ExpectNoCall("Remove");
            Assert.Throws<ImplementationNotFoundException>(() => TestStore.Remove(_digest1, _handler), "Should report if none of the stores contained the implementation");
        }
        #endregion

        #region Optimise
        [Test]
        public void TestOptimise()
        {
            _mock1.ExpectAndThrow("Optimise", new UnauthorizedAccessException(), _handler);
            _mock2.Expect("Optimise", _handler);
            TestStore.Optimise(_handler);
        }
        #endregion

        #region Verify
        [Test]
        public void TestVerify()
        {
            _mock1.Expect("Verify", _digest1, _handler);
            _mock2.Expect("Verify", _digest1, _handler);
            TestStore.Verify(_digest1, _handler);
        }
        #endregion

        #region Audit
        [Test]
        public void TestAuditBoth()
        {
            var problem1 = new DigestMismatchException("Problem 1");
            var problem2 = new DigestMismatchException("Problem 2");
            _mock1.ExpectAndReturn("Audit", new[] {problem1}, _handler);
            _mock2.ExpectAndReturn("Audit", new[] {problem2}, _handler);

            // Copy the result into a list to force the enumerator to run through
            var problems = new List<DigestMismatchException>(TestStore.Audit(_handler));
            CollectionAssert.AreEquivalent(new[] {problem1, problem2}, problems, "Should combine results from all stores");
        }

        [Test]
        public void TestAuditPartial()
        {
            var problem = new DigestMismatchException("Problem 1");
            _mock1.ExpectAndReturn("Audit", null, _handler);
            _mock2.ExpectAndReturn("Audit", new[] {problem}, _handler);

            // Copy the result into a list to force the enumerator to run through
            var problems = new List<DigestMismatchException>(TestStore.Audit(_handler));
            CollectionAssert.AreEquivalent(new[] {problem}, problems, "Should combine results from all stores");
        }
        #endregion
    }
}