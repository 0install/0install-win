/*
 * Copyright 2010 Bastian Eicher
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
using NUnit.Framework;

namespace ZeroInstall.MyApps
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
            return new AppList
            {
                Entries = {
                    new AppEntry { Interface = new Uri("http://0install.de/feeds/FileZilla.xml"), Name = "FileZilla", Integrations =
                    {
                        new MenuEntry { Name = "FileZilla", Category = "Network" },
                        new DesktopShortcut { Name = "FileZilla" },
                        new Alias { Name = "filezilla" }
                    } },
                    new AppEntry { Interface = new Uri("http://0install.de/feeds/VLC.xml"), Name = "VLC media player", Integrations =
                    {
                        new MenuEntry { Name = "VLC media player", Category = "AudioVideo" },
                        new DesktopShortcut { Name = "VLC media player" },
                        new Alias { Name = "vlc" }
                    } }
                }
            };
        }
        #endregion

        /// <summary>
        /// Ensures that the class is correctly serialized and deserialized.
        /// </summary>
        [Test]
        public void TestSaveLoad()
        {
            AppList appList1, appList2;
            string tempFile = null;
            try
            {
                tempFile = Path.GetTempFileName();

                // Write and read file
                appList1 = CreateTestAppList();
                appList1.Save(tempFile);
                appList2 = AppList.Load(tempFile);
            }
            finally
            { // Clean up
                if (tempFile != null) File.Delete(tempFile);
            }

            // Ensure data stayed the same
            Assert.AreEqual(appList1, appList2, "Serialized objects should be equal.");
            Assert.AreEqual(appList1.GetHashCode(), appList2.GetHashCode(), "Serialized objects' hashes should be equal.");
            Assert.IsFalse(ReferenceEquals(appList1, appList2), "Serialized objects should not return the same reference.");
        }

        /// <summary>
        /// Ensures that the class can be correctly cloned.
        /// </summary>
        [Test]
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