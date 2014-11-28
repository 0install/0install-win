/*
 * Copyright 2010-2014 Bastian Eicher
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

using System.Collections.Generic;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains test methods for <see cref="ConflictDataUtils"/>.
    /// </summary>
    [TestFixture]
    public sealed class ConflictDataUtilsTest
    {
        [Test]
        public void NoConflicts()
        {
            var accessPointA = new MockAccessPoint {ID = "a"};
            var appEntry1 = new AppEntry
            {
                Name = "App1", InterfaceUri = FeedTest.Test1Uri,
                AccessPoints = new AccessPointList {Entries = {accessPointA}}
            };
            var accessPointB = new MockAccessPoint {ID = "b"};
            var appEntry2 = new AppEntry {Name = "App2", InterfaceUri = FeedTest.Test2Uri};

            var appList = new AppList {Entries = {appEntry1}};
            appList.CheckForConflicts(new[] {accessPointB}, appEntry2);
        }

        [Test]
        public void ReApply()
        {
            var accessPointA = new MockAccessPoint {ID = "a"};
            var appEntry1 = new AppEntry
            {
                Name = "App1", InterfaceUri = FeedTest.Test1Uri,
                AccessPoints = new AccessPointList {Entries = {accessPointA}}
            };

            var appList = new AppList {Entries = {appEntry1}};
            appList.CheckForConflicts(new[] {accessPointA}, appEntry1);
        }

        [Test]
        public void Conflict()
        {
            var accessPointA = new MockAccessPoint {ID = "a"};
            var appEntry1 = new AppEntry
            {
                Name = "App1",
                InterfaceUri = FeedTest.Test1Uri,
                AccessPoints = new AccessPointList {Entries = {accessPointA}}
            };
            var appEntry2 = new AppEntry {Name = "App2", InterfaceUri = FeedTest.Test2Uri};

            var appList = new AppList {Entries = {appEntry1}};
            Assert.Throws<ConflictException>(() => appList.CheckForConflicts(new[] {accessPointA}, appEntry2));
        }

        [Test]
        public void AccessPointCandidates()
        {
            var accessPoints = new AccessPoint[] {new MockAccessPoint {ID = "a"}, new MockAccessPoint {ID = "b"}};
            var appEntry = new AppEntry {Name = "App"};

            CollectionAssert.AreEqual(
                expected: new[]
                {
                    new KeyValuePair<string, ConflictData>("mock:a", new ConflictData(accessPoints[0], appEntry)),
                    new KeyValuePair<string, ConflictData>("mock:b", new ConflictData(accessPoints[1], appEntry))
                },
                actual: accessPoints.GetConflictData(appEntry));
        }

        [Test]
        public void AccessPointCandidatesInternalConflict()
        {
            var accessPoints = new AccessPoint[] {new MockAccessPoint {ID = "a"}, new MockAccessPoint {ID = "a"}};
            var appEntry = new AppEntry {Name = "App"};

            Assert.Throws<ConflictException>(() => accessPoints.GetConflictData(appEntry));
        }

        [Test]
        public void ExistingAppEntries()
        {
            var appList = new[]
            {
                new AppEntry {Name = "App1", AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ID = "a"}}}},
                new AppEntry {Name = "App2", AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ID = "b"}}}}
            };

            CollectionAssert.AreEqual(
                expected: new[]
                {
                    new KeyValuePair<string, ConflictData>("mock:a", new ConflictData(appList[0].AccessPoints.Entries[0], appList[0])),
                    new KeyValuePair<string, ConflictData>("mock:b", new ConflictData(appList[1].AccessPoints.Entries[0], appList[1]))
                },
                actual: appList.GetConflictData());
        }

        [Test]
        public void ExistingAppEntriesInternalConflict()
        {
            var appList = new[]
            {
                new AppEntry {Name = "App1", AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ID = "a"}}}},
                new AppEntry {Name = "App2", AccessPoints = new AccessPointList {Entries = {new MockAccessPoint {ID = "a"}}}}
            };

            Assert.Throws<ConflictException>(() => appList.GetConflictData());
        }
    }
}
