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
        public void TestNormal()
        {
            var appList = new AppList();
            appList.SaveXml(_appListPath);

            var appListLast = new AppList();
            appListLast.SaveXml(_appListPath + SyncIntegrationManager.AppListLastSyncSuffix);

            using (var syncServer = new MicroServer("app-list", new MemoryStream()))
            using (var integrationManager = new SyncIntegrationManager(false, _appListPath, syncServer.ServerUri, null, null, null, new SilentTaskHandler()))
                integrationManager.Sync(SyncResetMode.None, interfaceId => null);
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
