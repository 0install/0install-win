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

using Common.Storage;
using NUnit.Framework;
using ZeroInstall.DesktopIntegration.AccessPoints;
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
        /// Creates a fictive test <see cref="AppList"/>.
        /// </summary>
        private static AppList CreateTestAppList()
        {
            return new AppList {Entries =
            {
                new AppEntry
                {
                    AutoUpdate = true,
                    CapabilityLists = {Capabilities.CapabilityListTest.CreateTestCapabilityList()},
                    AccessPoints = CreateTestAccessPointList()
                }
            } };
        }

        /// <summary>
        /// Creates a fictive test <see cref="AccessPoints.AccessPointList"/>.
        /// </summary>
        private static AccessPointList CreateTestAccessPointList()
        {
            return new AccessPointList {Entries =
            {
                new AppAlias {Command = "main", Name = "myapp"},
                new AutoPlay {Capability = "autoplay"},
                new CapabilityRegistration(),
                new ContextMenu {Capability = "context"},
                new DefaultProgram {Capability = "default"},
                new DesktopIcon {Command = "main", Name = "Desktop icon"},
                new FileType {Capability = "file_type"},
                new MenuEntry {Command = "main", Name = "Menu entry", Category = "Developer tools"},
                new UrlProtocol {Capability = "protocol"},
                new QuickLaunch {Command = "main", Name = "Quick Launch"}
            }};
        }
        #endregion

        [Test(Description = "Ensures that the class is correctly serialized and deserialized.")]
        public void TestSaveLoad()
        {
            AppList appList1, appList2;
            using (var tempFile = new TemporaryFile("0install-unit-tests"))
            {
                // Write and read file
                appList1 = CreateTestAppList();
                appList1.Save(tempFile.Path);
                appList2 = AppList.Load(tempFile.Path);
            }

            // Ensure data stayed the same
            Assert.AreEqual(appList1, appList2, "Serialized objects should be equal.");
            Assert.AreEqual(appList1.GetHashCode(), appList2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(appList1, appList2), "Serialized objects should not return the same reference.");
        }

        [Test(Description = "Ensures that the class can be correctly cloned.")]
        public void TestClone()
        {
            var archive1 = CreateTestAppList();
            var archive2 = archive1.CloneAppList();

            // Ensure data stayed the same
            Assert.AreEqual(archive1, archive2, "Cloned objects should be equal.");
            Assert.AreEqual(archive1.GetHashCode(), archive2.GetHashCode(), "Cloned objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(archive1, archive2), "Cloning should not return the same reference.");
        }
    }
}