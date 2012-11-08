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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Security.Principal;
using System.ServiceProcess;
using Common.Storage;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Service.Properties;

namespace ZeroInstall.Store.Service
{
    /// <summary>
    /// Represents a Windows service.
    /// </summary>
    public partial class StoreService : ServiceBase
    {
        #region Constants
        private const int IncorrectFunction = 1;
        private const int AccessDenied = 5;
        private const int InvalidHandle = 6;
        private const int UnableToWriteToDevice = 29;
        #endregion

        #region Variables
        /// <summary>
        /// The identity the service is running under.
        /// </summary>
        public static readonly WindowsIdentity Identity = WindowsIdentity.GetCurrent();

        /// <summary>IPC channel for providing services to clients.</summary>
        private readonly IChannelReceiver _serverChannel;

        /// <summary>IPC channel for launching callbacks in clients.</summary>
        private readonly IChannelSender _clientChannel;

        /// <summary>The store to provide to clients as a service.</summary>
        private MarshalByRefObject _store;

        /// <summary>The IPC remoting reference for <see cref="_store"/>.</summary>
        private ObjRef _objRef;
        #endregion

        #region Constructor
        public StoreService()
        {
            InitializeComponent();

            // Ensure the event log accepts messages from this service
            if (!EventLog.SourceExists(eventLog.Source)) EventLog.CreateEventSource(eventLog.Source, eventLog.Log);

            _serverChannel = new IpcServerChannel(
                new Hashtable
                {
                    {"name", IpcStoreProvider.IpcPortName},
                    {"portName", IpcStoreProvider.IpcPortName},
                    {"secure", true}, {"impersonate", true} // Use identity of client in server threads
                },
                new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full} // Allow deserialization of custom types
#if !__MonoCS__
                , IpcStoreProvider.IpcAcl
#endif
                );
            _clientChannel = new IpcClientChannel(
                new Hashtable
                {
                    {"name", IpcStoreProvider.IpcPortName + ".Callback"},
                },
                new BinaryClientFormatterSinkProvider());
        }
        #endregion

        #region Store
        /// <summary>
        /// Creates the store to provide to clients as a service.
        /// </summary>
        /// <exception cref="IOException">Thrown if a cache directory could not be created or if the underlying filesystem can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a cache directory is not permitted.</exception>
        private MarshalByRefObject CreateStore()
        {
            // Use first custom machine-wide location or fallback to default
            string path = StoreProvider.GetImplementationDirs(false).First();

            return new SecureStore(path, eventLog);
        }
        #endregion

        #region Service events
        protected override void OnStart(string[] args)
        {
            if (Locations.IsPortable)
            {
                eventLog.WriteEntry(Resources.NoPortableMode, EventLogEntryType.Error);
                ExitCode = IncorrectFunction;
                Stop();
                return;
            }

            try
            {
                ChannelServices.RegisterChannel(_serverChannel, false);
                ChannelServices.RegisterChannel(_clientChannel, false);
                _store = CreateStore();
                _objRef = RemotingServices.Marshal(_store, IpcStoreProvider.IpcObjectUri, typeof(IStore));
            }
                #region Error handling
            catch (IOException ex)
            {
                eventLog.WriteEntry(string.Format("Failed to open cache directory:\n{0}", ex), EventLogEntryType.Error);
                ExitCode = UnableToWriteToDevice;
                Stop();
            }
            catch (UnauthorizedAccessException ex)
            {
                eventLog.WriteEntry(string.Format("Failed to open cache directory:\n{0}", ex), EventLogEntryType.Error);
                ExitCode = AccessDenied;
                Stop();
            }
            catch (RemotingException ex)
            {
                eventLog.WriteEntry(string.Format("Failed to open IPC connection:\n{0}", ex), EventLogEntryType.Error);
                ExitCode = InvalidHandle;
                Stop();
            }
            catch (SecurityException ex)
            {
                eventLog.WriteEntry(string.Format("Failed to open IPC connection:\n{0}", ex), EventLogEntryType.Error);
                ExitCode = InvalidHandle;
                Stop();
            }
            #endregion
        }

        protected override void OnStop()
        {
            if (_objRef != null) RemotingServices.Unmarshal(_objRef);
            try
            {
                ChannelServices.UnregisterChannel(_clientChannel);
                ChannelServices.UnregisterChannel(_serverChannel);
            }
            catch (RemotingException)
            {}
        }
        #endregion
    }
}
