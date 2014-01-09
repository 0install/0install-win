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
using System.IO;
using Common;
using Common.Storage;

namespace ZeroInstall.Central
{
    /// <summary>
    /// Provides common actions for the Central GUI.
    /// </summary>
    public static class CentralUtils
    {
        /// <summary>
        /// Returns <see langword="true"/> the first time it is called and then always <see langword="false"/>.
        /// </summary>
        public static bool OnFirstRun()
        {
            string firstRunFlag = Locations.GetSaveConfigPath("0install.net", true, "central", "intro_done");
            bool firstRun = !File.Exists(firstRunFlag);

            try
            {
                File.WriteAllText(firstRunFlag, "");
            }
                #region Error handling
            catch (IOException ex)
            {
                Log.Error(ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Error(ex);
            }
            #endregion

            return firstRun;
        }
    }
}
