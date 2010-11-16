/*
 * Copyright 2010 Bastian Eicher
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

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.ServiceProcess;
using ZeroInstall.Store.Implementation;

namespace ZeroInstall.Store.Service
{
    public partial class Service : ServiceBase
    {
        private readonly IChannel _channel = new IpcChannel(ServiceStore.IpcPortName);

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ChannelServices.RegisterChannel(_channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(SecureStore), ServiceStore.IpcObjectUri, WellKnownObjectMode.Singleton);
        }

        protected override void OnStop()
        {
            ChannelServices.UnregisterChannel(_channel);
        }
    }
}
