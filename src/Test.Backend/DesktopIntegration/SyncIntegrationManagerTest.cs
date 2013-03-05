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
using Common;
using Common.Storage;
using Common.Tasks;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains test methods for <see cref="SyncIntegrationManager"/>.
    /// </summary>
    [TestFixture]
    public sealed class SyncIntegrationManagerTest
    {
        private TemporaryDirectory _tempDir;
        private string _appListPath;

        [SetUp]
        public void SetUp()
        {
            _tempDir = new TemporaryDirectory("0install-unit-tests");
            _appListPath = Path.Combine(_tempDir.Path, "app-list.xml");
        }

        [TearDown]
        public void TearDown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public void TestAddedLocal()
        {
            // Current: 1 entry
            bool apApplied = false;
            bool apUnapplied = false;
            var appEntry = new AppEntry
            {
                InterfaceID = "http://0install.de/feeds/test/test1.xml",
                AccessPoints = new AccessPointList { Entries = { new MockAccessPoint(() => apApplied = true, () => apUnapplied = true) } }
            };
            var appList = new AppList { Entries = { appEntry } };
            appList.SaveXml(_appListPath);

            // Last: 0 entries
            var appListLast = new AppList();
            appListLast.SaveXml(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            // Server: Same as last
            appListLast.SaveXmlZip(_appListPath + ".zip", null);

            AppList sentToServer;
            using (var serverFile = File.OpenRead(_appListPath + ".zip"))
            using (var syncServer = new MicroServer("app-list", serverFile))
            {
                using (var integrationManager = new SyncIntegrationManager(false, _appListPath, syncServer.ServerUri, null, null, null, new SilentTaskHandler()))
                    integrationManager.Sync(SyncResetMode.None, interfaceId => null);

                sentToServer = XmlStorage.LoadXmlZip<AppList>(syncServer.FileContent, null);
            }
            var savedToDisk = XmlStorage.LoadXml<AppList>(_appListPath);
            var savedToDiskLast = XmlStorage.LoadXml<AppList>(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            Assert.IsFalse(apApplied);
            Assert.IsFalse(apUnapplied);
            Assert.AreEqual(appList, sentToServer);
            Assert.AreEqual(appList, savedToDisk);
            Assert.AreEqual(appList, savedToDiskLast);
        }

        [Test]
        public void TestRemovedLocal()
        {
            // Current: 0 entries
            var appList = new AppList();
            appList.SaveXml(_appListPath);

            // Last: 1 entry
            bool apApplied = false;
            bool apUnapplied = false;
            var appEntry = new AppEntry
            {
                InterfaceID = "http://0install.de/feeds/test/test1.xml",
                AccessPoints = new AccessPointList {Entries = {new MockAccessPoint(() => apApplied = true, () => apUnapplied = true)}}
            };
            var appListLast = new AppList {Entries = {appEntry}};
            appListLast.SaveXml(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            // Server: Same as last
            appListLast.SaveXmlZip(_appListPath + ".zip", null);

            AppList sentToServer;
            using (var serverFile = File.OpenRead(_appListPath + ".zip"))
            using (var syncServer = new MicroServer("app-list", serverFile))
            {
                using (var integrationManager = new SyncIntegrationManager(false, _appListPath, syncServer.ServerUri, null, null, null, new SilentTaskHandler()))
                    integrationManager.Sync(SyncResetMode.None, interfaceId => null);

                sentToServer = XmlStorage.LoadXmlZip<AppList>(syncServer.FileContent, null);
            }
            var savedToDisk = XmlStorage.LoadXml<AppList>(_appListPath);
            var savedToDiskLast = XmlStorage.LoadXml<AppList>(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            Assert.IsFalse(apApplied);
            Assert.IsFalse(apUnapplied);
            Assert.AreEqual(appList, sentToServer);
            Assert.AreEqual(appList, savedToDisk);
            Assert.AreEqual(appList, savedToDiskLast);
        }

        [Test]
        public void TestResetClient()
        {
            var appList = new AppList();
            appList.SaveXml(_appListPath);

            var appListLast = new AppList();
            appListLast.SaveXml(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            using (var syncServer = new MicroServer("app-list", new MemoryStream()))
            using (var integrationManager = new SyncIntegrationManager(false, _appListPath, syncServer.ServerUri, null, null, null, new SilentTaskHandler()))
                integrationManager.Sync(SyncResetMode.Client, interfaceId => null);
        }

        [Test]
        public void TestResetServer()
        {
            var appList = new AppList();
            appList.SaveXml(_appListPath);

            var appListLast = new AppList();
            appListLast.SaveXml(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            using (var syncServer = new MicroServer("app-list", new MemoryStream()))
            using (var integrationManager = new SyncIntegrationManager(false, _appListPath, syncServer.ServerUri, null, null, null, new SilentTaskHandler()))
                integrationManager.Sync(SyncResetMode.Server, interfaceId => null);
        }
    }
}
