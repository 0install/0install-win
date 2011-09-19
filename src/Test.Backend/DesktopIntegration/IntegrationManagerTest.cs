/*
 * Copyright 2010-2011 Bastian Eicher
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
using Common.Storage;
using Common.Tasks;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains test methods for <see cref="IntegrationManager"/>.
    /// </summary>
    [TestFixture]
    public sealed class IntegrationManagerTest
    {
        private TemporaryFile _appListFile;
        private IntegrationManager _integrationManager;

        [SetUp]
        public void SetUp()
        {
            _appListFile = new TemporaryFile("0install-unit-tests");
            new AppList().Save(_appListFile.Path);
            _integrationManager = new IntegrationManager(false, _appListFile.Path);
        }

        [TearDown]
        public void TearDown()
        {
            _integrationManager.Dispose();
            _appListFile.Dispose();
        }

        [Test]
        public void TestAddApp()
        {
            var capabilityList = CapabilityListTest.CreateTestCapabilityList();
            var testApp = new InterfaceFeed("http://0install.de/feeds/test/test1.xml", new Feed {Name = "Test", CapabilityLists = {capabilityList}});
            _integrationManager.AddApp(testApp);

            var expectedAppEntry = new AppEntry {InterfaceID = testApp.InterfaceID, Name = testApp.Feed.Name, CapabilityLists = {capabilityList}};
            CollectionAssert.AreEqual(new[] {expectedAppEntry}, _integrationManager.AppList.Entries);

            Assert.Throws<InvalidOperationException>(() => _integrationManager.AddApp(testApp), "Do not allow adding applications to AppList more than once.");
        }

        [Test]
        public void TestAddAccessPoints()
        {
            var capabilityList = CapabilityListTest.CreateTestCapabilityList();
            var testApp1 = new InterfaceFeed("http://0install.de/feeds/test/test1.xml", new Feed {Name = "Test", CapabilityLists = {capabilityList}});
            var testApp2 = new InterfaceFeed("http://0install.de/feeds/test/test2.xml", new Feed {Name = "Test", CapabilityLists = {capabilityList}});

            Assert.AreEqual(0, _integrationManager.AppList.Entries.Count);
            _integrationManager.AddAccessPoints(testApp1, new AccessPoint[] {new MockAccessPoint {ID = "id1"}}, new SilentTaskHandler());
            Assert.AreEqual(1, _integrationManager.AppList.Entries.Count, "Should implicitly create missing AppEntries.");

            Assert.DoesNotThrow(() => _integrationManager.AddAccessPoints(testApp1, new AccessPoint[] {new MockAccessPoint {ID = "id1"}}, new SilentTaskHandler()), "Duplicate access points should be silently reapplied.");
            _integrationManager.AddAccessPoints(testApp1, new AccessPoint[] {new MockAccessPoint {ID = "id2"}}, new SilentTaskHandler());

            Assert.Throws<InvalidOperationException>(() => _integrationManager.AddAccessPoints(testApp2, new AccessPoint[] {new MockAccessPoint {ID = "id2"}}, new SilentTaskHandler()), "Should prevent access point conflicts.");
        }

        [Test]
        public void TestRemoveApp()
        {
            var testApp = new InterfaceFeed("http://0install.de/feeds/test/test1.xml", new Feed {Name = "Test"});
            _integrationManager.AddApp(testApp);

            _integrationManager.RemoveApp(testApp.InterfaceID);
            CollectionAssert.IsEmpty(_integrationManager.AppList.Entries);

            Assert.Throws<InvalidOperationException>(() => _integrationManager.RemoveApp(testApp.InterfaceID), "Do not allow removing entries that are not in the AppList.");
        }
    }
}