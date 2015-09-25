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
using FluentAssertions;
using NanoByte.Common.Collections;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Services;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;
using FileType = ZeroInstall.Store.Model.Capabilities.FileType;

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
            new AppList().SaveXml(_appListFile);
            _integrationManager = new IntegrationManager(_appListFile, new MockTaskHandler());
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
            var target = new FeedTarget(FeedTest.Test1Uri, new Feed {Name = "Test", CapabilityLists = {capabilityList}});
            _integrationManager.AddApp(target);

            _integrationManager.AppList.Entries
                .Should().Equal(new AppEntry {InterfaceUri = FeedTest.Test1Uri, Name = target.Feed.Name, CapabilityLists = {capabilityList}});

            _integrationManager.Invoking(x => x.AddApp(target))
                .ShouldThrow<InvalidOperationException>(because: "Do not allow adding applications to AppList more than once.");
        }

        [Test]
        public void TestRemoveApp()
        {
            var target = new FeedTarget(FeedTest.Test1Uri, new Feed {Name = "Test"});
            var appEntry = _integrationManager.AddApp(target);

            // Inject access point into AppEntry (without running integration)
            using (var unapplyFlag = new TemporaryFlagFile("0install-unit-tests"))
            {
                appEntry.AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {UnapplyFlagPath = unapplyFlag}}};

                _integrationManager.RemoveApp(appEntry);
                _integrationManager.AppList.Entries.Should().BeEmpty();

                unapplyFlag.Set.Should().BeTrue(because: "Access points should be unapplied when their AppEntry is removed");
                _integrationManager.Invoking(x => x.RemoveApp(appEntry))
                    .ShouldNotThrow(because: "Allow multiple removals of applications.");
            }
        }

        [Test]
        public void TestAddAccessPoints()
        {
            var capabilityList = CapabilityListTest.CreateTestCapabilityList();
            var feed1 = new Feed {Name = "Test", CapabilityLists = {capabilityList}};
            var feed2 = new Feed {Name = "Test", CapabilityLists = {capabilityList}};
            using (var applyFlag1 = new TemporaryFlagFile("0install-unit-tests"))
            using (var applyFlag2 = new TemporaryFlagFile("0install-unit-tests"))
            {
                var accessPoints1 = new AccessPoint[] {new MockAccessPoint {ID = "id1", Capability = "my_ext1", ApplyFlagPath = applyFlag1}};
                var accessPoints2 = new AccessPoint[] {new MockAccessPoint {ID = "id2", Capability = "my_ext2", ApplyFlagPath = applyFlag2}};

                _integrationManager.AppList.Entries.Count.Should().Be(0);
                var appEntry1 = _integrationManager.AddApp(new FeedTarget(FeedTest.Test1Uri, feed1));
                _integrationManager.AddAccessPoints(appEntry1, feed1, accessPoints1);
                _integrationManager.AppList.Entries.Count.Should().Be(1, because: "Should implicitly create missing AppEntries");
                applyFlag1.Set.Should().BeTrue(because: "Should apply AccessPoint");
                applyFlag1.Set = false;

                _integrationManager.Invoking(x => x.AddAccessPoints(appEntry1, feed1, accessPoints1))
                    .ShouldNotThrow(because: "Duplicate access points should be silently reapplied");
                applyFlag1.Set.Should().BeTrue(because: "Duplicate access points should be silently reapplied");

                _integrationManager.AddAccessPoints(appEntry1, feed1, accessPoints2);
                applyFlag2.Set = false;

                var appEntry2 = _integrationManager.AddApp(new FeedTarget(FeedTest.Test2Uri, feed2));
                _integrationManager.Invoking(x => x.AddAccessPoints(appEntry2, feed2, accessPoints2))
                    .ShouldThrow<ConflictException>(because: "Should prevent access point conflicts");
                applyFlag2.Set.Should().BeFalse(because: "Should prevent access point conflicts");
            }
        }

        [Test]
        public void TestRemoveAccessPoints()
        {
            var capabilityList = CapabilityListTest.CreateTestCapabilityList();
            var testApp = new Feed {Name = "Test", CapabilityLists = {capabilityList}};

            using (var unapplyFlag = new TemporaryFlagFile("0install-unit-tests"))
            {
                var accessPoint = new MockAccessPoint {ID = "id1", Capability = "my_ext1", UnapplyFlagPath = unapplyFlag};

                // Inject access point into AppEntry (without running integration)
                var appEntry = _integrationManager.AddApp(new FeedTarget(FeedTest.Test1Uri, testApp));
                appEntry.AccessPoints = new AccessPointList {Entries = {accessPoint}};

                _integrationManager.RemoveAccessPoints(appEntry, new[] {accessPoint});
                _integrationManager.AppList.Entries[0].AccessPoints.Entries.Should().BeEmpty();

                unapplyFlag.Set.Should().BeTrue(because: "Unapply() should be called");

                _integrationManager.Invoking(x => x.RemoveAccessPoints(appEntry, new[] {accessPoint}))
                    .ShouldNotThrow(because: "Allow multiple removals of access points.");
            }
        }

        [Test]
        public void TestUpdateApp()
        {
            var capabilitiyList = new CapabilityList
            {
                OS = Architecture.CurrentSystem.OS,
                Entries = {new FileType {ID = "my_ext1"}, new FileType {ID = "my_ext2"}}
            };
            var feed = new Feed {Name = "Test 1", CapabilityLists = {capabilitiyList}};
            var accessPoints = new AccessPoint[] {new MockAccessPoint {ID = "id1", Capability = "my_ext1"}};

            var appEntry = _integrationManager.AddApp(new FeedTarget(FeedTest.Test1Uri, feed));
            _integrationManager.AddAccessPoints(appEntry, feed, accessPoints);
            _integrationManager.AppList.Entries[0].AccessPoints.Entries
                .Should().Equal(accessPoints, because: "All access points should be applied.");

            // Modify feed
            feed.Name = "Test 2";
            capabilitiyList.Entries.RemoveLast();

            _integrationManager.UpdateApp(appEntry, feed);
            appEntry.Name.Should().Be("Test 2");
            _integrationManager.AppList.Entries[0].AccessPoints.Entries
                .Should().Equal(new[] {accessPoints[0]}, because: "Only the first access point should be left.");
        }

        [Test]
        public void TestRepair()
        {
            var target = new FeedTarget(FeedTest.Test1Uri, new Feed {Name = "Test"});
            var appEntry = _integrationManager.AddApp(target);

            using (var applyFlag = new TemporaryFlagFile("0install-unit-tests"))
            {
                // Inject access point into AppEntry (without running integration)
                appEntry.AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ApplyFlagPath = applyFlag}}};
                _integrationManager.Repair(uri => new Feed());

                applyFlag.Set.Should().BeTrue(because: "Access points should be reapplied");
            }
        }
    }
}
