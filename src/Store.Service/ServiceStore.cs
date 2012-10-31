/*
 * Copyright 2010-2012 Bastian Eicher
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
using System.Diagnostics;
using System.IO;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Implementation;
using ZeroInstall.Store.Implementation.Archive;

namespace ZeroInstall.Store.Service
{
    /// <summary>
    /// Provides a background service to add new entries to a store that requires elevated privileges to write.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class ServiceStore : DirectoryStore, IEquatable<ServiceStore>
    {
        #region Variables
        /// <summary>Writes to the Windows event log.</summary>
        internal static EventLog EventLog;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store using a specific path to a cache directory.
        /// </summary>
        /// <param name="path">A fully qualified directory path. The directory will be created if it doesn't exist yet.</param>
        /// <exception cref="IOException">Thrown if the directory could not be created or if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating the directory <paramref name="path"/> is not permitted.</exception>
        public ServiceStore(string path) : base(path)
        {
            if (EventLog != null) EventLog.WriteEntry(string.Format("Using '{0}' as store directory.", path));

            // ToDo: Prevent public write access with ACLs
        }

        /// <summary>
        /// Creates a new store using the default system cache directory.
        /// </summary>
        /// <exception cref="IOException">Thrown if the directory could not be created or if the underlying filesystem of the user profile can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating the directory path is not permitted.</exception>
        public ServiceStore() : this(FileUtils.PathCombine(Locations.SystemCacheDir, "0install.net", "implementations"))
        {}
        #endregion

        #region Lifetime
        public override object InitializeLifetimeService()
        {
            // Keep remoting object alive indefinitely
            return null;
        }
        #endregion

        //--------------------//

        #region Add
        /// <inheritdoc />
        public override void AddDirectory(string path, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            try
            {
                base.AddDirectory(path, manifestDigest, handler);
            }
                #region Error handling
            catch (Exception ex)
            {
                if (EventLog != null) EventLog.WriteEntry(string.Format("Failed to add directory '{0}' with expected digest '{1}':\n{2}", path, manifestDigest, ex), EventLogEntryType.Warning);
                throw; // Pass on to caller
            }
            #endregion

            if (EventLog != null) EventLog.WriteEntry(string.Format("Added directory '{0}' with expected digest '{1}'.", path, manifestDigest));
        }

        /// <inheritdoc />
        public override void AddArchives(IEnumerable<ArchiveFileInfo> archiveInfos, ManifestDigest manifestDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (archiveInfos == null) throw new ArgumentNullException("archiveInfos");
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            try
            {
                base.AddArchives(archiveInfos, manifestDigest, handler);
            }
                #region Error handling
            catch (Exception ex)
            {
                if (EventLog != null) EventLog.WriteEntry(string.Format("Failed to add archives with expected digest '{0}':\n{1}", manifestDigest, ex), EventLogEntryType.Warning);
                throw; // Pass on to caller
            }
            #endregion

            if (EventLog != null) EventLog.WriteEntry(string.Format("Added archives with expected digest '{0}'.", manifestDigest));
        }

        /// <inheritdoc />
        protected override string GetTempDir()
        {
            // ToDo: Prevent public read access with ACLs
            return base.GetTempDir();
        }
        #endregion

        #region Remove
        /// <inheritdoc />
        public override void Remove(ManifestDigest manifestDigest)
        {
            // ToDo: Restrict access
            base.Remove(manifestDigest);

            if (EventLog != null) EventLog.WriteEntry(string.Format("Removed implementation with digest '{0}'.", manifestDigest));
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <summary>
        /// Returns the Store in the form "SecureStore: DirectoryPath". Not safe for parsing!
        /// </summary>
        public override string ToString()
        {
            return "SecureStore: " + DirectoryPath;
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(ServiceStore other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(ServiceStore) && Equals((ServiceStore)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
