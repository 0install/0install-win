﻿/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Xml.Serialization;
using Common.Collections;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using Capabilities = ZeroInstall.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Indicates that all compatible capabilities should be registered.
    /// </summary>
    /// <seealso cref="ZeroInstall.Model.Capabilities"/>
    [XmlType("capability-registration", Namespace = AppList.XmlNamespace)]
    public class CapabilityRegistration : AccessPoint, IEquatable<CapabilityRegistration>
    {
        #region Constants
        /// <summary>
        /// The name of this category of <see cref="AccessPoint"/>s as used by command-line interfaces.
        /// </summary>
        public const string CategoryName = "capabilities";
        #endregion

        //--------------------//

        #region Conflict ID
        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            var idList = new LinkedList<string>();
            foreach (var capabilityList in appEntry.CapabilityLists)
            {
                if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;
                foreach (var capability in capabilityList.Entries)
                {
                    foreach (var conflictID in capability.ConflictIDs)
                        idList.AddLast("capability:" + conflictID);
                }
            }
            return idList;
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Register all applicable capabilities
            var target = new InterfaceFeed(appEntry.InterfaceID, feed);
            foreach (var capabilityList in appEntry.CapabilityLists.Filter(AreCapabilitiesApplicable))
            {
                var dispatcher = new PerTypeDispatcher<Capabilities.Capability>(true);
                if (WindowsUtils.IsWindows)
                {
                    dispatcher.Add((Capabilities.FileType fileType) => Windows.FileType.Register(target, fileType, false, systemWide, handler));
                    dispatcher.Add((Capabilities.UrlProtocol urlProtocol) => Windows.UrlProtocol.Register(target, urlProtocol, false, systemWide, handler));
                    dispatcher.Add((Capabilities.AutoPlay autoPlay) => Windows.AutoPlay.Register(target, autoPlay, false, systemWide, handler));
                    dispatcher.Add((Capabilities.ComServer comServer) => Windows.ComServer.Register(target, comServer, systemWide, handler));
                    dispatcher.Add((Capabilities.GamesExplorer gamesExplorer) => Windows.GamesExplorer.Register(target, gamesExplorer, systemWide, handler));
                    if (systemWide || WindowsUtils.IsWindows8)
                        dispatcher.Add((Capabilities.AppRegistration appRegistration) => Windows.AppRegistration.Register(target, appRegistration, EnumerableUtils.OfType<Capabilities.VerbCapability>(capabilityList.Entries), systemWide, handler));
                    if (systemWide)
                        dispatcher.Add((Capabilities.DefaultProgram defaultProgram) => Windows.DefaultProgram.Register(target, defaultProgram, false, handler));
                }
                dispatcher.Dispatch(capabilityList.Entries);
            }
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool systemWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            if (!WindowsUtils.IsWindows) return;

            // Unregister all applicable capabilities
            foreach (var capabilityList in appEntry.CapabilityLists.Filter(AreCapabilitiesApplicable))
            {
                var dispatcher = new PerTypeDispatcher<Capabilities.Capability>(true);
                if (WindowsUtils.IsWindows)
                {
                    dispatcher.Add((Capabilities.FileType fileType) => Windows.FileType.Unregister(fileType, false, systemWide));
                    dispatcher.Add((Capabilities.UrlProtocol urlProtocol) => Windows.UrlProtocol.Unregister(urlProtocol, false, systemWide));
                    dispatcher.Add((Capabilities.AutoPlay autoPlay) => Windows.AutoPlay.Unregister(autoPlay, false, systemWide));
                    dispatcher.Add((Capabilities.ComServer comServer) => Windows.ComServer.Unregister(comServer, systemWide));
                    dispatcher.Add((Capabilities.GamesExplorer gamesExplorer) => Windows.GamesExplorer.Unregister(gamesExplorer, systemWide));
                    if (systemWide || WindowsUtils.IsWindows8)
                        dispatcher.Add((Capabilities.AppRegistration appRegistration) => Windows.AppRegistration.Unregister(appRegistration, systemWide));
                    if (systemWide)
                        dispatcher.Add((Capabilities.DefaultProgram defaultProgram) => Windows.DefaultProgram.Unregister(defaultProgram, false));
                }
                dispatcher.Dispatch(capabilityList.Entries);
            }
        }

        private bool AreCapabilitiesApplicable(Capabilities.CapabilityList capabilityList)
        {
            return capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem);
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "CapabilityRegistration". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return string.Format("CapabilityRegistration");
        }
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone()
        {
            return new CapabilityRegistration();
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(CapabilityRegistration other)
        {
            if (other == null) return false;

            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(CapabilityRegistration) && Equals((CapabilityRegistration)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return 0;
        }
        #endregion
    }
}
