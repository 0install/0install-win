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

using System;
using System.IO;
using Moq;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NUnit.Framework;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Contains test methods for <see cref="CatalogManager"/>.
    /// </summary>
    [TestFixture]
    public class CatalogManagerTest : TestWithContainer<CatalogManager>
    {
        private Mock<ITrustManager> _trustManagerMock;

        protected override void Register(AutoMockContainer container)
        {
            base.Register(container);

            _trustManagerMock = container.GetMock<ITrustManager>();
        }

        [Test]
        public void TestGetOnline()
        {
            var catalog = CatalogTest.CreateTestCatalog();
            catalog.Normalize();

            var catalogStream = new MemoryStream();
            catalog.SaveXml(catalogStream);
            var array = catalogStream.ToArray();
            catalogStream.Position = 0;

            using (var server = new MicroServer("catalog.xml", catalogStream))
            {
                var uri = new FeedUri(server.FileUri);
                CatalogManager.SetSources(new[] {uri});
                _trustManagerMock.Setup(x => x.CheckTrust(array, uri, null)).Returns(OpenPgpUtilsTest.TestSignature);

                Assert.AreEqual(
                    expected: catalog,
                    actual: Target.GetOnline());
            }
        }

        [Test]
        public void TestGetCached()
        {
            var catalog = CatalogTest.CreateTestCatalog();
            catalog.Normalize();

            Assert.IsNull(Target.GetCached());
            TestGetOnline();
            Assert.AreEqual(
                expected: catalog,
                actual: Target.GetCached());
        }

        private static readonly FeedUri _testSource = new FeedUri("http://localhost/test/");

        [Test]
        public void TestAddSourceExisting()
        {
            Assert.IsFalse(Target.AddSource(CatalogManager.DefaultSource));
            CollectionAssert.AreEqual(
                expected: new[] {CatalogManager.DefaultSource},
                actual: CatalogManager.GetSources());
        }

        [Test]
        public void TestAddSourceNew()
        {
            Assert.IsTrue(Target.AddSource(_testSource));
            CollectionAssert.AreEqual(
                expected: new[] {CatalogManager.DefaultSource, _testSource},
                actual: CatalogManager.GetSources());
        }

        [Test]
        public void TestRemoveSource()
        {
            Assert.IsTrue(Target.RemoveSource(CatalogManager.DefaultSource));
            CollectionAssert.IsEmpty(CatalogManager.GetSources());
        }

        [Test]
        public void TestRemoveSourceMissing()
        {
            Assert.IsFalse(Target.RemoveSource(_testSource));
            CollectionAssert.AreEqual(
                expected: new[] {CatalogManager.DefaultSource},
                actual: CatalogManager.GetSources());
        }

        [Test]
        public void TestSetSources()
        {
            CatalogManager.SetSources(new[] {_testSource});
            CollectionAssert.AreEqual(
                expected: new[] {_testSource},
                actual: CatalogManager.GetSources());
        }
    }
}
