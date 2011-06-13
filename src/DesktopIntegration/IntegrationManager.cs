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

using System;
using System.IO;
using Common.Storage;
using Common.Utils;
using ZeroInstall.DesktopIntegration.Model;

namespace ZeroInstall.DesktopIntegration
{
    /// <summary>
    /// Manages desktop integration operations.
    /// </summary>
    public class IntegrationManager
    {
        #region Variables
        /// <summary>Apply operations system-wide instead of just for the current user.</summary>
        private readonly bool _global;

        /// <summary>The storage location of the <see cref="AppList"/> file.</summary>
        private readonly string _appListPath;

        /// <summary>Stores a list of applications and their desktop integrations.</summary>
        private readonly AppList _appList;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new integration manager.
        /// </summary>
        /// <param name="global">Apply operations system-wide instead of just for the current user.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while accessing the <see cref="AppList"/> file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read or wirte access to the <see cref="AppList"/> file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the XML data.</exception>
        public IntegrationManager(bool global)
        {
            _global = global;

            _appListPath = global
                ? FileUtils.PathCombine(Locations.SystemConfigDirs.Split(Path.PathSeparator)[0], "0install.net", "desktop-integration", "myapps.xml")
                : Locations.GetSaveConfigPath("0install.net", Path.Combine("desktop-integration", "myapps.xml"), false);

            if (File.Exists(_appListPath)) _appList = AppList.Load(_appListPath);
            else
            {
                _appList = new AppList();
                _appList.Save(_appListPath);
            }
        }
        #endregion

        //--------------------//

        // ToDo
    }
}
