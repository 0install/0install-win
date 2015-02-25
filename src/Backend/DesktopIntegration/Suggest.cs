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
using JetBrains.Annotations;
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
        [NotNull, ItemNotNull]
        public static IEnumerable<MenuEntry> MenuEntries([NotNull] Feed feed)
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
                        Name = GetName(feed),
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
                        Category = (category == null) ? feed.Name : category + "/" + feed.Name,
                        Name = GetName(feed, entryPoint),
                        Command = entryPoint.Command
                    }).DistinctBy(x => x.Name);
            }
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="DesktopIcon"/>s.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<DesktopIcon> DesktopIcons([NotNull] Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            return feed.NeedsTerminal
                ? new DesktopIcon[0]
                : new[] {new DesktopIcon {Name = GetName(feed), Command = Command.NameRun}};
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="SendTo"/>s.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<SendTo> SendTo([NotNull] Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            return (from entryPoint in feed.EntryPoints
                where !string.IsNullOrEmpty(entryPoint.Command) && entryPoint.SuggestSendTo
                select new SendTo
                {
                    Name = GetName(feed, entryPoint),
                    Command = entryPoint.Command
                }).DistinctBy(x => x.Name);
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="AppAlias"/>s.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<AppAlias> Aliases([NotNull] Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            if (feed.EntryPoints.Count == 0)
            { // Only one entry point
                if (feed.NeedsTerminal)
                {
                    // Try to guess reasonable alias name of command-line applications
                    return new[] {new AppAlias {Name = GetName(feed).Replace(' ', '-').ToLower(), Command = Command.NameRun}};
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
                    }).DistinctBy(x => x.Name);
            }
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="AutoStart"/>s.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<AutoStart> AutoStart([NotNull] Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException("feed");
            #endregion

            return (from entryPoint in feed.EntryPoints
                where !string.IsNullOrEmpty(entryPoint.Command) && entryPoint.SuggestAutoStart
                select new AutoStart
                {
                    Name = GetName(feed, entryPoint),
                    Command = entryPoint.Command
                }).DistinctBy(x => x.Name);
        }

        private static string GetName(Feed feed)
        {
            return (feed.Name ?? "").RemoveAll(Path.GetInvalidFileNameChars());
        }

        private static string GetName(Feed feed, EntryPoint entryPoint)
        {
            // Try to get a localized name for the command
            return entryPoint.Names.GetBestLanguage(CultureInfo.CurrentUICulture)
                // If that fails...
                   ?? ((entryPoint.Command == Command.NameRun)
                       // ... use the application's name
                       ? GetName(feed)
                       // ... or the application's name and the command
                       : GetName(feed) + " " + entryPoint.Command);
        }
    }
}
