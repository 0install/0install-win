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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Deployment.Compression.Cab;
using Microsoft.Deployment.WindowsInstaller;
using NanoByte.Common.Storage;
using NanoByte.Common.Streams;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Properties;

namespace ZeroInstall.Store.Implementations.Archives
{
    /// <summary>
    /// Extracts a Windows Installer MSI package (with one or more embedded CAB archives).
    /// </summary>
    public class MsiExtractor : MicrosoftExtractor
    {
        #region Database
        private readonly Database _database;

        /// <summary>
        /// Prepares to extract a Windows Installer MSI package contained in a stream.
        /// </summary>
        /// <param name="path">The path of the Windows Installer MSI package to be extracted.</param>
        /// <param name="target">The path to the directory to extract into.</param>
        /// <exception cref="IOException">Thrown if the package is damaged.</exception>
        internal MsiExtractor(string path, string target)
            : base(target)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            #endregion

            try
            {
                _database = new Database(path, DatabaseOpenMode.ReadOnly);
                ReadDirectories();
                ReadFiles();
                ReadCabinets();

                UnitsTotal = _files.Values.Sum(x => x.Size);
            }
                #region Error handling
            catch (InstallerException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            #endregion
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    _database.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }
        #endregion

        #region Directories
        private class MsiDirectory
        {
            public string Name;
            public string ParentID;
            public string FullPath;

            public override string ToString()
            {
                return FullPath;
            }
        }

        private readonly Dictionary<string, MsiDirectory> _directories = new Dictionary<string, MsiDirectory>();

        private void ReadDirectories()
        {
            using (var directoryView = _database.OpenView("SELECT Directory, DefaultDir, Directory_Parent FROM Directory"))
            {
                directoryView.Execute();
                foreach (var row in directoryView)
                {
                    _directories.Add(row["Directory"].ToString(), new MsiDirectory
                    {
                        Name = row["DefaultDir"].ToString().Split(':').Last().Split('|').Last(),
                        ParentID = row["Directory_Parent"].ToString()
                    });
                }
            }

            foreach (var directory in _directories.Values)
                ResolveDirectory(directory);
        }

        private void ResolveDirectory(MsiDirectory directory)
        {
            if (directory.FullPath != null) return;

            if (string.IsNullOrEmpty(directory.ParentID))
            { // Root directory
                directory.FullPath = directory.Name;
            }
            else
            {
                var parent = _directories[directory.ParentID];
                if (parent == directory)
                { // Root directory
                    directory.FullPath = directory.Name;
                }
                else
                { // Child directory
                    ResolveDirectory(parent);
                    directory.FullPath = (directory.Name == ".")
                        ? parent.FullPath
                        : parent.FullPath + "/" + directory.Name;
                }
            }
        }
        #endregion

        #region Files
        private class MsiFile
        {
            public string Name;
            public int Size;
            public string DirectoryId;
            public string FullPath;

            public override string ToString()
            {
                return FullPath;
            }
        }

        private readonly Dictionary<string, MsiFile> _files = new Dictionary<string, MsiFile>();

        private void ReadFiles()
        {
            using (var fileView = _database.OpenView("SELECT File, FileName, FileSize, Directory_ FROM File, Component WHERE Component_ = Component"))
            {
                fileView.Execute();
                foreach (var row in fileView)
                {
                    _files.Add(row["File"].ToString(), new MsiFile
                    {
                        Name = row["FileName"].ToString().Split(':').Last().Split('|').Last(),
                        Size = (int)row["FileSize"],
                        DirectoryId = row["Directory_"].ToString()
                    });
                }
            }

            foreach (var file in _files.Values)
                ResolveFile(file);
        }

        private void ResolveFile(MsiFile file)
        {
            file.FullPath = _directories[file.DirectoryId].FullPath + "/" + file.Name;
        }
        #endregion

        #region Cabinets
        private List<string> _cabinets;

        private void ReadCabinets()
        {
            using (var mediaView = _database.OpenView("SELECT Cabinet FROM Media"))
            {
                mediaView.Execute();
                _cabinets = mediaView
                    .Select(row => row["Cabinet"].ToString())
                    .Where(name => name.StartsWith("#"))
                    .Select(name => name.Substring(1))
                    .ToList();
            }
        }
        #endregion

        /// <inheritdoc/>
        protected override void Execute()
        {
            State = TaskState.Data;

            try
            {
                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (string cabinet in _cabinets)
                {
                    using (var streamsView = _database.OpenView("SELECT Data FROM _Streams WHERE Name = '{0}'", cabinet))
                    {
                        streamsView.Execute();
                        using (var record = streamsView.Fetch())
                        {
                            if (record == null) throw new IOException(Resources.ArchiveInvalid + "\n" + string.Format("Cabinet stream '{0}' missing", cabinet));

                            using (var stream = record.GetStream("Data"))
                                ExtractCab(stream);
                        }
                    }
                }
            }
                #region Error handling
            catch (InstallerException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            catch (CabException ex)
            {
                // Wrap exception since only certain exception types are allowed
                throw new IOException(Resources.ArchiveInvalid + "\n" + ex.Message, ex);
            }
            #endregion

            State = TaskState.Complete;
        }

        private void ExtractCab(Stream stream)
        {
            using (var tempFile = new TemporaryFile("0install"))
            {
                // Extract embedded CAB from MSI
                using (var tempStream = File.Create(tempFile))
                    stream.CopyTo(tempStream, cancellationToken: CancellationToken);

                // Extract individual files from CAB
                using (CabStream = File.OpenRead(tempFile))
                    CabEngine.Unpack(this, _ => true);
            }
        }

        /// <inheritdoc/>
        protected override string GetRelativePath(string entryName)
        {
            try
            {
                return base.GetRelativePath(_files[entryName].FullPath);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
    }
}
