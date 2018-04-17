// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

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

            ServiceBase.Run(new ServiceBase[] {new StoreService()});
            return 0;
        }
    }
}
