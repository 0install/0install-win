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
using System.IO;
using System.Security;
using Common.Storage;
using Microsoft.Win32;

namespace ZeroInstall.Capture
{
    /// <summary>
    /// Represents the systems state at a point in time. Used to determine changes.
    /// </summary>
    [Serializable]
    public class Snapshot
    {
        #region Variables
        private C5.ICollection<FileAssoc> _fileAssocs;

        private C5.ICollection<string> _progIDs;
        #endregion

        #region Static access
        /// <summary>
        /// Takes a snapshot of the current system state.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        public static Snapshot Take()
        {
            try
            {
                var fileAssocs = new C5.HashSet<FileAssoc>();
                var progIDs = new C5.HashSet<string>();
                foreach (var keyName in Registry.ClassesRoot.GetSubKeyNames())
                {
                    if (keyName.StartsWith("."))
                    {
                        using (var subKey = Registry.ClassesRoot.OpenSubKey(keyName))
                        {
                            if (subKey == null) continue;
                            string progID = (subKey.GetValue("") ?? "").ToString();
                            fileAssocs.Add(new FileAssoc(keyName, progID));
                        }
                    }
                    else progIDs.Add(keyName);
                }

                return new Snapshot
                {
                    _fileAssocs = fileAssocs,
                    _progIDs = progIDs
                };
            }
            #region Error handling
            catch (SecurityException ex)
            {
                // Wrap exception since only certain exception types are allowed in tasks
                throw new UnauthorizedAccessException(ex.Message, ex);
            }
            #endregion
        }
        #endregion

        //--------------------//

        // ToDo: Comparison method

        //--------------------//

        #region Storage
        /// <summary>
        /// Loads a <see cref="Snapshot"/> from a binary file.
        /// </summary>
        /// <param name="path">The file to load from.</param>
        /// <returns>The loaded <see cref="Snapshot"/>.</returns>
        /// <exception cref="IOException">Thrown if a problem occurs while reading the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the file is not permitted.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a problem occurs while deserializing the binary data.</exception>
        public static Snapshot Load(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            return BinaryStorage.Load<Snapshot>(path);
        }

        /// <summary>
        /// Loads a <see cref="Snapshot"/> from a stream containing a binary file.
        /// </summary>
        /// <param name="stream">The stream to load from.</param>
        /// <returns>The loaded <see cref="Snapshot"/>.</returns>
        public static Snapshot Load(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            return BinaryStorage.Load<Snapshot>(stream);
        }

        /// <summary>
        /// Saves this <see cref="Snapshot"/> to a binary file.
        /// </summary>
        /// <param name="path">The file to save in.</param>
        /// <exception cref="IOException">Thrown if a problem occurs while writing the file.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the file is not permitted.</exception>
        public void Save(string path)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            BinaryStorage.Save(path, this);
        }

        /// <summary>
        /// Saves this <see cref="Snapshot"/> to a stream as a binary file.
        /// </summary>
        /// <param name="stream">The stream to save in.</param>
        public void Save(Stream stream)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            #endregion

            BinaryStorage.Save(stream, this);
        }
        #endregion
    }
}
