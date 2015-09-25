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

using System.Collections.Generic;
using FluentAssertions;
using NanoByte.Common.Storage;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Contains test methods for <see cref="AppList"/>.
    /// </summary>
    [TestFixture]
    public sealed class AppListTest
    {
        #region Helpers
        /// <summary>
        /// Creates a fictive test <see cref="AppList"/> without <see cref="AccessPoint"/>s.
        /// </summary>
        private static AppList CreateTestAppListWithoutAPs()
        {
            return new AppList
            {
                Entries =
                {
                    new AppEntry
                    {
                        InterfaceUri = FeedTest.Test1Uri,
                        AutoUpdate = true,
                        CapabilityLists = {Store.Model.Capabilities.CapabilityListTest.CreateTestCapabilityList()}
                    }
                }
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="AppList"/> with <see cref="AccessPoint"/>s.
        /// </summary>
        private static AppList CreateTestAppListWithAPs()
        {
            return new AppList
            {
                Entries =
                {
                    new AppEntry
                    {
                        InterfaceUri = FeedTest.Test1Uri,
                        AutoUpdate = true,
                        CapabilityLists = {Store.Model.Capabilities.CapabilityListTest.CreateTestCapabilityList()},
                        AccessPoints = CreateTestAccessPointList()
                    }
                }
            };
        }

        /// <summary>
        /// Creates a fictive test <see cref="AccessPoints.AccessPointList"/>.
        /// </summary>
        private static AccessPointList CreateTestAccessPointList()
        {
            return new AccessPointList
            {
                Entries =
                {
                    new AppAlias {Command = "main", Name = "myapp"},
                    new AutoStart {Command = "main", Name = "myapp"},
                    new AutoPlay {Capability = "autoplay"},
                    new CapabilityRegistration(),
                    new ContextMenu {Capability = "context"},
                    new DefaultProgram {Capability = "default"},
                    new DesktopIcon {Command = "main", Name = "Desktop icon"},
                    new FileType {Capability = "file_type"},
                    new MenuEntry {Command = "main", Name = "Menu entry", Category = "Developer tools"},
                    new SendTo {Command = "main", Name = "Send to"},
                    new UrlProtocol {Capability = "protocol"},
                    new QuickLaunch {Command = "main", Name = "Quick Launch"}
                }
            };
        }
        #endregion

        [Test(Description = "Ensures that the class is correctly serialized and deserialized without AccessPoints.")]
        public void TestSaveLoadWithoutAPs()
        {
            TestSaveLoad(CreateTestAppListWithoutAPs());
        }

        [Test(Description = "Ensures that the class is correctly serialized and deserialized with AccessPoints.")]
        public void TestSaveLoadWithAPs()
        {
            TestSaveLoad(CreateTestAppListWithAPs());
        }

        private static void TestSaveLoad(AppList appList)
        {
            Assert.That(appList, Is.XmlSerializable);

            AppList appList2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                appList.SaveXml(tempFile);
                appList2 = XmlStorage.LoadXml<AppList>(tempFile);
            }

            // Ensure data stayed the same
            appList2.Should().Be(appList, because: "Serialized objects should be equal.");
            appList2.GetHashCode().Should().Be(appList.GetHashCode(), because: "Serialized objects' hashes should be equal.");
            appList2.Should().NotBeSameAs(appList, because: "Serialized objects should not return the same reference.");
        }

        [Test]
        public void TestContainsEntry()
        {
            var appList = CreateTestAppListWithAPs();
            appList.ContainsEntry(FeedTest.Test1Uri).Should().BeTrue();
            appList.ContainsEntry(FeedTest.Test2Uri).Should().BeFalse();
        }

        [Test]
        public void TestGetEntry()
        {
            var appList = CreateTestAppListWithAPs();

            appList.GetEntry(FeedTest.Test1Uri).Should().Be(appList.Entries[0]);
            appList[FeedTest.Test1Uri].Should().Be(appList.Entries[0]);

            appList.GetEntry(FeedTest.Test2Uri).Should().BeNull();
            // ReSharper disable once UnusedVariable
            appList.Invoking(x => { var _ = x[FeedTest.Test2Uri]; }).ShouldThrow<KeyNotFoundException>();
        }

        [Test]
        public void TestSearch()
        {
            var appA = new AppEntry {InterfaceUri = FeedTest.Test1Uri, Name = "AppA"};
            var appB = new AppEntry {InterfaceUri = FeedTest.Test2Uri, Name = "AppB"};
            var lib = new AppEntry {InterfaceUri = FeedTest.Test3Uri, Name = "Lib"};
            var appList = new AppList {Entries = {appA, appB, lib}};

            appList.Search("").Should().Equal(appA, appB, lib);
            appList.Search("App").Should().Equal(appA, appB);
            appList.Search("AppA").Should().Equal(appA);
            appList.Search("AppB").Should().Equal(appB);
        }

        [Test(Description = "Ensures that the class can be correctly cloned without AccessPoints.")]
        public void TestCloneWithoutAPs()
        {
            TestClone(CreateTestAppListWithoutAPs());
        }

        [Test(Description = "Ensures that the class can be correctly cloned with AccessPoints.")]
        public void TestCloneWithAPs()
        {
            TestClone(CreateTestAppListWithAPs());
        }

        private static void TestClone(AppList appList)
        {
            var appList2 = appList.Clone();

            // Ensure data stayed the same
            appList2.Should().Be(appList, because: "Cloned objects should be equal.");
            appList2.GetHashCode().Should().Be(appList.GetHashCode(), because: "Cloned objects' hashes should be equal.");
            appList2.Should().NotBeSameAs(appList, because: "Cloning should not return the same reference.");
        }
    }
}
