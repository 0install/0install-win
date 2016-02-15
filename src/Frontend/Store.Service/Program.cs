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

using System.ServiceProcess;
using NanoByte.Common;
using NanoByte.Common.Storage;

namespace ZeroInstall.Store.Service
{
    /// <summary>
    /// Launches a Windows service for managing the secure shared cache of Zero Install implementations.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        // NOTE: No [STAThread] here, because it could block .NET remoting
        private static int Main()
        {
            // Encode installation path into mutex name to allow instance detection during updates
            string mutexName = "mutex-" + Locations.InstallBase.GetHashCode();
            if (AppMutex.Probe(mutexName + "-update")) return 999;

            // NOTE: Do not block updater from starting because it will automatically stop service

            ServiceBase.Run(new ServiceBase[] { new StoreService() });
            return 0;
        }
    }
}
