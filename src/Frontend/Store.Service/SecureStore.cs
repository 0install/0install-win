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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using JetBrains.Annotations;
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Storage;
using NanoByte.Common.Tasks;
using ZeroInstall.Store.Implementations;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Service.Properties;

namespace ZeroInstall.Store.Service
{
    /// <summary>
    /// Models a cache directory that stores <see cref="Store.Model.Implementation"/>s using ACLs and impersonation to ensure security in IPC scenarios.
    /// </summary>
    /// <remarks>The represented store data is mutable but the class itself is immutable.</remarks>
    public class SecureStore : DirectoryStore, IEquatable<SecureStore>
    {
        #region Variables
        /// <summary>The identity the service was launched with.</summary>
        private readonly WindowsIdentity _serviceIdentity;

        /// <summary>Writes messages to the Windows Event Log.</summary>
        private readonly EventLog _eventLog;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new store using a specific path to a cache directory.
        /// </summary>
        /// <param name="path">A fully qualified directory path. The directory will be created if it doesn't exist yet.</param>
        /// <param name="serviceIdentity">The identity the service was launched with.</param>
        /// <param name="eventLog">Writes messages to the Windows Event Log.</param>
        /// <exception cref="IOException">The directory <paramref name="path"/> could not be created or if the underlying filesystem can not store file-changed times accurate to the second.</exception>
        /// <exception cref="UnauthorizedAccessException">Creating the directory <paramref name="path"/> is not permitted.</exception>
        public SecureStore([NotNull] string path, [NotNull] WindowsIdentity serviceIdentity, [NotNull] EventLog eventLog) : base(path)
        {
            #region Sanity checks
            if (eventLog == null) throw new ArgumentNullException("eventLog");
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException("path");
            if (eventLog == null) throw new ArgumentNullException("eventLog");
            #endregion

            _serviceIdentity = serviceIdentity;
            _eventLog = eventLog;
        }
        #endregion

        //--------------------//

        #region Temp dir
        /// <inheritdoc/>
        protected override string GetTempDir()
        {
            var callingIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(callingIdentity != null);
            Debug.Assert(callingIdentity.User != null);

            using (_serviceIdentity.Impersonate()) // Use system rights instead of calling user
            {
                try
                {
                    string path = base.GetTempDir();

                    // Give the calling user write access
                    var acl = Directory.GetAccessControl(path);
                    acl.CanonicalizeAcl();
                    acl.AddAccessRule(new FileSystemAccessRule(callingIdentity.User, FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                    Directory.SetAccessControl(path, acl);
                    return path;
                }
                    #region Error handling
                catch (Exception ex)
                {
                    _eventLog.WriteEntry(string.Format(Resources.FailedToCreateTempDir, callingIdentity.Name, DirectoryPath) + Environment.NewLine + ex.Message, EventLogEntryType.Error);
                    throw;
                }
                #endregion
            }
        }

        /// <inheritdoc/>
        protected override void DeleteTempDir(string path)
        {
            var callingIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(callingIdentity != null);

            using (_serviceIdentity.Impersonate()) // Use system rights instead of calling user
            {
                try
                {
                    var directory = new DirectoryInfo(path);
                    if (directory.Exists)
                    {
                        try
                        {
                            directory.ResetAcl();
                        }
                        catch (IndexOutOfRangeException)
                        {
                            // Workaround for .NET 2.0 bug
                        }
                        directory.Delete(true);
                    }
                }
                    #region Error handling
                catch (Exception ex)
                {
                    _eventLog.WriteEntry(string.Format(Resources.FailedToRemoveTempDir, callingIdentity.Name, DirectoryPath) + Environment.NewLine + ex.Message, EventLogEntryType.Error);
                    throw;
                }
                #endregion
            }
        }
        #endregion

        #region Verify directory
        /// <inheritdoc/>
        protected override string VerifyAndAdd(string tempID, ManifestDigest expectedDigest, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            var callingIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(callingIdentity != null);

            using (_serviceIdentity.Impersonate()) // Use system rights instead of calling user
            {
                try
                {
                    var tempDirectory = new DirectoryInfo(Path.Combine(DirectoryPath, tempID));
                    try
                    {
                        handler.RunTask(new SimpleTask(Resources.SettingFilePermissions, tempDirectory.ResetAcl) {Tag = expectedDigest});
                    }
                    catch (IndexOutOfRangeException)
                    {
                        // Workaround for .NET 2.0 bug
                    }

                    string result = base.VerifyAndAdd(tempID, expectedDigest, handler);
                    _eventLog.WriteEntry(string.Format(Resources.SuccessfullyAddedImplementation, callingIdentity.Name, expectedDigest.AvailableDigests.FirstOrDefault(), DirectoryPath));
                    return result;
                }
                    #region Error handling
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    _eventLog.WriteEntry(string.Format(Resources.FailedToAddImplementation, callingIdentity.Name, expectedDigest.AvailableDigests.FirstOrDefault(), DirectoryPath), EventLogEntryType.Warning);
                    throw;
                }
                #endregion
            }
        }
        #endregion

        #region Remove
        /// <inheritdoc/>
        public override bool Remove(ManifestDigest manifestDigest)
        {
            if (!Contains(manifestDigest)) return false;
            if (!WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminToRemove);

            var callingIdentity = WindowsIdentity.GetCurrent();
            Debug.Assert(callingIdentity != null);

            using (_serviceIdentity.Impersonate()) // Use system rights instead of calling user
            {
                bool removed;
                try
                {
                    removed = base.Remove(manifestDigest);
                }
                    #region Error handling
                catch (Exception)
                {
                    _eventLog.WriteEntry(string.Format(Resources.FailedToRemoveImplementation, callingIdentity.Name, manifestDigest, DirectoryPath), EventLogEntryType.Warning);
                    throw;
                }
                #endregion

                if (removed) _eventLog.WriteEntry(string.Format(Resources.SuccessfullyRemovedImplementation, callingIdentity.Name, manifestDigest, DirectoryPath));
                return removed;
            }
        }
        #endregion

        #region Nop
        /// <summary>
        /// Does nothing. Should be handled by an administrator directly instead of using the service.
        /// </summary>
        public override void Verify(ManifestDigest manifestDigest, ITaskHandler handler)
        {}

        /// <summary>
        /// Does nothing. Should be handled by an administrator directly instead of using the service.
        /// </summary>
        public override long Optimise(ITaskHandler handler)
        {
            if (!WindowsUtils.IsAdministrator) throw new NotAdminException(Resources.MustBeAdminToOptimise);

            return 0;
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
        public bool Equals(SecureStore other)
        {
            return base.Equals(other);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == typeof(SecureStore) && Equals((SecureStore)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}
