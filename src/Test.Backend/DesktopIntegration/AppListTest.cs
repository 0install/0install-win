﻿/*
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

using System.Collections.Generic;
using Common.Storage;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;

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
                        InterfaceID = "pet-name",
                        AutoUpdate = true,
                        Requirements = new Requirements {InterfaceID = "http://0install.de/feeds/test/test1.xml"},
                        CapabilityLists = {Capabilities.CapabilityListTest.CreateTestCapabilityList()}
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
                        InterfaceID = "http://0install.de/feeds/test/test1.xml",
                        AutoUpdate = true,
                        CapabilityLists = {Capabilities.CapabilityListTest.CreateTestCapabilityList()},
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
                appList.SaveXml(tempFile.Path);
                appList2 = XmlStorage.LoadXml<AppList>(tempFile.Path);
            }

            // Ensure data stayed the same
            Assert.AreEqual(appList, appList2, "Serialized objects should be equal.");
            Assert.AreEqual(appList.GetHashCode(), appList2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(appList, appList2), "Serialized objects should not return the same reference.");
        }

        [Test]
        public void TestContainsEntry()
        {
            var appList = CreateTestAppListWithAPs();
            Assert.IsTrue(appList.Contains("http://0install.de/feeds/test/test1.xml"));
            Assert.IsFalse(appList.Contains("http://0install.de/feeds/test/test2.xml"));
        }

        [Test]
        public void TestGetEntry()
        {
            var appList = CreateTestAppListWithAPs();
            Assert.AreEqual(appList.Entries.First, appList["http://0install.de/feeds/test/test1.xml"]);
            // ReSharper disable UnusedVariable
            Assert.Throws<KeyNotFoundException>(() => { var dummy = appList["http://0install.de/feeds/test/test2.xml"]; });
            // ReSharper restore UnusedVariable
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
            Assert.AreEqual(appList, appList2, "Cloned objects should be equal.");
            Assert.AreEqual(appList.GetHashCode(), appList2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(appList, appList2), "Cloning should not return the same reference.");
        }
    }
}
