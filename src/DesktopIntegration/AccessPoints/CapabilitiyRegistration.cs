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
using System.Linq;
using System.Xml.Serialization;
using Common.Collections;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;

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

            return appEntry.CapabilityLists.
                Where(capabilityList => capabilityList.Architecture.IsCompatible(Architecture.CurrentSystem)).
                SelectMany(capabilityList => capabilityList.Entries.SelectMany(capability => capability.ConflictIDs)).
                Select(conflictID => "capability:" + conflictID);
        }
        #endregion

        #region Apply
        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            // Register all applicable capabilities
            var target = new InterfaceFeed(appEntry.InterfaceID, feed);
            foreach (var capabilityList in appEntry.CapabilityLists.Where(AreCapabilitiesApplicable))
            {
                // ReSharper disable AccessToForEachVariableInClosure
                var dispatcher = new PerTypeDispatcher<Model.Capabilities.Capability>(true);
                if (WindowsUtils.IsWindows)
                {
                    dispatcher.Add((Model.Capabilities.FileType fileType) => Windows.FileType.Register(target, fileType, machineWide, handler));
                    dispatcher.Add((Model.Capabilities.UrlProtocol urlProtocol) => Windows.UrlProtocol.Register(target, urlProtocol, machineWide, handler));
                    dispatcher.Add((Model.Capabilities.AutoPlay autoPlay) => Windows.AutoPlay.Register(target, autoPlay, machineWide, handler));
                    dispatcher.Add((Model.Capabilities.ComServer comServer) => Windows.ComServer.Register(target, comServer, machineWide, handler));
                    if (machineWide || WindowsUtils.IsWindows8)
                        dispatcher.Add((Model.Capabilities.AppRegistration appRegistration) => Windows.AppRegistration.Register(target, appRegistration, capabilityList.Entries.OfType<Model.Capabilities.VerbCapability>(), machineWide, handler));
                    if (machineWide)
                        dispatcher.Add((Model.Capabilities.DefaultProgram defaultProgram) => Windows.DefaultProgram.Register(target, defaultProgram, handler));
                }
                else if (MonoUtils.IsUnix)
                {
                    dispatcher.Add((Model.Capabilities.FileType fileType) => Unix.FileType.Register(target, fileType, machineWide, handler));
                    dispatcher.Add((Model.Capabilities.UrlProtocol urlProtocol) => Unix.UrlProtocol.Register(target, urlProtocol, machineWide, handler));
                    dispatcher.Add((Model.Capabilities.DefaultProgram defaultProgram) => Unix.DefaultProgram.Register(target, defaultProgram, machineWide, handler));
                }
                dispatcher.Dispatch(capabilityList.Entries);
                // ReSharper restore AccessToForEachVariableInClosure
            }
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            // Unregister all applicable capabilities
            foreach (var capabilityList in appEntry.CapabilityLists.Where(AreCapabilitiesApplicable))
            {
                var dispatcher = new PerTypeDispatcher<Model.Capabilities.Capability>(true);
                if (WindowsUtils.IsWindows)
                {
                    dispatcher.Add((Model.Capabilities.FileType fileType) => Windows.FileType.Unregister(fileType, machineWide));
                    dispatcher.Add((Model.Capabilities.UrlProtocol urlProtocol) => Windows.UrlProtocol.Unregister(urlProtocol, machineWide));
                    dispatcher.Add((Model.Capabilities.AutoPlay autoPlay) => Windows.AutoPlay.Unregister(autoPlay, machineWide));
                    dispatcher.Add((Model.Capabilities.ComServer comServer) => Windows.ComServer.Unregister(comServer, machineWide));
                    if (machineWide || WindowsUtils.IsWindows8)
                        dispatcher.Add((Model.Capabilities.AppRegistration appRegistration) => Windows.AppRegistration.Unregister(appRegistration, machineWide));
                    if (machineWide)
                        dispatcher.Add((Model.Capabilities.DefaultProgram defaultProgram) => Windows.DefaultProgram.Unregister(defaultProgram));
                }
                else if (MonoUtils.IsUnix)
                {
                    dispatcher.Add((Model.Capabilities.FileType fileType) => Unix.FileType.Unregister(fileType, machineWide));
                    dispatcher.Add((Model.Capabilities.UrlProtocol urlProtocol) => Unix.UrlProtocol.Unregister(urlProtocol, machineWide));
                    dispatcher.Add((Model.Capabilities.DefaultProgram defaultProgram) => Unix.DefaultProgram.Unregister(defaultProgram, machineWide));
                }
                dispatcher.Dispatch(capabilityList.Entries);
            }
        }

        private static bool AreCapabilitiesApplicable(Model.Capabilities.CapabilityList capabilityList)
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
            return new CapabilityRegistration {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements};
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(CapabilityRegistration other)
        {
            return base.Equals(other);
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
            return base.GetHashCode();
        }
        #endregion
    }
}
