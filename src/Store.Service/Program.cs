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

using System.Security.Cryptography;
using System.ServiceProcess;
#if !DEBUG
using Common.Storage;
using Common.Utils;
#endif

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
        static void Main(string[] args)
        {
#if !DEBUG
            // Prevent launch during update and allow instance detection
            string mutexName = "mutex-" + StringUtils.Hash(Locations.InstallBase, SHA256.Create());
            if (AppMutex.Probe(mutexName + "-update")) return;
            AppMutex.Create(mutexName);
            AppMutex.Create("Zero Install");
#endif

            var servicesToRun = new ServiceBase[] { new Service() };
            ServiceBase.Run(servicesToRun);
        }
    }
}
