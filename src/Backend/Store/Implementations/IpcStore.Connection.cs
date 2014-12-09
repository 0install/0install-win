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
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ZeroInstall.Store.Implementations
{
    public partial class IpcStore
    {
        #region Constants
        /// <summary>
        /// The port name to use to contact the store service.
        /// </summary>
        public const string IpcPortName = "ZeroInstall.Store.Service";

        /// <summary>
        /// The Uri fragment to use to request an <see cref="IStore"/> object from other processes.
        /// </summary>
        public const string IpcObjectUri = "Store";

        /// <summary>
        /// ACL for IPC named pipes. Allows object owners, normal users and the system write access.
        /// </summary>
        public static readonly CommonSecurityDescriptor IpcAcl;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Must build ACL during init")]
        static IpcStore()
        {
            var dacl = new DiscretionaryAcl(false, false, 1);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.CreatorOwnerSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            dacl.AddAccess(AccessControlType.Allow, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), -1, InheritanceFlags.None, PropagationFlags.None);
            IpcAcl = new CommonSecurityDescriptor(false, false, ControlFlags.GroupDefaulted | ControlFlags.OwnerDefaulted | ControlFlags.DiscretionaryAclPresent, null, null, null, dacl);
        }
        #endregion

        #region Singleton
        private static readonly object _lock = new object();
        private static volatile IStore _store;

        /// <summary>
        /// Provides a proxy object for accessing the <see cref="IStore"/> in the store service.
        /// </summary>
        /// <exception cref="RemotingException">There is a problem connecting with the store service.</exception>
        /// <remarks>Always returns the same instance. Opens named pipes on first call. Connection is only established on demand.</remarks>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "May throw exceptions")]
        private static IStore GetServiceProxy()
        {
            // Thread-safe singleton with double-check
            if (_store == null)
            {
                lock (_lock)
                {
                    if (_store == null) _store = CreateServiceProxy();
                }
            }
            return _store;
        }

        /// <summary>
        /// Sets up named pipes and creates a proxy object for accessing the <see cref="IStore"/> in the store service.
        /// </summary>
        /// <remarks>Must only be called once per process!</remarks>
        private static IStore CreateServiceProxy()
        {
            // IPC channel for accessing the server
            ChannelServices.RegisterChannel(new IpcClientChannel(
                new Hashtable
                {
                    {"name", IpcPortName},
                    {"secure", true}, {"tokenImpersonationLevel", "impersonation"} // Allow server to use identity of client
                },
                new BinaryClientFormatterSinkProvider()),
                false);

            // IPC channel for providing callbacks to the server
            ChannelServices.RegisterChannel(new IpcServerChannel(
                new Hashtable
                {
                    {"name", IpcPortName + ".Callback"},
                    {"portName", IpcPortName + ".Callback." + Path.GetTempFileName()} // Random port to allow multiple instances
                },
                new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full} // Allow deserialization of custom types
#if !__MonoCS__
                , IpcAcl
#endif
                ), ensureSecurity: false);

            // Create proxy object
            return (IStore)Activator.GetObject(typeof(IStore), "ipc://" + IpcPortName + "/" + IpcObjectUri);
        }
        #endregion
    }
}
