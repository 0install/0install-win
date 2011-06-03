/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Represents the association of a file ending wiht a programatic identifier.
    /// </summary>
    [Serializable]
    public struct FileAssoc
    {
        /// <summary>
        /// The file ending.
        /// </summary>
        public string Ending;

        /// <summary>
        /// The programatic identifier.
        /// </summary>
        public string ProgID;

        /// <summary>
        /// Creates a new file association.
        /// </summary>
        /// <param name="ending">The file ending.</param>
        /// <param name="progID">The programatic identifier.</param>
        public FileAssoc(string ending, string progID)
        {
            Ending = ending;
            ProgID = progID;
        }
    }
}
