/*
 * Copyright 2010-2015 Bastian Eicher
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
using NanoByte.Common.Dispatch;
using NanoByte.Common.Native;
using NanoByte.Common.Tasks;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Model.Capabilities;

namespace ZeroInstall.DesktopIntegration.AccessPoints
{
    /// <summary>
    /// Indicates that all compatible capabilities should be registered.
    /// </summary>
    /// <seealso cref="ZeroInstall.Store.Model.Capabilities"/>
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

            return appEntry.CapabilityLists.CompatibleCapabilities().SelectMany(x => x.ConflictIDs);
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

            var capabilities = appEntry.CapabilityLists.CompatibleCapabilities().ToList();
            var target = new FeedTarget(appEntry.InterfaceUri, feed);

            var dispatcher = new PerTypeDispatcher<Capability>(ignoreMissing: true);
            if (WindowsUtils.IsWindows)
            {
                dispatcher.Add((Store.Model.Capabilities.FileType fileType) => Windows.FileType.Register(target, fileType, machineWide, handler));
                dispatcher.Add((Store.Model.Capabilities.UrlProtocol urlProtocol) => Windows.UrlProtocol.Register(target, urlProtocol, machineWide, handler));
                dispatcher.Add((Store.Model.Capabilities.AutoPlay autoPlay) => Windows.AutoPlay.Register(target, autoPlay, machineWide, handler));
                dispatcher.Add((ComServer comServer) => Windows.ComServer.Register(target, comServer, machineWide, handler));
                if (machineWide || WindowsUtils.IsWindows8)
                    dispatcher.Add((AppRegistration appRegistration) => Windows.AppRegistration.Register(target, appRegistration, capabilities.OfType<VerbCapability>(), machineWide, handler));
                if (machineWide)
                    dispatcher.Add((Store.Model.Capabilities.DefaultProgram defaultProgram) => Windows.DefaultProgram.Register(target, defaultProgram, handler));
            }
            else if (UnixUtils.IsUnix)
            {
                dispatcher.Add((Store.Model.Capabilities.FileType fileType) => Unix.FileType.Register(target, fileType, machineWide, handler));
                dispatcher.Add((Store.Model.Capabilities.UrlProtocol urlProtocol) => Unix.UrlProtocol.Register(target, urlProtocol, machineWide, handler));
                dispatcher.Add((Store.Model.Capabilities.DefaultProgram defaultProgram) => Unix.DefaultProgram.Register(target, defaultProgram, machineWide, handler));
            }

            dispatcher.Dispatch(capabilities);
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException("appEntry");
            #endregion

            var dispatcher = new PerTypeDispatcher<Capability>(ignoreMissing: true);
            if (WindowsUtils.IsWindows)
            {
                dispatcher.Add((Store.Model.Capabilities.FileType fileType) => Windows.FileType.Unregister(fileType, machineWide));
                dispatcher.Add((Store.Model.Capabilities.UrlProtocol urlProtocol) => Windows.UrlProtocol.Unregister(urlProtocol, machineWide));
                dispatcher.Add((Store.Model.Capabilities.AutoPlay autoPlay) => Windows.AutoPlay.Unregister(autoPlay, machineWide));
                dispatcher.Add((ComServer comServer) => Windows.ComServer.Unregister(comServer, machineWide));
                if (machineWide || WindowsUtils.IsWindows8)
                    dispatcher.Add((AppRegistration appRegistration) => Windows.AppRegistration.Unregister(appRegistration, machineWide));
                if (machineWide)
                    dispatcher.Add((Store.Model.Capabilities.DefaultProgram defaultProgram) => Windows.DefaultProgram.Unregister(defaultProgram));
            }
            else if (UnixUtils.IsUnix)
            {
                dispatcher.Add((Store.Model.Capabilities.FileType fileType) => Unix.FileType.Unregister(fileType, machineWide));
                dispatcher.Add((Store.Model.Capabilities.UrlProtocol urlProtocol) => Unix.UrlProtocol.Unregister(urlProtocol, machineWide));
                dispatcher.Add((Store.Model.Capabilities.DefaultProgram defaultProgram) => Unix.DefaultProgram.Unregister(defaultProgram, machineWide));
            }
            dispatcher.Dispatch(appEntry.CapabilityLists.CompatibleCapabilities());
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
