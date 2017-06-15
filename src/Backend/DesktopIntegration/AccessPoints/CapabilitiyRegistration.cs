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
using System.Linq;
using System.Xml.Serialization;
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

        /// <inheritdoc/>
        public override IEnumerable<string> GetConflictIDs(AppEntry appEntry)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            #endregion

            return appEntry.CapabilityLists.CompatibleCapabilities().SelectMany(x => x.ConflictIDs);
        }

        /// <inheritdoc/>
        public override void Apply(AppEntry appEntry, Feed feed, ITaskHandler handler, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            #endregion

            var capabilities = appEntry.CapabilityLists.CompatibleCapabilities().ToList();
            var target = new FeedTarget(appEntry.InterfaceUri, feed);

            foreach (var capability in capabilities)
            {
                switch (capability)
                {
                    case Store.Model.Capabilities.FileType fileType:
                        if (WindowsUtils.IsWindows) Windows.FileType.Register(target, fileType, machineWide, handler);
                        else if (UnixUtils.IsUnix) Unix.FileType.Register(target, fileType, machineWide, handler);
                        break;

                    case Store.Model.Capabilities.UrlProtocol urlProtocol:
                        if (WindowsUtils.IsWindows) Windows.UrlProtocol.Register(target, urlProtocol, machineWide, handler);
                        else if (UnixUtils.IsUnix) Unix.UrlProtocol.Register(target, urlProtocol, machineWide, handler);
                        break;

                    case Store.Model.Capabilities.AutoPlay autoPlay:
                        if (WindowsUtils.IsWindows) Windows.AutoPlay.Register(target, autoPlay, machineWide, handler);
                        break;

                    case AppRegistration appRegistration:
                        if ((WindowsUtils.IsWindows && machineWide) || WindowsUtils.IsWindows8) Windows.AppRegistration.Register(target, appRegistration, capabilities.OfType<VerbCapability>(), machineWide, handler);
                        break;

                    case Store.Model.Capabilities.DefaultProgram defaultProgram:
                        if (WindowsUtils.IsWindows && machineWide) Windows.DefaultProgram.Register(target, defaultProgram, handler);
                        else if (UnixUtils.IsUnix) Unix.DefaultProgram.Register(target, defaultProgram, machineWide, handler);
                        break;

                    case ComServer comServer:
                        if (WindowsUtils.IsWindows) Windows.ComServer.Register(target, comServer, machineWide, handler);
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public override void Unapply(AppEntry appEntry, bool machineWide)
        {
            #region Sanity checks
            if (appEntry == null) throw new ArgumentNullException(nameof(appEntry));
            #endregion

            foreach (var capability in appEntry.CapabilityLists.CompatibleCapabilities())
            {
                switch (capability)
                {
                    case Store.Model.Capabilities.FileType fileType:
                        if (WindowsUtils.IsWindows) Windows.FileType.Unregister(fileType, machineWide);
                        else if (UnixUtils.IsUnix) Unix.FileType.Unregister(fileType, machineWide);
                        break;

                    case Store.Model.Capabilities.UrlProtocol urlProtocol:
                        if (WindowsUtils.IsWindows) Windows.UrlProtocol.Unregister(urlProtocol, machineWide);
                        else if (UnixUtils.IsUnix) Unix.UrlProtocol.Unregister(urlProtocol, machineWide);
                        break;

                    case Store.Model.Capabilities.AutoPlay autoPlay:
                        if (WindowsUtils.IsWindows) Windows.AutoPlay.Unregister(autoPlay, machineWide);
                        break;

                    case AppRegistration appRegistration:
                        if ((WindowsUtils.IsWindows && machineWide) || WindowsUtils.IsWindows8) Windows.AppRegistration.Unregister(appRegistration, machineWide);
                        break;

                    case Store.Model.Capabilities.DefaultProgram defaultProgram:
                        if (WindowsUtils.IsWindows && machineWide) Windows.DefaultProgram.Unregister(defaultProgram);
                        else if (UnixUtils.IsUnix) Unix.DefaultProgram.Unregister(defaultProgram, machineWide);
                        break;

                    case ComServer comServer:
                        if (WindowsUtils.IsWindows) Windows.ComServer.Unregister(comServer, machineWide);
                        break;
                }
            }
        }

        #region Conversion
        /// <summary>
        /// Returns the access point in the form "CapabilityRegistration". Not safe for parsing!
        /// </summary>
        public override string ToString() => "CapabilityRegistration";
        #endregion

        #region Clone
        /// <inheritdoc/>
        public override AccessPoint Clone() => new CapabilityRegistration {UnknownAttributes = UnknownAttributes, UnknownElements = UnknownElements};
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
            if (obj == null) return false;
            if (obj == this) return true;
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
