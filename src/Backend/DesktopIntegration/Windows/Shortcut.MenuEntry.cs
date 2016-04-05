/*
 * Copyright 2010-2016 Bastian Eicher
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
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.DesktopIntegration.Properties;
using ZeroInstall.Store;

namespace ZeroInstall.DesktopIntegration.Windows
{
    public static partial class Shortcut
    {
        /// <summary>
        /// Creates a new Windows shortcut in the start menu or on the start page.
        /// </summary>
        /// <param name="menuEntry">Information about the shortcut to be created.</param>
        /// <param name="target">The target the shortcut shall point to.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <param name="machineWide">Create the shortcut machine-wide instead of just for the current user.</param>
        public static void Create([NotNull] MenuEntry menuEntry, FeedTarget target, [NotNull] ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (menuEntry == null) throw new ArgumentNullException("menuEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string dirPath = GetStartMenuCategoryPath(menuEntry.Category, machineWide);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            string filePath = GetStartMenuPath(menuEntry.Category, menuEntry.Name, machineWide);
            Create(filePath, target, menuEntry.Command, handler, machineWide);
        }

        /// <summary>
        /// Removes a Windows shortcut from the start menu or on the start page.
        /// </summary>
        /// <param name="menuEntry">Information about the shortcut to be removed.</param>
        /// <param name="machineWide">The shortcut was created machine-wide instead of just for the current user.</param>
        public static void Remove([NotNull] MenuEntry menuEntry, bool machineWide)
        {
            #region Sanity checks
            if (menuEntry == null) throw new ArgumentNullException("menuEntry");
            #endregion

            string filePath = GetStartMenuPath(menuEntry.Category, menuEntry.Name, machineWide);
            if (File.Exists(filePath)) File.Delete(filePath);

            // Delete category directory if empty
            string dirPath = GetStartMenuCategoryPath(menuEntry.Category, machineWide);
            if (Directory.Exists(dirPath) && Directory.GetFileSystemEntries(dirPath).Length == 0)
                Directory.Delete(dirPath, recursive: false);
        }

        /// <summary>
        /// Builds a path for a shortcut in the start menu programs folder, optionally appending a category.
        /// </summary>
        /// <param name="category">The name of the category/directory below the programs folder; can be <c>null</c>.</param>
        /// <param name="name">The name of the shortcut (without the .lnk ending).</param>
        /// <param name="machineWide"><c>true</c> to use the machine-wide start menu; <c>false</c> for the per-user variant.</param>
        /// <exception cref="IOException"><paramref name="name"/> or <paramref name="category"/> contains invalid characters.</exception>
        [NotNull]
        public static string GetStartMenuPath([CanBeNull] string category, [NotNull] string name, bool machineWide)
        {
            CheckName(name);

            return Path.Combine(GetStartMenuCategoryPath(category, machineWide), name + ".lnk");
        }

        /// <summary>
        /// Returns the start menu programs folder path, optionally appending a category.
        /// </summary>
        /// <param name="category">The name of the category/directory below the programs folder; can be <c>null</c>.</param>
        /// <param name="machineWide"><c>true</c> to use the machine-wide start menu; <c>false</c> for the per-user variant.</param>
        /// <exception cref="IOException"><paramref name="category"/> contains invalid characters.</exception>
        [NotNull]
        private static string GetStartMenuCategoryPath([CanBeNull] string category, bool machineWide)
        {
            string menuDir = machineWide
                ? RegistryUtils.GetString(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "Common Programs")
                : Environment.GetFolderPath(Environment.SpecialFolder.Programs);

            if (string.IsNullOrEmpty(category)) return menuDir;
            else
            {
                string categoryDir = FileUtils.UnifySlashes(category);
                if (categoryDir.IndexOfAny(Path.GetInvalidPathChars()) != -1 || FileUtils.IsBreakoutPath(categoryDir))
                    throw new IOException(string.Format(Resources.NameInvalidChars, category));

                return Path.Combine(menuDir, categoryDir);
            }
        }
    }
}
