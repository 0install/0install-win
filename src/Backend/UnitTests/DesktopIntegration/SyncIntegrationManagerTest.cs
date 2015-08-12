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

using System;
using System.IO;
using FluentAssertions;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains test methods for <see cref="SyncIntegrationManager"/>.
    /// </summary>
    [TestFixture]
    public sealed class SyncIntegrationManagerTest
    {
        #region Common
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
        #endregion

        #region Individual
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
                            InterfaceUri = FeedTest.Test1Uri,
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apApplied, UnapplyFlagPath = apUnapplied}}}
                        }
                    }
                };

                TestSync(SyncResetMode.None, appListLocal, new AppList(), new AppList());

                apApplied.Set.Should().BeFalse(because: "Locally existing access point should not be reapplied");
                apUnapplied.Set.Should().BeFalse(because: "Locally existing access point should not be removed");
            }
        }

        [Test]
        public void TestRemovedLocal()
        {
            using (var apApplied = new TemporaryFlagFile("ap-applied"))
            using (var apUnapplied = new TemporaryFlagFile("ap-unapplied"))
            {
                var appListServer = new AppList
                {
                    Entries =
                    {
                        new AppEntry
                        {
                            InterfaceUri = FeedTest.Test1Uri,
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apApplied, UnapplyFlagPath = apUnapplied}}}
                        }
                    }
                };

                TestSync(SyncResetMode.None, new AppList(), appListServer.Clone(), appListServer);

                apApplied.Set.Should().BeFalse(because: "Locally removed access point should not be reapplied");
                apUnapplied.Set.Should().BeFalse(because: "Locally removed access point should not be unapplied again");
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
                            InterfaceUri = FeedTest.Test1Uri, AutoUpdate = true, Timestamp = new DateTime(2001, 1, 1),
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
                            InterfaceUri = FeedTest.Test1Uri, AutoUpdate = false, Timestamp = new DateTime(2000, 1, 1),
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apRemoteApplied, UnapplyFlagPath = apRemoteUnapplied}}}
                        }
                    }
                };

                TestSync(SyncResetMode.None, appListLocal, null, appListServer);

                apLocalApplied.Set.Should().BeFalse(because: "Up-to-date access point should not be reapplied");
                apLocalUnapplied.Set.Should().BeFalse(because: "Up-to-date access point should not be removed");
                apRemoteApplied.Set.Should().BeFalse(because: "Outdated access point should not be reapplied");
                apRemoteUnapplied.Set.Should().BeFalse(because: "Outdated access point should not be removed");
            }
        }

        [Test]
        public void TestAddedRemote()
        {
            using (var apApplied = new TemporaryFlagFile("ap-applied"))
            using (var apUnapplied = new TemporaryFlagFile("ap-unapplied"))
            {
                var appListRemote = new AppList
                {
                    Entries =
                    {
                        new AppEntry
                        {
                            InterfaceUri = FeedTest.Test1Uri,
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apApplied, UnapplyFlagPath = apUnapplied}, new MockAccessPoint()}}
                        }
                    }
                };

                TestSync(SyncResetMode.None, new AppList(), new AppList(), appListRemote);

                apApplied.Set.Should().BeTrue(because: "New access point should be applied");
                apUnapplied.Set.Should().BeFalse(because: "New access point should not be unapplied");
            }
        }

        [Test]
        public void TestRemovedRemote()
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
                            InterfaceUri = FeedTest.Test1Uri,
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apApplied, UnapplyFlagPath = apUnapplied}}}
                        }
                    }
                };

                TestSync(SyncResetMode.None, appListLocal, appListLocal.Clone(), new AppList());

                apApplied.Set.Should().BeFalse(because: "Removed access point should not be reapplied");
                apUnapplied.Set.Should().BeTrue(because: "Removed point should be unapplied");
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
                            InterfaceUri = FeedTest.Test1Uri, AutoUpdate = true, Timestamp = new DateTime(1999, 1, 1, 0, 0, 0, DateTimeKind.Utc),
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
                            InterfaceUri = FeedTest.Test1Uri, AutoUpdate = false, Timestamp = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                            AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = apRemoteApplied, UnapplyFlagPath = apRemoteUnapplied}}}
                        }
                    }
                };

                TestSync(SyncResetMode.None, appListLocal, null, appListServer);

                apLocalApplied.Set.Should().BeFalse(because: "Outdated access point should not be reapplied");
                apLocalUnapplied.Set.Should().BeTrue(because: "Outdated access point should be removed");
                apRemoteApplied.Set.Should().BeTrue(because: "New access point should be applied");
                apRemoteUnapplied.Set.Should().BeFalse(because: "New access point should not be unapplied");
            }
        }

        /// <summary>
        /// Tests the sync logic with custom <see cref="AppList"/>s.
        /// </summary>
        /// <param name="resetMode">The <see cref="SyncResetMode"/> to pass to <see cref="SyncIntegrationManager.Sync"/>.</param>
        /// <param name="appListLocal">The current local <see cref="AppList"/>.</param>
        /// <param name="appListLast">The state of the <see cref="AppList"/> after the last successful sync.</param>
        /// <param name="appListServer">The current server-side <see cref="AppList"/>.</param>
        private void TestSync(SyncResetMode resetMode, AppList appListLocal, AppList appListLast, AppList appListServer)
        {
            appListLocal.SaveXml(_appListPath);
            appListLast?.SaveXml(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            using (var stream = File.Create(_appListPath + ".zip"))
                appListServer.SaveXmlZip(stream);

            using (var appListServerFile = File.OpenRead(_appListPath + ".zip"))
            using (var syncServer = new MicroServer("app-list", appListServerFile))
            {
                using (var integrationManager = new SyncIntegrationManager(_appListPath, new SyncServer {Uri = syncServer.ServerUri, Username = "user", Password = "pass"}, interfaceId => new Feed(), new SilentTaskHandler()))
                    integrationManager.Sync(resetMode);

                appListServer = AppList.LoadXmlZip(syncServer.FileContent);
            }

            appListLocal = XmlStorage.LoadXml<AppList>(_appListPath);
            appListLast = XmlStorage.LoadXml<AppList>(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);
            appListServer.Should().Be(appListLocal, because: "Server and local data should be equal after sync");
            appListLast.Should().Be(appListLocal, because: "Last sync snapshot and local data should be equal after sync");
        }
        #endregion

        #region Composite
        [Test]
        public void TestMixed()
        {
            using (var ap1Applied = new TemporaryFlagFile("ap1-applied"))
            using (var ap1Unapplied = new TemporaryFlagFile("ap1-unapplied"))
            using (var ap2Applied = new TemporaryFlagFile("ap2-applied"))
            using (var ap2Unapplied = new TemporaryFlagFile("ap2-unapplied"))
            using (var ap3Applied = new TemporaryFlagFile("ap3-applied"))
            using (var ap3Unapplied = new TemporaryFlagFile("ap3-unapplied"))
            using (var ap4Applied = new TemporaryFlagFile("ap4-applied"))
            using (var ap4Unapplied = new TemporaryFlagFile("ap4-unapplied"))
            {
                TestSync(SyncResetMode.None, ap1Applied, ap1Unapplied, ap2Applied, ap2Unapplied, ap3Applied, ap3Unapplied, ap4Applied, ap4Unapplied);
                ap1Applied.Set.Should().BeFalse();
                ap1Unapplied.Set.Should().BeFalse();
                ap2Applied.Set.Should().BeFalse();
                ap2Unapplied.Set.Should().BeFalse();
                ap3Applied.Set.Should().BeTrue(because: "remote add: appEntry3");
                ap3Unapplied.Set.Should().BeFalse();
                ap4Applied.Set.Should().BeFalse();
                ap4Unapplied.Set.Should().BeTrue(because: "remote remove: appEntry4");
            }
        }

        [Test]
        public void TestResetClient()
        {
            using (var ap1Applied = new TemporaryFlagFile("ap1-applied"))
            using (var ap1Unapplied = new TemporaryFlagFile("ap1-unapplied"))
            using (var ap2Applied = new TemporaryFlagFile("ap2-applied"))
            using (var ap2Unapplied = new TemporaryFlagFile("ap2-unapplied"))
            using (var ap3Applied = new TemporaryFlagFile("ap3-applied"))
            using (var ap3Unapplied = new TemporaryFlagFile("ap3-unapplied"))
            using (var ap4Applied = new TemporaryFlagFile("ap4-applied"))
            using (var ap4Unapplied = new TemporaryFlagFile("ap4-unapplied"))
            {
                TestSync(SyncResetMode.Client, ap1Applied, ap1Unapplied, ap2Applied, ap2Unapplied, ap3Applied, ap3Unapplied, ap4Applied, ap4Unapplied);
                ap1Applied.Set.Should().BeFalse();
                ap1Unapplied.Set.Should().BeTrue(because: "undo: local add: appEntry1");
                ap2Applied.Set.Should().BeTrue(because: "undo: local remove: appEntry2");
                ap2Unapplied.Set.Should().BeFalse();
                ap3Applied.Set.Should().BeTrue(because: "remote add: appEntry3");
                ap3Unapplied.Set.Should().BeFalse();
                ap4Applied.Set.Should().BeFalse();
                ap4Unapplied.Set.Should().BeTrue(because: "remote remove: appEntry4");
            }
        }

        [Test]
        public void TestResetServer()
        {
            using (var ap1Applied = new TemporaryFlagFile("ap1-applied"))
            using (var ap1Unapplied = new TemporaryFlagFile("ap1-unapplied"))
            using (var ap2Applied = new TemporaryFlagFile("ap2-applied"))
            using (var ap2Unapplied = new TemporaryFlagFile("ap2-unapplied"))
            using (var ap3Applied = new TemporaryFlagFile("ap3-applied"))
            using (var ap3Unapplied = new TemporaryFlagFile("ap3-unapplied"))
            using (var ap4Applied = new TemporaryFlagFile("ap4-applied"))
            using (var ap4Unapplied = new TemporaryFlagFile("ap4-unapplied"))
            {
                TestSync(SyncResetMode.Server, ap1Applied, ap1Unapplied, ap2Applied, ap2Unapplied, ap3Applied, ap3Unapplied, ap4Applied, ap4Unapplied);
                ap1Applied.Set.Should().BeFalse();
                ap1Unapplied.Set.Should().BeFalse();
                ap2Applied.Set.Should().BeFalse();
                ap2Unapplied.Set.Should().BeFalse();
                ap3Applied.Set.Should().BeFalse();
                ap3Unapplied.Set.Should().BeFalse();
                ap4Applied.Set.Should().BeFalse();
                ap4Unapplied.Set.Should().BeFalse();
            }
        }

        /// <summary>
        /// Tests the sync logic with pre-defined <see cref="AppList"/>s.
        /// local add: appEntry1, local remove: appEntry2, remote add: appEntry3, remote remove: appEntry4
        /// </summary>
        /// <param name="resetMode">The <see cref="SyncResetMode"/> to pass to <see cref="SyncIntegrationManager.Sync"/>.</param>
        /// <param name="ap1Applied">The flag file used to indicate that <see cref="MockAccessPoint.Apply"/> was called for appEntry1.</param>
        /// <param name="ap1Unapplied">The flag file used to indicate that <see cref="MockAccessPoint.Unapply"/> was called for appEntry1.</param>
        /// <param name="ap2Applied">The flag file used to indicate that <see cref="MockAccessPoint.Apply"/> was called for appEntry2.</param>
        /// <param name="ap2Unapplied">The flag file used to indicate that <see cref="MockAccessPoint.Unapply"/> was called for appEntry2.</param>
        /// <param name="ap3Applied">The flag file used to indicate that <see cref="MockAccessPoint.Apply"/> was called for appEntry3.</param>
        /// <param name="ap3Unapplied">The flag file used to indicate that <see cref="MockAccessPoint.Unapply"/> was called for appEntry3.</param>
        /// <param name="ap4Applied">The flag file used to indicate that <see cref="MockAccessPoint.Apply"/> was called for appEntry4.</param>
        /// <param name="ap4Unapplied">The flag file used to indicate that <see cref="MockAccessPoint.Unapply"/> was called for appEntry4.</param>
        private void TestSync(SyncResetMode resetMode,
            TemporaryFlagFile ap1Applied, TemporaryFlagFile ap1Unapplied,
            TemporaryFlagFile ap2Applied, TemporaryFlagFile ap2Unapplied,
            TemporaryFlagFile ap3Applied, TemporaryFlagFile ap3Unapplied,
            TemporaryFlagFile ap4Applied, TemporaryFlagFile ap4Unapplied)
        {
            var appEntry1 = new AppEntry
            {
                InterfaceUri = FeedTest.Test1Uri,
                AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = ap1Applied, UnapplyFlagPath = ap1Unapplied}}}
            };
            var appEntry2 = new AppEntry
            {
                InterfaceUri = FeedTest.Test2Uri,
                AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = ap2Applied, UnapplyFlagPath = ap2Unapplied}}}
            };
            var appEntry3 = new AppEntry
            {
                InterfaceUri = FeedTest.Test3Uri,
                AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = ap3Applied, UnapplyFlagPath = ap3Unapplied}}}
            };
            var appEntry4 = new AppEntry
            {
                InterfaceUri = new FeedUri("http://0install.de/feeds/test/test4.xml"),
                AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = ap4Applied, UnapplyFlagPath = ap4Unapplied}}}
            };
            var appListLocal = new AppList {Entries = {appEntry1, appEntry4}};
            var appListLast = new AppList {Entries = {appEntry2, appEntry4}};
            var appListServer = new AppList {Entries = {appEntry2, appEntry3}};

            TestSync(resetMode, appListLocal, appListLast, appListServer);
        }
        #endregion
    }
}
