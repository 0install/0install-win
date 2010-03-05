using System.ServiceProcess;

namespace ZeroInstall.StoreService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var ServicesToRun = new ServiceBase[] { new Service() };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
