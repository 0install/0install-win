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

using System;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Injector;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;
using FileType = ZeroInstall.Model.Capabilities.FileType;

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
            _integrationManager = new IntegrationManager(false, _appListFile.Path, new SilentHandler());
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
            var feed = new Feed {Name = "Test", CapabilityLists = {capabilityList}};
            _integrationManager.AddApp("http://0install.de/feeds/test/test1.xml", feed);

            var expectedAppEntry = new AppEntry {InterfaceID = "http://0install.de/feeds/test/test1.xml", Name = feed.Name, CapabilityLists = {capabilityList}};
            CollectionAssert.AreEqual(new[] {expectedAppEntry}, _integrationManager.AppList.Entries);

            Assert.Throws<InvalidOperationException>(() => _integrationManager.AddApp("http://0install.de/feeds/test/test1.xml", feed), "Do not allow adding applications to AppList more than once.");
        }

        [Test]
        public void TestRemoveApp()
        {
            var appEntry = _integrationManager.AddApp("http://0install.de/feeds/test/test1.xml", new Feed {Name = "Test"});

            _integrationManager.RemoveApp(appEntry);
            CollectionAssert.IsEmpty(_integrationManager.AppList.Entries);

            Assert.DoesNotThrow(() => _integrationManager.RemoveApp(appEntry), "Allow multiple removals of applications.");
        }

        [Test]
        public void TestAddAccessPoints()
        {
            var capabilityList = CapabilityListTest.CreateTestCapabilityList();
            var feed1 = new Feed {Name = "Test", CapabilityLists = {capabilityList}};
            var feed2 = new Feed {Name = "Test", CapabilityLists = {capabilityList}};
            var accessPoints1 = new AccessPoint[] {new MockAccessPoint {ID = "id1", Capability = "my_ext1"}};
            var accessPoints2 = new AccessPoint[] {new MockAccessPoint {ID = "id2", Capability = "my_ext2"}};

            Assert.AreEqual(0, _integrationManager.AppList.Entries.Count);
            var appEntry1 = _integrationManager.AddApp("http://0install.de/feeds/test/test1.xml", feed1);
            _integrationManager.AddAccessPoints(appEntry1, feed1, accessPoints1);
            Assert.AreEqual(1, _integrationManager.AppList.Entries.Count, "Should implicitly create missing AppEntries.");

            Assert.DoesNotThrow(() => _integrationManager.AddAccessPoints(appEntry1, feed1, accessPoints1), "Duplicate access points should be silently reapplied.");
            _integrationManager.AddAccessPoints(appEntry1, feed1, accessPoints2);

            var appEntry2 = _integrationManager.AddApp("http://0install.de/feeds/test/test2.xml", feed2);
            Assert.Throws<InvalidOperationException>(() => _integrationManager.AddAccessPoints(appEntry2, feed2, accessPoints2), "Should prevent access point conflicts.");
        }

        [Test]
        public void TestRemoveAccessPoints()
        {
            var capabilityList = CapabilityListTest.CreateTestCapabilityList();
            var testApp = new Feed {Name = "Test", CapabilityLists = {capabilityList}};

            var accessPoints = new AccessPoint[] {new MockAccessPoint {ID = "id1", Capability = "my_ext1"}};

            var appEntry = _integrationManager.AddApp("http://0install.de/feeds/test/test1.xml", testApp);
            _integrationManager.AddAccessPoints(appEntry, testApp, accessPoints);

            _integrationManager.RemoveAccessPoints(appEntry, accessPoints);
            CollectionAssert.IsEmpty(_integrationManager.AppList.Entries.First.AccessPoints.Entries);

            Assert.DoesNotThrow(() => _integrationManager.RemoveAccessPoints(appEntry, accessPoints), "Allow multiple removals of access points.");
        }

        [Test]
        public void TestUpdateApp()
        {
            var capabilitiyList = new CapabilityList
            {
                Architecture = Architecture.CurrentSystem,
                Entries = {new FileType {ID = "my_ext1"}, new FileType {ID = "my_ext2"}}
            };
            var feed = new Feed {Name = "Test 1", CapabilityLists = {capabilitiyList}};
            var accessPoints = new AccessPoint[] {new MockAccessPoint {ID = "id1", Capability = "my_ext1"}};

            var appEntry = _integrationManager.AddApp("http://0install.de/feeds/test/test1.xml", feed);
            _integrationManager.AddAccessPoints(appEntry, feed, accessPoints);
            CollectionAssert.AreEquivalent(accessPoints, _integrationManager.AppList.Entries.First.AccessPoints.Entries, "All access points should be applied.");

            // Modify feed
            feed.Name = "Test 2";
            capabilitiyList.Entries.RemoveLast();

            _integrationManager.UpdateApp(appEntry, feed);
            Assert.AreEqual("Test 2", appEntry.Name);
            CollectionAssert.AreEquivalent(new[] {accessPoints[0]}, _integrationManager.AppList.Entries.First.AccessPoints.Entries, "Only the first access point should be left.");
        }
    }
}
