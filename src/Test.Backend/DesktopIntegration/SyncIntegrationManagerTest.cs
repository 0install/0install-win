/*
 * Copyright 2010-2012 Bastian Eicher
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
using NUnit.Framework;
using ZeroInstall.Injector;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains test methods for <see cref="SyncIntegrationManager"/>.
    /// </summary>
    [TestFixture]
    public sealed class SyncIntegrationManagerTest
    {
        private TemporaryFile _appListFile;
        private SyncIntegrationManager _integrationManager;
        private MicroServer _syncServer;

        [SetUp]
        public void SetUp()
        {
            _appListFile = new TemporaryFile("0install-unit-tests");
            new AppList().Save(_appListFile.Path);
            _syncServer = new MicroServer("app-list", new MemoryStream());
            _integrationManager = new SyncIntegrationManager(false, _appListFile.Path, _syncServer.ServerUri, null, null, null, new SilentHandler());
        }

        [TearDown]
        public void TearDown()
        {
            _syncServer.Dispose();
            _integrationManager.Dispose();
            _appListFile.Dispose();
        }

        // ToDo: Add tests
    }
}
