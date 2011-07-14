/*
 * Copyright 2011 Bastian Eicher
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

namespace ZeroInstall.Capture.Cli
{
    /// <summary>
    /// Structure for storing user-selected arguments for a capture operation.
    /// </summary>
    public struct ParseResults
    {
        /// <summary>The command to execute.</summary>
        public string Command;

        /// <summary>The directory to execute the command on.</summary>
        public string DirectoryPath;

        /// <summary>Ignore warnings and perform the operation anyway.</summary>
        public bool Force;

        /// <summary>The directory the application to be captured is installed in.</summary>
        public string InstallationDirectory;

        /// <summary>The relative path to the main EXE of the application to be captured.</summary>
        public string MainExe;

        /// <summary>Indicates whether to collect installation files in addition to registry data.</summary>
        public bool GetFiles;
    }
}
