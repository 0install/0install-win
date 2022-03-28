// Copyright Bastian Eicher et al.
// Licensed under the GNU Lesser Public License

using System.Configuration.Install;
using System.ServiceProcess;

namespace ZeroInstall.Store.Service;

/// <summary>
/// Entry point used by InstallUtil.exe.
/// </summary>
[RunInstaller(true)]
public class StoreServiceInstaller : Installer
{
    public StoreServiceInstaller()
    {
        Installers.Add(new ServiceProcessInstaller {Account = ServiceAccount.LocalSystem});
        Installers.Add(new ServiceInstaller
        {
            Description = "Manages a Zero Install implementation cache shared between all users.",
            DisplayName = "Zero Install Store Service",
            ServiceName = "0store-service",
            StartType = ServiceStartMode.Automatic
        });
    }
}
