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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ZeroInstall.Store.Implementation
{
    /// <summary>
    /// Access <see cref="IStore"/> running in other proccesses via IPC.
    /// </summary>
    public static class RemoteStoreProvider
    {
        #region Constants
        /// <summary>
        /// The default port name to use to contact the background service process.
        /// </summary>
        public const string ServiceIpcPortName = "ZeroInstall.Store.Service";

        /// <summary>
        /// The Uri fragment to use to request an <see cref="IStore"/> object from another proccess.
        /// </summary>
        public const string IpcObjectUri = "Store";

        /// <summary>
        /// ACL used for IPC named pipes. Allows object owners, normal users and the system write access.
        /// </summary>
        public static readonly CommonSecurityDescriptor IpcAcl;
        #endregion

        #region Constructor
        static RemoteStoreProvider()
        {
            var dacl = new DiscretionaryAcl(false, false, 1);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.CreatorOwnerSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            IpcAcl = new CommonSecurityDescriptor(false, false, ControlFlags.GroupDefaulted | ControlFlags.OwnerDefaulted | ControlFlags.DiscretionaryAclPresent, null, null, null, dacl);

            // IPC channel for accessing the server
            ChannelServices.RegisterChannel(new IpcClientChannel("", new BinaryClientFormatterSinkProvider()), false);

            // IPC channel for providing callbacks to the server
            ChannelServices.RegisterChannel(new IpcServerChannel(
                new Dictionary<string, string> {{"name", ""}, {"portName", ServiceIpcPortName + ".Callback"}},
                new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full} // Allow deserialization of custom types
#if !__MonoCS__
                , IpcAcl
#endif
                ), false);
        }
        #endregion

        /// <summary>
        /// Creates a new proxy object for accessing the <see cref="IStore"/> in the background service.
        /// </summary>
        /// <exception cref="RemotingException">Thrown if there is a problem connecting with the background service.</exception>
        public static IStore GetServiceProxy()
        {
            return (IStore)Activator.GetObject(typeof(IStore), "ipc://" + ServiceIpcPortName + "/" + IpcObjectUri);
        }
    }
}
