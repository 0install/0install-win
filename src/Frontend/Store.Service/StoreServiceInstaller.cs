/*
 * Copyright 2010-2016 Bastian Eicher
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

using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace ZeroInstall.Store.Service
{
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
}
