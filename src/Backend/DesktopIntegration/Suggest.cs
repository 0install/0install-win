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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NanoByte.Common;
using NanoByte.Common.Collections;
using ZeroInstall.DesktopIntegration.AccessPoints;
using ZeroInstall.Store.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Suggests suitable default <see cref="AccessPoint"/>s for specific <see cref="Feed"/>s.
    /// </summary>
    public static class Suggest
    {
        /// <summary>
        /// Returns a list of suitable default <see cref="MenuEntry"/>s.
        /// </summary>
        public static IEnumerable<MenuEntry> MenuEntries(Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            var category = feed.Categories.FirstOrDefault();
            if (feed.EntryPoints.Count < 2)
            { // Only a single entry point
                return new[]
                {
                    new MenuEntry
                    {
                        Name = feed.Name.RemoveAll(Path.GetInvalidFileNameChars()),
                        Category = (category == null) ? "" : category.ToString(),
                        Command = Command.NameRun
                    }
                };
            }
            else
            { // Multiple entry points
                return (from entryPoint in feed.EntryPoints
                    where !string.IsNullOrEmpty(entryPoint.Command) && !entryPoint.NeedsTerminal
                    select new MenuEntry
                    {
                        // Try to get a localized name for the command
                        Name = entryPoint.Names.GetBestLanguage(CultureInfo.CurrentUICulture) ?? // If that fails...
                               ((entryPoint.Command == Command.NameRun)
                                   // ... use the application's name
                                   ? feed.Name.RemoveAll(Path.GetInvalidFileNameChars())
                                   // ... or the application's name and the command
                                   : feed.Name.RemoveAll(Path.GetInvalidFileNameChars()) + " " + entryPoint.Command),
                        // Group all entry points in a single folder
                        Category = (category == null) ? feed.Name : category + "/" + feed.Name,
                        Command = entryPoint.Command
                    }).Distinct(x => x.Name);
            }
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="DesktopIcon"/>s.
        /// </summary>
        public static IEnumerable<DesktopIcon> DesktopIcons(Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            return feed.NeedsTerminal
                ? new DesktopIcon[0]
                : new[] {new DesktopIcon {Name = feed.Name.RemoveAll(Path.GetInvalidFileNameChars()), Command = Command.NameRun}};
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="AppAlias"/>s.
        /// </summary>
        public static IEnumerable<AppAlias> Aliases(Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            if (feed.EntryPoints.Count == 0)
            { // Only one entry point
                if (feed.NeedsTerminal)
                {
                    // Try to guess reasonable alias name of command-line applications
                    return new[] {new AppAlias {Name = feed.Name.RemoveAll(Path.GetInvalidFileNameChars()).Replace(' ', '-').ToLower(), Command = Command.NameRun}};
                }
                else return new AppAlias[0];
            }
            else
            { // Multiple entry points
                return (from entryPoint in feed.EntryPoints
                    where !string.IsNullOrEmpty(entryPoint.Command) && (entryPoint.NeedsTerminal || feed.NeedsTerminal)
                    select new AppAlias
                    {
                        Name = entryPoint.BinaryName ?? entryPoint.Command,
                        Command = entryPoint.Command
                    }).Distinct(x => x.Name);
            }
        }
    }
}
