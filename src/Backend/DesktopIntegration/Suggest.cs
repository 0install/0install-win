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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
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
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            #endregion

            var category = feed.Categories.FirstOrDefault();
            string categoryString = (category == null)
                ? (feed.EntryPoints.Count > 1 ? feed.Name : "")
                : (feed.EntryPoints.Count > 1 ? category + "/" + feed.Name : category.ToString());

            return (from entryPoint in feed.EntryPoints
                select new MenuEntry
                {
                    Category = categoryString,
                    Name = feed.GetBestName(CultureInfo.CurrentUICulture, entryPoint.Command),
                    Command = entryPoint.Command
                }).DistinctBy(x => x.Name);
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="DesktopIcon"/>s.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<DesktopIcon> DesktopIcons([NotNull] Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            #endregion

            return (from entryPoint in feed.EntryPoints
                where entryPoint.Command == Command.NameRun || entryPoint.Command == Command.NameRunGui
                select new DesktopIcon
                {
                    Name = feed.GetBestName(CultureInfo.CurrentUICulture, entryPoint.Command),
                    Command = entryPoint.Command
                }).DistinctBy(x => x.Name);
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="SendTo"/>s.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<SendTo> SendTo([NotNull] Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            #endregion

            return (from entryPoint in feed.EntryPoints
                where entryPoint.SuggestSendTo
                select new SendTo
                {
                    Name = feed.GetBestName(CultureInfo.CurrentUICulture, entryPoint.Command),
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
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            #endregion

            return (from entryPoint in feed.EntryPoints
                where entryPoint.NeedsTerminal
                select new AppAlias
                {
                    Name = entryPoint.BinaryName ?? (entryPoint.Command == Command.NameRun ? feed.Name.Replace(' ', '-').ToLower() : entryPoint.Command),
                    Command = entryPoint.Command
                }).DistinctBy(x => x.Name);
        }

        /// <summary>
        /// Returns a list of suitable default <see cref="AutoStart"/>s.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<AutoStart> AutoStart([NotNull] Feed feed)
        {
            #region Sanity checks
            if (feed == null) throw new ArgumentNullException(nameof(feed));
            #endregion

            return (from entryPoint in feed.EntryPoints
                where entryPoint.SuggestAutoStart
                select new AutoStart
                {
                    Name = feed.GetBestName(CultureInfo.CurrentUICulture, entryPoint.Command),
                    Command = entryPoint.Command
                }).DistinctBy(x => x.Name);
        }
    }
}
