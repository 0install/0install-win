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

            if (!WindowsUtils.IsWindows) return;

            // Register all applicable capabilities
            var target = new InterfaceFeed(appEntry.InterfaceID, feed);
            foreach (var capabilityList in appEntry.CapabilityLists)
            {
                if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;

                // Note: Enumerating only once and using nested if-clauses to determine types would be more efficient but less maintainable
                foreach (var fileType in EnumerableUtils.OfType<Capabilities.FileType>(capabilityList.Entries))
                    Windows.FileType.Register(target, fileType, false, systemWide, handler);
                foreach (var urlProtocol in EnumerableUtils.OfType<Capabilities.UrlProtocol>(capabilityList.Entries))
                    Windows.UrlProtocol.Register(target, urlProtocol, false, systemWide, handler);
                foreach (var autoPlay in EnumerableUtils.OfType<Capabilities.AutoPlay>(capabilityList.Entries))
                    Windows.AutoPlay.Register(target, autoPlay, false, systemWide, handler);
                foreach (var comServer in EnumerableUtils.OfType<Capabilities.ComServer>(capabilityList.Entries))
                    Windows.ComServer.Register(target, comServer, systemWide, handler);
                foreach (var gamesExplorer in EnumerableUtils.OfType<Capabilities.GamesExplorer>(capabilityList.Entries))
                    Windows.GamesExplorer.Register(target, gamesExplorer, systemWide, handler);
                if (systemWide)
                {
                    foreach (var defaultProgram in EnumerableUtils.OfType<Capabilities.DefaultProgram>(capabilityList.Entries))
                        Windows.DefaultProgram.Register(target, defaultProgram, false, handler);
                    foreach (var appRegistration in EnumerableUtils.OfType<Capabilities.AppRegistration>(capabilityList.Entries))
                        Windows.AppRegistration.Apply(target, appRegistration, EnumerableUtils.OfType<Capabilities.VerbCapability>(capabilityList.Entries), handler);
                }
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
            foreach (var capabilityList in appEntry.CapabilityLists)
            {
                if (!capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)) continue;

                // Note: Enumerating only once and using nested if-clauses to determine types would be more efficient but less maintainable
                foreach (var fileType in EnumerableUtils.OfType<Capabilities.FileType>(capabilityList.Entries))
                    Windows.FileType.Unregister(fileType, false, systemWide);
                foreach (var urlProtocol in EnumerableUtils.OfType<Capabilities.UrlProtocol>(capabilityList.Entries))
                    Windows.UrlProtocol.Unregister(urlProtocol, false, systemWide);
                foreach (var autoPlay in EnumerableUtils.OfType<Capabilities.AutoPlay>(capabilityList.Entries))
                    Windows.AutoPlay.Unregister(autoPlay, false, systemWide);
                foreach (var comServer in EnumerableUtils.OfType<Capabilities.ComServer>(capabilityList.Entries))
                    Windows.ComServer.Unregister(comServer, systemWide);
                foreach (var gamesExplorer in EnumerableUtils.OfType<Capabilities.GamesExplorer>(capabilityList.Entries))
                    Windows.GamesExplorer.Unregister(gamesExplorer, systemWide);
                if (systemWide)
                {
                    foreach (var defaultProgram in EnumerableUtils.OfType<Capabilities.DefaultProgram>(capabilityList.Entries))
                        Windows.DefaultProgram.Unregister(defaultProgram, false);
                    foreach (var appRegistration in EnumerableUtils.OfType<Capabilities.AppRegistration>(capabilityList.Entries))
                        Windows.AppRegistration.Remove(appRegistration);
                }
            }
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
