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

using System;
using System.IO;
using Common;
using Common.Storage;
using Common.Tasks;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;

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
            _appListPath = Path.Combine(_tempDir, "app-list.xml");
        }

        [TearDown]
        public void TearDown()
        {
            _tempDir.Dispose();
        }

        [Test]
        public void TestAddedLocal()
        {
            using (var apApplied = new TemporaryFlagFile("ap-applied"))
            using (var apUnapplied = new TemporaryFlagFile("ap-unapplied"))
            {
                var appListLocal = new AppList
                {
                    Entries =
                    {
                        new AppEntry
                        {
                            InterfaceID = "http://0install.de/feeds/test/test1.xml",
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint{ApplyFlagPath = apApplied, UnapplyFlagPath = apUnapplied}}}
                        }
                    }
                };

                TestSync(appListLocal, new AppList(), new AppList());

                Assert.IsFalse(apApplied.Set, "Locally existing access point should not be reapplied"); 
                Assert.IsFalse(apUnapplied.Set, "Locally existing access point should not be removed");
            }
        }

        [Test]
        public void TestRemovedLocal()
        {
            using (var apApplied = new TemporaryFlagFile("ap-applied"))
            using (var apUnapplied = new TemporaryFlagFile("ap-unapplied"))
            {
                var appListLast = new AppList
                {
                    Entries =
                    {
                        new AppEntry
                        {
                            InterfaceID = "http://0install.de/feeds/test/test1.xml",
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apApplied, UnapplyFlagPath = apUnapplied}}}
                        }
                    }
                };

                TestSync(new AppList(), appListLast, appListLast.Clone());

                Assert.IsFalse(apApplied.Set, "Locally removed access point should not be reapplied");
                Assert.IsFalse(apUnapplied.Set, "Locally removed access point should not be unapplied again");
            }
        }

        [Test]
        public void TestModifiedLocal()
        {
            using (var apLocalApplied = new TemporaryFlagFile("ap-local-applied"))
            using (var apLocalUnapplied = new TemporaryFlagFile("ap-local-unapplied"))
            using (var apRemoteApplied = new TemporaryFlagFile("ap-remote-applied"))
            using (var apRemoteUnapplied = new TemporaryFlagFile("ap-remote-unapplied"))
            {
                var appListLocal = new AppList
                {
                    Entries =
                    {
                        new AppEntry
                        {
                            InterfaceID = "http://0install.de/feeds/test/test1.xml", AutoUpdate = true, Timestamp = new DateTime(2001, 1, 1),
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apLocalApplied, UnapplyFlagPath = apLocalUnapplied}}}
                        }
                    }
                };

                var appListServer = new AppList
                {
                    Entries =
                    {
                        new AppEntry
                        {
                            InterfaceID = "http://0install.de/feeds/test/test1.xml", AutoUpdate = false, Timestamp = new DateTime(2000, 1, 1),
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apRemoteApplied, UnapplyFlagPath = apRemoteUnapplied}}}
                        }
                    }
                };

                TestSync(appListLocal, null, appListServer);

                Assert.IsFalse(apLocalApplied.Set, "Up-to-date access point should not be reapplied");
                Assert.IsFalse(apLocalUnapplied.Set, "Up-to-date access point should not be removed");
                Assert.IsFalse(apRemoteApplied.Set, "Outdated access point should not be reapplied");
                Assert.IsFalse(apRemoteUnapplied.Set, "Outdated access point should not be removed");
            }
        }

        [Test]
        public void TestModifiedRemote()
        {
            using (var apLocalApplied = new TemporaryFlagFile("ap-local-applied"))
            using (var apLocalUnapplied = new TemporaryFlagFile("ap-local-unapplied"))
            using (var apRemoteApplied = new TemporaryFlagFile("ap-remote-applied"))
            using (var apRemoteUnapplied = new TemporaryFlagFile("ap-remote-unapplied"))
            {
                var appListLocal = new AppList
                {
                    Entries =
                    {
                        new AppEntry
                        {
                            InterfaceID = "http://0install.de/feeds/test/test1.xml", AutoUpdate = true, Timestamp = new DateTime(2000, 1, 1),
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apLocalApplied, UnapplyFlagPath = apLocalUnapplied}}}
                        }
                    }
                };

                var appListServer = new AppList
                {
                    Entries =
                    {
                        new AppEntry
                        {
                            InterfaceID = "http://0install.de/feeds/test/test1.xml", AutoUpdate = false, Timestamp = new DateTime(2001, 1, 1),
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apRemoteApplied, UnapplyFlagPath = apRemoteUnapplied}}}
                        }
                    }
                };

                TestSync(appListLocal, null, appListServer);

                Assert.IsFalse(apLocalApplied.Set, "Outdated access point should not be reapplied");
                Assert.IsTrue(apLocalUnapplied.Set, "Outdated access point should be removed");
                Assert.IsTrue(apRemoteApplied.Set, "New access point should be applied");
                Assert.IsFalse(apRemoteUnapplied.Set, "New access point should not be unapplied");
            }
        }

        private void TestSync(AppList appListLocal, AppList appListLast, AppList appListServer)
        {
            appListLocal.SaveXml(_appListPath);
            if (appListLast != null) appListLast.SaveXml(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            appListServer.SaveXmlZip(_appListPath + ".zip", null);
            using (var appListServerFile = File.OpenRead(_appListPath + ".zip"))
            using (var syncServer = new MicroServer("app-list", appListServerFile))
            {
                using (var integrationManager = new SyncIntegrationManager(false, _appListPath, syncServer.ServerUri, null, null, null, new SilentTaskHandler()))
                    integrationManager.Sync(SyncResetMode.None, interfaceId => new Feed());

                appListServer = XmlStorage.LoadXmlZip<AppList>(syncServer.FileContent, null);
            }

            appListLocal = XmlStorage.LoadXml<AppList>(_appListPath);
            appListLast = XmlStorage.LoadXml<AppList>(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);
            Assert.AreEqual(appListLocal, appListServer, "Server and local data should be equal after sync");
            Assert.AreEqual(appListLocal, appListLast, "Last sync snapshot and local data should be equal after sync");
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
