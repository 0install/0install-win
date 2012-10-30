/*
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
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Access an <see cref="IStore"/> running on a background service via IPC.
    /// </summary>
    public sealed class RemoteStoreProvider
    {
        #region Constants
        /// <summary>
        /// The default port name to use to contact the service process.
        /// </summary>
        public const string IpcPortName = "ZeroInstall.Store.Service";

        /// <summary>
        /// The Uri fragment to use to request an <see cref="IStore"/> object from the service.
        /// </summary>
        public const string IpcObjectUri = "Store";

        /// <summary>
        /// ACL that allows object owners, normal users and the system write access.
        /// </summary>
        public static readonly CommonSecurityDescriptor LiberalAcl;

        static RemoteStoreProvider()
        {
            var dacl = new DiscretionaryAcl(false, false, 1);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.CreatorOwnerSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            LiberalAcl = new CommonSecurityDescriptor(false, false, ControlFlags.GroupDefaulted | ControlFlags.OwnerDefaulted | ControlFlags.DiscretionaryAclPresent, null, null, null, dacl);
        }
        #endregion

        #region Variables
        /// <summary>IPC channel for accessing the server.</summary>
        private readonly IChannelSender _clientChannel = new IpcClientChannel();

        /// <summary>IPC channel for providing callbacks to the server.</summary>
        private readonly IChannelReceiver _callbackChannel = new IpcServerChannel(
            new Dictionary<string, string> {{"name", null}, {"portName", IpcPortName + ".Callback"}},
            new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full}, // Allow deserialization of custom types
            LiberalAcl);
        #endregion

        #region Properties
        /// <summary>The store to use for read-access.</summary>
        public IStore StoreProxy { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes the IPC subsystem using the default IPC port name. No actual connection is established yet.
        /// </summary>
        public RemoteStoreProvider() : this(IpcPortName)
        {}

        /// <summary>
        /// Initializes the IPC subsystem using a custom IPC port name. No actual connection is established yet.
        /// </summary>
        /// <param name="ipcPortName">The port name to use to contact the service process.</param>
        public RemoteStoreProvider(string ipcPortName)
        {
            ChannelServices.RegisterChannel(_clientChannel, false);
            ChannelServices.RegisterChannel(_callbackChannel, false);

            StoreProxy = (IStore)Activator.GetObject(typeof(IStore), "ipc://" + ipcPortName + "/" + IpcObjectUri);
        }

        ~RemoteStoreProvider()
        {
            ChannelServices.UnregisterChannel(_callbackChannel);
            ChannelServices.UnregisterChannel(_clientChannel);
        }
        #endregion
    }
}
